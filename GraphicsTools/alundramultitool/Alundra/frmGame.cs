using Alundra;
using Alundra.DatasBin;
using Alundra.Sound;
using Alundra.Text;
using Timer = System.Windows.Forms.Timer;

namespace GraphicsTools.Alundra
{
    public partial class FrmGame : Form
    {
        private readonly Game _engine;
        private readonly Timer _tmr;
        private readonly Bitmap _backBuffer = new(320, 240);

        public FrmGame(DatasBin datasBin, BalanceBin balanceBin, SoundBin soundBin, EtcResR etcResR, Font3 font3)
        {
            InitializeComponent();

            _engine = new Game(datasBin, balanceBin, soundBin, etcResR, font3);
            _engine.Initialize();
            _tmr = new Timer();
            _tmr.Interval = 1000 / 30;
            _tmr.Tick += Tmr_Tick;
        }

        private void Tmr_Tick(object sender, EventArgs e)
        {
            pctOut.Refresh();
        }

        private void pctOut_Paint(object sender, PaintEventArgs e)
        {
            using var g = Graphics.FromImage(_backBuffer);
            g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;

            _engine.MainUpdate(false);
            _engine.Render(g);
            //_engine.MainLoop(g);

            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            e.Graphics.Clear(Color.Black);
            e.Graphics.DrawImage(_backBuffer, 0, 0, pctOut.Width, pctOut.Height);
        }
    }
}
