#pragma once

#include <stdlib.h>

//modify by hand !
//only for structure to remove after
#include "LIBGPU.h"
#include "LIBCD.h"
#include "LIBSPU.h"

//don't delete
#define false 0
#define true  1

typedef long long longlong;
typedef unsigned long long ulonglong;

//exported from Ghidra

typedef unsigned char undefined;

typedef unsigned char bool;
typedef unsigned char byte;
typedef unsigned int dword;
typedef unsigned char uchar;
typedef unsigned int uint;
typedef unsigned int uint3;
typedef unsigned long ulong;
typedef unsigned char undefined1;
typedef unsigned short undefined2;
typedef unsigned int undefined3;
typedef unsigned int undefined4;
typedef unsigned long long undefined5;
typedef unsigned long long undefined8;
typedef unsigned short ushort;
typedef unsigned short word;


typedef struct Entity Entity, * PEntity;

//modify by hand don't delete
typedef void (*func)();
typedef void (*func49)(int, void (*func)());
typedef void (*code)(Entity*, uint*);
//end


typedef struct AnimationData AnimationData, * PAnimationData;

struct AnimationData {
    int* entries;
    int* frameListOffset;
    short entryIndex;
    short isZForceApplied;
    int* pointerListOffset;
    uint flags;
    int* rawPtrListOffset;
    byte offsetX;
    byte offsetY;
    byte offsetZ;
    byte sizeX;
    byte sizeY;
    byte sizeZ;
};

typedef struct AnimationTable AnimationTable, * PAnimationTable;

struct AnimationTable {
    struct AnimationData* baseDataPtr;
    int rawPointerList;
    int* field2_0x8;
    int* pointerList;
    int pointerListCount;
    int field5_0x14;
    int* frameList;
    int* field7_0x1c;
    int field8_0x20;
    int frameCount;
    int* scriptList;
    int scriptCount;
    int field12_0x30;
    int field13_0x34;
};

typedef struct BalanceAnimValRef BalanceAnimValRef, * PBalanceAnimValRef;

struct BalanceAnimValRef {
    byte val;
    byte u2;
};

typedef struct BalanceRecord BalanceRecord, * PBalanceRecord;

struct BalanceRecord {
    byte level;
    byte offsetToNextLevel;
    byte Hp;
    byte values[11];
    byte numAnimVals;
    int* animVals;
    struct BalanceRecord* next;
};

typedef struct EffectFrame EffectFrame, * PEffectFrame;

struct EffectFrame {
    byte delay;
    int imageSet; /* ImageSetPointer = br.ReadUInt16() * 2; */
};

//typedef struct Entity Entity, * PEntity;

typedef struct Frame Frame, * PFrame;

typedef struct FrameCollisionData FrameCollisionData, * PFrameCollisionData;

typedef struct LogicContext LogicContext, * PLogicContext;

typedef struct Script Script, * PScript;

