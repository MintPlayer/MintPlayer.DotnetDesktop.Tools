using MintPlayer.BrowserDialog.Presentation;
using MintPlayer.IconUtils;
using MintPlayer.PlatformBrowser;
using MintPlayer.PlatformBrowser.Enums;
using System.Collections.ObjectModel;
using System.Drawing;

namespace MintPlayer.BrowserDialog.Tests;

/// <summary>A browser source returning whatever a test hands it.</summary>
public sealed class FakePlatformBrowser : IPlatformBrowser
{
    /// <summary>Browsers reported as installed.</summary>
    public List<Browser> Browsers { get; init; } = [];

    /// <summary>Browser reported as the default, if any.</summary>
    public Browser? DefaultBrowser { get; set; }

    /// <summary>Thrown from GetInstalledBrowsers when set.</summary>
    public Exception? ScanFailure { get; set; }

    /// <summary>Thrown from GetDefaultBrowser when set.</summary>
    public Exception? DefaultFailure { get; set; }

    public Task<ReadOnlyCollection<Browser>> GetInstalledBrowsers() =>
        ScanFailure != null
            ? throw ScanFailure
            : Task.FromResult(Browsers.AsReadOnly());

    public Task<Browser?> GetDefaultBrowser(IEnumerable<Browser> browsers, EProtocolType protocolType) =>
        DefaultFailure != null ? throw DefaultFailure : Task.FromResult(DefaultBrowser);

    public Task<Browser?> GetDefaultBrowser(EProtocolType protocolType) => GetDefaultBrowser([], protocolType);

    public Task<Browser?> GetDefaultBrowser() => GetDefaultBrowser(EProtocolType.Http);

    public Task<Browser?> GetDefaultBrowser(IEnumerable<Browser> browsers, EFileType fileType) =>
        Task.FromResult(DefaultBrowser);

    public Task<Browser?> GetDefaultBrowser(EFileType fileType) => GetDefaultBrowser([], fileType);
}

/// <summary>An icon extractor returning icons of configurable sizes.</summary>
public sealed class FakeIconExtractor : IIconExtractor
{
    /// <summary>Icons returned from Split, one per "index" the registry might name.</summary>
    public List<Icon> SplitResult { get; init; } = [];

    /// <summary>Widths of the images inside the chosen icon, largest wins.</summary>
    public int[] ImageWidths { get; init; } = [16, 48, 32];

    /// <summary>Thrown from Split when set.</summary>
    public Exception? Failure { get; set; }

    /// <summary>Paths Split was called with.</summary>
    public List<string> SplitCalls { get; } = [];

    public Task<List<Icon>> Split(string filename)
    {
        SplitCalls.Add(filename);
        return Failure != null ? throw Failure : Task.FromResult(SplitResult);
    }

    public Task<List<Icon>> ExtractImagesFromIcon(Icon icon) =>
        Task.FromResult(ImageWidths.Select(MakeIcon).ToList());

    /// <summary>
    /// Builds a square icon of an exact width, so "largest" is well defined.
    /// </summary>
    /// <remarks>
    /// Goes through a Bitmap rather than <c>new Icon(SystemIcons.Application, w, h)</c>:
    /// that constructor snaps to the nearest size the source icon actually contains, so
    /// asking for 64 quietly yields 40 and the test stops testing what it says.
    /// </remarks>
    public static Icon MakeIcon(int width)
    {
        using var bitmap = new Bitmap(width, width);
        return Icon.FromHandle(bitmap.GetHicon());
    }
}

/// <summary>An image loader returning a fixed bitmap, or nothing.</summary>
public sealed class FakeImageLoader : IImageLoader
{
    /// <summary>Image returned for any path, or null to simulate an unreadable file.</summary>
    public Image? Result { get; set; } = new Bitmap(8, 8);

    /// <summary>Paths Load was called with.</summary>
    public List<string> LoadCalls { get; } = [];

    public Image? Load(string path)
    {
        LoadCalls.Add(path);
        return Result;
    }
}
