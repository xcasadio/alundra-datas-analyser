using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.ComponentModel;

namespace AlundraGameRuntimeMcpServer;

[McpServerToolType]
internal sealed class RuntimeTraceTools(RuntimeInspectorClient client)
{
    [McpServerTool(Name = "alundra_get_trace")]
    [Description("Return the recent execution checkpoints emitted by the running C# game.")]
    public async Task<CallToolResult> GetTrace(
        [Description("Number of recent entries to return (default 20).")] int count = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var trace = await client.GetTraceAsync(count, cancellationToken);
            if (trace.Entries.Count == 0)
            {
                return ToolSupport.Success("No runtime trace entries recorded yet.");
            }

            var lines = trace.Entries.Select(entry =>
                $"#{entry.Sequence} [{entry.TimestampUtc:O}] frame={entry.Frame} checkpoint={entry.Checkpoint}");

            return ToolSupport.Success(string.Join(Environment.NewLine, lines));
        }
        catch (Exception exception)
        {
            return ToolSupport.Error(exception);
        }
    }
}