namespace AlundraTools.GameControls.CommandControls.Commands;

public class CommandBaseDecimalParameters : CommandBase
{
    public CommandBaseDecimalParameters(byte code, byte[] parameters, string name, int memoryAddress) : 
        base(code, parameters, name, memoryAddress)
    {
    }

    protected override string PrintParameters()
    {
        return string.Join(", ", Parameters.Select(x => x.ToString("D3")));
    }
}