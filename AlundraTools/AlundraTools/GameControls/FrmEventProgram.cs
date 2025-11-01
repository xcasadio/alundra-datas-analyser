using AlundraEngine.DatasBin;
using AlundraTools.GameControls.EventControls;

namespace AlundraTools.GameControls
{
    public partial class FrmEventProgram : Form
    {
        private List<SiCommand> _commands;

        public FrmEventProgram()
        {
            InitializeComponent();
        }

        public void Init(List<SiCommand> commands)
        {
            _commands = commands;
        }

        
        private void FrmEventProgram_Load(object sender, EventArgs e)
        {
            var stack = new List<Stackframe>();

            foreach (var command in _commands)
            {
                lstProgram.Items.Add(command.Print(stack.Count, _commands));
                eventListView1.AddItem(new LabelScriptEvent(command.Command, command.PrintEvent(stack.Count, _commands), "test"));

                //if (cmd.command == 0xff && stack.Count == 0)
                //    break;
                
                if (command.GetType() == typeof(BranchCommand) && command.RefOffset > 0)
                {
                    stack.Add(new Stackframe { Length = command.RefOffset, Level = stack.Count });
                }

                for (var i = stack.Count -1;i >= 0;i--)
                {
                    var frame = stack[i];
                    frame.Length -= command.Size;
                    if (frame.Length <= 0)
                    {
                        stack.RemoveAt(i);
                    }
                }
            }
        }

        private void lstProgram_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstProgram.SelectedIndex >= 0)
            {
                lblmemaddr.Text = _commands[lstProgram.SelectedIndex].MemoryAddress.ToString("x6");
                lblcode.Text = _commands[lstProgram.SelectedIndex].Command.ToString("x2") + "(" + string.Join(",", _commands[lstProgram.SelectedIndex].Parameters.Select(x => x.ToString("x2"))) + ")";
            }
        }

        private int ParseNum(string num)
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

        private void btnFind_Click(object sender, EventArgs e)
        {
            var code = ParseNum(txtFind.Text);
            for (var dex = lstProgram.SelectedIndex + 1; dex < lstProgram.Items.Count; dex++)
            {
                if (_commands[dex].Command == code)
                {
                    lstProgram.SelectedIndex = dex;
                    break;
                }
            }
        }
    }

    internal class Stackframe
    {
        public int Level;
        public int Length;
    }
}
