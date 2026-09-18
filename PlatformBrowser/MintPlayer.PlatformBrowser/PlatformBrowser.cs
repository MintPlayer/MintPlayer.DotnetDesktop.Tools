using Microsoft.Win32;
using MintPlayer.PlatformBrowser.Exceptions;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace MintPlayer.PlatformBrowser;

public static class PlatformBrowser
{
    /// <summary>Retrieves a list of installed browsers from the registry.</summary>
    public static async Task<ReadOnlyCollection<Browser>> GetInstalledBrowsers()
    {
        var browsers = await Task.Run(() =>
        {
            #region Get registry keys containing browser information

            var machineInternetKey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\WOW6432Node\Clients\StartMenuInternet") ??
                              Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Clients\StartMenuInternet");
            var userInternetKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\WOW6432Node\Clients\StartMenuInternet") ??
                              Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Clients\StartMenuInternet");
            #endregion

            #region Loop through keys

            var result = new List<Browser>();
            foreach (var internetKey in new[] { userInternetKey, machineInternetKey }.Where(key => key != null).Cast<RegistryKey>())
            {
                foreach (var browserKeyName in internetKey.GetSubKeyNames())
                {
                    try
                    {
                        if (!result.Any(b => b.KeyName == browserKeyName))
                        {
                            // Key containing browser information
                            var browserKey = internetKey.OpenSubKey(browserKeyName);
                            if (browserKey == null)
                            {
                                throw new BrowserException("Unexpected exception, browserKey == null");
                            }

                            // Key containing executable path
                            var commandKey = browserKey!.OpenSubKey(@"shell\open\command");
                            if (commandKey == null)
                            {
                                throw new BrowserException(@"Browser key shell\open\command should exist");
                            }

                            // Key containing icon path
                            var iconKey = browserKey.OpenSubKey(@"DefaultIcon");
                            var iconPath = (string?)iconKey?.GetValue(null);
                            var iconParts = iconPath?.Split(',');

                            // Validate the values
                            string executablePath = ((string?)commandKey.GetValue(null)!).Trim('"');

                            // ExecutablePath must be .exe
                            if (Path.GetExtension(executablePath) != ".exe")
                            {
                                throw new BrowserException("ExecutablePath must be .exe");
                            }

                            // IconPath must be .exe or .ico
                            bool iconValid = new[] { ".exe", ".ico" }.Contains(Path.GetExtension(iconParts![0]));


                            #region Disconfigured browser installation, Add more browsers to this list

                            string browserIdFormat = string.Empty;
                            if (browserKeyName.Equals("IEXPLORE.EXE"))
                            {
                                browserIdFormat = "IE.AssocFile.{0}";
                            }

                            #endregion

                            // Add a default list of file associations
                            var associations = CreateDefaultFileAssociations(browserIdFormat);

                            ReadOnlyDictionary<string, object> fileAssociations = new ReadOnlyDictionary<string, object>(associations);
                            // Read the FileAssociations
                            var fileAssociationsKey = browserKey.OpenSubKey(@"Capabilities\FileAssociations");
                            if (fileAssociationsKey != null)
                            {
                                fileAssociations = new ReadOnlyDictionary<string, object>(fileAssociationsKey
                                    .GetValueNames().ToDictionary(v => v, v => fileAssociationsKey.GetValue(v)!));
                            }
                            // Use empty list for UrlAssociations
                            var urlAssociations =
                                new ReadOnlyDictionary<string, object>(new Dictionary<string, object>());
                            // Read the UrlAssociations
                            var urlAssociationsKey = browserKey.OpenSubKey(@"Capabilities\URLAssociations");
                            if (urlAssociationsKey != null)
                            {
                                urlAssociations = new ReadOnlyDictionary<string, object>(urlAssociationsKey
                                    .GetValueNames()
                                    .ToDictionary(v => v, v => urlAssociationsKey.GetValue(v)!));
                            }

                            var browser = new Browser
                            {
                                KeyName = browserKeyName,
                                Name = (string?)browserKey.GetValue(null) ?? string.Empty,
                                Source = EBrowserSource.Registry,
                                ExecutablePath = executablePath,
                                Version = FileVersionInfo.GetVersionInfo(executablePath)?.ProductVersion,
                                IconPath = iconValid ? iconParts[0] : executablePath,
                                IconIndex = !iconValid
                                    ? 0
                                    : iconParts.Length > 1
                                        ? Convert.ToInt32(iconParts[1])
                                        : 0,
                                FileAssociations = fileAssociations,
                                UrlAssociations = urlAssociations
                            };
                            result.Add(browser);
                        }
                    }
                    catch (Exception)
                    {
                        // Disconfigured browser
                    }
                }
            }
            #endregion

            #region Check if Edge is installed

            var systemAppsFolder = @"C:\Windows\SystemApps\";
            if (Directory.Exists(systemAppsFolder))
            {
                string[] directories = Directory.GetDirectories(systemAppsFolder);
                var edgeFolder = directories.FirstOrDefault(d => d.StartsWith($"{systemAppsFolder}Microsoft.MicrosoftEdge_"));

                if (edgeFolder != null)
                {
                    if (File.Exists($@"{edgeFolder}\MicrosoftEdge.exe"))
                    {
                        var edgePath = $@"{edgeFolder}\MicrosoftEdge.exe";
                        result.Add(new Browser
                        {
                            KeyName = "Microsoft Edge",
                            Name = "Microsoft Edge",
                            ExecutablePath = edgePath,
                            Version = FileVersionInfo.GetVersionInfo(edgePath).ProductVersion,
                            IconPath = edgePath,
                            IconIndex = 0,
                            // http://mikenation.net/files/win-10-reg.txt
                            FileAssociations = CreateEdgeFileAssociations().AsReadOnly(),
                            UrlAssociations = new ReadOnlyDictionary<string, object>(new Dictionary<string, object>()),

                            Source = EBrowserSource.HardCoded,
                        });
                    }
                }
            }

            #endregion

#if WINDOWS
            var supportedExtensions = new[] { ".htm", ".html", ".pdf" };
            var pm = new Windows.Management.Deployment.PackageManager();
            var packages = pm.FindPackagesForUser(string.Empty)
                .SelectMany(b => b.GetAppListEntries())
                .Where(a => a.AppInfo.SupportedFileExtensions != null)
                .Where(a => a.AppInfo.SupportedFileExtensions.Intersect(supportedExtensions).Any());
            //  .ToArray();

            result.AddRange(packages.Select(p => new Browser
            {
                Name = p.DisplayInfo.DisplayName,
                IconPath = p.AppInfo.Package.Logo.LocalPath,
                IconIndex = 0,
                Version = p.AppInfo.Package.Id.Version.ToFormattedString(),
                KeyName = null,
                Source = EBrowserSource.PackageManager,
                FileAssociations = p.AppInfo.SupportedFileExtensions.ToDictionary(x => x, x => (object)p.AppUserModelId).AsReadOnly(),
                ExecutablePath = p.AppInfo.Package.InstalledPath,
                UrlAssociations = new[] { "http", "https" }.ToDictionary(x => x, x => (object)p.AppUserModelId).AsReadOnly(),
            }));
#endif

            return new ReadOnlyCollection<Browser>(result);
        });

        return browsers;
    }

