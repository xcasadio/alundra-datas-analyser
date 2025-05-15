namespace alundramultitool.Decompiler.LibModule
{
    public class Lib
    {
        string[] _hlines;
        public Lib(string path, string hpath, List<string> otherdefs)
        {
            if (File.Exists(hpath))
            {
                _hlines = File.ReadAllLines(hpath);
            }
            else
            {
                _hlines = otherdefs.ToArray();
            }
            Name = Path.GetFileNameWithoutExtension(path);
            using (var br = new BinaryReader(File.OpenRead(path)))
            {
                Header = new LibHeader(br);
                while(br.BaseStream.Position + 8 < br.BaseStream.Length)
                {
                    var module = new LibModule(br, this);
                    if (module.Link != null)
                    {
                        Modules.Add(module);
                    }
                }
            }

            if (_hlines!=null)
            {
                foreach(var symb in Modules.SelectMany(x=>x.Link.Symbols.Where(x2=>x2.Type == SymbolType.Internal)))
                {
                    foreach(var hline in _hlines)
                    {
                        if (System.Text.RegularExpressions.Regex.IsMatch(hline, @"\s" + symb.Name + @"\s?\("))
                        {
                            ExportedFunctions.Add(symb.Mod.Header.ModuleName + " : " + symb.Name);
                            break;
                        }
                    }
                }
            }
            
        }
        public override string ToString()
        {
            return Name;
        }
        public string Name;
        public LibHeader Header;
        public List<LibModule> Modules = new();

        public List<string> ExportedFunctions = new();
    }
}
