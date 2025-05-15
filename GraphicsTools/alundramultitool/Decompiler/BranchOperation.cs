namespace alundramultitool.Decompiler;

public class BranchOperation<T> where T : IsInstruction
{
    public BranchOperation(T instruction, CodeBlock<T> block)
    {
        Instruction = instruction;
        Block = block;
        if (instruction.Cmd == "beq" || instruction.Cmd == "bne")
        {
            Comp1 = Mips.GetRegister(instruction.Rs);
            Comp2 = Mips.GetRegister(instruction.Rt);
            if (Comp1 == "r0")
            {
                Comp1 = "0";
            }

            if (Comp2 == "r0")
            {
                Comp2 = "0";
            }
        }
        else
        {
            Comp1 = Mips.GetRegister(instruction.Rs);
            Comp2 = "0";
        }
    }
    public T Instruction;
    public CodeBlock<T> Block;
    public string Comp1;
    public string Comp2;

    //todo make this generic and implimented for each instruction set
    public string Print()
    {
        var text = "if";

        T previnst = null;
        var prevdex = Block.Instructions.IndexOf(Instruction) - 1;
        if (prevdex >= 0)
        {
            previnst = Block.Instructions[prevdex];
        }

        //"beq", "bgezal", "bgez", "bltz", "bltzal", "bgtz", "blez", "bne"
        switch (Instruction.Cmd)
        {
            case "beq":
                if (Comp2 == "0" && previnst != null && previnst.Rd == Instruction.Rs && (previnst.Cmd == "slt" || previnst.Cmd == "sltu"))
                {
                    Comp1 = Mips.GetRegister(previnst.Rs);
                    Comp2 = Mips.GetRegister(previnst.Rt);
                    if (Comp1 == "r0")
                    {
                        Comp1 = "0";
                    }

                    if (Comp2 == "r0")
                    {
                        Comp2 = "0";
                    }

                    text += string.Format("({0} < {1})", Comp1, Comp2);
                }
                else
                {
                    text += string.Format("({0} != {1})", Comp1, Comp2);
                }

                break;
            case "bne":
                text += string.Format("({0} == {1})", Comp1, Comp2);
                break;
            case "bgez":
                text += string.Format("({0} < {1})", Comp1, Comp2);
                break;
            case "bltz":
                text += string.Format("({0} >= {1})", Comp1, Comp2);
                break;
            case "bgtz":
                text += string.Format("({0} <= {1})", Comp1, Comp2);
                break;
            case "blez":
                text += string.Format("({0} > {1})", Comp1, Comp2);
                break;
        }

        return text;
    }
}