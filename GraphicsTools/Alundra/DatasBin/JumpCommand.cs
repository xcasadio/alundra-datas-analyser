namespace Alundra.DatasBin;

public class JumpCommand : SiCommand
{
    public JumpCommand(byte command, byte[] parameters, string name, int memaddr)
        : base(command, 3, parameters, name, memaddr)
    {
        Refoffset = (short)(parameters[0] | (parameters[1] << 8));
    }

    public override string PrintParameters(List<SiCommand> commands)
    {
        var jumpamount = (short)Refoffset;// (Int16)(parameters[0] | parameters[1] << 8);
        var jumpaddr = Memaddr + jumpamount;
        int dex;
        for (dex = 0; dex < commands.Count; dex++)
        {
            if (commands[dex].Memaddr == jumpaddr)
            {
                break;
            }
        }
        return dex < commands.Count ? dex.ToString() : "?";
    }

}