    private static Dictionary<string, object> CreateEdgeFileAssociations()
    {
        var edgeAssociations = new Dictionary<string, object>
        {
            { ".htm", "AppX4hxtad77fbk3jkkeerkrm0ze94wjf3s9" },
            { ".html", "AppX4hxtad77fbk3jkkeerkrm0ze94wjf3s9" },
            { ".pdf", "AppXd4nrz8ff68srnhf9t5a8sbjyar1cr723" },
            { ".xml", "AppXcc58vyzkbjbs4ky0mxrmxf8278rk9b3t" },
            { ".svg", "AppXde74bfzw9j31bzhcvsrxsyjnhhbq66cs" },
        };
        return edgeAssociations;
    }

    private static Dictionary<string, object> CreateDefaultFileAssociations(string browserIdFormat)
    {
        var associations = new Dictionary<string, object>
        {
            {".htm", string.Format(browserIdFormat, "HTM")},
            {".html", string.Format(browserIdFormat, "HTML")},
            {".pdf", string.Format(browserIdFormat, "PDF")},
            {".shtml", string.Format(browserIdFormat, "SHTML")},
            {".svg", string.Format(browserIdFormat, "SVG")},
            {".webp", string.Format(browserIdFormat, "WEBP")},
            {".xht", string.Format(browserIdFormat, "XHT")},
            {".xhtml", string.Format(browserIdFormat, "XHTML")}
        };
        return associations;
    }

