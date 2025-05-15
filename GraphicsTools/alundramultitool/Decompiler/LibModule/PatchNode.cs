namespace alundramultitool.Decompiler.LibModule;

public class PatchNode
{
    public PatchNode(BinaryReader br)
    {
        var state = br.ReadByte();
        if (state<=24)
        {
            switch(state)
            {
                case 0:
                case (byte)PatchType.Value:
                    Value = br.ReadUInt32();
                    Type = PatchType.Value;
                    break;
                case (byte)PatchType.Ref:
                case (byte)PatchType.SectionBase:
                case (byte)PatchType.SectionStart:
                case (byte)PatchType.SectionEnd:
                    Symbol = br.ReadUInt16();
                    Type = (PatchType)state;
                    break;
            }
        }
        else
        {
            Type = PatchType.Expr;
            switch(state)
            {
                case (byte)PatchOp.Add:
                case (byte)PatchOp.Sub:
                case (byte)PatchOp.Div:
                case (byte)PatchOp.Exc:
                    break;
                default:
                    throw new Exception("bad patch");
            }

            Op = (PatchOp)state;

            Left = new PatchNode(br);
            Right = new PatchNode(br);
        }
    }
    public PatchNode Left;
    public PatchNode Right;
    public uint Value;
    public ushort Symbol;
    public PatchType Type;
    public PatchOp Op;
}