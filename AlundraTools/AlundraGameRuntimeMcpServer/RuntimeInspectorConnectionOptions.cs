using System.Globalization;

namespace AlundraGameRuntimeMcpServer;

internal sealed record RuntimeInspectorConnectionOptions(string PipeName, TimeSpan ConnectTimeout)
{
    public static RuntimeInspectorConnectionOptions FromEnvironment()
    {
        var pipeName = Environment.GetEnvironmentVariable("ALUNDRA_RUNTIME_PIPE") ?? "alundra-csharp-runtime";
        var timeoutMs = ParseInt("ALUNDRA_RUNTIME_CONNECT_TIMEOUT", 5000);
        return new RuntimeInspectorConnectionOptions(pipeName, TimeSpan.FromMilliseconds(Math.Clamp(timeoutMs, 100, 60000)));
    }

    private static int ParseInt(string variableName, int defaultValue)
    {
        var raw = Environment.GetEnvironmentVariable(variableName);
        return int.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value) ? value : defaultValue;
    }
}