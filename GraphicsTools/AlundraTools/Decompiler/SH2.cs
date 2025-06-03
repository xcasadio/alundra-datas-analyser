namespace AlundraTools.Decompiler
{
    public class Sh2: InstructionSet
    {
        const int Unused = 0x80001;

        //reg flags
        const int PredefReg = 0x80;
        const int PreDecReg = 0x800; //@-REG
        const int PostIncReg = 0x8000;//@REG+



        const int RegR0 = PredefReg + 0;
        const int RegPr = PredefReg + 1;
        const int RegSr = PredefReg + 2;
        const int RegMach = PredefReg + 3;
        const int RegMacl = PredefReg + 4;
        const int RegGbr = PredefReg + 5;
        const int RegVbr = PredefReg + 6;

        static string GetRegister(int reg)
        {
            if (reg == Unused)
            {
                return "?";
            }

            var numpart = reg & 0x3f;
            var sr = "?";
            if ((reg & PredefReg) == PredefReg)
            {
                switch(numpart)
                {
                    case 0:
                        sr = "r0";
                        break;
                    case 1:
                        sr = "pr";
                        break;
                    case 2:
                        sr = "sr";
                        break;
                    case 3:
                        sr = "mach";
                        break;
                    case 4:
                        sr = "macl";
                        break;
                    case 5:
                        sr = "gbr";
                        break;
                    case 6:
                        sr = "vbr";
                        break;
                }
            }
            else
            {
                sr = "r" + numpart;
            }

            if ((reg & PreDecReg) == PreDecReg)
            {
                sr = "-" + sr;
            }

            if ((reg & PostIncReg) == PostIncReg)
            {
                sr = sr + "+";
            }

            return sr;
        }

        static string GetImmediate(int i)
        {
            return "#" + i.ToString("x");
        }

        static string GetReferencedAddress(uint addr)
        {
            return "0x" + addr.ToString("x");
        }

        public class Instruction : IsInstruction
        {
            


            int[] _nibs = new int[4];
            public Instruction(uint address, uint instruction)
            {
                Address = address;
                Instruction = instruction;


                _nibs[0] = ValAtOffset(instruction, 0xf, 12);
                _nibs[1] = ValAtOffset(instruction, 0xf, 8);
                _nibs[2] = ValAtOffset(instruction, 0xf, 4);
                _nibs[3] = ValAtOffset(instruction, 0xf, 0);

                Opcode = _nibs[0];

                var novalformat = "{0}";
                var onevalformat = "{0} {1}";
                var onevalmemoryformat = "{0} @{1}";

                var twovalformat = "{0} {1},{2}";

                var twovalmemoryformat = "{0} @{1},@{2}";
                var twovalmemorydestformat = "{0} {1},@{2}";
                var twovalmemorysrcformat = "{0} @{1},{2}";

                var threevalmemorydestformat = "{0} {1},@({2},{3})";
                var threevalmemorysrcformat = "{0} @({1},{3}),{2}";

                //string threevalmemorydestformatalt = "{0} {1},{3}({2})";
                //string threevalmemorysrcformatalt = "{0} {3}({1}),{2}";


                var unknownformat = "{0} uknown opcode";

                Rn = Unused;//destination reg
                Rm = Unused;//source reg
                Rd = Unused;//displacement reg
                Disp = Unused;//displacement immediate
                Immediate = Unused;
                Immediateu = Unused;
                ReferencedAddress = Unused;

                var format = unknownformat;

                if (instruction == 0)
                {
                    Cmd = "nop";
                    format = novalformat;
                }
                else
                {
                    switch (Opcode)
                    {
                        case 0:
                            Funct = ValAtOffset(instruction, 0x3f, 0);
                            switch (Funct)
                            {
                                case 0x8://CLRT 0000000000001000 0 → T 1
                                    Cmd = "clrt";
                                    format = novalformat;
                                    break;
                                case 0x9://NOP 0000000000001001 No operation 
                                    Cmd = "nop";
                                    format = novalformat;
                                    break;
                                case 0xb://RTS 0000000000001011 Delayed branch, PR → PC
                                    Cmd = "rts";
                                    format = novalformat;
                                    break;
                                case 0x18://SETT 0000000000011000 1 → T
                                    Cmd = "sett";
                                    format = novalformat;
                                    break;
                                case 0x19://DIV0U 0000000000011001 0 → M/Q/T
                                    Cmd = "div0u";
                                    format = novalformat;
                                    break;
                                case 0x1b://SLEEP 0000000000011011 Sleep
                                    Cmd = "sleep";
                                    format = novalformat;
                                    break;
                                case 0x28://CLRMAC 0000000000101000 0 → MACH, MACL 
                                    Cmd = "clrmac";
                                    format = novalformat;
                                    break;
                                case 0x2b://RTE 0000000000101011 Delayed branch, stack area → PC / SR
                                    Cmd = "rte";//return
                                    format = novalformat;
                                    break;
                                case 0x2://STC SR,Rn 0000nnnn00000010 SR → Rn
                                    Cmd = "stc";//store control reg
                                    Rm = RegSr;
                                    Rn = _nibs[1];
                                    format = twovalformat;
                                    break;
                                case 0x3://BSRF Rm 0000mmmm00000011 Delayed branch, PC → PR, Rm + PC → PC
                                    Cmd = "bsrf";//branch sub routine far
                                    Rm = _nibs[1];
                                    format = onevalformat;
                                    break;
                                case 0xa://STS MACH,Rn 0000nnnn00001010 MACH → Rn
                                    Cmd = "sts";//store system reg
                                    Rm = RegMach;
                                    Rn = _nibs[1];
                                    format = twovalformat;
                                    break;
                                case 0x12://STC GBR,Rn 0000nnnn00010010 GBR → Rn
                                    Cmd = "stc";//store control reg
                                    Rm = RegGbr;
                                    Rn = _nibs[1];
                                    format = twovalformat;
                                    break;
                                case 0x1a://STS MACL,Rn 0000nnnn00011010 MACL → Rn
                                    Cmd = "sts";//store system reg
                                    Rm = RegMacl;
                                    Rn = _nibs[1];
                                    format = twovalformat;
                                    break;
                                case 0x22://STC VBR,Rn 0000nnnn00100010 VBR → Rn
                                    Cmd = "stc";//store control reg
                                    Rm = RegVbr;
                                    Rn = _nibs[1];
                                    format = twovalformat;
                                    break;
                                case 0x23://BRAF Rm 0000mmmm00100011 Delayed branch, Rm + PC → PC
                                    Cmd = "braf";//branch far
                                    Rm = _nibs[1];
                                    format = onevalformat;
                                    break;
                                case 0x29://MOVT Rn 0000nnnn00101001 T → Rn
                                    Cmd = "movt";//move t bit
                                    Rn = _nibs[1];
                                    format = onevalformat;
                                    break;
                                case 0x2a://STS PR,Rn 0000nnnn00101010 PR → Rn 1
                                    Cmd = "sts";//store system reg
                                    Rm = RegPr;
                                    Rn = _nibs[1];
                                    format = twovalformat;
                                    break;

                                case 0x4://MOV.B Rm,@(R0,Rn) 0000nnnnmmmm0100 Rm → (R0 + Rn)
                                    Cmd = "mov.b";
                                    Rn = _nibs[1];
                                    Rm = _nibs[2];
                                    Rd = RegR0;
                                    format = threevalmemorydestformat;
                                    break;
                                case 0x5://MOV.W Rm,@(R0,Rn) 0000nnnnmmmm0101 Rm → (R0 + Rn)
                                    Cmd = "mov.w";
                                    Rn = _nibs[1];
                                    Rm = _nibs[2];
                                    Rd = RegR0;
                                    format = threevalmemorydestformat;
                                    break;
                                case 0x6://MOV.L Rm,@(R0,Rn) 0000nnnnmmmm0110 Rm → (R0 + Rn)
                                    Cmd = "mov.l";
                                    Rn = _nibs[1];
                                    Rm = _nibs[2];
                                    Rd = RegR0;
                                    format = threevalmemorydestformat;
                                    break;
                                case 0x7://MUL.L Rm,Rn 0000nnnnmmmm0111 Rn x Rm → MACL
                                    Cmd = "mul.l";
                                    Rn = _nibs[1];
                                    Rm = _nibs[2];
                                    //rd = reg_macl;
                                    format = twovalformat;
                                    break;

                                case 0xc://MOV.B @(R0,Rm),Rn 0000nnnnmmmm1100 (R0 + Rm) → sign extension → Rn
                                    Cmd = "mov.b";
                                    Rn = _nibs[1];
                                    Rm = _nibs[2];
                                    Rd = RegR0;
                                    format = threevalmemorysrcformat;
                                    break;
                                case 0xd://MOV.W @(R0,Rm),Rn 0000nnnnmmmm1101 (R0 + Rm) → sign extension → Rn
                                    Cmd = "mov.w";
                                    Rn = _nibs[1];
                                    Rm = _nibs[2];
                                    Rd = RegR0;
                                    format = threevalmemorysrcformat;
                                    break;
                                case 0xe://MOV.L @(R0,Rm),Rn 0000nnnnmmmm1110 (R0 + Rm) → Rn
                                    Cmd = "mov.l";
                                    Rn = _nibs[1];
                                    Rm = _nibs[2];
                                    Rd = RegR0;
                                    format = threevalmemorysrcformat;
                                    break;
                                case 0xf://MAC.L @Rm+,@Rn+ 0000nnnnmmmm1111 Signed, (Rn) x (Rm) + MAC → MAC
                                    Cmd = "mac.l";
                                    Rn = _nibs[1] | PostIncReg;
                                    Rm = _nibs[2] | PostIncReg;
                                    format = twovalmemoryformat;
                                    break;
                                default:
                                    Cmd = "?";
                                    format = unknownformat;
                                    break;
                            }
                            break;
                        case 1://MOV.L Rm,@(disp,Rn) 0001nnnnmmmmdddd Rm → (disp × 4 + Rn)
                            Cmd = "mov.l";
                            Rn = _nibs[1];
                            Rm = _nibs[2];
                            Disp = _nibs[3];
                            format = threevalmemorydestformat;
                            break;
                        case 2:
                            Funct = _nibs[3];
                            switch (Funct)
                            {
                                case 0x0://MOV.B Rm,@Rn 0010nnnnmmmm0000 Rm → (Rn)
                                    Cmd = "mov.b";
                                    Rn = _nibs[1];
                                    Rm = _nibs[2];
                                    format = twovalmemorydestformat;
                                    break;
                                case 0x1://MOV.W Rm,@Rn 0010nnnnmmmm0001 Rm → (Rn)
                                    Cmd = "mov.w";
                                    Rn = _nibs[1];
                                    Rm = _nibs[2];
                                    format = twovalmemorydestformat;
                                    break;
                                case 0x2://MOV.L Rm,@Rn 0010nnnnmmmm0010 Rm → (Rn)
                                    Cmd = "mov.l";
                                    Rn = _nibs[1];
                                    Rm = _nibs[2];
                                    format = twovalmemorydestformat;
                                    break;

                                case 0x4://MOV.B Rm,@-Rn 0010nnnnmmmm0100 Rn – 1 → Rn, Rm → (Rn)
                                    Cmd = "mov.b";
                                    Rn = _nibs[1] | PreDecReg;
                                    Rm = _nibs[2];
                                    format = twovalmemorydestformat;
                                    break;
                                case 0x5://MOV.W Rm,@–Rn 0010nnnnmmmm0101 Rn – 2 → Rn, Rm → (Rn)
                                    Cmd = "mov.w";
                                    Rn = _nibs[1] | PreDecReg;
                                    Rm = _nibs[2];
                                    format = twovalmemorydestformat;
                                    break;
                                case 0x6://MOV.L Rm,@–Rn 0010nnnnmmmm0110 Rn – 4 → Rn, Rm → (Rn)
                                    Cmd = "mov.l";
                                    Rn = _nibs[1] | PreDecReg;
                                    Rm = _nibs[2];
                                    format = twovalmemorydestformat;
                                    break;

                                case 0x7://DIV0S Rm,Rn 0010nnnnmmmm0111 MSB of Rn → Q, MSB of Rm → M, M ^ Q → T
                                    Cmd = "div0s";
                                    Rn = _nibs[1];
                                    Rm = _nibs[2];
                                    format = twovalformat;
                                    break;
                                case 0x8://TST Rm,Rn 0010nnnnmmmm1000 Rn & Rm, when result is 0, 1 → T
                                    Cmd = "tst";
                                    Rn = _nibs[1];
                                    Rm = _nibs[2];
                                    format = twovalformat;
                                    break;
                                case 0x9://AND Rm,Rn 0010nnnnmmmm1001 Rn & Rm → Rn
                                    Cmd = "and";
                                    Rn = _nibs[1];
                                    Rm = _nibs[2];
                                    format = twovalformat;
                                    break;
                                case 0xa://XOR Rm,Rn 0010nnnnmmmm1010 Rn ^ Rm → Rn
                                    Cmd = "xor";
                                    Rn = _nibs[1];
                                    Rm = _nibs[2];
                                    format = twovalformat;
                                    break;
                                case 0xb://OR Rm,Rn 0010nnnnmmmm1011 Rn | Rm → Rn
                                    Cmd = "or";
                                    Rn = _nibs[1];
                                    Rm = _nibs[2];
                                    format = twovalformat;
                                    break;
                                case 0xc://CMP/STR Rm,Rn 0010nnnnmmmm1100 When a byte in Rn equals a byte in Rm, 1 → T
                                    Cmd = "cmp/str";
                                    Rn = _nibs[1];
                                    Rm = _nibs[2];
                                    format = twovalformat;
                                    break;
                                case 0xd://XTRCT Rm,Rn 0010nnnnmmmm1101 Center 32 bits of Rm and Rn → Rn
                                    Cmd = "xtrct";
                                    Rn = _nibs[1];
                                    Rm = _nibs[2];
                                    format = twovalformat;
                                    break;
                                case 0xe://MULU.W Rm,Rn 0010nnnnmmmm1110 Unsigned, Rn × Rm → MAC
                                    Cmd = "mulu.w";
                                    Rn = _nibs[1];
                                    Rm = _nibs[2];
                                    format = twovalformat;
                                    break;
                                case 0xf://MULS.W Rm,Rn 0010nnnnmmmm1111 Signed, Rn × Rm → MAC
                                    Cmd = "muls.w";
                                    Rn = _nibs[1];
                                    Rm = _nibs[2];
                                    format = twovalformat;
                                    break;
                                default:
                                    Cmd = "?";
                                    format = unknownformat;
                                    break;
                            }
                            break;
                        case 3:
                            Funct = _nibs[3];
                            Rn = _nibs[1];
                            Rm = _nibs[2];
                            switch (Funct)
                            {
                                case 0x0://CMP/EQ Rm,Rn 0011nnnnmmmm0000 When Rn = Rm, 1 → T 1
                                    Cmd = "cmp/eq";
                                    format = twovalformat;
                                    break;

                                case 0x2://CMP/HS Rm,Rn 0011nnnnmmmm0010 When unsigned and Rn ≥ Rm, 1 → T
                                    Cmd = "cmp/hs";
                                    format = twovalformat;
                                    break;
                                case 0x3://CMP/GE Rm,Rn 0011nnnnmmmm0011 When signed and Rn ≥ Rm, 1 → T
                                    Cmd = "cmp/ge";
                                    format = twovalformat;
                                    break;
                                case 0x4://DIV1 Rm,Rn 0011nnnnmmmm0100 1-step division (Rn ÷ Rm)
                                    Cmd = "div1";
                                    format = twovalformat;
                                    break;
                                case 0x5://DMULU.L Rm,Rn 0011nnnnmmmm0101 Unsigned, Rn x Rm → MACH, MACL
                                    Cmd = "dmulu.l";
                                    format = twovalformat;
                                    break;
                                case 0x6://CMP/HI Rm,Rn 0011nnnnmmmm0110 When unsigned and Rn > Rm, 1 → T
                                    Cmd = "cmp/hi";
                                    format = twovalformat;
                                    break;
                                case 0x7://CMP/GT Rm,Rn 0011nnnnmmmm0111 When signed and Rn > Rm, 1 → T
                                    Cmd = "cmp/gt";
                                    format = twovalformat;
                                    break;
                                case 0x8://SUB Rm,Rn 0011nnnnmmmm1000 Rn – Rm → Rn
                                    Cmd = "sub";
                                    format = twovalformat;
                                    break;

                                case 0xa://SUBC Rm,Rn 0011nnnnmmmm1010 Rn – Rm – T → Rn, borrow → T
                                    Cmd = "subc";
                                    format = twovalformat;
                                    break;
                                case 0xb://SUBV Rm,Rn 0011nnnnmmmm1011 Rn – Rm → Rn, underflow → T
                                    Cmd = "subv";
                                    format = twovalformat;
                                    break;
                                case 0xc://ADD Rm,Rn 0011nnnnmmmm1100 Rm + Rn → Rn
                                    Cmd = "add";
                                    format = twovalformat;
                                    break;
                                case 0xd://DMULS.L Rm,Rn 0011nnnnmmmm1101 Signed, Rn x Rm → MACH, MACL
                                    Cmd = "dmuls.l";
                                    format = twovalformat;
                                    break;
                                case 0xe://ADDC Rm,Rn 0011nnnnmmmm1110 Rn + Rm + T → Rn, carry → T
                                    Cmd = "addc";
                                    format = twovalformat;
                                    break;
                                case 0xf://ADDV Rm,Rn 0011nnnnmmmm1111 Rn + Rm → Rn, overflow → T
                                    Cmd = "addv";
                                    format = twovalformat;
                                    break;

                                default:
                                    Cmd = "?";
                                    format = unknownformat;
                                    break;
                            }
                            break;
                        case 4:
                            Funct = ValAtOffset(instruction, 0x3f, 0);
                            Rn = _nibs[1];
                            switch(Funct)
                            {
                                case 0x0://SHLL Rn 0100nnnn00000000 T ← Rn ← 0
                                    Cmd = "shll";
                                    format = onevalformat;
                                    break;
                                case 0x1://SHLR Rn 0100nnnn00000001 0 → Rn → T 
                                    Cmd = "shlr";
                                    format = onevalformat;
                                    break;
                                case 0x2://STS.L MACH,@–Rn 0100nnnn00000010 Rn – 4 → Rn, MACH → (Rn)
                                    Cmd = "sts.l";
                                    Rn |= PreDecReg;
                                    Rm = RegMach;
                                    format = twovalmemorydestformat;
                                    break;
                                case 0x3://STC.L SR,@-Rn 0100nnnn00000011 Rn – 4 → Rn, SR → (Rn)
                                    Cmd = "stc.l";
                                    Rn |= PreDecReg;
                                    Rm = RegSr;
                                    format = twovalmemorydestformat;
                                    break;
                                case 0x4://ROTL Rn 0100nnnn00000100 T ← Rn ← MSB
                                    Cmd = "rotl";
                                    format = onevalformat;
                                    break;
                                case 0x5://ROTR Rn 0100nnnn00000101 LSB → Rn → T
                                    Cmd = "rotr";
                                    format = onevalformat;
                                    break;
                                case 0x6://LDS.L @Rm+,MACH 0100mmmm00000110 (Rm) → MACH, Rm + 4 → Rm
                                    Cmd = "lds.l";
                                    Rm = _nibs[1] | PostIncReg;
                                    Rn = RegMach; 
                                    format = twovalmemorysrcformat;
                                    break;
                                case 0x7://LDC.L @Rm+,SR 0100mmmm00000111 (Rm) → SR, Rm + 4 → Rm
                                    Cmd = "ldc.l";
                                    Rm = _nibs[1] | PostIncReg;
                                    Rn = RegSr;
                                    format = twovalmemorysrcformat;
                                    break;
                                case 0x8://SHLL2 Rn 0100nnnn00001000 Rn<<2 → Rn
                                    Cmd = "shll2";
                                    format = onevalformat;
                                    break;
                                case 0x9://SHLR2 Rn 0100nnnn00001001 Rn>>2 → Rn
                                    Cmd = "shlr2";
                                    format = onevalformat;
                                    break;
                                case 10://LDS Rm,MACH 0100mmmm00001010 Rm → MACH
                                    Cmd = "lds";
                                    Rm = _nibs[1];
                                    Rn = RegMach;
                                    format = twovalformat;
                                    break;
                                case 11://JSR @Rm 0100mmmm00001011 Delayed branch, PC → PR, Rm → PC
                                    Cmd = "jsr";
                                    Rm = _nibs[1];
                                    Rn = Unused;
                                    format = onevalmemoryformat;
                                    break;
                                case 14://LDC Rm,SR 0100mmmm00001110 Rm → SR
                                    Cmd = "ldc";
                                    Rm = _nibs[1];
                                    Rn = RegSr;
                                    format = twovalformat;
                                    break;
                                case 16://DT Rn 0100nnnn00010000 Rn - 1 → Rn; if Rn is 0, 1 → T, if Rn is nonzero, 0 → T
                                    Cmd = "dt";
                                    format = onevalformat;
                                    break;
                                case 17://CMP/PZ Rn 0100nnnn00010001 Rn ≥ 0, 1 → T
                                    Cmd = "cmp/pz";
                                    format = onevalformat;
                                    break;
                                case 18://STS.L MACL,@–Rn 0100nnnn00010010 Rn – 4 → Rn, MACL → (Rn)
                                    Cmd = "sts.l";
                                    Rn |= PreDecReg;
                                    Rm = RegMacl;
                                    format = twovalmemorydestformat;
                                    break;
                                case 19://STC.L GBR,@-Rn 0100nnnn00010011 Rn – 4 → Rn, GBR → (Rn)
                                    Cmd = "stc.l";
                                    Rn |= PreDecReg;
                                    Rm = RegGbr;
                                    format = twovalmemorydestformat;
                                    break;
                                case 21://CMP/PL Rn 0100nnnn00010101 Rn > 0, 1 → T
                                    Cmd = "cmp/pl";
                                    format = onevalformat;
                                    break;
                                case 22://LDS.L @Rm+,MACL 0100mmmm00010110 (Rm) → MACL, Rm + 4 → Rm
                                    Cmd = "lds.l";
                                    Rm = _nibs[1] | PostIncReg;
                                    Rn = RegMacl;
                                    format = twovalmemorysrcformat;
                                    break;
                                case 23://LDC.L @Rm+,GBR 0100mmmm00010111 (Rm) → GBR, Rm + 4 → Rm
                                    Cmd = "ldc.l";
                                    Rm |= _nibs[1] | PostIncReg;
                                    Rn = RegGbr;
                                    format = twovalmemorysrcformat;
                                    break;
                                case 24://SHLL8 Rn 0100nnnn00011000 Rn<<8 → Rn
                                    Cmd = "shll8";
                                    format = onevalformat;
                                    break;
                                case 25://SHLR8 Rn 0100nnnn00011001 Rn>>8 → Rn
                                    Cmd = "shlr8";
                                    format = onevalformat;
                                    break;
                                case 26://LDS Rm,MACL 0100mmmm00011010 Rm → MACL
                                    Cmd = "lds";
                                    Rm = _nibs[1];
                                    Rn = RegMacl;
                                    format = twovalformat;
                                    break;
                                case 27://TAS.B @Rn 0100nnnn00011011 When (Rn) is 0, 1 → T, 1 → MSB of (Rn)
                                    Cmd = "tas.b";
                                    format = onevalmemoryformat;
                                    break;
                                case 30://LDC Rm,GBR 0100mmmm00011110 Rm → GBR
                                    Cmd = "ldc";
                                    Rm = _nibs[1];
                                    Rn = RegGbr;
                                    format = twovalformat;
                                    break;
                                case 32:////SHAL Rn 0100nnnn00100000 T ← Rn ← 0
                                    Cmd = "shal";
                                    format = onevalformat;
                                    break;
                                case 33:////SHAR Rn 0100nnnn00100001 MSB → Rn → T
                                    Cmd = "shar";
                                    format = onevalformat;
                                    break;
                                case 34://STS.L PR,@–Rn 0100nnnn00100010 Rn – 4 → Rn, PR → (Rn)
                                    Cmd = "sts.l";
                                    Rn |= PreDecReg;
                                    Rm = RegPr;
                                    format = twovalmemorydestformat;
                                    break;
                                case 35://STC.L VBR,@-Rn 0100nnnn00100011 Rn – 4 → Rn, VBR → (Rn)
                                    Cmd = "stc.l";
                                    Rn |= PreDecReg;
                                    Rm = RegGbr;
                                    format = twovalmemorydestformat;
                                    break;
                                case 36://ROTCL Rn 0100nnnn00100100 T ← Rn ← T
                                    Cmd = "rotcl";
                                    format = onevalformat;
                                    break;
                                case 37://ROTCR Rn 0100nnnn00100101 T → Rn → T
                                    Cmd = "rotcr";
                                    format = onevalformat;
                                    break;
                                case 38://LDS.L @Rm+,PR 0100mmmm00100110 (Rm) → PR, Rm + 4 → Rm
                                    Cmd = "lds.l";
                                    Rm = _nibs[1] | PostIncReg;
                                    Rn = RegPr;
                                    format = twovalmemorysrcformat;
                                    break;
                                case 39://LDC.L @Rm+,VBR 0100mmmm00100111 (Rm) → VBR, Rm + 4 → Rm
                                    Cmd = "ldc.l";
                                    Rm = _nibs[1] | PostIncReg;
                                    Rn = RegVbr;
                                    format = twovalmemorysrcformat;
                                    break;
                                case 40://SHLL16 Rn 0100nnnn00101000 Rn<<16 → Rn
                                    Cmd = "shll16";
                                    format = onevalformat;
                                    break;
                                case 41://SHLR16 Rn 0100nnnn00101001 Rn>>16 → Rn
                                    Cmd = "1hlr16";
                                    format = onevalformat;
                                    break;
                                case 42://LDS Rm,PR 0100mmmm00101010 Rm → PR
                                    Cmd = "lds";
                                    Rm = _nibs[1];
                                    Rn = RegPr;
                                    format = twovalformat;
                                    break;
                                case 43://JMP @Rm 0100mmmm00101011 Delayed branch, Rm → PC
                                    Cmd = "jmp";
                                    Rm = _nibs[1];
                                    Rn = Unused;// should we have rn be pc in these cases?
                                    format = onevalmemoryformat;
                                    break;
                                case 46://LDC Rm,VBR 0100mmmm00101110 Rm → VBR
                                    Cmd = "ldc";
                                    Rm = _nibs[1];
                                    Rn = RegVbr;
                                    format = twovalformat;
                                    break;

                                default:
                                    if ((Funct & 0xf) == 0xf)//MAC.W @Rm+,@Rn+ 0100nnnnmmmm1111 Signed, (Rn) × (Rm) + MAC → MAC
                                    {
                                        Cmd = "mac.w";
                                        Rn = _nibs[1] | PostIncReg;
                                        Rm = _nibs[2] | PostIncReg;
                                        format = twovalmemoryformat;
                                    }
                                    else
                                    {
                                        Cmd = "?";
                                        format = unknownformat;
                                    }
                                    break;
                            }
                            break;
                        case 5://MOV.L @(disp,Rm),Rn 0101nnnnmmmmdddd (disp + Rm) → Rn
                            Cmd = "mov.l";
                            Rn = _nibs[1];
                            Rm = _nibs[2];
                            Disp = _nibs[3]*4;
                            format = threevalmemorysrcformat;
                            break;
                        case 6:
                            Funct = _nibs[3];
                            Rn = _nibs[1];
                            Rm = _nibs[2];
                            switch(Funct)
                            {
                                case 0x0://MOV.B @Rm,Rn 0110nnnnmmmm0000 (Rm) → sign extension → Rn
                                    Cmd = "mov.b";
                                    format = twovalmemorysrcformat;
                                    break;
                                case 0x1://MOV.W @Rm,Rn 0110nnnnmmmm0001 (Rm) → sign extension → Rn
                                    Cmd = "mov.w";
                                    format = twovalmemorysrcformat;
                                    break;
                                case 0x2://MOV.L @Rm,Rn 0110nnnnmmmm0010 (Rm) → Rn
                                    Cmd = "mov.l";
                                    format = twovalmemorysrcformat;
                                    break;
                                case 0x3://MOV Rm,Rn 0110nnnnmmmm0011 Rm → Rn
                                    Cmd = "mov";
                                    format = twovalformat;
                                    break;
                                case 0x4://MOV.B @Rm+,Rn 0110nnnnmmmm0100 (Rm) → sign extension → Rn, Rm + 1 → Rm
                                    Cmd = "mov.b";
                                    Rm |= PostIncReg;
                                    format = twovalmemorysrcformat;
                                    break;
                                case 0x5://MOV.W @Rm+,Rn 0110nnnnmmmm0101 (Rm) → sign extension → Rn, Rm + 2 → Rm
                                    Cmd = "mov.w";
                                    Rm |= PostIncReg;
                                    format = twovalmemorysrcformat;
                                    break;
                                case 0x6://MOV.L @Rm+,Rn 0110nnnnmmmm0110 (Rm) → Rn, Rm + 4 → Rm
                                    Cmd = "mov.l";
                                    Rm |= PostIncReg;
                                    format = twovalmemorysrcformat;
                                    break;
                                case 0x7://NOT Rm,Rn 0110nnnnmmmm0111 ~Rm → Rn
                                    Cmd = "not";
                                    format = twovalformat;
                                    break;
                                case 0x8://SWAP.B Rm,Rn 0110nnnnmmmm1000 Rm → Swap upper and lower halves of lower 2 bytes → Rn
                                    Cmd = "swap.b";
                                    format = twovalformat;
                                    break;
                                case 0x9://SWAP.W Rm,Rn 0110nnnnmmmm1001 Rm → Swap upper and lower word → Rn
                                    Cmd = "swap.w";
                                    format = twovalformat;
                                    break;
                                case 0xa://NEGC Rm,Rn 0110nnnnmmmm1010 0 – Rm – T → Rn, borrow → T
                                    Cmd = "negc";
                                    format = twovalformat;
                                    break;
                                case 0xb://NEG Rm,Rn 0110nnnnmmmm1011 0 – Rm → Rn
                                    Cmd = "neg";
                                    format = twovalformat;
                                    break;
                                case 0xc://EXTU.B Rm,Rn 0110nnnnmmmm1100 Zero-extends Rm from byte → Rn
                                    Cmd = "extu.b";
                                    format = twovalformat;
                                    break;
                                case 0xd://EXTU.W Rm,Rn 0110nnnnmmmm1101 Zero-extends Rm from word → Rn
                                    Cmd = "extu.w";
                                    format = twovalformat;
                                    break;
                                case 0xe://EXTS.B Rm,Rn 0110nnnnmmmm1110 Sign-extends Rm from byte → Rn
                                    Cmd = "exts.b";
                                    format = twovalformat;
                                    break;
                                case 0xf://EXTS.W Rm,Rn 0110nnnnmmmm1111 Sign-extends Rm from word → Rn
                                    Cmd = "exts.w";
                                    format = twovalformat;
                                    break;
                                default:
                                    Cmd = "?";
                                    format = unknownformat;
                                    break;
                            }
                            break;
                        case 7://ADD #imm,Rn 0111nnnniiiiiiii Rn + imm → Rn
                            Cmd = "add";
                            Rn = _nibs[1];
                            Immediate = SignedValAtOffset(instruction, 8, 0);
                            format = twovalformat;
                            break;
                        case 8:
                            Funct = _nibs[1];
                            switch(Funct)
                            {
                                case 0://MOV.B R0,@(disp,Rn) 10000000nnnndddd R0 → (disp + Rn)
                                    Cmd = "mov.b";
                                    Rm = RegR0;
                                    Rn = _nibs[2];
                                    Disp = _nibs[3];
                                    format = threevalmemorydestformat;
                                    break;
                                case 1://MOV.W R0,@(disp,Rn) 10000001nnnndddd R0 → (disp × 2 + Rn)
                                    Cmd = "mov.w";
                                    Rm = RegR0;
                                    Rn = _nibs[2];
                                    Disp = _nibs[3] * 2;
                                    format = threevalmemorydestformat;
                                    break;
                                case 4://MOV.B @(disp,Rm),R0 10000100mmmmdddd (disp + Rm) → sign extension → R0
                                    Cmd = "mov.b";
                                    Rn = RegR0;
                                    Rm = _nibs[2];
                                    Disp = _nibs[3];
                                    format = threevalmemorysrcformat;
                                    break;
                                case 5://MOV.W @(disp,Rm),R0 10000101mmmmdddd (disp × 2 + Rm) → sign extension → R0
                                    Cmd = "mov.w";
                                    Rn = RegR0;
                                    Rm = _nibs[2];
                                    Disp = _nibs[3] * 2;
                                    format = threevalmemorysrcformat;
                                    break;
                                case 8://CMP/EQ #imm,R0 10001000iiiiiiii When R0 = imm, 1 → T
                                    Cmd = "cmp/eq";
                                    Immediate = SignedValAtOffset(instruction, 8, 0);
                                    Rn = RegR0;
                                    format = twovalformat;
                                    break;
                                case 9://BT label 10001001dddddddd When T = 1, disp × 2 + PC → PC; When T = 0, nop.
                                    Cmd = "bt";//branch if true
                                    ReferencedAddress = (uint)(SignedValAtOffset(instruction, 8, 0) * 2 + address + 4);
                                    format = onevalformat;
                                    break;
                                case 13://BT/S label* 10001101dddddddd When T = 1, disp × 2 + PC → PC; When T = 0, nop.

                                    Cmd = "bt/s";// branch if true with delay slot
                                    ReferencedAddress = (uint)(SignedValAtOffset(instruction, 8, 0) * 2 + address + 4);
                                    format = onevalformat;
                                    break;
                                case 11://BF label 10001011dddddddd When T = 0, disp × 2 + PC → PC; When T = 1, nop
                                    Cmd = "bf";//branch if false
                                    ReferencedAddress = (uint)(SignedValAtOffset(instruction, 8, 0) * 2 + address + 4);
                                    format = onevalformat;
                                    break;
                                case 15://BF/S label* 10001111dddddddd When T = 0, disp × 2 + PC → PC; When T = 1, nop
                                    Cmd = "bf/s";//branch if false with delay slot
                                    ReferencedAddress = (uint)(SignedValAtOffset(instruction, 8, 0) * 2 + address + 4);
                                    format = onevalformat;
                                    break;
                                default:
                                    Cmd = "?";
                                    format = unknownformat;
                                    break;
                            }
                            break;
                        case 9://MOV.W @(disp,PC),Rn 1001nnnndddddddd (disp × 2 + PC) → sign extension → Rn
                            Cmd = "mov.w";
                            Rn = _nibs[1];
                            //disp = ValAtOffset(instruction, 8, 0)*2;
                            ReferencedAddress = (uint)(ValAtOffset(instruction, 8, 0) * 2 + address);
                            format = twovalmemorysrcformat;
                            break;
                        case 10://BRA label 1010dddddddddddd Delayed branch, disp × 2 + PC → PC
                            Cmd = "bra";//branch
                            ReferencedAddress = (uint)(SignedValAtOffset(instruction, 12, 0) * 2 + address + 4);
                            format = onevalformat;
                            break;
                        case 11://BSR label 1011dddddddddddd Delayed branch, PC → PR, disp × 2 + PC → PC
                            Cmd = "bsr";//branch to subroutine
                            ReferencedAddress = (uint)(SignedValAtOffset(instruction, 12, 0) * 2 + address + 4);
                            format = onevalformat;
                            break;
                        case 12:
                            Funct = _nibs[1];
                            switch (Funct)
                            {
                                case 0x0://MOV.B R0,@(disp,GBR) 11000000dddddddd R0 → (disp + GBR)
                                    Cmd = "mov.b";
                                    Rm = RegR0;
                                    Rn = RegGbr;
                                    Disp = ValAtOffset(instruction, 8, 0);
                                    format = threevalmemorydestformat;
                                    break;
                                case 0x1://MOV.W R0,@(disp,GBR) 11000001dddddddd R0 → (disp × 2 + GBR)
                                    Cmd = "mov.w";
                                    Rm = RegR0;
                                    Rn = RegGbr;
                                    Disp = ValAtOffset(instruction, 8, 0)*2;
                                    format = threevalmemorydestformat;
                                    break;
                                case 0x2://MOV.L R0,@(disp,GBR) 11000010dddddddd R0 → (disp × 4 + GBR)
                                    Cmd = "mov.l";
                                    Rm = RegR0;
                                    Rn = RegGbr;
                                    Disp = ValAtOffset(instruction, 8, 0)*4;
                                    format = threevalmemorydestformat;
                                    break;
                                case 0x3://TRAPA #imm 11000011iiiiiiii PC/SR → Stack area, (imm × 4 + VBR) → PC
                                    Cmd = "trapa";
                                    Immediate = ValAtOffset(instruction, 8, 0)*4;
                                    format = onevalformat;
                                    break;
                                case 0x4://MOV.B @(disp,GBR),R0 11000100dddddddd (disp + GBR) → sign extension → R0
                                    Cmd = "mov.b";
                                    Rn = RegR0;
                                    Rm = RegGbr;
                                    Disp = ValAtOffset(instruction, 8, 0);
                                    format = threevalmemorysrcformat;
                                    break;
                                case 0x5://MOV.W @(disp,GBR),R0 11000101dddddddd (disp × 2 + GBR) → sign extension → R0
                                    Cmd = "mov.w";
                                    Rn = RegR0;
                                    Rm = RegGbr;
                                    Disp = ValAtOffset(instruction, 8, 0)*2;
                                    format = threevalmemorysrcformat;
                                    break;
                                case 0x6://MOV.L @(disp,GBR),R0 11000110dddddddd (disp × 4 + GBR) → R0
                                    Cmd = "mov.l";
                                    Rn = RegR0;
                                    Rm = RegGbr;
                                    Disp = ValAtOffset(instruction, 8, 0)*4;
                                    format = threevalmemorysrcformat;
                                    break;
                                case 0x7://MOVA @(disp,PC),R0 11000111dddddddd disp × 4 + PC → R0
                                    Cmd = "mova";
                                    Rn = RegR0;
                                    //disp = ValAtOffset(instruction, 8, 0)*4;
                                    ReferencedAddress = (uint)(ValAtOffset(instruction, 8, 0) * 4 + address);
                                    format = twovalmemorysrcformat;
                                    break;
                                case 0x8://TST #imm,R0 11001000iiiiiiii R0 & imm, when result is 0, 1 → T
                                    Cmd = "tst";
                                    Rn = RegR0;
                                    Immediate = ValAtOffset(instruction, 8, 0);
                                    format = twovalformat;
                                    break;
                                case 0x9://AND #imm,R0 11001001iiiiiiii R0 & imm → R0
                                    Cmd = "and";
                                    Rn = RegR0;
                                    Immediate = ValAtOffset(instruction, 8, 0);
                                    format = twovalformat;
                                    break;
                                case 0xa://XOR #imm,R0 11001010iiiiiiii R0 ^ imm → R0
                                    Cmd = "xor";
                                    Rn = RegR0;
                                    Immediate = ValAtOffset(instruction, 8, 0);
                                    format = twovalformat;
                                    break;
                                case 0xb://OR #imm,R0 11001011iiiiiiii R0 | imm → R0
                                    Cmd = "or";
                                    Rn = RegR0;
                                    Immediate = ValAtOffset(instruction, 8, 0);
                                    format = twovalformat;
                                    break;
                                case 0xc://TST.B #imm,@(R0,GBR) 11001100iiiiiiii (R0 + GBR) & imm, when result is 0, 1 → T
                                    Cmd = "tst.b";
                                    Rm = RegR0;
                                    Rn = RegGbr;
                                    Immediate = ValAtOffset(instruction, 8, 0);
                                    format = threevalmemorydestformat;
                                    break;
                                case 0xd://AND.B #imm,@(R0,GBR) 11001101iiiiiiii (R0 + GBR) & imm → (R0 + GBR)
                                    Cmd = "and.b";
                                    Rm = RegR0;
                                    Rn = RegGbr;
                                    Immediate = ValAtOffset(instruction, 8, 0);
                                    format = threevalmemorydestformat;
                                    break;
                                case 0xe://XOR.B #imm,@(R0,GBR) 11001110iiiiiiii (R0 + GBR) ^ imm → (R0 + GBR)
                                    Cmd = "xor.b";
                                    Rm = RegR0;
                                    Rn = RegGbr;
                                    Immediate = ValAtOffset(instruction, 8, 0);
                                    format = threevalmemorydestformat;
                                    break;
                                case 0xf://OR.B #imm,@(R0,GBR) 11001111iiiiiiii (R0 + GBR) | imm → (R0 + GBR)
                                    Cmd = "or.b";
                                    Rm = RegR0;
                                    Rn = RegGbr;
                                    Immediate = ValAtOffset(instruction, 8, 0);
                                    format = threevalmemorydestformat;
                                    break;
                            }
                            break;
                        case 13://MOV.L @(disp,PC),Rn 1101nnnndddddddd (disp × 4 + PC) → Rn
                            Cmd = "mov.l";
                            Rn = _nibs[1];
                            ReferencedAddress = (uint)(ValAtOffset(instruction, 8, 0) * 4 + address);
                            format = twovalmemorysrcformat;
                            break;
                        case 14://MOV #imm,Rn 1110nnnniiiiiiii imm → sign extension → Rn
                            Cmd = "mov";
                            Rn = _nibs[1];
                            Immediate = SignedValAtOffset(instruction, 8, 0);
                            format = twovalformat;
                            break;
                        default:
                            Cmd = "?";
                            format = unknownformat;
                            break;
                    }

                    if (format == novalformat)
                    {
                        Display = string.Format(format, Cmd);
                    }
                    else if (format == onevalformat)
                    {
                        var param = "?";
                        if (Rn != Unused)
                        {
                            param = GetRegister(Rn);
                        }
                        else if (Rm != Unused)
                        {
                            param = GetRegister(Rm);
                        }
                        else if (ReferencedAddress != Unused)
                        {
                            param = GetReferencedAddress(ReferencedAddress);
                        }
                        else if (Immediate != Unused)
                        {
                            param = GetImmediate(Immediate);
                        }

                        Display = string.Format(format, Cmd, param);
                    }
                    else if (format == onevalmemoryformat)
                    {
                        var param = "?";
                        if (Rn != Unused)
                        {
                            param = GetRegister(Rn);
                        }
                        else if (Rm != Unused)
                        {
                            param = GetRegister(Rm);
                        }

                        Display = string.Format(format, Cmd, param);
                    }
                    else if (format == twovalformat)
                    {
                        var param2 = GetRegister(Rn);
                        var param1 = "?";
                        if (Rm != Unused)
                        {
                            param1 = GetRegister(Rm);
                        }
                        else if (Immediate != Unused)
                        {
                            param1 = GetImmediate(Immediate);
                        }

                        Display = string.Format(format, param1, param2);
                    }
                    else if (format == twovalmemoryformat)
                    {
                        var param2 = GetRegister(Rn);
                        var param1 = GetRegister(Rm);
                        Display = string.Format(format, param1, param2);
                    }
                    else if (format == twovalmemorydestformat)
                    {
                        var param2 = GetRegister(Rn);
                        var param1 = GetRegister(Rm);
                        Display = string.Format(format, param1, param2);
                    }
                    else if (format == twovalmemorysrcformat)
                    {
                        var param2 = GetRegister(Rn);
                        var param1 = "?";
                        if (Rm != Unused)
                        {
                            param1 = GetRegister(Rm);
                        }
                        else if (ReferencedAddress != Unused)
                        {
                            param1 = GetReferencedAddress(ReferencedAddress);
                        }

                        Display = string.Format(format, param1, param2);
                    }
                }

            }

            public override bool IsBranch
            {
                get 
                { 
                    System.Diagnostics.Debugger.Break();
                    throw new NotImplementedException(); }
            }

            public override bool IsCall
            {
                get 
                { 
                    System.Diagnostics.Debugger.Break();
                    throw new NotImplementedException();
                }
            }

            public override bool IsJump 
            {
                get 
                { 
                    System.Diagnostics.Debugger.Break();
                    throw new NotImplementedException();
                }
            }
            public override bool IsReturn 
            {
                get 
                { 
                    System.Diagnostics.Debugger.Break();
                    throw new NotImplementedException();
                }
            }

            public override bool IsAssignment 
            {
                get 
                { 
                    System.Diagnostics.Debugger.Break();
                    throw new NotImplementedException();
                }
            }

            public override uint GetGlobalVariable(CodeBlock<IsInstruction> block)
            {
                System.Diagnostics.Debugger.Break();
                throw new NotImplementedException();
            }

            public override void GetAssignmentGlobals(out uint left, out string right, CodeBlock<IsInstruction> block)
            {
                System.Diagnostics.Debugger.Break();
                throw new NotImplementedException();
            }
        }
    }
}
