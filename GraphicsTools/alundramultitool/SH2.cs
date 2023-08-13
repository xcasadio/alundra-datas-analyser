using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace alundramultitool
{
    public class SH2: InstructionSet
    {
        const int unused = 0x80001;

        //reg flags
        const int predef_reg = 0x80;
        const int pre_dec_reg = 0x800; //@-REG
        const int post_inc_reg = 0x8000;//@REG+



        const int reg_r0 = predef_reg + 0;
        const int reg_pr = predef_reg + 1;
        const int reg_sr = predef_reg + 2;
        const int reg_mach = predef_reg + 3;
        const int reg_macl = predef_reg + 4;
        const int reg_gbr = predef_reg + 5;
        const int reg_vbr = predef_reg + 6;

        static string GetRegister(int reg)
        {
            if (reg == unused)
                return "?";

            int numpart = (reg & 0x3f);
            string sr = "?";
            if ((reg & predef_reg) == predef_reg)
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

            if ((reg & pre_dec_reg) == pre_dec_reg)
                sr = "-" + sr;

            if ((reg & post_inc_reg) == post_inc_reg)
                sr = sr + "+";

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

        public class Instruction : ISInstruction
        {
            


            int[] nibs = new int[4];
            public Instruction(uint address, uint instruction)
            {
                this.address = address;
                this.instruction = instruction;


                nibs[0] = ValAtOffset(instruction, 0xf, 12);
                nibs[1] = ValAtOffset(instruction, 0xf, 8);
                nibs[2] = ValAtOffset(instruction, 0xf, 4);
                nibs[3] = ValAtOffset(instruction, 0xf, 0);

                opcode = nibs[0];

                string novalformat = "{0}";
                string onevalformat = "{0} {1}";
                string onevalmemoryformat = "{0} @{1}";

                string twovalformat = "{0} {1},{2}";

                string twovalmemoryformat = "{0} @{1},@{2}";
                string twovalmemorydestformat = "{0} {1},@{2}";
                string twovalmemorysrcformat = "{0} @{1},{2}";

                string threevalmemorydestformat = "{0} {1},@({2},{3})";
                string threevalmemorysrcformat = "{0} @({1},{3}),{2}";

                //string threevalmemorydestformatalt = "{0} {1},{3}({2})";
                //string threevalmemorysrcformatalt = "{0} {3}({1}),{2}";


                string unknownformat = "{0} uknown opcode";

                rn = unused;//destination reg
                rm = unused;//source reg
                rd = unused;//displacement reg
                disp = unused;//displacement immediate
                immediate = unused;
                immediateu = unused;
                referencedAddress = unused;

                string format = unknownformat;

                if (instruction == 0)
                {
                    cmd = "nop";
                    format = novalformat;
                }
                else
                {
                    switch (opcode)
                    {
                        case 0:
                            funct = ValAtOffset(instruction, 0x3f, 0);
                            switch (funct)
                            {
                                case 0x8://CLRT 0000000000001000 0 → T 1
                                    cmd = "clrt";
                                    format = novalformat;
                                    break;
                                case 0x9://NOP 0000000000001001 No operation 
                                    cmd = "nop";
                                    format = novalformat;
                                    break;
                                case 0xb://RTS 0000000000001011 Delayed branch, PR → PC
                                    cmd = "rts";
                                    format = novalformat;
                                    break;
                                case 0x18://SETT 0000000000011000 1 → T
                                    cmd = "sett";
                                    format = novalformat;
                                    break;
                                case 0x19://DIV0U 0000000000011001 0 → M/Q/T
                                    cmd = "div0u";
                                    format = novalformat;
                                    break;
                                case 0x1b://SLEEP 0000000000011011 Sleep
                                    cmd = "sleep";
                                    format = novalformat;
                                    break;
                                case 0x28://CLRMAC 0000000000101000 0 → MACH, MACL 
                                    cmd = "clrmac";
                                    format = novalformat;
                                    break;
                                case 0x2b://RTE 0000000000101011 Delayed branch, stack area → PC / SR
                                    cmd = "rte";//return
                                    format = novalformat;
                                    break;
                                case 0x2://STC SR,Rn 0000nnnn00000010 SR → Rn
                                    cmd = "stc";//store control reg
                                    rm = reg_sr;
                                    rn = nibs[1];
                                    format = twovalformat;
                                    break;
                                case 0x3://BSRF Rm 0000mmmm00000011 Delayed branch, PC → PR, Rm + PC → PC
                                    cmd = "bsrf";//branch sub routine far
                                    rm = nibs[1];
                                    format = onevalformat;
                                    break;
                                case 0xa://STS MACH,Rn 0000nnnn00001010 MACH → Rn
                                    cmd = "sts";//store system reg
                                    rm = reg_mach;
                                    rn = nibs[1];
                                    format = twovalformat;
                                    break;
                                case 0x12://STC GBR,Rn 0000nnnn00010010 GBR → Rn
                                    cmd = "stc";//store control reg
                                    rm = reg_gbr;
                                    rn = nibs[1];
                                    format = twovalformat;
                                    break;
                                case 0x1a://STS MACL,Rn 0000nnnn00011010 MACL → Rn
                                    cmd = "sts";//store system reg
                                    rm = reg_macl;
                                    rn = nibs[1];
                                    format = twovalformat;
                                    break;
                                case 0x22://STC VBR,Rn 0000nnnn00100010 VBR → Rn
                                    cmd = "stc";//store control reg
                                    rm = reg_vbr;
                                    rn = nibs[1];
                                    format = twovalformat;
                                    break;
                                case 0x23://BRAF Rm 0000mmmm00100011 Delayed branch, Rm + PC → PC
                                    cmd = "braf";//branch far
                                    rm = nibs[1];
                                    format = onevalformat;
                                    break;
                                case 0x29://MOVT Rn 0000nnnn00101001 T → Rn
                                    cmd = "movt";//move t bit
                                    rn = nibs[1];
                                    format = onevalformat;
                                    break;
                                case 0x2a://STS PR,Rn 0000nnnn00101010 PR → Rn 1
                                    cmd = "sts";//store system reg
                                    rm = reg_pr;
                                    rn = nibs[1];
                                    format = twovalformat;
                                    break;

                                case 0x4://MOV.B Rm,@(R0,Rn) 0000nnnnmmmm0100 Rm → (R0 + Rn)
                                    cmd = "mov.b";
                                    rn = nibs[1];
                                    rm = nibs[2];
                                    rd = reg_r0;
                                    format = threevalmemorydestformat;
                                    break;
                                case 0x5://MOV.W Rm,@(R0,Rn) 0000nnnnmmmm0101 Rm → (R0 + Rn)
                                    cmd = "mov.w";
                                    rn = nibs[1];
                                    rm = nibs[2];
                                    rd = reg_r0;
                                    format = threevalmemorydestformat;
                                    break;
                                case 0x6://MOV.L Rm,@(R0,Rn) 0000nnnnmmmm0110 Rm → (R0 + Rn)
                                    cmd = "mov.l";
                                    rn = nibs[1];
                                    rm = nibs[2];
                                    rd = reg_r0;
                                    format = threevalmemorydestformat;
                                    break;
                                case 0x7://MUL.L Rm,Rn 0000nnnnmmmm0111 Rn x Rm → MACL
                                    cmd = "mul.l";
                                    rn = nibs[1];
                                    rm = nibs[2];
                                    //rd = reg_macl;
                                    format = twovalformat;
                                    break;

                                case 0xc://MOV.B @(R0,Rm),Rn 0000nnnnmmmm1100 (R0 + Rm) → sign extension → Rn
                                    cmd = "mov.b";
                                    rn = nibs[1];
                                    rm = nibs[2];
                                    rd = reg_r0;
                                    format = threevalmemorysrcformat;
                                    break;
                                case 0xd://MOV.W @(R0,Rm),Rn 0000nnnnmmmm1101 (R0 + Rm) → sign extension → Rn
                                    cmd = "mov.w";
                                    rn = nibs[1];
                                    rm = nibs[2];
                                    rd = reg_r0;
                                    format = threevalmemorysrcformat;
                                    break;
                                case 0xe://MOV.L @(R0,Rm),Rn 0000nnnnmmmm1110 (R0 + Rm) → Rn
                                    cmd = "mov.l";
                                    rn = nibs[1];
                                    rm = nibs[2];
                                    rd = reg_r0;
                                    format = threevalmemorysrcformat;
                                    break;
                                case 0xf://MAC.L @Rm+,@Rn+ 0000nnnnmmmm1111 Signed, (Rn) x (Rm) + MAC → MAC
                                    cmd = "mac.l";
                                    rn = nibs[1] | post_inc_reg;
                                    rm = nibs[2] | post_inc_reg;
                                    format = twovalmemoryformat;
                                    break;
                                default:
                                    cmd = "?";
                                    format = unknownformat;
                                    break;
                            }
                            break;
                        case 1://MOV.L Rm,@(disp,Rn) 0001nnnnmmmmdddd Rm → (disp × 4 + Rn)
                            cmd = "mov.l";
                            rn = nibs[1];
                            rm = nibs[2];
                            disp = nibs[3];
                            format = threevalmemorydestformat;
                            break;
                        case 2:
                            funct = nibs[3];
                            switch (funct)
                            {
                                case 0x0://MOV.B Rm,@Rn 0010nnnnmmmm0000 Rm → (Rn)
                                    cmd = "mov.b";
                                    rn = nibs[1];
                                    rm = nibs[2];
                                    format = twovalmemorydestformat;
                                    break;
                                case 0x1://MOV.W Rm,@Rn 0010nnnnmmmm0001 Rm → (Rn)
                                    cmd = "mov.w";
                                    rn = nibs[1];
                                    rm = nibs[2];
                                    format = twovalmemorydestformat;
                                    break;
                                case 0x2://MOV.L Rm,@Rn 0010nnnnmmmm0010 Rm → (Rn)
                                    cmd = "mov.l";
                                    rn = nibs[1];
                                    rm = nibs[2];
                                    format = twovalmemorydestformat;
                                    break;

                                case 0x4://MOV.B Rm,@-Rn 0010nnnnmmmm0100 Rn – 1 → Rn, Rm → (Rn)
                                    cmd = "mov.b";
                                    rn = nibs[1] | pre_dec_reg;
                                    rm = nibs[2];
                                    format = twovalmemorydestformat;
                                    break;
                                case 0x5://MOV.W Rm,@–Rn 0010nnnnmmmm0101 Rn – 2 → Rn, Rm → (Rn)
                                    cmd = "mov.w";
                                    rn = nibs[1] | pre_dec_reg;
                                    rm = nibs[2];
                                    format = twovalmemorydestformat;
                                    break;
                                case 0x6://MOV.L Rm,@–Rn 0010nnnnmmmm0110 Rn – 4 → Rn, Rm → (Rn)
                                    cmd = "mov.l";
                                    rn = nibs[1] | pre_dec_reg;
                                    rm = nibs[2];
                                    format = twovalmemorydestformat;
                                    break;

                                case 0x7://DIV0S Rm,Rn 0010nnnnmmmm0111 MSB of Rn → Q, MSB of Rm → M, M ^ Q → T
                                    cmd = "div0s";
                                    rn = nibs[1];
                                    rm = nibs[2];
                                    format = twovalformat;
                                    break;
                                case 0x8://TST Rm,Rn 0010nnnnmmmm1000 Rn & Rm, when result is 0, 1 → T
                                    cmd = "tst";
                                    rn = nibs[1];
                                    rm = nibs[2];
                                    format = twovalformat;
                                    break;
                                case 0x9://AND Rm,Rn 0010nnnnmmmm1001 Rn & Rm → Rn
                                    cmd = "and";
                                    rn = nibs[1];
                                    rm = nibs[2];
                                    format = twovalformat;
                                    break;
                                case 0xa://XOR Rm,Rn 0010nnnnmmmm1010 Rn ^ Rm → Rn
                                    cmd = "xor";
                                    rn = nibs[1];
                                    rm = nibs[2];
                                    format = twovalformat;
                                    break;
                                case 0xb://OR Rm,Rn 0010nnnnmmmm1011 Rn | Rm → Rn
                                    cmd = "or";
                                    rn = nibs[1];
                                    rm = nibs[2];
                                    format = twovalformat;
                                    break;
                                case 0xc://CMP/STR Rm,Rn 0010nnnnmmmm1100 When a byte in Rn equals a byte in Rm, 1 → T
                                    cmd = "cmp/str";
                                    rn = nibs[1];
                                    rm = nibs[2];
                                    format = twovalformat;
                                    break;
                                case 0xd://XTRCT Rm,Rn 0010nnnnmmmm1101 Center 32 bits of Rm and Rn → Rn
                                    cmd = "xtrct";
                                    rn = nibs[1];
                                    rm = nibs[2];
                                    format = twovalformat;
                                    break;
                                case 0xe://MULU.W Rm,Rn 0010nnnnmmmm1110 Unsigned, Rn × Rm → MAC
                                    cmd = "mulu.w";
                                    rn = nibs[1];
                                    rm = nibs[2];
                                    format = twovalformat;
                                    break;
                                case 0xf://MULS.W Rm,Rn 0010nnnnmmmm1111 Signed, Rn × Rm → MAC
                                    cmd = "muls.w";
                                    rn = nibs[1];
                                    rm = nibs[2];
                                    format = twovalformat;
                                    break;
                                default:
                                    cmd = "?";
                                    format = unknownformat;
                                    break;
                            }
                            break;
                        case 3:
                            funct = nibs[3];
                            rn = nibs[1];
                            rm = nibs[2];
                            switch (funct)
                            {
                                case 0x0://CMP/EQ Rm,Rn 0011nnnnmmmm0000 When Rn = Rm, 1 → T 1
                                    cmd = "cmp/eq";
                                    format = twovalformat;
                                    break;

                                case 0x2://CMP/HS Rm,Rn 0011nnnnmmmm0010 When unsigned and Rn ≥ Rm, 1 → T
                                    cmd = "cmp/hs";
                                    format = twovalformat;
                                    break;
                                case 0x3://CMP/GE Rm,Rn 0011nnnnmmmm0011 When signed and Rn ≥ Rm, 1 → T
                                    cmd = "cmp/ge";
                                    format = twovalformat;
                                    break;
                                case 0x4://DIV1 Rm,Rn 0011nnnnmmmm0100 1-step division (Rn ÷ Rm)
                                    cmd = "div1";
                                    format = twovalformat;
                                    break;
                                case 0x5://DMULU.L Rm,Rn 0011nnnnmmmm0101 Unsigned, Rn x Rm → MACH, MACL
                                    cmd = "dmulu.l";
                                    format = twovalformat;
                                    break;
                                case 0x6://CMP/HI Rm,Rn 0011nnnnmmmm0110 When unsigned and Rn > Rm, 1 → T
                                    cmd = "cmp/hi";
                                    format = twovalformat;
                                    break;
                                case 0x7://CMP/GT Rm,Rn 0011nnnnmmmm0111 When signed and Rn > Rm, 1 → T
                                    cmd = "cmp/gt";
                                    format = twovalformat;
                                    break;
                                case 0x8://SUB Rm,Rn 0011nnnnmmmm1000 Rn – Rm → Rn
                                    cmd = "sub";
                                    format = twovalformat;
                                    break;

                                case 0xa://SUBC Rm,Rn 0011nnnnmmmm1010 Rn – Rm – T → Rn, borrow → T
                                    cmd = "subc";
                                    format = twovalformat;
                                    break;
                                case 0xb://SUBV Rm,Rn 0011nnnnmmmm1011 Rn – Rm → Rn, underflow → T
                                    cmd = "subv";
                                    format = twovalformat;
                                    break;
                                case 0xc://ADD Rm,Rn 0011nnnnmmmm1100 Rm + Rn → Rn
                                    cmd = "add";
                                    format = twovalformat;
                                    break;
                                case 0xd://DMULS.L Rm,Rn 0011nnnnmmmm1101 Signed, Rn x Rm → MACH, MACL
                                    cmd = "dmuls.l";
                                    format = twovalformat;
                                    break;
                                case 0xe://ADDC Rm,Rn 0011nnnnmmmm1110 Rn + Rm + T → Rn, carry → T
                                    cmd = "addc";
                                    format = twovalformat;
                                    break;
                                case 0xf://ADDV Rm,Rn 0011nnnnmmmm1111 Rn + Rm → Rn, overflow → T
                                    cmd = "addv";
                                    format = twovalformat;
                                    break;

                                default:
                                    cmd = "?";
                                    format = unknownformat;
                                    break;
                            }
                            break;
                        case 4:
                            funct = ValAtOffset(instruction, 0x3f, 0);
                            rn = nibs[1];
                            switch(funct)
                            {
                                case 0x0://SHLL Rn 0100nnnn00000000 T ← Rn ← 0
                                    cmd = "shll";
                                    format = onevalformat;
                                    break;
                                case 0x1://SHLR Rn 0100nnnn00000001 0 → Rn → T 
                                    cmd = "shlr";
                                    format = onevalformat;
                                    break;
                                case 0x2://STS.L MACH,@–Rn 0100nnnn00000010 Rn – 4 → Rn, MACH → (Rn)
                                    cmd = "sts.l";
                                    rn |= pre_dec_reg;
                                    rm = reg_mach;
                                    format = twovalmemorydestformat;
                                    break;
                                case 0x3://STC.L SR,@-Rn 0100nnnn00000011 Rn – 4 → Rn, SR → (Rn)
                                    cmd = "stc.l";
                                    rn |= pre_dec_reg;
                                    rm = reg_sr;
                                    format = twovalmemorydestformat;
                                    break;
                                case 0x4://ROTL Rn 0100nnnn00000100 T ← Rn ← MSB
                                    cmd = "rotl";
                                    format = onevalformat;
                                    break;
                                case 0x5://ROTR Rn 0100nnnn00000101 LSB → Rn → T
                                    cmd = "rotr";
                                    format = onevalformat;
                                    break;
                                case 0x6://LDS.L @Rm+,MACH 0100mmmm00000110 (Rm) → MACH, Rm + 4 → Rm
                                    cmd = "lds.l";
                                    rm = nibs[1] | post_inc_reg;
                                    rn = reg_mach; 
                                    format = twovalmemorysrcformat;
                                    break;
                                case 0x7://LDC.L @Rm+,SR 0100mmmm00000111 (Rm) → SR, Rm + 4 → Rm
                                    cmd = "ldc.l";
                                    rm = nibs[1] | post_inc_reg;
                                    rn = reg_sr;
                                    format = twovalmemorysrcformat;
                                    break;
                                case 0x8://SHLL2 Rn 0100nnnn00001000 Rn<<2 → Rn
                                    cmd = "shll2";
                                    format = onevalformat;
                                    break;
                                case 0x9://SHLR2 Rn 0100nnnn00001001 Rn>>2 → Rn
                                    cmd = "shlr2";
                                    format = onevalformat;
                                    break;
                                case 10://LDS Rm,MACH 0100mmmm00001010 Rm → MACH
                                    cmd = "lds";
                                    rm = nibs[1];
                                    rn = reg_mach;
                                    format = twovalformat;
                                    break;
                                case 11://JSR @Rm 0100mmmm00001011 Delayed branch, PC → PR, Rm → PC
                                    cmd = "jsr";
                                    rm = nibs[1];
                                    rn = unused;
                                    format = onevalmemoryformat;
                                    break;
                                case 14://LDC Rm,SR 0100mmmm00001110 Rm → SR
                                    cmd = "ldc";
                                    rm = nibs[1];
                                    rn = reg_sr;
                                    format = twovalformat;
                                    break;
                                case 16://DT Rn 0100nnnn00010000 Rn - 1 → Rn; if Rn is 0, 1 → T, if Rn is nonzero, 0 → T
                                    cmd = "dt";
                                    format = onevalformat;
                                    break;
                                case 17://CMP/PZ Rn 0100nnnn00010001 Rn ≥ 0, 1 → T
                                    cmd = "cmp/pz";
                                    format = onevalformat;
                                    break;
                                case 18://STS.L MACL,@–Rn 0100nnnn00010010 Rn – 4 → Rn, MACL → (Rn)
                                    cmd = "sts.l";
                                    rn |= pre_dec_reg;
                                    rm = reg_macl;
                                    format = twovalmemorydestformat;
                                    break;
                                case 19://STC.L GBR,@-Rn 0100nnnn00010011 Rn – 4 → Rn, GBR → (Rn)
                                    cmd = "stc.l";
                                    rn |= pre_dec_reg;
                                    rm = reg_gbr;
                                    format = twovalmemorydestformat;
                                    break;
                                case 21://CMP/PL Rn 0100nnnn00010101 Rn > 0, 1 → T
                                    cmd = "cmp/pl";
                                    format = onevalformat;
                                    break;
                                case 22://LDS.L @Rm+,MACL 0100mmmm00010110 (Rm) → MACL, Rm + 4 → Rm
                                    cmd = "lds.l";
                                    rm = nibs[1] | post_inc_reg;
                                    rn = reg_macl;
                                    format = twovalmemorysrcformat;
                                    break;
                                case 23://LDC.L @Rm+,GBR 0100mmmm00010111 (Rm) → GBR, Rm + 4 → Rm
                                    cmd = "ldc.l";
                                    rm |= nibs[1] | post_inc_reg;
                                    rn = reg_gbr;
                                    format = twovalmemorysrcformat;
                                    break;
                                case 24://SHLL8 Rn 0100nnnn00011000 Rn<<8 → Rn
                                    cmd = "shll8";
                                    format = onevalformat;
                                    break;
                                case 25://SHLR8 Rn 0100nnnn00011001 Rn>>8 → Rn
                                    cmd = "shlr8";
                                    format = onevalformat;
                                    break;
                                case 26://LDS Rm,MACL 0100mmmm00011010 Rm → MACL
                                    cmd = "lds";
                                    rm = nibs[1];
                                    rn = reg_macl;
                                    format = twovalformat;
                                    break;
                                case 27://TAS.B @Rn 0100nnnn00011011 When (Rn) is 0, 1 → T, 1 → MSB of (Rn)
                                    cmd = "tas.b";
                                    format = onevalmemoryformat;
                                    break;
                                case 30://LDC Rm,GBR 0100mmmm00011110 Rm → GBR
                                    cmd = "ldc";
                                    rm = nibs[1];
                                    rn = reg_gbr;
                                    format = twovalformat;
                                    break;
                                case 32:////SHAL Rn 0100nnnn00100000 T ← Rn ← 0
                                    cmd = "shal";
                                    format = onevalformat;
                                    break;
                                case 33:////SHAR Rn 0100nnnn00100001 MSB → Rn → T
                                    cmd = "shar";
                                    format = onevalformat;
                                    break;
                                case 34://STS.L PR,@–Rn 0100nnnn00100010 Rn – 4 → Rn, PR → (Rn)
                                    cmd = "sts.l";
                                    rn |= pre_dec_reg;
                                    rm = reg_pr;
                                    format = twovalmemorydestformat;
                                    break;
                                case 35://STC.L VBR,@-Rn 0100nnnn00100011 Rn – 4 → Rn, VBR → (Rn)
                                    cmd = "stc.l";
                                    rn |= pre_dec_reg;
                                    rm = reg_gbr;
                                    format = twovalmemorydestformat;
                                    break;
                                case 36://ROTCL Rn 0100nnnn00100100 T ← Rn ← T
                                    cmd = "rotcl";
                                    format = onevalformat;
                                    break;
                                case 37://ROTCR Rn 0100nnnn00100101 T → Rn → T
                                    cmd = "rotcr";
                                    format = onevalformat;
                                    break;
                                case 38://LDS.L @Rm+,PR 0100mmmm00100110 (Rm) → PR, Rm + 4 → Rm
                                    cmd = "lds.l";
                                    rm = nibs[1] | post_inc_reg;
                                    rn = reg_pr;
                                    format = twovalmemorysrcformat;
                                    break;
                                case 39://LDC.L @Rm+,VBR 0100mmmm00100111 (Rm) → VBR, Rm + 4 → Rm
                                    cmd = "ldc.l";
                                    rm = nibs[1] | post_inc_reg;
                                    rn = reg_vbr;
                                    format = twovalmemorysrcformat;
                                    break;
                                case 40://SHLL16 Rn 0100nnnn00101000 Rn<<16 → Rn
                                    cmd = "shll16";
                                    format = onevalformat;
                                    break;
                                case 41://SHLR16 Rn 0100nnnn00101001 Rn>>16 → Rn
                                    cmd = "1hlr16";
                                    format = onevalformat;
                                    break;
                                case 42://LDS Rm,PR 0100mmmm00101010 Rm → PR
                                    cmd = "lds";
                                    rm = nibs[1];
                                    rn = reg_pr;
                                    format = twovalformat;
                                    break;
                                case 43://JMP @Rm 0100mmmm00101011 Delayed branch, Rm → PC
                                    cmd = "jmp";
                                    rm = nibs[1];
                                    rn = unused;// should we have rn be pc in these cases?
                                    format = onevalmemoryformat;
                                    break;
                                case 46://LDC Rm,VBR 0100mmmm00101110 Rm → VBR
                                    cmd = "ldc";
                                    rm = nibs[1];
                                    rn = reg_vbr;
                                    format = twovalformat;
                                    break;

                                default:
                                    if ((funct & 0xf) == 0xf)//MAC.W @Rm+,@Rn+ 0100nnnnmmmm1111 Signed, (Rn) × (Rm) + MAC → MAC
                                    {
                                        cmd = "mac.w";
                                        rn = nibs[1] | post_inc_reg;
                                        rm = nibs[2] | post_inc_reg;
                                        format = twovalmemoryformat;
                                    }
                                    else
                                    {
                                        cmd = "?";
                                        format = unknownformat;
                                    }
                                    break;
                            }
                            break;
                        case 5://MOV.L @(disp,Rm),Rn 0101nnnnmmmmdddd (disp + Rm) → Rn
                            cmd = "mov.l";
                            rn = nibs[1];
                            rm = nibs[2];
                            disp = nibs[3]*4;
                            format = threevalmemorysrcformat;
                            break;
                        case 6:
                            funct = nibs[3];
                            rn = nibs[1];
                            rm = nibs[2];
                            switch(funct)
                            {
                                case 0x0://MOV.B @Rm,Rn 0110nnnnmmmm0000 (Rm) → sign extension → Rn
                                    cmd = "mov.b";
                                    format = twovalmemorysrcformat;
                                    break;
                                case 0x1://MOV.W @Rm,Rn 0110nnnnmmmm0001 (Rm) → sign extension → Rn
                                    cmd = "mov.w";
                                    format = twovalmemorysrcformat;
                                    break;
                                case 0x2://MOV.L @Rm,Rn 0110nnnnmmmm0010 (Rm) → Rn
                                    cmd = "mov.l";
                                    format = twovalmemorysrcformat;
                                    break;
                                case 0x3://MOV Rm,Rn 0110nnnnmmmm0011 Rm → Rn
                                    cmd = "mov";
                                    format = twovalformat;
                                    break;
                                case 0x4://MOV.B @Rm+,Rn 0110nnnnmmmm0100 (Rm) → sign extension → Rn, Rm + 1 → Rm
                                    cmd = "mov.b";
                                    rm |= post_inc_reg;
                                    format = twovalmemorysrcformat;
                                    break;
                                case 0x5://MOV.W @Rm+,Rn 0110nnnnmmmm0101 (Rm) → sign extension → Rn, Rm + 2 → Rm
                                    cmd = "mov.w";
                                    rm |= post_inc_reg;
                                    format = twovalmemorysrcformat;
                                    break;
                                case 0x6://MOV.L @Rm+,Rn 0110nnnnmmmm0110 (Rm) → Rn, Rm + 4 → Rm
                                    cmd = "mov.l";
                                    rm |= post_inc_reg;
                                    format = twovalmemorysrcformat;
                                    break;
                                case 0x7://NOT Rm,Rn 0110nnnnmmmm0111 ~Rm → Rn
                                    cmd = "not";
                                    format = twovalformat;
                                    break;
                                case 0x8://SWAP.B Rm,Rn 0110nnnnmmmm1000 Rm → Swap upper and lower halves of lower 2 bytes → Rn
                                    cmd = "swap.b";
                                    format = twovalformat;
                                    break;
                                case 0x9://SWAP.W Rm,Rn 0110nnnnmmmm1001 Rm → Swap upper and lower word → Rn
                                    cmd = "swap.w";
                                    format = twovalformat;
                                    break;
                                case 0xa://NEGC Rm,Rn 0110nnnnmmmm1010 0 – Rm – T → Rn, borrow → T
                                    cmd = "negc";
                                    format = twovalformat;
                                    break;
                                case 0xb://NEG Rm,Rn 0110nnnnmmmm1011 0 – Rm → Rn
                                    cmd = "neg";
                                    format = twovalformat;
                                    break;
                                case 0xc://EXTU.B Rm,Rn 0110nnnnmmmm1100 Zero-extends Rm from byte → Rn
                                    cmd = "extu.b";
                                    format = twovalformat;
                                    break;
                                case 0xd://EXTU.W Rm,Rn 0110nnnnmmmm1101 Zero-extends Rm from word → Rn
                                    cmd = "extu.w";
                                    format = twovalformat;
                                    break;
                                case 0xe://EXTS.B Rm,Rn 0110nnnnmmmm1110 Sign-extends Rm from byte → Rn
                                    cmd = "exts.b";
                                    format = twovalformat;
                                    break;
                                case 0xf://EXTS.W Rm,Rn 0110nnnnmmmm1111 Sign-extends Rm from word → Rn
                                    cmd = "exts.w";
                                    format = twovalformat;
                                    break;
                                default:
                                    cmd = "?";
                                    format = unknownformat;
                                    break;
                            }
                            break;
                        case 7://ADD #imm,Rn 0111nnnniiiiiiii Rn + imm → Rn
                            cmd = "add";
                            rn = nibs[1];
                            immediate = SignedValAtOffset(instruction, 8, 0);
                            format = twovalformat;
                            break;
                        case 8:
                            funct = nibs[1];
                            switch(funct)
                            {
                                case 0://MOV.B R0,@(disp,Rn) 10000000nnnndddd R0 → (disp + Rn)
                                    cmd = "mov.b";
                                    rm = reg_r0;
                                    rn = nibs[2];
                                    disp = nibs[3];
                                    format = threevalmemorydestformat;
                                    break;
                                case 1://MOV.W R0,@(disp,Rn) 10000001nnnndddd R0 → (disp × 2 + Rn)
                                    cmd = "mov.w";
                                    rm = reg_r0;
                                    rn = nibs[2];
                                    disp = nibs[3] * 2;
                                    format = threevalmemorydestformat;
                                    break;
                                case 4://MOV.B @(disp,Rm),R0 10000100mmmmdddd (disp + Rm) → sign extension → R0
                                    cmd = "mov.b";
                                    rn = reg_r0;
                                    rm = nibs[2];
                                    disp = nibs[3];
                                    format = threevalmemorysrcformat;
                                    break;
                                case 5://MOV.W @(disp,Rm),R0 10000101mmmmdddd (disp × 2 + Rm) → sign extension → R0
                                    cmd = "mov.w";
                                    rn = reg_r0;
                                    rm = nibs[2];
                                    disp = nibs[3] * 2;
                                    format = threevalmemorysrcformat;
                                    break;
                                case 8://CMP/EQ #imm,R0 10001000iiiiiiii When R0 = imm, 1 → T
                                    cmd = "cmp/eq";
                                    immediate = SignedValAtOffset(instruction, 8, 0);
                                    rn = reg_r0;
                                    format = twovalformat;
                                    break;
                                case 9://BT label 10001001dddddddd When T = 1, disp × 2 + PC → PC; When T = 0, nop.
                                    cmd = "bt";//branch if true
                                    referencedAddress = (uint)(SignedValAtOffset(instruction, 8, 0) * 2 + address + 4);
                                    format = onevalformat;
                                    break;
                                case 13://BT/S label* 10001101dddddddd When T = 1, disp × 2 + PC → PC; When T = 0, nop.

                                    cmd = "bt/s";// branch if true with delay slot
                                    referencedAddress = (uint)(SignedValAtOffset(instruction, 8, 0) * 2 + address + 4);
                                    format = onevalformat;
                                    break;
                                case 11://BF label 10001011dddddddd When T = 0, disp × 2 + PC → PC; When T = 1, nop
                                    cmd = "bf";//branch if false
                                    referencedAddress = (uint)(SignedValAtOffset(instruction, 8, 0) * 2 + address + 4);
                                    format = onevalformat;
                                    break;
                                case 15://BF/S label* 10001111dddddddd When T = 0, disp × 2 + PC → PC; When T = 1, nop
                                    cmd = "bf/s";//branch if false with delay slot
                                    referencedAddress = (uint)(SignedValAtOffset(instruction, 8, 0) * 2 + address + 4);
                                    format = onevalformat;
                                    break;
                                default:
                                    cmd = "?";
                                    format = unknownformat;
                                    break;
                            }
                            break;
                        case 9://MOV.W @(disp,PC),Rn 1001nnnndddddddd (disp × 2 + PC) → sign extension → Rn
                            cmd = "mov.w";
                            rn = nibs[1];
                            //disp = ValAtOffset(instruction, 8, 0)*2;
                            referencedAddress = (uint)(ValAtOffset(instruction, 8, 0) * 2 + address);
                            format = twovalmemorysrcformat;
                            break;
                        case 10://BRA label 1010dddddddddddd Delayed branch, disp × 2 + PC → PC
                            cmd = "bra";//branch
                            referencedAddress = (uint)(SignedValAtOffset(instruction, 12, 0) * 2 + address + 4);
                            format = onevalformat;
                            break;
                        case 11://BSR label 1011dddddddddddd Delayed branch, PC → PR, disp × 2 + PC → PC
                            cmd = "bsr";//branch to subroutine
                            referencedAddress = (uint)(SignedValAtOffset(instruction, 12, 0) * 2 + address + 4);
                            format = onevalformat;
                            break;
                        case 12:
                            funct = nibs[1];
                            switch (funct)
                            {
                                case 0x0://MOV.B R0,@(disp,GBR) 11000000dddddddd R0 → (disp + GBR)
                                    cmd = "mov.b";
                                    rm = reg_r0;
                                    rn = reg_gbr;
                                    disp = ValAtOffset(instruction, 8, 0);
                                    format = threevalmemorydestformat;
                                    break;
                                case 0x1://MOV.W R0,@(disp,GBR) 11000001dddddddd R0 → (disp × 2 + GBR)
                                    cmd = "mov.w";
                                    rm = reg_r0;
                                    rn = reg_gbr;
                                    disp = ValAtOffset(instruction, 8, 0)*2;
                                    format = threevalmemorydestformat;
                                    break;
                                case 0x2://MOV.L R0,@(disp,GBR) 11000010dddddddd R0 → (disp × 4 + GBR)
                                    cmd = "mov.l";
                                    rm = reg_r0;
                                    rn = reg_gbr;
                                    disp = ValAtOffset(instruction, 8, 0)*4;
                                    format = threevalmemorydestformat;
                                    break;
                                case 0x3://TRAPA #imm 11000011iiiiiiii PC/SR → Stack area, (imm × 4 + VBR) → PC
                                    cmd = "trapa";
                                    immediate = ValAtOffset(instruction, 8, 0)*4;
                                    format = onevalformat;
                                    break;
                                case 0x4://MOV.B @(disp,GBR),R0 11000100dddddddd (disp + GBR) → sign extension → R0
                                    cmd = "mov.b";
                                    rn = reg_r0;
                                    rm = reg_gbr;
                                    disp = ValAtOffset(instruction, 8, 0);
                                    format = threevalmemorysrcformat;
                                    break;
                                case 0x5://MOV.W @(disp,GBR),R0 11000101dddddddd (disp × 2 + GBR) → sign extension → R0
                                    cmd = "mov.w";
                                    rn = reg_r0;
                                    rm = reg_gbr;
                                    disp = ValAtOffset(instruction, 8, 0)*2;
                                    format = threevalmemorysrcformat;
                                    break;
                                case 0x6://MOV.L @(disp,GBR),R0 11000110dddddddd (disp × 4 + GBR) → R0
                                    cmd = "mov.l";
                                    rn = reg_r0;
                                    rm = reg_gbr;
                                    disp = ValAtOffset(instruction, 8, 0)*4;
                                    format = threevalmemorysrcformat;
                                    break;
                                case 0x7://MOVA @(disp,PC),R0 11000111dddddddd disp × 4 + PC → R0
                                    cmd = "mova";
                                    rn = reg_r0;
                                    //disp = ValAtOffset(instruction, 8, 0)*4;
                                    referencedAddress = (uint)(ValAtOffset(instruction, 8, 0) * 4 + address);
                                    format = twovalmemorysrcformat;
                                    break;
                                case 0x8://TST #imm,R0 11001000iiiiiiii R0 & imm, when result is 0, 1 → T
                                    cmd = "tst";
                                    rn = reg_r0;
                                    immediate = ValAtOffset(instruction, 8, 0);
                                    format = twovalformat;
                                    break;
                                case 0x9://AND #imm,R0 11001001iiiiiiii R0 & imm → R0
                                    cmd = "and";
                                    rn = reg_r0;
                                    immediate = ValAtOffset(instruction, 8, 0);
                                    format = twovalformat;
                                    break;
                                case 0xa://XOR #imm,R0 11001010iiiiiiii R0 ^ imm → R0
                                    cmd = "xor";
                                    rn = reg_r0;
                                    immediate = ValAtOffset(instruction, 8, 0);
                                    format = twovalformat;
                                    break;
                                case 0xb://OR #imm,R0 11001011iiiiiiii R0 | imm → R0
                                    cmd = "or";
                                    rn = reg_r0;
                                    immediate = ValAtOffset(instruction, 8, 0);
                                    format = twovalformat;
                                    break;
                                case 0xc://TST.B #imm,@(R0,GBR) 11001100iiiiiiii (R0 + GBR) & imm, when result is 0, 1 → T
                                    cmd = "tst.b";
                                    rm = reg_r0;
                                    rn = reg_gbr;
                                    immediate = ValAtOffset(instruction, 8, 0);
                                    format = threevalmemorydestformat;
                                    break;
                                case 0xd://AND.B #imm,@(R0,GBR) 11001101iiiiiiii (R0 + GBR) & imm → (R0 + GBR)
                                    cmd = "and.b";
                                    rm = reg_r0;
                                    rn = reg_gbr;
                                    immediate = ValAtOffset(instruction, 8, 0);
                                    format = threevalmemorydestformat;
                                    break;
                                case 0xe://XOR.B #imm,@(R0,GBR) 11001110iiiiiiii (R0 + GBR) ^ imm → (R0 + GBR)
                                    cmd = "xor.b";
                                    rm = reg_r0;
                                    rn = reg_gbr;
                                    immediate = ValAtOffset(instruction, 8, 0);
                                    format = threevalmemorydestformat;
                                    break;
                                case 0xf://OR.B #imm,@(R0,GBR) 11001111iiiiiiii (R0 + GBR) | imm → (R0 + GBR)
                                    cmd = "or.b";
                                    rm = reg_r0;
                                    rn = reg_gbr;
                                    immediate = ValAtOffset(instruction, 8, 0);
                                    format = threevalmemorydestformat;
                                    break;
                            }
                            break;
                        case 13://MOV.L @(disp,PC),Rn 1101nnnndddddddd (disp × 4 + PC) → Rn
                            cmd = "mov.l";
                            rn = nibs[1];
                            referencedAddress = (uint)(ValAtOffset(instruction, 8, 0) * 4 + address);
                            format = twovalmemorysrcformat;
                            break;
                        case 14://MOV #imm,Rn 1110nnnniiiiiiii imm → sign extension → Rn
                            cmd = "mov";
                            rn = nibs[1];
                            immediate = SignedValAtOffset(instruction, 8, 0);
                            format = twovalformat;
                            break;
                        default:
                            cmd = "?";
                            format = unknownformat;
                            break;
                    }

                    if (format == novalformat)
                    {
                        display = string.Format(format, cmd);
                    }
                    else if (format == onevalformat)
                    {
                        string param = "?";
                        if (rn != unused)
                            param = GetRegister(rn);
                        else if (rm != unused)
                            param = GetRegister(rm);
                        else if (referencedAddress != unused)
                            param = GetReferencedAddress(referencedAddress);
                        else if (immediate != unused)
                            param = GetImmediate(immediate);
                        display = string.Format(format, cmd, param);
                    }
                    else if (format == onevalmemoryformat)
                    {
                        string param = "?";
                        if (rn != unused)
                            param = GetRegister(rn);
                        else if (rm != unused)
                            param = GetRegister(rm);
                        display = string.Format(format, cmd, param);
                    }
                    else if (format == twovalformat)
                    {
                        string param2 = GetRegister(rn);
                        string param1 = "?";
                        if (rm != unused)
                            param1 = GetRegister(rm);
                        else if (immediate != unused)
                            param1 = GetImmediate(immediate);
                        display = string.Format(format, param1, param2);
                    }
                    else if (format == twovalmemoryformat)
                    {
                        string param2 = GetRegister(rn);
                        string param1 = GetRegister(rm);
                        display = string.Format(format, param1, param2);
                    }
                    else if (format == twovalmemorydestformat)
                    {
                        string param2 = GetRegister(rn);
                        string param1 = GetRegister(rm);
                        display = string.Format(format, param1, param2);
                    }
                    else if (format == twovalmemorysrcformat)
                    {
                        string param2 = GetRegister(rn);
                        string param1 = "?";
                        if (rm != unused)
                            param1 = GetRegister(rm);
                        else if (referencedAddress != unused)
                            param1 = GetReferencedAddress(referencedAddress);
                        display = string.Format(format, param1, param2);
                    }
                }

            }

            public override bool IsBranch => throw new NotImplementedException();
            public override bool IsCall => throw new NotImplementedException();
            public override bool IsJump => throw new NotImplementedException();
            public override bool IsReturn => throw new NotImplementedException();

            public override bool IsAssignment => throw new NotImplementedException();

            public override uint GetGlobalVariable(CodeBlock<ISInstruction> block)
            {
                throw new NotImplementedException();
            }

            public override void GetAssignmentGlobals(out uint left, out string right, CodeBlock<ISInstruction> block)
            {
                throw new NotImplementedException();
            }

        }


    }
}
