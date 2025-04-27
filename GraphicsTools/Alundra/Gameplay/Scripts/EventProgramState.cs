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
    public byte[] Code;
}