namespace MintPlayer.Bacon.IconEditor;

/// <summary>
/// A Panel with double buffering enabled to reduce flickering.
/// </summary>
internal class DoubleBufferedPanel : Panel
{
    public DoubleBufferedPanel()
    {
        DoubleBuffered = true;
        SetStyle(ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.UserPaint |
                 ControlStyles.OptimizedDoubleBuffer, true);
    }
}
