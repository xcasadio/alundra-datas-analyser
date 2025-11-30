using System.ComponentModel;

namespace AlundraTools.CustomControls;

public class ImageViewerControl : UserControl
{
    private Image? _image;
    private readonly float[] _zoomLevels = { 0.5f, 1f, 2f, 4f };
    private int _zoomIndex = 1; // default 1x
    private PointF _translation = new(0, 0);

    private bool _dragging;
    private Point _dragStart;
    private PointF _translationStart;

    public ImageViewerControl()
    {
        DoubleBuffered = true;
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        TabStop = true; // allow focus so wheel events can be received
        // ensure we get mouse wheel when focused
        MouseClick += (s, e) => Focus();
    }

    [Browsable(true)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public Image? Image
    {
        get => _image;
        set
        {
            _image = value;
            ResetView();
            Invalidate();
        }
    }

    [Browsable(false)]
    public float Zoom => _zoomLevels[_zoomIndex];

    // Center on a given tile position (tileX, tileY). Defaults to 24x16 tile size.
    public void CenterAt(int tileX, int tileY, int tileWidth = 24, int tileHeight = 16)
    {
        if (_image == null) return;
        var z = Zoom;
        var px = tileX * tileWidth;
        var py = tileY * tileHeight;
        var cx = Width / 2f;
        var cy = Height / 2f;
        _translation.X = cx - px * z;
        _translation.Y = cy - py * z;
        Invalidate();
    }

    public void SetZoom(float zoom)
    {
        // choose nearest supported zoom level
        var best = 0;
        var bestDiff = float.MaxValue;
        for (var i = 0; i < _zoomLevels.Length; i++)
        {
            var diff = Math.Abs(_zoomLevels[i] - zoom);
            if (diff < bestDiff)
            {
                bestDiff = diff;
                best = i;
            }
        }

        SetZoomIndex(best, new Point(Width / 2, Height / 2));
    }

    public void SetZoomIndex(int index, Point? centerPoint = null)
    {
        if (_image == null) return;
        index = Math.Clamp(index, 0, _zoomLevels.Length - 1);
        if (index == _zoomIndex) return;

        var oldZoom = Zoom;
        var newZoom = _zoomLevels[index];
        var focus = centerPoint ?? new Point(Width / 2, Height / 2);

        // keep the point under the cursor stable while zooming:
        // imageCoord = (focus - translation) / oldZoom
        var imageCoordX = (focus.X - _translation.X) / oldZoom;
        var imageCoordY = (focus.Y - _translation.Y) / oldZoom;

        _zoomIndex = index;

        // newTranslation = focus - imageCoord * newZoom
        _translation.X = focus.X - imageCoordX * newZoom;
        _translation.Y = focus.Y - imageCoordY * newZoom;

        Invalidate();
    }

    public void ResetView()
    {
        if (_image == null)
        {
            _translation = new PointF(0, 0);
            _zoomIndex = 1;
            return;
        }

        // center image
        var z = Zoom;
        var w = _image.Width * z;
        var h = _image.Height * z;
        _translation.X = (Width - w) / 2f;
        _translation.Y = (Height - h) / 2f;
        _zoomIndex = 1;
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        // optional: keep current translation but if image is null nothing to do
        if (_image != null && !_dragging)
        {
            // keep view consistent: do nothing, or re-center on reset:
            // ResetView();
        }

        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.Clear(BackColor);

        if (_image == null)
        {
            // nothing
            return;
        }

        // high quality rendering for zoomed images
        g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
        g.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.HighSpeed;
        g.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighSpeed;

        var z = Zoom;
        var destRect = new RectangleF(_translation.X, _translation.Y, _image.Width * z, _image.Height * z);
        g.DrawImage(_image, destRect);
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (e.Button == MouseButtons.Left)
        {
            _dragging = true;
            _dragStart = e.Location;
            _translationStart = _translation;
            Capture = true;
            Focus();
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (_dragging && e.Button == MouseButtons.Left)
        {
            var dx = e.X - _dragStart.X;
            var dy = e.Y - _dragStart.Y;
            _translation = new PointF(_translationStart.X + dx, _translationStart.Y + dy);
            Invalidate();
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (e.Button == MouseButtons.Left)
        {
            _dragging = false;
            Capture = false;
        }
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);
        if (_image == null) return;

        // choose zoom level step by wheel direction
        if (e.Delta > 0)
        {
            SetZoomIndex(Math.Min(_zoomIndex + 1, _zoomLevels.Length - 1), e.Location);
        }
        else if (e.Delta < 0)
        {
            SetZoomIndex(Math.Max(_zoomIndex - 1, 0), e.Location);
        }
    }

    // optional keyboard support: + / - to zoom
    protected override bool IsInputKey(Keys keyData)
    {
        if (keyData == Keys.Add || keyData == Keys.Subtract || keyData == Keys.Oemplus || keyData == Keys.OemMinus)
            return true;
        return base.IsInputKey(keyData);
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (_image == null) return;

        if (e.KeyCode == Keys.Add || e.KeyCode == Keys.Oemplus)
        {
            SetZoomIndex(Math.Min(_zoomIndex + 1, _zoomLevels.Length - 1), new Point(Width / 2, Height / 2));
        }
        else if (e.KeyCode == Keys.Subtract || e.KeyCode == Keys.OemMinus)
        {
            SetZoomIndex(Math.Max(_zoomIndex - 1, 0), new Point(Width / 2, Height / 2));
        }
    }
}