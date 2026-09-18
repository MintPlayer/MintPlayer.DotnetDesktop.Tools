namespace MintPlayer.PlatformBrowser;

/// <summary>
/// Packaged-browser catalog for target frameworks without a Windows package manager.
/// </summary>
/// <remarks>
/// This file is compiled only on the plain (non platform-versioned) legs;
/// DefaultPackagedBrowserCatalog.Windows.cs supplies the same type on the
/// net*-windows10.0.22621 legs. Selecting the implementation in the project file rather
/// than with #if keeps the conditional compilation out of the logic, so nothing in
/// PlatformBrowser has to know which platform it was built for.
/// </remarks>
public sealed class DefaultPackagedBrowserCatalog : IPackagedBrowserCatalog
{
    /// <inheritdoc />
    public IEnumerable<Browser> GetPackagedBrowsers(IEnumerable<string> supportedExtensions) => [];
}
