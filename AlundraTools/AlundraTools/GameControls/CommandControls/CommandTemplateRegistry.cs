namespace AlundraTools.GameControls.CommandControls;

public static class CommandTemplateRegistry
{
    private static readonly Dictionary<Type, Func<object, Control>> _factories = new();

    public static void Register<TEvent>(Func<TEvent, Control> factory)
    {
        _factories[typeof(TEvent)] = (obj) => factory((TEvent)obj);
    }

    public static bool TryCreate(object ev, out Control control)
    {
        var t = ev.GetType();

        if (_factories.TryGetValue(t, out var f))
        {
            control = f(ev);
            return true;
        }
        foreach (var kv in _factories)
        {
            if (kv.Key.IsAssignableFrom(t))
            {
                control = kv.Value(ev);
                return true;
            }
        }
        control = null!;
        return false;
    }
}