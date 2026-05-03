using System.IO.Pipes;
using System.Text;
using System.Text.Json;

namespace AlundraGameRuntimeMcpServer;

internal sealed class RuntimeInspectorClient(RuntimeInspectorConnectionOptions options)
{
    public Task<RuntimeStatusSnapshot> GetStatusAsync(CancellationToken cancellationToken = default)
        => SendAsync<RuntimeStatusSnapshot>("status", null, cancellationToken);

    public Task<RuntimeStackSnapshot> GetStackAsync(CancellationToken cancellationToken = default)
        => SendAsync<RuntimeStackSnapshot>("stack", null, cancellationToken);

    public Task<RuntimeValueSnapshot> ReadValueAsync(string path, CancellationToken cancellationToken = default)
        => SendAsync<RuntimeValueSnapshot>("read", new RuntimePathRequest { Path = path }, cancellationToken);

    public Task<RuntimeMembersSnapshot> ListMembersAsync(string? path, CancellationToken cancellationToken = default)
        => SendAsync<RuntimeMembersSnapshot>("members", new RuntimePathRequest { Path = path }, cancellationToken);

    public Task<RuntimeTraceSnapshot> GetTraceAsync(int count, CancellationToken cancellationToken = default)
        => SendAsync<RuntimeTraceSnapshot>("trace", new RuntimeTraceRequest { Count = count }, cancellationToken);

    private async Task<T> SendAsync<T>(string command, object? arguments, CancellationToken cancellationToken)
    {
        using var pipe = new NamedPipeClientStream(".", options.PipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(options.ConnectTimeout);

        await pipe.ConnectAsync(timeout.Token).ConfigureAwait(false);

        using var reader = new StreamReader(pipe, Encoding.UTF8, false, 4096, leaveOpen: true);
        using var writer = new StreamWriter(pipe, new UTF8Encoding(false), 4096, leaveOpen: true)
        {
            AutoFlush = true,
        };

        var request = new RuntimeInspectionRequest
        {
            Command = command,
            Arguments = arguments == null
                ? default
                : JsonSerializer.SerializeToElement(arguments, RuntimeInspectorJson.Options),
        };

        await writer.WriteLineAsync(JsonSerializer.Serialize(request, RuntimeInspectorJson.Options)).ConfigureAwait(false);
        var line = await reader.ReadLineAsync().ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(line))
        {
            throw new InvalidOperationException("The running game returned an empty response.");
        }

        var response = JsonSerializer.Deserialize<RuntimeInspectionResponse>(line, RuntimeInspectorJson.Options)
                       ?? throw new InvalidOperationException("The running game returned an invalid response.");

        if (!response.Success)
        {
            throw new InvalidOperationException(response.Error ?? "Runtime inspection failed.");
        }

        var value = response.Data.Deserialize<T>(RuntimeInspectorJson.Options);
        return value ?? throw new InvalidOperationException("The running game returned an empty payload.");
    }
}