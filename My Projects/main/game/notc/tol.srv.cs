// Copyright information can be found in the file named COPYING
// located in the root directory of this distribution.

//-----------------------------------------------------------------------------

// Variables used by server scripts & code.  The ones marked with (c)
// are accessed from code.  Variables preceeded by Pref:: are server
// preferences and stored automatically in the ServerPrefs.cs file
// in between server sessions.
//
//    (c) Server::ServerType              {SinglePlayer, MultiPlayer}
//    (c) Server::GameType                Unique game name
//    (c) Server::Dedicated               Bool
//    ( ) Server::MissionFile             Mission .mis file name
//    (c) Server::MissionName             DisplayName from .mis file
//    (c) Server::MissionType             Not used
//    (c) Server::PlayerCount             Current player count
//    (c) Server::GuidList                Player GUID (record list?)
//    (c) Server::Status                  Current server status
//
//    (c) Pref::Server::Name              Server Name
//    (c) Pref::Server::Password          Password for client connections
//    ( ) Pref::Server::AdminPassword     Password for client admins
//    (c) Pref::Server::Info              Server description
//    (c) Pref::Server::MaxPlayers        Max allowed players
//    (c) Pref::Server::RegionMask        Registers this mask with master server
//    ( ) Pref::Server::BanTime           Duration of a player ban
//    ( ) Pref::Server::KickBanTime       Duration of a player kick & ban
//    ( ) Pref::Server::MaxChatLen        Max chat message len
//    ( ) Pref::Server::FloodProtectionEnabled Bool

//-----------------------------------------------------------------------------

function destroyServer()
{
   $Server::ServerType = "";
   allowConnections(false);
   stopHeartbeat();
   sAuthStop();
   $missionRunning = false;

   // End any running levels
   endMission();
   
   physicsDestroyWorld( "server" );

   // Clean up the GameCore package here as it persists over the
   // life of the server.
   if (isPackage(GameCore))
   {
      deactivatePackage(GameCore);
   }

   // Delete all the server objects
   if (isObject(ServerGroup))
      ServerGroup.delete();

   // Delete all the connections:
   while (ClientGroup.getCount())
   {
      %client = ClientGroup.getObject(0);
      %client.delete();
   }

   $Server::GuidList = "";

   // Delete all the data blocks...
   deleteDataBlocks();

   // Save any server settings
   echo( "Exporting server prefs..." );
   export( "$Pref::Server::*", "~/prefs.cs", false );

   // Increase the server session number.  This is used to make sure we're
   // working with the server session we think we are.
   $Server::Session++;
}

function createServer(%gameType, %args)
{
   // Server::GameType is sent to the master server.
   // This variable should uniquely identify your game and/or mod.
   $Server::GameType = "TOL" SPC $GameVersionString;
   
   // Server::Status is returned in the Game Info Query and represents the
   // current status of the server. This string sould be very short.
   $Server::Status = "Unknown";
   
   // Turn on testing/debug script functions
   $Server::TestCheats = false;

   // Specify where the mission files are.
   $Server::MissionFileSpec = "content/*.mis";
   
   %serverType = "MultiPlayer";
   %mode = "ETH";
   %map = "eth1";

   // Parse arguments.
   for(%i = 0; %i < getWordCount(%args); %i++)
   {
      %arg = getWord(%args, %i);
      switch$(%arg)
      {
         case "-sp":
            %serverType = "SinglePlayer";

         case "-mode":
            %nextarg = getWord(%args, %i+1);
            if(%nextarg !$= "")
            {
               %mode = strupr(%nextarg);
               %map = strlwr(%nextarg) @ "1";
            }
               
         case "-map":
            %nextarg = getWord(%args, %i+1);
            if(%nextarg !$= "")
               %map = %nextarg;
      }
   }

   // The common module provides the basic server functionality
   initBaseServer();
   
   exec("./defaults.cs");

   // Server::GameType is sent to the master server.
   // This variable should uniquely identify your game and/or mod.
   $Server::GameType = "TOL" SPC $GameVersionString;

   // Server::MissionType sent to the master server.  Clients can
   // filter servers based on mission type.
   $Server::MissionType = %mode;

   // GameStartTime is the sim time the game started. Used to calculated
   // game elapsed time.
   $Game::StartTime = 0;

   // Create the server physics world.
   physicsInitWorld( "server" );

   // Load up any core datablocks
   exec("core/art/datablocks/datablockExec.cs");

   // Load up any objects or datablocks saved to the editor managed scripts
   %datablockFiles = new ArrayObject();
   %datablockFiles.add( "content/cleanup/particleData.cs" );
   %datablockFiles.add( "content/cleanup/particleEmitterData.cs" );
   %datablockFiles.add( "content/cleanup/decalData.cs" );
   %datablockFiles.add( "content/cleanup/datablocks.cs" );
   %datablockFiles.add( "content/cleanup/managedItemData.cs" );
   loadDatablockFiles( %datablockFiles, true );

   // Load up game mode scripts
   switch$(%mode)
   {
      case "DM":
         exec("./dm/exec.cs");
      case "ETH":
         exec("./eth/exec.cs");
      case "TE":
         exec("./TE/exec.cs");
   }
   
   %level = "content/xa/notc/mis/" @ %map @ "/mission.mis";

   // Make sure our level name is relative so that it can send
   // across the network correctly
   %level = makeRelativePath(%level, getWorkingDirectory());

   // Extract mission info from the mission file,
   // including the display name and stuff to send
   // to the client.
   buildLoadInfo(%level);

   if(!isObject(theLevelInfo))
   {
      error("createServer(): no level info");
      return false;
   }

   $missionSequence = 0;
   $Server::PlayerCount = 0;
   $Server::ServerType = %serverType;
   $Server::LoadFailMsg = "";
   $Physics::isSinglePlayer = true;

   // Setup for multi-player, the network must have been
   // initialized before now.
   if (%serverType $= "MultiPlayer")
   {
      $Physics::isSinglePlayer = false;

      echo("Starting multiplayer mode");

      // Make sure the network port is set to the correct pref.
      portInit($Pref::Server::Port);
      allowConnections(true);

      if ($pref::Net::DisplayOnMaster !$= "Never" )
         schedule(0,0,startHeartbeat);
   }

   // Start player authentication facilities
   schedule(0, 0, sAuthStart);

   // Create the ServerGroup that will persist for the lifetime of the server.
   new SimGroup(ServerGroup);

   loadMission(%level, true);

   return true;
}

function resetServerDefaults()
{
   echo( "Resetting server defaults..." );

   exec( "~/defaults.cs" );
   exec( "~/prefs.cs" );

   // Reload the current level
   loadMission( $Server::MissionFile );
}
