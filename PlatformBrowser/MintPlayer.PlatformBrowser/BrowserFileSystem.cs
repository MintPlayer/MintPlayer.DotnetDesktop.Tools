using System.Diagnostics;

namespace MintPlayer.PlatformBrowser;

/// <summary>The real file system, used by default. A thin adapter - it holds no logic.</summary>
public sealed class BrowserFileSystem : IBrowserFileSystem
{
    /// <inheritdoc />
    public bool DirectoryExists(string path) => Directory.Exists(path);

    /// <inheritdoc />
    public string[] GetDirectories(string path) => Directory.GetDirectories(path);

    /// <inheritdoc />
    public bool FileExists(string path) => File.Exists(path);

    /// <inheritdoc />
    public string? GetProductVersion(string executablePath)
    {
        // Swallowing here rather than at the call site: an executable that is present but
        // whose version resource cannot be read (locked, truncated, not a PE file) is still
        // a usable browser, and the contract says an unreadable version is null.
        try
        {
            return FileVersionInfo.GetVersionInfo(executablePath).ProductVersion;
        }
        catch (FileNotFoundException)
        {
            return null;
        }
        catch (IOException)
        {
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }
    }
}
