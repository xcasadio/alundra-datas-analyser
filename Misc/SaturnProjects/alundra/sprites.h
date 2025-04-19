#ifndef ALUNDRA_SPRITES
#define ALUNDRA_SPRITES


#include    "alundra.h"
#include	"datasbin.h"


typedef struct packed CachedImage
{
	Uint16 cacheindex;
	union packed
	{
		Uint32 signature;//campare this with uncached images to see if it matches
		struct packed
		{
			Uint8 sx;
			Uint8 sy;
			Uint8 swidth;
			Uint8 sheight;
		};
	};
	Uint16 vramaddr;//address of image in vram / 8
} CachedImage;

typedef struct packed SpriteCache
{
	int numimages;
	CachedImage images[1000];//1000 images TODO:check how many are being loaded
}SpriteCache;

typedef struct packed EventProgramState
{
	Uint8 *		sp;//program start pointer
	Uint8 *		exp;//program execution pointer
	Uint16		elapsedMs;
	Uint8		isWaiting;
	Uint8		logicResult;
}EventProgramState;

typedef struct packed SpriteInstance
{
	DBSprite *	spr;
	Uint8		enabled;
	Uint8		currentAnimId;
	DBFrame *	currentAnim;
	Uint8		curFrame;
	Uint32		lastTick;
	Uint16		elapsedMs;
	Sint16		x;
	Sint16		y;
	Sint16		z;
	Uint8		tilex;
	Uint8		tiley;
	Uint8		tilez;
	Uint8		dir;
	Uint8		tempdir;//used for facing during dialogs
	Uint8		gravity;//turn off for climbing and flying
	Uint8		canpickup;
	Uint8		canstack;
	Uint8		canthrow;
	Uint8		shadowtype;
	Uint8		hasportrait;
	DBSprite *	breakanim;
	Uint8 *		breaksound;
	Uint8 *		contents;//object contents must be a new sector, perhaps the one before dbsprites

	Uint8 *		maploadprogram;//program that runs on map load
	EventProgramState tickprogram;//event program that runs every tick
	Uint8 *		interactprogram;//program that runs on interaction (like npc dialog)
	
}SpriteInstance;

typedef struct packed MapEventInstance
{
	Uint32		lastTick;
	EventProgramState tickprogram;//event program that runs every tick

}MapEventInstance;

#define MAX_MASTER_SPRITES	64
#define MAX_MAP_EVENTS		16

#define SPRITE_IS_ENABLED(spr)	(spr->enabled >> 1 == 1)
#define SPRITE_IS_DISABLED(spr)	(spr->enabled >> 1 == 0)

extern Uint32 totalloadedsprites;
extern Uint32 nextvramaddr;
extern volatile SpriteInstance *mastersprites;
extern volatile MapEventInstance *mastermapevents;

void init_spritecache(Uint8* spritesheetimagedata);
CachedImage * cache_image(DBImage * img, int isgenimage);
Uint16 process_image(DBImage * img, int load);
int get_starting_animid(DBSpriteInfo * sinfo, DBSprite * spr, DBEntity * entity);
void update_sprite(SpriteInstance * si, DBMap * map, Uint16 curtick);
void update_mapevent(MapEventInstance * ei, DBMap * map, Uint16 curtick);
void set_sprite_anim(SpriteInstance * si, int animid, Uint16 curtick);
int loadsprites(DBSpriteInfo * sinfo, SpriteInstance sprites[], Uint16 curtick);
int loadmapevents(DBSpriteInfo * sinfo, MapEventInstance sprites[], Uint16 curtick);

#endif
