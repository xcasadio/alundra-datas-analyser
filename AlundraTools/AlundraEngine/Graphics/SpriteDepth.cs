namespace AlundraEngine.Graphics;

public class SpriteDepth
{
    public const int DebugCollision = FadeTransitionEffect - 1;
    public const int FadeTransitionEffect = BackgroundUI - 1;
    public const int BackgroundUI = ForegroundUI - 1;
    public const int ForegroundUI = ForegroundUICursor - 1;
    public const int ForegroundUICursor = ForegroundUICursor2 - 1;
    public const int ForegroundUICursor2 = ForegroundEffect - 1;
    public const int ForegroundEffect = int.MaxValue;
}