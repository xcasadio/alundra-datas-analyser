namespace AlundraTools.GameControls.EventControls;

public record LabelEvent(string Text);

public class LabelEventView : UserControl
{
    public LabelEventView(LabelEvent @event)
    {
        Height = 36;
        var lbl = new Label
        {
            Dock = DockStyle.Fill,
            Text = @event.Text,
            AutoEllipsis = true,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(6, 0, 0, 0)
        };
        Controls.Add(lbl);
    }
}