namespace AlundraEngine.Closing;

// GHIDRA: CreditsBlockEntry @ /CLOSING.EXE (structure, size 0x8, packed)
// SOURCE: Ghidra (ReVa get-structure-info), struct already closed in the project.
// nameType: CORRECTION per DrawCreditsTextLine's proven signature - this is a start-X indentation
// selector (0=0px, 1=0x20px, else 8px), not a font style as first assumed.
// unknown_0x02: not analyzed in this pass; kept raw.
public class CreditsBlockEntry
{
    public short NameType;
    public short Unknown0x02;
    // GHIDRA: namePtr is a RAM pointer (void*) in the original; ported as a CLOSING.EXE file offset
    // (see CreditsPictureEntry.TimDataFileOffset) since raw pointers aren't used in this port.
    public int NamePtrFileOffset;
    // JUSTIFICATION: C# language bridge only
    // RELATION: original tests `entries[0].namePtr == (void*)0x0`; recorded directly instead of
    // re-deriving it from NamePtrFileOffset.
    public bool IsEmpty;

    public CreditsBlockEntry(BinaryReader br, uint fileToRamOffset)
    {
        NameType = br.ReadInt16();
        Unknown0x02 = br.ReadInt16();
        var namePtrRamAddress = br.ReadUInt32();
        IsEmpty = namePtrRamAddress == 0;
        NamePtrFileOffset = IsEmpty ? 0 : (int)(namePtrRamAddress - fileToRamOffset);
    }
}
