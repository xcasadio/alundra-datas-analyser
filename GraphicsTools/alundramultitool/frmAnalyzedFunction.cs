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
using static GraphicsTools.frmFileAnalyzer;

namespace alundramultitool
{
    public partial class frmAnalyzedFunction : Form
    {
        AnalyzedFunction func;
        string datafile;
        public frmAnalyzedFunction(AnalyzedFunction func,string datafile)
        {
            this.func = func;
            this.datafile = datafile;
            InitializeComponent();
            
        }

        private void frmAnalyzedFunction_Load(object sender, EventArgs e)
        {
            lblName.Text = func.ToString() + string.Join(">",func.stack.Select(x=>x.DisplayName));
            txtNotes.Text = func.notes;

            foreach(var cfunc in func.calledfunctions)
            {
                lstCalledFunctions.Items.Add(cfunc);
            }

            foreach (var cfunc in func.calledby)
            {
                lstCalledBy.Items.Add(cfunc.ToString());
            }

            foreach (var gvar in func.globalvariables)
            {
                lstGlobalVars.Items.Add(gvar.DisplayName);
            }

            foreach (var dstring in func.debugstrings)
            {
                lstDebugStrings.Items.Add(dstring);
            }


            int chunklength = 1024 * 1024;
            var fdat = new byte[chunklength];
            var stream = File.OpenRead(datafile);
            stream.Position = func.address;
            int numread = stream.Read(fdat, 0, chunklength);
            stream.Close();

            bool exit = false;

            var selectedFunction = new List<ISInstruction>();

            for (int dex = 0; dex < 10000; dex += 4)
            {
                var inst = new MIPS.Instruction((uint)(func.address + dex), (uint)(fdat[dex] | fdat[dex + 1] << 8 | fdat[dex + 2] << 16 | (uint)fdat[dex + 3] << 24));
                selectedFunction.Add(inst);
                if (exit)
                    break;
                if (inst.IsReturn)
                    exit = true;
            }
            var blocks = AnalyzeFunction(selectedFunction);

            txtFunction.Text = PrintFunction(blocks);
        }

        private void lstGlobalVars_SelectedIndexChanged(object sender, EventArgs e)
        {
            lstVarsIncludedIn.Items.Clear();
            if (lstGlobalVars.SelectedIndex != -1)
            {
                var gvar = func.globalvariables.FirstOrDefault(x => x.DisplayName == lstGlobalVars.SelectedItem.ToString());
                foreach (var ifunc in gvar.functions)
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
                        var frm = new frmAnalyzedFunction(cfunc, datafile);
                        frm.Show();
                    }

            }
        }

        private void FindText(string tofind)
        {
            int pos = txtFunction.SelectionStart;
            var next = txtFunction.Text.IndexOf(tofind, pos + 1);
            if (next == -1)
                next = txtFunction.Text.IndexOf(tofind);

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
            foreach (var cfunc in func.calledby)
            {
                if (cfunc.ToString() == (string)lstCalledBy.SelectedItem)
                {
                    var frm = new frmAnalyzedFunction(cfunc, datafile);
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
                var frm = new frmAnalyzedFunction(cfunc, datafile);
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
