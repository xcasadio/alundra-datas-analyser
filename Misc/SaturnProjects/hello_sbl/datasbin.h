#include	<machine.h>
#include	<sega_gfs.h>

//external file stuff (put this in another file)
extern GfsHn openFileByName(GfsDirTbl *dirtbl, Sint8 *fname);
extern GfsDirTbl dirtbl;
#define MAX_OPEN        1
#define MAX_DIR         100
#define SECT_SIZE       2048
extern volatile unsigned char buff[SECT_SIZE];
extern volatile Uint8		*VRAM;



//map stuff
#define MAXMAPS 502

typedef	struct	DBHeader{
	Uint32		unknownblockoffset;
	Uint32		alundraspritesoffset;
	Uint32		alundraspritesoffset2;
	Uint32		stringtabledebugoffset;
	Uint32		stringtabledebugoffset2;
	Uint32		unknownmapaoffset;
	Uint32		unknownmapboffset;
	Uint32		unknownmapb2offset;
	Uint32		unknownmapb3offset;
	Uint32		unknownmapb4offset;
	Uint32		gamemapoffsets[MAXMAPS];
}	DBHeader;