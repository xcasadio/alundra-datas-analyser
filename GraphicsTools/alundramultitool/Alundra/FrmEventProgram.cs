using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GraphicsTools.Alundra
{
    public partial class FrmEventProgram : Form
    {
        List<SICommand> commands;

        public FrmEventProgram()
        {
            InitializeComponent();
        }

        public void Init(List<SICommand> commands)
        {
            this.commands = commands;
        }

        
        private void FrmEventProgram_Load(object sender, EventArgs e)
        {
            List<stackframe> stack = new List<stackframe>();
            foreach (var cmd in commands)
            {
                lstProgram.Items.Add(cmd.Print(stack.Count, commands));

                //if (cmd.command == 0xff && stack.Count == 0)
                //    break;
                
                if (cmd.GetType() == typeof(BranchCommand) && cmd.refoffset > 0)
                {
                    stack.Add(new stackframe { length = cmd.refoffset, level = stack.Count });
                }

                for (int dex = stack.Count -1;dex >= 0;dex--)
                {
                    var frame = stack[dex];
                    frame.length -= cmd.size;
                    if (frame.length <= 0)
                        stack.RemoveAt(dex);
                }
            }
        }

        private void lstProgram_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstProgram.SelectedIndex >= 0)
            {
                lblmemaddr.Text = commands[lstProgram.SelectedIndex].memaddr.ToString("x6");
                lblcode.Text = commands[lstProgram.SelectedIndex].command.ToString("x2") + "(" + string.Join(",", commands[lstProgram.SelectedIndex].parameters.Select(x => x.ToString("x2"))) + ")";
            }
        }

        int ParseNum(string num)
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

        private void btnFind_Click(object sender, EventArgs e)
        {
            int code = ParseNum(txtFind.Text);
            for (int dex = lstProgram.SelectedIndex + 1; dex < lstProgram.Items.Count; dex++)
            {
                if (commands[dex].command == code)
                {
                    lstProgram.SelectedIndex = dex;
                    break;
                }
            }
        }
    }

    class stackframe
    {
        public int level;
        public int length;
    }
}
