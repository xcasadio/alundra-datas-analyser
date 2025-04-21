namespace GraphicsTools
{
    public partial class FrmViewer : Form
    {
        public FrmViewer()
        {
            InitializeComponent();
        }

        Color[] _palette;
        float _scale = 1;
        Bitmap _loadedImage;
        byte[] _imagedata;
        int _width;
        int _height;
        int _palbpp = 16;
        int _bpp = 4;
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

        FrmViewer _viewer;
        bool _isPalette;
        public void InitPalette(FrmViewer viewer, byte[]imagedata, int palbpp, int bpp, int width, int height)
        {
            _bpp = bpp;
            _palbpp = palbpp;
            _viewer = viewer;
            _isPalette = true;
            _scale = 8;
            _imagedata = imagedata;
            _width = width;
            _height = height;
            var bmp = new Bmp(width, height, 24);
            var dex = 0;
            for (var y = height - 1; y >= 0; y--)
            {
                var bmpdex = 0;
                for (var x = 0; x < width; x++)
                {
                    var c = Color.Black;
                    if (palbpp == 16)
                    {
                        var b2 = imagedata[dex++];
                        var b1 = imagedata[dex++];
                        //Color c = Utils.FromPsxColor(b1, b2);
                        c = Utils.FromPsxColor((b1 << 8) | b2);
                    }
                    else if (palbpp == 24)
                    {
                        var r = imagedata[dex++];
                        var g = imagedata[dex++];
                        var b = imagedata[dex++];
                        c = Color.FromArgb(r, g, b);
                    }
                    bmp.Pixels[y * bmp.Rowsize + bmpdex++] = c.R;
                    bmp.Pixels[y * bmp.Rowsize + bmpdex++] = c.G;
                    bmp.Pixels[y * bmp.Rowsize + bmpdex++] = c.B;
                }
            }
            var ms = new MemoryStream();
            bmp.Write(ms);
            ms.Position = 0;
            frmViewer_Resize(this, null);
            _loadedImage = new Bitmap(ms);
            picOut.Image = new Bitmap(ScaledWidth, ScaledHeight);
            DrawImage(_loadedImage, -hScroll.Value, -vScroll.Value, _scale);
        }
        public void Init(byte[] imagedata,int palbpp, int bpp, int width, int height,Color[]palette)
        {
            _bpp = bpp;
            _scale = 4;
            _palbpp = palbpp;
            _imagedata = imagedata;
            _width = width;
            _height = height;
            var bmp = new Bmp(width, height, 24);
            var dex = 0;
            for (var y = height - 1; y >= 0; y--)
            {
                var bmpdex = 0;
                if (bpp == 4)
                {
                    for (var x = 0; x < width / 2; x++)
                    {
                        var c = palette[imagedata[dex] & 0xf];
                        bmp.Pixels[y * bmp.Rowsize + bmpdex++] = c.R;
                        bmp.Pixels[y * bmp.Rowsize + bmpdex++] = c.G;
                        bmp.Pixels[y * bmp.Rowsize + bmpdex++] = c.B;
                        c = palette[(imagedata[dex] & 0xf0) >> 4];
                        bmp.Pixels[y * bmp.Rowsize + bmpdex++] = c.R;
                        bmp.Pixels[y * bmp.Rowsize + bmpdex++] = c.G;
                        bmp.Pixels[y * bmp.Rowsize + bmpdex++] = c.B;
                        dex++;

                    }
                }
                else if (bpp == 8)
                {
                    for (var x = 0; x < width; x++)
                    {
                        var c = palette[imagedata[dex]];
                        bmp.Pixels[y * bmp.Rowsize + bmpdex++] = c.R;
                        bmp.Pixels[y * bmp.Rowsize + bmpdex++] = c.G;
                        bmp.Pixels[y * bmp.Rowsize + bmpdex++] = c.B;
                        dex++;

                    }
                }
                else if (bpp == 1)
                {
                    for (var x = 0; x < width / 8; x++)
                    {

                        for (var shift = 0; shift < 8; shift++)
                        {
                            var test = (byte)(imagedata[dex] & (0x1 << shift));
                            test = (byte)(test != 0 ? 255 : 0);
                            bmp.Pixels[y * bmp.Rowsize + bmpdex++] = test;
                            bmp.Pixels[y * bmp.Rowsize + bmpdex++] = test;
                            bmp.Pixels[y * bmp.Rowsize + bmpdex++] = test;
                        }
                        dex++;

                    }
                }
            }
            var ms = new MemoryStream();
            bmp.Write(ms);
            ms.Position = 0;
            frmViewer_Resize(this, null);
            _loadedImage = new Bitmap(ms);
            picOut.Image = new Bitmap(ScaledWidth, ScaledHeight);
            DrawImage(_loadedImage, -hScroll.Value, -vScroll.Value, _scale);

        }

        public void Updatepalette(Color[] palette)
        {
            var bmp = new Bmp(_width, _height, 24);
            var dex = 0;
            for (var y = _height - 1; y >= 0; y--)
            {
                var bmpdex = 0;
                if (_bpp == 4)
                {
                    for (var x = 0; x < _width / 2; x++)
                    {
                        var c = palette[_imagedata[dex] & 0xf];
                        bmp.Pixels[y * bmp.Rowsize + bmpdex++] = c.R;
                        bmp.Pixels[y * bmp.Rowsize + bmpdex++] = c.G;
                        bmp.Pixels[y * bmp.Rowsize + bmpdex++] = c.B;
                        c = palette[(_imagedata[dex] & 0xf0) >> 4];
                        bmp.Pixels[y * bmp.Rowsize + bmpdex++] = c.R;
                        bmp.Pixels[y * bmp.Rowsize + bmpdex++] = c.G;
                        bmp.Pixels[y * bmp.Rowsize + bmpdex++] = c.B;
                        dex++;

                    }
                }
                else if (_bpp == 8)
                {
                    for (var x = 0; x < _width; x++)
                    {
                        var c = palette[_imagedata[dex]];
                        bmp.Pixels[y * bmp.Rowsize + bmpdex++] = c.R;
                        bmp.Pixels[y * bmp.Rowsize + bmpdex++] = c.G;
                        bmp.Pixels[y * bmp.Rowsize + bmpdex++] = c.B;
                        dex++;

                    }
                }
            }
            var ms = new MemoryStream();
            bmp.Write(ms);
            ms.Position = 0;
            frmViewer_Resize(this, null);
            _loadedImage = new Bitmap(ms);
            DrawImage(_loadedImage, -hScroll.Value, -vScroll.Value, _scale);

        }

        void DrawImage(Image img, int xoff, int yoff, float scale)
        {
            if (picOut.Image != null)
            {
                using (var gr = Graphics.FromImage(picOut.Image))
                {
                    gr.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                    gr.DrawImage(img, new Rectangle(xoff+(int)(scale/2), yoff+ (int)(scale / 2), (int)(img.Width * scale), (int)(img.Height * scale)), new Rectangle(0,0,img.Width,img.Height),GraphicsUnit.Pixel);
                }
                picOut.Refresh();
            }


        }

        private void frmViewer_Resize(object sender, EventArgs e)
        {
            var imageLoaded = _width > 0 && _height > 0;
            if (picOut.Parent != null)
            {
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
        }

        private void vScroll_Scroll(object sender, ScrollEventArgs e)
        {
            DrawImage(_loadedImage, -hScroll.Value, -vScroll.Value, _scale);
        }

        private void hScroll_Scroll(object sender, ScrollEventArgs e)
        {
            DrawImage(_loadedImage, -hScroll.Value, -vScroll.Value, _scale);
        }

        int _palettex, _palettey;
        private void picOut_MouseClick(object sender, MouseEventArgs e)
        {
            if (_isPalette)
            {
                if (_bpp == 4)
                {
                    _palettex = (int)((e.X + hScroll.Value) / _scale);
                    _palettex -= _palettex % 16;
                    _palettey = (int)((e.Y + vScroll.Value) / _scale);
                    if (_palettex < _width && _palettey < _height)
                    {
                        var imagedex = _palettey * _width * 2 + _palettex * 2;
                        var palette = new Color[(int)Math.Pow(2, _bpp)];
                        for (var dex = 0; dex < palette.Length; dex++)
                        {

                            var b2 = _imagedata[imagedex++];
                            var b1 = _imagedata[imagedex++];
                            palette[dex] = Utils.FromPsxColor((b1 << 8) | b2);
                        }
                        if (_viewer != null)
                        {
                            Program.Palette = palette;
                            _viewer.Updatepalette(palette);
                        }
                    }
                }
                else if (_bpp == 8)
                {
                    _palettex = (int)((e.X + hScroll.Value) / _scale);
                    _palettex -= _palettex % 256;
                    _palettey = (int)((e.Y + vScroll.Value) / _scale);
                    if (_palettex < _width && _palettey < _height)
                    {
                        var imagedex = _palettey * _width + _palettex * 2;
                        var palette = new Color[(int)Math.Pow(2, _bpp)];
                        for (var dex = 0; dex < palette.Length; dex++)
                        {

                            var b = _imagedata[imagedex++];
                            var g = _imagedata[imagedex++];
                            var r = _imagedata[imagedex++];
                            palette[dex] = Color.FromArgb(r, g, b);
                        }
                        Program.Palette = palette;
                        if (_viewer != null)
                        {
                            _viewer.Updatepalette(palette);
                        }
                    }
                }
                picOut.Refresh();
            }
        }

        private void picOut_Paint(object sender, PaintEventArgs e)
        {
            if (_isPalette)
            {

                e.Graphics.FillRectangle(Brushes.Red, _palettex * _scale - hScroll.Value, _palettey * _scale - (vScroll.Value - 1), (int)Math.Pow(2,_bpp)*_scale,_scale);
            }
        }
    }
}
