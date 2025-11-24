namespace AlundraTools.GameControls.CommandControls.Commands;

public class DialogCommand : CommandBaseDecimalParameters
{
    public int TextId;
    public int PlayerControlFlag;

    public DialogCommand(byte command, byte[] parameters, string name, int memoryAddress)
        : base(command, parameters, name, memoryAddress)
    {
        TextId = parameters[0];
        PlayerControlFlag = parameters[1];
    }
}