namespace MintPlayer.KarnaughMap;

/// <summary>
/// Everything the presenter needs from the control. Four members, none of them a drawing
/// primitive - the presenter never tells the view how to paint, it produces a
/// <see cref="KarnaughRenderModel"/> and asks for a repaint.
/// </summary>
public interface IKarnaughMapView
{
    /// <summary>Asks the view to repaint from the current render model.</summary>
    void Invalidate();

    /// <summary>
    /// Line height of the view's font. Layout depends on it - the axis gutters are one
    /// line high - and it is the one piece of view state the presenter cannot compute.
    /// </summary>
    int FontHeight { get; }

    /// <summary>Whether the view currently has keyboard focus, which decides whether a focus rectangle is drawn.</summary>
    bool HasFocus { get; }

    /// <summary>
    /// Reports a problem to the user. Exists so the presenter does not call MessageBox.Show
    /// directly - that single dependency is what previously made the solve paths impossible
    /// to test, because a failure blocked on a modal dialog.
    /// </summary>
    void ShowError(string message);
}
