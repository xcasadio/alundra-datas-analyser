/*----------------------------------------------------------------------------
 *  V_Blank.c -- V-Blank割り込み処理内ルーチンサンプル
 *  Copyright(c) 1994 SEGA
 *  Written by K.M on 1994-05-16 Ver.1.00
 *  Updated by K.M on 1994-09-21 Ver.1.00
 *
 *  UsrVblankStart()：V-Blank開始割り込み処理サンプル
 *  UsrVblankEnd()  ：V-Blank終了割り込み処理サンプル
 *
 *----------------------------------------------------------------------------
 */

#include	<machine.h>
#include	<sega_spr.h>
#include	<sega_scl.h>
#include	<sgl.h> 
#include	"../PER/SMPCLIB/per_x.h"

volatile trigger_t	PadData1  = 0x0000;
volatile trigger_t	PadData1E = 0x0000;
volatile trigger_t	PadData2  = 0x0000;
volatile trigger_t	PadData2E = 0x0000;
SysPort	*__port = NULL;

void	UsrVblankIn( void ){
	BlankIn();
}

void   UsrVblankOut( void ){
	BlankOut();
	
	if( __port != NULL ){
		const SysDevice	*device;
		
		PER_GetPort( __port );
		
		if(( device = PER_GetDeviceR( &__port[0], 0 )) != NULL ){
			trigger_t	prev = PadData1;
			trigger_t	current = PER_GetTrigger( device );
			
			PadData1  = current;
			PadData1E = PER_GetPressEdge( prev, current );
		}
		else{
			PadData1 = PadData1E = 0;
		}
		
		if(( device = PER_GetDeviceR( &__port[1], 0 )) != NULL ){
			trigger_t	prev = PadData2;
			trigger_t	current = PER_GetTrigger( device );
			
			PadData2  = current;
			PadData2E = PER_GetPressEdge( prev, current );
		}
		else{
			PadData2 = PadData2E = 0;
		}
	}
}
