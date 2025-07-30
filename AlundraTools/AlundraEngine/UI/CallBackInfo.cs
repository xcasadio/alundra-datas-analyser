namespace AlundraEngine.UI;

public class CallBackInfo //UiRecord
{
    public int Flags;
    public TextTilesConfiguration? Data;
    public short X;	
    public short Y;	
    public short Width;	
    public short Height;
    public Action<CallBackInfo>? InitializeFunc;
    public Action<CallBackInfo>? RenderFunc;
    public uint Arg;
}