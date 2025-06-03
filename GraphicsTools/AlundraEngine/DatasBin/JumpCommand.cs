namespace AlundraEngine.DatasBin;

public class JumpCommand : SiCommand
{
    public JumpCommand(byte command, byte[] parameters, string name, int memoryAddress)
        : base(command, 3, parameters, name, memoryAddress)
    {
        RefOffset = (short)(parameters[0] | (parameters[1] << 8));
    }

    public override string PrintParameters(List<SiCommand> commands)
    {
        var jumpamount = (short)RefOffset;// (Int16)(parameters[0] | parameters[1] << 8);
        var jumpaddr = MemoryAddress + jumpamount;
        int dex;
        for (dex = 0; dex < commands.Count; dex++)
        {
            if (commands[dex].MemoryAddress == jumpaddr)
            {
                break;
            }
        }
        return dex < commands.Count ? dex.ToString() : "?";
    }

}