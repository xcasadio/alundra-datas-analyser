#include    <string.h> 
#include	<stdarg.h>
#include	<machine.h>
#define		_SPR2_
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

GfsHn openFileByName(GfsDirTbl *dirtbl, Sint8 *fname);
Uint32 lib_work[GFS_WORK_SIZE(MAX_OPEN) / sizeof(Uint32)];
GfsDirTbl dirtbl;
GfsDirName dirname[MAX_DIR];

volatile unsigned char buff[SECT_SIZE];
volatile Uint8		*VRAM;

typedef struct
{
	Sint32 width;
	Sint32 height;
	Uint32 bpp;
	Uint32 palette_id;
	Uint32 data_offset;
} char_def;

typedef struct
{
	Uint32 num_defs;
	char_def defs[];
} char_defs;

typedef struct
{
	Uint16 charid;
	Sint16  frameoffsetx;
	Sint16  frameoffsety;
}spr_charref;
typedef struct
{
	Uint16 specificframetime;
	Uint16 num_chars;
	spr_charref chars[4];
}spr_frame;

typedef struct
{
	Sint8 name[20];
	Uint16 frametime;
	Uint8	loop;
	Uint8 num_frame;
	spr_frame frames[8];
} anim_def;

typedef struct
{
	Uint32 num_animations;
	anim_def anims[];
} spr_def;

typedef struct
{
	spr_def * sprite;
	int currentAnimId;
	anim_def * currentAnim;
	int curFrame;
	Uint32 lastFrame;
	int x;
	int y;
	Uint8 flipy;

}spr_instance;

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


Uint16 get_elapsed(Uint16 tick1, Uint16 tick2);
void debug_print(int x, int y, const char *control, ...);
char_defs * load_chars(Sint8 *fname);
spr_def * load_sprite(Sint8 *fname);
void draw_char(char_def *cdef, int x, int y, Uint8 flipy);
void draw_box(int x, int y, int width, int height, Uint16 color);
void spr_update(spr_instance * si, map * map, Uint16 curtick);
void spr_set_anim(spr_instance * si, int animid, Uint16 curtick);
void spr_draw(spr_instance * si, map * map, char_defs * cdefs);

int spritedex;
int dbg_on = 0;

Uint16 currenttick;
Uint16 lasttick;
Uint16 elapsedticks;

