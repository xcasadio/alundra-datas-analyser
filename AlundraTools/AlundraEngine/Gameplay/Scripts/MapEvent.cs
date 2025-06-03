using AlundraEngine.DatasBin;

namespace AlundraEngine.Gameplay.Scripts;

//0x48 byte record
public class MapEvent
{
    public int Id;//index of this mapevent
    public SiMapEventRecord MapEventRecord;//4
    public int ProgramBMap;//8
    public Entity Entity;//c
    public EventProgramState EventData = new();//10
}