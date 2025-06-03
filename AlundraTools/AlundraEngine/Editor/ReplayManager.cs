namespace AlundraEngine.Editor;

public class ReplayManager
{
    public List<FrameSnapshot> Frames { get; } = new(10000);

    public bool IsSaving { get; set; }

    public bool ApplyCurrentFrame { get; set; }
    public int CurrentFrame { get; set; }
    public int FrameCount => Frames.Count;

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

        Frames.Add(new FrameSnapshot());
    }

    public void PlayOneFrame()
    {
        Frames[CurrentFrame].CopyToMemory();
        ApplyCurrentFrame = false;
    }
}