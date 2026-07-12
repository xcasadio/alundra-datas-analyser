namespace AlundraEngine.Closing;

// GHIDRA: CreditsPictureEntry @ /CLOSING.EXE (structure, size 0xC, packed)
// SOURCE: Ghidra (ReVa get-structure-info), struct already closed in the project.
// unknown_0x00 is read into DAT_801b6b54 by LoadNextCreditsPicture but that global's use is not
// analyzed in this pass; kept raw.
public class CreditsPictureEntry
{
    public uint Unknown0x00;
    // GHIDRA: timData is a RAM pointer (u_long*) in the original; ported as the CLOSING.EXE file
    // offset of the pointed-to TIM resource ("File Offset = RAM Address - 0x8001F800", per the
    // comment on ClosingEngine's constructor) since raw pointers aren't used in this port (no unsafe).
    public int TimDataFileOffset;
    public ushort LayoutIndex;
    public ushort Unknown0x0A;

    // JUSTIFICATION: C# language bridge only
    // RELATION: resolved once by ClosingEngine.ReadCreditsPictureTable to the Inspector image index
    // whose file offset matches TimDataFileOffset (the 20-entry table only ever references the 10
    // pictures Inspector already catalogues), so LoadNextCreditsPicture can reuse Inspector's
    // existing per-index Bitmap cache instead of re-decoding the same TIM from scratch. -1 if no
    // match was found.
    public int InspectorImageIndex = -1;

    public CreditsPictureEntry(BinaryReader br, uint fileToRamOffset)
    {
        Unknown0x00 = br.ReadUInt32();
        var timDataRamAddress = br.ReadUInt32();
        TimDataFileOffset = (int)(timDataRamAddress - fileToRamOffset);
        LayoutIndex = br.ReadUInt16();
        Unknown0x0A = br.ReadUInt16();
    }
}
