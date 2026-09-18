using MintPlayer.PlatformBrowser.Enums;
using MintPlayer.PlatformBrowser.Exceptions;
using MintPlayer.PlatformBrowser.Registry;
using System.Collections.ObjectModel;
using System.Globalization;

namespace MintPlayer.PlatformBrowser;

/// <inheritdoc cref="IPlatformBrowser" />
/// <remarks>
/// Was a static class. It is an instance class now so the registry, the file system and
/// the UWP package manager can be substituted - almost all of this type is parsing, and
/// only its edges touch machine state.
/// </remarks>
public class PlatformBrowser : IPlatformBrowser
{
    private readonly IRegistry registry;
    private readonly IBrowserFileSystem fileSystem;
    private readonly IPackagedBrowserCatalog packagedBrowsers;

    /// <summary>Creates a browser resolver backed by the real registry, file system and package manager.</summary>
    public PlatformBrowser()
        : this(new WindowsRegistry(), new BrowserFileSystem(), new DefaultPackagedBrowserCatalog())
    {
    }

    /// <summary>Creates a browser resolver over the supplied seams.</summary>
    public PlatformBrowser(IRegistry registry, IBrowserFileSystem fileSystem, IPackagedBrowserCatalog packagedBrowsers)
    {
        this.registry = registry;
        this.fileSystem = fileSystem;
        this.packagedBrowsers = packagedBrowsers;
    }

    /// <summary>Extensions that mark a packaged app as a browser.</summary>
    private static readonly string[] packagedBrowserExtensions = [".htm", ".html", ".pdf"];

    /// <inheritdoc />
    public async Task<ReadOnlyCollection<Browser>> GetInstalledBrowsers()
    {
        var browsers = await Task.Run(() =>
        {
            var result = new List<Browser>();

            ReadRegistryBrowsers(result);
            ReadLegacyEdge(result);
            result.AddRange(packagedBrowsers.GetPackagedBrowsers(packagedBrowserExtensions));

            return new ReadOnlyCollection<Browser>(result);
        });

        return browsers;
    }

    private void ReadRegistryBrowsers(List<Browser> result)
    {
        var machineInternetKey = registry.OpenLocalMachine(@"SOFTWARE\WOW6432Node\Clients\StartMenuInternet")
            ?? registry.OpenLocalMachine(@"SOFTWARE\Clients\StartMenuInternet");
        var userInternetKey = registry.OpenCurrentUser(@"SOFTWARE\WOW6432Node\Clients\StartMenuInternet")
            ?? registry.OpenCurrentUser(@"SOFTWARE\Clients\StartMenuInternet");

        foreach (var internetKey in new[] { userInternetKey, machineInternetKey }.Where(key => key != null).Cast<IRegistryKey>())
        {
            foreach (var browserKeyName in internetKey.GetSubKeyNames())
            {
                try
                {
                    if (result.Any(b => b.KeyName == browserKeyName))
                    {
                        continue;
                    }

                    result.Add(ReadBrowser(internetKey, browserKeyName));
                }
                catch (Exception)
                {
                    // Disconfigured browser: skip it rather than failing the whole scan.
                }
            }
        }
    }

