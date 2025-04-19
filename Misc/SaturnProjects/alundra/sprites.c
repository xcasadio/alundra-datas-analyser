#include <SEGA_TIM.H>
#include "sprites.h"

volatile SpriteInstance * mastersprites = GEN_SI_ADDR - sizeof(DBHeader) - (sizeof(SpriteInstance) * MAX_MASTER_SPRITES);
volatile MapEventInstance * mastermapevents = GEN_SI_ADDR - sizeof(DBHeader)-(sizeof(SpriteInstance)* MAX_MASTER_SPRITES) - (sizeof(MapEventInstance)* MAX_MAP_EVENTS);
volatile SpriteCache * spritecache = 0x00200000;//workramlow enough room for 8k here

Uint8* spritesheetimagedata;
Uint32 nextvramaddr;
Uint32 totalloadedsprites;

void init_spritecache(Uint8* imagedata)
{
	spritesheetimagedata = imagedata;
	/*int dex;
	for (dex = 0; dex < 5; dex++)
	{
		spritecache[dex].numimages = 0;
	}*/
	spritecache->numimages = 0;
	nextvramaddr = SPRITES_ADDR;
	totalloadedsprites = 0;
	//console_print("%8x", 0);
	//console_print("%8x", TILESETS_ADDR);
	//console_print("%8x", CLUTS_ADDR);
	//console_print("%8x", SPRITES_ADDR);
}

CachedImage * cache_image(DBImage * img, int isgenimage)
{

	int dex;
	SpriteCache * cache = spritecache;// &spritecache[img->spritesheet];

	if (img->cachedimage >= cache && img->cachedimage < (Uint32)cache + 8000)//if its already been cached
	{
		return img->cachedimage;
	}
	else
	{
		//look for existing match
		for (dex = 0; dex < cache->numimages; dex++)
		{
			if (cache->images[dex].signature == img->cachedimage && ((cache->images[dex].cacheindex >> 15) == isgenimage))
			{
				process_image(img, 0);//process image without loading it (its already loaded)
				img->cachedimage = &cache->images[dex];//overwrite signature with pointer to cached image
				return img->cachedimage;//a cache match was found
			}
		}
		
		//create new cache image
		CachedImage * cimg = &cache->images[cache->numimages];

		//console_print("processing %x x:%d y:%d w:%d h:%d",img->cachedimage, img->sx, img->sy, img->swidth, img->sheight);

		cimg->cacheindex = (isgenimage << 15) | cache->numimages;
		cimg->signature = img->cachedimage;

		//if (cimg->sheight > 50)
		//	console_print("cacheing x:%d y:%d w:%d h:%d", cimg->sx, cimg->sy, cimg->swidth, cimg->sheight);

		//load to vram and set address
		cimg->vramaddr = process_image(img, 1);

		//console_print("%8x", cimg->vramaddr*8);

		

		img->cachedimage = cimg;//overwrite signature with pointer to cached image

		cache->numimages++;

		return cimg;
	}
}


