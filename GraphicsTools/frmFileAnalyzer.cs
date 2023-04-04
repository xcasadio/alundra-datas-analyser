using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;
using alundramultitool;
using System.Numerics;

namespace GraphicsTools
{
    public partial class frmFileAnalyzer : Form
    {
        public frmFileAnalyzer()
        {
            InitializeComponent();
            Alundra.DebugSymbols.Init();
            canvas.MouseWheel += Canvas_MouseWheel;

            canvas.MouseDown += Canvas_MouseDown;

            canvas.MouseMove += Canvas_MouseMove;
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                int moved = e.Location.Y - lastpos.Y;
                rot += 0.1f * moved;
                lastpos = e.Location;
                canvas.Refresh();
            }
        }

        Point lastpos;

        private void Canvas_MouseDown(object sender, MouseEventArgs e)
        {
            lastpos = e.Location;
        }

        float camzoom = 1;
        private void Canvas_MouseWheel(object sender, MouseEventArgs e)
        {
            camzoom += 0.5f * (e.Delta > 0 ? 1 : -1);
            canvas.Refresh();
        }

        Vector3 camerapos = new Vector3(0, -100, 0);
        Vector3 cameratarget = new Vector3(0, 0, 0);
        Matrix4x4 camera; //= Matrix4x4.CreateLookAt(camerapos, cameratarget, new Vector3(0, 1, 0));

        public string datafile;

        public int offset;
        public int memaddress;
        public int startoffset = 0;
        public int startsize = 0;
        int chunklength = 1024 * 1024;
        byte[] data;

        public int ParseNum(string num)
        {
            int i = 0;
            if (num.StartsWith("0x"))
            {
                int.TryParse(num.Replace("0x", ""), System.Globalization.NumberStyles.AllowHexSpecifier, null, out i);
            }
            else
            {
                int.TryParse(num, out i);
            }
            return i;
        }

        List<frmLib.AnalyzedFunction> libfuncs;
        private void frmFileAnalyzer_Load(object sender, EventArgs e)
        {
            loadchunk(offset);

            var frm = new frmLib(
                @"..\..\..\psyq_sdk\LIB\LIBSND.LIB",
                @"..\..\..\psyq_sdk\INCLUDE\LIBSND.H",
                @"..\..\..\psyq_sdk\LIB",
                @"..\..\..\psyq_sdk\INCLUDE",
                this);
            frm.Show();
            libfuncs = frm.analyzedfuncs;
        }

        int instOffset = 0;
        List<ISInstruction> instructions = new List<ISInstruction>();
        List<uint> functions = new List<uint>();
        void loadchunk(int offset, bool alundraeventlist = false)
        {
            txtAddressOffset.Text = (memaddress - offset).ToString();
            this.offset = offset;
            data = new byte[chunklength];
            var stream = File.OpenRead(datafile);
            stream.Position = offset;
            int numread = stream.Read(data, 0, chunklength);
            stream.Close();
            rtfText.LoadFile(new MemoryStream(data), RichTextBoxStreamType.PlainText);
            rtfText.Select(startoffset, startsize);

            instOffset = ParseNum(txtInstructionsOffset.Text);
            instructions.Clear();
            lstInstructions.Items.Clear();
            lstFunctions.Items.Clear();
            var mips = new MIPS();
            if (alundraeventlist)
            {
                //offset = 0x9b5b4;
                functions.Clear();
                for (int dex = 0; dex <= 0x3FC; dex += 4)
                {
                    uint addr = (uint)(data[dex] | data[dex + 1] << 8 | data[dex + 2] << 16 | (uint)data[dex + 3] << 24);
                    functions.Add(0xFFFFFFF & addr);
                }

                for (int fdex = 0; fdex < functions.Count; fdex++)
                {
                    var functaddr = functions[fdex];
                    var sicode = Alundra.SpriteInfoEventCodes.GetCode((byte)fdex);

                    string fname = $"({sicode.code.ToString("x2")}_{sicode.name}_handler)";

                    lstFunctions.Items.Add("0x" + functaddr.ToString("x") + fname);
                }
            }
            else
            {
                for (int dex = 0; dex < 0x6000; dex += 4)
                {
                    var inst = new MIPS.Instruction((uint)(instOffset + offset + dex), (uint)(data[dex] | data[dex + 1] << 8 | data[dex + 2] << 16 | (uint)data[dex + 3] << 24));
                    if (inst.cmd == "jal")
                        functions.Add(inst.referencedAddress);

                    instructions.Add(inst);
                    lstInstructions.Items.Add(string.Format("{0}:  {1}", inst.address.ToString("x8"), inst.display));
                }
                foreach (var functaddr in functions.Distinct().OrderBy(x => x))
                {
                    string fname = "";
                    if (Alundra.DebugSymbols.FunctionNames.ContainsKey(functaddr))
                    {
                        fname += "(";
                        var fref = Alundra.DebugSymbols.FunctionNames[functaddr];
                        if (!string.IsNullOrEmpty(fref.name))
                            fname += fref.name;
                        if (!string.IsNullOrEmpty(fref.comment))
                            fname += "//" + fref.comment;
                        fname += ")";
                    }
                    lstFunctions.Items.Add("0x" + functaddr.ToString("x8") + fname);
                }
            }
        }

        bool flip = false;
        void DisplayData(int pos)
        {
            int addrOffset = 0;
            int.TryParse(txtAddressOffset.Text, out addrOffset);
            txtOffset.Text = offset.ToString();
            lblCursorOffset.Text = (pos + offset).ToString() + "(" + (pos + offset + addrOffset).ToString("x6") + ")";
            lblRelOffset.Text = pos.ToString();
            lblSelLength.Text = rtfText.SelectionLength.ToString();

            lbl8bit.Text = data[pos].ToString() + " (" + data[pos].ToString("x2") + ")"; ;
            long l = data[pos] | data[pos + 1] << 8;
            if (flip)
                l = data[pos + 1] | data[pos] << 8;
            lbl16bit.Text = l.ToString() + " (" + l.ToString("x4") + ")";
            l = data[pos] | data[pos + 1] << 8 | data[pos + 2] << 16 | (long)data[pos + 3] << 24;
            if (flip)
                l = data[pos + 3] | data[pos + 2] << 8 | data[pos + 1] << 16 | (long)data[pos + 0] << 24;
            lbl32bit.Text = l.ToString() + " (" + l.ToString("x8") + ")";

            short s = (short)l;
            float f = s * 360f / 65536f;
            lblFloat16.Text = f.ToString();
            short s1 = (short)(data[pos + 1] | data[pos] << 8);
            ushort s2 = (ushort)(data[pos + 3] | data[pos + 2] << 8);
            f = s1 + s2 / 65536.0f;
            lblFloat.Text = f.ToString();
            lbl4bita.Text = ((data[pos] & 0xf0) >> 4).ToString();
            lbl4bitb.Text = (data[pos] & 0xf).ToString();
        }

        private void rtfText_SelectionChanged(object sender, EventArgs e)
        {
            if (rtfText.SelectionStart >= 0)
                DisplayData(rtfText.SelectionStart);
        }



