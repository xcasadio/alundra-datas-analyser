using System.Text;
using AlundraTools.Decompiler;
using AlundraTools.Decompiler.LibModule;
using AlundraTools.GameControls;

namespace AlundraTools
{
    public partial class FrmLib : Form
    {
        Lib _lib;
        string[] _hlines;
        public List<AnalyzedFunction> Analyzedfuncs = new();
        FrmFileAnalyzer _sister;
        string _libFile;
        List<Lib> _libs = new();

        public FrmLib(string psyqSdkFolder, FrmFileAnalyzer sister)
        {
            _sister = sister;
            InitializeComponent();

            var libFile = Path.Combine(psyqSdkFolder, "LIB", "LIBSND.LIB");
            var hFile = Path.Combine(psyqSdkFolder, "INCLUDE", "LIBSND.H");
            var libdir = Path.Combine(psyqSdkFolder, "LIB");
            var includdir = Path.Combine(psyqSdkFolder, "INCLUDE");

            var otherdefs = new List<string>();
            foreach(var file in Directory.GetFiles(includdir, "*.H"))
            {
                var id = Path.GetFileNameWithoutExtension(file);
                if (!id.StartsWith("LIB"))
                {
                    otherdefs.AddRange(File.ReadAllLines(file));
                }
            }

            foreach(var file in Directory.GetFiles(libdir,"*.LIB"))
            {
                var id = Path.GetFileNameWithoutExtension(file);
                var hf = Path.Combine(includdir, id  + ".H");
                var lib = new Lib(file, hf, otherdefs);
                _libs.Add(lib);
            }

            _libFile = libFile;
            _lib = new Lib(libFile, hFile, null);

            _hlines = File.ReadAllLines(hFile);

            foreach(var mod in _lib.Modules)
            {
                foreach(var sym in mod.Link.Symbols)
                {
                    if (sym.Type == SymbolType.Internal)
                    {
                        var hdef = FindDef(sym);
                        var func = new AnalyzedFunction(sym, mod, FrmFileAnalyzer.AnalyzeFunction, hdef);
                        Analyzedfuncs.Add(func);
                    }
                    else if (sym.Type == SymbolType.Local)
                    {
                        var section = mod.Link.Sections.FirstOrDefault(x => x.Symbol == sym.Section);
                        if (section.Patches.Exists(x=>x.Value == sym.Offset && x.PatchType == PatchType.SectionBase && x.RelocType == RelocType.FunctionCall))
                        {
                            var hdef = FindDef(sym);
                            var func = new AnalyzedFunction(sym, mod, FrmFileAnalyzer.AnalyzeFunction, hdef);
                            Analyzedfuncs.Add(func);
                        }
                    }
                }
            }

            foreach (var lib in _libs)
            {
                foreach(var mod in lib.Modules)
                {
                    foreach(var sym in mod.Link.Symbols)
                    {
                        AnalyzedFunction func = null;
                        if (sym.Type == SymbolType.Internal)
                        {
                            var section = mod.Link.Sections.FirstOrDefault(x => x.Symbol == sym.Section);
                            if (lib.ExportedFunctions.Contains(mod.Header.ModuleName + " : "+ sym.Name) || section.Patches.Exists(x => x.Symbol == sym.Sym && x.RelocType == RelocType.FunctionCall))
                            {
                                func = new AnalyzedFunction(sym, mod, FrmFileAnalyzer.AnalyzeFunction, null);
                            }
                            
                        }
                        else if (sym.Type == SymbolType.Local)
                        {
                            var section = mod.Link.Sections.FirstOrDefault(x => x.Symbol == sym.Section);
                            if (section.Patches.Exists(x => x.Value == sym.Offset && x.PatchType == PatchType.SectionBase && x.RelocType == RelocType.FunctionCall))
                            {
                                func = new AnalyzedFunction(sym, mod, FrmFileAnalyzer.AnalyzeFunction, null);
                            }
                        }
                        if (func!=null)
                        {
                            Fullanalyzedfuncs.Add(func);
                            if (lib.ExportedFunctions.Contains(mod.Header.ModuleName + " : " + sym.Name))
                            {
                                Exportedanalyzedfuncs.Add(func);
                            }
                        }
                    }
                }
            }

            for(var dex = 0;dex<Fullanalyzedfuncs.Count;dex++)
            {
                var func = Fullanalyzedfuncs[dex];

                if (func is UnknownAnalyzedFunction)
                {
                    continue;
                }

                foreach(var sym in func.Calledsymbols)
                {
                    var found = false;
                    if (sym.Type == SymbolType.Local)
                    {
                        foreach(var checkme in Fullanalyzedfuncs.Where(x=>x.Module == sym.Mod))
                        {
                            if (checkme.Name == sym.Name)
                            {
                                func.Calledfunctions.Add(checkme);
                                found = true;
                            }
                        }
                    }
                    else if (sym.Type == SymbolType.Internal)
                    {
                        foreach (var checkme in Fullanalyzedfuncs.Where(x => x.Module == sym.Mod))
                        {
                            if (checkme.Name == sym.Name)
                            {
                                func.Calledfunctions.Add(checkme);
                                found = true;
                            }
                        }
                    }
                    else
                    {
                        foreach (var checkme in Exportedanalyzedfuncs)
                        {
                            if (checkme.Name == sym.Name)
                            {
                                func.Calledfunctions.Add(checkme);
                                found = true;
                            }
                        }
                        //should i also try full list
                    }
                    if (!found)
                    {
                        
                        foreach (var checkme in Fullanalyzedfuncs)
                        {
                            if (checkme.Name == sym.Name)
                            {
                                func.Calledfunctions.Add(checkme);
                                found = true;
                            }
                        }
                    }
                    if (!found)
                    {
                        var unanalyzed = new UnknownAnalyzedFunction(sym.Name);
                        func.Calledfunctions.Add(unanalyzed);
                        Fullanalyzedfuncs.Add(unanalyzed);
                    }
                }
            }
            foreach (var func in Fullanalyzedfuncs)
            {
                func.GetDepth(new List<AnalyzedFunction>());
            }

            foreach (var lib in _libs)
            {
                lstLibs.Items.Add(lib);
            }
        }
        public List<AnalyzedFunction> Fullanalyzedfuncs = new();
        public List<AnalyzedFunction> Exportedanalyzedfuncs = new();

