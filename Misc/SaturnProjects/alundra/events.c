#include "events.h":


int numUnhandledCommands = 0;
UnhandledCommand unhandledCommands[128];
int EchNull(EventProgramState* eps, SpriteInstance* si)
{
	Uint8 cmd = eps->exp[0];
	//update count if its already in unhandled command list
	int dex;
	for (dex = 0; dex < numUnhandledCommands; dex++)
	{
		if (unhandledCommands[dex].cmd == cmd)
		{
			unhandledCommands[dex].count++;
			return 0;
		}
	}
	//otherwise add it to unhandled command list
	unhandledCommands[dex].cmd = cmd;
	unhandledCommands[dex].count = 1;
	numUnhandledCommands++;
	return 0;
}

/* 0x02 */
int EchGoto(EventProgramState* eps, SpriteInstance* si)
{
	return (Sint16)(eps->exp[1] | eps->exp[2] << 8);
}

/* 0x03 */
int EchIfFalse(EventProgramState* eps, SpriteInstance* si)
{
	if (eps->logicResult == 0)
		return 3;
	return (Sint16)(eps->exp[1] | eps->exp[2] << 8);
}

/* 0x04 */
int EchWhileFalse(EventProgramState* eps, SpriteInstance* si)
{
	if (eps->logicResult != 0)
		return 3;
	return (Sint16)(eps->exp[1] | eps->exp[2] << 8);
}

Uint32 mapgameflags[1024];
Uint32 globalgameflags[1024];
/* 0x05 */
int EchFlagOn(EventProgramState* eps, SpriteInstance* si)
{
	Uint16 flag = (Uint16)(eps->exp[1] | eps->exp[2] << 8);
	Uint16 flagdex = (flag >> 5) & 0x03ff;	//0111111111100000
	Uint8 flagval = flag & 0x1f;			//0000000000011111
	if (flag & 0x8000 == 0)					//1000000000000000
	{
		globalgameflags[flagdex] |= (1 << flagval);
	}
	else
	{
		mapgameflags[flagdex] |= (1 << flagval);
	}

	return 3;
}

/* 0x06 */
int EchFlagOff(EventProgramState* eps, SpriteInstance* si)
{
	Uint16 flag = (Uint16)(eps->exp[1] | eps->exp[2] << 8);
	Uint16 flagdex = (flag >> 5) & 0x03ff;	//0111111111100000
	Uint8 flagval = flag & 0x1f;			//0000000000011111
	if (flag & 0x8000 == 0)					//1000000000000000
	{
		globalgameflags[flagdex] &= ~(1 << flagval);
	}
	else
	{
		mapgameflags[flagdex] &= ~(1 << flagval);
	}

	return 3;
}

/* 0x07 */
int EchCheckEntityInArea(EventProgramState* eps, SpriteInstance* si)
{
	SpriteInstance* entity = si;
	int entityid = eps->exp[1];
	if (entityid & 0x8000 == 0)
	{
		entity = &mastersprites[entityid];
	}

	Uint8 x = entity->tilex;
	Uint8 y = entity->tiley;
	Uint8 z = entity->tilez;

	if (x >= eps->exp[2] && x <= eps->exp[3] &&
		y >= eps->exp[4] && y <= eps->exp[5] &&
		z >= eps->exp[6] && z <= eps->exp[7])
		eps->logicResult = 1;
	else
		eps->logicResult = 0;
	return 8;
}

/* 0x09 */
int EchSetDir(EventProgramState* eps, SpriteInstance* si)
{
	si->dir = eps->exp[1] & 0x1f;
	return 2;
}

/* 0x0a */
int EchReverseDir(EventProgramState* eps, SpriteInstance* si)
{
	Uint8 dir = si->dir + 0x10;//add 16
	si->dir &= 0x1f;
	return 1;
}

/* 0x0d */
int EchShowDialog(EventProgramState* eps, SpriteInstance* si)
{
	int sindex = eps->exp[1] & 0x7f;
	int dflags = eps->exp[2];
	DBStringTable* st = &dbmap->stringtable;
	Uint8* cstr = &st->stringdata[st->stringoffsets[sindex]];
	debug_print(2, 22, cstr);
	return 3;
}


