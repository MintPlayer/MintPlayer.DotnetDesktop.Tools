using MintPlayer.IconUtils;
using MintPlayer.PlatformBrowser;
using MintPlayer.PlatformBrowser.Enums;
using System.Drawing;

namespace MintPlayer.BrowserDialog.Presentation;

/// <summary>
/// Builds the browser list the dialog renders. Holds every decision the dialog used to
/// make inline inside its Load handler: which icon strategy applies to a browser, which
/// image is the largest, and which row is the system default.
/// </summary>
public sealed class BrowserDialogPresenter
{
    private static readonly string[] extractableIconExtensions = [".ico", ".cur", ".exe"];

    private readonly IPlatformBrowser platformBrowser;
    private readonly IIconExtractor iconExtractor;
    private readonly IImageLoader imageLoader;

    /// <summary>Creates a presenter backed by the real browser, icon and image sources.</summary>
    public BrowserDialogPresenter()
        : this(new PlatformBrowser.PlatformBrowser(), new IconExtractor(), new ImageLoader())
    {
    }

    /// <summary>Creates a presenter over the supplied seams.</summary>
    public BrowserDialogPresenter(IPlatformBrowser platformBrowser, IIconExtractor iconExtractor, IImageLoader imageLoader)
    {
        this.platformBrowser = platformBrowser;
        this.iconExtractor = iconExtractor;
        this.imageLoader = imageLoader;
    }

    /// <summary>
    /// Resolves the installed browsers and their icons.
    /// </summary>
    /// <remarks>
    /// Never throws. The dialog previously wrapped this whole flow in one try/catch, which
    /// meant a single browser with an unreadable icon abandoned the entire list halfway.
    /// Failures are contained per browser here, so one misconfigured install costs its own
    /// icon and nothing else.
    /// </remarks>
    public async Task<BrowserListModel> BuildModel()
    {
        IReadOnlyList<Browser> browsers;
        try
        {
            browsers = await platformBrowser.GetInstalledBrowsers();
        }
        catch (Exception)
        {
            return BrowserListModel.Empty;
        }

        var entries = new List<BrowserListEntry>(browsers.Count);
        foreach (var browser in browsers)
        {
            var (icon, image) = await ResolveArtwork(browser);
            entries.Add(new BrowserListEntry(
                browser.Name,
                browser.ExecutablePath.Trim('"'),
                icon,
                image,
                browser));
        }

        return new BrowserListModel(entries, await ResolveDefaultIndex(browsers, entries));
    }

    /// <summary>Picks the largest available artwork for a browser, or (null, null) when it has none.</summary>
    private async Task<(Icon? Icon, Image? Image)> ResolveArtwork(Browser browser)
    {
        if (string.IsNullOrEmpty(browser.IconPath))
        {
            return (null, null);
        }

        try
        {
            if (!extractableIconExtensions.Contains(Path.GetExtension(browser.IconPath)))
            {
                return (null, imageLoader.Load(browser.IconPath));
            }

            var split = await iconExtractor.Split(browser.IconPath);
            if (split.Count == 0)
            {
                return (null, null);
            }

            // A negative index is a misconfigured registry entry, not a reason to fail; an
            // index past the end is clamped for the same reason.
            var index = browser.IconIndex < 0 ? 0 : browser.IconIndex;
            if (index >= split.Count)
            {
                index = split.Count - 1;
            }

            var images = await iconExtractor.ExtractImagesFromIcon(split[index]);
            if (images.Count == 0)
            {
                return (null, null);
            }

            var largestSize = images.Max(i => i.Width);
            return (images.LastOrDefault(i => i.Width == largestSize), null);
        }
        catch (Exception)
        {
            // This browser gets no artwork; the rest of the list is unaffected.
            return (null, null);
        }
    }

    /// <summary>Finds which row is the default browser for HTTP, or -1.</summary>
    private async Task<int> ResolveDefaultIndex(IReadOnlyList<Browser> browsers, List<BrowserListEntry> entries)
    {
        Browser? defaultBrowser;
        try
        {
            defaultBrowser = await platformBrowser.GetDefaultBrowser(browsers.ToList(), EProtocolType.Http);
        }
        catch (Exception)
        {
            // No URL association registered, or the registry says something we cannot map.
            return -1;
        }

        if (defaultBrowser == null)
        {
            return -1;
        }

        // Matched on executable path rather than reference, because the default browser may
        // be a different instance describing the same install.
        return entries.FindIndex(e => e.Browser.ExecutablePath == defaultBrowser.ExecutablePath);
    }
}
