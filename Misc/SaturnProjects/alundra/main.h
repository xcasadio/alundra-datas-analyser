#ifndef MAINH
#define MAINH






#endif
















//char_defs * load_chars(Sint8 *fname);
//spr_def * load_sprite(Sint8 *fname);
//void draw_char(char_def *cdef, int x, int y, Uint8 flipy);
//void spr_update(spr_instance * si, map * map, Uint16 curtick);
//void spr_set_anim(spr_instance * si, int animid, Uint16 curtick);
//void spr_draw(spr_instance * si, map * map, char_defs * cdefs);


/*
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
}map;*/

