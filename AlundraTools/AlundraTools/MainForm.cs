using AlundraEngine;
using AlundraEngine.Balance;
using AlundraEngine.DatasBin;
using AlundraEngine.Editor;
using AlundraEngine.Etc;
using AlundraEngine.Sound;
using AlundraEngine.Text;
using AlundraTools.GameControls;

namespace AlundraTools;

public partial class MainForm : Form
{
    private static int _isGameRunning;

    public MainForm()
    {
        InitializeComponent();
        Load += MainForm_Load;
    }

    private void MainForm_Load(object? sender, EventArgs e)
    {
        var saveStateDirectory = Path.Combine(Environment.CurrentDirectory, "SaveStates");

        if (Directory.Exists(saveStateDirectory))
        {
            foreach (var file in Directory.GetFiles(saveStateDirectory, "*.json"))
            {
                listBoxSaveStates.Items.Add(Path.GetFileName(file));
            }
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
                var frmAlundra = new FrmAlundra();
                frmAlundra.Show();
                var datasBin = new DatasBin(ofd.FileName);
                var dataFolder = Path.GetDirectoryName(ofd.FileName);
                var balanceFile = Path.Combine(dataFolder, "BALANCE.BIN");
                var balanceBin = new BalanceBin(balanceFile);
                var soundBinFileName = Path.Combine(dataFolder, "SOUND.BIN");
                var soundBin = new SoundBin(soundBinFileName);
                var font3 = new Font3(Path.Combine(dataFolder, "..", "TAKI\\SCREEN"));
                var etcResFileName = PathHelper.GetEtcFileName(dataFolder);
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

    private void exitToolStripMenuItem_Click(object sender, EventArgs e)
    {
        Application.Exit();
    }

    private void toolStripMenuItem1_Click(object sender, EventArgs e)
    {
        LaunchGame(-1, null);
    }

    private void listBoxSaveStates_MouseDoubleClick(object sender, MouseEventArgs e)
    {
        if (listBoxSaveStates.SelectedIndex != -1)
        {
            var selectedFile = listBoxSaveStates.SelectedItem.ToString();
            var saveStateDirectory = Path.Combine(Environment.CurrentDirectory, "SaveStates");
            var filePath = Path.Combine(saveStateDirectory, selectedFile);
            LaunchGame(-1, filePath);
        }
    }
    private static void LaunchGame(int mapId, string? gameStateFile)
    {
        var ofd = new OpenFileDialog();
        ofd.Filter = "DATAS.BIN|DATAS.BIN|All Files (*.*)|*.*";
        ofd.InitialDirectory = @"D:\development\repo\Alundra Remake\Alundra (France)\Alundra (France)_extracted\DATA";

        if (ofd.ShowDialog() == DialogResult.OK)
        {
            if (!string.IsNullOrWhiteSpace(ofd.FileName))
            {
                try
                {
                    if (Interlocked.CompareExchange(ref _isGameRunning, 1, 0) != 0)
                    {
                        MessageBox.Show("AlundraGame is already running in this process.", "Unable to launch AlundraGame", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }

                    var datasBinFilePath = ofd.FileName;
                    var gameThread = new Thread(() => RunGameInProcess(datasBinFilePath, mapId, gameStateFile))
                    {
                        IsBackground = true,
                        Name = "AlundraGameThread"
                    };

                    gameThread.SetApartmentState(ApartmentState.STA);
                    gameThread.Start();
                }
                catch (Exception ex)
                {
                    Interlocked.Exchange(ref _isGameRunning, 0);
                    MessageBox.Show(ex.Message, "Unable to launch AlundraGame", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
    }

    private static void RunGameInProcess(string datasBinFilePath, int mapId, string? gameStateFile)
    {
        try
        {
            AlundraGame.AlundraGameRunner.Run(datasBinFilePath, mapId, gameStateFile);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.ToString(), "Unable to launch AlundraGame", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            Interlocked.Exchange(ref _isGameRunning, 0);
        }
    }
}