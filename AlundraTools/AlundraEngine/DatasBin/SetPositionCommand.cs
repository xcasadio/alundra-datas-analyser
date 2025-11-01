namespace AlundraEngine.DatasBin;

public class SetPositionCommand : SiCommand
{
    public SetPositionCommand(byte command, byte[] parameters, string name, int memoryAddress)
        : base(command, 8, parameters, name, memoryAddress)
    {
    }

    public override string PrintParameters(List<SiCommand> commands)
    {
        var parms = new List<string>();

        parms.Add(Parameters[0].ToString("x2"));
        parms.Add((Parameters[1] | (Parameters[2] << 8)).ToString("x4").TrimStart('0'));
        parms.Add((Parameters[3] | (Parameters[4] << 8)).ToString("x4").TrimStart('0'));
        parms.Add((Parameters[5] | (Parameters[6] << 8)).ToString("x4").TrimStart('0'));

        return string.Join(", ", parms);
    }
}