namespace AlundraTools.GameControls.CommandControls.Commands;

public class DialogCommandWithChoice : DialogCommand
{
    public int EntityIndex;

    public DialogCommandWithChoice(byte command, byte[] parameters, string name, int memoryAddress)
        : base(command, parameters, name, memoryAddress)
    {
        EntityIndex = parameters[0];
        TextId = parameters[1];
        PlayerControlFlag = parameters[2];
    }
}