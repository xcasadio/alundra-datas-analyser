using System.Diagnostics;
using AlundraEngine.Gameplay;

namespace AlundraEngine;

public class LogManager
{
    private const string DefaultCategory = "default";
    private const string LogFilePath = "log.txt";
    private string _currentCategory = DefaultCategory;
    private readonly StreamWriter _logFileWriter;

    public bool TraceEnabled { get; set; } = false;

    private readonly GameEngine _gameEngine;

    public List<string> Logs { get; } = new();

    public LogManager(GameEngine gameEngine)
    {
        _gameEngine = gameEngine;
        _logFileWriter = new StreamWriter(LogFilePath, append: false) { AutoFlush = true };
    }

    public void SetCategory(string categoryName)
    {
        _currentCategory = categoryName;
    }

    public void ResetCategory()
    {
        _currentCategory = DefaultCategory;
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
        var logPrefix = $"map#{_gameEngine.StaticVariables.g_currentMap:d3} frame#{_gameEngine.StaticVariables.FrameNumber:d6}";
        var logWithCategory = $"{logPrefix} [{_currentCategory}] {message}";

        if (TraceEnabled)
        {
            Debug.WriteLine(logWithCategory);
        }

        Logs.Add(logWithCategory);
        _logFileWriter.WriteLine(logWithCategory);
    }

    public void Clear()
    {
        Logs.Clear();
    }
}