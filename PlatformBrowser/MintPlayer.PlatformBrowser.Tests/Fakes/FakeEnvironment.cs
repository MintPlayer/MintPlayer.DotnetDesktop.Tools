namespace MintPlayer.PlatformBrowser.Tests.Fakes;

/// <summary>An in-memory file system. Nothing exists unless a test says it does.</summary>
public sealed class FakeBrowserFileSystem : IBrowserFileSystem
{
    /// <summary>Directories that exist.</summary>
    public HashSet<string> Directories { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Files that exist.</summary>
    public HashSet<string> Files { get; } = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>Product version reported per executable path.</summary>
    public Dictionary<string, string?> ProductVersions { get; } = new(StringComparer.OrdinalIgnoreCase);

    public bool DirectoryExists(string path) => Directories.Contains(path);

    public string[] GetDirectories(string path) =>
        [.. Directories.Where(d => d.StartsWith(path, StringComparison.OrdinalIgnoreCase) && !string.Equals(d, path, StringComparison.OrdinalIgnoreCase))];

    public bool FileExists(string path) => Files.Contains(path);

    public string? GetProductVersion(string executablePath) =>
        ProductVersions.TryGetValue(executablePath, out var version) ? version : null;
}

/// <summary>A packaged-browser catalog returning whatever a test hands it.</summary>
public sealed class FakePackagedBrowserCatalog(params Browser[] browsers) : IPackagedBrowserCatalog
{
    /// <summary>Extensions the production code asked about, captured for assertions.</summary>
    public List<string> RequestedExtensions { get; } = [];

    public IEnumerable<Browser> GetPackagedBrowsers(IEnumerable<string> supportedExtensions)
    {
        RequestedExtensions.AddRange(supportedExtensions);
        return browsers;
    }
}
