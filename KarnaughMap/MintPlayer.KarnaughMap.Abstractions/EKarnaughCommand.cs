namespace MintPlayer.KarnaughMap;

/// <summary>
/// A keyboard action the map understands, decoupled from System.Windows.Forms.Keys.
/// </summary>
/// <remarks>
/// The control maps its key codes onto these, which is what lets the presenter's input
/// handling be exercised without a message loop. It also collapses the duplicate key
/// bindings - D0 and NumPad0 are one command here, not two cases.
/// </remarks>
public enum EKarnaughCommand
{
    /// <summary>Not a key the map handles.</summary>
    None,
    /// <summary>Move focus one cell left, wrapping.</summary>
    MoveLeft,
    /// <summary>Move focus one cell right, wrapping.</summary>
    MoveRight,
    /// <summary>Move focus one cell up, wrapping.</summary>
    MoveUp,
    /// <summary>Move focus one cell down, wrapping.</summary>
    MoveDown,
    /// <summary>Advance the focused cell to its next value, or toggle its selection.</summary>
    Toggle,
    /// <summary>Set the focused cell to zero.</summary>
    SetZero,
    /// <summary>Set the focused cell to one.</summary>
    SetOne,
    /// <summary>Set the focused cell to a don't-care.</summary>
    SetDontCare,
    /// <summary>Clear the focused cell.</summary>
    SetUndefined,
}
