namespace AlundraEngine.DatasBin;

public class ScrollScreen
{
    public readonly int FactorXNum; // How many pixels to scroll horizontal against camera movement (negative = scroll with camera)
    public readonly int FactorXDenom;
    public readonly int FactorYNum; // How many pixels to scroll vertical against camera movement (negative = scroll with camera)
    public readonly int FactorYDenom;
    public readonly int ScrollXSpeed; // How many pixels to scroll horizontal per tick
    public readonly int ScrollXPeriod; // How many ticks to skip for horizontal scrolling
    public readonly int ScrollYSpeed; // How many pixels to scroll vertical per tick
    public readonly int ScrollYPeriod; // How many ticks to skip for vertical scrolling

    public ScrollScreen(BinaryReader br)
    {
        FactorXNum = br.ReadInt32();
        FactorXDenom = br.ReadInt32();
        FactorYNum = br.ReadInt32();
        FactorYDenom = br.ReadInt32();
        ScrollXSpeed = br.ReadInt32();
        ScrollXPeriod = br.ReadInt32();
        ScrollYSpeed = br.ReadInt32();
        ScrollYPeriod = br.ReadInt32();
    }

    public override string ToString()
    {
        return $"{FactorXDenom} {FactorXNum} {FactorYDenom} {FactorYNum} {ScrollXSpeed} {ScrollXPeriod} {ScrollYSpeed} {ScrollYPeriod}";
    }
}