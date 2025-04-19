#ifndef ALUNDRA_EVENTS
#define ALUNDRA_EVENTS

#include	"alundra.h"
#include	"sprites.h"


typedef struct packed UnhandledCommand
{
	Uint16 cmd;
	Uint16 count;
}UnhandledCommand;

extern int numUnhandledCommands;
extern UnhandledCommand unhandledCommands[128];
extern int playerhascontrol;
extern void ProcessEvent(EventProgramState * eps, SpriteInstance * si);
extern void ProcessOneTimeEvent(Uint8* evt, SpriteInstance * si);






















#endif