#define MAX_SPRITES		7

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
    /* control  */ (SKIP_NEXT | FUNC_NORMALSP),
    /* link     */ 0,
    /* drawMode */ (ECD_DISABLE | COLOR_5 | COMPO_REP),
    /* color    */ 0,
    /* charAddr */ 0,  /* 1024 byte pos */
    /* charSize */ 0,  /* x*y = 32*72     */
    /* ax, ay   */   0,   0,
    /* bx, by   */   0,   0,
    /* cx, cy   */   0,   0,
    /* dx, dy   */   0,   0,
    /* grshAddr */   0,
    /* dummy    */   0},

	{/* [ 3]     */
    /* control  */ (SKIP_NEXT | FUNC_NORMALSP),
    /* link     */ 0,
    /* drawMode */ (ECD_DISABLE | COLOR_5 | COMPO_REP),
    /* color    */ 0,
    /* charAddr */ 0, 
    /* charSize */ 0,  
    /* ax, ay   */   0,   0,
    /* bx, by   */   0,   0,
    /* cx, cy   */   0,   0,
    /* dx, dy   */   0,   0,
    /* grshAddr */   0,
    /* dummy    */   0},

	{/* [ 4]     */
    /* control  */ (SKIP_NEXT | FUNC_NORMALSP),
    /* link     */ 0,
    /* drawMode */ (ECD_DISABLE | COLOR_5 | COMPO_REP),
    /* color    */ 0,
    /* charAddr */ 0, 
    /* charSize */ 0,  
    /* ax, ay   */   0,   0,
    /* bx, by   */   0,   0,
    /* cx, cy   */   0,   0,
    /* dx, dy   */   0,   0,
    /* grshAddr */   0,
    /* dummy    */   0},

	{/* [ 5]     */
    /* control  */ (SKIP_NEXT | FUNC_NORMALSP),
    /* link     */ 0,
    /* drawMode */ (ECD_DISABLE | COLOR_5 | COMPO_REP),
    /* color    */ 0,
    /* charAddr */ 0, 
    /* charSize */ 0,  
    /* ax, ay   */   0,   0,
    /* bx, by   */   0,   0,
    /* cx, cy   */   0,   0,
    /* dx, dy   */   0,   0,
    /* grshAddr */   0,
    /* dummy    */   0},

	{/* [ 6]     */
    /* control  */ (SKIP_NEXT | FUNC_NORMALSP),
    /* link     */ 0,
    /* drawMode */ (ECD_DISABLE | COLOR_5 | COMPO_REP),
    /* color    */ 0,
    /* charAddr */ 0, 
    /* charSize */ 0,  
    /* ax, ay   */   0,   0,
    /* bx, by   */   0,   0,
    /* cx, cy   */   0,   0,
    /* dx, dy   */   0,   0,
    /* grshAddr */   0,
    /* dummy    */   0},

	{/* [ 7]     */
    /* control  */ (SKIP_NEXT | FUNC_NORMALSP),
    /* link     */ 0,
    /* drawMode */ (ECD_DISABLE | COLOR_5 | COMPO_REP),
    /* color    */ 0,
    /* charAddr */ 0, 
    /* charSize */ 0,  
    /* ax, ay   */   0,   0,
    /* bx, by   */   0,   0,
    /* cx, cy   */   0,   0,
    /* dx, dy   */   0,   0,
    /* grshAddr */   0,
    /* dummy    */   0},

	{/* [ 8]     */
    /* control  */ (SKIP_NEXT | FUNC_NORMALSP),
    /* link     */ 0,
    /* drawMode */ (ECD_DISABLE | COLOR_5 | COMPO_REP),
    /* color    */ 0,
    /* charAddr */ 0, 
    /* charSize */ 0,  
    /* ax, ay   */   0,   0,
    /* bx, by   */   0,   0,
    /* cx, cy   */   0,   0,
    /* dx, dy   */   0,   0,
    /* grshAddr */   0,
    /* dummy    */   0},

   {/* END      */
    /* control  */ CTRL_END,
    /* link     */ 0,
    /* drawMode */ 0,
    /* color    */ 0,
    /* charAddr */ 0,
    /* charSize */ 0,
    /* ax, ay   */ 0,
    /* bx, by   */ 0,
    /* cx, cy   */ 0,
    /* dx, dy   */ 0,
    /* grshAddr */ 0,
    /* dummy    */ 0}
};

SprSpCmd * spr_cmds;;


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

	SCL_SetPriority(SCL_NBG0, 6);
	SCL_SetPriority(SCL_NBG1, 7);
	SCL_SetPriority(SCL_SP0 | SCL_SP1 | SCL_SP2 | SCL_SP3 |
		SCL_SP4 | SCL_SP5 | SCL_SP6 | SCL_SP7, 7);

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

