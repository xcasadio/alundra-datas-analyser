using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace GraphicsTools
{
    public partial class frmViewer : Form
    {
        public frmViewer()
        {
            InitializeComponent();
        }

        Color[] palette;
        float scale = 1;
        Bitmap loadedImage;
        byte[] imagedata;
        int width;
        int height;
        int palbpp = 16;
        int bpp = 4;
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

        frmViewer viewer;
        bool isPalette;
        public void initpalette(frmViewer viewer, byte[]imagedata,int palbpp, int bpp, int width, int height)
        {
            this.bpp = bpp;
            this.palbpp = palbpp;
            this.viewer = viewer;
            isPalette = true;
            scale = 8;
            this.imagedata = imagedata;
            this.width = width;
            this.height = height;
            bmp bmp = new bmp(width, height, 24);
            int dex = 0;
            for (int y = height - 1; y >= 0; y--)
            {
                int bmpdex = 0;
                for (int x = 0; x < width; x++)
                {
                    Color c = Color.Black;
                    if (palbpp == 16)
                    {
                        byte b2 = imagedata[dex++];
                        byte b1 = imagedata[dex++];
                        //Color c = Utils.FromPsxColor(b1, b2);
                        c = Utils.FromPsxColor((b1 << 8) | b2);
                    }
                    else if (palbpp == 24)
                    {
                        byte r = imagedata[dex++];
                        byte g = imagedata[dex++];
                        byte b = imagedata[dex++];
                        c = Color.FromArgb(r, g, b);
                    }
                    bmp.pixels[y * bmp.rowsize + bmpdex++] = c.R;
                    bmp.pixels[y * bmp.rowsize + bmpdex++] = c.G;
                    bmp.pixels[y * bmp.rowsize + bmpdex++] = c.B;
                }
            }
            var ms = new MemoryStream();
            bmp.Write(ms);
            ms.Position = 0;
            frmViewer_Resize(this, null);
            loadedImage = new Bitmap(ms);
            picOut.Image = new Bitmap(ScaledWidth, ScaledHeight);
            drawImage(loadedImage, -hScroll.Value, -vScroll.Value, scale);
        }
        public void init(byte[] imagedata,int palbpp, int bpp, int width, int height,Color[]palette)
        {
            this.bpp = bpp;
            this.scale = 4;
            this.palbpp = palbpp;
            this.imagedata = imagedata;
            this.width = width;
            this.height = height;
            bmp bmp = new bmp(width, height, 24);
            int dex = 0;
            for (int y = height - 1; y >= 0; y--)
            {
                int bmpdex = 0;
                if (bpp == 4)
                {
                    for (int x = 0; x < width / 2; x++)
                    {
                        Color c = palette[imagedata[dex] & 0xf];
                        bmp.pixels[y * bmp.rowsize + bmpdex++] = c.R;
                        bmp.pixels[y * bmp.rowsize + bmpdex++] = c.G;
                        bmp.pixels[y * bmp.rowsize + bmpdex++] = c.B;
                        c = palette[(imagedata[dex] & 0xf0) >> 4];
                        bmp.pixels[y * bmp.rowsize + bmpdex++] = c.R;
                        bmp.pixels[y * bmp.rowsize + bmpdex++] = c.G;
                        bmp.pixels[y * bmp.rowsize + bmpdex++] = c.B;
                        dex++;

                    }
                }
                else if (bpp == 8)
                {
                    for (int x = 0; x < width; x++)
                    {
                        Color c = palette[imagedata[dex]];
                        bmp.pixels[y * bmp.rowsize + bmpdex++] = c.R;
                        bmp.pixels[y * bmp.rowsize + bmpdex++] = c.G;
                        bmp.pixels[y * bmp.rowsize + bmpdex++] = c.B;
                        dex++;

                    }
                }
                else if (bpp == 1)
                {
                    for (int x = 0; x < width / 8; x++)
                    {

                        for (int shift = 0; shift < 8; shift++)
                        {
                            byte test = (byte)(imagedata[dex] & (0x1 << shift));
                            test = (byte)(test != 0 ? 255 : 0);
                            bmp.pixels[y * bmp.rowsize + bmpdex++] = test;
                            bmp.pixels[y * bmp.rowsize + bmpdex++] = test;
                            bmp.pixels[y * bmp.rowsize + bmpdex++] = test;
                        }
                        dex++;

                    }
                }
            }
            var ms = new MemoryStream();
            bmp.Write(ms);
            ms.Position = 0;
            frmViewer_Resize(this, null);
            loadedImage = new Bitmap(ms);
            picOut.Image = new Bitmap(ScaledWidth, ScaledHeight);
            drawImage(loadedImage, -hScroll.Value, -vScroll.Value, scale);

        }

        public void updatepalette(Color[] palette)
        {
            bmp bmp = new bmp(width, height, 24);
            int dex = 0;
            for (int y = height - 1; y >= 0; y--)
            {
                int bmpdex = 0;
                if (bpp == 4)
                {
                    for (int x = 0; x < width / 2; x++)
                    {
                        Color c = palette[imagedata[dex] & 0xf];
                        bmp.pixels[y * bmp.rowsize + bmpdex++] = c.R;
                        bmp.pixels[y * bmp.rowsize + bmpdex++] = c.G;
                        bmp.pixels[y * bmp.rowsize + bmpdex++] = c.B;
                        c = palette[(imagedata[dex] & 0xf0) >> 4];
                        bmp.pixels[y * bmp.rowsize + bmpdex++] = c.R;
                        bmp.pixels[y * bmp.rowsize + bmpdex++] = c.G;
                        bmp.pixels[y * bmp.rowsize + bmpdex++] = c.B;
                        dex++;

                    }
                }
                else if (bpp == 8)
                {
                    for (int x = 0; x < width; x++)
                    {
                        Color c = palette[imagedata[dex]];
                        bmp.pixels[y * bmp.rowsize + bmpdex++] = c.R;
                        bmp.pixels[y * bmp.rowsize + bmpdex++] = c.G;
                        bmp.pixels[y * bmp.rowsize + bmpdex++] = c.B;
                        dex++;

                    }
                }
            }
            var ms = new MemoryStream();
            bmp.Write(ms);
            ms.Position = 0;
            frmViewer_Resize(this, null);
            loadedImage = new Bitmap(ms);
            drawImage(loadedImage, -hScroll.Value, -vScroll.Value, scale);

        }

        void drawImage(Image img, int xoff, int yoff, float scale)
        {
            if (picOut.Image != null)
            {
                using (Graphics gr = Graphics.FromImage(picOut.Image))
                {
                    gr.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                    gr.DrawImage(img, new Rectangle(xoff+(int)(scale/2), yoff+ (int)(scale / 2), (int)(img.Width * scale), (int)(img.Height * scale)), new Rectangle(0,0,img.Width,img.Height),GraphicsUnit.Pixel);
                }
                picOut.Refresh();
            }


        }

        private void frmViewer_Resize(object sender, EventArgs e)
        {
            bool imageLoaded = width > 0 && height > 0;
            if (picOut.Parent != null)
            {
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
        }

        private void vScroll_Scroll(object sender, ScrollEventArgs e)
        {
            drawImage(loadedImage, -hScroll.Value, -vScroll.Value, scale);
        }

        private void hScroll_Scroll(object sender, ScrollEventArgs e)
        {
            drawImage(loadedImage, -hScroll.Value, -vScroll.Value, scale);
        }

        int palettex, palettey;
        private void picOut_MouseClick(object sender, MouseEventArgs e)
        {
            if (isPalette)
            {
                if (bpp == 4)
                {
                    palettex = (int)((e.X + hScroll.Value) / scale);
                    palettex -= palettex % 16;
                    palettey = (int)((e.Y + vScroll.Value) / scale);
                    if (palettex < width && palettey < height)
                    {
                        int imagedex = palettey * width * 2 + palettex * 2;
                        var palette = new Color[(int)Math.Pow(2, bpp)];
                        for (int dex = 0; dex < palette.Length; dex++)
                        {

                            byte b2 = imagedata[imagedex++];
                            byte b1 = imagedata[imagedex++];
                            palette[dex] = Utils.FromPsxColor((b1 << 8) | b2);
                        }
                        if (viewer != null)
                        {
                            GraphicsTools.Program.palette = palette;
                            viewer.updatepalette(palette);
                        }
                    }
                }
                else if (bpp == 8)
                {
                    palettex = (int)((e.X + hScroll.Value) / scale);
                    palettex -= palettex % 256;
                    palettey = (int)((e.Y + vScroll.Value) / scale);
                    if (palettex < width && palettey < height)
                    {
                        int imagedex = palettey * width + palettex * 2;
                        var palette = new Color[(int)Math.Pow(2, bpp)];
                        for (int dex = 0; dex < palette.Length; dex++)
                        {

                            byte b = imagedata[imagedex++];
                            byte g = imagedata[imagedex++];
                            byte r = imagedata[imagedex++];
                            palette[dex] = Color.FromArgb(r, g, b);
                        }
                        GraphicsTools.Program.palette = palette;
                        if (viewer != null)
                        {
                            viewer.updatepalette(palette);
                        }
                    }
                }
                picOut.Refresh();
            }
        }

        private void picOut_Paint(object sender, PaintEventArgs e)
        {
            if (isPalette)
            {

                e.Graphics.FillRectangle(Brushes.Red, palettex * scale - hScroll.Value, (palettey * scale) - (vScroll.Value - 1), (int)Math.Pow(2,bpp)*scale,scale);
            }
        }
    }
}
