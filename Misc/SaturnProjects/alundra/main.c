#include	<stdarg.h>
#include	<machine.h>
#define		_SPR2_
#include    <sega_spr.h>
#include	<sega_scl.h> 
#include	<sega_gfs.h>
#include    <sega_tim.h>

#include	"..\v_blank\v_blank.h"

#include    "alundra.h"
#include	"filesystem.h"
#include	"datasbin.h"
#include	"sprites.h"
#include	"events.h"


void draw_box(int x, int y, int width, int height, Uint16 color);
void draw_tile(Uint16 tileid, int x, int y);
void draw_sprite(CachedImage * cimg, DBImage * img, int x, int y);

int spritedex;
int dbg_on = 0;
volatile Uint8		*VRAM;
Uint16 currenttick;
Uint16 lasttick;
Uint16 elapsedticks;

SprSpCmd * spr_cmds;;


/*----------------------------
SCROLL NBG0 Cycle Table
Pattern Name Table location:B0
Character location: B1
Color Mode: C256
Zoom Mode: 1
----------------------------*/
Uint16	CycleTb[] = {
	0xffff, 0xffff,
	0xffff, 0xffff,
	0x0f44, 0xffff,//NGB0 Pattern Name Data Read,No Access, NGB0 Character Pattern Data Read, NGB0 Character Pattern Data Read
	0xffff, 0xffff
};

static SprSpCmd  spCmd16_320_240[] = {
   {/* [ 0]     */
    /* control  */ (JUMP_NEXT | FUNC_SCLIP),
    /* link     */ 0,
    /* drawMode */ 0,
    /* color    */ 0,
    /* charAddr */ 0,
    /* charSize */ 0,
    /* ax, ay   */ 0, 0,
    /* bx, by   */ 0, 0,
    /* cx, cy   */ 320-1, 240-1,
    /* dx, dy   */ 0, 0,
    /* grshAddr */ 0,
    /* dummy    */ 0},

   {/* [ 1]     */
    /* control  */ (JUMP_NEXT | FUNC_LCOORD),
    /* link     */ 0,
    /* drawMode */ 0,
    /* color    */ 0,
    /* charAddr */ 0,
    /* charSize */ 0,
    /* ax, ay   */ 0, 0,
    /* bx, by   */ 0, 0,
    /* cx, cy   */ 0, 0,
    /* dx, dy   */ 0, 0,
    /* grshAddr */ 0,
    /* dummy    */ 0},

	{/* [ 2]     */
    /* control  */ (JUMP_NEXT | FUNC_NORMALSP),
    /* link     */ 0,
    /* drawMode */ (ECD_DISABLE | COLOR_1 | COMPO_REP),
    /* color    */ (TILE_CLUTS_ADDR + 13*CLUT_SIZE)/8,
	/* charAddr */ (TILESETS_ADDR + 1*TILE_SIZE)/8,
	/* charSize */ (24 / 8) << 8 | 16,
    /* ax, ay   */   10,   10,
    /* bx, by   */   0,   0,
    /* cx, cy   */   0,   0,
    /* dx, dy   */   0,   0,
    /* grshAddr */   0,
    /* dummy    */   0}

};




void initscl()
{
	SclConfig	scfg;

	SCL_Vdp2Init();
	SCL_SetColRamMode(SCL_CRM24_1024);
	SetVblank();
	set_imask(0);
	SCL_SetFrameInterval(1);//what does this mean?

	Uint16 BackCol = 0x0000;
	SCL_SetBack(SCL_VDP2_VRAM + 0x80000 - 2, 1, &BackCol);

	SCL_InitConfigTb(&scfg);
	scfg.dispenbl = ON;
	scfg.bmpsize = SCL_BMP_SIZE_1024X512;
	scfg.coltype = SCL_COL_TYPE_256;
	scfg.datatype = SCL_BITMAP;
	scfg.mapover = SCL_OVER_0;
	scfg.plate_addr[0] = SCL_VDP2_VRAM_A0;
	SCL_SetConfig(SCL_NBG1, &scfg);

	/*******************************************
	*	VRAM Access Pattern Set            *
	*******************************************/
	SCL_SetCycleTable(CycleTb);

	SCL_SetPriority(SCL_NBG0, 7);
	SCL_SetPriority(SCL_NBG1, 5);
	SCL_SetPriority(SCL_SP0 | SCL_SP1 | SCL_SP2 | SCL_SP3 |
		SCL_SP4 | SCL_SP5 | SCL_SP6 | SCL_SP7, 6);

	SCL_Open(SCL_NBG1);
	SCL_MoveTo(FIXED(0), FIXED(0), 0);/* Home Position */
	SCL_Scale(FIXED(1.0), FIXED(1.0));
	SCL_Close();

	SCL_SetSpriteMode(SCL_TYPE1, SCL_MIX, SCL_SP_WINDOW);
	
}

