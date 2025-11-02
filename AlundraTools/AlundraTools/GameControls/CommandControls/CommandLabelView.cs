namespace AlundraTools.GameControls.CommandControls;

public record CommandModel(byte Code, string? ToolTipText);

public record CommandModelLabel(byte Code, string Text, string? ToolTipText) : CommandModel(Code, ToolTipText);

public class CommandLabelView : UserControl
{
    private readonly ToolTip _toolTip = new();

    public CommandLabelView(CommandModelLabel scriptEvent)
    {
        Height = 36;
        var label = new Label
        {
            Dock = DockStyle.Fill,  
            Text = scriptEvent.Text,
            AutoEllipsis = true,
            TextAlign = ContentAlignment.MiddleLeft,
            Padding = new Padding(6, 0, 0, 0),
            UseMnemonic = false
        };

        Controls.Add(label);

        if (!string.IsNullOrWhiteSpace(scriptEvent.ToolTipText))
        {
            _toolTip.SetToolTip(label, scriptEvent.ToolTipText);
        }
    }
}