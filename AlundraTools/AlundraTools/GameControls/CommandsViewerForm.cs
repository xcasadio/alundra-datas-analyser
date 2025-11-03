using AlundraEngine.DatasBin;
using AlundraEngine.DatasBin.Commands;
using AlundraTools.GameControls.CommandControls;
using AlundraTools.GameControls.CommandControls.Commands;
using Microsoft.VisualBasic;

namespace AlundraTools.GameControls
{
    public partial class CommandsViewerForm : Form
    {
        private List<SiCommand> _commands;
        private GameMap? _alundraGameMap;
        private GameMap? _currentGameMap;

        public CommandsViewerForm()
        {
            InitializeComponent();
        }

        public void Init(List<SiCommand> commands, GameMap? alundraGameMap, GameMap? currentGameMap)
        {
            _commands = commands;
            _alundraGameMap = alundraGameMap;
            _currentGameMap = currentGameMap;
        }

        private void CommandsViewerForm_Load(object sender, EventArgs e)
        {
            var commandBases = CommandsBuilder.Convert(_commands);
            int index = 0;

            foreach (var commandBase in commandBases)
            {
                CreateTreeViewNode(ref index, commandBase);
                index++;
            }

            treeView1.ExpandAll();
        }

        private void CreateTreeViewNode(ref int index, CommandBase commandBase, TreeNode? parentNode = null)
        {
            TreeNodeCollection nodes = parentNode == null ? treeView1.Nodes : parentNode.Nodes;
            parentNode = nodes.Add(index.ToString(), $"{index:D2} - {commandBase.PrintName()}");
            parentNode.ToolTipText = commandBase.Description();
            parentNode.Tag = commandBase;

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

                    textBox1.Text = $@"Text load from {map} map =>{Environment.NewLine}";
                    var text = strings?[dialogCommand.TextId & 0x7f];
                    textBox1.Text += text;
                }
            }
        }
    }
}
