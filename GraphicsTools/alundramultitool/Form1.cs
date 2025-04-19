using System.Drawing.Imaging;
using GraphicsTools.Alundra;

namespace GraphicsTools
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        void Open(Image img)
        {
            _width = img.Width;
            _height = img.Height;
            var orig = new Bitmap(img);

            var clone = new Bitmap(orig.Width, orig.Height, PixelFormat.Format24bppRgb);
            using (var gr = Graphics.FromImage(clone))
            {
                gr.DrawImage(orig, new Rectangle(0, 0, clone.Width, clone.Height));
            }



            SetImage(clone, 0, 0, _scale);

            //get colors

        }

        void DrawImage(Image img, int xoff, int yoff, float scale)
        {
            if (picOut.Image != null)
            {
                using (var gr = Graphics.FromImage(picOut.Image))
                {
                    gr.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                    gr.DrawImage(img, new Rectangle(xoff, yoff, (int)(img.Width * scale), (int)(img.Height * scale)));
                }
                picOut.Refresh();
            }


        }

        void SetImage(Bitmap img, int xoff, int yoff, float scale)
        {
            var scaled = new Bitmap((int)(img.Width * scale), (int)(img.Height * scale), img.PixelFormat);
            _scale = scale;
            picOut.Image = scaled;

            if (_loadedImage != img)
            {
                _loadedImage = img;
                _width = img.Width;
                _height = img.Height;
                _colors = new Color[_width * _height];
                _colorBank = new Dictionary<Color, int>();
                for (var y = 0; y < _height; y++)
                {
                    for (var x = 0; x < _width; x++)
                    {
                        var color = img.GetPixel(x, y);
                        _colors[y * _width + x] = color;
                        //if (!colorBank.ContainsKey(color))
                        //    colorBank.Add(color, 1);
                        //else
                        //    colorBank[color] += 1;
                    }
                }
                lblColors.Text = _colorBank.Count.ToString();
                lsvColors.Items.Clear();
                foreach (var item in _colorBank.OrderByDescending(x => x.Value))
                {
                    lsvColors.Items.Add(new ListViewItem { BackColor = item.Key, Text = item.Value.ToString() });
                }
                _cells = new Dictionary<Color, int>[_width / Cellwidth * (_height / Cellheight)];
            }

            Form1_Resize(this, null);
            DrawImage(_loadedImage, -hScroll.Value, -vScroll.Value, scale);
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.ShowDialog();
            if (!string.IsNullOrEmpty(ofd.FileName))
            {
                Open(Image.FromFile(ofd.FileName));

            }
        }

        float _scale = 1;
        Bitmap _loadedImage;
        int _width;
        int _height;
        const int Cellwidth = 8; const int Cellheight = 8;
        Dictionary<Color, int>[] _cells;
        Color[] _colors;
        Dictionary<Color, int> _colorBank;

        private void picOut_Paint(object sender, PaintEventArgs e)
        {
            if (chkGrid.Checked)
            {
                for (var x = 0; x < _width / Cellwidth; x++)
                {
                    e.Graphics.DrawLine(Pens.Red, x * Cellwidth * _scale, 0, x * Cellwidth * _scale, picOut.Height);
                }
                for (var y = 0; y < _height / Cellheight; y++)
                {
                    e.Graphics.DrawLine(Pens.Red, 0, y * Cellheight * _scale, picOut.Width, y * Cellheight * _scale);
                }
                var fnt = new Font(FontFamily.GenericSansSerif, 9);
                for (var y = 0; y < _height / Cellheight; y++)
                {
                    for (var x = 0; x < _width / Cellwidth; x++)
                    {
                        var dex = y * (_width / Cellwidth) + x;
                        if (_cells[dex] != null)
                        {
                            e.Graphics.DrawString(_cells[dex].Count.ToString(), fnt, Brushes.Red, x * Cellwidth * _scale, y * Cellheight * _scale);

                        }
                    }
                }
            }
        }

        private void picOut_MouseClick(object sender, MouseEventArgs e)
        {
            var cellx = e.X / (int)(Cellwidth * _scale);
            var celly = e.Y / (int)(Cellheight * _scale);
            var dex = celly * (_width / Cellwidth) + cellx;
            _cells[dex] = new Dictionary<Color, int>();
            for (var y = 0; y < Cellheight; y++)
            {
                for (var x = 0; x < Cellwidth; x++)
                {
                    var color = _colors[(y + celly * Cellheight) * _width + x + cellx * Cellwidth];
                    //cellcolors[y * width + x] = color;
                    if (!_cells[dex].ContainsKey(color))
                    {
                        _cells[dex].Add(color, 1);
                    }
                    else
                    {
                        _cells[dex][color] += 1;
                    }
                }
            }
            lblCellColors.Text = _cells[dex].Count.ToString();
            lsvCellColors.Items.Clear();
            foreach (var item in _cells[dex].OrderByDescending(x => x.Value))
            {
                lsvCellColors.Items.Add(new ListViewItem { BackColor = item.Key, Text = item.Value.ToString() });
            }
            picOut.Refresh();



        }


        private void SavePalette(List<Color> pal, string file)
        {
            using (var br = new BinaryWriter(File.Open(file, FileMode.Create)))
            {
                foreach (var c in pal)
                {
                    //ushort us = c.ToRbg15();
                    //br.Write((ushort)((us & 0xff) << 8 | (us & 0xff00) >> 8));
                    br.Write((byte)0);
                    br.Write((byte)c.B);
                    br.Write((byte)c.G);
                    br.Write((byte)c.R);
                }
                br.Close();
            }
        }
        private void SaveImage(Color[] image, int[] map, string file)
        {
            using (var br = new BinaryWriter(File.Open(file, FileMode.Create)))
            {
                for (var y = 0; y < _height; y++)
                {
                    for (var x = 0; x < _width; x++)
                    {
                        var color = _colors[y * _width + x].ToRbg24();
                        br.Write((byte)map[color]);
                    }
                }
                br.Close();
            }
        }
        private void btnProcess_Click(object sender, EventArgs e)
        {
            if (_colors == null)
            {
                MessageBox.Show("colors is null", "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            var cbuff = new int[0xffffff];
            foreach (var c in _colors)
            {
                var color = c.ToRbg24();// 15();
                cbuff[color] = cbuff[color] + 1;
            }

            var palette = Utils.MedianCut(ref cbuff, 256);


            var bmp = new Bmp(_width, _height, 24);
            var pp = 0;
            for (var y = 0; y < _height; y++)
            {
                pp = (_height - 1 - y) * bmp.Rowsize;
                for (var x = 0; x < _width; x++)
                {
                    var color = _colors[y * _width + x].ToRbg24();
                    var realcolor = palette[cbuff[color]].ToRbg24();

                    bmp.Pixels[pp++] = (byte)(realcolor & (int)0xff);
                    bmp.Pixels[pp++] = (byte)((realcolor & (int)0xff00) >> 8);
                    bmp.Pixels[pp++] = (byte)((realcolor & (int)0xff0000) >> 16);

                }
            }

            //SavePalette(palette, "D:\\TEST.PAL");
            //SaveImage(_colors, cbuff, "D:\\TEST.IMG"); ;



            //var stream = File.Open("D:\\test.bmp", FileMode.OpenOrCreate);
            //bmp.Write(stream);
            //stream.Close();
            var ms = new MemoryStream();
            bmp.Write(ms);
            ms.Position = 0;
            SetImage((Bitmap)Bitmap.FromStream(ms), -hScroll.Value, -vScroll.Value, _scale);
        }

        int ScaledHeight
        {
            get
            {
                return (int)(_height * _scale);
            }
        }

        int ScaledWidth
        {
            get
            {
                return (int)(_width * _scale);
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            var imageLoaded = _width > 0 && _height > 0;
            picOut.Width = picOut.Parent.Width - picOut.Left - 40;
            picOut.Height = picOut.Parent.Height - picOut.Top - 70;
            if (imageLoaded)
            {
                if (picOut.Width > ScaledWidth)
                {
                    picOut.Width = ScaledWidth;
                }

                if (picOut.Height > ScaledHeight)
                {
                    picOut.Height = ScaledHeight;
                }
            }
            vScroll.Left = picOut.Right + 3;
            vScroll.Height = picOut.Height;
            hScroll.Top = picOut.Bottom + 3;
            hScroll.Width = picOut.Width;
            if (imageLoaded)
            {
                var ydiff = ScaledHeight - picOut.Height;
                if (ydiff > 0)
                {
                    vScroll.Minimum = 0;
                    vScroll.Maximum = ydiff;
                    vScroll.Enabled = true;
                }
                else
                {
                    vScroll.Minimum = 0;
                    vScroll.Maximum = 0;
                    vScroll.Enabled = false;
                }
                var xdiff = ScaledWidth - picOut.Width;
                if (xdiff > 0)
                {
                    hScroll.Minimum = 0;
                    hScroll.Maximum = xdiff;
                    hScroll.Enabled = true;
                }
                else
                {
                    hScroll.Minimum = 0;
                    hScroll.Maximum = 0;
                    hScroll.Enabled = false;
                }
            }
        }

        private void vScroll_Scroll(object sender, ScrollEventArgs e)
        {
            DrawImage(_loadedImage, -hScroll.Value, -vScroll.Value, _scale);
        }

        private void hScroll_Scroll(object sender, ScrollEventArgs e)
        {
            DrawImage(_loadedImage, -hScroll.Value, -vScroll.Value, _scale);
        }

        private void chkGrid_CheckedChanged(object sender, EventArgs e)
        {
            picOut.Refresh();
        }

        private void btnZoomIn_Click(object sender, EventArgs e)
        {
            _scale *= 2;
            SetImage(_loadedImage, -hScroll.Value, -vScroll.Value, _scale);
        }

        private void btnZoomOut_Click(object sender, EventArgs e)
        {
            _scale /= 2;
            SetImage(_loadedImage, -hScroll.Value, -vScroll.Value, _scale);
        }

        private void analyzeFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.ShowDialog();
            if (!string.IsNullOrEmpty(ofd.FileName))
            {
                var frm = new FrmFileAnalyzer();
                frm.Datafile = ofd.FileName;
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
                var balanceFile = Path.Combine(Path.GetDirectoryName(ofd.FileName), "BALANCE.BIN");
                var balanceBin = new BalanceBin(balanceFile);
                var soundBinFileName = Path.Combine(Path.GetDirectoryName(ofd.FileName), "SOUND.BIN");
                var soundBin = new SoundBin(soundBinFileName);

                frmAlundra.Init(datasBin, balanceBin, soundBin);
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void analyzeCarpetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var ofd = new OpenFileDialog();
            ofd.ShowDialog();
            if (!string.IsNullOrEmpty(ofd.FileName))
            {
                var frm = new FrmCarpetAnalyzer();
                frm.Datafile = ofd.FileName;
                frm.Show();
            }
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

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }

}
