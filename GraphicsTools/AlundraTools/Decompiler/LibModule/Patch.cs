using System.Text;

namespace AlundraTools.Decompiler.LibModule;

public class Patch
{
    public void Patch2(BinaryReader br)
    {
        var type = br.ReadByte();
        switch(type)
        {
            case (byte)RelocType.WordLiteral:
            case (byte)RelocType.FunctionCall:
            case (byte)RelocType.UpperImmediate:
            case (byte)RelocType.LowerImmediate:
                RelocType = (RelocType)type;
                break;
            default:
                throw new Exception("bad patch");
        }
        Offset = br.ReadUInt16();

        var node = new PatchNode(br);
        if (node.Type == PatchType.Expr && node.Left.Type == PatchType.Value)
        {
            if (node.Right.Type == PatchType.SectionBase)
            {
                //swap left and right.  why?
                var tmp = node.Left;
                node.Left = node.Right;
                node.Right = tmp;
            }
            else if(node.Right.Type != PatchType.Ref)
            {
                node = node.Right;
            }
        }

        switch(node.Type)
        {
            case PatchType.Expr:
                switch(node.Left.Type)
                {
                    case PatchType.SectionBase:
                        PatchType = node.Left.Type;
                        Symbol = node.Left.Symbol;
                        Value = node.Right.Value;
                        break;
                    case PatchType.SectionStart:
                        PatchType = PatchType.SectionSize;
                        Symbol = node.Left.Symbol;
                        break;
                    case PatchType.Value:
                        PatchType = PatchType.Ref;
                        Symbol = node.Right.Symbol;
                        break;
                    default:
                        throw new Exception("bad patch");
                }
                break;
            case PatchType.Ref:
            case PatchType.SectionBase:
            case PatchType.SectionStart:
            case PatchType.SectionEnd:
                PatchType = node.Type;
                Symbol = node.Symbol;
                break;
            default:
                throw new Exception("bad patch");
        }
    }
    public StringBuilder ActivityLog = new();
    void Log(string text)
    {
        ActivityLog.Append(text);
    }
    public Patch(BinaryReader br, int offsetadjust)
    {
        var type = br.ReadByte();
            
        switch (type)
        {
            case (byte)RelocType.WordLiteral:
            case (byte)RelocType.FunctionCall:
            case (byte)RelocType.UpperImmediate:
            case (byte)RelocType.LowerImmediate:
                RelocType = (RelocType)type;
                break;
            default:
                throw new Exception("bad patch");
        }
        Offset = (ushort)(br.ReadUInt16() + offsetadjust);

        Log("Patch type " + type + " at offset " + Offset.ToString("x") + " with ");
        var next = br.ReadByte();
        Log($"({next.ToString()}) ");
        br.BaseStream.Position--;

        var node = new PatchNode(br);
        if (node.Type == PatchType.Expr && node.Left.Type == PatchType.Value)
        {
            if (node.Right.Type == PatchType.SectionBase)
            {
                //swap left and right.  why?
                var tmp = node.Left;
                node.Left = node.Right;
                node.Right = tmp;
            }
            else if (node.Right.Type != PatchType.Ref)
            {
                node = node.Right;
            }
        }

        switch (node.Type)
        {
            case PatchType.Expr:
                switch (node.Left.Type)
                {
                    case PatchType.SectionBase:
                        PatchType = node.Left.Type;
                        Symbol = node.Left.Symbol;
                        Value = node.Right.Value;
                        break;
                    case PatchType.SectionStart:
                        PatchType = PatchType.SectionSize;
                        Symbol = node.Left.Symbol;
                        break;
                    case PatchType.Value:
                        PatchType = PatchType.Ref;
                        Symbol = node.Right.Symbol;
                        break;
                    default:
                        throw new Exception("bad patch");
                }
                break;
            case PatchType.Ref:
            case PatchType.SectionBase:
            case PatchType.SectionStart:
            case PatchType.SectionEnd:
                PatchType = node.Type;
                Symbol = node.Symbol;
                break;
            default:
                throw new Exception("bad patch");
        }

        Log($"{PatchType} {Symbol.ToString("x")} {Value.ToString("x")}");
    }
    public uint Value;
    public ushort Offset;
    public ushort Symbol;
    public RelocType RelocType;
    public PatchType PatchType;

    public override string ToString()
    {
        return $"{PatchType}:{RelocType}";
    }
}