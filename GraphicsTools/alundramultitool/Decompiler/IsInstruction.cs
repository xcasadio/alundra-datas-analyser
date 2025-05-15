namespace alundramultitool.Decompiler;

public abstract class IsInstruction
{
    public string Cmd;
    public uint ReferencedAddress;
    public string Display;

    public uint Instruction;
    public int Opcode;
    public int Funct;
    public int Rs;
    public int Rt;
    public int Rd;//displacement register
    public int Shamt;//shift amount
    public uint Address;
    public int Immediate;
    public uint Immediateu;

    public int Rn;//destination register
    public int Rm;//source register
    public int Disp;//displacement immediate

    public abstract bool IsBranch { get; }
    public abstract bool IsCall { get; }
    public abstract bool IsJump { get; }
    public abstract bool IsReturn { get; }

    public int GlobalRegisterOffset = 0;
    public abstract uint GetGlobalVariable(CodeBlock<IsInstruction> block);

    public abstract bool IsAssignment { get;}

    public abstract void GetAssignmentGlobals(out uint left, out string right, CodeBlock<IsInstruction> block);
}