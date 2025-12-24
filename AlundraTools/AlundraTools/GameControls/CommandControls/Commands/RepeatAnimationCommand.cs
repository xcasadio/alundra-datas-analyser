namespace AlundraTools.GameControls.CommandControls.Commands;

public class RepeatAnimationCommand : CommandBase
{
    public RepeatAnimationCommand(byte code, byte[] parameters, string name, int offset) :
        base(code, parameters, name, offset)
    {
        Name += $" for {(parameters[0])} times";
    }
}