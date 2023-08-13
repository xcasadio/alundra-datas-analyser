using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GraphicsTools.Alundra
{
    public static class EntityVars
    {

        public static string[] VarOffsets = new string[GameMap.eventobject_size / 4];

        public static void Init()
        {
            //VarOffsets[0x0] = "entityid";

            VarOffsets[0x28] = "";
            VarOffsets[0x44] = "datasbinrecord";
            VarOffsets[0x64] = "sprite";
            VarOffsets[0x6c] = "";

            VarOffsets[0x8c] = "targetdir";

            VarOffsets[0x94] = "dir";
            VarOffsets[0x98] = "framedex";
            VarOffsets[0x9c] = "animset";
            VarOffsets[0xa0] = "curframe";
            VarOffsets[0xa4] = "nextframe";

            VarOffsets[0xb8] = "zforce";
            VarOffsets[0xbc] = "targetxforce";
            VarOffsets[0xc0] = "targetyforce";
            VarOffsets[0xc4] = "xforce";
            VarOffsets[0xc8] = "yforce";
            VarOffsets[0xcc] = "";
            VarOffsets[0xd0] = "";
            VarOffsets[0xd4] = "xforcestep";
            VarOffsets[0xd8] = "yforcestep";
            VarOffsets[0xdc] = "adjustedxforce";
            VarOffsets[0xe0] = "adjustedyforce";
            VarOffsets[0xe4] = "finalxforce";
            VarOffsets[0xe8] = "finalyforce";
            VarOffsets[0xec] = "finalzforce";

            VarOffsets[0xf0] = "acceleration";
            VarOffsets[0xf4] = "speed";
            VarOffsets[0xf8] = "appliedzforce";
            VarOffsets[0xfc] = "aboveabovexpos";
            VarOffsets[0x100] = "aboveaboveypos";
            VarOffsets[0x108] = "abovexpos";
            VarOffsets[0x10c] = "aboveypos";

            VarOffsets[0x114] = "xpos";
            VarOffsets[0x118] = "ypos";
            VarOffsets[0x11c] = "xpos";
            VarOffsets[0x120] = "xtile";
            VarOffsets[0x124] = "ytile";
            VarOffsets[0x128] = "ztile";
            VarOffsets[0x12c] = "ridingentity";

            VarOffsets[0x134] = "";
            VarOffsets[0x13c] = "forceadjusted";
            VarOffsets[0x140] = "";
            VarOffsets[0x144] = "";
            VarOffsets[0x148] = "maptiletl";
            VarOffsets[0x14c] = "maptiletr";
            VarOffsets[0x150] = "maptilebl";
            VarOffsets[0x154] = "maptilebr";
            VarOffsets[0x158] = "mapheighttl";
            VarOffsets[0x15c] = "mapheighttr";
            VarOffsets[0x160] = "mapheightbl";
            VarOffsets[0x164] = "mapheightbr";
            VarOffsets[0x168] = "donemoving";

            VarOffsets[0x180] = "";
            VarOffsets[0x184] = "";
            VarOffsets[0x188] = "";
            VarOffsets[0x18c] = "";
            VarOffsets[0x190] = "";
            VarOffsets[0x194] = "imageset";
            VarOffsets[0x198] = "xpos2?";
            VarOffsets[0x19c] = "ypos2?";
            VarOffsets[0x1a0] = "zpos2?";
            
            VarOffsets[0x1d4] = "";
            VarOffsets[0x1d8] = "adjustedxpos";
            VarOffsets[0x1dc] = "adjustedypos";
            VarOffsets[0x1e0] = "adjustedzpos";
            VarOffsets[0x1e4] = "xmod";
            VarOffsets[0x1e8] = "ymod";
            VarOffsets[0x1ec] = "zmod";
            VarOffsets[0x1f0] = "width";
            VarOffsets[0x1f4] = "depth";
            VarOffsets[0x1f8] = "height";
            VarOffsets[0x1fc] = "";
            VarOffsets[0x200] = "";
            VarOffsets[0x204] = "";
            VarOffsets[0x208] = "";
            VarOffsets[0x20c] = "";
            VarOffsets[0x210] = "";

            VarOffsets[0x230] = "entity(self)";
            VarOffsets[0x234] = "tickprogsp";
            VarOffsets[0x238] = "tickprogexp";

        }
    }
}
