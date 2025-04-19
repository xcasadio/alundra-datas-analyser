/*----------------------------------------------------------------------------
 *  Set_VB.c -- V-BlankäÑÇËçûÇ›ÉãÅ[É`ÉìÇÃìoò^
 *  Copyright(c) 1994 SEGA
 *  Written by K.M on 1994-05-16 Ver.0.90
 *  Updated by K.M on 1994-10-04 Ver.1.02
 *----------------------------------------------------------------------------
 */

#include	<sega_xpt.h>
#include	<sega_int.h>
#include	"../PER/SMPCLIB/per_x.h"

void	UsrVblankIn( void );
void	UsrVblankOut( void );
extern SysPort	*__port;

void	SetVblank( void ){
	
	__port = PER_OpenPort();
	
	/* V-BlankäÑÇËçûÇ›ÉãÅ[É`ÉìÇÃìoò^ */
	INT_ChgMsk(INT_MSK_NULL,INT_MSK_VBLK_IN | INT_MSK_VBLK_OUT);
	INT_SetScuFunc(INT_SCU_VBLK_IN,UsrVblankIn);
	INT_SetScuFunc(INT_SCU_VBLK_OUT,UsrVblankOut);
	INT_ChgMsk(INT_MSK_VBLK_IN | INT_MSK_VBLK_OUT,INT_MSK_NULL);
}
