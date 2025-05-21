using Alundra.DatasBin;

namespace Alundra.Gameplay.Scripts;

public class EventProgramState
{
    public int Sp; // pointer on code
    public readonly int[] Exp = new int[10];
    public int Result;
    public int _30;
    
    public byte[] Codes;
    public int CodeIndex = 0;

    public void CopyFrom(EventProgramState other)
    {
        Sp = other.Sp;

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
    }
}