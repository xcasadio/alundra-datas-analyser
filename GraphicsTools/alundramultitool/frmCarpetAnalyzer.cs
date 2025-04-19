using alundramultitool;

namespace GraphicsTools
{
    public partial class FrmCarpetAnalyzer : Form
    {
        public FrmCarpetAnalyzer()
        {
            InitializeComponent();
            Alundra.DebugSymbols.Init();
        }

        public string Datafile;

        public int Offset;
        public int Memaddress;
        public int Startoffset = 0;
        public int Startsize = 0;
        int _chunklength = 1024*1024;
        byte[] _data;

        int ParseNum(string num)
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

        private void frmFileAnalyzer_Load(object sender, EventArgs e)
        {
            Loadchunk(Offset);
        }

        int _instOffset = 0;
        List<IsInstruction> _instructions = new();
        List<uint> _functions = new();
        void Loadchunk(int offset)
        {
            Text = "frmCarpetAnalyzer: " + Datafile + " : " + offset.ToString();
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
            for (var dex = 0; dex < 0x6000; dex+=4)
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
                    fname = " (" + Alundra.DebugSymbols.FunctionNames[functaddr] + ")";
                }

                lstFunctions.Items.Add("0x" + functaddr.ToString("x8") + fname);
            }
        }

        void DisplayData(int pos)
        {
            var addrOffset = 0;
            int.TryParse(txtAddressOffset.Text, out addrOffset);
            txtOffset.Text = Offset.ToString();
            lblCursorOffset.Text = (pos + Offset).ToString() + "(" + (pos + Offset + addrOffset).ToString("x6") + ")";
            lblRelOffset.Text = pos.ToString();
            lblSelLength.Text = rtfText.SelectionLength.ToString();

            lbl8bit.Text = _data[pos].ToString() + " (" + _data[pos].ToString("x2") + ")"; ;
            long l = _data[pos + 1] | _data[pos] << 8;
            lbl16bit.Text = l.ToString() + " (" + l.ToString("x4") + ")";
            l = _data[pos+3] | _data[pos + 2] << 8 | _data[pos + 1] << 16 | (long)_data[pos + 0] << 24;
            lbl32bit.Text = l.ToString() + " (" + l.ToString("x8") + ")";

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

        FrmViewer _viewer;
        FrmViewer _pal;

        private void btnViewImage_Click(object sender, EventArgs e)
        {
            int stride, width, height,startx,starty, bpp;
            int.TryParse(txtStride.Text, out stride);
            int.TryParse(txtWidth.Text, out width);
            int.TryParse(txtHeight.Text, out height);
            int.TryParse(txtStartx.Text, out startx);
            int.TryParse(txtStarty.Text, out starty);
            int.TryParse(txtBpp.Text, out bpp);

            var imagestart = rtfText.SelectionStart;

            var imagedata = new byte[width*height*bpp/8];

            if (stride == -1)
            {
                //compressed
                var imagedex = 0;
                var buffdex = imagestart;
                while(imagedex < imagedata.Length)
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
                throw new Exception("no palette specified");
                /*for (int dex = 0; dex < palettes.Length; dex++)
                {
                    int ddex = imagestart + paloffset + dex * 2;
                    palettes[dex] = Utils.FromPsxColor(data[ddex + 1], data[dex]);// Color.FromArgb(255, (data[ddex + 1] & 0x1f) << 3, ((data[ddex + 1] & 0xe0) >> 2) | ((data[ddex] & 0x3) << 6), data[ddex] & 0x7c);
                }*/
            }
            _viewer = new FrmViewer();
            _viewer.Show();
            _viewer.Init(imagedata,24,bpp, width, height, palettes);
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

            var palbpp = 24;
            var bpp = 8;
            var imagedata = new byte[width * height * palbpp / 8];
            for (var y = 0; y < height; y++)
            {
                Buffer.BlockCopy(_data, palettestart + y * stride, imagedata, y * width * palbpp / 8, width * palbpp / 8);

            }
            var frm = new FrmViewer();
            frm.Show();
            frm.Initpalette(_viewer, imagedata,palbpp,bpp, width, height);
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
            for (var dex = rtfText.SelectionStart+1; dex < _data.Length - 3; dex++)
            {
                long num = _data[dex] | _data[dex + 1] << 8 | _data[dex + 2] << 16 | _data[dex + 3] << 24;
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
                AnalyzeFunction();
            }
        }

        void AnalyzeFunction()
        {
            //string ftext = "0x";
            var refs = _selectedFunction.Select(x => x.ReferencedAddress).Where(x => x != 0).ToList();
            
            var fnames = Alundra.DebugSymbols.FunctionNames;
            var evars = Alundra.DebugSymbols.EntityVarOffsets;

            var blocks = new List<CodeBlock<IsInstruction>>();

            var block = new CodeBlock<IsInstruction>();

            for (var dex = 0; dex < _selectedFunction.Count; dex++)
            {
                var inst = _selectedFunction[dex];
                if (refs.Contains(inst.Address))
                {
                    if (block.Instructions.Count > 0)
                    {
                        block.BlockType = BlockType.FallThrough;
                        block.OutAddresses.Add(inst.Address);
                        blocks.Add(block);
                        block = new CodeBlock<IsInstruction>();
                    }
                    //ftext += "\r\n0x";
                }
                if (inst.IsCall)
                {
                    block.BlockType = BlockType.Call;
                    block.OutAddresses.Add(inst.Address + 8);
                    block.Instructions.Add(inst);
                    dex++;
                    inst = _selectedFunction[dex];
                    block.Instructions.Add(inst);
                    blocks.Add(block);
                    block = new CodeBlock<IsInstruction>();
                }
                else if (inst.IsReturn)
                {
                    block.BlockType = BlockType.Return;
                    block.Instructions.Add(inst);
                    dex++;
                    if (dex < _selectedFunction.Count)
                    {
                        inst = _selectedFunction[dex];
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
                    inst = _selectedFunction[dex];
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
                    inst = _selectedFunction[dex];
                    block.Instructions.Add(inst);
                    blocks.Add(block);
                    block = new CodeBlock<IsInstruction>();
                }
                else
                {
                    block.Instructions.Add(inst);
                }

                /*string ccode = "";
                if (fnames.ContainsKey(inst.address))
                    ccode = "void " + fnames[inst.address] + "()";
                else
                {
                    switch(inst.cmd)
                    {
                        case "jal":
                            if (fnames.ContainsKey(inst.referencedAddress))
                                ccode = fnames[inst.referencedAddress];
                            else
                                ccode = inst.referencedAddress.ToString("x");
                            ccode += "()";
                            break;
                        case "sw":
                        case "lw":
                            if (inst.rs != 29)//if not local variable declaration
                            {
                                //assume entity struct
                                if (inst.immediate > 0 && inst.immediate < evars.Length)
                                {
                                    if (!string.IsNullOrEmpty(evars[inst.immediate]))
                                    {
                                        if (inst.cmd == "lw")
                                            ccode = GetRegister(inst.rt) + " = " + GetRegister(inst.rs) + "." + evars[inst.immediate];
                                        else if (inst.cmd == "sw")
                                            ccode = GetRegister(inst.rs) + "." + evars[inst.immediate] + " = " + GetRegister(inst.rt);
                                        break;
                                    }
                                }
                                if (inst.cmd == "lw")
                                    ccode = GetRegister(inst.rt) + " = " + GetRegister(inst.rs) + "[" + inst.immediate.ToString("x") + "]";
                                else if (inst.cmd == "sw")
                                    ccode = GetRegister(inst.rs) + "[" + inst.immediate.ToString("x") + "]" + " = " + GetRegister(inst.rt);
                            }
                            break;
                    }
                }

                ftext += string.Format("{0}: {1} {2}\t\t{3}\r\n", inst.address.ToString("x8"), inst.instruction.ToString("x8"), inst.display, ccode);
                */
            }

            foreach (var b in blocks)
            {
                b.OutEdges = b.OutAddresses.Select(x => blocks.Where(y => y.Address == x).FirstOrDefault()).ToList();
                b.InEdges = blocks.Where(x => x.OutAddresses.Contains(b.Address)).ToList();
            }

            //txtFunction.Text = ftext;
        }

    }
}
