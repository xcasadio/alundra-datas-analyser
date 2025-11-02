namespace AlundraTools.GameControls.CommandControls;

public class CommandListView : UserControl
{
    private readonly Panel _stack = new()
    {
        Dock = DockStyle.Fill,
        AutoScroll = true
    };

    private readonly List<CommandModel> _items = [];

    public CommandListView()
    {
        Controls.Add(_stack);
    }

    public void SetItems(IEnumerable<CommandModel> eventsEnum)
    {
        SuspendLayout();
        _stack.SuspendLayout();

        foreach (Control c in _stack.Controls) c.Dispose();
        _stack.Controls.Clear();
        _items.Clear();

        foreach (var ev in eventsEnum)
        {
            AddItem(ev);
        }

        _stack.ResumeLayout();
        ResumeLayout();
    }

    public void AddItem(CommandModel ev)
    {
        if (!CommandTemplateRegistry.TryCreate(ev, out var content))
        {
            content = CreateFallback(ev);
        }
        var rowIndex = _items.Count;
        _items.Add(ev);
        var row = new CommandRowControl { Dock = DockStyle.Top };
        row.Bind(rowIndex, ev.Code, content);
        _stack.Controls.Add(row);
        _stack.Controls.SetChildIndex(row, 0);
    }

    public void AddItems(IEnumerable<CommandModel> eventsEnum)
    {
        foreach (var ev in eventsEnum)
        {
            AddItem(ev);
        }
    }

    protected override void OnResize(System.EventArgs e)
    {
        base.OnResize(e);
        foreach (Control c in _stack.Controls)
        {
            c.Width = _stack.ClientSize.Width;
        }
    }

    private void InitializeComponent()
    {

    }

    private Control CreateFallback(object ev)
    {
        var pg = new Label
        {
            Text = ev?.ToString() ?? "<null object>",
            Dock = DockStyle.Fill
        };
        return pg;
    }
}