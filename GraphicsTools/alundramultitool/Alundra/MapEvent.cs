namespace GraphicsTools.Alundra
{
    //0x48 byte record
    public class MapEvent
    {
        public int Id;//index of this mapevent
        public SiMapEventRecord MapEventRecord;//4
        public int ProgramBMap;//8
        public SpriteInstance Entity;//c
        public EventProgramState EventData;//10
    }
}
