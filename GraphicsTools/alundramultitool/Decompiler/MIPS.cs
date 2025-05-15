namespace alundramultitool.Decompiler
{
    public class Mips: InstructionSet
    {
        public static string GetRegister(int num)
        {
            return "r" + num;
        }

        public enum InstructionType
        {
            Rtype,
            Jtype,
            Itype
        }
        
        public class Instruction : IsInstruction
        {
            public Instruction(uint address, uint instruction)
            {
                Address = address;
                Instruction = instruction;

                Opcode = ValAtOffset(instruction, 0x3f, 26);

                var onevalformat = "{0} {1}";
                var twovalformat = "{0} {1}, {2}";
                var threevalformat = "{0} {1},{2},{3}";
                var threevalmemoryformat = "{0} {1},{3}({2})";

                if (instruction == 0)
                {
                    Cmd = "nop";
                    Display = Cmd;
                }
                else if (Opcode == 0x0)//R type instruction
                {
                    Type = InstructionType.Rtype;
                    Funct = (int)(instruction & 0x3f);
                    Rs = ValAtOffset(instruction, 0x1f, 6 + 5 * 3);
                    var srs = GetRegister(Rs);
                    Rt = ValAtOffset(instruction, 0x1f, 6 + 5 * 2);
                    var srt = GetRegister(Rt);
                    Rd = ValAtOffset(instruction, 0x1f, 6 + 5 * 1);
                    var srd = GetRegister(Rd);
                    Shamt = ValAtOffset(instruction, 0x1f, 6 + 5 * 0);

                    switch (Funct)
                    {
                        case 0x20://add
                            Cmd = "add";
                            Display = string.Format(threevalformat, "add", srd, srs, srt);
                            break;
                        case 0x21://add unsigned
                            Cmd = "addu";
                            Display = string.Format(threevalformat, "addu", srd, srs, srt);
                            break;
                        case 0x22://subtract
                            Cmd = "sub";
                            Display = string.Format(threevalformat, "sub", srd, srs, srt);
                            break;
                        case 0x23://subtract unsigned
                            Cmd = "subu";
                            Display = string.Format(threevalformat, "subu", srd, srs, srt);
                            break;
                        case 0x18://multiply
                            Cmd = "mult";
                            Display = string.Format(twovalformat, "mult", srs, srt);
                            break;
                        case 0x19://multiply unsigned
                            Cmd = "multu";
                            Display = string.Format(twovalformat, "multu", srs, srt);
                            break;
                        case 0x1a://divide
                            Cmd = "div";
                            Display = string.Format(twovalformat, "div", srs, srt);
                            break;
                        case 0x1b://divide unsigned
                            Cmd = "divu";
                            Display = string.Format(twovalformat, "divu", srs, srt);
                            break;
                        case 0x10://move from hi
                            Cmd = "mfhi";
                            Display = string.Format(onevalformat, "mfhi", srd);
                            break;
                        case 0x12://move from low
                            Cmd = "mflo";
                            Display = string.Format(onevalformat, "mflo", srd);
                            break;
                        case 0x24://and
                            Cmd = "and";
                            Display = string.Format(threevalformat, "and", srd, srs, srt);
                            break;
                        case 0x25://or
                            Cmd = "or";
                            Display = string.Format(threevalformat, "or", srd, srs, srt);
                            break;
                        case 0x26://xor
                            Cmd = "xor";
                            Display = string.Format(threevalformat, "xor", srd, srs, srt);
                            break;
                        case 0x27://nor
                            Cmd = "nor";
                            Display = string.Format(threevalformat, "nor", srd, srs, srt);
                            break;
                        case 0x2a://set on less than
                            Cmd = "slt";
                            Display = string.Format(threevalformat, "slt", srd, srs, srt);
                            break;
                        case 0x2b://set on less than unsigned
                            Cmd = "sltu";
                            Display = string.Format(threevalformat, "slt", srd, srs, srt);
                            break;
                        case 0x0://shift left logical immediate
                            Cmd = "sll";
                            Display = string.Format(threevalformat, "sll", srd, srt, Shamt);
                            break;
                        case 0x2://shift right logical immediate
                            Cmd = "srl";
                            Display = string.Format(threevalformat, "srl", srd, srt, Shamt);
                            break;
                        case 0x3://shift right arithmetic immediate
                            Cmd = "sra";
                            Display = string.Format(threevalformat, "sra", srd, srt, Shamt);
                            break;
                        case 0x4://shift left logical
                            Cmd = "sllv";
                            Display = string.Format(threevalformat, "sllv", srd, srt, srs);
                            break;
                        case 0x6://shift right logical
                            Cmd = "srlv";
                            Display = string.Format(threevalformat, "srlv", srd, srt, srs);
                            break;
                        case 0x7://shift right arithmetic
                            Cmd = "srav";
                            Display = string.Format(threevalformat, "srav", srd, srt, srs);
                            break;
                        case 0x8://jump register
                            Cmd = "jr";
                            Display = string.Format(onevalformat, "jr", srs);
                            break;
                        case 0x9://jump and link register
                            Cmd = "jalr";
                            Display = string.Format(twovalformat, "jalr", srs, srd);
                            break;
                        default:
                            Cmd = "???";
                            Display = "unknown R type funct: " + Funct.ToString("x2");
                            break;
                    }
                }
                else if (Opcode == 0x2 || Opcode == 0x3)//J type instruction
                {
                    Type = InstructionType.Jtype;
                    Immediateu = (uint)ValAtOffset(instruction, 0x3ffffff, 0);
                    ReferencedAddress = Immediateu << 2;
                    switch (Opcode)
                    {
                        case 0x2:
                            Cmd = "j";
                            Display = string.Format(onevalformat, "j", "0x" + ReferencedAddress.ToString("x8"));
                            break;
                        case 0x3:
                            Cmd = "jal";
                            Display = string.Format(onevalformat, "jal", "0x" + ReferencedAddress.ToString("x8"));
                            break;
                    }
                }
                else//I type instruction
                {
                    Type = InstructionType.Itype;
                    Rs = ValAtOffset(instruction, 0x1f, 6 + 5 * 3);
                    var srs = GetRegister(Rs);
                    Rt = ValAtOffset(instruction, 0x1f, 6 + 5 * 2);
                    var srt = GetRegister(Rt);
                    Immediate = (short)SignedValAtOffset(instruction, 0xffff, 0);
                    var simmediate = "0x" + ((short)Immediate).ToString("x4");
                    Immediateu = (ushort)ValAtOffset(instruction, 0xffff, 0);
                    var simmediateu = "0x" + ((ushort)Immediateu).ToString("x4");

                    switch (Opcode)
                    {
                        case 0x8://add immediate
                            Cmd = "addi";
                            Display =  string.Format(threevalformat, "addi", srt, srs, simmediate);
                            break;
                        case 0x9://add immediate unsigned (the immediate is always signed, this is kind of nuts 
                            Cmd = "addiu";
                            Display = string.Format(threevalformat, "addiu", srt, srs, simmediate);
                            break;
                        case 0x23://load word
                            Cmd = "lw";
                            Display = string.Format(threevalmemoryformat, "lw", srt, srs, simmediate);
                            break;
                        case 0x21://load halfword
                            Cmd = "lh";
                            Display = string.Format(threevalmemoryformat, "lh", srt, srs, simmediate);
                            break;
                        case 0x25://load halfword unsigned
                            Cmd = "lhu";
                            Display = string.Format(threevalmemoryformat, "lhu", srt, srs, simmediate);
                            break;
                        case 0x20://load byte
                            Cmd = "lb";
                            Display = string.Format(threevalmemoryformat, "lb", srt, srs, simmediate);
                            break;
                        case 0x24://load byte unsigned
                            Cmd = "lbu";
                            Display = string.Format(threevalmemoryformat, "lbu", srt, srs, simmediate);
                            break;
                        case 0x2b://store word
                            Cmd = "sw";
                            Display = string.Format(threevalmemoryformat, "sw", srt, srs, simmediate);
                            break;
                        case 0x29://store halfword
                            Cmd = "sh";
                            Display = string.Format(threevalmemoryformat, "sh", srt, srs, simmediate);
                            break;
                        case 0x28://store byte
                            Cmd = "sb";
                            Display = string.Format(threevalmemoryformat, "sb", srt, srs, simmediate);
                            break;
                        case 0xf://load upper immediate
                            Cmd = "lui";
                            Display = string.Format(twovalformat, "lui", srt, simmediate);
                            break;
                        case 0xc://and immediate
                            Cmd = "andi";
                            Display = string.Format(threevalformat, "andi", srt, srs, simmediate);
                            break;
                        case 0xd://or immediate
                            Cmd = "ori";
                            Display = string.Format(threevalformat, "ori", srt, srs, simmediate);
                            break;
                        case 0xe://xor immediate
                            Cmd = "xori";
                            Display = string.Format(threevalformat, "xori", srt, srs, simmediate);
                            break;
                        case 0xa://set on less than immediate
                            Cmd = "slti";
                            Display = string.Format(threevalformat, "slti", srt, srs, simmediate);
                            break;
                        case 0xb://set on less than immediate unsigned
                            Cmd = "sltiu";
                            Display = string.Format(threevalformat, "sltiu", srt, srs, simmediateu);
                            break;
                        case 0x4://branch on equal
                            Cmd = "beq";
                            ReferencedAddress = (uint)(address + 4 + (Immediate << 2));
                            Display = string.Format(threevalformat, "beq", srs, srt, "0x" + ReferencedAddress.ToString("x8"));
                            break;
                        case 0x1://branch on greater than or equal to zero
                            if (Rt == 0x11)
                            {
                                Cmd = "bgezal";
                            }
                            else if (Rt == 0x1)
                            {
                                Cmd = "bgez";
                            }
                            else if (Rt == 0)
                            {
                                Cmd = "bltz";
                            }
                            else if (Rt == 0x10)
                            {
                                Cmd = "bltzal";
                            }

                            ReferencedAddress = (uint)(address + 4 + (Immediate << 2));
                            Display = string.Format(twovalformat, Cmd, srs, "0x" + ReferencedAddress.ToString("x8"));
                            break;
                        case 0x7://branch on greater than zero
                            Cmd = "bgtz";
                            ReferencedAddress = (uint)(address + 4 + (Immediate << 2));
                            Display = string.Format(twovalformat, Cmd, srs, "0x" + ReferencedAddress.ToString("x8"));
                            break;
                        case 0x6://branch on less than or equal to zero
                            Cmd = "blez";
                            ReferencedAddress = (uint)(address + 4 + (Immediate << 2));
                            Display = string.Format(twovalformat, "blez", srs, "0x" + ReferencedAddress.ToString("x8"));
                            break;
                        case 0x5://branch on not equal
                            Cmd = "bne";
                            ReferencedAddress = (uint)(address + 4 + (Immediate << 2));
                            Display = string.Format(threevalformat, "bne", srs, srt, "0x" + ReferencedAddress.ToString("x8"));
                            break;
                        default:
                            Cmd = "?";
                            Display = "unknown I type opcode: " + Opcode.ToString("x2");
                            break;

                    }
                }
            }

            public InstructionType Type;


            public override bool IsCall
            {
                get
                {
                    return new[] { "jal", "jalr"}.Contains(Cmd);
                }
            }

            public override bool IsBranch
            {
                get
                {
                    return new[] { "beq", "bgezal", "bgez", "bltz", "bltzal", "bgtz", "blez", "bne" }.Contains(Cmd);
                }
            }

            public override bool IsJump
            {
                get
                {
                    return Cmd == "j";
                }
            }

            public override bool IsReturn
            {
                get
                {
                    return Cmd == "jr" && Rs == 31;
                }
            }

            public override bool IsAssignment
            {
                get
                {
                    return Type == InstructionType.Itype && Cmd[0] == 's';
                }
            }
            public override void GetAssignmentGlobals(out uint left, out string right, CodeBlock<IsInstruction> block)
            {
                left = 0;
                right = "?";
                //assumed itype
                if (Type == InstructionType.Itype)
                {
                    var fulladdr = GetGlobalVariable(block);
                    if (fulladdr != 0)
                    {
                        left = fulladdr;
                        var reg = Rt;

                        right = "r" + reg;

                        var mdex = block.Instructions.IndexOf(this);
                        var seekback = 2;
                        for (var dex = mdex - 1; dex >= mdex - (1 + seekback) && dex >= 0; dex--)
                        {
                            var binst = (Instruction)block.Instructions[dex];
                            if (binst.Type == InstructionType.Rtype && binst.Rd == reg)
                            {
                                right = "r" + reg + "(" + binst.Disp + ")";

                                break;
                            }
                            else if (binst.Type == InstructionType.Itype)
                            {
                                if (binst.IsAssignment)
                                {
                                    if (binst.Rs == reg)
                                    {
                                        right = "r" + reg + "(" + binst.Disp + ")";

                                        break;
                                    }
                                }
                                else
                                {
                                    if (binst.Rt == reg)
                                    {
                                        right = "r" + reg + "(" + binst.Disp + ")";

                                        break;
                                    }
                                }
                            }

                        }
                    }


                }
            }
            
            public override uint GetGlobalVariable(CodeBlock<IsInstruction> block)
            {
                GlobalRegisterOffset = 0;
                //var varlist = GraphicsTools.Alundra.DebugSymbols.GlobalVariableNames;
                //if its a global variable then return the address
                var typesthatreallyare = new[] { "addiu", "addi", "ori" };
                var typesthatpotentiallyare = new[] { "lw", "sw", "lhu", "lh", "shu", "sh", "lbu", "lb", "sbu", "sb" };//
                if (typesthatreallyare.Contains(Cmd) || typesthatpotentiallyare.Contains(Cmd))
                {
                    var mdex = block.Instructions.IndexOf(this);
                    var seekback = 2;
                    for (var dex = mdex - 1; dex >= mdex - (1 + seekback) && dex >= 0; dex--)
                    {
                        var binst = (Instruction)block.Instructions[dex];
                        if (binst.Cmd == "lui" && binst.Rt == Rs)
                        {
                            uint fulladdr = 0;
                            switch (Cmd)
                            {
                                case "ori":
                                    fulladdr = (uint)((uint)(((uint)binst.Immediate & 0xff) << 16) | Immediateu);
                                    break;
                                case "addiu":
                                case "addi":
                                    fulladdr = (uint)((uint)(((uint)binst.Immediate & 0xff) << 16) + Immediate);
                                    break;

                                case "lw":
                                case "sw":
                                case "lhu":
                                case "lh":
                                case "shu":
                                case "sh":
                                case "lb":
                                case "sb":
                                case "lbu":
                                case "sbu":
                                    fulladdr = (uint)((uint)(((uint)binst.Immediate & 0xff) << 16) + Immediate);
                                    break;
                            }
                            return fulladdr;

                        }
                        else if (binst.Type == InstructionType.Itype && binst.Rt == Rs)
                        {
                            break;//dont seek back any further because the register in question was already overwritten
                        }
                        else if (binst.Type == InstructionType.Rtype && binst.Rd == Rs)
                        {
                            //"addu", srd, srs, srt)
                            if ((binst.Cmd == "addu" || binst.Cmd == "add") && binst.Rs == Rs )
                            {
                                GlobalRegisterOffset = binst.Rt;
                            }
                            else
                            {
                                break;
                            }
                        }
                    }
                }

                return 0;
            }
        }

        

        
    }
}
