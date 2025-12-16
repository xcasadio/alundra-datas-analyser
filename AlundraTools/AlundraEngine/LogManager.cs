using System.Diagnostics;
using AlundraEngine.Gameplay;

namespace AlundraEngine;

public class LogManager
{
    private readonly GameEngine _gameEngine;

    public List<string> Logs { get; } = new();

    public LogManager(GameEngine gameEngine)
    {
        _gameEngine = gameEngine;
    }

    public void Log(string message)
    {
        LogImpl(message);
    }
    
    public void Log(Entity entity, string message)
    {
        LogImpl($"entity[{entity}] {message}");
    }

    public void Log(SpriteEffect effect, string message)
    {
        LogImpl($"effect[{effect}] {message}");
    }

    private void LogImpl(string message)
    {
        var log = $"Map#{_gameEngine.StaticVariables.g_currentMap} frame#{_gameEngine.StaticVariables.FrameNumber} {message}";
        Logs.Add(log);
        Debug.WriteLine(log);
    }
}