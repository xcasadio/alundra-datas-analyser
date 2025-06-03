namespace AlundraEngine.UI;

public class UiDrawCmd
{
    public short X, Y;
    public byte U, V;
    public short Uipaletteindex;//(clut address - 0x7812)/ 64
    public short Spritesheet;
    public short W, H;

    public long Signature { get
        {
            return Spritesheet | Uipaletteindex << 8 | U << 16 | V << 24 | W << 32 | H << 38;
        }
    }
}