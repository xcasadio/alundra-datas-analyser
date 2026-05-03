using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text;

namespace AlundraGameRuntimeMcpServer;

[McpServerToolType]
internal sealed class RuntimeStatusTools(RuntimeInspectorClient client)
{
    [McpServerTool(Name = "alundra_get_status")]
    [Description("Get a compact snapshot of the running C# game's state from the main thread.")]
    public async Task<CallToolResult> GetStatus(CancellationToken cancellationToken = default)
    {
        try
        {
            var status = await client.GetStatusAsync(cancellationToken);
            var builder = new StringBuilder();
            builder.AppendLine($"Checkpoint: {status.Checkpoint}");
            builder.AppendLine($"Frame: {status.Frame}");
            builder.AppendLine($"Paused: {status.IsPaused}");
            builder.AppendLine($"DoNextFrame: {status.DoNextFrame}");
            builder.AppendLine($"CurrentMap: {status.CurrentMap}");
            builder.AppendLine($"DesiredMap: {status.DesiredMap}");

            if (status.Player != null)
            {
                builder.AppendLine($"Player: pos=({status.Player.PosX}, {status.Player.PosY}, {status.Player.PosZ}) hp={status.Player.Hp}/{status.Player.HpMax} anim={status.Player.CurrentAnimationId}->{status.Player.TargetAnimationId}");
            }

            return ToolSupport.Success(builder.ToString().TrimEnd());
        }
        catch (Exception exception)
        {
            return ToolSupport.Error(exception);
        }
    }

    [McpServerTool(Name = "alundra_get_stacktrace")]
    [Description("Capture the stack trace of the game thread at the next runtime inspection checkpoint.")]
    public async Task<CallToolResult> GetStackTrace(CancellationToken cancellationToken = default)
    {
        try
        {
            var stack = await client.GetStackAsync(cancellationToken);
            return ToolSupport.Success($"Checkpoint: {stack.Checkpoint}{Environment.NewLine}{stack.StackTrace}".TrimEnd());
        }
        catch (Exception exception)
        {
            return ToolSupport.Error(exception);
        }
    }
}