        string FindDef(Symbol sym)
        {
            for (var dex = 0;dex<_hlines.Length;dex++)
            {
                if (_hlines[dex].Replace(" (","(").Contains(" " + sym.Name + "("))
                {
                    return _hlines[dex].Trim().Replace(";", "");
                }
            }
            return null;
        }

        private void frmLib_Load(object sender, EventArgs e)
        {
            foreach (var func in Analyzedfuncs)
                lstFuncs.Items.Add(func);

            foreach(var module in _lib.Modules)
            {
                lstModules.Items.Add(module.Header.ModuleName);
            }
        }

        public static string PrintFunction(List<CodeBlock<IsInstruction>> blocks, Link link, Section section)
        {
            var ftext = "";
            if (blocks.Count == 0)
            {
                return "";
            }

            var addradjust = blocks.First().Instructions.First().Address;

            var fnames = DebugSymbols.FunctionNames;
            var evars = DebugSymbols.EntityVarOffsets;
            var globalvars = DebugSymbols.GlobalVariableNames;
            var comments = DebugSymbols.Comments;

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
                                {
                                    var patch = section.Patches.FirstOrDefault(x =>
                                        //(x.Value == 0 && x.Offset == inst.address)
                                        //|| (x.Value != 0 && x.Value == inst.address)
                                        x.Offset == inst.Address
                                    );// (x.PatchType == PATCH_TYPE.REF && x.Offset + addradjust == inst.address) || x.Offset + addradjust == inst.address);
                                    if (patch != null)
                                    {
                                        var sym = link.Symbols.FirstOrDefault(x => x.Sym == patch.Symbol || (patch.PatchType == PatchType.SectionBase && x.Offset == patch.Value));
                                        if (sym != null)
                                        {
                                            sname = sym.Name;
                                        }

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
                                    /*if (inst.immediate > 0xc && inst.immediate < evars.Length)
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

                                    }*/

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
                                        //var evarname = evars[off];
                                        //if (!string.IsNullOrEmpty(evarname))
                                        //    name2 += "." + evarname;
                                        //else
                                            name2 += "[" + off.ToString("x") + "]";
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

        public class UnknownAnalyzedFunction : AnalyzedFunction
        {
            public UnknownAnalyzedFunction(string name):
                base(null,null,null,null)
            {
                Name = name;
                Fullname = name + " (unalyzed)";
            }

            
        }

        public class AnalyzedFunction
        {
            public string Name;
            public string Fullname;
            public int Length;
            public bool Hasloop;
            public bool Callsfunctionpointers;
            public List<AnalyzedFunction> Calledfunctions = new();
            public List<Symbol> Calledsymbols = new();
            public List<AnalyzedFunction> Calledby = new();
            public List<IsInstruction> Instructions;
            public List<CodeBlock<IsInstruction>> Blocks;
            public LibModule Module;
            public Section Section;

            public override string ToString()
            {
                return Fullname;
            }
            public AnalyzedFunction(Symbol symb, LibModule module, Func<List<IsInstruction>, List<CodeBlock<IsInstruction>>> analyzeFunction, string hdef)
            {
                if (symb == null)
                {
                    return;
                }

                Module = module;
                var link = module.Link;
                Section = link.Sections.FirstOrDefault(x => x.Symbol == symb.Section);
                var fdat = Section.Code;
                Name = symb.Name;
                if (!string.IsNullOrEmpty(hdef))
                {
                    Fullname = hdef;
                }
                else
                {
                    Fullname = Name;
                }

                var address = 0;
                var endaddr = 0;
                var exit = false;

                Instructions = new List<IsInstruction>();

                for (var dex = symb.Offset; dex < fdat.Length; dex += 4)
                {
                    var inst = new Mips.Instruction((uint)(address + dex), (uint)(fdat[dex] | fdat[dex + 1] << 8 | fdat[dex + 2] << 16 | (uint)fdat[dex + 3] << 24));

                    //check patch
                    if (inst.Cmd == "j")
                    {
                        var patch = Section.Patches.FirstOrDefault(x =>
                            x.Offset == inst.Address
                        );
                        if (patch != null)
                        {
                            inst.ReferencedAddress = patch.Value;
                            var onevalformat = "{0} {1}";
                            inst.Display = string.Format(onevalformat, "j", "0x" + inst.ReferencedAddress.ToString("x8"));
                        }
                    }
                    else if (inst.Cmd == "jal")
                    {
                        var patch = Section.Patches.FirstOrDefault(x =>
                            x.Offset == inst.Address
                        );
                        if (patch != null)
                        {
                            var sym = link.Symbols.FirstOrDefault(x => x.Sym == patch.Symbol || (patch.PatchType == PatchType.SectionBase && x.Offset == patch.Value));
                            if (sym != null)
                            {
                                if (!Calledsymbols.Exists(x => x.Name == sym.Name))
                                {
                                    Calledsymbols.Add(sym);
                                }
                            }
                        }
                    }

                    Instructions.Add(inst);
                    if (exit)
                    {
                        break;
                    }

                    if (inst.IsReturn || (endaddr != 0 && inst.Address == endaddr))
                    {
                        exit = true;
                    }
                }

                Length = (int)(Instructions.Last().Address - address);
                Blocks = analyzeFunction(Instructions);

                foreach (var block in Blocks)
                {
                    foreach (var inst in block.Instructions)
                    {
                        switch (inst.Cmd)
                        {
                            case "jal":
                                /*var calledfunc = funclist.FirstOrDefault(x => x.address == inst.referencedAddress);
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
                                */
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
                                /*var fulladdr = inst.GetGlobalVariable(block);
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
                                }*/
                                break;


                        }

                    }
                    if (block.EndsLoop)
                    {
                        Hasloop = true;
                    }
                }

            }

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
        }

        private void lstFuncs_SelectedIndexChanged(object sender, EventArgs e)
        {
            var func = (AnalyzedFunction)lstFuncs.SelectedItem;// analyzedfuncs.FirstOrDefault(x => x.fullname == (string)lstFuncs.SelectedItem);

            var blocks = FrmFileAnalyzer.AnalyzeFunction(func.Instructions);

            txtCode.Text = PrintFunction(blocks, func.Module.Link, func.Section);
        }

        private void lstModules_SelectedIndexChanged(object sender, EventArgs e)
        {
            var module = _lib.Modules.FirstOrDefault(x => x.Header.ModuleName == (string)lstModules.SelectedItem);
            if (module!=null)
            {
                DumpModule(module);
            }
            else
            {
                txtDump.Text = "";
            }
        }

        private void DumpModule(LibModule module)
        {
            using (var br = new BinaryReader(File.OpenRead(_libFile)))
            {
                module.Rerun(br);
            }
            
            txtDump.Text = module.Link.ActivityLog.ToString();
            //StringBuilder sb = new StringBuilder();



            //txtDump.Text = sb.ToString();
        }

        Lib _selectedLib;
        private void lstLibs_SelectedIndexChanged(object sender, EventArgs e)
        {
            //lstLibModules.Items.Clear();
            lstExportedFuncs.Items.Clear();
            _selectedLib = (Lib)lstLibs.SelectedItem;
            tvFuncs.Nodes.Clear();
            if (_selectedLib != null)
            {
                foreach(var func in Exportedanalyzedfuncs.Where(x=>x.Module.Lib == _selectedLib))
                {
                    var node = GetNode(func);
                    tvFuncs.Nodes.Add(node);
                }
                foreach (var s in _selectedLib.ExportedFunctions)
                    lstExportedFuncs.Items.Add(s);
            }
        }
        LibModule _selectedMod;
        private void lstLibModules_SelectedIndexChanged(object sender, EventArgs e)
        {
            /*lstExportedFuncs.Items.Clear();
            if (selectedLib == null)
                return;
            selectedMod = (Lib_Module)lstLibModules.SelectedItem;
            foreach(var mod in selectedLib.modules)
            {
                if (selectedMod == null || selectedMod == mod)
                {
                    lstExportedFuncs.Items.AddRange(mod.Link.ex)
                }
            }*/
        }


        TreeNode GetNode(AnalyzedFunction func, bool recursive = false)
        {
            var displayname = func.ToString();
            var node = new TreeNode(displayname);
            node.Tag = func;
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
        bool _ignoreevents = false;
        private void tvFuncs_AfterSelect(object sender, TreeViewEventArgs e)
        {
            _ignoreevents = true;
            if (sender != null && e != null)
            {
                txtFilter.Text = "";
            }

            _ignoreevents = false;
            var func = (AnalyzedFunction)tvFuncs.SelectedNode?.Tag;
            lstPotentialMatches.Items.Clear();
            if (func != null && !(func is UnknownAnalyzedFunction))
            {
                var blocks = FrmFileAnalyzer.AnalyzeFunction(func.Instructions);

                txtCode.Text = PrintFunction(blocks, func.Module.Link, func.Section);
                
                FindPotentialMatches(func);
            }
            else
            {
                txtCode.Text = "";
            }
        }

        void FindPotentialMatches(AnalyzedFunction func)
        {
            var minaddress = _sister.Datafile.Contains("startscreen") ? 0 : 0x82478;
            foreach(var checkme in _sister.Analyzedfunctions.Where(
                    x=> x.Address > minaddress && 
                    x.Blocks.Count == func.Blocks.Count && 
                    x.Calledfunctions.Count == func.Calledfunctions.Count
                )
            )
            {
                var filterHit = false;
                int dex;
                for(dex = 0;dex<checkme.Blocks.Count;dex++)
                {
                    var b1 = checkme.Blocks[dex];
                    var b2 = func.Blocks[dex];
                    if (b1.BlockType != b2.BlockType)
                    {
                        break;
                    }

                    if (txtFilter.Text!="")
                    {
                        if (b1.Instructions.Any(x=>x.Display.Contains(txtFilter.Text) || x.Instruction.ToString("x8").Contains(txtFilter.Text)))
                        {
                            filterHit = true;
                        }
                    }

                    var instructionsdif = Math.Abs(b1.Instructions.Count - b2.Instructions.Count);
                    var instdifper = b1.Instructions.Count / (float)b2.Instructions.Count;
                    if (!(instructionsdif <= 2 || (b1.Instructions.Count > 20 && instdifper > 0.9 && instdifper < 1.1)))
                    {
                        break;
                    }

                    if (b1.Instructions.Where(x => x.Cmd == "jal").Count() != b2.Instructions.Where(x => x.Cmd == "jal").Count())
                    {
                        break;
                    }
                }
                if (txtFilter.Text != "" && !filterHit)
                {
                    continue;
                }

                if (dex == checkme.Blocks.Count)
                {
                    //it made it through all the blocks
                    lstPotentialMatches.Items.Add(checkme);
                }
            }
        }

        private void lstPotentialMatches_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var func = (FrmFileAnalyzer.AnalyzedFunction)lstPotentialMatches.SelectedItem;
            if (func!=null)
            {
                var frm = new FrmAnalyzedFunction(func, _sister.Datafile);
                frm.Show();
            }
            
        }
        StringBuilder _definer = new();
        void AddToDef(AnalyzedFunction func, FrmFileAnalyzer.AnalyzedFunction match)
        {
            _definer.AppendLine($"AddFunction(0x{match.Address.ToString("x")}, \"{func.Name}\", \"\")");
            for (var dex = 0;dex<func.Calledfunctions.Count;dex++)
            {
                var subfunc = func.Calledfunctions[dex];
                var submatch = match.Calledfunctions[dex];
                AddToDef(subfunc, submatch);
            }
        }
        private void btnDefineMatch_Click(object sender, EventArgs e)
        {
            var func = (AnalyzedFunction)tvFuncs.SelectedNode?.Tag;
            var match = (FrmFileAnalyzer.AnalyzedFunction)lstPotentialMatches.SelectedItem;
            if (match == null)
            {
                var addr = _sister.ParseNum(txtAddr.Text);
                match = _sister.Analyzedfunctions.FirstOrDefault(x => x.Address == addr);
            }
            _definer = new StringBuilder();
            if (func!= null && match!=null)
            {
                AddToDef(func, match);
            }
            txtDefine.Text = _definer.ToString();
        }

        private void txtFilter_TextChanged(object sender, EventArgs e)
        {
            if (_ignoreevents)
            {
                return;
            }

            //refresh matches view
            tvFuncs_AfterSelect(null, null);
        }
    }
}
