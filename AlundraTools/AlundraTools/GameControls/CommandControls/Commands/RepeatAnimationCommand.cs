namespace AlundraTools.GameControls.CommandControls.Commands;

public class RepeatAnimationCommand : CommandBase
{
    public RepeatAnimationCommand(byte code, byte[] parameters, string name, int memoryAddress) :
        base(code, parameters, name, memoryAddress)
    {
        Name += $" for {(parameters[0])} times";
    }
}