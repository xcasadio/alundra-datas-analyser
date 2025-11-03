using AlundraTools.GameControls.CommandControls.Commands;

namespace AlundraEngine.DatasBin.Commands;

public class BranchCommand : ContainerCommand
{
    private int _lastCommandMemoryAddress;
    public override int LastCommandMemoryAddress => _lastCommandMemoryAddress;

    public BranchCommand(byte command, byte[] parameters, string name, int memoryAddress)
        : base(command, parameters, name, memoryAddress)
    {
        RefOffset = (short)(parameters[Size - 3] | (parameters[Size - 2] << 8));
    }

    protected override string PrintParameters()
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

    public override int Build(int i, List<SiCommand> commands)
    {
        _lastCommandMemoryAddress = MemoryAddress + Math.Max(((Parameters[0] + Parameters[1] * 0x100) * 0x10000) >> 0x10, Size);
        return base.Build(i, commands);
    }
}