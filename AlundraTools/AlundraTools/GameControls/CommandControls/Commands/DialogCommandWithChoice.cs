namespace AlundraTools.GameControls.CommandControls.Commands;

public class DialogCommandWithChoice : DialogCommand
{
    public DialogCommandWithChoice(byte command, byte[] parameters, string name, int memoryAddress)
        : base(command, parameters, name, memoryAddress)
    {
        TextId = parameters[1];
        PlayerControlFlag = parameters[2];
    }
}