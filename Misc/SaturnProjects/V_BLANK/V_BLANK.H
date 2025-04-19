#if	!defined( __V_BLANK_H )
	#define	__V_BLANK_H
	
	/****************************************
	*			include files				*
	****************************************/
//	#include	"..\PER\smpclib\per_x.h"
	#include	"C:\SaturnOrbit\SBL_601\SEGASMP\PER\smpclib\per_x.h"
	
	/****************************************
	*			define constants			*
	****************************************/
	#define PAD_U	TRG_UP
	#define PAD_D	TRG_DOWN
	#define PAD_R	TRG_RIGHT
	#define PAD_L	TRG_LEFT
	#define PAD_A	TRG_A
	#define PAD_B	TRG_B
	#define PAD_C	TRG_C
	#define PAD_S	TRG_START
	#define PAD_X	TRG_X
	#define PAD_Y	TRG_Y
	#define PAD_Z	TRG_Z
	#define PAD_RB	TRG_R
	#define PAD_LB	TRG_L

	
	/****************************************
	*		declare external objects		*
	****************************************/
	extern volatile trigger_t	PadData1;
	extern volatile trigger_t	PadData1E;
	extern volatile trigger_t	PadData2;
	extern volatile trigger_t	PadData2E;

	extern volatile Uint32		VBLANK_CNT;
	
	/****************************************
	*			declare functions			*
	****************************************/
	void	SetVblank( void );
	
#endif
