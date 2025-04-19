#include	"datasbin.h"
#include    "sprites.h"

volatile DBHeader * dbheader = GEN_SI_ADDR - sizeof(DBHeader);
volatile DBGameMap * dbmap = GAMEMAP_ADDR;
volatile DBSpriteInfo * gensi = GEN_SI_ADDR;

DBHeader * initdatas()
{
	GfsHn gfs = openFileByName("DATAS.BIN");
	Uint8*datap = buff+SECT_SIZE;
	Uint8*destp = (Uint8*)dbheader;
	readData(gfs, &datap, &destp, sizeof(DBHeader));


	FLIPU32(dbheader->alundraspriteinfooffset);
	FLIPU32(dbheader->alundraspritesheetsoffset);
	FLIPU32(dbheader->alundraspritesheetsoffset2);
	FLIPU32(dbheader->alundrastringtableoffset);
	FLIPU32(dbheader->alundrastringtableoffset2);
	FLIPU32(dbheader->unknownmapaoffset);
	FLIPU32(dbheader->unknownmapboffset);
	FLIPU32(dbheader->unknownmapb2offset);
	FLIPU32(dbheader->unknownmapb3offset);
	FLIPU32(dbheader->unknownmapb4offset);
	int sinfosize = dbheader->alundraspritesheetsoffset - dbheader->alundraspriteinfooffset;
	int sheetsize = dbheader->alundrastringtableoffset - dbheader->alundraspritesheetsoffset;
	console_print("sinfo:%x ssheets:%x", sinfosize, sheetsize);

	//read general spritesheets
	datap = buff + SECT_SIZE;
	destp = gensi;
	GFS_Seek(gfs, dbheader->alundraspritesheetsoffset / SECT_SIZE, GFS_SEEK_SET);
	readData(gfs, &datap, &destp, sheetsize);
	//deflate
	Uint8 * data = gensi;
	data += 6;//seek past EZ.tx
	volatile Uint8 * bitmap = 0x00202000;//work ram low + 8k for imagecache
	int deflated = deflate(data, bitmap, sheetsize - 6, 256 * 256 * 8 / 2);
	console_print("deflated:%x bytes of ssheets", deflated);


	//read general sprite definitions
	datap = buff + SECT_SIZE;
	destp = gensi;
	GFS_Seek(gfs, dbheader->alundraspriteinfooffset / SECT_SIZE, GFS_SEEK_SET);
	readData(gfs, &datap, &destp, sinfosize);

	GFS_Close(gfs);

	int dex,cdex;

	//set sprite relative pointers
	DBSpriteInfo * sinfo = gensi;
	sinfo->entities = flipu32(sinfo->entities) + (Uint32)sinfo;
	sinfo->sector3 = flipu32(sinfo->sector3) + (Uint32)sinfo;
	sinfo->mapevents = flipu32(sinfo->mapevents) + (Uint32)sinfo;
	sinfo->spritetable = flipu32(sinfo->spritetable) + (Uint32)sinfo;
	sinfo->unknown = flipu32(sinfo->unknown) + (Uint32)sinfo;
	sinfo->spritepalettes = flipu32(sinfo->spritepalettes) + (Uint32)sinfo;
	sinfo->eventcodesa = flipu32(sinfo->eventcodesa) + (Uint32)sinfo;
	sinfo->eventcodesb = flipu32(sinfo->eventcodesb) + (Uint32)sinfo;
	sinfo->eventcodesc = flipu32(sinfo->eventcodesc) + (Uint32)sinfo;
	sinfo->eventcodesd = flipu32(sinfo->eventcodesd) + (Uint32)sinfo;
	sinfo->eventcodese = flipu32(sinfo->eventcodese) + (Uint32)sinfo;
	sinfo->eventcodesf = flipu32(sinfo->eventcodesf) + (Uint32)sinfo;


	//byteswap palette colors
	int numpalettes = ((Uint32)sinfo->eventcodesa - (Uint32)sinfo->spritepalettes) / CLUT_SIZE;
	for (dex = 0; dex < numpalettes; dex++)
	{
		DBPalette * pal = &(*sinfo->spritepalettes)[dex];

		for (cdex = 0; cdex < 16; cdex++)
		{
			Uint16 c = pal->colors[cdex];
			c = ((c & 0xff) << 8) | ((c & 0xff00) >> 8);
			if (c != 0)
				c |= 0x8000;//turn on top bit so its interpreted as an rgb color instead of a colorbank code
			pal->colors[cdex] = c;
		}
	}

	//load palettes into video memory
	memcpy(VRAM + GENSI_CLUTS_ADDR, *sinfo->spritepalettes, CLUT_SIZE * numpalettes);


	init_spritecache(bitmap);


	int numimages = 0;
	for (dex = 0; dex < 255; dex++)
	{
		if ((*sinfo->spritetable)[dex] != 0xffffffff)
		{
			(*sinfo->spritetable)[dex] = flipu32((*sinfo->spritetable)[dex]) + (Uint32)sinfo;
			//preprocess sprite
			DBSprite * spr = (*sinfo->spritetable)[dex];

			spr->animsets = flipu32(spr->animsets) + (Uint32)sinfo;
			spr->framesdata = flipu32(spr->framesdata) + (Uint32)sinfo;
			spr->unknowndata = flipu32(spr->unknowndata) + (Uint32)sinfo;
			spr->imagesetdata = flipu32(spr->imagesetdata) + (Uint32)sinfo;


			int numanim = ((Uint32)spr->framesdata - (Uint32)spr->animsets) / 14;


			int animdex;
			for (animdex = 0; animdex < numanim; animdex++)
			{
				FLIP16(spr->animsets[animdex].speed);
				int dirdex;
				for (dirdex = 0; dirdex < 4; dirdex++)
				{
					spr->animsets[animdex].diroffsets[dirdex] = flipu16(spr->animsets[animdex].diroffsets[dirdex]);
					if (spr->animsets[animdex].diroffsets[dirdex] != 0xffff)
					{
						DBFrame * frames = (DBFrame*)&(*spr->framesdata)[spr->animsets[animdex].diroffsets[dirdex]];
						int framedex;
						for (framedex = 0; framedex < 32; framedex++)
						{
							DBFrame * frame = &frames[framedex];
							if ((frame->delay & 0x80) != 0x80)
								break;

							DBImageSet* imageset = (DBImageSet*)&(*spr->imagesetdata)[flipu16(frame->imagesetoffset) << 1];

							int imagedex;
							for (imagedex = 0; imagedex < imageset->numimages; imagedex++)
							{
								numimages++;
								if (dex > 0)
									cache_image(&imageset->images[imagedex],1);
							}

						}

					}
				}


			}
		}
	}


	return dbheader;
}

