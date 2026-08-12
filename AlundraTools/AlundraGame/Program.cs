using System;
using AlundraEngine;

namespace AlundraGame;

public static class AlundraGameRunner
{
    // JUSTIFICATION: backend MonoGame only
    public static void Run(string? datasBinFilePath = null, int forceMapId = -1, string? gameStateFileName = null)
    {
        StaticVariables.ForceDesiredMap = forceMapId;
        StaticVariables.GameStateFileNameToLoad = gameStateFileName;

        try
        {
            using var game = new AlundraGame(datasBinFilePath);
            game.Run();
        }
        finally
        {
            StaticVariables.ForceDesiredMap = -1;
            StaticVariables.GameStateFileNameToLoad = null;
        }
    }
}

internal static class Program
{
    // JUSTIFICATION: backend MonoGame only
    [STAThread]
    private static void Main(string[] args)
    {
        // JUSTIFICATION: backend MonoGame only.
        // The engine reports what it could not load through Debug.WriteLine - a missing movie, a
        // string table that would not parse, an extraction that costs the movie audio. Without a
        // listener those only reach an attached debugger, so running the built exe gives no clue
        // why something is missing. Routing them to stdout makes them visible to anyone who
        // redirects it; in a Release build Debug.WriteLine compiles out and this costs nothing.
        System.Diagnostics.Trace.Listeners.Add(new System.Diagnostics.ConsoleTraceListener());
        System.Diagnostics.Trace.AutoFlush = true;

        string? datasBinFilePath = null;
        int forceMapId = -1;
        string? gameStateFileName = null;

        for (int index = 0; index < args.Length; index++)
        {
            string argument = args[index];
            if (string.Equals(argument, "--datas-bin", StringComparison.OrdinalIgnoreCase) && index + 1 < args.Length)
            {
                datasBinFilePath = args[++index];
            }
            else if (string.Equals(argument, "--game-state-file", StringComparison.OrdinalIgnoreCase) && index + 1 < args.Length)
            {
                gameStateFileName = args[++index];
            }
            else if (string.Equals(argument, "--force-map", StringComparison.OrdinalIgnoreCase) && index + 1 < args.Length && int.TryParse(args[++index], out int mapId))
            {
                forceMapId = mapId;
            }
        }

        AlundraGameRunner.Run(datasBinFilePath, forceMapId, gameStateFileName);
    }
}