    /// <summary>Get the default browser by protocol (eg. HTTP, HTTPS, FTP, SMS, MailTo, ...)</summary>
    /// <param name="browsers">When you already called GetInstalledBrowsers(), pass it here.</param>
    /// <param name="protocolType">Protocol type</param>
    /// <returns></returns>
    public static Task<Browser?> GetDefaultBrowser(IEnumerable<Browser> browsers, Enums.EProtocolType protocolType)
    {
        var urlAssociationsKey = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\Shell\Associations\URLAssociations");
        var protocolName = Enum.GetName(typeof(Enums.EProtocolType), protocolType)?.ToLower(CultureInfo.InvariantCulture);
        if (urlAssociationsKey != null && !urlAssociationsKey.GetSubKeyNames().Contains(protocolName))
        {
            throw new BrowserException($"No url association for {protocolName}");
        }

        var userChoiceKey = urlAssociationsKey?.OpenSubKey($@"{protocolName}\UserChoice");
        var defaultBrowserProgId = userChoiceKey?.GetValue("ProgId");

        var foundNormalBrowser = browsers.FirstOrDefault(
            b => b.UrlAssociations.Any(
                a => (a.Key == protocolName) && a.Value.Equals(defaultBrowserProgId)
            )
        );

        if (foundNormalBrowser != null)
        {
            return Task.FromResult<Browser?>(foundNormalBrowser);
        }

        var classesRootProgKey = Registry.ClassesRoot.OpenSubKey($@"{defaultBrowserProgId}\Shell\open");
        var appUserModelId = classesRootProgKey.GetValue("AppUserModelID");

        foundNormalBrowser = browsers.FirstOrDefault(
            b => b.UrlAssociations.Any(
                a => (a.Key == protocolName) && a.Value.Equals(appUserModelId)
            )
        );

        if (foundNormalBrowser != null)
        {
            return Task.FromResult<Browser?>(foundNormalBrowser);
        }

        switch (defaultBrowserProgId)
        {
            case "AppX4hxtad77fbk3jkkeerkrm0ze94wjf3s9": // htm, html
            case "AppXd4nrz8ff68srnhf9t5a8sbjyar1cr723": // pdf
            case "AppXde74bfzw9j31bzhcvsrxsyjnhhbq66cs": // svg
            case "AppXcc58vyzkbjbs4ky0mxrmxf8278rk9b3t": // xml
            case "AppXq0fevzme2pys62n3e0fbqa7peapykr8v":
                // Old Edge
                return Task.FromResult(browsers.FirstOrDefault(
                    b => ((b.KeyName == "Microsoft Edge") && (b.Source == EBrowserSource.HardCoded))
                ));
            case "IE.HTTP":
                // Internet Explorer
                return Task.FromResult(browsers.FirstOrDefault(
                    b => b.KeyName == "IEXPLORE.EXE"
                ));
            default:
                return Task.FromResult<Browser?>(null);
        }
    }

    /// <summary>Get the default browser by protocol (eg. HTTP, HTTPS, FTP, SMS, MailTo, ...).</summary>
    /// <param name="protocolType">Protocol type</param>
    /// <returns></returns>
    public static async Task<Browser?> GetDefaultBrowser(Enums.EProtocolType protocolType)
    {
        var browsers = await GetInstalledBrowsers();
        var defaultBrowser = await GetDefaultBrowser(browsers, protocolType);
        return defaultBrowser;
    }

    /// <summary>Get the default browser for the HTTP protocol.</summary>
    /// <returns></returns>
    public static async Task<Browser?> GetDefaultBrowser()
    {
        var browser = await GetDefaultBrowser(Enums.EProtocolType.Http);
        return browser;
    }

    /// <summary>Get the default browser for the specified file type (html, pdf, svg, ...)</summary>
    /// <param name="browsers">When you already called GetInstalledBrowsers(), pass it here.</param>
    /// <param name="fileType">The filetype you want to open</param>
    public static async Task<Browser?> GetDefaultBrowser(IEnumerable<Browser> browsers, Enums.EFileType fileType)
    {
        var browser = await Task.Run(() =>
        {
            var ext = Enum.GetName(typeof(Enums.EFileType), fileType)?.ToLower(CultureInfo.InvariantCulture);
            var fileExtsKey = Registry.CurrentUser.OpenSubKey($@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\FileExts");

            if (fileExtsKey == null || !fileExtsKey.GetSubKeyNames().Contains($@".{ext}"))
            {
                throw new BrowserException("The specified filetype is not present in the registry");
            }

            var fileTypeKey = fileExtsKey.OpenSubKey($@".{ext}\UserChoice");
            if (fileTypeKey == null)
            {
                throw new BrowserException("The specified filetype is not present in the registry");
            }
            var progId = fileTypeKey.GetValue("ProgId");

            return browsers.FirstOrDefault(
                // Don't compare key AND value. Just check if the value exists in the list.
                b => b.FileAssociations.Any(v => v.Value.Equals(progId))
            );
        });

        return browser;
    }

    /// <summary>Get the default browser for the specified file type (html, pdf, svg, ...)</summary>
    /// <param name="fileType">The filetype you want to open</param>
    public static async Task<Browser?> GetDefaultBrowser(Enums.EFileType fileType)
    {
        var browsers = await GetInstalledBrowsers();
        var defaultBrowser = await GetDefaultBrowser(browsers, fileType);
        return defaultBrowser;
    }
}