struct Entity {
    struct Entity* previousEntity;
    struct Entity* nextEntity; /* collisionMask too!! */
    struct Entity* childEntity;
    struct Entity* parentEntity;
    int status; /* //0=destroyed,1=loaded,2=normal,3=deactivated,4=flagtodestroy,5=? */
    int hp;
    int hpMax;
    int hitFrameCounter;
    int isNotProcessable;
    int flags2;
    struct Entity* platformEntity;
    int actionState;
    int contentsItemId;
    int contentsGameFlag;
    void* entityRecord;
    void* scriptCallback;
    int programIndexes[6]; /* [0]=scriptId, [1]=initDataId, [2]=Entity, logicmode... */
    int field17_0x58;
    int zVelocity;
    int warpScriptPointer;
    struct AnimationData* spriteRecordPtr; /* SpriteRecord */
    int spriteTableIndex;
    int flags; /* 0x800000 = portrait,0x0100 = gravity,0xf = ?, 0x1 = ? , 0x80 = collidable */
    int spriteProgramIndexes[6];
    uint targetAnimationId; /* mask with 0xff */
    uint targetDirection;
    uint currentAnimationId;
    uint currentDirection;
    int currentFrameIndex;
    int* animSet;
    struct Frame* initialFrame;
    struct Frame* frame;
    int nextFrameDelay;
    int forceResetAnimationFlag;
    int animCompleteCounter;
    int animFlags;
    int zForce;
    int targetXForce;
    int targetYForce;
    int targetZForce;
    int targetXYZForce;
    int previousAdjustedXForce;
    int previousAdjustedYForce;
    int xForceStep;
    int yForceStep;
    int adjustedXForce;
    int adjustedYForce;
    int finalXForce;
    int finalyForce;
    int finalZForce;
    int acceleration;
    int speed;
    int isZForceApplied;
    int boundingBoxMaxX;
    int boundingBoxMaxY;
    int boundingBoxMaxZ;
    int boundingBoxMinX;
    int boundingBoxMinY;
    int field58_0x110;
    int xpos;
    int ypos;
    int zpos;
    int tileX;
    int tileY;
    int tileZ;
    struct Entity* ridingEntity;
    struct Entity* XCollisionEntity;
    int floorHeight;
    int terrainHeight;
    int forceAdjusted;
    int collidedWithEntityZ; /* some boolean that has to do with if moddedzpos is greater than hity from collideentitiesz */
    int isAboveGround;
    int mapTiles[4]; /* convert to MapTile */
    int mapHeights[4];
    int platformUpdateFlag;
    int field75_0x16c;
    int hitboxOriginX;
    int hitboxOriginY;
    int hitboxOriginZ;
    int field79_0x17c;
    int combinedVramFlagsOR;
    int combinedVramFlagsAND;
    int tileAttributes;
    int hitboxHeightY; /* slope ? */
    int hitboxHeightZ; /* slope ? */
    int* spriteImages;
    int spriteX;
    int spriteY;
    int spriteZ;
    int depthSortVal;
    int spriteNumberOfImages;
    int field91_0x1ac;
    int paletteIndex; /* represents offset where the pallets and sheets are in memory for map vs global sprites */
    int sheetSize; /* represents offset where the pallets and sheets are in memory for map vs global sprites */
    int warpLinkedEntity;
    int zSortValue;
    int zSortDepth;
    struct BalanceRecord* balanceRecord;
    struct BalanceAnimValRef* balanceAnimValRef;
    int damagedTickCounter;
    int frameColTickCounter;
    struct FrameCollisionData* frameCollisionData;
    int adjustedPosX;
    int adjustedPosY;
    int adjustedPosZ;
    int aPosX;
    int aPosY;
    int aPosZ;
    int width2;
    int heightY;
    int heightZ;
    int hitBoxX;
    int hitBoxY;
    int hitBoxZ;
    int transformX;
    int transformY;
    int transformZ;
    uint transformWidth;
    uint transformDepth;
    uint transformHeight;
    int hitCounter;
    struct Entity* touchingEntity;
    int eventTrigger; /* for the player character this holds the id of the map event that is triggering, for other entities this holds the type of event slot to trigger */
    int mapEventProgramId;
    struct Entity* logicContextEntity;
    struct LogicContext* logicContext;
    struct Script* script;
    byte _23c[40];
    int _264;
    int _268;
    int lastTargetAnimationId;
    int lastTargetDirection;
    int spawnCustomByte;
    int initialXPos;
    int initialYPos; /* can be equals to spawnedGameFlag[2] */
    int spawnedGameFlag[5]; /* [0] = animId?? [1] arg can be an Entity [2] = duration [3] phaseNum [10] remainingDelay */
};

struct Script {
    int command;
    byte _4[10];
};

struct Frame {
    byte delay; /* delay & 0x80 → indique un frame avec données de sprite/transform. */
    byte transformIndexLow; /* transformIndex = high<<8 | low → index dans la table de TransformData */
    byte transformIndexHigh; /* transformIndex = high<<8 | low → index dans la table de TransformData */
    byte spriteIndexLow; /* spriteIndex = high<<8 | low → index vers la table de SpriteImageData */
    byte spriteIndexHigh; /* spriteIndex = high<<8 | low → index vers la table de SpriteImageData */
};

struct LogicContext {
    int field0_0x0;
    struct Script* script;
    int previousCommandPtr;
    int xpos;
    int savedY;
    int savedZ;
    int animRepeatCount;
    int frameCounter;
    int _20;
    int _24;
    int _28;
    int isCommandSuccess;
    byte saveY;
    byte _31;
    byte _32;
    byte _33;
    struct Script* nextInstruction;
};

struct FrameCollisionData {
    char xOffset;
    char yOffset;
    char zOffset;
    byte width;
    byte depth;
    byte heigth;
};

typedef struct EntityEffect EntityEffect, * PEntityEffect;