void initspr()
{
	SPR_Initial(&VRAM);

	//load command table
	memcpy(VRAM, spCmd16_320_240, sizeof(spCmd16_320_240));
	spr_cmds = &((SprSpCmd*)VRAM)[2];

	SPR_SetTvMode(SPR_TV_NORMAL, SPR_TV_320X240, OFF);
	SPR_SetEraseData(0x0000, 0, 0, 320 - 1, 240 - 1);
}


SclRgb		start, end;
void fadein()
{
	
	start.red = start.green = start.blue = -255;
	end.red = end.green = end.blue = 0;

	//fade in
	SCL_SetColOffset(SCL_OFFSET_A, SCL_NBG1, start.red, start.green, start.blue);
	SCL_SetAutoColOffset(SCL_OFFSET_A, 1, 10, &start, &end);
}

void fadeout()
{
	start.red = start.green = start.blue = 0;
	end.red = end.green = end.blue = -255;

	//fade out
	SCL_SetColOffset(SCL_OFFSET_A, SCL_NBG1, start.red, start.green, start.blue);
	SCL_SetAutoColOffset(SCL_OFFSET_A, 1, 10, &start, &end);
}



void main()
{
	Uint16  	i, PadData1EW;
	
	
	
	initscl();
	initspr();
	initgfs();

	

	DBG_Initial(&PadData1, RGB16_COLOR(31, 31, 31), 0);

	//init timer
	TIM_FRT_INIT(TIM_CKS_128);
	


	//load sprite characters
	//char_defs *cdefs = load_chars("TESTING.CHR");
	//load sprite
	//spr_def *sprdef = load_sprite("TESTING.SPR");


	//set up main character sprite instance
	//move this to create_spr_instance function
	/*spr_instance mainchar;
	mainchar.sprite = sprdef;
	mainchar.currentAnimId = -1;
	spr_set_anim(&mainchar, 0, 0);
	mainchar.flipy = 0;

	map map;
	map.curxpos = 0;
	map.curypos = 0;

	load_map(&map, &mainchar, 150, 120, "STT_01.IMG", "STT_01.PAL");
	*/
	lasttick = TIM_FRT_GET_16();
	elapsedticks = 0;

	debug_print(10, 8, "Alundra");

	DBHeader * dbheader;
	dbheader = initdatas();
	int mapsize;
	DBGameMap * dbgamemap = loadmap(/*1*/389, &mapsize);
	DBMap * dbmap = dbgamemap->map;
	SpriteInstance *sprites = mastersprites;//128 max sprites?
	MapEventInstance *mapevents = mastermapevents;//16 max map events?
	int numsprites = loadsprites(dbgamemap->spriteinfo, sprites, lasttick);
	int nummapevents = loadmapevents(dbgamemap->spriteinfo, mapevents, lasttick);

	debug_print(10, 9, "mastersprites %x:%x", sprites, sizeof(SpriteInstance)* 128);
	debug_print(10, 10, "dbheader %x:%x", dbheader, sizeof(DBHeader));
	debug_print(10, 11, "loaded map %d %x:%x", flipu32(dbgamemap->info->mapid), dbgamemap, mapsize);
	debug_print(10, 12, "endofmemory %x", (int)dbgamemap + mapsize);
	debug_print(10, 13, "loaded %d sprites  ", totalloadedsprites);
	debug_print(10, 14, "used %x / %x vram ", nextvramaddr, 1024 * 1024 / 2);
	//debug_print(10, 13, "tiles %s", dbgamemap->header.tilesheets);
	//debug_print(10, 14, "sprites %s", dbgamemap->header.spritesheets);
	//debug_print(10, 15, "cluts_addr %d", CLUTS_ADDR);

	SCL_DisplayFrame();
	
	//initialize sprites
	int sdex;
	for (sdex = 0; sdex < numsprites; sdex++)
	{
		SpriteInstance * si = &sprites[sdex];
		update_sprite(si, dbmap, currenttick);
		if (si->maploadprogram != -1)
			ProcessOneTimeEvent(si->maploadprogram, si);
	}
	
	

	int curxpos = 0;
	int curypos = 0;
	int moved = 0;
	while (1) {
		currenttick = TIM_FRT_GET_16();
		elapsedticks = get_elapsed(currenttick, lasttick);
		
		//TIM_FRT_SET_16(0);
		PadData1EW = PadData1E;
		PadData1E = 0;

		
		moved = 0;
		float moveamount = elapsedticks / 1300.0;
		if (PadData1 & PAD_R)
		{
			curxpos += moveamount;
			//spr_set_anim(&mainchar, 1, currenttick);
			//mainchar.flipy = 1;
			moved = 1;
		}
		else if (PadData1 & PAD_L)
		{
			curxpos -= moveamount;
			//spr_set_anim(&mainchar, 1, currenttick);
			//mainchar.flipy = 0;
			moved = 1;
		}

		if (PadData1 & PAD_U)
		{
			curypos -= moveamount;
			//spr_set_anim(&mainchar, 1, currenttick);
			moved = 1;
		}
		else if (PadData1 & PAD_D)
		{
			curypos += moveamount;
			//spr_set_anim(&mainchar, 1, currenttick);
			moved = 1;
		}

		if (!moved)
		{
			//spr_set_anim(&mainchar, 0, currenttick);
		}

		//update map events
		int edex;
		for (edex = 0; edex < numsprites; edex++)
		{
			MapEventInstance * ei = &mapevents[edex];
			update_mapevent(ei, dbmap, currenttick);
			if (ei->tickprogram.exp != -1)
				ProcessEvent(&ei->tickprogram, 0);
		}
		//update event objects
		for (sdex = 0; sdex < numsprites; sdex++)
		{
			SpriteInstance * si = &sprites[sdex];
			update_sprite(si, dbmap, currenttick);
			if (si->tickprogram.exp != -1)
				ProcessEvent(&si->tickprogram, si);
		}


		//reset spritedex
		spritedex = 0;
		console_line = 0;
		console_col = 0;
		
		int x, y;
		int curxtile = curxpos / 24;
		
		DBSpriteInfo * sinfo = dbgamemap->spriteinfo;
		for (y = 0; y < 60; y++)
		{
			//draw tiles on this row
			for (x = curxtile; x < curxtile + SCREEN_WIDTH  / 24 + 2; x++)
			{
				DBMapTile * tile = &dbmap->tiles[y * 52 + x];
				//render tile
				int dx = x * 24 - curxpos;
				int dy = (y - tile->height) * 16 - curypos;

				if (dy > -16 && dy < SCREEN_HEIGHT && tile->tileid != 0xffff)
					draw_tile(flipu16(tile->tileid), dx, dy);
				if (tile->walltiles != 0xffff)
				{
					DBWallTiles * walltiles = (DBWallTiles *)&dbmap->walltilesdata[flipu16(tile->walltiles) << 1];
					int dex;
					dy -= (walltiles->offset)*16;
					for (dex = 0; dex < walltiles->count; dex++)
					{
						dy += 16;
						//render wall tile
						if (dy > -16 && dy < SCREEN_HEIGHT && walltiles->tiles[dex] != 0xffff)
							draw_tile(flipu16(walltiles->tiles[dex]), dx, dy);
						
					}
				}
			}

			//draw sprites who are on this row
			for (sdex = 0; sdex < numsprites; sdex++)
			{
				SpriteInstance * si = &sprites[sdex];
				if (SPRITE_IS_DISABLED(si))
					continue;
				if (si->y / 16 != y)
					continue;//if its not in this row, continue


				//var tile = selectedGame.map.maptiles[sx + sy * selectedGame.map.width];
				int scx = si->x - curxpos;
				int scy = (si->y - si->z) - curypos;
				if (si->spr != -1)
				{
						int idex;
						DBImageSet * iset = GETIMAGESET(si->spr, &si->currentAnim[si->curFrame]);
						for (idex = iset->numimages - 1; idex >= 0; idex--)
						{
							DBImage * img = &iset->images[idex];
							draw_sprite(img->cachedimage, img, scx, scy);
						}

				}
			}
		}


		/*int idex;
		DBSprite*tspr = (*gensi->spritetable)[1];
		DBImageSet * iset = GETIMAGESET(tspr, GETFRAME(tspr,0,0));
		for (idex = iset->numimages - 1; idex >= 0; idex--)
		{
			DBImage * img = &iset->images[idex];
			draw_sprite(img->cachedimage, img, 100, 100);
			
		}

		CachedImage* tsci = (CachedImage*)iset->images[0].cachedimage;
		debug_print(10, 16, "%d %d %d %x", tsci->cacheindex, tsci->swidth, tsci->sheight, (int)tsci->vramaddr*8);
		*/

		debug_print(10, 15, "sprites drawn: %d   ", spritedex);
		//debug_print(10, 16, "entities: %d   ", sdex);
		
		//end the commandlist
		spr_cmds[spritedex].control = CTRL_END;


		if ((PadData1EW & PAD_S))
		{
			dbg_on = ~dbg_on;
			if (dbg_on)
			{
				DBG_DisplayOn();
			}
			else
			{
				DBG_DisplayOff();
				//update scroll pos
			}
		}

		//Uint16 timeCnt1 = get_elapsed(TIM_FRT_GET_16(), currenttick);
		SCL_DisplayFrame();
		//Uint16 timeCnt2 = get_elapsed(TIM_FRT_GET_16(), currenttick);

		
		//Uint32 micro_sec = (Uint32)TIM_FRT_CNT_TO_MCR(timeCnt1);
		//debug_print(3, 5, "update cnt=%8d\n", timeCnt1);
		//debug_print(3, 6, "update us= %8d\n", micro_sec);
		//micro_sec = TIM_FRT_CNT_TO_MCR(timeCnt2);
		//debug_print(3, 7, "draw cnt=  %8d\n", timeCnt2);
		//debug_print(3, 8, "draw us=   %8d\n", micro_sec);
		//debug_print(3, 4, "elapsed=   %8d\n", elapsedticks);

		lasttick = currenttick;
	}
}

