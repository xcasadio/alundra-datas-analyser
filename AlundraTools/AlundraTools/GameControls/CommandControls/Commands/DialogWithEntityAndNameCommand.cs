namespace AlundraTools.GameControls.CommandControls.Commands;

public class DialogWithEntityAndNameCommand : DialogCommandWithChoice
{
    public DialogWithEntityAndNameCommand(byte command, byte[] parameters, string name, int offset)
        : base(command, parameters, name, offset)
    {
        EntityIndex = parameters[0];
        TextId = parameters[3];
        PlayerControlFlag = parameters[4];
    }
}