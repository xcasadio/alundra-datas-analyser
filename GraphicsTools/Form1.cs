using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace GraphicsTools
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }


        void open(Image img)
        {
            width = img.Width;
            height = img.Height;
            Bitmap orig = new Bitmap(img);

            Bitmap clone = new Bitmap(orig.Width, orig.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
            using (Graphics gr = Graphics.FromImage(clone))
            {
                gr.DrawImage(orig, new Rectangle(0, 0, clone.Width, clone.Height));
            }



            setImage(clone, 0, 0, scale);

            //get colors

        }

        void drawImage(Image img, int xoff, int yoff, float scale)
        {
            if (picOut.Image != null)
            {
                using (Graphics gr = Graphics.FromImage(picOut.Image))
                {
                    gr.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                    gr.DrawImage(img, new Rectangle(xoff, yoff, (int)(img.Width * scale), (int)(img.Height * scale)));
                }
                picOut.Refresh();
            }


        }

        void setImage(Bitmap img, int xoff, int yoff, float scale)
        {
            Bitmap scaled = new Bitmap((int)(img.Width * scale), (int)(img.Height * scale), img.PixelFormat);
            this.scale = scale;
            picOut.Image = scaled;

            if (loadedImage != img)
            {
                loadedImage = img;
                width = img.Width;
                height = img.Height;
                colors = new Color[width * height];
                colorBank = new Dictionary<Color, int>();
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        var color = img.GetPixel(x, y);
                        colors[y * width + x] = color;
                        //if (!colorBank.ContainsKey(color))
                        //    colorBank.Add(color, 1);
                        //else
                        //    colorBank[color] += 1;
                    }
                }
                lblColors.Text = colorBank.Count.ToString();
                lsvColors.Items.Clear();
                foreach (var item in colorBank.OrderByDescending(x => x.Value))
                {
                    lsvColors.Items.Add(new ListViewItem { BackColor = item.Key, Text = item.Value.ToString() });
                }
                cells = new Dictionary<Color, int>[(width / cellwidth) * (height / cellheight)];
            }

            Form1_Resize(this, null);
            drawImage(loadedImage, -hScroll.Value, -vScroll.Value, scale);
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.ShowDialog();
            if (!string.IsNullOrEmpty(ofd.FileName))
            {
                open(Image.FromFile(ofd.FileName));

            }
        }

        float scale = 1;
        Bitmap loadedImage;
        int width;
        int height;
        const int cellwidth = 8; const int cellheight = 8;
        Dictionary<Color, int>[] cells;
        Color[] colors;
        Dictionary<Color, int> colorBank;

        private void picOut_Paint(object sender, PaintEventArgs e)
        {
            if (chkGrid.Checked)
            {
                for (int x = 0; x < width / cellwidth; x++)
                {
                    e.Graphics.DrawLine(Pens.Red, x * cellwidth * scale, 0, x * cellwidth * scale, picOut.Height);
                }
                for (int y = 0; y < height / cellheight; y++)
                {
                    e.Graphics.DrawLine(Pens.Red, 0, y * cellheight * scale, picOut.Width, y * cellheight * scale);
                }
                Font fnt = new Font(FontFamily.GenericSansSerif, 9);
                for (int y = 0; y < height / cellheight; y++)
                {
                    for (int x = 0; x < width / cellwidth; x++)
                    {
                        int dex = y * (width / cellwidth) + x;
                        if (cells[dex] != null)
                        {
                            e.Graphics.DrawString(cells[dex].Count.ToString(), fnt, Brushes.Red, x * cellwidth * scale, y * cellheight * scale);

                        }
                    }
                }
            }
        }

        private void picOut_MouseClick(object sender, MouseEventArgs e)
        {
            int cellx = e.X / (int)(cellwidth * scale);
            int celly = e.Y / (int)(cellheight * scale);
            int dex = celly * (width / cellwidth) + cellx;
            cells[dex] = new Dictionary<Color, int>();
            for (int y = 0; y < cellheight; y++)
            {
                for (int x = 0; x < cellwidth; x++)
                {
                    var color = colors[(y + celly * cellheight) * width + (x + cellx * cellwidth)];
                    //cellcolors[y * width + x] = color;
                    if (!cells[dex].ContainsKey(color))
                        cells[dex].Add(color, 1);
                    else
                        cells[dex][color] += 1;
                }
            }
            lblCellColors.Text = cells[dex].Count.ToString();
            lsvCellColors.Items.Clear();
            foreach (var item in cells[dex].OrderByDescending(x => x.Value))
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
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        int color = colors[y * width + x].ToRbg24();
                        br.Write((byte)map[color]);
                    }
                }
                br.Close();
            }
        }
        private void btnProcess_Click(object sender, EventArgs e)
        {
            if (colors == null)
            {
                MessageBox.Show("colors is null", "error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int[] cbuff = new int[0xffffff];
            foreach (var c in colors)
            {
                int color = c.ToRbg24();// 15();
                cbuff[color] = cbuff[color] + 1;
            }

            var palette = Utils.MedianCut(ref cbuff, 256);


            var bmp = new bmp(width, height, 24);
            int pp = 0;
            for (int y = 0; y < height; y++)
            {
                pp = ((height - 1) - y) * bmp.rowsize;
                for (int x = 0; x < width; x++)
                {
                    int color = colors[y * width + x].ToRbg24();
                    int realcolor = palette[cbuff[color]].ToRbg24();

                    bmp.pixels[pp++] = (byte)(realcolor & (int)0xff);
                    bmp.pixels[pp++] = (byte)((realcolor & (int)0xff00) >> 8);
                    bmp.pixels[pp++] = (byte)((realcolor & (int)0xff0000) >> 16);

                }
            }

            SavePalette(palette, "D:\\TEST.PAL");
            SaveImage(colors, cbuff, "D:\\TEST.IMG"); ;



            //var stream = File.Open("D:\\test.bmp", FileMode.OpenOrCreate);
            //bmp.Write(stream);
            //stream.Close();
            MemoryStream ms = new MemoryStream();
            bmp.Write(ms);
            ms.Position = 0;
            setImage((Bitmap)Bitmap.FromStream(ms), -hScroll.Value, -vScroll.Value, scale);
        }

        int ScaledHeight
        {
            get
            {
                return (int)(height * scale);
            }
        }

        int ScaledWidth
        {
            get
            {
                return (int)(width * scale);
            }
        }

        private void Form1_Resize(object sender, EventArgs e)
        {
            bool imageLoaded = width > 0 && height > 0;
            picOut.Width = picOut.Parent.Width - picOut.Left - 40;
            picOut.Height = picOut.Parent.Height - picOut.Top - 70;
            if (imageLoaded)
            {
                if (picOut.Width > ScaledWidth)
                    picOut.Width = ScaledWidth;
                if (picOut.Height > ScaledHeight)
                    picOut.Height = ScaledHeight;
            }
            vScroll.Left = picOut.Right + 3;
            vScroll.Height = picOut.Height;
            hScroll.Top = picOut.Bottom + 3;
            hScroll.Width = picOut.Width;
            if (imageLoaded)
            {
                int ydiff = ScaledHeight - picOut.Height;
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
                int xdiff = ScaledWidth - picOut.Width;
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
            drawImage(loadedImage, -hScroll.Value, -vScroll.Value, scale);
        }

        private void hScroll_Scroll(object sender, ScrollEventArgs e)
        {
            drawImage(loadedImage, -hScroll.Value, -vScroll.Value, scale);
        }

        private void chkGrid_CheckedChanged(object sender, EventArgs e)
        {
            picOut.Refresh();
        }

        private void btnZoomIn_Click(object sender, EventArgs e)
        {
            scale *= 2;
            setImage(loadedImage, -hScroll.Value, -vScroll.Value, scale);
        }

        private void btnZoomOut_Click(object sender, EventArgs e)
        {
            scale /= 2;
            setImage(loadedImage, -hScroll.Value, -vScroll.Value, scale);
        }

        private void analyzeFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.ShowDialog();
            if (!string.IsNullOrEmpty(ofd.FileName))
            {
                var frm = new frmFileAnalyzer();
                frm.datafile = ofd.FileName;
                frm.Show();
            }
        }


        private void openDATASBINToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "DATAS.BIN|DATAS.BIN|All Files (*.*)|*.*";
            ofd.ShowDialog();
            if (!string.IsNullOrWhiteSpace(ofd.FileName))
            {
                var frmAlundra = new Alundra.frmAlundra();
                frmAlundra.Show();
                var datasBin = new Alundra.DatasBin(ofd.FileName);
                frmAlundra.Init(datasBin);

                //show soundboard too
                var frmSoundboard = new Alundra.frmSoundboard(datasBin, Path.Combine(Path.GetDirectoryName(ofd.FileName), "SOUND.BIN"));
                frmSoundboard.Show();
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void analyzeCarpetToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.ShowDialog();
            if (!string.IsNullOrEmpty(ofd.FileName))
            {
                var frm = new frmCarpetAnalyzer();
                frm.datafile = ofd.FileName;
                frm.Show();
            }
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "DATAS.BIN|DATAS.BIN|All Files (*.*)|*.*";
            ofd.ShowDialog();
            if (!string.IsNullOrWhiteSpace(ofd.FileName))
            {
                var frmGame = new Alundra.frmGame(new Alundra.DatasBin(ofd.FileName), new Alundra.SoundBin(Path.Combine(Path.GetDirectoryName(ofd.FileName), "SOUND.BIN")));
                frmGame.Show();
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }

}
