namespace MintPlayer.PlatformBrowser;

/// <summary>
/// Packaged-browser catalog backed by <c>Windows.Management.Deployment.PackageManager</c>.
/// </summary>
/// <remarks>
/// Compiled only on the net*-windows10.0.22621 legs, where the package manager exists.
/// DefaultPackagedBrowserCatalog.cs supplies an empty implementation of the same type
/// elsewhere.
/// </remarks>
public sealed class DefaultPackagedBrowserCatalog : IPackagedBrowserCatalog
{
    /// <inheritdoc />
    public IEnumerable<Browser> GetPackagedBrowsers(IEnumerable<string> supportedExtensions)
    {
        var extensions = supportedExtensions as string[] ?? supportedExtensions.ToArray();

        var packageManager = new Windows.Management.Deployment.PackageManager();
        var entries = packageManager.FindPackagesForUser(string.Empty)
            .SelectMany(package => package.GetAppListEntries())
            .Where(entry => entry.AppInfo.SupportedFileExtensions != null)
            .Where(entry => entry.AppInfo.SupportedFileExtensions.Intersect(extensions).Any());

        return entries.Select(entry => new Browser
        {
            Name = entry.DisplayInfo.DisplayName,
            IconPath = entry.AppInfo.Package.Logo.LocalPath,
            IconIndex = 0,
            Version = entry.AppInfo.Package.Id.Version.ToFormattedString(),
            KeyName = null,
            Source = EBrowserSource.PackageManager,
            FileAssociations = entry.AppInfo.SupportedFileExtensions.ToDictionary(x => x, x => (object)entry.AppUserModelId).AsReadOnly(),
            ExecutablePath = entry.AppInfo.Package.InstalledPath,
            UrlAssociations = new[] { "http", "https" }.ToDictionary(x => x, x => (object)entry.AppUserModelId).AsReadOnly(),
        }).ToList();
    }
}