        private void btnViewImage_Click(object sender, EventArgs e)
        {
            int stride, width, height, startx, starty;
            int.TryParse(txtStride.Text, out stride);
            int.TryParse(txtWidth.Text, out width);
            int.TryParse(txtHeight.Text, out height);
            int.TryParse(txtStartx.Text, out startx);
            int.TryParse(txtStarty.Text, out starty);

            int imagestart = rtfText.SelectionStart;

            int bpp = 4;
            byte[] imagedata = new byte[width * height * bpp / 8];

            if (stride == -1)
            {
                //compressed
                int imagedex = 0;
                int buffdex = imagestart;
                while (imagedex < imagedata.Length)
                {
                    byte b = data[buffdex++];
                    if (b == 0xad)
                    {
                        int seek = data[buffdex++];
                        if (seek == 0)
                        {
                            imagedata[imagedex++] = b;
                        }
                        else
                        {
                            int len = data[buffdex++];
                            int seekdex = imagedex - seek;
                            while (len-- > 0)
                                imagedata[imagedex++] = imagedata[seekdex++];
                        }
                    }
                    else
                        imagedata[imagedex++] = b;
                }
            }
            else
            {
                for (int y = 0; y < height; y++)
                {
                    Buffer.BlockCopy(data, imagestart + (y + starty) * stride + (startx / 2), imagedata, y * width * bpp / 8, width * bpp / 8);
                }
            }

            int paloffset = 0;
            Color[] palettes = new Color[(int)Math.Pow(2, bpp)];
            if (Program.palette != null)
                palettes = Program.palette;
            else
            {
                for (int dex = 0; dex < palettes.Length; dex++)
                {
                    int ddex = imagestart + paloffset + dex * 2;
                    palettes[dex] = Utils.FromPsxColor(data[ddex + 1], data[dex]);// Color.FromArgb(255, (data[ddex + 1] & 0x1f) << 3, ((data[ddex + 1] & 0xe0) >> 2) | ((data[ddex] & 0x3) << 6), data[ddex] & 0x7c);
                }
            }
            Program.viewer = new frmViewer();
            Program.viewer.Show();
            Program.viewer.init(imagedata, 16, 4, width, height, palettes);
        }

        private void btnJump_Click(object sender, EventArgs e)
        {
            offset = ParseNum(txtOffset.Text);
            loadchunk(offset);
        }



        private void btnViewPal_Click(object sender, EventArgs e)
        {
            int stride, width, height;
            int.TryParse(txtStride.Text, out stride);
            int.TryParse(txtWidth.Text, out width);
            int.TryParse(txtHeight.Text, out height);

            int palettestart = rtfText.SelectionStart;

            int bpp = 16;
            byte[] imagedata = new byte[width * height * bpp / 8];
            for (int y = 0; y < height; y++)
            {
                Buffer.BlockCopy(data, palettestart + y * stride, imagedata, y * width * bpp / 8, width * bpp / 8);

            }
            var frm = new frmViewer();
            frm.Show();
            frm.initpalette(Program.viewer, imagedata, 16, 4, width, height);
        }

