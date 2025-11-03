using AlundraEngine;
using AlundraEngine.DatasBin;
using AlundraEngine.Sound;
using AlundraEngine.Text;
using AlundraTools.GameControls;
using Microsoft.Win32;
using System.Text.Json;
using AlundraTools.GameControls.CommandControls;

namespace AlundraTools;

public partial class MainForm : Form
{
    public MainForm()
    {
        InitializeComponent();
        Load += MainForm_Load;
    }

    private void MainForm_Load(object? sender, EventArgs e)
    {
        //toolStripMenuItem1_Click(this, EventArgs.Empty);
    }

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
        ofd.InitialDirectory = @"D:\development\repo\Alundra Remake\Alundra (France)\Alundra (France)_extracted\DATA";

        if (ofd.ShowDialog() == DialogResult.OK)
        {
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
                var font3 = new Font3(Path.Combine(dataFolder, "..", "TAKI\\SCREEN"));
                var etcResFileName = GetEtcFileName(dataFolder);
                EtcRes etcRes;

                if (Path.GetFileName(etcResFileName).Contains("usa", StringComparison.InvariantCultureIgnoreCase))
                {
                    etcRes = new EtcResUsa(etcResFileName);
                }
                else
                {
                    etcRes = new EtcResR(etcResFileName);
                }

                frmAlundra.Init(datasBin, balanceBin, soundBin, etcRes, font3);
            }
        }
    }

    private static string GetEtcFileName(string dataFolder)
    {
        //"ETC_RES.R"
        var files = Directory.GetFiles(dataFolder, "*.R");

        if (files.Length == 0)
        {
            throw new FileNotFoundException($"ETC_XXX.R file not found in the specified data folder {dataFolder}.");
        }

        return Path.Combine(dataFolder, files[0]);
    }

    private void exitToolStripMenuItem_Click(object sender, EventArgs e)
    {
        Application.Exit();
    }

    private void toolStripMenuItem1_Click(object sender, EventArgs e)
    {
        var ofd = new OpenFileDialog();
        ofd.Filter = "DATAS.BIN|DATAS.BIN|All Files (*.*)|*.*";
        ofd.InitialDirectory = @"D:\development\repo\Alundra Remake\Alundra (France)\Alundra (France)_extracted\DATA";

        if (ofd.ShowDialog() == DialogResult.OK)
        {
            if (!string.IsNullOrWhiteSpace(ofd.FileName))
            {
                var datasBin = new DatasBin(ofd.FileName);
                var dataFolder = Path.GetDirectoryName(ofd.FileName);
                var soundFile = Path.Combine(dataFolder, "SOUND.BIN");
                var balanceFile = Path.Combine(dataFolder, "BALANCE.BIN");
                var font3Folder = Path.Combine(dataFolder, "..", "TAKI\\SCREEN");
                var etcResFileName = GetEtcFileName(dataFolder);
                EtcRes etcRes;

                if (Path.GetFileName(etcResFileName).Contains("usa", StringComparison.InvariantCultureIgnoreCase))
                {
                    etcRes = new EtcResUsa(etcResFileName);
                }
                else
                {
                    etcRes = new EtcResR(etcResFileName);
                }

                var frmGame = new FrmGame(
                    datasBin,
                    new BalanceBin(balanceFile),
                    new SoundBin(soundFile),
                    etcRes,
                    new Font3(font3Folder));
                frmGame.Show();
            }
        }
    }
}