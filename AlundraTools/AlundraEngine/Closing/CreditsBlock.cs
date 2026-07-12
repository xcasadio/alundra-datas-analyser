namespace AlundraEngine.Closing;

// GHIDRA: CreditsBlock @ /CLOSING.EXE (structure, size 0x58, packed)
// SOURCE: Ghidra (ReVa get-structure-info), struct already closed in the project.
// blockType == 0 is the end-of-table sentinel (per UpdateCreditsTextSequencer).
// unknown_0x04: PROBABLE per raw-table cross-check (2026-07-12) - g_creditsBlockExtraHoldFlag is
// copied from exactly this field (`*(short*)(&DAT_80039898 + iVar1*0x58)`, and DAT_80039898 ==
// &g_creditsBlockTable + 4 == this field's address), so its OBSERVED role is confirmed, though the
// game-meaning of "extra hold" itself is still not elucidated. unknown_0x06 still fully unknown.
public class CreditsBlock
{
    public int BlockType;
    public short Unknown0x04;
    public short Unknown0x06;
    public CreditsBlockEntry[] Entries;

    public CreditsBlock(BinaryReader br, uint fileToRamOffset)
    {
        BlockType = br.ReadInt32();
        Unknown0x04 = br.ReadInt16();
        Unknown0x06 = br.ReadInt16();
        Entries = new CreditsBlockEntry[10];
        for (var i = 0; i < Entries.Length; i++)
        {
            Entries[i] = new CreditsBlockEntry(br, fileToRamOffset);
        }
    }
}
