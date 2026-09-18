namespace MintPlayer.KarnaughMap.Tests;

/// <summary>
/// Stands in for the control. Four members, no message loop - which is what the MVP split
/// bought: none of these tests construct a UserControl.
/// </summary>
public sealed class FakeKarnaughMapView : IKarnaughMapView
{
    /// <summary>How many repaints were requested.</summary>
    public int InvalidateCount { get; private set; }

    /// <summary>Messages the presenter tried to show the user.</summary>
    public List<string> Errors { get; } = [];

    /// <inheritdoc />
    public int FontHeight { get; set; } = 15;

    /// <inheritdoc />
    public bool HasFocus { get; set; } = true;

    /// <inheritdoc />
    public void Invalidate() => InvalidateCount++;

    /// <inheritdoc />
    public void ShowError(string message) => Errors.Add(message);
}