    private Browser ReadBrowser(IRegistryKey internetKey, string browserKeyName)
    {
        // Key containing browser information
        var browserKey = internetKey.OpenSubKey(browserKeyName)
            ?? throw new BrowserException("Unexpected exception, browserKey == null");

        // Key containing executable path
        var commandKey = browserKey.OpenSubKey(@"shell\open\command")
            ?? throw new BrowserException(@"Browser key shell\open\command should exist");

        // Key containing icon path
        var iconKey = browserKey.OpenSubKey(@"DefaultIcon");
        var iconPath = (string?)iconKey?.GetValue(null);
        var iconParts = iconPath?.Split(',');

        var executablePath = ((string?)commandKey.GetValue(null)!).Trim('"');

        // ExecutablePath must be .exe
        if (Path.GetExtension(executablePath) != ".exe")
        {
            throw new BrowserException("ExecutablePath must be .exe");
        }

        // IconPath must be .exe or .ico
        var iconValid = new[] { ".exe", ".ico" }.Contains(Path.GetExtension(iconParts![0]));

        // Disconfigured browser installation, add more browsers to this list
        var browserIdFormat = browserKeyName.Equals("IEXPLORE.EXE") ? "IE.AssocFile.{0}" : string.Empty;

        var fileAssociations = ReadAssociations(browserKey, @"Capabilities\FileAssociations")
            ?? new ReadOnlyDictionary<string, object>(CreateDefaultFileAssociations(browserIdFormat));

        var urlAssociations = ReadAssociations(browserKey, @"Capabilities\URLAssociations")
            ?? new ReadOnlyDictionary<string, object>(new Dictionary<string, object>());

        return new Browser
        {
            KeyName = browserKeyName,
            Name = (string?)browserKey.GetValue(null) ?? string.Empty,
            Source = EBrowserSource.Registry,
            ExecutablePath = executablePath,
            Version = fileSystem.GetProductVersion(executablePath),
            IconPath = iconValid ? iconParts[0] : executablePath,
            IconIndex = !iconValid
                ? 0
                : iconParts.Length > 1
                    ? Convert.ToInt32(iconParts[1])
                    : 0,
            FileAssociations = fileAssociations,
            UrlAssociations = urlAssociations
        };
    }

    private static ReadOnlyDictionary<string, object>? ReadAssociations(IRegistryKey browserKey, string path)
    {
        var key = browserKey.OpenSubKey(path);
        if (key == null)
        {
            return null;
        }

        return new ReadOnlyDictionary<string, object>(
            key.GetValueNames().ToDictionary(v => v, v => key.GetValue(v)!));
    }

    private void ReadLegacyEdge(List<Browser> result)
    {
        const string systemAppsFolder = @"C:\Windows\SystemApps\";
        if (!fileSystem.DirectoryExists(systemAppsFolder))
        {
            return;
        }

        var edgeFolder = fileSystem.GetDirectories(systemAppsFolder)
            .FirstOrDefault(d => d.StartsWith($"{systemAppsFolder}Microsoft.MicrosoftEdge_"));
        if (edgeFolder == null)
        {
            return;
        }

        var edgePath = $@"{edgeFolder}\MicrosoftEdge.exe";
        if (!fileSystem.FileExists(edgePath))
        {
            return;
        }

        result.Add(new Browser
        {
            KeyName = "Microsoft Edge",
            Name = "Microsoft Edge",
            ExecutablePath = edgePath,
            Version = fileSystem.GetProductVersion(edgePath),
            IconPath = edgePath,
            IconIndex = 0,
            // http://mikenation.net/files/win-10-reg.txt
            FileAssociations = CreateEdgeFileAssociations().AsReadOnly(),
            UrlAssociations = new ReadOnlyDictionary<string, object>(new Dictionary<string, object>()),
            Source = EBrowserSource.HardCoded,
        });
    }

    private static Dictionary<string, object> CreateEdgeFileAssociations() => new()
    {
        { ".htm", "AppX4hxtad77fbk3jkkeerkrm0ze94wjf3s9" },
        { ".html", "AppX4hxtad77fbk3jkkeerkrm0ze94wjf3s9" },
        { ".pdf", "AppXd4nrz8ff68srnhf9t5a8sbjyar1cr723" },
        { ".xml", "AppXcc58vyzkbjbs4ky0mxrmxf8278rk9b3t" },
        { ".svg", "AppXde74bfzw9j31bzhcvsrxsyjnhhbq66cs" },
    };

    private static Dictionary<string, object> CreateDefaultFileAssociations(string browserIdFormat) => new()
    {
        { ".htm", string.Format(browserIdFormat, "HTM") },
        { ".html", string.Format(browserIdFormat, "HTML") },
        { ".pdf", string.Format(browserIdFormat, "PDF") },
        { ".shtml", string.Format(browserIdFormat, "SHTML") },
        { ".svg", string.Format(browserIdFormat, "SVG") },
        { ".webp", string.Format(browserIdFormat, "WEBP") },
        { ".xht", string.Format(browserIdFormat, "XHT") },
        { ".xhtml", string.Format(browserIdFormat, "XHTML") }
    };