Uint16 get_elapsed(Uint16 tick1, Uint16 tick2)
{
	if (tick1 >= tick2)
	{
		return tick1 - tick2;
	}
	else
	{
		return (Uint16)((Uint32)tick1 + 0xffff - tick2);
	}

}

void debug_print(int x, int y, const char *control, ...)
{
	va_list args;
	char buf[256];

	va_start(args, control);
	vsprintf(buf, control, args);
	va_end(args);

	DBG_SetCursol(x, y);
	DBG_Printf(buf);
}

int console_line = 0;
int console_col = 0;
void console_print(const char *control, ...)
{
	va_list args;
	char buf[256];

	va_start(args, control);
	vsprintf(buf, control, args);
	va_end(args);

	DBG_SetCursol(console_col, console_line++);
	DBG_Printf(buf);

	//DBG_SetCursol(0, console_line);
	//DBG_Printf("                ");

	if (console_line >= 28)
	{
		console_line = 0;
		console_col += 9;
	}

}




void draw_tile(Uint16 tileid, int x, int y)
{
	int tiledex = tileid & 0x3ff;
	int paldex = (tileid & 0xf000) >> 12;

	SprSpCmd * cmd = &spr_cmds[spritedex++];

	cmd->control =	JUMP_NEXT | FUNC_NORMALSP;// | (flipy ? DIR_LRREV : DIR_NOREV);
	cmd->link =		0;
	cmd->drawMode = (ECD_DISABLE | COLOR_1 | COMPO_REP);
	cmd->color =	(TILE_CLUTS_ADDR + paldex * CLUT_SIZE) / 8;
	cmd->charAddr = (TILESETS_ADDR + tiledex * TILE_SIZE) / 8;
	cmd->charSize = (24 / 8) << 8 | 16,
	cmd->ax =		x;
	cmd->ay =		y;
	cmd->bx =		0;
	cmd->by =		0;
	cmd->cx =		0;
	cmd->cy =		0;
	cmd->dx =		0;
	cmd->dy =		0;
	cmd->grshAddr = 0;
	cmd->dummy =	0;
}

