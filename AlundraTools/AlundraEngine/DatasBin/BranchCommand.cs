namespace AlundraEngine.DatasBin;

public class BranchCommand : SiCommand
{
    public BranchCommand(byte command, int size, byte[] parameters, string name, int memoryAddress)
        : base(command, size, parameters, name, memoryAddress)
    {
        RefOffset = (short)(parameters[size - 3] | (parameters[size - 2] << 8));
    }

    public override string PrintParameters(List<SiCommand> commands)
    {
        var parms = new List<string>();
        if (Size == 5)
        {
            parms.Add((Parameters[Size - 5] | (Parameters[Size - 4] << 8)).ToString("x4"));
        }
        else
        {

            for (var dex = 0; dex < Size - 3; dex++)
            {
                parms.Add(Parameters[dex].ToString("x2"));
            }
        }
        parms.Add(RefOffset.ToString());

        return string.Join(", ", parms);
    }
}