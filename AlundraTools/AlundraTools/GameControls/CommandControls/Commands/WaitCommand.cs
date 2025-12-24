namespace AlundraTools.GameControls.CommandControls.Commands;

public class WaitCommand : CommandBase
{
    public WaitCommand(byte code, byte[] parameters, string name, int offset):
        base(code, parameters, name, offset)
    {
        Name += $" {parameters[0]:D2} frames";
    }
}