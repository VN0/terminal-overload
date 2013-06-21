// Copyright information can be found in the file named COPYING
// located in the root directory of this distribution.

datablock SFXProfile(WpnBadgerFireSound)
{
   filename = "library/sound/rotc/fire2";
   description = AudioClose3D;
   preload = true;
};

datablock SFXProfile(WpnBadgerDryFireSound)
{
   filename = "library/sound/rotc/weaponEmpty";
   description = AudioClose3D;
   preload = true;
};

datablock SFXProfile(WpnBadgerReloadSound)
{
   filename = "art/sound/weapons/wpn_ryder_reload";
   description = AudioClose3D;
   preload = true;
};

datablock SFXProfile(WpnBadgerSwitchinSound)
{
   filename = "art/sound/weapons/wpn_ryder_switchin";
   description = AudioClose3D;
   preload = true;
};

datablock SFXPlayList(WpnBadgerFireSoundList)
{
   // Use a looped description so the list playback will loop.
   description = AudioClose3D;

   track[ 0 ] = WpnBadgerFireSound;
};

