using AlundraEngine.Gameplay;

namespace AlundraEngine.Editor
{
    public class FrameSnapshot
    {
        private uint[] MapFlags { get; }
        private uint[] GlobalFlags { get; }
        private Entity[] Entities { get; }

        public FrameSnapshot()
        {
            Entities = new Entity[StaticVariables.g_entitySlots.Length];

            for (int i = 0; i < StaticVariables.g_entitySlots.Length; i++)
            {
                Entities[i] = StaticVariables.g_entitySlots[i].ShallowCopy(); // create copy,ShallowCopy() create bugs with linked entity
            }

            MapFlags = new uint[StaticVariables.g_mapFlags.Length];
            Array.Copy(StaticVariables.g_mapFlags, MapFlags, StaticVariables.g_mapFlags.Length);

            GlobalFlags = new uint[StaticVariables.g_globalFlags.Length];
            Array.Copy(StaticVariables.g_globalFlags, GlobalFlags, StaticVariables.g_globalFlags.Length);

            
            //add StaticVariables.g_eventProgramState

            //add effects

            //add map tiles

            //add camera parameters
        }

        public void CopyToMemory()
        {
            for (int i = 0; i < StaticVariables.g_entitySlots.Length; i++)
            {
                StaticVariables.g_entitySlots[i] = Entities[i].ShallowCopy();
            }

            Array.Copy(MapFlags, StaticVariables.g_mapFlags, StaticVariables.g_mapFlags.Length);
            Array.Copy(GlobalFlags, StaticVariables.g_globalFlags, StaticVariables.g_globalFlags.Length);
        }
    }
}
