using AlundraEngine.DatasBin;

namespace AlundraTools.GameControls.CommandControls.Commands;

public class DirectionBranchCommand : CommandBase
{
    public DirectionBranchCommand(byte command, byte[] parameters, string name, int memoryAddress)
        : base(command, parameters, name, memoryAddress)
    {
        _offsets[0] = (short)(parameters[Size - 9] | (parameters[Size - 8] << 8));
        _offsets[1] = (short)(parameters[Size - 7] | (parameters[Size - 6] << 8));
        _offsets[2] = (short)(parameters[Size - 5] | (parameters[Size - 4] << 8));
        _offsets[3] = (short)(parameters[Size - 3] | (parameters[Size - 2] << 8));
    }

    private readonly int[] _offsets = new int[4];

    public override int Build(int i, List<SiCommand> commands)
    {
        System.Diagnostics.Debugger.Break();

        var parms = new List<string>();
        foreach (var offset in _offsets)
        {
            var jumpaddr = MemoryAddress + offset;
            int j;
            for (j = 0; i < commands.Count; j++)
            {
                if (commands[j].MemoryAddress == jumpaddr)
                {
                    break;
                }
            }
            parms.Add(j < commands.Count ? j.ToString() : "?");
        }

        //return string.Join(", ", parms);

        return base.Build(i, commands);
    }
}