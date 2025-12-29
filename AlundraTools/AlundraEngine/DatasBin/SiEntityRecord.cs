using System.Diagnostics;

namespace AlundraEngine.DatasBin;

public class SiEntityRecord
{
    public readonly int MemoryAddress;

    public readonly byte XMin;//if character isnt within this bounding box, dont activate the entity
    public readonly byte YMin;
    public readonly byte XMax;//0x2
    public readonly byte YMax;
    public readonly byte IsEnabled;
    public readonly byte SpriteDirection;//0,c0,c1,c2,c3,80
    public readonly byte SpriteTableIndex;
    public readonly byte XPos;//divide by 2
    public readonly byte YPos;//divide by 2
    public readonly byte Height;//divide by 2
    public readonly byte EventCodesA_LoadIndex;
    public readonly byte EventCodesB_MapIndex;
    public readonly byte EventCodesC_TickIndex;
    public readonly byte EventCodesD_TouchIndex;
    public readonly byte EventCodesE_DeactivateIndex;
    public readonly byte EventCodesF_InteractIndex; //0xf
    //public readonly ushort U7;
    public readonly ushort _10;
    public readonly ushort Contents;

    public SiEntityRecord(BinaryReader br, int memoryAddress)
    {
        MemoryAddress = memoryAddress;
        //i used to think these were the last of the previous entry, but its the first of this oneMinx = br.ReadByte();//0
        XMin = br.ReadByte();//0
        YMin = br.ReadByte();//1
        XMax = br.ReadByte();//2
        YMax = br.ReadByte();//3
        IsEnabled = br.ReadByte();//4
        SpriteDirection = br.ReadByte();//5
        SpriteTableIndex = br.ReadByte();//6
        XPos = br.ReadByte();//7
        YPos = br.ReadByte();//8
        Height = br.ReadByte();//9
        EventCodesA_LoadIndex = br.ReadByte();
        EventCodesB_MapIndex = br.ReadByte();
        EventCodesC_TickIndex = br.ReadByte();
        EventCodesD_TouchIndex = br.ReadByte();
        EventCodesE_DeactivateIndex = br.ReadByte();
        EventCodesF_InteractIndex = br.ReadByte();
        _10 = br.ReadUInt16();
        Contents = br.ReadUInt16();
    }

    public SiAnimation GetSprite(BinaryReader br, SpriteInfo si)
    {
        var sector5 = si.SpriteRecords[SpriteTableIndex];

        if (sector5 != null && SpriteDirection >> 4 != 0x4 && SpriteDirection >> 4 != 0x0)
        {
            var commands = new List<SiCommand>();
            if (EventCodesA_LoadIndex != 0xff && EventCodesA_LoadIndex != 0)
            {
                commands.AddRange(si.EventCodes.GetCommandsOnlyAtOffset(si.EventCodes.EventCodesATable[EventCodesA_LoadIndex & 0x7f]));
            }

            if (commands.Count == 0 && EventCodesC_TickIndex != 0xff && EventCodesC_TickIndex != 0)
            {
                commands.AddRange(si.EventCodes.GetCommandsOnlyAtOffset(si.EventCodes.EventCodesCTable[EventCodesC_TickIndex & 0x7f]));
            }

            foreach (var cmd in commands)
            {
                if (cmd.Command == 0x1a)//set animation
                {
                    if (cmd.Parameters[0] >= (sector5.AnimSets?.Length ?? -1))
                    {
                        Debugger.Break();
                        cmd.Parameters[0] = (byte)((sector5.AnimSets?.Length ?? 1) - 1);
                    }

                    var animSet = sector5.AnimSets[cmd.Parameters[0]];
                    return sector5.GetAnimation(br, animSet.AnimationOffsets[SpriteDirection & 0x3]);
                }
            }

            return sector5.GetAnimation(br, sector5.AnimSets[0].AnimationOffsets[SpriteDirection & 0x3]);//default anim
        }

        return null;
    }
}