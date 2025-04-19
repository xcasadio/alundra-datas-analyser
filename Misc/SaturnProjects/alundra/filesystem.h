#ifndef ALUNDRA_FILE
#define ALUNDRA_FILE

#include	<machine.h>
#include	<sega_gfs.h>

#define MAX_OPEN        1
#define MAX_DIR         100
#define SECT_SIZE       2048

#define FLIPU32(u32)	u32 = flipu32(u32)
#define FLIPU16(u16)	u16 = flipu16(u16)
#define FLIP16(i16)		i16 = flip16(i16)

extern volatile unsigned char buff[SECT_SIZE];

void initgfs();
GfsHn openFileByName(Sint8 *fname);
int readData(GfsHn gfs, Uint8** datap, Uint8** destp, int datalength);

Uint32 flipu32(Uint32 in);
Uint16 flipu16(Uint16 in);
Sint16 flip16(Sint16 in);

void memcpyswap(Uint8 * dest, Uint8* src, int length);


#endif

