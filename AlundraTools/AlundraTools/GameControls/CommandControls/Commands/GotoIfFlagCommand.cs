using System.Reflection;
using AlundraEngine;
using AlundraEngine.DatasBin;

namespace AlundraTools.GameControls.CommandControls.Commands;

public class GotoIfFlagCommand : ContainerCommand
{
    public string CustomName { get; private set; }
    public override int LastCommandMemoryAddress => MemoryAddress + (((Parameters[2] + Parameters[3] * 0x100) * 0x10000) >> 0x10);

    public GotoIfFlagCommand(byte command, byte[] parameters, string name, int memoryAddress)
        : base(command, parameters, name, memoryAddress)
    {
        RefOffset = (short)(parameters[0] | (parameters[1] << 8));
    }

    public override string PrintName()
    {
        return CustomName;
    }

    public override string PrintParameters()
    {
        return string.Join(", ", Parameters.Select(x => x.ToString("x2")));
    }

    public override int Build(int i, List<SiCommand> commands)
    {
        var name = "If ";
        var flag = (uint)(Parameters[0] + Parameters[1] * 0x100);
        name += (flag & 0x8000) == 0 ? "MapFlags" : "GlobalFlags";
        name += $"[{(flag >> 3) & 0xffc}]";
        name += $" & {1 << (Parameters[0] & 0x1f)} is ";
        name += $"{(Command == 0x30 ? "off" : "on")}"; 
        //name += " goto ";
        //var offset = ((Parameters[2] + Parameters[3] * 0x100) * 0x10000) >> 0x10;
        //var index = GetCommandNameByOffset(offset, commands);
        //name += $"{(index == -1 ? "?" : $"{index}")}";
        ////name += $" ({commands[index].Name})";
        //name += " else goto ";
        //index = GetCommandNameByOffset(Size, commands);
        //name += $"{(index == -1 ? "?" : $"{index}")}";
        ////name += $" ({commands[index].Name})";
        CustomName = name;

        return base.Build(i, commands);
    }
}