namespace Alundra.DatasBin;

public class SiCommand
{
    public SiCommand(byte command, int size, byte[] parameters, string name, int memaddr)
    {
        Memaddr = memaddr;
        Command = command;
        Parameters = parameters;
        Size = size;
        Name = name;
    }
    public readonly int Memaddr;
    public readonly byte Command;
    public readonly byte[] Parameters;
    public readonly int Size;
    public int Refoffset;

    public readonly string Name;

    public string PrintName()
    {
        return !string.IsNullOrEmpty(Name) ? Name : Command.ToString("x2");
    }

    public virtual string PrintParameters(List<SiCommand> commands)
    {
        return string.Join(", ", Parameters.Select(x => x.ToString("x2")));
    }

    public string Print(int depth, List<SiCommand> commands)
    {
        var index = commands.IndexOf(this);
        var output = index.ToString("d3") + " ";
        output += new string(' ', depth * 4);
        output += PrintName();
        if (Command != 0 && Command != 0xff)
        {
            output += "(";
            output += PrintParameters(commands);
            output += ")";
        }
        return output;
    }
}