void draw_sprite(CachedImage * cimg, DBImage * img, int x, int y)
{
	int paldex = img->palette & 0x1f;
	int swidth = cimg->swidth;
	if (swidth % 8 > 0)
		swidth += 8 - swidth % 8;

	SprSpCmd * cmd = &spr_cmds[spritedex++];

	cmd->control = JUMP_NEXT | FUNC_DISTORSP;// | (flipy ? DIR_LRREV : DIR_NOREV);
	cmd->link = 0;
	cmd->drawMode = (ECD_DISABLE | COLOR_1 | COMPO_REP);
	cmd->color = ((cimg->cacheindex>>15 > 0 ? GENSI_CLUTS_ADDR : SI_CLUTS_ADDR) + paldex * CLUT_SIZE) / 8;
	cmd->charAddr = cimg->vramaddr;
	cmd->charSize = (swidth / 8) << 8 | cimg->sheight,
	cmd->ax = x + img->x1;
	cmd->ay = y + img->y1;
	cmd->bx = x + img->x2;
	cmd->by = y + img->y2;
	cmd->cx = x + img->x4;
	cmd->cy = y + img->y4;
	cmd->dx = x + img->x3;
	cmd->dy = y + img->y3;
	cmd->grshAddr = 0;
	cmd->dummy = 0;

	//console_print("%d %d %d,%d %d %x   ", x, y, swidth, cimg->sheight, img->spritesheet, cimg->vramaddr * 8);
}


