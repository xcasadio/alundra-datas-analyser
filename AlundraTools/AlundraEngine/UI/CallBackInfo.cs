namespace AlundraEngine.UI;

public class CallBackInfo //UiRecord
{
    //debug purpose
    public int Id;

    public int Flags;
    public UIBoxConfiguration? Data;
    public short X;	
    public short Y;	
    public short Width;	
    public short Height;
    public Action<CallBackInfo>? InitializeFunc;
    public Action<CallBackInfo>? RenderFunc;
    public uint Arg;
}