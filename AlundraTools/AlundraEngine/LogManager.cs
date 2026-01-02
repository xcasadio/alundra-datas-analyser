using System.Diagnostics;
using AlundraEngine.Gameplay;

namespace AlundraEngine;

public class LogManager
{
    private const string DefaultCategory = "default";
    private string CurrentCategory = DefaultCategory;

    public bool TraceEnabled { get; set; } = false;

    private readonly GameEngine _gameEngine;

    public List<string> Logs { get; } = new();

    public Dictionary<string, List<string>> LogByCategories { get; } = new();

    public LogManager(GameEngine gameEngine)
    {
        _gameEngine = gameEngine;
    }

    public void SetCategory(string categoryName)
    {
        CurrentCategory = categoryName;
    }

    public void ResetCategory()
    {
        CurrentCategory = DefaultCategory;
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
        var logWithCategory = $"{logPrefix} [{CurrentCategory}] {message}";

        if (TraceEnabled)
        {
            Debug.WriteLine(logWithCategory);
        }

        Logs.Add(logWithCategory);

        if (!LogByCategories.ContainsKey(CurrentCategory))
        {
            LogByCategories[CurrentCategory] = new List<string>();
        }

        LogByCategories[CurrentCategory].Add($"{logPrefix} {message}");
    }

    public void Clear()
    {
        LogByCategories.Clear();
        Logs.Clear();
    }
}