void draw_box(int x, int y, int width, int height, Uint16 color)
{
	SprSpCmd * cmd = &spr_cmds[spritedex++];

	cmd->control = JUMP_NEXT | FUNC_POLYLINE;
	cmd->color = color;
	cmd->ax = x;
	cmd->ay = y;
	cmd->bx = x+width;
	cmd->by = y;
	cmd->cx = x+width;
	cmd->cy = y+height;
	cmd->dx = x;
	cmd->dy = y+height;
}

















/*void load_map(map * map, spr_instance * mainchar, int x, int y, Sint8 *bgimg, Sint8 * bgpal)
{
	fadeout();
	Uint16 cnt = TIM_FRT_MCR_TO_CNT(1000000);//render for 1 second to see fadeout
	TIM_FRT_SET_16(0);
	while ((cnt) > TIM_FRT_GET_16())
	{
		SCL_DisplayFrame();
	}

	map->width = 1024;
	map->height = 256;
	map->clipx = 0;
	map->clipy = 0;
	map->clipwidth = 720;
	map->clipheight = 256;
	mainchar->x = x;
	mainchar->y = y;

	//load bg image
	GfsHn gfs = openFileByName(bgimg);
	int dex;
	Uint8 *dest = (Uint8 *)SCL_VDP2_VRAM_A0;
	for (dex = 0; dex < 1024 * 256 / SECT_SIZE; dex++)
	{
		GFS_Fread(gfs, 1, buff, SECT_SIZE);
		memcpy(dest, buff, SECT_SIZE);
		dest += SECT_SIZE;
	}
	GFS_Close(gfs);

	//load bg palete
	gfs = openFileByName(bgpal);
	GFS_Fread(gfs, 1, buff, SECT_SIZE);
	GFS_Close(gfs);
	SCL_AllocColRam(SCL_NBG1, 256, OFF);
	SCL_SetColRam(SCL_NBG1, 0, 256, (void *)buff);

	fadein();
}*/



