using System.Text.RegularExpressions;

namespace AlundraEngine.Editor;

public class ReplayManager
{
    public List<FrameSnapshot> Frames { get; } = new(10000);

    public bool IsSaving { get; set; }

    public bool ApplyCurrentFrame { get; set; }
    public int CurrentFrame { get; set; }
    public int FrameCount => Frames.Count;

    private readonly Regex FrameRegex = new(@"^alundra_frame_(\d{6})\.json$", RegexOptions.IgnoreCase);

    public void StartSaving()
    {
        ApplyCurrentFrame = false;
        CurrentFrame = 0;

        IsSaving = true;
        Frames.Clear();
    }

    public void StopSaving()
    {
        IsSaving = false;
    }

    public void SaveFrame()
    {
        if (!IsSaving)
        {
            return;
        }

        var frameSnapshot = new FrameSnapshot();
        Frames.Add(frameSnapshot);
        frameSnapshot.CopyFromMemory();
    }

    public void PlayOneFrame()
    {
        Frames[CurrentFrame].CopyToMemory();
        ApplyCurrentFrame = false;
    }

    public void LoadFromDump(string directoryPath)
    {
        var files = GetSortedFrameFiles(directoryPath);

        Frames.Clear();

        foreach (var file in files)
        {
            Frames.Add(FrameSnapshotLoader.LoadFromJson(file));
        }
    }

    private List<string> GetSortedFrameFiles(string directoryPath)
    {
        return Directory
            .EnumerateFiles(directoryPath, "alundra_frame_*.json")
            .Select(path => new FileInfo(path))
            .Where(fi => FrameRegex.IsMatch(fi.Name))
            .Select(fi => new
            {
                FilePath = fi.FullName,
                FrameNumber = int.Parse(FrameRegex.Match(fi.Name).Groups[1].Value)
            })
            .OrderBy(x => x.FrameNumber)
            .Select(x => x.FilePath)
            .ToList();
    }
}