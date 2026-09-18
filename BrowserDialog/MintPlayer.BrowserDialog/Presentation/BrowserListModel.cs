using System.Drawing;

namespace MintPlayer.BrowserDialog.Presentation;

/// <summary>One row of the browser list, already resolved to what the view needs to draw.</summary>
/// <param name="Name">Display name.</param>
/// <param name="ExecutablePath">Executable path, quotes already trimmed.</param>
/// <param name="Icon">
/// The largest icon extracted from an .ico/.cur/.exe, or null. The view owns the lifetime
/// once it adds this to an ImageList.
/// </param>
/// <param name="Image">The image loaded from a non-icon file, or null.</param>
/// <param name="Browser">The browser this row was built from.</param>
/// <remarks>
/// Icon and Image are kept as separate properties rather than collapsed into one Image
/// because ImageList.Images.Add is overloaded for both, and converting an Icon with
/// ToBitmap would change how it is rendered. At most one of the two is ever set.
/// </remarks>
public sealed record BrowserListEntry(
    string Name,
    string ExecutablePath,
    Icon? Icon,
    Image? Image,
    PlatformBrowser.Browser Browser)
{
    /// <summary>Whether this row has any artwork to show.</summary>
    public bool HasArtwork => Icon != null || Image != null;
}

/// <summary>
/// Everything the dialog needs in order to render itself: the rows, and which one is the
/// system default.
/// </summary>
/// <param name="Entries">Rows, in display order.</param>
/// <param name="DefaultIndex">
/// Index into <paramref name="Entries"/> of the default browser, or -1 when there is none
/// or it could not be determined.
/// </param>
public sealed record BrowserListModel(
    IReadOnlyList<BrowserListEntry> Entries,
    int DefaultIndex)
{
    /// <summary>An empty list, used when the scan fails outright.</summary>
    public static BrowserListModel Empty { get; } = new([], -1);
}