    /// <inheritdoc />
    public Task<Browser?> GetDefaultBrowser(IEnumerable<Browser> browsers, EProtocolType protocolType)
    {
        var urlAssociationsKey = registry.OpenCurrentUser(@"SOFTWARE\Microsoft\Windows\Shell\Associations\URLAssociations");
        var protocolName = Enum.GetName(typeof(EProtocolType), protocolType)?.ToLower(CultureInfo.InvariantCulture);
        if (urlAssociationsKey != null && !urlAssociationsKey.GetSubKeyNames().Contains(protocolName))
        {
            throw new BrowserException($"No url association for {protocolName}");
        }

        var userChoiceKey = urlAssociationsKey?.OpenSubKey($@"{protocolName}\UserChoice");
        var defaultBrowserProgId = userChoiceKey?.GetValue("ProgId");

        var foundNormalBrowser = browsers.FirstOrDefault(
            b => b.UrlAssociations.Any(a => a.Key == protocolName && a.Value.Equals(defaultBrowserProgId)));

        if (foundNormalBrowser != null)
        {
            return Task.FromResult<Browser?>(foundNormalBrowser);
        }

        // Was an unguarded dereference: a ProgId with no Shell\open key threw
        // NullReferenceException out of a method whose return type is already nullable.
        // "No AppUserModelID to match on" is an ordinary not-found, so it reads as one.
        var classesRootProgKey = registry.OpenClassesRoot($@"{defaultBrowserProgId}\Shell\open");
        var appUserModelId = classesRootProgKey?.GetValue("AppUserModelID");

        if (appUserModelId != null)
        {
            foundNormalBrowser = browsers.FirstOrDefault(
                b => b.UrlAssociations.Any(a => a.Key == protocolName && a.Value.Equals(appUserModelId)));

            if (foundNormalBrowser != null)
            {
                return Task.FromResult<Browser?>(foundNormalBrowser);
            }
        }

        return defaultBrowserProgId switch
        {
            // Old Edge
            "AppX4hxtad77fbk3jkkeerkrm0ze94wjf3s9" or // htm, html
            "AppXd4nrz8ff68srnhf9t5a8sbjyar1cr723" or // pdf
            "AppXde74bfzw9j31bzhcvsrxsyjnhhbq66cs" or // svg
            "AppXcc58vyzkbjbs4ky0mxrmxf8278rk9b3t" or // xml
            "AppXq0fevzme2pys62n3e0fbqa7peapykr8v" =>
                Task.FromResult(browsers.FirstOrDefault(
                    b => b.KeyName == "Microsoft Edge" && b.Source == EBrowserSource.HardCoded)),
            // Internet Explorer
            "IE.HTTP" =>
                Task.FromResult(browsers.FirstOrDefault(b => b.KeyName == "IEXPLORE.EXE")),
            _ => Task.FromResult<Browser?>(null),
        };
    }

    /// <inheritdoc />
    public async Task<Browser?> GetDefaultBrowser(EProtocolType protocolType)
    {
        var browsers = await GetInstalledBrowsers();
        return await GetDefaultBrowser(browsers, protocolType);
    }

    /// <inheritdoc />
    public async Task<Browser?> GetDefaultBrowser() => await GetDefaultBrowser(EProtocolType.Http);

    /// <inheritdoc />
    public async Task<Browser?> GetDefaultBrowser(IEnumerable<Browser> browsers, EFileType fileType)
    {
        return await Task.Run(() =>
        {
            var ext = Enum.GetName(typeof(EFileType), fileType)?.ToLower(CultureInfo.InvariantCulture);
            var fileExtsKey = registry.OpenCurrentUser(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\FileExts");

            if (fileExtsKey == null || !fileExtsKey.GetSubKeyNames().Contains($".{ext}"))
            {
                throw new BrowserException("The specified filetype is not present in the registry");
            }

            var fileTypeKey = fileExtsKey.OpenSubKey($@".{ext}\UserChoice")
                ?? throw new BrowserException("The specified filetype is not present in the registry");

            var progId = fileTypeKey.GetValue("ProgId");

            return browsers.FirstOrDefault(
                // Don't compare key AND value. Just check if the value exists in the list.
                b => b.FileAssociations.Any(v => v.Value.Equals(progId)));
        });
    }

    /// <inheritdoc />
    public async Task<Browser?> GetDefaultBrowser(EFileType fileType)
    {
        var browsers = await GetInstalledBrowsers();
        return await GetDefaultBrowser(browsers, fileType);
    }
}
