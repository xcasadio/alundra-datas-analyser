using alundramultitool;
using GraphicsTools.LIB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GraphicsTools
{
    public partial class frmLib : Form
    {
        LIB.LIB _lib;
        string[] hlines;
        public List<AnalyzedFunction> analyzedfuncs = new List<AnalyzedFunction>();
        frmFileAnalyzer sister;
        string libFile;
        List<LIB.LIB> _libs = new List<LIB.LIB>();
        public frmLib(string libFile, string hFile, string libdir, string includdir, frmFileAnalyzer sister)
        {
            this.sister = sister;
            InitializeComponent();
            List<string> otherdefs = new List<string>();
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
                var lib = new LIB.LIB(file, hf, otherdefs);
                _libs.Add(lib);
            }

            this.libFile = libFile;
            _lib = new LIB.LIB(libFile, hFile, null);


            hlines = System.IO.File.ReadAllLines(hFile);
            

            foreach(var mod in _lib.modules)
            {
                foreach(var sym in mod.Link.symbols)
                {
                    if (sym.Type == LIB.SYMBOL_TYPE.INTERNAL)
                    {
                        var hdef = FindDef(sym);
                        var func = new AnalyzedFunction(sym, mod, frmFileAnalyzer.AnalyzeFunction, hdef);
                        analyzedfuncs.Add(func);
                    }
                    else if (sym.Type == SYMBOL_TYPE.LOCAL)
                    {
                        var section = mod.Link.sections.FirstOrDefault(x => x.Symbol == sym.Section);
                        if (section.patches.Exists(x=>x.Value == sym.Offset && x.PatchType == PATCH_TYPE.SECTION_BASE && x.RelocType == RELOC_TYPE.FUNCTION_CALL))
                        {
                            var hdef = FindDef(sym);
                            var func = new AnalyzedFunction(sym, mod, frmFileAnalyzer.AnalyzeFunction, hdef);
                            analyzedfuncs.Add(func);
                        }
                    }
                }
            }

            foreach (var lib in _libs)
            {
                foreach(var mod in lib.modules)
                {
                    foreach(var sym in mod.Link.symbols)
                    {
                        AnalyzedFunction func = null;
                        if (sym.Type == LIB.SYMBOL_TYPE.INTERNAL)
                        {
                            var section = mod.Link.sections.FirstOrDefault(x => x.Symbol == sym.Section);
                            if (lib.ExportedFunctions.Contains(mod.header.ModuleName + " : "+ sym.Name) || section.patches.Exists(x => x.Symbol == sym.Sym && x.RelocType == RELOC_TYPE.FUNCTION_CALL))
                            {
                                func = new AnalyzedFunction(sym, mod, frmFileAnalyzer.AnalyzeFunction, null);
                            }
                            
                        }
                        else if (sym.Type == SYMBOL_TYPE.LOCAL)
                        {
                            var section = mod.Link.sections.FirstOrDefault(x => x.Symbol == sym.Section);
                            if (section.patches.Exists(x => x.Value == sym.Offset && x.PatchType == PATCH_TYPE.SECTION_BASE && x.RelocType == RELOC_TYPE.FUNCTION_CALL))
                            {
                                func = new AnalyzedFunction(sym, mod, frmFileAnalyzer.AnalyzeFunction, null);
                            }
                        }
                        if (func!=null)
                        {
                            fullanalyzedfuncs.Add(func);
                            if (lib.ExportedFunctions.Contains(mod.header.ModuleName + " : " + sym.Name))
                                exportedanalyzedfuncs.Add(func);
                        }
                    }
                }
            }

            for(int dex = 0;dex<fullanalyzedfuncs.Count;dex++)
            {
                var func = fullanalyzedfuncs[dex];

                if (func is UnknownAnalyzedFunction)
                    continue;

                foreach(var sym in func.calledsymbols)
                {
                    bool found = false;
                    if (sym.Type == SYMBOL_TYPE.LOCAL)
                    {
                        foreach(var checkme in fullanalyzedfuncs.Where(x=>x.module == sym.Mod))
                        {
                            if (checkme.name == sym.Name)
                            {
                                func.calledfunctions.Add(checkme);
                                found = true;
                            }
                        }
                    }
                    else if (sym.Type == SYMBOL_TYPE.INTERNAL)
                    {
                        foreach (var checkme in fullanalyzedfuncs.Where(x => x.module == sym.Mod))
                        {
                            if (checkme.name == sym.Name)
                            {
                                func.calledfunctions.Add(checkme);
                                found = true;
                            }
                        }
                    }
                    else
                    {
                        foreach (var checkme in exportedanalyzedfuncs)
                        {
                            if (checkme.name == sym.Name)
                            {
                                func.calledfunctions.Add(checkme);
                                found = true;
                            }
                        }
                        //should i also try full list
                    }
                    if (!found)
                    {
                        
                        foreach (var checkme in fullanalyzedfuncs)
                        {
                            if (checkme.name == sym.Name)
                            {
                                func.calledfunctions.Add(checkme);
                                found = true;
                            }
                        }
                    }
                    if (!found)
                    {
                        var unanalyzed = new UnknownAnalyzedFunction(sym.Name);
                        func.calledfunctions.Add(unanalyzed);
                        fullanalyzedfuncs.Add(unanalyzed);
                    }
                }
            }
            foreach (var func in fullanalyzedfuncs)
            {
                func.GetDepth(new List<AnalyzedFunction>());
            }

            foreach (var lib in _libs)
            {
                lstLibs.Items.Add(lib);
            }
        }
        public List<AnalyzedFunction> fullanalyzedfuncs = new List<AnalyzedFunction>();
        public List<AnalyzedFunction> exportedanalyzedfuncs = new List<AnalyzedFunction>();

        string FindDef(Symbol sym)
        {
            for (int dex = 0;dex<hlines.Length;dex++)
            {
                if (hlines[dex].Replace(" (","(").Contains(" " + sym.Name + "("))
                    return hlines[dex].Trim().Replace(";", "");
            }
            return null;
        }

        private void frmLib_Load(object sender, EventArgs e)
        {
            foreach (var func in analyzedfuncs)
                lstFuncs.Items.Add(func);

            foreach(var module in _lib.modules)
            {
                lstModules.Items.Add(module.header.ModuleName);
            }
        }

        public static string PrintFunction(List<CodeBlock<ISInstruction>> blocks, Link link, Section section)
        {
            string ftext = "";
            if (blocks.Count == 0)
                return "";
            uint addradjust = blocks.First().Instructions.First().address;

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
                                {
                                    var patch = section.patches.FirstOrDefault(x =>
                                        //(x.Value == 0 && x.Offset == inst.address)
                                        //|| (x.Value != 0 && x.Value == inst.address)
                                        x.Offset == inst.address
                                    );// (x.PatchType == PATCH_TYPE.REF && x.Offset + addradjust == inst.address) || x.Offset + addradjust == inst.address);
                                    if (patch != null)
                                    {
                                        var sym = link.symbols.FirstOrDefault(x => x.Sym == patch.Symbol || (patch.PatchType == PATCH_TYPE.SECTION_BASE && x.Offset == patch.Value));
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
                                if (inst.rs != 29)//if not local variable declaration
                                {
                                    bool nudgewierdness = false;
                                    //if (inst.rt == 16 || inst.rs == 16)
                                    //    nudgewierdness = true;
                                    if (nudgewierdness)
                                        inst.immediate += 0x134;
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
                                        //var evarname = evars[off];
                                        //if (!string.IsNullOrEmpty(evarname))
                                        //    name2 += "." + evarname;
                                        //else
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

        public class UnknownAnalyzedFunction : AnalyzedFunction
        {
            public UnknownAnalyzedFunction(string name):
                base(null,null,null,null)
            {
                this.name = name;
                this.fullname = name + " (unalyzed)";
            }

            
        }

        public class AnalyzedFunction
        {
            public string name;
            public string fullname;
            public int length;
            public bool hasloop;
            public bool callsfunctionpointers;
            public List<AnalyzedFunction> calledfunctions = new List<AnalyzedFunction>();
            public List<Symbol> calledsymbols = new List<Symbol>();
            public List<AnalyzedFunction> calledby = new List<AnalyzedFunction>();
            public List<ISInstruction> instructions;
            public List<CodeBlock<ISInstruction>> blocks;
            public Lib_Module module;
            public Section section;

            public override string ToString()
            {
                return fullname;
            }
            public AnalyzedFunction(Symbol symb, Lib_Module module, Func<List<ISInstruction>, List<CodeBlock<ISInstruction>>> AnalyzeFunction, string hdef)
            {
                if (symb == null)
                    return;
                this.module = module;
                var link = module.Link;
                section = link.sections.FirstOrDefault(x => x.Symbol == symb.Section);
                var fdat = section.Code;
                name = symb.Name;
                if (!string.IsNullOrEmpty(hdef))
                    fullname = hdef;
                else
                    fullname = name;
                int address = 0;
                int endaddr = 0;
                bool exit = false;

                instructions = new List<ISInstruction>();

                for (int dex = symb.Offset; dex < fdat.Length; dex += 4)
                {
                    var inst = new MIPS.Instruction((uint)(address + dex), (uint)(fdat[dex] | fdat[dex + 1] << 8 | fdat[dex + 2] << 16 | (uint)fdat[dex + 3] << 24));

                    //check patch
                    if (inst.cmd == "j")
                    {
                        var patch = section.patches.FirstOrDefault(x =>
                            x.Offset == inst.address
                        );
                        if (patch != null)
                        {
                            inst.referencedAddress = patch.Value;
                            string onevalformat = "{0} {1}";
                            inst.display = string.Format(onevalformat, "j", "0x" + inst.referencedAddress.ToString("x8"));
                        }
                    }
                    else if (inst.cmd == "jal")
                    {
                        var patch = section.patches.FirstOrDefault(x =>
                            x.Offset == inst.address
                        );
                        if (patch != null)
                        {
                            var sym = link.symbols.FirstOrDefault(x => x.Sym == patch.Symbol || (patch.PatchType == PATCH_TYPE.SECTION_BASE && x.Offset == patch.Value));
                            if (sym != null)
                            {
                                if (!calledsymbols.Exists(x => x.Name == sym.Name))
                                    calledsymbols.Add(sym);
                            }
                        }
                    }

                    instructions.Add(inst);
                    if (exit)
                        break;
                    if (inst.IsReturn || (endaddr != 0 && inst.address == endaddr))
                        exit = true;
                }

                length = (int)(instructions.Last().address - address);
                blocks = AnalyzeFunction(instructions);

                foreach (var block in blocks)
                {
                    foreach (var inst in block.Instructions)
                    {
                        switch (inst.cmd)
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
                        hasloop = true;
                    }
                }

            }

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
        }

        private void lstFuncs_SelectedIndexChanged(object sender, EventArgs e)
        {
            var func = (AnalyzedFunction)lstFuncs.SelectedItem;// analyzedfuncs.FirstOrDefault(x => x.fullname == (string)lstFuncs.SelectedItem);

            var blocks = frmFileAnalyzer.AnalyzeFunction(func.instructions);

            txtCode.Text = PrintFunction(blocks, func.module.Link, func.section);
        }

        private void lstModules_SelectedIndexChanged(object sender, EventArgs e)
        {
            var module = _lib.modules.FirstOrDefault(x => x.header.ModuleName == (string)lstModules.SelectedItem);
            if (module!=null)
            {
                DumpModule(module);
            }
            else
            {
                txtDump.Text = "";
            }
        }

        private void DumpModule(Lib_Module module)
        {
            using (var br = new BinaryReader(File.OpenRead(libFile)))
            {
                module.Rerun(br);
            }
            
            txtDump.Text = module.Link.ActivityLog.ToString();
            //StringBuilder sb = new StringBuilder();



            //txtDump.Text = sb.ToString();
        }

        LIB.LIB selectedLib;
        private void lstLibs_SelectedIndexChanged(object sender, EventArgs e)
        {
            //lstLibModules.Items.Clear();
            lstExportedFuncs.Items.Clear();
            selectedLib = (LIB.LIB)lstLibs.SelectedItem;
            tvFuncs.Nodes.Clear();
            if (selectedLib != null)
            {
                foreach(var func in exportedanalyzedfuncs.Where(x=>x.module.lib == selectedLib))
                {
                    var node = GetNode(func);
                    tvFuncs.Nodes.Add(node);
                }
                foreach (var s in selectedLib.ExportedFunctions)
                    lstExportedFuncs.Items.Add(s);
            }
        }
        Lib_Module selectedMod;
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
        bool ignoreevents = false;
        private void tvFuncs_AfterSelect(object sender, TreeViewEventArgs e)
        {
            ignoreevents = true;
            if (sender != null && e != null)
                txtFilter.Text = "";
            ignoreevents = false;
            var func = (AnalyzedFunction)tvFuncs.SelectedNode?.Tag;
            lstPotentialMatches.Items.Clear();
            if (func != null && !(func is UnknownAnalyzedFunction))
            {
                var blocks = frmFileAnalyzer.AnalyzeFunction(func.instructions);

                txtCode.Text = PrintFunction(blocks, func.module.Link, func.section);
                
                FindPotentialMatches(func);
            }
            else
            {
                txtCode.Text = "";
            }
        }

        void FindPotentialMatches(AnalyzedFunction func)
        {
            int minaddress = sister.datafile.Contains("startscreen") ? 0 : 0x82478;
            foreach(var checkme in sister.analyzedfunctions.Where(
                    x=> x.address > minaddress && 
                    x.blocks.Count == func.blocks.Count && 
                    x.calledfunctions.Count == func.calledfunctions.Count
                )
            )
            {
                bool filterHit = false;
                int dex;
                for(dex = 0;dex<checkme.blocks.Count;dex++)
                {
                    var b1 = checkme.blocks[dex];
                    var b2 = func.blocks[dex];
                    if (b1.BlockType != b2.BlockType)
                        break;

                    if (txtFilter.Text!="")
                    {
                        if (b1.Instructions.Any(x=>x.display.Contains(txtFilter.Text) || x.instruction.ToString("x8").Contains(txtFilter.Text)))
                        {
                            filterHit = true;
                        }
                    }

                    var instructionsdif = Math.Abs(b1.Instructions.Count - b2.Instructions.Count);
                    var instdifper = b1.Instructions.Count / (float)b2.Instructions.Count;
                    if (!(instructionsdif <= 2 || (b1.Instructions.Count > 20 && instdifper > 0.9 && instdifper < 1.1)))
                        break;
                    if (b1.Instructions.Where(x => x.cmd == "jal").Count() != b2.Instructions.Where(x => x.cmd == "jal").Count())
                        break;
                }
                if (txtFilter.Text != "" && !filterHit)
                    continue;
                if (dex == checkme.blocks.Count)
                {
                    //it made it through all the blocks
                    lstPotentialMatches.Items.Add(checkme);
                }
            }
        }

        private void lstPotentialMatches_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var func = (frmFileAnalyzer.AnalyzedFunction)lstPotentialMatches.SelectedItem;
            if (func!=null)
            {
                var frm = new frmAnalyzedFunction(func, sister.datafile);
                frm.Show();
            }
            
        }
        StringBuilder definer = new StringBuilder();
        void AddToDef(AnalyzedFunction func, frmFileAnalyzer.AnalyzedFunction match)
        {
            definer.AppendLine($"AddFunction(0x{match.address.ToString("x")}, \"{func.name}\", \"\")");
            for (int dex = 0;dex<func.calledfunctions.Count;dex++)
            {
                var subfunc = func.calledfunctions[dex];
                var submatch = match.calledfunctions[dex];
                AddToDef(subfunc, submatch);
            }
        }
        private void btnDefineMatch_Click(object sender, EventArgs e)
        {
            var func = (AnalyzedFunction)tvFuncs.SelectedNode?.Tag;
            var match = (frmFileAnalyzer.AnalyzedFunction)lstPotentialMatches.SelectedItem;
            if (match == null)
            {
                var addr = sister.ParseNum(txtAddr.Text);
                match = sister.analyzedfunctions.FirstOrDefault(x => x.address == addr);
            }
            definer = new StringBuilder();
            if (func!= null && match!=null)
            {
                AddToDef(func, match);
            }
            txtDefine.Text = definer.ToString();
        }

        private void txtFilter_TextChanged(object sender, EventArgs e)
        {
            if (ignoreevents)
                return;
            //refresh matches view
            tvFuncs_AfterSelect(null, null);
        }
    }
}
