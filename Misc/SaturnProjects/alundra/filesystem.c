#include    <string.h> 
#include	"filesystem.h"

Uint32 lib_work[GFS_WORK_SIZE(MAX_OPEN) / sizeof(Uint32)];
GfsDirTbl dirtbl;
GfsDirName dirname[MAX_DIR];

volatile Uint8 buff[SECT_SIZE];

void initgfs()
{
	GFS_DIRTBL_TYPE(&dirtbl) = GFS_DIR_NAME;
	GFS_DIRTBL_DIRNAME(&dirtbl) = dirname;
	GFS_DIRTBL_NDIR(&dirtbl) = MAX_DIR;
	GFS_Init(MAX_OPEN, lib_work, &dirtbl);
}

GfsHn openFileByName(Sint8 *fname)
{
	Sint32 fid;
	fid = GFS_NameToId(fname);
	return GFS_Open(fid);
}

int readData(GfsHn gfs, Uint8** datap, Uint8** destp, int datalength)
{
	int datacopied = 0;

	while (datacopied < datalength)
	{
		if ((*datap - buff) == SECT_SIZE)
		{
			GFS_Fread(gfs, 1, (void *)buff, SECT_SIZE);
			*datap = (Uint8*)buff;
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

Uint32 flipu32(Uint32 in)
{
	return ((in & 0xff) << 24) | ((in & 0xff00) << 8) | ((in & 0xff0000) >> 8) | ((in & 0xff000000) >> 24);
}

Uint16 flipu16(Uint16 in)
{
	return ((in & 0xff) << 8) | ((in & 0xff00) >> 8);
}

Sint16 flip16(Sint16 in)
{
	
	return (Sint16)((((Uint16)in & 0xff) << 8) | (((Uint16)in & 0xff00) >> 8));
}

void memcpyswap(Uint8 * dest, Uint8* src, int length)
{
	int dex;
	for (dex = 0; dex < length; dex++)
	{
		Uint8 i = src[dex];
		dest[dex] = ((i & 0xf) << 4) | ((i & 0xf0) >> 4);
	}
}


