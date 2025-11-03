namespace AlundraTools.GameControls.CommandControls.Commands;

public class WaitCommand : CommandBase
{
    public WaitCommand(byte code, byte[] parameters, string name, int memoryAddress):
        base(code, parameters, name, memoryAddress)
    {
        Name += $" {parameters[0]:D2} frames";
    }
}