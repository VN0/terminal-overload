// Copyright information can be found in the file named COPYING
// located in the root directory of this distribution.

#if 0 // BORQUE_NEEDS_PORTING

#ifndef _NORTDISC_H_
#define _NORTDISC_H_

#ifndef _GAMEBASE_H_
#include "game/gameBase.h"
#endif
#ifndef _TSSHAPE_H_
#include "ts/tsShape.h"
#endif
#ifndef _LIGHTMANAGER_H_
#include "sceneGraph/lightManager.h"
#endif
#ifndef _PLATFORMAUDIO_H_
#include "platform/platformAudio.h"
#endif
#ifndef _NETCONNECTION_H_
#include "sim/netConnection.h"
#endif

#include "game/fx/particleEmitter.h"
#include "game/fx/laserBeam.h"
#include "game/fx/fxLight.h"
#include "game/player.h"
#include "game/projectile.h"

class ExplosionData;
class ShapeBase;
class TSShapeInstance;
class TSThread;

//--------------------------------------------------------------------------

// a useful little NetEvent instructing client
// to create specified explosion at specified position
class CreateExplosionEvent: public NetEvent
{
	ExplosionData* mData;
	Point3F mPos;
	Point3F mNorm;

  public:
   CreateExplosionEvent(ExplosionData* data, const Point3F& p, const Point3F &n);
	CreateExplosionEvent();

   void pack(NetConnection*, BitStream* bstream);
   void write(NetConnection*, BitStream* bstream);
   void unpack(NetConnection*, BitStream* bstream);
   void process(NetConnection*);

   DECLARE_CONOBJECT(CreateExplosionEvent);
};

//--------------------------------------------------------------------------

// NetEvent to inform the server that a NortDisc has
// hit something or has been deflected...
//class NortDiscHitEvent : public NetEvent
//{
//	typedef NetEvent Parent;
//
//	RayInfo mInfo;      
//	
//	S32     mObjId;     // ghost ID of the object that was hit
//	S32     mDiscId;    // ghost ID of the disc that hit
//	Point3F mVec;       // collision vector
//	bool    mDeflected; // disc deflected?
//	
//  public:
//	NortDiscHitEvent();
//	NortDiscHitEvent(const RayInfo& rInfo,U32 objId, U32 discId,
//									const Point3F& vec, bool deflected);
//    
//	virtual void pack   (NetConnection* conn, BitStream* bstream);
//	virtual void unpack (NetConnection* conn, BitStream* bstream);
//	virtual void write  (NetConnection* conn, BitStream* bstream);
//	virtual void process(NetConnection* conn);
// 
//	DECLARE_CONOBJECT(NortDiscHitEvent);
//};

//--------------------------------------------------------------------------

class NortDiscData : public ProjectileData
{
   typedef ProjectileData Parent;

protected:
   bool onAdd();

public:
	F32 maxVelocity;
	F32 acceleration;

	bool startVertical;

   NortDiscData();

   void packData(BitStream*);
   void unpackData(BitStream*);
   bool preload(bool server, char errorBuffer[256]);

   static void initPersistFields();
   DECLARE_CONOBJECT(NortDiscData);
};
DECLARE_CONSOLETYPE(NortDiscData)

//--------------------------------------------------------------------------

class NortDisc : public Projectile
{
   typedef Projectile Parent;

   NortDiscData* mDataBlock;

 public:
	enum State
	{
		Attacking,
		Returning,
		Deflected
	};
	
 private:
	State mState;

	GameBase* mInitialTarget;
	bool      mInitialTargetExists;

	Point3F mSpinTargetPos;

 public:
	NortDisc();
   ~NortDisc();

	// ConsoleObject...
   static void initPersistFields();
   static void consoleInit();

	// NetObject...
	U32  packUpdate(NetConnection* con, U32 mask, BitStream* stream);
	void unpackUpdate(NetConnection* con, BitStream* stream);

	// SimObject...
	bool onAdd();
	void onRemove();
	void onDeleteNotify(SimObject* obj);

	// SceneObject...
	bool castRay(const Point3F &start, const Point3F &end, RayInfo* info);

	// GameBase...
	void processTick(const Move*);
	bool onNewDataBlock(GameBaseData*);

	// NortDisc...
	bool findCollision(const Point3F &start, const Point3F &end, RayInfo* info);
	void updateSpin();
	virtual void setTarget(GameBase* target);
	Point3F bounce(const RayInfo& rInfo, const Point3F& vec, bool bounceExp=false);
	State state() { return mState; };
	void createExplosion(const Point3F& p, const Point3F& n, ExplosionType type);
	void hit(GameBase* obj, const RayInfo& rInfo);
	void deflected(const Point3F& newVel);
	void stopAttacking(U32 targetType);
	//void processNortDiscHitEvent(NetConnection* conn, GameBase* obj,
	//	const RayInfo& rInfo, const Point3F& vec, bool deflected); 

   DECLARE_CONOBJECT(NortDisc);
};

#endif // _NORTDISC_H_

#endif // BORQUE_NEEDS_PORTING