/*void spr_update(spr_instance * si, map * map, Uint16 curtick)
{
	Uint16 elapsed = get_elapsed(curtick, si->lastFrame);
	Uint16 elapsedms = TIM_FRT_CNT_TO_MCR(elapsed)/1000;

	Uint16 frameTime = si->currentAnim->frametime;
	if (si->currentAnim->frames[si->curFrame].specificframetime > 0)
		frameTime = si->currentAnim->frames[si->curFrame].specificframetime;
	if (elapsedms > frameTime)
	{
		si->lastFrame = curtick;
		if (si->curFrame < si->currentAnim->num_frame - 1)
		{
			si->curFrame++;
		}
		else
		{
			si->curFrame = si->currentAnim->loop ? 0 : si->curFrame;
		}

	}
}

void spr_set_anim(spr_instance * si, int animid, Uint16 curtick)
{
	if (animid != si->currentAnimId)
	{
		si->currentAnimId = animid;
		si->currentAnim = &si->sprite->anims[animid];
		si->curFrame = 0;
		si->lastFrame = curtick;
	}
}

void spr_draw(spr_instance * si, map * map, char_defs * cdefs)
{
	int dex = 0;
	for (dex = 0; dex < si->currentAnim->frames[si->curFrame].num_chars; dex++)
	{
		spr_charref * charref = &si->currentAnim->frames[si->curFrame].chars[dex];
		if (si->flipy)
		{
			draw_char(&cdefs->defs[charref->charid], (si->x - map->curxpos) - (cdefs->defs[charref->charid].width - charref->frameoffsetx), (si->y - map->curypos) - charref->frameoffsety, si->flipy);
		}
		else
		{
			draw_char(&cdefs->defs[charref->charid], (si->x - map->curxpos) - charref->frameoffsetx, (si->y - map->curypos) - charref->frameoffsety, si->flipy);
		}

	}
}*/

/*spr_def * load_sprite(Sint8 *fname)
{
	spr_def	*sprdef = 0x6050000;
	Uint8		*data;
	Uint8		*dest;

	GfsHn gfs = openFileByName(fname);
	GFS_Fread(gfs, 1, buff, SECT_SIZE);

	int		num_anims = *((int*)&buff[0]);

	data = buff;
	dest = sprdef;

	int sprdefsize = 4 + (num_anims)* sizeof(anim_def);
	readData(gfs, &data, &dest, sprdefsize);
	
	GFS_Close(gfs);

	return sprdef;
}

char_defs * load_chars(Sint8 *fname)
{
	char_defs	*cdefs = 0x6040000;
	Uint8		*data;
	Uint8		*dest;


	GfsHn gfs = openFileByName(fname);
	GFS_Fread(gfs, 1, buff, SECT_SIZE);

	int		num_cdefs = *((int*)&buff[0]);

	data = buff;
	dest = cdefs;
	int cdefssize = 4 + (num_cdefs) * sizeof(char_def);
	//read character definitions
	readData(gfs, &data, &dest, cdefssize);
	

	int dex;
	dest = &VRAM[1024];
	//read character data
	for (dex = 0; dex < cdefs->num_defs; dex++)
	{
		
		int datalength = cdefs->defs[dex].width * cdefs->defs[dex].height * cdefs->defs[dex].bpp / 8;
		
		//debugBreak(datalength);
		readData(gfs, &data, &dest, datalength);
	}

	GFS_Close(gfs);

	return cdefs;
}*/


/*void draw_char(char_def *cdef, int x, int y, Uint8 flipy)
{
	SprSpCmd * cmd = &spr_cmds[spritedex++];

	cmd->control = JUMP_NEXT | FUNC_NORMALSP | (flipy ? DIR_LRREV : DIR_NOREV);
	cmd->charAddr = (1024 + cdef->data_offset) / 8;
	cmd->charSize = (cdef->width / 8) << 8 | cdef->height;
	cmd->ax = x;
	cmd->ay = y;
}*/

int debugBreak(int checkval)
{
	if(checkval == 99)
	{
		return 99;
	}
	else
	{
		return checkval;
	}

}