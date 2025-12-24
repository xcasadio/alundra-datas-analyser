namespace AlundraTools.GameControls.CommandControls.Commands;

public class CommandBaseDecimalParameters : CommandBase
{
    public CommandBaseDecimalParameters(byte code, byte[] parameters, string name, int offset) : 
        base(code, parameters, name, offset)
    {
    }

    protected override string PrintParameters()
    {
        return string.Join(", ", Parameters.Select(x => x.ToString("D3")));
    }
}