void initgfs()
{
	GFS_DIRTBL_TYPE(&dirtbl) = GFS_DIR_NAME;
	GFS_DIRTBL_DIRNAME(&dirtbl) = dirname;
	GFS_DIRTBL_NDIR(&dirtbl) = MAX_DIR;
	GFS_Init(MAX_OPEN, lib_work, &dirtbl);
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

void load_map(map * map, spr_instance * mainchar, int x, int y, Sint8 *bgimg, Sint8 * bgpal)
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
	GfsHn gfs = openFileByName(&dirtbl, bgimg);
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
	gfs = openFileByName(&dirtbl, bgpal);
	GFS_Fread(gfs, 1, buff, SECT_SIZE);
	GFS_Close(gfs);
	SCL_AllocColRam(SCL_NBG1, 256, OFF);
	SCL_SetColRam(SCL_NBG1, 0, 256, (void *)buff);

	fadein();
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
	char_defs *cdefs = load_chars("TESTING.CHR");
	//load sprite
	spr_def *sprdef = load_sprite("TESTING.SPR");


	//set up main character sprite instance
	//move this to create_spr_instance function
	spr_instance mainchar;
	mainchar.sprite = sprdef;
	mainchar.currentAnimId = -1;
	spr_set_anim(&mainchar, 0, 0);
	mainchar.flipy = 0;

	map map;
	map.curxpos = 0;
	map.curypos = 0;

	load_map(&map, &mainchar, 150, 120, "STT_01.IMG", "STT_01.PAL");

	SCL_DisplayFrame();
	

	
	lasttick = TIM_FRT_GET_16();
	elapsedticks = 0;

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
			mainchar.x += moveamount;
			spr_set_anim(&mainchar, 1, currenttick);
			mainchar.flipy = 1;
			moved = 1;
		}
		else if (PadData1 & PAD_L)
		{
			mainchar.x -= moveamount;
			spr_set_anim(&mainchar, 1, currenttick);
			mainchar.flipy = 0;
			moved = 1;
		}

		if (PadData1 & PAD_U)
		{
			mainchar.y -= moveamount;
			spr_set_anim(&mainchar, 1, currenttick);
			moved = 1;
		}
		else if (PadData1 & PAD_D)
		{
			mainchar.y += moveamount;
			spr_set_anim(&mainchar, 1, currenttick);
			moved = 1;
		}

		if (!moved)
		{
			spr_set_anim(&mainchar, 0, currenttick);
		}

		//update sprites
		spr_update(&mainchar, &map, currenttick);

		if (mainchar.x > map.clipx + map.clipwidth + 10)
			load_map(&map, &mainchar, map.clipx - 10, mainchar.y, "STT_01.IMG", "STT_01.PAL");
		else if (mainchar.x < map.clipx - 10)
			load_map(&map, &mainchar, map.clipx + map.clipwidth + 10, mainchar.y, "STT_01.IMG", "STT_01.PAL");

		//move map camera
		map.curxpos = mainchar.x - SCREEN_WIDTH/2;
		map.curypos = mainchar.y - SCREEN_HEIGHT/2;
		if (map.curxpos < map.clipx)
			map.curxpos = map.clipx;
		if (map.curxpos > map.clipx + map.clipwidth - SCREEN_WIDTH)
			map.curxpos = map.clipx + map.clipwidth - SCREEN_WIDTH;

		if (map.curypos < map.clipy)
			map.curypos = map.clipy;
		if (map.curypos > map.clipy + map.clipheight - SCREEN_HEIGHT)
			map.curypos = map.clipy + map.clipheight - SCREEN_HEIGHT;

		//move bg to match camera
		SCL_Open(SCL_NBG1);
		SCL_MoveTo(FIXED(map.curxpos), FIXED(map.curypos), FIXED(0));
		SCL_Close();

		

		//draw sprites

		//reset spritedex
		spritedex = 0;

		//draw_box(mainchar.x, mainchar.y, 2, 2, RGB16_COLOR(31, 0, 0));
		
		spr_draw(&mainchar, &map, cdefs);
		
		//skip the remaining sprite commands
		while (spritedex < MAX_SPRITES)
		{
			spr_cmds[spritedex++].control = SKIP_NEXT;
		}

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

GfsHn openFileByName(GfsDirTbl *dirtbl, Sint8 *fname)
{
	Sint32 fid;
	fid = GFS_NameToId(fname);
	return GFS_Open(fid);
}

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


int readData(GfsHn gfs, Uint8** datap, Uint8** destp, int datalength)
{
	int datacopied = 0;

	while (datacopied < datalength)
	{
		if ((*datap - buff) == SECT_SIZE)
		{
			GFS_Fread(gfs, 1, buff, SECT_SIZE);
			*datap = buff;
		}
		int len = datalength - datacopied;
		if (len > SECT_SIZE - (*datap - buff))
			len = SECT_SIZE - (*datap - buff);

		//debugBreak(vrampos);
		//debugBreak(data - buff);
		//debugBreak(len);

		memcpy(*destp, *datap, len);
		*destp += len;
		datacopied += len;
		*datap += len;
	}

	return datacopied;
}

spr_def * load_sprite(Sint8 *fname)
{
	spr_def	*sprdef = 0x6050000;
	Uint8		*data;
	Uint8		*dest;

	GfsHn gfs = openFileByName(&dirtbl, fname);
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


	GfsHn gfs = openFileByName(&dirtbl, fname);
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
}


void draw_char(char_def *cdef, int x, int y, Uint8 flipy)
{
	SprSpCmd * cmd = &spr_cmds[spritedex++];

	cmd->control = JUMP_NEXT | FUNC_NORMALSP | (flipy ? DIR_LRREV : DIR_NOREV);
	cmd->charAddr = (1024 + cdef->data_offset) / 8;
	cmd->charSize = (cdef->width / 8) << 8 | cdef->height;
	cmd->ax = x;
	cmd->ay = y;
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

void spr_update(spr_instance * si, map * map, Uint16 curtick)
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
}