Uint16 process_image(DBImage * img, int load)
{

	int shiftleft = img->sx % 2 == 1;
	int swidth = img->swidth;
	int readwidth = swidth;
	int outputwidth = img->swidth;
	if (outputwidth % 8 > 0)//make output interval of 8
		outputwidth += 8 - outputwidth % 8;
	if (shiftleft)//make sure theres an extra byte if shifting left
		readwidth++;
	if (readwidth % 2 == 1)// or if odd width
		readwidth++;


	if (load)
	{

		//console_print("writing to %x with %d,%d %d %d", nextvramaddr, img->sx, img->sy, img->swidth, img->sheight);

		Uint8 * buff = VRAM + nextvramaddr;
		Uint8 readbuff[256 / 2];

		int y;
		for (y = 0; y < img->sheight; y++)
		{
			memcpyswap(readbuff, &spritesheetimagedata[((img->spritesheet & 0x7) * 256 + img->sy + y) * 256 / 2 + img->sx / 2], readwidth / 2);
			
			if (shiftleft)
			{
				int dex;
				for (dex = 0; dex < 256 / 2 - 1; dex++)
				{
					buff[y * outputwidth / 2 + dex] = (Uint8)((readbuff[dex] & 0x0f) << 4 | (readbuff[dex + 1] & 0xf0) >> 4);
				}

			}
			else
			{
				memcpy(&buff[y * outputwidth / 2], readbuff, readwidth / 2);
			}

			if (swidth % 2 == 1)
			{
				buff[y * outputwidth / 2 + swidth / 2] = (Uint8)(buff[y * outputwidth / 2 + swidth / 2] & 0x0f);
			}

		}
	}


	if (outputwidth > swidth)
	{
		//img->swidth = (byte)outputwidth;

		float vec1x = (img->x2 - img->x1) / (float)swidth;//get the normalized (normalized to ratio of swidth/outputwidth) vector of point 1
		float vec1y = (img->y2 - img->y1) / (float)swidth;

		float vec3x = (img->x4 - img->x3) / (float)swidth;//get normalized vector of point 3
		float vec3y = (img->y4 - img->y3) / (float)swidth;

		img->x2 = (Sint8)(img->x1 + (vec1x * outputwidth));//extend point 2 to new width
		img->y2 = (Sint8)(img->y1 + (vec1y * outputwidth));

		img->x4 = (Sint8)(img->x3 + (vec3x * outputwidth));//extend point 4 to new width
		img->y4 = (Sint8)(img->y3 + (vec3y * outputwidth));
	}

	if (load)
	{

		Uint16 addr = nextvramaddr / 8;
		nextvramaddr += (outputwidth*img->sheight) / 2;
		if (nextvramaddr % 8 > 0)
			nextvramaddr += 8 - nextvramaddr % 8;
		totalloadedsprites++;
		return addr;
	}
	return 0;
}

int get_starting_animid_from_code(DBEntity * entity, DBSprite * spr, Uint8* code)
{
	for (;;)
	{
		Uint8 cmd = *code;
		if (cmd == 0)
			break;
		if (cmd == 0xff)
			break;
		if (cmd == 0x1a)
		{
			return code[1];
		}
		Uint16 size = processeventcode(code);
		if (size == 0)
			break;
		code += size;
	}
	return -1;
}


int get_starting_animid(DBSpriteInfo * sinfo, DBSprite * spr, DBEntity * entity)
{
	int animid = -1;
	if (entity->eventcodesaindex != 0 && entity->eventcodesaindex != 0xff)
	{
		Uint8* code = CODESADATA(sinfo, entity);
		animid = get_starting_animid_from_code(entity, spr, code);
	}
	if (animid == -1 && entity->eventcodescindex != 0 && entity->eventcodescindex != 0xff)
	{
		Uint8* code = CODESCDATA(sinfo, entity);
		animid = get_starting_animid_from_code(entity, spr, code);
	}
	return animid != -1 ? animid : 0;
}

int loadmapevents(DBSpriteInfo * sinfo, MapEventInstance mapevents[], Uint16 curtick)
{
	int dex;
	for (dex = 0; dex < MAX_MAP_EVENTS; dex++)
	{
		DBMapEvent * dbme = &sinfo->mapevents->mapevents[dex];
		if (dbme->u1 == 0 && dbme->u2 == 0)
			break;
		MapEventInstance *ei = &mapevents[dex];

		//load tick program
		if (dbme->eventprogram1bindex != 0)
		{

			ei->tickprogram.sp = CODESBDATA(sinfo, dbme);
			ei->tickprogram.exp = ei->tickprogram.sp;
		}
		else
		{
			ei->tickprogram.sp = -1;
			ei->tickprogram.exp = -1;
		}
		ei->tickprogram.elapsedMs = 0;
		ei->tickprogram.isWaiting = 0;
		ei->tickprogram.logicResult = 0;

		ei->lastTick = curtick;
	}

	return dex;
}

