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
