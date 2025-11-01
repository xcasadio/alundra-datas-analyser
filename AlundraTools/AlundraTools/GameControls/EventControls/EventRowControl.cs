namespace AlundraTools.GameControls.EventControls;

public class EventRowControl : UserControl
{
    private readonly Label _lblIndex = new() { AutoSize = false, TextAlign = ContentAlignment.MiddleRight, Width = 48, Dock = DockStyle.Left };
    private readonly Panel _host = new() { Dock = DockStyle.Fill };

    public EventRowControl()
    {
        Height = 40;
        Padding = new Padding(0, 1, 0, 1);
        Margin = new Padding(0);
        Controls.Add(_host);
        Controls.Add(_lblIndex);
        Paint += (s, e) =>
        {
            using var p = new Pen(Color.FromArgb(230, 230, 230));
            e.Graphics.DrawLine(p, 0, Height - 1, Width, Height - 1);
        };
    }

    public void Bind(int index, Control content)
    {
        _lblIndex.Text = index.ToString();
        _host.Controls.Clear();
        content.Dock = DockStyle.Fill;
        content.TabStop = false;
        _host.Controls.Add(content);
    }
}