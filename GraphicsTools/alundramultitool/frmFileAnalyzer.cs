using System.Text;
using alundramultitool;
using System.Numerics;

namespace GraphicsTools
{
    public partial class FrmFileAnalyzer : Form
    {
        public FrmFileAnalyzer()
        {
            InitializeComponent();
            Alundra.DebugSymbols.Init();
            canvas.MouseWheel += Canvas_MouseWheel;
            canvas.MouseDown += Canvas_MouseDown;
            canvas.MouseMove += Canvas_MouseMove;
        }

        public void Initialize(string psyqSdkFolder)
        {
            _psyqSdkFolder = psyqSdkFolder;
        }

        private void Canvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                var moved = e.Location.Y - _lastpos.Y;
                _rot += 0.1f * moved;
                _lastpos = e.Location;
                canvas.Refresh();
            }
        }

        Point _lastpos;

        private void Canvas_MouseDown(object sender, MouseEventArgs e)
        {
            _lastpos = e.Location;
        }

        float _camzoom = 1;
        private void Canvas_MouseWheel(object sender, MouseEventArgs e)
        {
            _camzoom += 0.5f * (e.Delta > 0 ? 1 : -1);
            canvas.Refresh();
        }

        Vector3 _camerapos = new(0, -100, 0);
        Vector3 _cameratarget = new(0, 0, 0);
        Matrix4x4 _camera; //= Matrix4x4.CreateLookAt(camerapos, cameratarget, new Vector3(0, 1, 0));

        public string Datafile;

        public int Offset;
        public int Memaddress;
        public int Startoffset = 0;
        public int Startsize = 0;
        int _chunklength = 1024 * 1024;
        byte[] _data;

        public int ParseNum(string num)
        {
            var i = 0;
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

        List<FrmLib.AnalyzedFunction> _libfuncs;
        private void frmFileAnalyzer_Load(object sender, EventArgs e)
        {
            Loadchunk(Offset);
            var frm = new FrmLib(_psyqSdkFolder, this);
            frm.Show();
            _libfuncs = frm.Analyzedfuncs;
        }

        int _instOffset = 0;
        List<IsInstruction> _instructions = new();
        List<uint> _functions = new();
        void Loadchunk(int offset, bool alundraeventlist = false)
        {
            txtAddressOffset.Text = (Memaddress - offset).ToString();
            Offset = offset;
            _data = new byte[_chunklength];
            var stream = File.OpenRead(Datafile);
            stream.Position = offset;
            var numread = stream.Read(_data, 0, _chunklength);
            stream.Close();
            rtfText.LoadFile(new MemoryStream(_data), RichTextBoxStreamType.PlainText);
            rtfText.Select(Startoffset, Startsize);

            _instOffset = ParseNum(txtInstructionsOffset.Text);
            _instructions.Clear();
            lstInstructions.Items.Clear();
            lstFunctions.Items.Clear();
            var mips = new Mips();
            if (alundraeventlist)
            {
                //offset = 0x9b5b4;
                _functions.Clear();
                for (var dex = 0; dex <= 0x3FC; dex += 4)
                {
                    var addr = (uint)(_data[dex] | _data[dex + 1] << 8 | _data[dex + 2] << 16 | (uint)_data[dex + 3] << 24);
                    _functions.Add(0xFFFFFFF & addr);
                }

                for (var fdex = 0; fdex < _functions.Count; fdex++)
                {
                    var functaddr = _functions[fdex];
                    var sicode = Alundra.SpriteInfoEventCodes.GetCode((byte)fdex);

                    var fname = $"({sicode.Code.ToString("x2")}_{sicode.Name}_handler)";

                    lstFunctions.Items.Add("0x" + functaddr.ToString("x") + fname);
                }
            }
            else
            {
                for (var dex = 0; dex < 0x6000; dex += 4)
                {
                    var inst = new Mips.Instruction((uint)(_instOffset + offset + dex), (uint)(_data[dex] | _data[dex + 1] << 8 | _data[dex + 2] << 16 | (uint)_data[dex + 3] << 24));
                    if (inst.Cmd == "jal")
                    {
                        _functions.Add(inst.ReferencedAddress);
                    }

                    _instructions.Add(inst);
                    lstInstructions.Items.Add(string.Format("{0}:  {1}", inst.Address.ToString("x8"), inst.Display));
                }
                foreach (var functaddr in _functions.Distinct().OrderBy(x => x))
                {
                    var fname = "";
                    if (Alundra.DebugSymbols.FunctionNames.ContainsKey(functaddr))
                    {
                        fname += "(";
                        var fref = Alundra.DebugSymbols.FunctionNames[functaddr];
                        if (!string.IsNullOrEmpty(fref.Name))
                        {
                            fname += fref.Name;
                        }

                        if (!string.IsNullOrEmpty(fref.Comment))
                        {
                            fname += "//" + fref.Comment;
                        }

                        fname += ")";
                    }
                    lstFunctions.Items.Add("0x" + functaddr.ToString("x8") + fname);
                }
            }
        }

        bool _flip = false;
        void DisplayData(int pos)
        {
            var addrOffset = 0;
            int.TryParse(txtAddressOffset.Text, out addrOffset);
            txtOffset.Text = Offset.ToString();
            lblCursorOffset.Text = (pos + Offset).ToString() + "(" + (pos + Offset + addrOffset).ToString("x6") + ")";
            lblRelOffset.Text = pos.ToString();
            lblSelLength.Text = rtfText.SelectionLength.ToString();

            lbl8bit.Text = _data[pos].ToString() + " (" + _data[pos].ToString("x2") + ")"; ;
            long l = _data[pos] | _data[pos + 1] << 8;
            if (_flip)
            {
                l = _data[pos + 1] | _data[pos] << 8;
            }

            lbl16bit.Text = l.ToString() + " (" + l.ToString("x4") + ")";
            l = _data[pos] | _data[pos + 1] << 8 | _data[pos + 2] << 16 | (long)_data[pos + 3] << 24;
            if (_flip)
            {
                l = _data[pos + 3] | _data[pos + 2] << 8 | _data[pos + 1] << 16 | (long)_data[pos + 0] << 24;
            }

            lbl32bit.Text = l.ToString() + " (" + l.ToString("x8") + ")";

            var s = (short)l;
            var f = s * 360f / 65536f;
            lblFloat16.Text = f.ToString();
            var s1 = (short)(_data[pos + 1] | _data[pos] << 8);
            var s2 = (ushort)(_data[pos + 3] | _data[pos + 2] << 8);
            f = s1 + s2 / 65536.0f;
            lblFloat.Text = f.ToString();
            lbl4bita.Text = ((_data[pos] & 0xf0) >> 4).ToString();
            lbl4bitb.Text = (_data[pos] & 0xf).ToString();
        }

        private void rtfText_SelectionChanged(object sender, EventArgs e)
        {
            if (rtfText.SelectionStart >= 0)
            {
                DisplayData(rtfText.SelectionStart);
            }
        }



        private void btnViewImage_Click(object sender, EventArgs e)
        {
            int stride, width, height, startx, starty;
            int.TryParse(txtStride.Text, out stride);
            int.TryParse(txtWidth.Text, out width);
            int.TryParse(txtHeight.Text, out height);
            int.TryParse(txtStartx.Text, out startx);
            int.TryParse(txtStarty.Text, out starty);

            var imagestart = rtfText.SelectionStart;

            var bpp = 4;
            var imagedata = new byte[width * height * bpp / 8];

            if (stride == -1)
            {
                //compressed
                var imagedex = 0;
                var buffdex = imagestart;
                while (imagedex < imagedata.Length)
                {
                    var b = _data[buffdex++];
                    if (b == 0xad)
                    {
                        int seek = _data[buffdex++];
                        if (seek == 0)
                        {
                            imagedata[imagedex++] = b;
                        }
                        else
                        {
                            int len = _data[buffdex++];
                            var seekdex = imagedex - seek;
                            while (len-- > 0)
                                imagedata[imagedex++] = imagedata[seekdex++];
                        }
                    }
                    else
                    {
                        imagedata[imagedex++] = b;
                    }
                }
            }
            else
            {
                for (var y = 0; y < height; y++)
                {
                    Buffer.BlockCopy(_data, imagestart + (y + starty) * stride + startx / 2, imagedata, y * width * bpp / 8, width * bpp / 8);
                }
            }

            var paloffset = 0;
            var palettes = new Color[(int)Math.Pow(2, bpp)];
            if (Program.Palette != null)
            {
                palettes = Program.Palette;
            }
            else
            {
                for (var dex = 0; dex < palettes.Length; dex++)
                {
                    var ddex = imagestart + paloffset + dex * 2;
                    palettes[dex] = Utils.FromPsxColor(_data[ddex + 1], _data[dex]);// Color.FromArgb(255, (data[ddex + 1] & 0x1f) << 3, ((data[ddex + 1] & 0xe0) >> 2) | ((data[ddex] & 0x3) << 6), data[ddex] & 0x7c);
                }
            }
            Program.Viewer = new FrmViewer();
            Program.Viewer.Show();
            Program.Viewer.Init(imagedata, 16, 4, width, height, palettes);
        }

        private void btnJump_Click(object sender, EventArgs e)
        {
            Offset = ParseNum(txtOffset.Text);
            Loadchunk(Offset);
        }



        private void btnViewPal_Click(object sender, EventArgs e)
        {
            int stride, width, height;
            int.TryParse(txtStride.Text, out stride);
            int.TryParse(txtWidth.Text, out width);
            int.TryParse(txtHeight.Text, out height);

            var palettestart = rtfText.SelectionStart;

            var bpp = 16;
            var imagedata = new byte[width * height * bpp / 8];
            for (var y = 0; y < height; y++)
            {
                Buffer.BlockCopy(_data, palettestart + y * stride, imagedata, y * width * bpp / 8, width * bpp / 8);
            }

            var frm = new FrmViewer();
            frm.Show();
            frm.InitPalette(Program.Viewer, imagedata, 16, 4, width, height);
        }

        private void rtfText_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (rtfText.SelectionStart >= 0)
            {
                lstInstructions.SelectedIndex = rtfText.SelectionStart / 4;

            }
        }



        private void btnFindInt32_Click(object sender, EventArgs e)
        {
            long search = ParseNum(txtSearch.Text);
            var range = ParseNum(txtSearchRange.Text);
            for (var dex = rtfText.SelectionStart + 1; dex < _data.Length - 3; dex++)
            {
                long num = _data[dex] | _data[dex + 1] << 8 | _data[dex + 2] << 16 | _data[dex + 3] << 24;
                if (_flip)
                {
                    num = _data[dex + 3] | _data[dex + 2] << 8 | _data[dex + 1] << 16 | _data[dex + 0] << 24;
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

        private void frmFileAnalyzer_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F3)
            {
                btnFindInt32_Click(null, null);
            }
        }

        private void frmFindInt16_Click(object sender, EventArgs e)
        {
            long search = ParseNum(txtSearch.Text);
            var range = ParseNum(txtSearchRange.Text);
            for (var dex = rtfText.SelectionStart + 1; dex < _data.Length - 3; dex++)
            {
                long num = _data[dex] | _data[dex + 1] << 8;// | data[dex + 2] << 16 | data[dex + 3] << 24;
                if (_flip)
                {
                    num = _data[dex + 1] | _data[dex + 0] << 8;
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
        List<IsInstruction> _selectedFunction = null;
        private void lstFunctions_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstFunctions.SelectedIndex >= 0)
            {
                var address = ParseNum(lstFunctions.Text.Split('(')[0].Trim());
                var fdat = new byte[_chunklength];
                var stream = File.OpenRead(Datafile);
                stream.Position = address;
                var numread = stream.Read(fdat, 0, _chunklength);
                stream.Close();

                var exit = false;

                _selectedFunction = new List<IsInstruction>();

                for (var dex = 0; dex < 10000; dex += 4)
                {
                    var inst = new Mips.Instruction((uint)(address + dex), (uint)(fdat[dex] | fdat[dex + 1] << 8 | fdat[dex + 2] << 16 | (uint)fdat[dex + 3] << 24));
                    _selectedFunction.Add(inst);
                    if (exit)
                    {
                        break;
                    }

                    if (inst.IsReturn)
                    {
                        exit = true;
                    }
                }
                var blocks = AnalyzeFunction(_selectedFunction);
                txtFunction.Text = PrintFunction(blocks);

            }
        }

        public static string PrintFunction(List<CodeBlock<IsInstruction>> blocks)
        {
            var ftext = "";

            var fnames = Alundra.DebugSymbols.FunctionNames;
            var evars = Alundra.DebugSymbols.EntityVarOffsets;
            var globalvars = Alundra.DebugSymbols.GlobalVariableNames;
            var comments = Alundra.DebugSymbols.Comments;

            var indentlevel = 0;
            var codestart = 40;
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
                    var ccode = "";
                    if (fnames.ContainsKey(inst.Address))
                    {
                        ccode = "void " + fnames[inst.Address].Name + "()";
                        if (!string.IsNullOrEmpty(fnames[inst.Address].Comment))
                        {
                            ccode += "//" + fnames[inst.Address].Comment;
                        }
                    }
                    else
                    {
                        switch (inst.Cmd)
                        {
                            case "jal":
                                var sname = inst.ReferencedAddress.ToString("x");
                                var scomment = "";
                                if (fnames.ContainsKey(inst.ReferencedAddress))
                                {
                                    var fref = fnames[inst.ReferencedAddress];
                                    if (!string.IsNullOrEmpty(fref.Name))
                                    {
                                        sname = fref.Name;
                                    }

                                    if (!string.IsNullOrEmpty(fref.Comment))
                                    {
                                        scomment = fref.Comment;
                                    }
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
                                if (inst.Rs != 29)//if not local variable declaration
                                {
                                    var nudgewierdness = false;
                                    //if (inst.rt == 16 || inst.rs == 16)
                                    //    nudgewierdness = true;
                                    if (nudgewierdness)
                                    {
                                        inst.Immediate += 0x134;
                                    }

                                    //assume entity struct
                                    if (inst.Immediate > 0xc && inst.Immediate < evars.Length)
                                    {

                                        if (!string.IsNullOrEmpty(evars[inst.Immediate]))
                                        {

                                            if (inst.Cmd == "lw" || inst.Cmd == "lh" || inst.Cmd == "lhu" || inst.Cmd == "lb" || inst.Cmd == "lbu")
                                            {
                                                ccode = Mips.GetRegister(inst.Rt) + " = " + Mips.GetRegister(inst.Rs) + "." + evars[inst.Immediate];
                                            }
                                            else if (inst.Cmd == "sw" || inst.Cmd == "sh" || inst.Cmd == "shu" || inst.Cmd == "sb" || inst.Cmd == "sbu")
                                            {
                                                ccode = Mips.GetRegister(inst.Rs) + "." + evars[inst.Immediate] + " = " + Mips.GetRegister(inst.Rt);
                                            }

                                            if (nudgewierdness)
                                            {
                                                inst.Immediate -= 0x134;
                                            }

                                            break;
                                        }

                                    }

                                    var fulladdr2 = inst.GetGlobalVariable(block);
                                    if (fulladdr2 > 0)
                                    {
                                        var name2 = "0x" + fulladdr2.ToString("x");
                                        if (globalvars.ContainsKey(fulladdr2))
                                        {
                                            name2 = globalvars[fulladdr2].Name;
                                        }

                                        if (fulladdr2 > 0x1ac498 && fulladdr2 < 0x1ac498 + 0x294)
                                        {
                                            name2 = "playercharacter";
                                            var off = fulladdr2 - 0x1ac498;
                                            var evarname = evars[off];
                                            if (!string.IsNullOrEmpty(evarname))
                                            {
                                                name2 += "." + evarname;
                                            }
                                            else
                                            {
                                                name2 += "[" + off.ToString("x") + "]";
                                            }
                                        }
                                        if (inst.GlobalRegisterOffset > 0)
                                        {
                                            name2 += "[" + Mips.GetRegister(inst.GlobalRegisterOffset) + "]";
                                        }

                                        if (inst.Cmd == "lw" || inst.Cmd == "lh" || inst.Cmd == "lhu" || inst.Cmd == "lb" || inst.Cmd == "lbu")
                                        {
                                            ccode = Mips.GetRegister(inst.Rt) + " = *" + name2;
                                        }
                                        else if (inst.Cmd == "sw" || inst.Cmd == "sh" || inst.Cmd == "shu" || inst.Cmd == "sb" || inst.Cmd == "sbu")
                                        {
                                            ccode = "*" + name2 + " = " + Mips.GetRegister(inst.Rt);
                                        }
                                    }
                                    else
                                    {
                                        if (inst.Cmd == "lw" || inst.Cmd == "lh" || inst.Cmd == "lhu" || inst.Cmd == "lb" || inst.Cmd == "lbu")
                                        {
                                            ccode = Mips.GetRegister(inst.Rt) + " = " + Mips.GetRegister(inst.Rs) + "[" + inst.Immediate.ToString("x") + "]";
                                        }
                                        else if (inst.Cmd == "sw" || inst.Cmd == "sh" || inst.Cmd == "shu" || inst.Cmd == "sb" || inst.Cmd == "sbu")
                                        {
                                            ccode = Mips.GetRegister(inst.Rs) + "[" + inst.Immediate.ToString("x") + "]" + " = " + Mips.GetRegister(inst.Rt);
                                        }
                                    }
                                    if (nudgewierdness)
                                    {
                                        inst.Immediate -= 0x134;
                                    }
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
                                    {
                                        ccode = "//" + globalvars[fulladdr].Name;
                                    }
                                    else if (fulladdr > 0x1ac498 && fulladdr < 0x1ac498 + 0x294)
                                    {
                                        var name2 = "playercharacter";
                                        var off = fulladdr - 0x1ac498;
                                        var evarname = evars[off];
                                        if (!string.IsNullOrEmpty(evarname))
                                        {
                                            name2 += "." + evarname;
                                        }
                                        else
                                        {
                                            name2 += "[" + off.ToString("x") + "]";
                                        }

                                        ccode = "//" + name2;
                                    }
                                    else
                                    {
                                        ccode = "//0x" + fulladdr.ToString("x");
                                    }
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
                    if (comments.ContainsKey(inst.Address))
                    {
                        ccode += "//" + comments[inst.Address];
                    }

                    var asm = string.Format("{0}: {1} {2}", (block.Instructions.IndexOf(inst) == 0 && block.IsJumpTarget ? "0x" : "") + inst.Address.ToString("x8"), inst.Instruction.ToString("x8"), inst.Display);
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
            var indent = "";
            for (var dex = 0; dex < indentlevel; dex++)
            {
                indent += "  ";
            }
            return indent;
        }

        static string RawIndent(int indentlevel)
        {
            var indent = "";
            for (var dex = 0; dex < indentlevel; dex++)
            {
                indent += " ";
            }
            return indent;
        }

        public static List<CodeBlock<IsInstruction>> AnalyzeFunction(List<IsInstruction> function)
        {
            var refs = function.Select(x => x.ReferencedAddress).Where(x => x != 0).ToList();


            var blocks = new List<CodeBlock<IsInstruction>>();

            var block = new CodeBlock<IsInstruction>();

            for (var dex = 0; dex < function.Count; dex++)
            {
                var inst = function[dex];
                if (refs.Contains(inst.Address))
                {
                    if (block.Instructions.Count > 0)
                    {
                        block.BlockType = BlockType.FallThrough;
                        block.OutAddresses.Add(inst.Address);
                        blocks.Add(block);
                        block = new CodeBlock<IsInstruction>();
                    }
                }
                if (inst.IsCall)
                {
                    block.BlockType = BlockType.Call;
                    block.OutAddresses.Add(inst.Address + 8);
                    block.Instructions.Add(inst);
                    dex++;
                    inst = function[dex];
                    block.Instructions.Add(inst);
                    blocks.Add(block);
                    block = new CodeBlock<IsInstruction>();
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
                    block.OutAddresses.Add(inst.ReferencedAddress);
                    block.OutAddresses.Add(inst.Address + 8);
                    block.Instructions.Add(inst);
                    dex++;
                    inst = function[dex];
                    block.Instructions.Add(inst);
                    blocks.Add(block);
                    block = new CodeBlock<IsInstruction>();
                }
                else if (inst.IsJump)
                {
                    block.BlockType = BlockType.OneWay;
                    block.OutAddresses.Add(inst.ReferencedAddress);
                    block.Instructions.Add(inst);
                    dex++;
                    inst = function[dex];
                    block.Instructions.Add(inst);
                    blocks.Add(block);
                    block = new CodeBlock<IsInstruction>();
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
            Offset = ParseNum(txtOffset.Text);
            Loadchunk(Offset, true);
        }

        private void btnAlundraEventFuncs_Click(object sender, EventArgs e)
        {
            lstInstructions.Width -= 100;
            lstFunctions.Left -= 100;
            lstFunctions.Width += 20;
            txtFunction.Left -= 80;
            txtFunction.Width += 10;
            Offset = 0x9b5b4;
            Loadchunk(Offset, true);
        }


        public class AnalyzedGlobalVariable
        {
            public AnalyzedGlobalVariable(uint addr, List<AnalyzedGlobalVariable> varlist)
            {
                varlist.Add(this);
                var globalvars = Alundra.DebugSymbols.GlobalVariableNames;
                Address = addr;
                if (globalvars.ContainsKey(Address))
                {
                    Name = globalvars[Address].Name;
                }

                if (addr > 0x1ac498 && addr < 0x1ac498 + 0x294)
                {
                    //if (name!=null)
                    //{
                    //    string s = "testc";
                    //}
                    Name = "playercharacter";
                    var off = addr - 0x1ac498;
                    var evarname = Alundra.DebugSymbols.EntityVarOffsets[off];
                    if (!string.IsNullOrEmpty(evarname))
                    {
                        Name += "." + evarname;
                    }
                    else
                    {
                        Name += "[" + off.ToString("x") + "]";
                    }
                }
            }
            public uint Address;
            public string Name;
            public string Notes;
            public List<AnalyzedFunction> Functions = new();
            public List<VariableAssignment> Assignments = new();

            public string DisplayName
            {
                get
                {
                    if (!string.IsNullOrEmpty(Name))
                    {
                        return Name + "(" + Address.ToString("x") + ")";
                    }

                    return Address.ToString("x");
                }
            }

            public override string ToString()
            {
                return DisplayName + "[" + Functions.Count + "]";
            }
        }

        public class VariableAssignment
        {
            public AnalyzedFunction Func;
            public AnalyzedGlobalVariable Left;
            public AnalyzedGlobalVariable Right;
            public string Rightstring;
        }
        public class AnalyzedFunction
        {
            public AnalyzedFunction(uint addr, List<AnalyzedFunction> funclist, List<AnalyzedGlobalVariable> varlist, string datafile, Func<List<IsInstruction>, List<CodeBlock<IsInstruction>>> analyzeFunction, uint endaddr = 0, string fname = null)
            {
                var fnames = Alundra.DebugSymbols.FunctionNames;
                var evars = Alundra.DebugSymbols.EntityVarOffsets;
                var globalvars = Alundra.DebugSymbols.GlobalVariableNames;
                var chunklength = 1024 * 1024;

                funclist.Add(this);
                Address = addr;
                if (fname != null)
                {
                    Name = fname;
                }
                else if (fnames.ContainsKey(Address))
                {
                    Name = fnames[Address].Name;
                    Notes = fnames[Address].Comment;
                }



                var fdat = new byte[chunklength];
                var stream = File.OpenRead(datafile);
                stream.Position = Address;
                var numread = stream.Read(fdat, 0, chunklength);
                stream.Close();

                var exit = false;

                var function = new List<IsInstruction>();
                Instructions = function;

                for (var dex = 0; dex < 10000; dex += 4)
                {
                    var inst = new Mips.Instruction((uint)(Address + dex), (uint)(fdat[dex] | fdat[dex + 1] << 8 | fdat[dex + 2] << 16 | (uint)fdat[dex + 3] << 24));
                    function.Add(inst);
                    if (exit)
                    {
                        break;
                    }

                    if (inst.IsReturn || (endaddr != 0 && inst.Address == endaddr))
                    {
                        exit = true;
                    }
                }
                Length = (int)(function.Last().Address - Address);
                Blocks = analyzeFunction(function);

                var debugnames = new[] { "outputdebuginfo", "printdebug", "printdebugparams", "printdebugerror" };

                foreach (var block in Blocks)
                {
                    foreach (var inst in block.Instructions)
                    {
                        switch (inst.Cmd)
                        {
                            case "jal":
                                var calledfunc = funclist.FirstOrDefault(x => x.Address == inst.ReferencedAddress);
                                if (calledfunc == null)
                                {
                                    calledfunc = new AnalyzedFunction(inst.ReferencedAddress, funclist, varlist, datafile, analyzeFunction);
                                }
                                if (!Calledfunctions.Contains(calledfunc))
                                {
                                    Calledfunctions.Add(calledfunc);
                                }

                                if (debugnames.Contains(calledfunc.Name))
                                {
                                    var mdex = block.Instructions.IndexOf(inst);
                                    var seekback = 10;
                                    for (var dex = mdex + 1; dex >= mdex - (1 + seekback) && dex >= 0; dex--)
                                    {
                                        var reg = 4;
                                        if (calledfunc.Name == "printdebugerror")
                                        {
                                            reg = 5;
                                        }

                                        var tinst = (Mips.Instruction)block.Instructions[dex];
                                        if (tinst.Type == Mips.InstructionType.Itype && tinst.Rt == reg)
                                        {
                                            var spos = tinst.GetGlobalVariable(block);
                                            if (spos > 0 && spos < 0x80000)
                                            {
                                                var sstream = File.OpenRead(datafile);
                                                sstream.Position = spos;
                                                var buff = new byte[1024];
                                                sstream.Read(buff, 0, 1024);
                                                var sb = new StringBuilder();
                                                for (var sdex = 0; sdex < 1024; sdex++)
                                                {
                                                    if (buff[sdex] == 0)
                                                    {
                                                        break;
                                                    }

                                                    sb.Append((char)buff[sdex]);

                                                }
                                                Debugstrings.Add(sb.ToString());
                                            }
                                            else
                                            {
                                                var s = "why";
                                            }
                                            break;
                                        }
                                    }
                                }

                                break;
                            case "jalr":
                                Callsfunctionpointers = true;
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
                                    var gvar = varlist.FirstOrDefault(x => x.Address == fulladdr);
                                    if (gvar == null)
                                    {
                                        gvar = new AnalyzedGlobalVariable(fulladdr, varlist);
                                    }
                                    if (!Globalvariables.Contains(gvar))
                                    {
                                        Globalvariables.Add(gvar);
                                    }
                                }


                                //if its an assignment, get the left and right operands
                                if (inst.IsAssignment)
                                {
                                    uint fulladr;
                                    string right;
                                    inst.GetAssignmentGlobals(out fulladr, out right, block);
                                    if (fulladr != 0)
                                    {
                                        var gvar = varlist.FirstOrDefault(x => x.Address == fulladr);
                                        if (gvar != null)
                                        {
                                            var assn = new VariableAssignment { Func = this, Left = gvar, Rightstring = right };
                                            gvar.Assignments.Add(assn);
                                        }
                                    }
                                }
                                break;


                        }

                    }
                    if (block.EndsLoop)
                    {
                        Hasloop = true;
                    }
                }

                foreach (var gvar in Globalvariables)
                {
                    if (gvar.Address >= 0x29f30 && gvar.Address <= 0x2aaba)
                    {
                        //its in the range of some string variables
                        var sstream = File.OpenRead(datafile);
                        sstream.Position = gvar.Address;
                        var buff = new byte[1024];
                        sstream.Read(buff, 0, 1024);
                        var sb = new StringBuilder();
                        for (var sdex = 0; sdex < 1024; sdex++)
                        {
                            if (buff[sdex] == 0)
                            {
                                break;
                            }

                            sb.Append((char)buff[sdex]);

                        }
                        var toadd = sb.ToString();
                        if (!Debugstrings.Contains(toadd))
                        {
                            Debugstrings.Add(toadd);
                        }

                        Hasdebugoutput = true;
                    }
                }

                if (Calledfunctions.Any(x => debugnames.Contains(x.Name)))
                {
                    Hasdebugoutput = true;
                }
            }
            public List<IsInstruction> Instructions;
            public List<CodeBlock<IsInstruction>> Blocks;
            public uint Address;
            public string Libname;
            public List<FrmLib.AnalyzedFunction> Potentiallibs = new();
            public string Addressstring { get { return Address.ToString("x"); } }
            public int Length;
            public string Name;
            public string Notes;
            public List<string> Parameters = new();
            public string Returnval;
            public List<AnalyzedFunction> Calledfunctions = new();
            public List<AnalyzedGlobalVariable> Globalvariables = new();
            public List<AnalyzedFunction> Calledby = new();
            public bool Callsfunctionpointers = false;
            public bool Hasloop = false;
            public bool Hasdebugoutput = false;
            public List<string> Debugstrings = new();

            public List<AnalyzedFunction> Stack = new();
            public List<AnalyzedFunction> Maxstack = new();

            public List<AnalyzedFunction> GetDepth(List<AnalyzedFunction> depthstack)
            {
                Stack = depthstack;
                Maxstack = Stack;
                //detect recursion
                if (depthstack.Contains(this))
                {
                    return depthstack;
                }

                depthstack.Add(this);


                foreach (var func in Calledfunctions)
                {
                    var potentialstack = func.GetDepth(depthstack.ToList());
                    if (potentialstack.Count > Maxstack.Count)
                    {
                        Maxstack = potentialstack;
                    }
                }

                return Maxstack;
            }

            public string DisplayName
            {
                get
                {

                    if (!string.IsNullOrEmpty(Libname))
                    {
                        return Libname;
                    }

                    if (!string.IsNullOrEmpty(Name))
                    {
                        return Name;
                    }

                    //if (potentiallibs.Count > 0)
                    //{
                    //    return name + string.Join(",", potentiallibs.Select(x => x.name));
                    //}
                    return Address.ToString("x");
                }
            }

            public override string ToString()
            {
                return DisplayName + "()" + (!string.IsNullOrEmpty(Notes) ? $"//{Notes}" : "") +
                    " funcs:" + Calledfunctions.Count +
                    " calledby:" + Calledby.Count +
                    " depth:" + Maxstack.Count +
                    (Hasdebugoutput ? "hasdebug" : "");
            }
        }

        public List<AnalyzedFunction> Analyzedfunctions = new();
        List<AnalyzedGlobalVariable> _analyzedglobalvariables = new();
        AnalyzedFunction _root = null;


        TreeNode GetNode(AnalyzedFunction func, bool recursive = false)
        {
            var displayname = func.ToString();
            var node = new TreeNode(displayname);
            if (!recursive)
            {
                foreach (var child in func.Calledfunctions)
                {
                    var isrecursive = func.Stack.Contains(child);
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

        float Compare(AnalyzedFunction func, FrmLib.AnalyzedFunction comp)
        {
            var ablocks = func.Blocks;
            var bblocks = comp.Blocks;
            var hits = 0;
            var misses = 0;
            if (ablocks.Count != bblocks.Count)
            {
                return 0;
            }

            for (var bdex = 0; bdex < ablocks.Count; bdex++)
            {
                var ablock = ablocks[bdex];
                var bblock = bblocks[bdex];
                if (ablock.BlockType != bblock.BlockType)
                {
                    return 0;
                }

                var ainst = ablock.Instructions.Where(x => x.Cmd != "nop").OrderBy(x => x.Cmd).ToList();
                var binst = bblock.Instructions.Where(x => x.Cmd != "nop").OrderBy(x => x.Cmd).ToList();
                if (ainst.Count == binst.Count)
                {
                    for (var dex = 0; dex < ainst.Count; dex++)
                    {
                        if (ainst[dex].Cmd == binst[dex].Cmd)
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

            var percent = hits / (float)(hits + misses);
            return percent;
        }

        Dictionary<string, List<AnalyzedFunction>> _debugStrings = new();
        List<AnalyzedFunction> _eventFuncs = new();
        List<AnalyzedFunction> _importantFuncs = new();
        private void btnFunctionTracer_Click(object sender, EventArgs e)
        {
            Height = 1000;
            var address = (uint)ParseNum(txtOffset.Text);

            address = 0x8db44;//alun_cd.exe entry point
            if (Datafile.Contains("startscreen"))
            {
                address = 0x36028;//slus_005.53 entry point (start screen)
            }

            //address = 0x8db44;//alun_cd.exe entry point
            //address = 0x002c038;//main function
            //address = 0x0002c4a4;//inner loop of main function
            var endaddress = (uint)0x0002c518;
            Analyzedfunctions = new List<AnalyzedFunction>();
            _analyzedglobalvariables = new List<AnalyzedGlobalVariable>();

            _root = new AnalyzedFunction(address, Analyzedfunctions, _analyzedglobalvariables, Datafile, AnalyzeFunction, endaddress);

            //do the alundra event funcs too
            //offset = 0x9b5b4;
            _eventFuncs = new List<AnalyzedFunction>();
            var addresses = new List<uint>();
            var fdata = new byte[_chunklength];
            var stream = File.OpenRead(Datafile);
            stream.Position = 0x9b5b4;
            var numread = stream.Read(fdata, 0, _chunklength);
            stream.Close();
            for (var dex = 0; dex <= 0x3FC; dex += 4)
            {
                var addr = (uint)(fdata[dex] | fdata[dex + 1] << 8 | fdata[dex + 2] << 16 | (uint)fdata[dex + 3] << 24);
                addresses.Add(0xFFFFFFF & addr);
            }

            for (var fdex = 0; fdex < addresses.Count; fdex++)
            {
                var functaddr = addresses[fdex];
                var sicode = Alundra.SpriteInfoEventCodes.GetCode((byte)fdex);

                var fname = $"({sicode.Code.ToString("x2")}_{sicode.Name}_handler)";

                var efunc = new AnalyzedFunction(functaddr, Analyzedfunctions, _analyzedglobalvariables, Datafile, AnalyzeFunction, 0, fname);
                _eventFuncs.Add(efunc);
            }

            //do the sprite event functions
            var seventptrs = new uint[6];
            stream = File.OpenRead(Datafile);
            stream.Position = 0x9b554;
            numread = stream.Read(fdata, 0, _chunklength);
            stream.Close();
            for (var dex = 0; dex < 6 * 4; dex += 4)
            {
                var addr = (uint)(fdata[dex] | fdata[dex + 1] << 8 | fdata[dex + 2] << 16 | (uint)fdata[dex + 3] << 24);
                seventptrs[dex / 4] = 0xFFFFFFF & addr;
            }
            for (var sdex = 0; sdex < 6; sdex++)
            {
                if (seventptrs[sdex] == 0)
                {
                    continue;
                }

                var saddresses = new List<uint>();
                stream = File.OpenRead(Datafile);
                stream.Position = seventptrs[sdex];
                numread = stream.Read(fdata, 0, _chunklength);
                stream.Close();
                for (var dex = 0; dex <= 0x3FC; dex += 4)
                {
                    var addr = (uint)(fdata[dex] | fdata[dex + 1] << 8 | fdata[dex + 2] << 16 | (uint)fdata[dex + 3] << 24);
                    saddresses.Add(0xFFFFFFF & addr);
                }

                for (var fdex = 0; fdex < saddresses.Count; fdex++)
                {
                    var functaddr = saddresses[fdex];
                    if (functaddr == 0)
                    {
                        continue;
                    }

                    //var sicode = Alundra.SpriteInfoEventCodes.GetCode((byte)fdex);
                    var sicodename = "";//TODO, add a way to register names for these
                    var eventtypename = "";
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
                    var fname = $"({eventtypename}_{fdex.ToString("x2")}_{sicodename}_handler)";

                    var efunc = new AnalyzedFunction(functaddr, Analyzedfunctions, _analyzedglobalvariables, Datafile, AnalyzeFunction, 0, fname);
                    _eventFuncs.Add(efunc);
                }
            }

            _importantFuncs = new List<AnalyzedFunction>();
            //add the ui initialize and rendering functions, they are called by register/function pointer so not found  with the function crawler
            _importantFuncs.Add(new AnalyzedFunction(0x491a4, Analyzedfunctions, _analyzedglobalvariables, Datafile, AnalyzeFunction));
            _importantFuncs.Add(new AnalyzedFunction(0x4c998, Analyzedfunctions, _analyzedglobalvariables, Datafile, AnalyzeFunction));
            _importantFuncs.Add(new AnalyzedFunction(0x550d4, Analyzedfunctions, _analyzedglobalvariables, Datafile, AnalyzeFunction));
            _importantFuncs.Add(new AnalyzedFunction(0x5c300, Analyzedfunctions, _analyzedglobalvariables, Datafile, AnalyzeFunction));

            _importantFuncs.Add(new AnalyzedFunction(0x47de4, Analyzedfunctions, _analyzedglobalvariables, Datafile, AnalyzeFunction));
            _importantFuncs.Add(new AnalyzedFunction(0x4d218, Analyzedfunctions, _analyzedglobalvariables, Datafile, AnalyzeFunction));
            _importantFuncs.Add(new AnalyzedFunction(0x50bcc, Analyzedfunctions, _analyzedglobalvariables, Datafile, AnalyzeFunction));
            _importantFuncs.Add(new AnalyzedFunction(0x518c4, Analyzedfunctions, _analyzedglobalvariables, Datafile, AnalyzeFunction));
            _importantFuncs.Add(new AnalyzedFunction(0x54bcc, Analyzedfunctions, _analyzedglobalvariables, Datafile, AnalyzeFunction));
            _importantFuncs.Add(new AnalyzedFunction(0x4ba10, Analyzedfunctions, _analyzedglobalvariables, Datafile, AnalyzeFunction));
            _importantFuncs.Add(new AnalyzedFunction(0x5695c, Analyzedfunctions, _analyzedglobalvariables, Datafile, AnalyzeFunction));
            _importantFuncs.Add(new AnalyzedFunction(0x4c170, Analyzedfunctions, _analyzedglobalvariables, Datafile, AnalyzeFunction));
            _importantFuncs.Add(new AnalyzedFunction(0x52584, Analyzedfunctions, _analyzedglobalvariables, Datafile, AnalyzeFunction));
            _importantFuncs.Add(new AnalyzedFunction(0x5a1f8, Analyzedfunctions, _analyzedglobalvariables, Datafile, AnalyzeFunction));
            _importantFuncs.Add(new AnalyzedFunction(0x52c50, Analyzedfunctions, _analyzedglobalvariables, Datafile, AnalyzeFunction));
            _importantFuncs.Add(new AnalyzedFunction(0x5c4ac, Analyzedfunctions, _analyzedglobalvariables, Datafile, AnalyzeFunction));

            //importantFuncs.Add(new AnalyzedFunction(0x2c038))
            foreach (var func in Analyzedfunctions)
            {
                //set calledby
                foreach (var testfunc in Analyzedfunctions)
                {
                    if (func != testfunc)
                    {
                        if (testfunc.Calledfunctions.Contains(func))
                        {
                            func.Calledby.Add(testfunc);
                        }
                    }
                }
                func.Calledby = func.Calledby.OrderBy(x => x.Address).ToList();
            }

            var maxstack = _root.GetDepth(new List<AnalyzedFunction>());
            foreach (var efunc in _eventFuncs)
            {
                efunc.GetDepth(new List<AnalyzedFunction>());
            }

            foreach (var ifunc in _importantFuncs)
            {
                ifunc.GetDepth(new List<AnalyzedFunction>());
            }

            Analyzedfunctions = Analyzedfunctions.OrderBy(x => x.Address).ToList();

            lstFunctions.Items.Clear();
            _debugStrings = new Dictionary<string, List<AnalyzedFunction>>();
            foreach (var func in Analyzedfunctions)
            {


                var fname = "";
                if (!string.IsNullOrEmpty(func.Name) || !string.IsNullOrEmpty(func.Notes))
                {
                    fname += " (";
                    if (!string.IsNullOrEmpty(func.Name))
                    {
                        fname += func.Name;
                    }

                    if (!string.IsNullOrEmpty(func.Notes))
                    {
                        fname += "//" + func.Notes;
                    }

                    fname += ")";
                }


                //try to find if its in the lib
                var len = func.Instructions.Count;
                func.Potentiallibs.Clear();
                if (len > 5)
                {
                    foreach (var testme in _libfuncs)
                    {
                        var percent = Compare(func, testme);
                        if (percent > 0.90)
                        {
                            func.Potentiallibs.Add(testme);
                        }
                    }
                }
                if (func.Potentiallibs.Count > 0)
                {
                    fname += " (" + string.Join(",", func.Potentiallibs.Select(x => x.Name)) + ")";
                }

                foreach (var dbs in func.Debugstrings)
                {
                    List<AnalyzedFunction> dbfuncs;
                    if (!_debugStrings.ContainsKey(dbs))
                    {
                        dbfuncs = new List<AnalyzedFunction>();
                        _debugStrings.Add(dbs, dbfuncs);
                    }
                    else
                    {
                        dbfuncs = _debugStrings[dbs];
                    }

                    if (!dbfuncs.Contains(func))
                    {
                        dbfuncs.Add(func);
                    }
                }

                lstFunctions.Items.Add("0x" + func.Address.ToString("x") + fname);
            }

            foreach (var gvar in _analyzedglobalvariables)
            {
                foreach (var testfunc in Analyzedfunctions)
                {
                    if (testfunc.Globalvariables.Contains(gvar))
                    {
                        gvar.Functions.Add(testfunc);
                    }
                }
            }

            _analyzedglobalvariables = _analyzedglobalvariables.OrderByDescending(x => x.Functions.Count).ToList();

            tvFuncs.Nodes.Clear();

            tvFuncs.Nodes.Add(GetNode(_root));
            foreach (var efunc in _eventFuncs)
            {
                tvFuncs.Nodes.Add(GetNode(efunc));
            }
            foreach (var ifunc in _importantFuncs)
            {
                tvFuncs.Nodes.Add(GetNode(ifunc));
            }

            lstDebugs.Items.Clear();
            foreach (var item in _debugStrings.OrderByDescending(x => x.Value.Count))
            {
                lstDebugs.Items.Add(item.Key);
            }

            lstGlobals.Items.Clear();
            foreach (var item in _analyzedglobalvariables.OrderByDescending(x => x.Functions.Count))
            {
                lstGlobals.Items.Add(item.DisplayName);
            }
        }

        float _rot = 0f;
        List<Vector3> _points = new() { new Vector3(-10, 10, 0), new Vector3(10, 10, 0), new Vector3(10, -10, 0), new Vector3(-10, -10, 0) };
        private void canvas_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            //camera = Matrix4x4.CreateLookAt(camerapos, camerapos + new Vector3(0,0,1), new Vector3(0, 1, 0));
            //var proj = Matrix4x4.CreatePerspectiveFieldOfView((float)Math.PI / 4f * 1.4f, 9.0f / 6.0f, 1, 30000.0f);
            var rotmat = Matrix4x4.CreateRotationX(_rot);
            _camera = Matrix4x4.CreateScale(new Vector3(_camzoom, _camzoom, _camzoom));
            _camera.Translation = new Vector3(canvas.Width / 2, canvas.Height / 2, 0);

            _camera = _camera * rotmat;


            float width = canvas.Width / 2;
            float height = canvas.Height / 2;

            if (_points != null)
            {
                foreach (var pnt in _points)
                {
                    var p1 = Vector3.Transform(pnt, _camera);
                    g.DrawLine(Pens.Red, p1.X, p1.Y, p1.X + 1, p1.Y + 1);
                }
            }

        }

        int _drawpos = 0;
        private string _psyqSdkFolder;

        float GetSaturnFixedFloat()
        {
            var s = (short)(_data[_drawpos + 0] << 8 | _data[_drawpos + 1]);
            _drawpos += 2;
            var s2 = (short)(_data[_drawpos + 0] << 8 | _data[_drawpos + 1]);
            _drawpos += 2;
            return s + s2 / 65536.0f;
        }

        private void btnDraw_Click(object sender, EventArgs e)
        {
            _points = new List<Vector3>();
            var numverts = ParseNum(txtNumVerts.Text);
            _drawpos = rtfText.SelectionStart;
            for (var vert = 0; vert < numverts; vert++)
            {
                Vector3 v;
                v.X = GetSaturnFixedFloat();
                v.Y = GetSaturnFixedFloat();
                v.Z = GetSaturnFixedFloat();
                _points.Add(v);
            }
            canvas.Refresh();
        }

        private void tvFuncs_DoubleClick(object sender, EventArgs e)
        {
            if (tvFuncs.SelectedNode != null)
            {
                var func = Analyzedfunctions.FirstOrDefault(x => x.ToString() == tvFuncs.SelectedNode.Text);
                var frm = new FrmAnalyzedFunction(func, Datafile);
                frm.Show();
            }
        }

        private void lstDebugs_SelectedIndexChanged(object sender, EventArgs e)
        {
            lstDebugIncludeds.Items.Clear();
            if (lstDebugs.SelectedItem != null)
            {
                var funcs = _debugStrings[(string)lstDebugs.SelectedItem];
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
                var func = Analyzedfunctions.FirstOrDefault(x => x.ToString() == (string)lstDebugIncludeds.SelectedItem);
                var frm = new FrmAnalyzedFunction(func, Datafile);
                frm.Show();
            }
        }

        private void lstGlobals_SelectedIndexChanged(object sender, EventArgs e)
        {
            lstGlobalIncludeds.Items.Clear();
            if (lstGlobals.SelectedItem != null)
            {
                var gvar = _analyzedglobalvariables.FirstOrDefault(x => x.DisplayName == (string)lstGlobals.SelectedItem);

                foreach (var func in gvar.Functions)
                {
                    lstGlobalIncludeds.Items.Add(func.ToString());
                }
                foreach (var asn in gvar.Assignments)
                {
                    lstGlobalIncludeds.Items.Add(asn.Func.ToString() + " * = " + asn.Rightstring);
                }
            }
        }

        private void lstGlobalIncludeds_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (lstGlobalIncludeds.SelectedItem != null)
            {
                var functext = (string)lstGlobalIncludeds.SelectedItem;
                if (functext.Contains(" * ="))
                {
                    functext = functext.Substring(0, functext.IndexOf(" * ="));
                }

                var func = Analyzedfunctions.FirstOrDefault(x => x.ToString() == functext);
                var frm = new FrmAnalyzedFunction(func, Datafile);
                frm.Show();
            }
        }

        private void chkSortGlobal_CheckedChanged(object sender, EventArgs e)
        {
            lstGlobals.Items.Clear();
            var list = _analyzedglobalvariables.OrderByDescending(x => x.Functions.Count);
            if (chkSortGlobal.Checked)
            {
                list = _analyzedglobalvariables.OrderBy(x => x.Address);
            }

            foreach (var item in list)
            {
                lstGlobals.Items.Add(item.DisplayName);
            }
        }

        private void btnUtility_Click(object sender, EventArgs e)
        {


            var s = "short[] DirectionTable = new short[]{\r\n";
            var dex = 0;

            for (var y = 0; y < 16; y++)
            {
                var line = "";
                for (var x = 0; x < 16; x++)
                {
                    line += "0x" + (_data[dex + 0] + (_data[dex + 1] << 8)).ToString("x1") + ",";
                    dex += 2;
                }
                s += line + "\r\n";
            }

            s += "};";
            Clipboard.SetText(s);

            s = "uint[] DivTable = new uint[]{\r\n";
            dex = 0;

            for (var i = 0; i < 29; i++)
            {
                var line = "0x" + (_data[dex + 0] + (_data[dex + 1] << 8) + (_data[dex + 2] << 16) + (_data[dex + 3] << 24)).ToString("x8") + ",";
                dex += 4;
                s += line + "\r\n";
            }

            s += "};";
            Clipboard.SetText(s);


            s = "int[] FrameDexTable = new int[]{\r\n";
            dex = 0;

            for (var i = 0; i < 32; i++)
            {
                var line = "0x" + (_data[dex + 0] + (_data[dex + 1] << 8) + (_data[dex + 2] << 16) + (_data[dex + 3] << 24)).ToString("x8") + ",";
                dex += 4;
                s += line + "\r\n";
            }

            s += "};";
            Clipboard.SetText(s);

            s = "int[] FrameDexTable = new int[]{\r\n";
            dex = 0;
            for (var i = 0; i < 255; i++)
            {
                var line = "0x" + (_data[dex + 0] + (_data[dex + 1] << 8) + (_data[dex + 2] << 16) + (_data[dex + 3] << 24)).ToString("x8") + ",//0x" + i.ToString("x2");
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
            for (var i = 0; i < 255 * 2; i++)
            {

                var cmd = new Alundra.UiDrawCmd();
                cmd.U = _data[dex + 0xc];
                cmd.V = _data[dex + 0xd];
                var addr = _data[dex + 0xe] | _data[dex + 0xf] << 8;
                cmd.Uipaletteindex = (short)((addr - 0x7812) / 64);
                var line = $"new UIDrawCmd{{ u = 0x{cmd.U.ToString("x")}, v = 0x{cmd.V.ToString("x")}, w = 8, h = 8, uipaletteindex = {cmd.Uipaletteindex}}},";
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
            foreach (var func in Analyzedfunctions)
            {
                if (func.Address < addr && func.Address + func.Length > addr)
                {
                    var frm = new FrmAnalyzedFunction(func, Datafile);
                    frm.Show();
                    break;
                }
            }
        }

        private void tvFuncs_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (tvFuncs.SelectedNode != null)
            {
                var func = Analyzedfunctions.FirstOrDefault(x => x.ToString() == tvFuncs.SelectedNode.Text);
                if (func.Name.Contains("_handler"))
                {
                    if (func.Name.Contains("eload_"))
                    {

                    }
                }
            }
        }
    }
}
