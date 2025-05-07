namespace Alundra.DatasBin;

public class DirectionBranchCommand : SiCommand
{
    public DirectionBranchCommand(byte command, byte[] parameters, string name, int memoryAddress)
        : base(command, 9, parameters, name, memoryAddress)
    {

        _offsets[0] = (short)(parameters[Size - 9] | (parameters[Size - 8] << 8));
        _offsets[1] = (short)(parameters[Size - 7] | (parameters[Size - 6] << 8));
        _offsets[2] = (short)(parameters[Size - 5] | (parameters[Size - 4] << 8));
        _offsets[3] = (short)(parameters[Size - 3] | (parameters[Size - 2] << 8));
    }

    private readonly int[] _offsets = new int[4];

    public override string PrintParameters(List<SiCommand> commands)
    {
        var parms = new List<string>();
        foreach (var offset in _offsets)
        {
            var jumpaddr = MemoryAddress + offset;
            int dex;
            for (dex = 0; dex < commands.Count; dex++)
            {
                if (commands[dex].MemoryAddress == jumpaddr)
                {
                    break;
                }
            }
            parms.Add(dex < commands.Count ? dex.ToString() : "?");
        }

        return string.Join(", ", parms);
    }
}