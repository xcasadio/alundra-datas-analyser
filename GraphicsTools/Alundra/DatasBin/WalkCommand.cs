namespace Alundra.DatasBin;

public class WalkCommand : SiCommand
{
    public WalkCommand(byte command, byte[] parameters, string name, int memoryAddress)
        : base(command, 3, parameters, name, memoryAddress)
    {
    }

    public override string PrintParameters(List<SiCommand> commands)
    {
        return (Parameters[0] | (Parameters[1] << 8)).ToString("x4");
    }
}