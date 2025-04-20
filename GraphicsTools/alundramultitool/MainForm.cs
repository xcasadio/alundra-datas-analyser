using System.Text.Json;
using GraphicsTools.Alundra;

namespace GraphicsTools
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        float _scale = 1;
        Bitmap _loadedImage;
        int _width;
        int _height;
        Dictionary<Color, int>[] _cells;
        Color[] _colors;
        Dictionary<Color, int> _colorBank;

        private void analyzeFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.ShowDialog();
            if (!string.IsNullOrEmpty(ofd.FileName))
            {
                var config = JsonSerializer.Deserialize<EditorConfiguration>(File.ReadAllText("config.json"));

                if (string.IsNullOrWhiteSpace(config.PsyqSdkFolder) || !Directory.Exists(config.PsyqSdkFolder))
                {
                    MessageBox.Show("Please set the Psy-Q SDK folder in the config.json.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                var frm = new FrmFileAnalyzer();
                frm.Datafile = ofd.FileName;
                frm.Initialize(config.PsyqSdkFolder);
                frm.Show();
            }
        }

        private void openDATASBINToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Title = "Select DATAS.BIN";
            ofd.Filter = "DATAS.BIN|DATAS.BIN|All Files (*.*)|*.*";
            ofd.ShowDialog();

            if (!string.IsNullOrWhiteSpace(ofd.FileName))
            {
                DebugSymbols.Init();
                var frmAlundra = new FrmAlundra();
                frmAlundra.Show();
                var datasBin = new DatasBin(ofd.FileName);
                var dataFolder = Path.GetDirectoryName(ofd.FileName);
                var balanceFile = Path.Combine(dataFolder, "BALANCE.BIN");
                var balanceBin = new BalanceBin(balanceFile);
                var soundBinFileName = Path.Combine(dataFolder, "SOUND.BIN");
                var soundBin = new SoundBin(soundBinFileName);
                var etcResRFileName = Path.Combine(dataFolder, "ETC_RES.R");
                var etcResR = new EtcResR(etcResRFileName);

                frmAlundra.Init(datasBin, balanceBin, soundBin, etcResR);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.Filter = "DATAS.BIN|DATAS.BIN|All Files (*.*)|*.*";
            ofd.ShowDialog();
            if (!string.IsNullOrWhiteSpace(ofd.FileName))
            {
                var soundFile = Path.Combine(Path.GetDirectoryName(ofd.FileName), "SOUND.BIN");
                var balanceFile = Path.Combine(Path.GetDirectoryName(ofd.FileName), "BALANCE.BIN");

                var frmGame = new FrmGame(
                    new DatasBin(ofd.FileName), 
                    new BalanceBin(balanceFile),
                    new SoundBin(soundFile));
                frmGame.Show();
            }
        }
    }
}
