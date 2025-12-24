namespace AlundraTools.GameControls.CommandControls.Commands;

public class SetPositionCommand : CommandBase
{
    public SetPositionCommand(byte command, byte[] parameters, string name, int offset)
        : base(command, parameters, name, offset)
    {
    }

    protected override string PrintParameters()
    {
        var parms = new List<string>();

        parms.Add(Parameters[0].ToString("x2"));
        parms.Add((Parameters[1] | (Parameters[2] << 8)).ToString("x4").TrimStart('0'));
        parms.Add((Parameters[3] | (Parameters[4] << 8)).ToString("x4").TrimStart('0'));
        parms.Add((Parameters[5] | (Parameters[6] << 8)).ToString("x4").TrimStart('0'));

        return string.Join(", ", parms);
    }
}