DBGameMap *loadmap(int mapid, int*mapsize)
{
	DBGameMap * map = dbmap;
	//console_print("%d %d", flipu32(dbheader->gamemapoffsets[mapid]), flipu32(dbheader->gamemapoffsets[mapid + 1]));
	int offset = flipu32(dbheader->gamemapoffsets[mapid]);
	*mapsize = flipu32(dbheader->gamemapoffsets[mapid + 1]) - offset;

	GfsHn gfs = openFileByName("DATAS.BIN");
	Uint8*datap = buff + SECT_SIZE;
	Uint8*destp = map;
	GFS_Seek(gfs, offset / SECT_SIZE, GFS_SEEK_SET);
	readData(gfs, &datap, &destp, *mapsize);
	GFS_Close(gfs);

	//set relative pointers
	map->info = flipu32(map->info) + (Uint32)map;
	map->map = flipu32(map->map) + (Uint32)map;
	map->tilesheets = flipu32(map->tilesheets) + (Uint32)map;
	map->spriteinfo = flipu32(map->spriteinfo) + (Uint32)map;
	map->spritesheets = flipu32(map->spritesheets) + (Uint32)map;
	map->scrollscreen = flipu32(map->scrollscreen) + (Uint32)map;
	map->stringtable = flipu32(map->stringtable) + (Uint32)map;


	FLIP16(map->info->gravity);
	FLIP16(map->info->terminal_velocity);

	//byteswap palette colors
	int dex,cdex;
	for (dex = 0; dex < 32; dex++)
	{
		DBPalette * pal = &map->info->palettes[dex];
		for (cdex = 0; cdex < 16; cdex++)
		{
			Uint16 c = pal->colors[cdex];
			c = ((c & 0xff) << 8) | ((c & 0xff00) >> 8);
			if (c != 0)
				c |= 0x8000;//turn on top bit so its interpreted as an rgb color instead of a colorbank code
			pal->colors[cdex] = c;
		}
	}

	
	//load palettes into video memory
	memcpy(VRAM + CLUTS_ADDR, map->info->palettes, CLUT_SIZE * 32);

	//deflate tiles
	Uint8* data = map->tilesheets;
	data += 6;//seek past EZ.tx
	Uint8*bitmap = 0x00202000;//work ram low
	deflate(data, bitmap, ((Uint32)map->spriteinfo - (Uint32)map->tilesheets) - 6, 256 * 256 * 6 / 2);


	//load tiles into video memory
	int sheetdex;
	int tiledex = 0;
	for (sheetdex = 0; sheetdex < 6; sheetdex++)
	{
		int tilex;
		int tiley;
		for (tiley = 0; tiley < 16; tiley++)
		{
			for (tilex = 0; tilex < 10; tilex++)
			{
				int row;
				for (row = 0; row < 16; row++)
				{
					memcpyswap(VRAM + TILESETS_ADDR + tiledex*TILE_SIZE + row * 24 / 2, bitmap + sheetdex * 256 * 256 / 2 + (tiley * 16 + row) * 256 / 2 + (tilex * 24 / 2), 24 / 2);
				}
				tiledex++;
			}
		}
	}

	//deflate sprites
	data = map->spritesheets;
	data += 6;//seek past EZ.tx
	bitmap = 0x00202000;//work ram low + 8k for imagecache
	deflate(data, bitmap, ((Uint32)map->scrollscreen - (Uint32)map->spritesheets) - 6, 256 * 256 * 5 / 2);

	

	//set sprite relative pointers
	DBSpriteInfo * sinfo = map->spriteinfo;
	sinfo->entities = flipu32(sinfo->entities) + (Uint32)sinfo;
	sinfo->sector3 = flipu32(sinfo->sector3) + (Uint32)sinfo;
	sinfo->mapevents = flipu32(sinfo->mapevents) + (Uint32)sinfo;
	sinfo->spritetable = flipu32(sinfo->spritetable) + (Uint32)sinfo;
	sinfo->unknown = flipu32(sinfo->unknown) + (Uint32)sinfo;
	sinfo->spritepalettes = flipu32(sinfo->spritepalettes) + (Uint32)sinfo;
	sinfo->eventcodesa = flipu32(sinfo->eventcodesa) + (Uint32)sinfo;
	sinfo->eventcodesb = flipu32(sinfo->eventcodesb) + (Uint32)sinfo;
	sinfo->eventcodesc = flipu32(sinfo->eventcodesc) + (Uint32)sinfo;
	sinfo->eventcodesd = flipu32(sinfo->eventcodesd) + (Uint32)sinfo;
	sinfo->eventcodese = flipu32(sinfo->eventcodese) + (Uint32)sinfo;
	sinfo->eventcodesf = flipu32(sinfo->eventcodesf) + (Uint32)sinfo;


	//byteswap palette colors
	int numpalettes = ((Uint32)map->spriteinfo->eventcodesa - (Uint32)map->spriteinfo->spritepalettes) / CLUT_SIZE;
	for (dex = 0; dex < numpalettes; dex++)
	{
		DBPalette * pal = &(*map->spriteinfo->spritepalettes)[dex];
		
		for (cdex = 0; cdex < 16; cdex++)
		{
			Uint16 c = pal->colors[cdex];
			c = ((c & 0xff) << 8) | ((c & 0xff00) >> 8);
			if (c != 0)
				c |= 0x8000;//turn on top bit so its interpreted as an rgb color instead of a colorbank code
			pal->colors[cdex] = c;
		}
	}

	//load palettes into video memory
	memcpy(VRAM + SI_CLUTS_ADDR, *map->spriteinfo->spritepalettes, CLUT_SIZE * numpalettes);


	//init_spritecache(bitmap);

	int numimages = 0;
	for (dex = 0; dex < 255; dex++)
	{
		if ((*sinfo->spritetable)[dex] != 0xffffffff)
		{
			(*sinfo->spritetable)[dex] = flipu32((*sinfo->spritetable)[dex]) + (Uint32)sinfo;
			//preprocess sprite
			DBSprite * spr = (*sinfo->spritetable)[dex];

			spr->animsets = flipu32(spr->animsets) + (Uint32)sinfo;
			spr->framesdata = flipu32(spr->framesdata) + (Uint32)sinfo;
			spr->unknowndata = flipu32(spr->unknowndata) + (Uint32)sinfo;
			spr->imagesetdata = flipu32(spr->imagesetdata) + (Uint32)sinfo;
			

			int numanim = ((Uint32)spr->framesdata - (Uint32)spr->animsets) / 14;


			int animdex;
			for (animdex = 0; animdex < numanim; animdex++)
			{
				int dirdex;
				FLIP16(spr->animsets[animdex].speed);
				for (dirdex = 0; dirdex < 4; dirdex++)
				{
					spr->animsets[animdex].diroffsets[dirdex] = flipu16(spr->animsets[animdex].diroffsets[dirdex]);
					if (spr->animsets[animdex].diroffsets[dirdex] != 0xffff)
					{
						DBFrame * frames = (DBFrame*)&(*spr->framesdata)[spr->animsets[animdex].diroffsets[dirdex]];
						int framedex;
						for (framedex = 0; framedex < 32; framedex++)
						{
							DBFrame * frame = &frames[framedex];
							if ((frame->delay & 0x80) != 0x80)
								break;

							DBImageSet* imageset = (DBImageSet*)&(*spr->imagesetdata)[flipu16(frame->imagesetoffset) << 1];

							int imagedex;
							for (imagedex = 0; imagedex < imageset->numimages; imagedex++)
							{
								numimages++;
								cache_image(&imageset->images[imagedex],0);
							}

						}
						
					}
				}

				
			}
		}
	}
	
	return map;
}

