namespace MintPlayer.PlatformBrowser;

/// <summary>
/// The file-system seam. Browser detection probes C:\Windows\SystemApps for legacy Edge
/// and reads product versions off executables; both are machine state that a test cannot
/// arrange.
/// </summary>
public interface IBrowserFileSystem
{
    /// <summary>Whether a directory exists.</summary>
    bool DirectoryExists(string path);

    /// <summary>Immediate subdirectories of <paramref name="path"/>.</summary>
    string[] GetDirectories(string path);

    /// <summary>Whether a file exists.</summary>
    bool FileExists(string path);

    /// <summary>
    /// Product version of an executable, or null when it cannot be read. Returns null
    /// rather than throwing: a browser whose version is unreadable is still a browser,
    /// and the caller records the version as unknown.
    /// </summary>
    string? GetProductVersion(string executablePath);
}
