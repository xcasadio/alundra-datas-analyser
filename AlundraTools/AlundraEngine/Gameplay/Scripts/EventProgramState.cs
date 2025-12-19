using AlundraEngine.DatasBin;

namespace AlundraEngine.Gameplay.Scripts;

public class EventProgramState
{
    public int Sp; // pointer on code => use Codes instead
    public readonly int[] Parameters = new int[10];
    public int Result;
    public int _30;
    public int _34;

    public byte[] Codes;
    public int CodeIndex = 0;

    public void CopyFrom(EventProgramState other)
    {
        Sp = other.Sp;
        Codes = other.Codes;
        CodeIndex = other.CodeIndex;
        Array.Copy(other.Parameters, Parameters, Parameters.Length);
        Result = other.Result;
        _30 = other._30;
        _34 = other._34;
    }

    public override string ToString()
    {
        var actionName = SpriteInfoEventCodes.CommandNameByCode.GetValueOrDefault((byte)Sp, "?");
        return $"{nameof(Sp)}:{Sp} {actionName} ci:{CodeIndex} Parameters:{string.Join(',', Parameters)} {nameof(_30)}:{_30} {nameof(_34)}:{_34}";
    }
}