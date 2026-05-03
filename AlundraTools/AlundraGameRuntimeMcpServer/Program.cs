using System.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace AlundraGameRuntimeMcpServer;

internal static class Program
{
    private const string ServerProcessName = "AlundraGameRuntimeMcpServer";

    public static async Task Main(string[] args)
    {
        TerminateOtherServerInstances();

        var builder = Host.CreateApplicationBuilder(args);

        builder.Logging.ClearProviders();
        builder.Logging.AddConsole(options =>
        {
            options.LogToStandardErrorThreshold = LogLevel.Trace;
        });

        var options = RuntimeInspectorConnectionOptions.FromEnvironment();

        builder.Services.AddSingleton(options);
        builder.Services.AddSingleton<RuntimeInspectorClient>();

        builder.Services
            .AddMcpServer(serverOptions =>
            {
                serverOptions.ServerInfo = new Implementation
                {
                    Name = "alundra-csharp-runtime",
                    Version = "1.0.0",
                    Title = "Alundra C# Runtime Inspector",
                    Description = "MCP server for inspecting the running C# Alundra process: game status, object values, member listings, trace checkpoints, and main-thread stack snapshots.",
                };
                serverOptions.ServerInstructions = "Use these tools to inspect the running C# game while comparing it against the original game. Paths are rooted at game, engine, static, player, map, and renderer.";
            })
            .WithStdioServerTransport()
            .WithToolsFromAssembly();

        await builder.Build().RunAsync();
    }

    private static void TerminateOtherServerInstances()
    {
        using var currentProcess = Process.GetCurrentProcess();

        foreach (var process in Process.GetProcessesByName(ServerProcessName))
        {
            using (process)
            {
                if (process.Id == currentProcess.Id)
                {
                    continue;
                }

                try
                {
                    if (process.HasExited)
                    {
                        continue;
                    }

                    process.Kill(entireProcessTree: true);
                    process.WaitForExit(TimeSpan.FromSeconds(2));
                }
                catch (Exception exception)
                {
                    Console.Error.WriteLine($"Failed to stop previous {ServerProcessName} instance {process.Id}: {exception.Message}");
                }
            }
        }
    }
}