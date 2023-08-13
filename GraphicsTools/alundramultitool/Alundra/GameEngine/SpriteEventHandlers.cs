using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicsTools.Alundra
{
    public class SpriteEventHandlers
    {
        GameState gameState;
        Dictionary<int, SpriteEventHandler>[] TypeHandlers = new Dictionary<int, SpriteEventHandler>[6];
        public SpriteEventHandlers(GameState gameState)
        {
            this.gameState = gameState;

            TypeHandlers[Helper.PROGRAM_A_LOAD] = new Dictionary<int, SpriteEventHandler>();
            //there are no spriteevent handlers for map
            TypeHandlers[Helper.PROGRAM_C_TICK] = new Dictionary<int, SpriteEventHandler>();
            TypeHandlers[Helper.PROGRAM_D_TOUCH] = new Dictionary<int, SpriteEventHandler>();
            TypeHandlers[Helper.PROGRAM_E_DEACTIVATE] = new Dictionary<int, SpriteEventHandler>();
            TypeHandlers[Helper.PROGRAM_F_INTERACT] = new Dictionary<int, SpriteEventHandler>();

            //register the ones that have been implimented here
            Register(Helper.PROGRAM_C_TICK, 0x17, etick_17_jarsandboxes_Handler);
        }

        void Register(int type, byte code, SpriteEventHandler handler)
        {
            TypeHandlers[type].Add(code, handler);
        }

        public void RunSpriteHandler(int eventtype, int eventid, SpriteInstance entity)
        {
            var handlers = TypeHandlers[eventtype];
            if (handlers.ContainsKey(eventid))
            {
                handlers[eventid](entity);
                return;
            }
        }

        public void etick_17_jarsandboxes_Handler(SpriteInstance entity)
        {
            if (entity.PlatformEntity == null)
                return;
            if (entity._24 == 0)
            {
                entity.TargetAnim = 0;
                return;
            }


            if(entity._24 == -1)
            {
                entity.TargetAnim = 3;
            }
            else
            {
                entity.TargetAnim = Helper.Anim_24_Table[entity._24];
            }

            if (entity.PlatformEntity != null)//redudant check
                entity._2c = 0;

            entity.PlatformEntity = null;
            entity.Flags = (entity.Flags | 0x30) & 0xff7f;//turn off bit 8, turn on bits 5 and 6

        }
    }

    public delegate void SpriteEventHandler(SpriteInstance entity);

}
