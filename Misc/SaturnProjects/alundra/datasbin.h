#ifndef ALUNDRA_DATASBIN
#define ALUNDRA_DATASBIN

#include	"alundra.h"
#include	"filesystem.h"

//map stuff
#define MAXMAPS 502

#define MAXSPRITES 600//this will no doubt have to grow

#define TILE_SIZE (24*16/2)
#define CLUT_SIZE (16*2)

#define TILESETS_ADDR		(MAXSPRITES * 32)
#define TILESETS_SIZE		(10*16*6*TILE_SIZE)
#define CLUTS_ADDR			(TILESETS_ADDR + TILESETS_SIZE)
#define TILE_CLUTS_ADDR		CLUTS_ADDR
#define SI_CLUTS_ADDR		CLUTS_ADDR + (CLUT_SIZE*32)
#define GENSI_CLUTS_ADDR	CLUTS_ADDR + (CLUT_SIZE*64)
#define CLUTS_SIZE			(CLUT_SIZE*(32+32+32))//tile cluts, mapsprite cluts, general sprite cluts
#define SPRITES_ADDR		(CLUTS_ADDR + CLUTS_SIZE)

#define CODESADATA(sinfo,entity) &(*(sinfo)->eventcodesdata)[flipu16((*(sinfo)->eventcodesa)[(entity)->eventcodesaindex & 0x7f])]
#define CODESBDATA(sinfo,mapevent) &(*(sinfo)->eventcodesdata)[flipu16((*(sinfo)->eventcodesb)[(mapevent)->eventprogram1bindex & 0x7f])]
#define CODESCDATA(sinfo,entity) &(*(sinfo)->eventcodesdata)[flipu16((*(sinfo)->eventcodesc)[(entity)->eventcodescindex & 0x7f])]
#define CODESDDATA(sinfo,entity) &(*(sinfo)->eventcodesdata)[flipu16((*(sinfo)->eventcodesd)[(entity)->eventcodesdindex & 0x7f])]
#define CODESEDATA(sinfo,entity) &(*(sinfo)->eventcodesdata)[flipu16((*(sinfo)->eventcodese)[(entity)->eventcodeseindex & 0x7f])]
#define CODESFDATA(sinfo,entity) &(*(sinfo)->eventcodesdata)[flipu16((*(sinfo)->eventcodesf)[(entity)->eventcodesfindex & 0x7f])]

#define GETIMAGESET(spr, frame) ((DBImageSet*)&(*(spr)->imagesetdata)[flipu16((frame)->imagesetoffset) << 1])
#define GETFRAME(spr, animid, dir) ((DBFrame*)&(*(spr)->framesdata)[(spr)->animsets[animid].diroffsets[dir]])

typedef	struct packed DBHeader{
	Uint32			alundraspriteinfooffset;
	Uint32			alundraspritesheetsoffset;
	Uint32			alundraspritesheetsoffset2;
	Uint32			alundrastringtableoffset;
	Uint32			alundrastringtableoffset2;
	Uint32			unknownmapaoffset;
	Uint32			unknownmapboffset;
	Uint32			unknownmapb2offset;
	Uint32			unknownmapb3offset;
	Uint32			unknownmapb4offset;
	Uint32			gamemapoffsets[MAXMAPS];
}	DBHeader;

typedef struct packed DBPalette{
	Uint16 colors[16];
} DBPalette;

typedef struct packed DBPortal{
	Uint8 x1;
	Uint8 y1;
	Uint8 x2;
	Uint8 y2;
	Uint16 destmapid;
	Uint8 destx;
	Uint8 desty;
	Uint8 u1;
	Uint8 u2;
	Uint8 u3;
	Uint8 u4;
} DBPortal;

typedef struct packed DBGameMapInfo{
	Uint32 mapid;
	Uint16 gravity;// << 8
	Uint16 terminal_velocity;// << 8
	Uint16 u512;
	Uint16 u7;
	Uint8  u3882a;
	Uint8  u3882b;
	Uint16 u17;
	DBPalette palettes[32];
	Uint8 unknown[24];
	Uint8 portalflag1;
	Uint8 portalflag2;
	DBPortal portals[];
} DBGameMapInfo;

typedef struct packed DBWallTiles{
	Sint8 offset;
	Uint8 count;
	Uint16 tiles[];
} DBWallTiles;

typedef struct packed DBMapTile{
	Uint8 walkability;
	Uint8 groundproperty;
	Uint8 slope;
	Uint8 height;
	Uint16 tileid;//palette = 0xf000 >> 12  tile = 0x3ff
	Uint16 walltiles;//offset in walltiles array  << 1 
} DBMapTile;

