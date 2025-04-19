#include	<machine.h>
#include    <sega_spr.h>
#include	<sega_scl.h> 
#include	<sega_gfs.h>
#include	<sega_dbg.h>
#include    <sega_tim.h>

#include	"..\v_blank\v_blank.h"

#define MAX_OPEN        1
#define MAX_DIR         100
#define SECT_SIZE       2048

#define SCREEN_WIDTH	320
#define SCREEN_HEIGHT	224

extern GfsHn openFileByName(GfsDirTbl *dirtbl, Sint8 *fname);
extern GfsDirTbl dirtbl;

extern volatile unsigned char buff[SECT_SIZE];
extern volatile Uint8		*VRAM;



#define MAX_TILES_X	52
#define MAX_TILES_Y	60
#define CELL_WIDTH 8
#define CELL_HEIGHT 8
#define TILE_WIDTH (CELL_WIDTH * 3) //24
#define TILE_HEIGHT (CELL_HEIGHT * 2) //16


typedef struct
{
	int curxpos;
	int curypos;
	int width;
	int height;
	int clipx;
	int clipy;
	int clipwidth;
	int clipheight;
}map;

typedef struct
{
	Uint16 tile;
	Uint16 foretile;
	Uint8  ramp_type;//if non zero its a ramp and level is adjusted
	Uint8  surfacelevel;
	Uint8  surfacelevel2;
	Uint8  wall_level
} tile;


typedef struct
{
	Uint8 width;
	Uint8 height;
	tile tiles[MAX_TILES_X * MAX_TILES_Y];
}tilemap;

extern Uint16 get_elapsed(Uint16 tick1, Uint16 tick2);
extern void debug_print(int x, int y, const char *control, ...);



#define CELLS_PER_CHAR 1
#define CELLS_PER_PAGE 64 
#define PAGES_PER_PLANE 2
#define PLANES_PER_MAP 2

#define CHARS_PER_PAGE (CELLS_PER_PAGE / CELLS_PER_CHAR) //64

#define CELLS_PER_PLANE (PAGES_PER_PLANE * CELLS_PER_PAGE) //128
#define CELLS_PER_MAP	(PLANES_PER_MAP * CELLS_PER_PLANE) //256



void write_map_vram(tilemap* map, Uint16 celllocation, Uint16 cellsize, Uint16 * mapaddr, Uint16 * foremapaddr)
{
	int x, y;
	for (y = 0; y < map->height; y++)
	{
		for (x = 0; x < map->width; x++)
		{
			tile* tile = &map->tiles[y*map->width + x];
			int charid = tile->tile;
			//6 cells per tile
			int cx, cy;
			for (cy = 0; cy < 2; cy++)
			{
				for (cx = 0; cx < 3; cx++)
				{
					int cellx = x * 3 + cx;
					int celly = y * 2 + cy;
					
					int plane = (celly / CELLS_PER_PLANE) * PLANES_PER_MAP + (cellx / CELLS_PER_PLANE); //planey*vmapwidth + planex
					int page = ((celly % CELLS_PER_PLANE) / CELLS_PER_PAGE) * PAGES_PER_PLANE + ((cellx % CELLS_PER_PLANE) / CELLS_PER_PAGE);
					int cell = (celly % CELLS_PER_PAGE) * CELLS_PER_PAGE + (cellx % CELLS_PER_PAGE);

					//write tile
					int cellid = tile->tile * 6 + (cy * 3 + cx);
					mapaddr[plane*CELLS_PER_PLANE + page*CELLS_PER_PAGE + cell] = (celllocation + cellid*cellsize) / 32;
					//write foretile
					cellid = tile->foretile * 6 + (cy * 3 + cx);
					foremapaddr[plane*CELLS_PER_PLANE + page*CELLS_PER_PAGE + cell] = (celllocation + cellid*cellsize) / 32;
				}
			}

		}
	}
}


