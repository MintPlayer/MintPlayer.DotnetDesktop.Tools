namespace MintPlayer.PlatformBrowser;

/// <summary>
/// The UWP seam. Packaged (Store) browsers come from
/// <c>Windows.Management.Deployment.PackageManager</c>, which only exists on the
/// platform-versioned Windows target frameworks. Behind this interface the caller does
/// not need to know that, and the plain net10.0/net11.0 legs get an implementation that
/// returns nothing instead of a block of conditionally-compiled code.
/// </summary>
public interface IPackagedBrowserCatalog
{
    /// <summary>
    /// Packaged apps that declare support for any of <paramref name="supportedExtensions"/>,
    /// already mapped onto <see cref="Browser"/>. Returns an empty sequence where the
    /// platform has no package manager.
    /// </summary>
    IEnumerable<Browser> GetPackagedBrowsers(IEnumerable<string> supportedExtensions);
}
