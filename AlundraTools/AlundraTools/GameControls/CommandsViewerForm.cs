using AlundraEngine.DatasBin;
using AlundraTools.GameControls.CommandControls.Commands;

namespace AlundraTools.GameControls
{
    public partial class CommandsViewerForm : Form
    {
        private List<SiCommand> _commands;
        private GameMap? _alundraGameMap;
        private GameMap? _currentGameMap;
        private byte[]? _codes;
        private int _selectedCommandIndex;

        public CommandsViewerForm()
        {
            InitializeComponent();
        }

        public void Init(List<SiCommand> commands, GameMap? alundraGameMap, GameMap? currentGameMap, 
            int selectedCommandIndex, byte[] codes = null)
        {
            _selectedCommandIndex = selectedCommandIndex;
            _codes = codes;
            _commands = commands;
            _alundraGameMap = alundraGameMap;
            _currentGameMap = currentGameMap;
        }

        private void CommandsViewerForm_Load(object sender, EventArgs e)
        {
            treeView1.SuspendLayout();

            try
            {
                var commandBases = CommandsBuilder.Convert(_commands);
                int index = 0;

                foreach (var commandBase in commandBases)
                {
                    CreateTreeViewNode(ref index, commandBase);
                    index++;
                }
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.ToString(), "error", MessageBoxButtons.OK);
                throw;
            }
            
            treeView1.ExpandAll();
            treeView1.ResumeLayout();

            if (treeView1.SelectedNode != null)
            {
                treeView1.SelectedNode.EnsureVisible();
            }

            if (_codes != null)
            {
                textBoxRawCodes.Text = string.Join(' ', _codes.Select(b => b.ToString("X2")));
            }
        }

        private void CreateTreeViewNode(ref int index, CommandBase commandBase, TreeNode? parentNode = null)
        {
            TreeNodeCollection nodes = parentNode == null ? treeView1.Nodes : parentNode.Nodes;
            parentNode = nodes.Add(index.ToString(), $"{commandBase.Offset:D4} - {commandBase.PrintName()}");//index:D2
            parentNode.ToolTipText = commandBase.Description();
            parentNode.Tag = commandBase;

            if (commandBase.Command == 0)
            {
                parentNode.ForeColor = Color.DarkGray;
            }
            else if (commandBase.Command == 255)
            {
                parentNode.ForeColor = Color.Red;
            }
            else if (commandBase is DialogCommand)
            {
                parentNode.ForeColor = Color.Blue;
            }

            if (index == _selectedCommandIndex)
            {
                treeView1.SelectedNode = parentNode;
                parentNode.BackColor = Color.LightGreen;
            }

            if (commandBase is not ContainerCommand container)
            {
                return;
            }

            foreach (var child in container.Children)
            {
                index++;
                CreateTreeViewNode(ref index, child, parentNode);
            }
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            textBox1.Text = "";

            if (e.Node != null)
            {
                if (e.Node.Tag is DialogCommand dialogCommand)
                {
                    var strings = _alundraGameMap?.Strings;
                    var map = "";

                    if ((dialogCommand.TextId & 0x80) != 0)
                    {
                        strings = _currentGameMap?.Strings;
                        map = "current";
                    }
                    else
                    {
                        map = "alundra";
                    }

                    textBox1.Text = $@"Text load from {map} map ";

                    if (dialogCommand is DialogCommandWithChoice dialogCommandWithChoice)
                    {
                        textBox1.Text += $@"with entity[{dialogCommandWithChoice.EntityIndex}] ";
                    }

                    textBox1.Text += $@"=>{Environment.NewLine}";
                    var text = strings?[dialogCommand.TextId & 0x7f];
                    textBox1.Text += text;
                }
            }
        }
    }
}
