/*----------------------------------------------------------------------*/
/*	Ascii Scroll							*/
/*----------------------------------------------------------------------*/
#include	"sgl.h"

#define		BACK_COL_ADR		( VDP2_VRAM_A1 + 0x1fffe )

void ss_main(void)
{

	slInitSystem(TV_320x224,NULL,1);
    slTVOff();
	slPrint("hello sgl" , slLocate(9,2));

	slColRAMMode(CRM16_1024);
	slBack1ColSet((void *)BACK_COL_ADR , 0);

	
	slScrAutoDisp(NBG0ON);
	slTVOn();

	while(1) {
		slSynch();
	} 
}

