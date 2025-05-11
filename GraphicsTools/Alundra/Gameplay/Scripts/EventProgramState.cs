using Alundra.DatasBin;

namespace Alundra.Gameplay.Scripts;

public class EventProgramState
{
    public int Sp; // pointer on code
    public int[] Exp = new int[10];
    public int Result;
    public int _30;
    //public int Tick;
    //public readonly int[] Variables = new int[8];
    //public int LogicResult;
    //
    //public ushort ElapsedMs;
    //public byte IsWaiting;
    //public byte[] Codes = new byte[9];
    
    public int CommandIndex;
    public List<SiCommand> Commands;

    public void CopyFrom(EventProgramState other)
    {
        Sp = other.Sp; //.Clear();
        //Sp.AddRange(other.Sp); 
        CommandIndex = other.CommandIndex; 

        for (int i = 0; i < Exp.Length; i++)
        {
            Exp[i] = other.Exp[i];
        }

        Result = other.Result;
        _30 = other._30;
    }

    public override string ToString()
    {
        return $"Sp:{string.Join(',', Sp)} Exp:{string.Join(',', Exp)}";
        //return $"Sp:{Sp} Exp:{Exp} Tick:{Tick} Vars:{string.Join(',', Variables)} LR:{LogicResult} EM:{ElapsedMs} IW:{IsWaiting} Codes:{string.Join(',', Codes)}";
    }
}