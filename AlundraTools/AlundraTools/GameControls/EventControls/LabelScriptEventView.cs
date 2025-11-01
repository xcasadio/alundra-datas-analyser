namespace AlundraTools.GameControls.EventControls;

public record ScriptEvent(byte Code, string? ToolTipText);

public record LabelScriptEvent(byte Code, string Text, string? ToolTipText) : ScriptEvent(Code, ToolTipText);

public class LabelScriptEventView : UserControl
{
    private readonly ToolTip _toolTip = new();

    public LabelScriptEventView(LabelScriptEvent scriptEvent)
    {
        Height = 36;
        var label = new Label
        {
            Dock = DockStyle.Fill,  
            Text = scriptEvent.Text,
            AutoEllipsis = true,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(6, 0, 0, 0)
        };

        Controls.Add(label);

        if (!string.IsNullOrWhiteSpace(scriptEvent.ToolTipText))
        {
            _toolTip.SetToolTip(label, scriptEvent.ToolTipText);
        }
    }
}