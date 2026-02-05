using System.Diagnostics;

namespace AlundraEngine;

public static class Breakpoint
{
    [Conditional("DEBUG")]
    public static void TriggerBreak()
    {
        Debugger.Break();
    }
}