int deflate(Uint8*data, Uint8*dest, int datalen, int destlen)
{
	int dex = 0;
	int buffdex = 0;
	while (dex < destlen && buffdex < datalen)
	{
		Uint8 b = data[buffdex++];
		if (b == 0xad)
		{
			int seek = data[buffdex++];
			if (seek == 0)
			{
				dest[dex++] = b;
			}
			else
			{
				int len = data[buffdex++];
				int seekdex = dex - seek;
				while (len-- > 0)
					dest[dex++] = dest[seekdex++];
			}
		}
		else
			dest[dex++] = b;
	}
	return dex;
}



Uint16 processeventcode(Uint8* code)
{
	Uint8 cmd = code[0];
	Uint16 size = 0;
	switch (cmd)
	{
		//logic/flow
	case 0x05:
		//name = "varon";
		size = 3;
		break;
	case 0x06:
		//name = "varoff";
		size = 3;
		break;
	case 0x30:
		//name = "if";
		size = 5;
		break;
	case 0x31:
		//name = "ifnot";
		size = 5;
		break;
	case 0x36:
		//name = "until";
		size = 3;
		break;
	case 0x02:
		//name = "jump";
		size = 3;
		break;


	case 0x44:
		//name = "waitdialogchoice";
		size = 1;
		break;
	case 0x51:
		//name = "getdialogchoice";
		size = 1;
		break;
	case 0x03:
		//name = "ifno";
		size = 3;
		break;



		//dialog
	case 0x0d:
		//name = "dialog";
		size = 3;
		break;
		//animation
	case 0x1a:
		//name = "setanim";
		size = 2;
		break;

	case 0x27:
		size = 1;
		break;
	case 0x09:
	case 0x2e:
	case 0x2d:
	case 0x50:
		size = 2;
		break;
	case 0xbd:
	case 0x59:
		size = 3;
		break;
	case 0xac:
	case 0x62:
	case 0x63:
		size = 4;
		break;
	case 0x64:
		size = 8;
		break;
	}
	return size;
}



