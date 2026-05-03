using ModelContextProtocol.Protocol;

namespace AlundraGameRuntimeMcpServer;

internal static class ToolSupport
{
    public static CallToolResult Success(string text)
        => new()
        {
            Content = [new TextContentBlock { Text = text }],
        };

    public static CallToolResult Error(Exception exception)
        => Error(exception.Message);

    public static CallToolResult Error(string message)
        => new()
        {
            IsError = true,
            Content = [new TextContentBlock { Text = $"Error: {message}" }],
        };
}