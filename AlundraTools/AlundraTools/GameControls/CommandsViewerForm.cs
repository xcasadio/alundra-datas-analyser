using AlundraEngine.DatasBin;
using AlundraEngine.Gameplay.Scripts;

namespace AlundraTools.GameControls
{
    public partial class CommandsViewerForm : Form
    {
        private List<SiCommand> _commands;
        private GameMap? _alundraGameMap;
        private GameMap? _currentGameMap;
        private byte[]? _codes;
        private int _selectedOffset;

        public CommandsViewerForm()
        {
            InitializeComponent();
        }

        public void Init(List<SiCommand> commands, GameMap? alundraGameMap, GameMap? currentGameMap, 
            int selectedOffset, byte[] codes = null)
        {
            _selectedOffset = selectedOffset;
            _codes = codes;
            _commands = commands;
            _alundraGameMap = alundraGameMap;
            _currentGameMap = currentGameMap;
        }

        private void CommandsViewerForm_Load(object sender, EventArgs e)
        {
            FillTreeView(treeView1, _commands, _selectedOffset);

            if (_codes != null)
            {
                textBoxRawCodes.Text = string.Join(' ', _codes.Select(b => b.ToString("X2")));
            }
        }

        public static void FillTreeView(TreeView treeView, List<SiCommand> commands, int selectedOffset)
        {
            treeView.SuspendLayout();

            try
            {
                int index = 0;

                foreach (var command in commands)
                {
                    CreateTreeViewNode(ref index, command, selectedOffset, treeView);
                    index++;
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString(), "error", MessageBoxButtons.OK);
                throw;
            }
            
            treeView.ExpandAll();
            treeView.ResumeLayout();

            if (treeView.SelectedNode != null)
            {
                treeView.SelectedNode.EnsureVisible();
            }
        }

        public static void CreateTreeViewNode(ref int index, SiCommand commandBase, int selectedOffset, TreeView treeView, TreeNode? parentNode = null)
        {
            TreeNodeCollection nodes = parentNode == null ? treeView.Nodes : parentNode.Nodes;
            parentNode = nodes.Add(index.ToString(), $"{commandBase.Offset:D4} - {EventCodeDebugger.CreateLog(commandBase.Offset, commandBase.Command, commandBase.Parameters, false)}");
            parentNode.ToolTipText = EventCodeDebugger.GetHandlerName(commandBase.Command);
            parentNode.Tag = commandBase;

            if (commandBase.Command == 0)
            {
                parentNode.ForeColor = Color.DarkGray;
            }
            else if (commandBase.Command == 255)
            {
                parentNode.ForeColor = Color.Red;
            }
            else if (commandBase.Command is 0x0D or 0x5C or 0xC4)
            {
                parentNode.ForeColor = Color.Blue;
            }
            else if (commandBase.Command is 0x02 or 0x03 or 0x04 or 0x58)
            {
                parentNode.ForeColor = Color.Coral;
            }

            if (commandBase.Offset == selectedOffset)
            {
                treeView.SelectedNode = parentNode;
                parentNode.BackColor = Color.LightGreen;
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            textBox1.Text = "";

            if (e.Node != null)
            {
                if (e.Node.Tag is SiCommand command && command.Command is 0x0D or 0x5C or 0xC4)
                {
                    var strings = _alundraGameMap?.Strings;
                    var map = "";
                    var textId = command.Parameters[command.Command == 0x0D ? 0 : 1];

                    if ((textId & 0x80) != 0)
                    {
                        strings = _currentGameMap?.Strings;
                        map = "current";
                    }
                    else
                    {
                        map = "alundra";
                    }

                    textBox1.Text = $@"Text load from {map} map ";

                    if (command.Command is 0x5C or 0xC4)
                    {
                        textBox1.Text += $@"with entity[{command.Parameters[0]}]";
                    }

                    textBox1.Text += $@"=>{Environment.NewLine}";
                    var text = strings?[textId & 0x7f];
                    textBox1.Text += text;
                }
            }
        }
    }
}
