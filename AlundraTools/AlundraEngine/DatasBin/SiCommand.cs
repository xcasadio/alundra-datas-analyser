using AlundraEngine.Gameplay.Scripts;

namespace AlundraEngine.DatasBin;

public class SiCommand
{
    public SiCommand(byte command, byte[] parameters, string name, int offset)
    {
        Offset = offset;
        Command = command;
        Parameters = parameters;
        Size = EventCodeDebugger.CommandPropertiesByCode.GetValueOrDefault(command).Size;
        Name = name;
    }

    public readonly string Name;
    public readonly int Offset;
    public readonly byte Command;
    public readonly byte[] Parameters;
    public readonly int Size;
}