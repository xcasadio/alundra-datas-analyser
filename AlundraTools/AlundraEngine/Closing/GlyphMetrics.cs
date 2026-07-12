namespace AlundraEngine.Closing;

// GHIDRA: GlyphMetrics @ /CLOSING.EXE (structure, size 0x14)
// SOURCE: Ghidra (ReVa get-structure-info), struct already closed in the project.
public class GlyphMetrics
{
    public int Width;
    public int Height;
    public uint SheetX;
    public int SheetRow;
    public int YOffset;

    public GlyphMetrics(BinaryReader br)
    {
        Width = br.ReadInt32();
        Height = br.ReadInt32();
        SheetX = br.ReadUInt32();
        SheetRow = br.ReadInt32();
        YOffset = br.ReadInt32();
    }
}