typedef struct packed DBMap{
	Uint8 width;
	Uint8 height;
	Uint8 width2;
	Uint8 height2;
	Uint8 unknown[1536];
	DBMapTile tiles[52*60];
	Uint8 walltilesdata[];//cast to DBWallTile
} DBMap;


typedef struct packed DBMapEvent
{
	Uint8 u1;
	Uint8 u2;
	Uint8 eventprogram1bindex;
	Uint8 u4;
	Uint8 u5;
	Uint8 u6;
	Uint8 u7;
	Uint8 u8;
}DBMapEvent;

typedef struct packed DBMapEvents{
	Uint16 unknown;
	DBMapEvent mapevents[];
} DBMapEvents;


typedef struct packed DBEntity{
	Uint8 u1;
	Uint8 u2;
	Uint8 u3;
	Uint8 spritetypeanddir;
	Uint8 spritetableindex;
	Uint8 xpos;
	Uint8 ypos;
	Uint8 height;
	Uint8 eventcodesaindex;
	Uint8 eventcodesbindex;
	Uint8 eventcodescindex;
	Uint8 eventcodesdindex;
	Uint8 eventcodeseindex;
	Uint8 eventcodesfindex;
	Uint8 u15;
	Uint8 u16;
	Uint8 u17;
	Uint8 u18;
	Uint8 u19;
	Uint8 u20;
} DBEntity;


typedef struct packed DBEntities{
	Uint16 unknown;
	DBEntity entities[];
} DBEntities;

typedef struct packed DBImage{
	Uint8 spritesheet;
	Uint8 palette;
	union packed
	{
		void * cachedimage;//check if its in the memory range of spritecache to see if its been set
		struct packed
		{
			Uint8 sx;
			Uint8 sy;
			Uint8 swidth;
			Uint8 sheight;
		};
	};
	Sint8 x1;
	Sint8 y1;
	Sint8 x2;
	Sint8 y2;
	Sint8 x3;
	Sint8 y3;
	Sint8 x4;
	Sint8 y4;
}DBImage;

typedef struct packed DBImageSet{
	Uint8 unkinown;
	Uint8 numimages;
	DBImage images[];
}DBImageSet;

typedef struct packed DBFrame{
	Uint8 delay;
	Uint16 unknown;
	Uint16 imagesetoffset;
} DBFrame;

typedef struct packed DBAnimSet{
	union packed{
		Uint16 diroffsets[4];
		struct packed{
			Uint16 downoffset;
			Uint16 upoffset;
			Uint16 leftoffset;
			Uint16 rightoffset;
		};
	};
	Uint16 speed;
	Uint8 unknown[4];
} DBAnimSet;


typedef struct packed DBSprite{
	DBAnimSet * animsets;
	Uint8 (*framesdata)[];//cast to DBFrame[]
	Uint8 (*unknowndata)[];
	Uint8 (*imagesetdata)[];//cast to ImageSet
	//Uint8 unknown[16];
	Uint8 u1;
	Uint8 gravityandcanpickup;
	Uint8 portraitandshadowtype;
	Uint8 u4;
	Uint8 throwtype;
	Uint8 u6;
	Uint8 breaksound;
	Uint8 u8;
	Uint8 u9;
	Uint8 u10;
	Uint8 u11;
	Uint8 u12;
	Uint8 u13;
	Uint8 stackable;
	Uint8 breakanim;
	Uint8 contents;
} DBSprite;

typedef struct packed DBSpriteInfo{
	DBEntities * entities;
	Uint8 * sector3;
	DBMapEvents * mapevents;
	DBSprite* (*spritetable)[255];
	Uint8 * unknown;
	DBPalette (*spritepalettes)[32];
	union packed
	{
		Uint8 (*eventcodesdata)[];
		Uint16 (*eventcodesa)[];
	};
	Uint16 (*eventcodesb)[];
	Uint16 (*eventcodesc)[];
	Uint16 (*eventcodesd)[];
	Uint16 (*eventcodese)[];
	Uint16 (*eventcodesf)[];
} DBSpriteInfo;

typedef struct packed DBStringTable{
	union packed{
		Uint8  *stringdata;
		Uint16 *stringoffsets;
	};
}DBStringTable;

typedef struct packed DBGameMap{
	DBGameMapInfo *info;
	DBMap * map;
	Uint8 * tilesheets;
	DBSpriteInfo * spriteinfo;
	Uint8 * spritesheets;
	Uint8 *  scrollscreen;
	DBStringTable *  stringtable;
} DBGameMap;

DBHeader * initdatas();
DBGameMap * loadmap(int mapid,int*mapsize);

extern volatile DBSpriteInfo * gensi;
extern volatile DBGameMap * dbmap;

int deflate(Uint8*data, Uint8*dest, int datalen, int destlen);

Uint16 processeventcode(Uint8* code);


#endif

