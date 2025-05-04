namespace Alundra.Gameplay.Scripts;

public class EventProgramState
{
    public int Sp;
    public int Exp;
    public int Tick;
    public readonly int[] Variables = new int[8];
    public int LogicResult;

    public ushort ElapsedMs;
    public byte IsWaiting;
    public byte[] Codes = new byte[9];

    public override string ToString()
    {
        return $"Sp:{Sp} Exp:{Exp} Tick:{Tick} Vars:{string.Join(',', Variables)} LR:{LogicResult} EM:{ElapsedMs} IW:{IsWaiting} Codes:{string.Join(',', Codes)}";
    }
}