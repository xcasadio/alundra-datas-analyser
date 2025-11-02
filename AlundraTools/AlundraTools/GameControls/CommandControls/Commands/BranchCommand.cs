using AlundraTools.GameControls.CommandControls.Commands;

namespace AlundraEngine.DatasBin.Commands;

public class BranchCommand : CommandBase
{
    public BranchCommand(byte command, byte[] parameters, string name, int memoryAddress)
        : base(command, parameters, name, memoryAddress)
    {
        RefOffset = (short)(parameters[Size - 3] | (parameters[Size - 2] << 8));
    }

    public override string PrintParameters()
    {
        var parms = new List<string>();
        if (Size == 5)
        {
            parms.Add((Parameters[Size - 5] | (Parameters[Size - 4] << 8)).ToString("x4").TrimStart('0'));
        }
        else
        {

            for (var i = 0; i < Size - 3; i++)
            {
                parms.Add(Parameters[i].ToString("x2"));
            }
        }
        parms.Add(RefOffset.ToString());

        return string.Join(", ", parms);
    }
}