int playerhascontrol = 1;
/* 0x10 */
int EchLoseControl(EventProgramState* eps, SpriteInstance* si)
{
	playerhascontrol = 0;
	return 1;
}

/* 0x11 */
int EchGainControl(EventProgramState* eps, SpriteInstance* si)
{
	playerhascontrol = 1;
	return 1;
}

/* 0x16 */
int EchEnabledGravity(EventProgramState* eps, SpriteInstance* si)
{
	si->gravity = 1;
	return 1;
}

/* 0x17 */
int EchDisableGravity(EventProgramState* eps, SpriteInstance* si)
{
	si->gravity = 0;
	return 1;
}

/* 0x19 */
int EchDeactivate(EventProgramState* eps, SpriteInstance* si)
{
	si->enabled &= ~0x80;
	return 1;
}

/* 0x1a */
int EchSetAnim(EventProgramState* eps, SpriteInstance* si)
{
	set_sprite_anim(si, eps->exp[1], si->lastTick);
	return 2;
}





int(*EventCommandHandlers[])(EventProgramState* eps, SpriteInstance* si) = {
	/*0x00*/ EchNull,
	/*0x01*/ EchNull,
	/*0x02*/ EchGoto,
	/*0x03*/ EchIfFalse,
	/*0x04*/ EchWhileFalse,
	/*0x05*/ EchFlagOn,
	/*0x06*/ EchFlagOff,
	/*0x07*/ EchCheckEntityInArea,
	/*0x08*/ EchNull,
	/*0x09*/ EchSetDir,
	/*0x0a*/ EchReverseDir,
	/*0x0b*/ EchNull,
	/*0x0c*/ EchNull,
	/*0x0d*/ EchShowDialog,
	/*0x0e*/ EchNull,
	/*0x0f*/ EchNull,
	/*0x10*/ EchLoseControl,
	/*0x11*/ EchGainControl,
	/*0x12*/ EchNull,
	/*0x13*/ EchNull,
	/*0x14*/ EchNull,
	/*0x15*/ EchNull,
	/*0x16*/ EchEnabledGravity,
	/*0x17*/ EchDisableGravity,
	/*0x18*/ EchNull,
	/*0x19*/ EchDeactivate,
	/*0x1a*/ EchSetAnim,
	/*0x1b*/ EchNull,
	/*0x1c*/ EchNull,
	/*0x1d*/ EchNull,
	/*0x1e*/ EchNull,
	/*0x1f*/ EchNull,
	/*0x20*/ EchNull,
	/*0x21*/ EchNull,
	/*0x22*/ EchNull,
	/*0x23*/ EchNull,
	/*0x24*/ EchNull,
	/*0x25*/ EchNull,
	/*0x26*/ EchNull,
	/*0x27*/ EchNull,
	/*0x28*/ EchNull,
	/*0x29*/ EchNull,
	/*0x2a*/ EchNull,
	/*0x2b*/ EchNull,
	/*0x2c*/ EchNull,
	/*0x2d*/ EchNull,
	/*0x2e*/ EchNull,
	/*0x2f*/ EchNull,
	/*0x30*/ EchNull,
	/*0x31*/ EchNull,
	/*0x32*/ EchNull,
	/*0x33*/ EchNull,
	/*0x34*/ EchNull,
	/*0x35*/ EchNull,
	/*0x36*/ EchNull,
	/*0x37*/ EchNull,
	/*0x38*/ EchNull,
	/*0x39*/ EchNull,
	/*0x3a*/ EchNull,
	/*0x3b*/ EchNull,
	/*0x3c*/ EchNull,
	/*0x3d*/ EchNull,
	/*0x3e*/ EchNull,
	/*0x3f*/ EchNull,
	/*0x40*/ EchNull,
	/*0x41*/ EchNull,
	/*0x42*/ EchNull,
	/*0x43*/ EchNull,
	/*0x44*/ EchNull,
	/*0x45*/ EchNull,
	/*0x46*/ EchNull,
	/*0x47*/ EchNull,
	/*0x48*/ EchNull,
	/*0x49*/ EchNull,
	/*0x4a*/ EchNull,
	/*0x4b*/ EchNull,
	/*0x4c*/ EchNull,
	/*0x4d*/ EchNull,
	/*0x4e*/ EchNull,
	/*0x4f*/ EchNull,
	/*0x50*/ EchNull,
	/*0x51*/ EchNull,
	/*0x52*/ EchNull,
	/*0x53*/ EchNull,
	/*0x54*/ EchNull,
	/*0x55*/ EchNull,
	/*0x56*/ EchNull,
	/*0x57*/ EchNull,
	/*0x58*/ EchNull,
	/*0x59*/ EchNull,
	/*0x5a*/ EchNull,
	/*0x5b*/ EchNull,
	/*0x5c*/ EchNull,
	/*0x5d*/ EchNull,
	/*0x5e*/ EchNull,
	/*0x5f*/ EchNull,
	/*0x60*/ EchNull,
	/*0x61*/ EchNull,
	/*0x62*/ EchNull,
	/*0x63*/ EchNull,
	/*0x64*/ EchNull,
	/*0x65*/ EchNull,
	/*0x66*/ EchNull,
	/*0x67*/ EchNull,
	/*0x68*/ EchNull,
	/*0x69*/ EchNull,
	/*0x6a*/ EchNull,
	/*0x6b*/ EchNull,
	/*0x6c*/ EchNull,
	/*0x6d*/ EchNull,
	/*0x6e*/ EchNull,
	/*0x6f*/ EchNull,
	/*0x70*/ EchNull,
	/*0x71*/ EchNull,
	/*0x72*/ EchNull,
	/*0x73*/ EchNull,
	/*0x74*/ EchNull,
	/*0x75*/ EchNull,
	/*0x76*/ EchNull,
	/*0x77*/ EchNull,
	/*0x78*/ EchNull,
	/*0x79*/ EchNull,
	/*0x7a*/ EchNull,
	/*0x7b*/ EchNull,
	/*0x7c*/ EchNull,
	/*0x7d*/ EchNull,
	/*0x7e*/ EchNull,
	/*0x7f*/ EchNull,
	/*0x80*/ EchNull,
	/*0x81*/ EchNull,
	/*0x82*/ EchNull,
	/*0x83*/ EchNull,
	/*0x84*/ EchNull,
	/*0x85*/ EchNull,
	/*0x86*/ EchNull,
	/*0x87*/ EchNull,
	/*0x88*/ EchNull,
	/*0x89*/ EchNull,
	/*0x8a*/ EchNull,
	/*0x8b*/ EchNull,
	/*0x8c*/ EchNull,
	/*0x8d*/ EchNull,
	/*0x8e*/ EchNull,
	/*0x8f*/ EchNull,
	/*0x90*/ EchNull,
	/*0x91*/ EchNull,
	/*0x92*/ EchNull,
	/*0x93*/ EchNull,
	/*0x94*/ EchNull,
	/*0x95*/ EchNull,
	/*0x96*/ EchNull,
	/*0x97*/ EchNull,
	/*0x98*/ EchNull,
	/*0x99*/ EchNull,
	/*0x9a*/ EchNull,
	/*0x9b*/ EchNull,
	/*0x9c*/ EchNull,
	/*0x9d*/ EchNull,
	/*0x9e*/ EchNull,
	/*0x9f*/ EchNull,
	/*0xa0*/ EchNull,
	/*0xa1*/ EchNull,
	/*0xa2*/ EchNull,
	/*0xa3*/ EchNull,
	/*0xa4*/ EchNull,
	/*0xa5*/ EchNull,
	/*0xa6*/ EchNull,
	/*0xa7*/ EchNull,
	/*0xa8*/ EchNull,
	/*0xa9*/ EchNull,
	/*0xaa*/ EchNull,
	/*0xab*/ EchNull,
	/*0xac*/ EchNull,
	/*0xad*/ EchNull,
	/*0xae*/ EchNull,
	/*0xaf*/ EchNull,
	/*0xb0*/ EchNull,
	/*0xb1*/ EchNull,
	/*0xb2*/ EchNull,
	/*0xb3*/ EchNull,
	/*0xb4*/ EchNull,
	/*0xb5*/ EchNull,
	/*0xb6*/ EchNull,
	/*0xb7*/ EchNull,
	/*0xb8*/ EchNull,
	/*0xb9*/ EchNull,
	/*0xba*/ EchNull,
	/*0xbb*/ EchNull,
	/*0xbc*/ EchNull,
	/*0xbd*/ EchNull,
	/*0xbe*/ EchNull,
	/*0xbf*/ EchNull,
	/*0xc0*/ EchNull,
	/*0xc1*/ EchNull,
	/*0xc2*/ EchNull,
	/*0xc3*/ EchNull,
	/*0xc4*/ EchNull,
	/*0xc5*/ EchNull,
	/*0xc6*/ EchNull,
	/*0xc7*/ EchNull,
	/*0xc8*/ EchNull,
	/*0xc9*/ EchNull,
	/*0xca*/ EchNull,
	/*0xcb*/ EchNull,
	/*0xcc*/ EchNull,
	/*0xcd*/ EchNull,
	/*0xce*/ EchNull,
	/*0xcf*/ EchNull,
	/*0xd0*/ EchNull,
	/*0xd1*/ EchNull,
	/*0xd2*/ EchNull,
	/*0xd3*/ EchNull,
	/*0xd4*/ EchNull,
	/*0xd5*/ EchNull,
	/*0xd6*/ EchNull,
	/*0xd7*/ EchNull,
	/*0xd8*/ EchNull,
	/*0xd9*/ EchNull,
	/*0xda*/ EchNull,
	/*0xdb*/ EchNull,
	/*0xdc*/ EchNull,
	/*0xdd*/ EchNull,
	/*0xde*/ EchNull,
	/*0xdf*/ EchNull,
	/*0xe0*/ EchNull,
	/*0xe1*/ EchNull,
	/*0xe2*/ EchNull,
	/*0xe3*/ EchNull,
	/*0xe4*/ EchNull,
	/*0xe5*/ EchNull,
	/*0xe6*/ EchNull,
	/*0xe7*/ EchNull,
	/*0xe8*/ EchNull,
	/*0xe9*/ EchNull,
	/*0xea*/ EchNull,
	/*0xeb*/ EchNull,
	/*0xec*/ EchNull,
	/*0xed*/ EchNull,
	/*0xee*/ EchNull,
	/*0xef*/ EchNull,
	/*0xf0*/ EchNull,
	/*0xf1*/ EchNull,
	/*0xf2*/ EchNull,
	/*0xf3*/ EchNull,
	/*0xf4*/ EchNull,
	/*0xf5*/ EchNull,
	/*0xf6*/ EchNull,
	/*0xf7*/ EchNull,
	/*0xf8*/ EchNull,
	/*0xf9*/ EchNull,
	/*0xfa*/ EchNull,
	/*0xfb*/ EchNull,
	/*0xfc*/ EchNull,
	/*0xfd*/ EchNull,
	/*0xfe*/ EchNull,
	/*0xff*/ EchNull

};


/* processes event commands until 0x00 or 0xff is reached */
void ProcessEvent(EventProgramState* eps, SpriteInstance* si)
{
	while (1)
	{
		int cmd = eps->exp[0];
		if (cmd == 0x00)
		{
			eps->exp += 1;
			break;
		}
		if (cmd == 0xff)
			break;

		int seek = EventCommandHandlers[cmd](eps, si);
		eps->exp += seek;
	}
}

void ProcessOneTimeEvent(Uint8* evt, SpriteInstance* si)
{
	EventProgramState eps;
	eps.elapsedMs = 0;
	eps.isWaiting = 0;
	eps.logicResult = 0;
	eps.sp = evt;
	eps.exp = evt;

	ProcessEvent(&eps, si);
}