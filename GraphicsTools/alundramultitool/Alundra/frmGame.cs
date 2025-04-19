using Timer = System.Windows.Forms.Timer;

namespace GraphicsTools.Alundra
{
    public partial class FrmGame : Form
    {
        private GameEngine _engine;
        private Timer _tmr;
        public FrmGame(DatasBin datasBin, BalanceBin balanceBin, SoundBin soundBin)
        {
            InitializeComponent();

            _engine = new GameEngine(datasBin, balanceBin, soundBin);
            var map = datasBin.GameMaps[389]; // 165
            _engine.LoadMap(map);
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
            var backbuffer = new Bitmap(320, 240);
            using (var g = Graphics.FromImage(backbuffer))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;

                //engine.MainUpdate(false);
                _engine.Render(g);
            }
            e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
            e.Graphics.Clear(Color.Black);
            e.Graphics.DrawImage(backbuffer, 0, 0, pctOut.Width, pctOut.Height);

        }
    }
}
