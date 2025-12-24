namespace AlundraTools.GameControls.CommandControls.Commands;

public class DialogCommand : CommandBaseDecimalParameters
{
    public int TextId;
    public int PlayerControlFlag;

    public DialogCommand(byte command, byte[] parameters, string name, int offset)
        : base(command, parameters, name, offset)
    {
        TextId = parameters[0];
        PlayerControlFlag = parameters[1];
    }
}