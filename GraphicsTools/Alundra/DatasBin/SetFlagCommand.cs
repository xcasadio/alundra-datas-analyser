namespace Alundra.DatasBin;

public class SetFlagCommand : SiCommand
{
    public SetFlagCommand(byte command, byte[] parameters, string name, int memaddr)
        : base(command, 3, parameters, name, memaddr)
    {
    }

    public override string PrintParameters(List<SiCommand> commands)
    {
        return (Parameters[0] | (Parameters[1] << 8)).ToString("x4");
    }
}