        private void rtfText_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (rtfText.SelectionStart >= 0)
            {
                lstInstructions.SelectedIndex = (rtfText.SelectionStart) / 4;

            }
        }



        private void btnFindInt32_Click(object sender, EventArgs e)
        {
            long search = ParseNum(txtSearch.Text);
            int range = ParseNum(txtSearchRange.Text);
            for (int dex = rtfText.SelectionStart + 1; dex < data.Length - 3; dex++)
            {
                long num = data[dex] | data[dex + 1] << 8 | data[dex + 2] << 16 | data[dex + 3] << 24;
                if (flip)
                    num = data[dex + 3] | data[dex + 2] << 8 | data[dex + 1] << 16 | data[dex + 0] << 24;
                if (num >= search && num <= search + range)
                {
                    rtfText.Focus();
                    rtfText.Select(dex, 0);
                    //rtfText.ScrollToCaret();
                    break;
                }
            }
        }

        private void frmFileAnalyzer_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F3)
                btnFindInt32_Click(null, null);
        }

        private void frmFindInt16_Click(object sender, EventArgs e)
        {
            long search = ParseNum(txtSearch.Text);
            int range = ParseNum(txtSearchRange.Text);
            for (int dex = rtfText.SelectionStart + 1; dex < data.Length - 3; dex++)
            {
                long num = data[dex] | data[dex + 1] << 8;// | data[dex + 2] << 16 | data[dex + 3] << 24;
                if (flip)
                {
                    num = data[dex + 1] | data[dex + 0] << 8;
                }
                if (num >= search && num <= search + range)
                {
                    rtfText.Focus();
                    rtfText.Select(dex, 0);
                    //rtfText.ScrollToCaret();
                    break;
                }
            }
        }
        List<ISInstruction> selectedFunction = null;
        private void lstFunctions_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstFunctions.SelectedIndex >= 0)
            {
                int address = ParseNum(lstFunctions.Text.Split('(')[0].Trim());
                var fdat = new byte[chunklength];
                var stream = File.OpenRead(datafile);
                stream.Position = address;
                int numread = stream.Read(fdat, 0, chunklength);
                stream.Close();

                bool exit = false;

                selectedFunction = new List<ISInstruction>();

                for (int dex = 0; dex < 10000; dex += 4)
                {
                    var inst = new MIPS.Instruction((uint)(address + dex), (uint)(fdat[dex] | fdat[dex + 1] << 8 | fdat[dex + 2] << 16 | (uint)fdat[dex + 3] << 24));
                    selectedFunction.Add(inst);
                    if (exit)
                        break;
                    if (inst.IsReturn)
                        exit = true;
                }
                var blocks = AnalyzeFunction(selectedFunction);
                txtFunction.Text = PrintFunction(blocks);

            }
        }

        public static string PrintFunction(List<CodeBlock<ISInstruction>> blocks)
        {
            string ftext = "";

            var fnames = Alundra.DebugSymbols.FunctionNames;
            var evars = Alundra.DebugSymbols.EntityVarOffsets;
            var globalvars = Alundra.DebugSymbols.GlobalVariableNames;
            var comments = Alundra.DebugSymbols.Comments;

            int indentlevel = 0;
            int codestart = 40;
            foreach (var block in blocks)
            {
                if (block.BeginsLoop)
                {
                    ftext += RawIndent(codestart) + Indent(indentlevel) + "do{\r\n";
                    indentlevel++;
                }
                if (block.IsJumpTarget)
                {
                    ftext += "\r\n";
                }
                foreach (var inst in block.Instructions)
                {
                    string ccode = "";
                    if (fnames.ContainsKey(inst.address))
                    {
                        ccode = "void " + fnames[inst.address].name + "()";
                        if (!string.IsNullOrEmpty(fnames[inst.address].comment))
                            ccode += "//" + fnames[inst.address].comment;
                    }
                    else
                    {
                        switch (inst.cmd)
                        {
                            case "jal":
                                var sname = inst.referencedAddress.ToString("x");
                                var scomment = "";
                                if (fnames.ContainsKey(inst.referencedAddress))
                                {
                                    var fref = fnames[inst.referencedAddress];
                                    if (!string.IsNullOrEmpty(fref.name))
                                        sname = fref.name;
                                    if (!string.IsNullOrEmpty(fref.comment))
                                        scomment = fref.comment;
                                }
                                ccode = sname + "()//" + scomment;
                                break;
                            case "sw":
                            case "lw":
                            case "lhu":
                            case "lh":
                            case "shu":
                            case "sh":
                            case "lb":
                            case "sb":
                            case "lbu":
                            case "sbu":
                                if (inst.rs != 29)//if not local variable declaration
                                {
                                    bool nudgewierdness = false;
                                    //if (inst.rt == 16 || inst.rs == 16)
                                    //    nudgewierdness = true;
                                    if (nudgewierdness)
                                        inst.immediate += 0x134;
                                    //assume entity struct
                                    if (inst.immediate > 0xc && inst.immediate < evars.Length)
                                    {

                                        if (!string.IsNullOrEmpty(evars[inst.immediate]))
                                        {

                                            if (inst.cmd == "lw" || inst.cmd == "lh" || inst.cmd == "lhu" || inst.cmd == "lb" || inst.cmd == "lbu")
                                                ccode = MIPS.GetRegister(inst.rt) + " = " + MIPS.GetRegister(inst.rs) + "." + evars[inst.immediate];
                                            else if (inst.cmd == "sw" || inst.cmd == "sh" || inst.cmd == "shu" || inst.cmd == "sb" || inst.cmd == "sbu")
                                                ccode = MIPS.GetRegister(inst.rs) + "." + evars[inst.immediate] + " = " + MIPS.GetRegister(inst.rt);
                                            if (nudgewierdness)
                                                inst.immediate -= 0x134;
                                            break;
                                        }

                                    }

                                    var fulladdr2 = inst.GetGlobalVariable(block);
                                    if (fulladdr2 > 0)
                                    {
                                        var name2 = "0x" + fulladdr2.ToString("x");
                                        if (globalvars.ContainsKey(fulladdr2))
                                            name2 = globalvars[fulladdr2].name;

                                        if (fulladdr2 > 0x1ac498 && fulladdr2 < (0x1ac498 + 0x294))
                                        {
                                            name2 = "playercharacter";
                                            var off = fulladdr2 - 0x1ac498;
                                            var evarname = evars[off];
                                            if (!string.IsNullOrEmpty(evarname))
                                                name2 += "." + evarname;
                                            else
                                                name2 += "[" + off.ToString("x") + "]";

                                        }
                                        if (inst.GlobalRegisterOffset > 0)
                                        {
                                            name2 += "[" + MIPS.GetRegister(inst.GlobalRegisterOffset) + "]";
                                        }

                                        if (inst.cmd == "lw" || inst.cmd == "lh" || inst.cmd == "lhu" || inst.cmd == "lb" || inst.cmd == "lbu")
                                            ccode = MIPS.GetRegister(inst.rt) + " = *" + name2;
                                        else if (inst.cmd == "sw" || inst.cmd == "sh" || inst.cmd == "shu" || inst.cmd == "sb" || inst.cmd == "sbu")
                                            ccode = "*" + name2 + " = " + MIPS.GetRegister(inst.rt);
                                    }
                                    else
                                    {
                                        if (inst.cmd == "lw" || inst.cmd == "lh" || inst.cmd == "lhu" || inst.cmd == "lb" || inst.cmd == "lbu")
                                            ccode = MIPS.GetRegister(inst.rt) + " = " + MIPS.GetRegister(inst.rs) + "[" + inst.immediate.ToString("x") + "]";
                                        else if (inst.cmd == "sw" || inst.cmd == "sh" || inst.cmd == "shu" || inst.cmd == "sb" || inst.cmd == "sbu")
                                            ccode = MIPS.GetRegister(inst.rs) + "[" + inst.immediate.ToString("x") + "]" + " = " + MIPS.GetRegister(inst.rt);
                                    }
                                    if (nudgewierdness)
                                        inst.immediate -= 0x134;
                                }
                                break;

                            case "addiu":
                            case "addi":
                            case "ori":
                                //case "lw":
                                //case "sw":
                                //case "lhu":
                                //case "lh":
                                //case "shu":
                                //case "sh":
                                var fulladdr = inst.GetGlobalVariable(block);
                                if (fulladdr != 0)
                                {
                                    if (globalvars.ContainsKey(fulladdr))
                                        ccode = "//" + globalvars[fulladdr].name;
                                    else if (fulladdr > 0x1ac498 && fulladdr < (0x1ac498 + 0x294))
                                    {
                                        var name2 = "playercharacter";
                                        var off = fulladdr - 0x1ac498;
                                        var evarname = evars[off];
                                        if (!string.IsNullOrEmpty(evarname))
                                            name2 += "." + evarname;
                                        else
                                            name2 += "[" + off.ToString("x") + "]";
                                        ccode = "//" + name2;
                                    }
                                    else
                                        ccode = "//0x" + fulladdr.ToString("x");
                                }
                                break;
                                /*case "addiu":
                                    //get prev lui
                                    var mdex = block.Instructions.IndexOf(inst);
                                    uint addr = inst.immediateu;
                                    var reg = inst.rs;
                                    for (int dex = mdex-1;mdex>=dex-3 && dex >=0;dex--)
                                    {
                                        var binst = block.Instructions[dex];
                                        if (binst.cmd == "lui" && binst.rt == reg)
                                        {
                                            uint fulladdr = (uint)((UInt32)(((uint)binst.immediate & 0xff) << 16) | addr);

                                            if (globalvars.ContainsKey(fulladdr))
                                                ccode = "//" + globalvars[fulladdr];
                                            else
                                                ccode = "//0x" + fulladdr.ToString("x");
                                            break;
                                        }
                                    }

                                    //get prev
                                    break;*/
                        }
                    }
                    if (comments.ContainsKey(inst.address))
                        ccode += "//" + comments[inst.address];
                    string asm = string.Format("{0}: {1} {2}", ((block.Instructions.IndexOf(inst) == 0 && block.IsJumpTarget) ? "0x" : "") + inst.address.ToString("x8"), inst.instruction.ToString("x8"), inst.display);
                    ftext += string.Format("{0}{1}{2}{3}\r\n", asm, RawIndent(codestart - asm.Length), Indent(indentlevel), ccode);
                }
                if (block.EndsLoop)
                {
                    indentlevel--;
                    ftext += RawIndent(codestart) + Indent(indentlevel) + "}\r\n";
                }
                if (block.BlockType == BlockType.TwoWay)
                {
                    if (block.EndsLoop)
                    {
                        //output nothing special, this jump is just ending a posttested loop
                    }
                    else if (block.BranchOperation != null)
                    {
                        ftext += RawIndent(codestart) + Indent(indentlevel) + block.BranchOperation.Print() + "\r\n";
                    }
                    else
                    {
                        ftext += RawIndent(codestart) + Indent(indentlevel) + "if\r\n";
                    }
                }
                if (block.BlockType == BlockType.OneWay)
                {
                    if (block.EndsLoop)
                    {
                        //output nothing special, this jump is just ending a pretested or infinite loop
                    }
                    else if (block.OutEdges[0] != null && block.OutEdges[0].BlockType == BlockType.Return)
                    {
                        ftext += RawIndent(codestart) + Indent(indentlevel) + "return\r\n";
                    }
                    else if (block.OutEdges[0] != null && block.OutEdges[0].EndsLoop)
                    {
                        ftext += RawIndent(codestart) + Indent(indentlevel) + "continue\r\n";
                    }
                    //need a way to detect a break
                    else
                    {
                        ftext += RawIndent(codestart) + Indent(indentlevel) + "else\r\n";
                    }
                }

            }

            return ftext;
        }

        static string Indent(int indentlevel)
        {
            string indent = "";
            for (int dex = 0; dex < indentlevel; dex++)
            {
                indent += "  ";
            }
            return indent;
        }

        static string RawIndent(int indentlevel)
        {
            string indent = "";
            for (int dex = 0; dex < indentlevel; dex++)
            {
                indent += " ";
            }
            return indent;
        }

        public static List<CodeBlock<ISInstruction>> AnalyzeFunction(List<ISInstruction> function)
        {
            List<uint> refs = function.Select(x => x.referencedAddress).Where(x => x != 0).ToList();


            List<CodeBlock<ISInstruction>> blocks = new List<CodeBlock<ISInstruction>>();

            var block = new CodeBlock<ISInstruction>();

            for (int dex = 0; dex < function.Count; dex++)
            {
                var inst = function[dex];
                if (refs.Contains(inst.address))
                {
                    if (block.Instructions.Count > 0)
                    {
                        block.BlockType = BlockType.FallThrough;
                        block.OutAddresses.Add(inst.address);
                        blocks.Add(block);
                        block = new CodeBlock<ISInstruction>();
                    }
                }
                if (inst.IsCall)
                {
                    block.BlockType = BlockType.Call;
                    block.OutAddresses.Add(inst.address + 8);
                    block.Instructions.Add(inst);
                    dex++;
                    inst = function[dex];
                    block.Instructions.Add(inst);
                    blocks.Add(block);
                    block = new CodeBlock<ISInstruction>();
                }
                else if (inst.IsReturn)
                {
                    block.BlockType = BlockType.Return;
                    block.Instructions.Add(inst);
                    dex++;
                    if (dex < function.Count)
                    {
                        inst = function[dex];
                        block.Instructions.Add(inst);
                    }
                    blocks.Add(block);
                    break;
                }
                else if (inst.IsBranch)
                {
                    block.BlockType = BlockType.TwoWay;
                    block.OutAddresses.Add(inst.referencedAddress);
                    block.OutAddresses.Add(inst.address + 8);
                    block.Instructions.Add(inst);
                    dex++;
                    inst = function[dex];
                    block.Instructions.Add(inst);
                    blocks.Add(block);
                    block = new CodeBlock<ISInstruction>();
                }
                else if (inst.IsJump)
                {
                    block.BlockType = BlockType.OneWay;
                    block.OutAddresses.Add(inst.referencedAddress);
                    block.Instructions.Add(inst);
                    dex++;
                    inst = function[dex];
                    block.Instructions.Add(inst);
                    blocks.Add(block);
                    block = new CodeBlock<ISInstruction>();
                }
                else
                {
                    block.Instructions.Add(inst);
                }

            }

            foreach (var b in blocks)
            {
                b.OutEdges = b.OutAddresses.Select(x => blocks.Where(y => y.Address == x).FirstOrDefault()).ToList();
                b.InEdges = blocks.Where(x => x.OutAddresses.Contains(b.Address)).ToList();
            }


            return blocks;
        }

        private void btnJumpFunctionList_Click(object sender, EventArgs e)
        {
            offset = ParseNum(txtOffset.Text);
            loadchunk(offset, true);
        }

        private void btnAlundraEventFuncs_Click(object sender, EventArgs e)
        {
            lstInstructions.Width -= 100;
            lstFunctions.Left -= 100;
            lstFunctions.Width += 20;
            txtFunction.Left -= 80;
            txtFunction.Width += 10;
            offset = 0x9b5b4;
            loadchunk(offset, true);
        }


        public class AnalyzedGlobalVariable
        {
            public AnalyzedGlobalVariable(uint addr, List<AnalyzedGlobalVariable> varlist)
            {
                varlist.Add(this);
                var globalvars = Alundra.DebugSymbols.GlobalVariableNames;
                address = addr;
                if (globalvars.ContainsKey(address))
                    name = globalvars[address].name;

                if (addr > 0x1ac498 && addr < (0x1ac498 + 0x294))
                {
                    //if (name!=null)
                    //{
                    //    string s = "testc";
                    //}
                    name = "playercharacter";
                    var off = addr - 0x1ac498;
                    var evarname = Alundra.DebugSymbols.EntityVarOffsets[off];
                    if (!string.IsNullOrEmpty(evarname))
                        name += "." + evarname;
                    else
                        name += "[" + off.ToString("x") + "]";

                }
            }
            public uint address;
            public string name;
            public string notes;
            public List<AnalyzedFunction> functions = new List<AnalyzedFunction>();
            public List<VariableAssignment> assignments = new List<VariableAssignment>();

            public string DisplayName
            {
                get
                {
                    if (!string.IsNullOrEmpty(name))
                        return name + "(" + address.ToString("x") + ")";
                    return address.ToString("x");
                }
            }

            public override string ToString()
            {
                return DisplayName + "[" + functions.Count + "]";
            }
        }

        public class VariableAssignment
        {
            public AnalyzedFunction func;
            public AnalyzedGlobalVariable left;
            public AnalyzedGlobalVariable right;
            public string rightstring;
        }
        public class AnalyzedFunction
        {
            public AnalyzedFunction(uint addr, List<AnalyzedFunction> funclist, List<AnalyzedGlobalVariable> varlist, string datafile, Func<List<ISInstruction>, List<CodeBlock<ISInstruction>>> AnalyzeFunction, uint endaddr = 0, string fname = null)
            {
                var fnames = Alundra.DebugSymbols.FunctionNames;
                var evars = Alundra.DebugSymbols.EntityVarOffsets;
                var globalvars = Alundra.DebugSymbols.GlobalVariableNames;
                int chunklength = 1024 * 1024;

                funclist.Add(this);
                address = addr;
                if (fname != null)
                {
                    name = fname;
                }
                else if (fnames.ContainsKey(address))
                {
                    name = fnames[address].name;
                    notes = fnames[address].comment;
                }



                var fdat = new byte[chunklength];
                var stream = File.OpenRead(datafile);
                stream.Position = address;
                int numread = stream.Read(fdat, 0, chunklength);
                stream.Close();

                bool exit = false;

                var function = new List<ISInstruction>();
                instructions = function;

                for (int dex = 0; dex < 10000; dex += 4)
                {
                    var inst = new MIPS.Instruction((uint)(address + dex), (uint)(fdat[dex] | fdat[dex + 1] << 8 | fdat[dex + 2] << 16 | (uint)fdat[dex + 3] << 24));
                    function.Add(inst);
                    if (exit)
                        break;
                    if (inst.IsReturn || (endaddr != 0 && inst.address == endaddr))
                        exit = true;
                }
                length = (int)(function.Last().address - address);
                blocks = AnalyzeFunction(function);

                var debugnames = new[] { "outputdebuginfo", "printdebug", "printdebugparams", "printdebugerror" };

                foreach (var block in blocks)
                {
                    foreach (var inst in block.Instructions)
                    {
                        switch (inst.cmd)
                        {
                            case "jal":
                                var calledfunc = funclist.FirstOrDefault(x => x.address == inst.referencedAddress);
                                if (calledfunc == null)
                                {
                                    calledfunc = new AnalyzedFunction(inst.referencedAddress, funclist, varlist, datafile, AnalyzeFunction);
                                }
                                if (!calledfunctions.Contains(calledfunc))
                                    calledfunctions.Add(calledfunc);

                                if (debugnames.Contains(calledfunc.name))
                                {
                                    var mdex = block.Instructions.IndexOf(inst);
                                    int seekback = 10;
                                    for (int dex = mdex + 1; dex >= mdex - (1 + seekback) && dex >= 0; dex--)
                                    {
                                        int reg = 4;
                                        if (calledfunc.name == "printdebugerror")
                                            reg = 5;
                                        var tinst = (MIPS.Instruction)block.Instructions[dex];
                                        if (tinst.type == MIPS.InstructionType.Itype && tinst.rt == reg)
                                        {
                                            var spos = tinst.GetGlobalVariable(block);
                                            if (spos > 0 && spos < 0x80000)
                                            {
                                                var sstream = File.OpenRead(datafile);
                                                sstream.Position = spos;
                                                byte[] buff = new byte[1024];
                                                sstream.Read(buff, 0, 1024);
                                                StringBuilder sb = new StringBuilder();
                                                for (int sdex = 0; sdex < 1024; sdex++)
                                                {
                                                    if (buff[sdex] == 0)
                                                        break;
                                                    sb.Append((char)buff[sdex]);

                                                }
                                                debugstrings.Add(sb.ToString());
                                            }
                                            else
                                            {
                                                string s = "why";
                                            }
                                            break;
                                        }
                                    }
                                }

                                break;
                            case "jalr":
                                callsfunctionpointers = true;
                                break;
                            //TODO record global variables
                            case "addiu":
                            case "addi":
                            case "ori":
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
                                var fulladdr = inst.GetGlobalVariable(block);
                                if (fulladdr != 0)
                                {
                                    var gvar = varlist.FirstOrDefault(x => x.address == fulladdr);
                                    if (gvar == null)
                                    {
                                        gvar = new AnalyzedGlobalVariable(fulladdr, varlist);
                                    }
                                    if (!globalvariables.Contains(gvar))
                                        globalvariables.Add(gvar);
                                }


                                //if its an assignment, get the left and right operands
                                if (inst.IsAssignment)
                                {
                                    uint fulladr;
                                    string right;
                                    inst.GetAssignmentGlobals(out fulladr, out right, block);
                                    if (fulladr != 0)
                                    {
                                        var gvar = varlist.FirstOrDefault(x => x.address == fulladr);
                                        if (gvar != null)
                                        {
                                            var assn = new VariableAssignment { func = this, left = gvar, rightstring = right };
                                            gvar.assignments.Add(assn);
                                        }
                                    }
                                }
                                break;


                        }

                    }
                    if (block.EndsLoop)
                    {
                        hasloop = true;
                    }
                }

                foreach (var gvar in globalvariables)
                {
                    if (gvar.address >= 0x29f30 && gvar.address <= 0x2aaba)
                    {
                        //its in the range of some string variables
                        var sstream = File.OpenRead(datafile);
                        sstream.Position = gvar.address;
                        byte[] buff = new byte[1024];
                        sstream.Read(buff, 0, 1024);
                        StringBuilder sb = new StringBuilder();
                        for (int sdex = 0; sdex < 1024; sdex++)
                        {
                            if (buff[sdex] == 0)
                                break;
                            sb.Append((char)buff[sdex]);

                        }
                        var toadd = sb.ToString();
                        if (!debugstrings.Contains(toadd))
                            debugstrings.Add(toadd);
                        hasdebugoutput = true;
                    }
                }

                if (calledfunctions.Any(x => debugnames.Contains(x.name)))
                    hasdebugoutput = true;
            }
            public List<ISInstruction> instructions;
            public List<CodeBlock<ISInstruction>> blocks;
            public uint address;
            public string libname;
            public List<frmLib.AnalyzedFunction> potentiallibs = new List<frmLib.AnalyzedFunction>();
            public string addressstring { get { return address.ToString("x"); } }
            public int length;
            public string name;
            public string notes;
            public List<string> parameters = new List<string>();
            public string returnval;
            public List<AnalyzedFunction> calledfunctions = new List<AnalyzedFunction>();
            public List<AnalyzedGlobalVariable> globalvariables = new List<AnalyzedGlobalVariable>();
            public List<AnalyzedFunction> calledby = new List<AnalyzedFunction>();
            public bool callsfunctionpointers = false;
            public bool hasloop = false;
            public bool hasdebugoutput = false;
            public List<string> debugstrings = new List<string>();

            public List<AnalyzedFunction> stack = new List<AnalyzedFunction>();
            public List<AnalyzedFunction> maxstack = new List<AnalyzedFunction>();

            public List<AnalyzedFunction> GetDepth(List<AnalyzedFunction> depthstack)
            {
                stack = depthstack;
                maxstack = stack;
                //detect recursion
                if (depthstack.Contains(this))
                    return depthstack;

                depthstack.Add(this);


                foreach (var func in calledfunctions)
                {
                    var potentialstack = func.GetDepth(depthstack.ToList());
                    if (potentialstack.Count > maxstack.Count)
                        maxstack = potentialstack;
                }

                return maxstack;
            }

            public string DisplayName
            {
                get
                {

                    if (!string.IsNullOrEmpty(libname))
                        return libname;
                    if (!string.IsNullOrEmpty(name))
                        return name;
                    //if (potentiallibs.Count > 0)
                    //{
                    //    return name + string.Join(",", potentiallibs.Select(x => x.name));
                    //}
                    return address.ToString("x");
                }
            }

            public override string ToString()
            {
                return DisplayName + "()" + (!string.IsNullOrEmpty(notes) ? $"//{notes}" : "") +
                    " funcs:" + calledfunctions.Count +
                    " calledby:" + calledby.Count +
                    " depth:" + maxstack.Count +
                    (hasdebugoutput ? "hasdebug" : "");
            }
        }

        public List<AnalyzedFunction> analyzedfunctions = new List<AnalyzedFunction>();
        List<AnalyzedGlobalVariable> analyzedglobalvariables = new List<AnalyzedGlobalVariable>();
        AnalyzedFunction root = null;


        TreeNode GetNode(AnalyzedFunction func, bool recursive = false)
        {
            var displayname = func.ToString();
            var node = new TreeNode(displayname);
            if (!recursive)
            {
                foreach (var child in func.calledfunctions)
                {
                    bool isrecursive = func.stack.Contains(child);
                    node.Nodes.Add(GetNode(child, isrecursive));
                }
            }
            else
            {
                //its a recursively called function
                //indicate it somehow?
            }


            return node;
        }

        float Compare(AnalyzedFunction func, frmLib.AnalyzedFunction comp)
        {
            var ablocks = func.blocks;
            var bblocks = comp.blocks;
            int hits = 0;
            int misses = 0;
            if (ablocks.Count != bblocks.Count)
                return 0;
            for (int bdex = 0; bdex < ablocks.Count; bdex++)
            {
                var ablock = ablocks[bdex];
                var bblock = bblocks[bdex];
                if (ablock.BlockType != bblock.BlockType)
                    return 0;
                var ainst = ablock.Instructions.Where(x => x.cmd != "nop").OrderBy(x => x.cmd).ToList();
                var binst = bblock.Instructions.Where(x => x.cmd != "nop").OrderBy(x => x.cmd).ToList();
                if (ainst.Count == binst.Count)
                {
                    for (int dex = 0; dex < ainst.Count; dex++)
                    {
                        if (ainst[dex].cmd == binst[dex].cmd)
                        {
                            hits++;
                        }
                        else
                        {
                            misses++;
                        }
                    }
                }
                else
                {
                    return 0;
                }
            }

            float percent = hits / (float)(hits + misses);
            return percent;
        }

        Dictionary<string, List<AnalyzedFunction>> debugStrings = new Dictionary<string, List<AnalyzedFunction>>();
        List<AnalyzedFunction> eventFuncs = new List<AnalyzedFunction>();
        List<AnalyzedFunction> importantFuncs = new List<AnalyzedFunction>();
        private void btnFunctionTracer_Click(object sender, EventArgs e)
        {
            this.Height = 1000;
            var address = (uint)ParseNum(txtOffset.Text);

            address = 0x8db44;//alun_cd.exe entry point
            if (datafile.Contains("startscreen"))
                address = 0x36028;//slus_005.53 entry point (start screen)
            //address = 0x8db44;//alun_cd.exe entry point
            //address = 0x002c038;//main function
            //address = 0x0002c4a4;//inner loop of main function
            var endaddress = (uint)0x0002c518;
            analyzedfunctions = new List<AnalyzedFunction>();
            analyzedglobalvariables = new List<AnalyzedGlobalVariable>();

            root = new AnalyzedFunction(address, analyzedfunctions, analyzedglobalvariables, datafile, AnalyzeFunction, endaddress);

            //do the alundra event funcs too
            //offset = 0x9b5b4;
            eventFuncs = new List<AnalyzedFunction>();
            List<uint> addresses = new List<uint>();
            var fdata = new byte[chunklength];
            var stream = File.OpenRead(datafile);
            stream.Position = 0x9b5b4;
            int numread = stream.Read(fdata, 0, chunklength);
            stream.Close();
            for (int dex = 0; dex <= 0x3FC; dex += 4)
            {
                uint addr = (uint)(fdata[dex] | fdata[dex + 1] << 8 | fdata[dex + 2] << 16 | (uint)fdata[dex + 3] << 24);
                addresses.Add(0xFFFFFFF & addr);
            }

            for (int fdex = 0; fdex < addresses.Count; fdex++)
            {
                var functaddr = addresses[fdex];
                var sicode = Alundra.SpriteInfoEventCodes.GetCode((byte)fdex);

                string fname = $"({sicode.code.ToString("x2")}_{sicode.name}_handler)";

                var efunc = new AnalyzedFunction(functaddr, analyzedfunctions, analyzedglobalvariables, datafile, AnalyzeFunction, 0, fname);
                eventFuncs.Add(efunc);
            }

            //do the sprite event functions
            var seventptrs = new uint[6];
            stream = File.OpenRead(datafile);
            stream.Position = 0x9b554;
            numread = stream.Read(fdata, 0, chunklength);
            stream.Close();
            for (int dex = 0; dex < 6 * 4; dex += 4)
            {
                uint addr = (uint)(fdata[dex] | fdata[dex + 1] << 8 | fdata[dex + 2] << 16 | (uint)fdata[dex + 3] << 24);
                seventptrs[dex / 4] = (0xFFFFFFF & addr);
            }
            for (int sdex = 0; sdex < 6; sdex++)
            {
                if (seventptrs[sdex] == 0)
                    continue;
                var saddresses = new List<uint>();
                stream = File.OpenRead(datafile);
                stream.Position = seventptrs[sdex];
                numread = stream.Read(fdata, 0, chunklength);
                stream.Close();
                for (int dex = 0; dex <= 0x3FC; dex += 4)
                {
                    uint addr = (uint)(fdata[dex] | fdata[dex + 1] << 8 | fdata[dex + 2] << 16 | (uint)fdata[dex + 3] << 24);
                    saddresses.Add(0xFFFFFFF & addr);
                }

                for (int fdex = 0; fdex < saddresses.Count; fdex++)
                {
                    var functaddr = saddresses[fdex];
                    if (functaddr == 0)
                        continue;
                    //var sicode = Alundra.SpriteInfoEventCodes.GetCode((byte)fdex);
                    var sicodename = "";//TODO, add a way to register names for these
                    string eventtypename = "";
                    switch (sdex)
                    {
                        case 0:
                            eventtypename = "eload";
                            break;
                        case 1:
                            continue;
                        case 2:
                            eventtypename = "etick";
                            break;
                        case 3:
                            eventtypename = "etouch";
                            break;
                        case 4:
                            eventtypename = "edeactivate";
                            break;
                        case 5:
                            eventtypename = "einteract";
                            break;
                    }
                    string fname = $"({eventtypename}_{fdex.ToString("x2")}_{sicodename}_handler)";

                    var efunc = new AnalyzedFunction(functaddr, analyzedfunctions, analyzedglobalvariables, datafile, AnalyzeFunction, 0, fname);
                    eventFuncs.Add(efunc);
                }
            }

            importantFuncs = new List<AnalyzedFunction>();
            //add the ui initialize and rendering functions, they are called by register/function pointer so not found  with the function crawler
            importantFuncs.Add(new AnalyzedFunction(0x491a4, analyzedfunctions, analyzedglobalvariables, datafile, AnalyzeFunction));
            importantFuncs.Add(new AnalyzedFunction(0x4c998, analyzedfunctions, analyzedglobalvariables, datafile, AnalyzeFunction));
            importantFuncs.Add(new AnalyzedFunction(0x550d4, analyzedfunctions, analyzedglobalvariables, datafile, AnalyzeFunction));
            importantFuncs.Add(new AnalyzedFunction(0x5c300, analyzedfunctions, analyzedglobalvariables, datafile, AnalyzeFunction));

            importantFuncs.Add(new AnalyzedFunction(0x47de4, analyzedfunctions, analyzedglobalvariables, datafile, AnalyzeFunction));
            importantFuncs.Add(new AnalyzedFunction(0x4d218, analyzedfunctions, analyzedglobalvariables, datafile, AnalyzeFunction));
            importantFuncs.Add(new AnalyzedFunction(0x50bcc, analyzedfunctions, analyzedglobalvariables, datafile, AnalyzeFunction));
            importantFuncs.Add(new AnalyzedFunction(0x518c4, analyzedfunctions, analyzedglobalvariables, datafile, AnalyzeFunction));
            importantFuncs.Add(new AnalyzedFunction(0x54bcc, analyzedfunctions, analyzedglobalvariables, datafile, AnalyzeFunction));
            importantFuncs.Add(new AnalyzedFunction(0x4ba10, analyzedfunctions, analyzedglobalvariables, datafile, AnalyzeFunction));
            importantFuncs.Add(new AnalyzedFunction(0x5695c, analyzedfunctions, analyzedglobalvariables, datafile, AnalyzeFunction));
            importantFuncs.Add(new AnalyzedFunction(0x4c170, analyzedfunctions, analyzedglobalvariables, datafile, AnalyzeFunction));
            importantFuncs.Add(new AnalyzedFunction(0x52584, analyzedfunctions, analyzedglobalvariables, datafile, AnalyzeFunction));
            importantFuncs.Add(new AnalyzedFunction(0x5a1f8, analyzedfunctions, analyzedglobalvariables, datafile, AnalyzeFunction));
            importantFuncs.Add(new AnalyzedFunction(0x52c50, analyzedfunctions, analyzedglobalvariables, datafile, AnalyzeFunction));
            importantFuncs.Add(new AnalyzedFunction(0x5c4ac, analyzedfunctions, analyzedglobalvariables, datafile, AnalyzeFunction));

            //importantFuncs.Add(new AnalyzedFunction(0x2c038))
            foreach (var func in analyzedfunctions)
            {
                //set calledby
                foreach (var testfunc in analyzedfunctions)
                {
                    if (func != testfunc)
                    {
                        if (testfunc.calledfunctions.Contains(func))
                        {
                            func.calledby.Add(testfunc);
                        }
                    }
                }
                func.calledby = func.calledby.OrderBy(x => x.address).ToList();
            }

            var maxstack = root.GetDepth(new List<AnalyzedFunction>());
            foreach (var efunc in eventFuncs)
            {
                efunc.GetDepth(new List<AnalyzedFunction>());
            }

            foreach (var ifunc in importantFuncs)
            {
                ifunc.GetDepth(new List<AnalyzedFunction>());
            }

            analyzedfunctions = analyzedfunctions.OrderBy(x => x.address).ToList();

            lstFunctions.Items.Clear();
            debugStrings = new Dictionary<string, List<AnalyzedFunction>>();
            foreach (var func in analyzedfunctions)
            {


                string fname = "";
                if (!string.IsNullOrEmpty(func.name) || !string.IsNullOrEmpty(func.notes))
                {
                    fname += " (";
                    if (!string.IsNullOrEmpty(func.name))
                        fname += func.name;
                    if (!string.IsNullOrEmpty(func.notes))
                        fname += "//" + func.notes;
                    fname += ")";
                }


                //try to find if its in the lib
                int len = func.instructions.Count;
                func.potentiallibs.Clear();
                if (len > 5)
                {
                    foreach (var testme in libfuncs)
                    {
                        var percent = Compare(func, testme);
                        if (percent > 0.90)
                        {
                            func.potentiallibs.Add(testme);
                        }
                    }
                }
                if (func.potentiallibs.Count > 0)
                {
                    fname += " (" + string.Join(",", func.potentiallibs.Select(x => x.name)) + ")";
                }

                foreach (var dbs in func.debugstrings)
                {
                    List<AnalyzedFunction> dbfuncs;
                    if (!debugStrings.ContainsKey(dbs))
                    {
                        dbfuncs = new List<AnalyzedFunction>();
                        debugStrings.Add(dbs, dbfuncs);
                    }
                    else
                        dbfuncs = debugStrings[dbs];
                    if (!dbfuncs.Contains(func))
                        dbfuncs.Add(func);
                }

                lstFunctions.Items.Add("0x" + func.address.ToString("x") + fname);
            }

            foreach (var gvar in analyzedglobalvariables)
            {
                foreach (var testfunc in analyzedfunctions)
                {
                    if (testfunc.globalvariables.Contains(gvar))
                    {
                        gvar.functions.Add(testfunc);
                    }
                }
            }

            analyzedglobalvariables = analyzedglobalvariables.OrderByDescending(x => x.functions.Count).ToList();

            tvFuncs.Nodes.Clear();

            tvFuncs.Nodes.Add(GetNode(root));
            foreach (var efunc in eventFuncs)
            {
                tvFuncs.Nodes.Add(GetNode(efunc));
            }
            foreach (var ifunc in importantFuncs)
            {
                tvFuncs.Nodes.Add(GetNode(ifunc));
            }

            lstDebugs.Items.Clear();
            foreach (var item in debugStrings.OrderByDescending(x => x.Value.Count))
            {
                lstDebugs.Items.Add(item.Key);
            }

            lstGlobals.Items.Clear();
            foreach (var item in analyzedglobalvariables.OrderByDescending(x => x.functions.Count))
            {
                lstGlobals.Items.Add(item.DisplayName);
            }
        }

        float rot = 0f;
        List<Vector3> points = new List<Vector3> { new Vector3(-10, 10, 0), new Vector3(10, 10, 0), new Vector3(10, -10, 0), new Vector3(-10, -10, 0) };
        private void canvas_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            //camera = Matrix4x4.CreateLookAt(camerapos, camerapos + new Vector3(0,0,1), new Vector3(0, 1, 0));
            //var proj = Matrix4x4.CreatePerspectiveFieldOfView((float)Math.PI / 4f * 1.4f, 9.0f / 6.0f, 1, 30000.0f);
            var rotmat = Matrix4x4.CreateRotationX(rot);
            camera = Matrix4x4.CreateScale(new Vector3(camzoom, camzoom, camzoom));
            camera.Translation = new Vector3(canvas.Width / 2, canvas.Height / 2, 0);

            camera = camera * rotmat;


            float width = canvas.Width / 2;
            float height = canvas.Height / 2;

            if (points != null)
            {
                foreach (var pnt in points)
                {
                    var p1 = Vector3.Transform(pnt, camera);
                    g.DrawLine(Pens.Red, p1.X, p1.Y, p1.X + 1, p1.Y + 1);
                }
            }

        }

        int drawpos = 0;
        float GetSaturnFixedFloat()
        {
            short s = (short)(data[drawpos + 0] << 8 | data[drawpos + 1]);
            drawpos += 2;
            short s2 = (short)(data[drawpos + 0] << 8 | data[drawpos + 1]);
            drawpos += 2;
            return s + (s2 / 65536.0f);
        }

        private void btnDraw_Click(object sender, EventArgs e)
        {
            points = new List<Vector3>();
            var numverts = ParseNum(txtNumVerts.Text);
            drawpos = rtfText.SelectionStart;
            for (var vert = 0; vert < numverts; vert++)
            {
                Vector3 v;
                v.X = GetSaturnFixedFloat();
                v.Y = GetSaturnFixedFloat();
                v.Z = GetSaturnFixedFloat();
                points.Add(v);
            }
            canvas.Refresh();
        }

        private void tvFuncs_DoubleClick(object sender, EventArgs e)
        {
            if (tvFuncs.SelectedNode != null)
            {
                var func = analyzedfunctions.FirstOrDefault(x => x.ToString() == tvFuncs.SelectedNode.Text);
                var frm = new frmAnalyzedFunction(func, datafile);
                frm.Show();
            }
        }

        private void lstDebugs_SelectedIndexChanged(object sender, EventArgs e)
        {
            lstDebugIncludeds.Items.Clear();
            if (lstDebugs.SelectedItem != null)
            {
                var funcs = debugStrings[(string)lstDebugs.SelectedItem];
                foreach (var func in funcs)
                {
                    lstDebugIncludeds.Items.Add(func.ToString());
                }
            }
        }

        private void lstDebugIncludeds_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (lstDebugIncludeds.SelectedItem != null)
            {
                var func = analyzedfunctions.FirstOrDefault(x => x.ToString() == (string)lstDebugIncludeds.SelectedItem);
                var frm = new frmAnalyzedFunction(func, datafile);
                frm.Show();
            }
        }

        private void lstGlobals_SelectedIndexChanged(object sender, EventArgs e)
        {
            lstGlobalIncludeds.Items.Clear();
            if (lstGlobals.SelectedItem != null)
            {
                var gvar = analyzedglobalvariables.FirstOrDefault(x => x.DisplayName == (string)lstGlobals.SelectedItem);

                foreach (var func in gvar.functions)
                {
                    lstGlobalIncludeds.Items.Add(func.ToString());
                }
                foreach (var asn in gvar.assignments)
                {
                    lstGlobalIncludeds.Items.Add(asn.func.ToString() + " * = " + asn.rightstring);
                }
            }
        }

        private void lstGlobalIncludeds_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (lstGlobalIncludeds.SelectedItem != null)
            {
                var functext = (string)lstGlobalIncludeds.SelectedItem;
                if (functext.Contains(" * ="))
                    functext = functext.Substring(0, functext.IndexOf(" * ="));
                var func = analyzedfunctions.FirstOrDefault(x => x.ToString() == functext);
                var frm = new frmAnalyzedFunction(func, datafile);
                frm.Show();
            }
        }

        private void chkSortGlobal_CheckedChanged(object sender, EventArgs e)
        {
            lstGlobals.Items.Clear();
            var list = analyzedglobalvariables.OrderByDescending(x => x.functions.Count);
            if (chkSortGlobal.Checked)
                list = analyzedglobalvariables.OrderBy(x => x.address);
            foreach (var item in list)
            {
                lstGlobals.Items.Add(item.DisplayName);
            }
        }

        private void btnUtility_Click(object sender, EventArgs e)
        {


            string s = "short[] DirectionTable = new short[]{\r\n";
            int dex = 0;

            for (int y = 0; y < 16; y++)
            {
                string line = "";
                for (int x = 0; x < 16; x++)
                {
                    line += "0x" + (data[dex + 0] + (data[dex + 1] << 8)).ToString("x1") + ",";
                    dex += 2;
                }
                s += line + "\r\n";
            }

            s += "};";
            Clipboard.SetText(s);

            s = "uint[] DivTable = new uint[]{\r\n";
            dex = 0;

            for (int i = 0; i < 29; i++)
            {
                string line = "0x" + (data[dex + 0] + (data[dex + 1] << 8) + (data[dex + 2] << 16) + (data[dex + 3] << 24)).ToString("x8") + ",";
                dex += 4;
                s += line + "\r\n";
            }

            s += "};";
            Clipboard.SetText(s);


            s = "int[] FrameDexTable = new int[]{\r\n";
            dex = 0;

            for (int i = 0; i < 32; i++)
            {
                string line = "0x" + (data[dex + 0] + (data[dex + 1] << 8) + (data[dex + 2] << 16) + (data[dex + 3] << 24)).ToString("x8") + ",";
                dex += 4;
                s += line + "\r\n";
            }

            s += "};";
            Clipboard.SetText(s);

            s = "int[] FrameDexTable = new int[]{\r\n";
            dex = 0;
            for (int i = 0; i < 255; i++)
            {
                string line = "0x" + (data[dex + 0] + (data[dex + 1] << 8) + (data[dex + 2] << 16) + (data[dex + 3] << 24)).ToString("x8") + ",//0x" + i.ToString("x2");
                dex += 4;
                s += line + "\r\n";
            }
            s += "};";
            Clipboard.SetText(s);

            /*s = "byte[][] ContentsTable = new byte[][]{\r\n";
            dex = 0;
            for (int i = 0; i < 0x3c2; i++)
            {
                string line = "new byte[]{";
                for (int i2=0;i2<22;i2++)
                {
                    line += "0x" + data[dex + i2].ToString("x2") + ",";
                }
                dex += 22;
                s += line + "},\r\n";
            }
            s += "};";
            Clipboard.SetText(s);*/

            s = "cmds = new {\r\n";
            dex = 0;
            for (int i = 0; i < 255 * 2; i++)
            {

                var cmd = new Alundra.UIDrawCmd();
                cmd.u = data[dex + 0xc];
                cmd.v = data[dex + 0xd];
                var addr = data[dex + 0xe] | data[dex + 0xf] << 8;
                cmd.uipaletteindex = (short)((addr - 0x7812) / 64);
                string line = $"new UIDrawCmd{{ u = 0x{cmd.u.ToString("x")}, v = 0x{cmd.v.ToString("x")}, w = 8, h = 8, uipaletteindex = {cmd.uipaletteindex}}},";
                dex += 20;
                s += line + "\r\n";
            }
            s += "}";
            Clipboard.SetText(s);
            /*
                        s = "infos = new {\r\n";
                        dex = 0;
                        for (int i = 0; i < 128; i++)
                        {

                            int[] vals= new int[5];
                            for(int sdex=0; sdex<5;sdex++)
                            {
                                vals[sdex] = data[dex + 3] << 24 | data[dex + 2] << 16 | data[dex + 1] << 8 | data[dex + 0];
                                dex += 4;
                            }
                            string line = $"new FontCharInfo{{ width = 0x{vals[0].ToString("x")}, height = 0x{vals[1].ToString("x")}, sx = 0x{vals[3].ToString("x")}, sy = 0x{vals[3].ToString("x")}, y = 0x{vals[4].ToString("x")}}},//0x{i.ToString("x")}";
                            s += line + "\r\n";
                        }
                        s += "}";
                        Clipboard.SetText(s);

                        s = "string [] StringTable = new string[] {\r\n";
                        dex = 0;

                        int soffset = data[dex + 3] << 24 | data[dex + 2] << 16 | data[dex + 1] << 8 | data[dex + 0];
                        while(soffset >> 31 == 1)
                        {
                            soffset = soffset & 0xffffff;
                            int diff = soffset - offset;
                            if (diff <= 0)
                                break;
                            StringBuilder sb = new StringBuilder();
                            char chr = (char)data[diff++];
                            while(chr != 0)
                            {
                                sb.Append(chr);
                                chr = (char)data[diff++];
                            }
                            s += $"\"{sb.ToString()}\",\r\n";
                            dex += 4;
                            soffset = data[dex + 3] << 24 | data[dex + 2] << 16 | data[dex + 1] << 8 | data[dex + 0];
                        }
                        s += "}";
                        Clipboard.SetText(s);
                        */
        }

        private void btnFuncContainsAddr_Click(object sender, EventArgs e)
        {
            var addr = ParseNum(txtOffset.Text);
            foreach (var func in analyzedfunctions)
            {
                if (func.address < addr && (func.address + func.length) > addr)
                {
                    var frm = new frmAnalyzedFunction(func, datafile);
                    frm.Show();
                    break;
                }
            }
        }

        private void tvFuncs_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (tvFuncs.SelectedNode != null)
            {
                var func = analyzedfunctions.FirstOrDefault(x => x.ToString() == tvFuncs.SelectedNode.Text);
                if (func.name.Contains("_handler"))
                {
                    if (func.name.Contains("eload_"))
                    {

                    }
                }
            }
        }
    }
}
