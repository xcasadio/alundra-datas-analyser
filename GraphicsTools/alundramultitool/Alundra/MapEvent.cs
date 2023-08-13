using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicsTools.Alundra
{
    //0x48 byte record
    public class MapEvent
    {
        public int id;//index of this mapevent
        public SIMapEventRecord MapEventRecord;//4
        public int ProgramB_Map;//8
        public SpriteInstance Entity;//c
        public EventProgramState EventData;//10
    }
}