struct EntityEffect {
    int Id;
    int* mapEffectRecord; /* MapEffectRecord */
    int* spriteEffectRecord; /* SpriteEffectRecord */
    int* spriteRef; /* SpriteRef */
    struct EntityEffect* nextEffect;
    int _14;
    int _18;
    int zSortValue2;
    int _20;
    int _24;
    int sheetSize;
    int paletteIndex;
    int mapEffectId;
    int updateMode;
    struct Entity* attachedEntity;
    int x;
    int y;
    int z;
    int velocityX;
    int velocityY;
    int velocityZ;
    int xForce;
    int yForce;
    int zForce;
    int zSortOffset;
    int zSortValue; /* stored to 1c, is it a depth sorting id? */
    int status; /* 2 is active */
    bool targetIsMapSprite;
    bool currentIsMapSprite;
    byte targetSpriteTableIndex;
    byte currentSpriteTableIndex;
    byte targetAnimation;
    byte currentAnimation;
    short _72;
    struct EffectFrame* effectFrame;
    struct EffectFrame* initialEffectFrame;
    byte delay;
    byte destroyFlag; /* if this is set true the effect is destroyed on next update (status = 0) */
};

typedef struct EntityRecord EntityRecord, * PEntityRecord;

struct EntityRecord {
    byte xMin;
    byte yMin;
    byte xMax;
    byte yMax;
    byte isEnabled;
    byte spriteDirection;
    byte spriteTableIndex;
    byte xPox;
    byte yPos;
    byte height;
    byte eventCodesA_LoadIndex;
    byte eventCodesB_MapIndex;
    byte eventCodesC_TickIndex;
    byte eventCodesD_TouchIndex;
    byte eventCodesE_DeactivateIndex;
    byte eventCodesF_InteractIndex;
    byte _10;
    byte _11;
    byte contents;
    byte _13;
};

typedef struct FadeControl FadeControl, * PFadeControl;

struct FadeControl {
    byte _0;
    byte _1;
    short warpVisualId; /* Created by retype action */
    short targetFadeLevel;
    short maxFadeLevel;
};

typedef struct PadState PadState, * PPadState;

struct PadState {
    uint maxNbFrameHeld;
    uint repeatInterval;
    uint isOverThanMaxNbFrameHeld;
    uint numberOfFrameHold;
    ushort buttonsHold; /* Created by retype action */
    ushort buttonsJustPressed;
    ushort buttonReleased;
    ushort buttonsJustPressedByInterval;
};

typedef struct SpriteMapEntry SpriteMapEntry, * PSpriteMapEntry;

struct SpriteMapEntry {
    bool enabled;
    byte vramShift;
    char tileWidth;
    byte rowCount;
    byte offsetX;
    byte offsetY;
    byte offsetZ;
};

typedef struct SprtGridDescriptor SprtGridDescriptor, * PSprtGridDescriptor;

typedef struct SPRT * PSPRT;

typedef ulong u_long;

typedef uchar u_char;

typedef ushort u_short;

struct SprtGridDescriptor {
    short _0;
    short _2;
    short spriteCountX;
    short spriteCountY;
    struct SPRT* spriteTablePtr;
};

struct SPRT {
    u_long tag;
    u_char r0;
    u_char g0;
    u_char b0;
    u_char code;
    short x0;
    short y0;
    u_char u0;
    u_char v0;
    u_short clut;
    short w;
    short h;
};

typedef struct TileSetMetaData TileSetMetaData, * PTileSetMetaData;

struct TileSetMetaData {
    byte tileUVCoordinates; /* Created by retype action */
    byte _1;
    byte tileRenderDataLow; /* Created by retype action */
    byte tileRenderDataHigh; /* Created by retype action */
    byte numberOfLayers; /* Created by retype action */
    short tileDepth; /* Created by retype action */
    int _7;
    byte tileAnimationMode; /* Created by retype action */
    byte frameOffsetTable; /* Created by retype action */
    byte _d;
    byte _e;
    byte _f;
    int tileAnimationBankOffset; /* Created by retype action */
    int tileAnimationOffset;
};

typedef struct Voice Voice, * PVoice;

struct Voice {
    undefined1 volLeft;
    byte _1;
    byte _2;
    byte _3;
    byte pitch;
    byte _5;
    byte reverbDepth;
    byte _7;
    byte adsrAttack;
    byte _9;
    byte adsrSustain;
    byte _b;
    byte status;
    byte _d[3];
    struct Voice* nextVoice;
    byte _14[359];
    short _17b;
    short _17d;
    short _17f;
    short _181;
    byte _183[5];
    short _188;
    short _18a;
    byte _18c;
    byte _18d;
    short _18e;
    int _190;
    ushort _194;
    ushort _196;
    short _198;
    short _19a;
    byte _19c[14];
    ushort _1aa;
};

typedef struct WarpData WarpData, * PWarpData;

struct WarpData {
    char tileX1;
    char tileY1;
    char tileX2;
    char tileY2;
    ushort mapId;
    char destinationTileX;
    char destinationTileY;
    short zLevel;
    ushort flags;
};

