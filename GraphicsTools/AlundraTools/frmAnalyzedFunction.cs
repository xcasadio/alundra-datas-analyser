using AlundraTools.Decompiler;
using static AlundraTools.FrmFileAnalyzer;

namespace AlundraTools
{
    public partial class FrmAnalyzedFunction : Form
    {
        AnalyzedFunction _func;
        string _datafile;
        public FrmAnalyzedFunction(AnalyzedFunction func,string datafile)
        {
            _func = func;
            _datafile = datafile;
            InitializeComponent();
            
        }

        private void frmAnalyzedFunction_Load(object sender, EventArgs e)
        {
            lblName.Text = _func.ToString() + string.Join(">",_func.Stack.Select(x=>x.DisplayName));
            txtNotes.Text = _func.Notes;

            foreach(var cfunc in _func.Calledfunctions)
            {
                lstCalledFunctions.Items.Add(cfunc);
            }

            foreach (var cfunc in _func.Calledby)
            {
                lstCalledBy.Items.Add(cfunc.ToString());
            }

            foreach (var gvar in _func.Globalvariables)
            {
                lstGlobalVars.Items.Add(gvar.DisplayName);
            }

            foreach (var dstring in _func.Debugstrings)
            {
                lstDebugStrings.Items.Add(dstring);
            }


            var chunklength = 1024 * 1024;
            var fdat = new byte[chunklength];
            var stream = File.OpenRead(_datafile);
            stream.Position = _func.Address;
            var numread = stream.Read(fdat, 0, chunklength);
            stream.Close();

            var exit = false;

            var selectedFunction = new List<IsInstruction>();

            for (var dex = 0; dex < 10000; dex += 4)
            {
                var inst = new Mips.Instruction((uint)(_func.Address + dex), (uint)(fdat[dex] | fdat[dex + 1] << 8 | fdat[dex + 2] << 16 | (uint)fdat[dex + 3] << 24));
                selectedFunction.Add(inst);
                if (exit)
                {
                    break;
                }

                if (inst.IsReturn)
                {
                    exit = true;
                }
            }
            var blocks = AnalyzeFunction(selectedFunction);

            txtFunction.Text = PrintFunction(blocks);
        }

        private void lstGlobalVars_SelectedIndexChanged(object sender, EventArgs e)
        {
            lstVarsIncludedIn.Items.Clear();
            if (lstGlobalVars.SelectedIndex != -1)
            {
                var gvar = _func.Globalvariables.FirstOrDefault(x => x.DisplayName == lstGlobalVars.SelectedItem.ToString());
                foreach (var ifunc in gvar.Functions)
                    lstVarsIncludedIn.Items.Add(ifunc);
            }
        }

        private void lstGlobalVars_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (lstGlobalVars.SelectedItem != null)
            {
                var tofind = lstGlobalVars.SelectedItem.ToString().Split('(')[0];
                
                FindText(tofind);


            }
        }

        private void lstCalledFunctions_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (lstCalledFunctions.SelectedItem != null)
            {
                    var cfunc = (AnalyzedFunction)lstCalledFunctions.SelectedItem;
                    if (cfunc != null)
                    {
                        var frm = new FrmAnalyzedFunction(cfunc, _datafile);
                        frm.Show();
                    }

            }
        }

        private void FindText(string tofind)
        {
            var pos = txtFunction.SelectionStart;
            var next = txtFunction.Text.IndexOf(tofind, pos + 1);
            if (next == -1)
            {
                next = txtFunction.Text.IndexOf(tofind);
            }

            if (next >= 0)
            {
                txtFunction.SelectionStart = next;
                txtFunction.SelectionLength = tofind.Length;
                txtFunction.ScrollToCaret();
                txtFunction.Focus();
            }
        }

        private void lstCalledBy_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            foreach (var cfunc in _func.Calledby)
            {
                if (cfunc.ToString() == (string)lstCalledBy.SelectedItem)
                {
                    var frm = new FrmAnalyzedFunction(cfunc, _datafile);
                    frm.Show();
                    break;
                }
            }
        }

        private void lstVarsIncludedIn_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            var cfunc = (AnalyzedFunction)lstVarsIncludedIn.SelectedItem;
            if (cfunc != null)
            {
                var frm = new FrmAnalyzedFunction(cfunc, _datafile);
                frm.Show();
            }
        }

        private void lstCalledFunctions_MouseClick(object sender, MouseEventArgs e)
        {
            var tofind = lstCalledFunctions.SelectedItem.ToString().Split('(')[0];

            FindText(tofind);
        }
    }
}