int loadsprites(DBSpriteInfo * sinfo, SpriteInstance sprites[], Uint16 curtick)
{
	int dex;
	for (dex = 0; dex < MAX_MASTER_SPRITES; dex++)
	{
		DBEntity * dbe = &sinfo->entities->entities[dex];
		if (dbe->u1 == 0 && dbe->u2 == 0)
			break;
		SpriteInstance *spr = &sprites[dex];

		spr->maploadprogram = -1;
		spr->tickprogram.sp = -1;
		spr->tickprogram.exp = -1;
		spr->interactprogram = -1;
		//load tick program
		if (dbe->eventcodescindex != 0)
		{
			spr->tickprogram.sp = CODESCDATA(sinfo, dbe);
			spr->tickprogram.exp = spr->tickprogram.sp;
		}
		spr->tickprogram.elapsedMs = 0;
		spr->tickprogram.isWaiting = 0;
		spr->tickprogram.logicResult = 0;

		if (dbe->eventcodesaindex != 0)
		{
			spr->maploadprogram = CODESADATA(sinfo, dbe);
		}
		if (dbe->eventcodesfindex != 0)
		{
			spr->interactprogram = CODESFDATA(sinfo, dbe);
		}


		spr->x = dbe->xpos * 12 + 12;
		spr->y = dbe->ypos * 8 + 8;
		spr->z = dbe->height * 8;

		if (dbe->spritetypeanddir >> 4 == 0x0 || dbe->spritetypeanddir >> 4 == 0x4)
		{
			spr->spr = (*gensi->spritetable)[dbe->spritetableindex];
		}
		else if (dbe->spritetypeanddir >> 4 == 0x8 || dbe->spritetypeanddir >> 4 == 0xc)
		{
			spr->spr = (*sinfo->spritetable)[dbe->spritetableindex];
		}
		spr->enabled = dbe->spritetypeanddir >> 6;
		spr->dir = dbe->spritetypeanddir & 0x3f;
		spr->tempdir = spr->dir;
		if (spr->spr != -1)
		{
			
			
			spr->gravity = spr->spr->gravityandcanpickup & 1;
			spr->canpickup = spr->spr->gravityandcanpickup >> 1 & 1;
			spr->shadowtype = spr->spr->portraitandshadowtype & 0x3f;//probably not this many bits
			spr->hasportrait = spr-spr->portraitandshadowtype >> 7 & 1;
			spr->canthrow = spr->spr->throwtype;
			spr->canstack = spr->spr->stackable >> 4 & 1;

			//crashsound
			//crashanim
			//contents

			spr->elapsedMs = 0;
			spr->lastTick = curtick;
			set_sprite_anim(spr, get_starting_animid(sinfo, spr->spr, dbe), curtick);
		}
		else
		{
			//defaults for some of these?
		}
	}
	return dex;
}

void update_mapevent(MapEventInstance * ei, DBMap * map, Uint16 curtick)
{
	Uint16 elapsed = get_elapsed(curtick, ei->lastTick);
	Uint16 elapsedms = TIM_FRT_CNT_TO_MCR(elapsed) / 1000;
	ei->tickprogram.elapsedMs += elapsedms;
	ei->lastTick = curtick;
}

void update_sprite(SpriteInstance * si, DBMap * map, Uint16 curtick)
{
	if (si->enabled >> 1 == 0)
		return;
	Uint16 elapsed = get_elapsed(curtick, si->lastTick);
	Uint16 elapsedms = TIM_FRT_CNT_TO_MCR(elapsed) / 1000;
	si->elapsedMs += elapsedms;
	si->tickprogram.elapsedMs = si->elapsedMs;
	si->lastTick= curtick;

	si->currentAnim = GETFRAME(si->spr, si->currentAnimId, si->dir);


	Uint16 frameTime = (si->currentAnim[si->curFrame].delay & 0x7f) * 23;
	if (si->elapsedMs > frameTime)
	{
		si->elapsedMs = 0;
		//set num frames
		int numframes;
		for (numframes = 0; numframes < 32; numframes++)
		{
			DBFrame * frame = &si->currentAnim[numframes];
			if ((frame->delay & 0x80) != 0x80)
				break;
		}

		if (si->curFrame < numframes - 1)
		{
			si->curFrame++;
		}
		else
		{
			si->curFrame = /*si->currentAnim->loop ?*/ 0 /*: si->curFrame*/;
		}

	}

	si->tilex = si->x / 24;
	si->tiley = si->y / 16;
	si->tilez = si->z / 16;
}

void set_sprite_anim(SpriteInstance * si, int animid, Uint16 curtick)
{
	if (animid != si->currentAnimId)
	{
		si->currentAnimId = animid;
		si->curFrame = 0;
		si->lastTick = curtick;
		si->elapsedMs = 0;
		update_sprite(si, 0, curtick);
	}
}
