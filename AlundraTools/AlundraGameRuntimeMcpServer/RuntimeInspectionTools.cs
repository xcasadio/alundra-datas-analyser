using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text;

namespace AlundraGameRuntimeMcpServer;

[McpServerToolType]
internal sealed class RuntimeInspectionTools(RuntimeInspectorClient client)
{
    [McpServerTool(Name = "alundra_read_value")]
    [Description("Read a value from the running C# game using a reflection path. Roots: game, engine, static, player, map, renderer.")]
    public async Task<CallToolResult> ReadValue(
        [Description("Reflection path such as 'static.g_currentMap', 'player.PosX', or 'engine.CurrentMap.Info._11'.")] string path,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var value = await client.ReadValueAsync(path, cancellationToken);
            var builder = new StringBuilder();
            builder.AppendLine($"Path: {value.Path}");
            builder.AppendLine($"Type: {value.TypeName}");
            builder.AppendLine($"Summary: {value.Summary}");

            if (!string.IsNullOrWhiteSpace(value.ScalarValue))
            {
                builder.AppendLine($"Scalar: {value.ScalarValue}");
            }

            return ToolSupport.Success(builder.ToString().TrimEnd());
        }
        catch (Exception exception)
        {
            return ToolSupport.Error(exception);
        }
    }

    [McpServerTool(Name = "alundra_list_members")]
    [Description("List fields and properties available on a runtime object path. Leave the path empty to list the available roots.")]
    public async Task<CallToolResult> ListMembers(
        [Description("Optional reflection path. Empty returns roots.")] string path = "",
        CancellationToken cancellationToken = default)
    {
        try
        {
            var members = await client.ListMembersAsync(string.IsNullOrWhiteSpace(path) ? null : path, cancellationToken);
            if (members.Members.Count == 0)
            {
                return ToolSupport.Success($"Path: {members.Path}{Environment.NewLine}Type: {members.TypeName}{Environment.NewLine}No members.");
            }

            var lines = members.Members
                .Select(member => $"[{member.Kind}] {member.Name} : {member.TypeName} = {member.Summary}");

            return ToolSupport.Success($"Path: {members.Path}{Environment.NewLine}Type: {members.TypeName}{Environment.NewLine}{string.Join(Environment.NewLine, lines)}");
        }
        catch (Exception exception)
        {
            return ToolSupport.Error(exception);
        }
    }
}