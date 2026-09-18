using MintPlayer.PlatformBrowser.Enums;
using MintPlayer.PlatformBrowser.Exceptions;
using MintPlayer.PlatformBrowser.Tests.Fakes;

namespace MintPlayer.PlatformBrowser.Tests;

public class PlatformBrowserTests
{
    private const string StartMenuInternet = @"SOFTWARE\Clients\StartMenuInternet";

    /// <summary>Builds a registry containing one well-formed browser under HKCU.</summary>
    private static FakeRegistry RegistryWithBrowser(
        string keyName = "firefox.exe",
        string displayName = "Firefox",
        string executablePath = @"C:\Program Files\Firefox\firefox.exe",
        string? iconPath = null,
        string? httpProgId = "FirefoxURL")
    {
        var registry = new FakeRegistry();
        registry.CurrentUser.WithSubKey($@"{StartMenuInternet}\{keyName}", browser =>
        {
            browser.WithDefault(displayName);
            browser.WithSubKey(@"shell\open\command", command => command.WithDefault($"\"{executablePath}\""));
            browser.WithSubKey("DefaultIcon", icon => icon.WithDefault(iconPath ?? $"{executablePath},0"));
            if (httpProgId != null)
            {
                browser.WithSubKey(@"Capabilities\URLAssociations", url => url.With("http", httpProgId));
            }
        });
        return registry;
    }

    private static PlatformBrowser Create(
        FakeRegistry registry,
        FakeBrowserFileSystem? fileSystem = null,
        FakePackagedBrowserCatalog? packaged = null) =>
        new(registry, fileSystem ?? new FakeBrowserFileSystem(), packaged ?? new FakePackagedBrowserCatalog());

    [Fact]
    public async Task Reads_a_browser_out_of_the_registry()
    {
        var fileSystem = new FakeBrowserFileSystem();
        fileSystem.ProductVersions[@"C:\Program Files\Firefox\firefox.exe"] = "128.0";

        var browsers = await Create(RegistryWithBrowser(), fileSystem).GetInstalledBrowsers();

        var browser = Assert.Single(browsers);
        Assert.Equal("Firefox", browser.Name);
        Assert.Equal("firefox.exe", browser.KeyName);
        Assert.Equal(@"C:\Program Files\Firefox\firefox.exe", browser.ExecutablePath);
        Assert.Equal("128.0", browser.Version);
        Assert.Equal(EBrowserSource.Registry, browser.Source);
    }

    [Fact]
    public async Task Strips_the_quotes_the_registry_wraps_the_command_in()
    {
        var browsers = await Create(RegistryWithBrowser()).GetInstalledBrowsers();

        Assert.DoesNotContain('"', Assert.Single(browsers).ExecutablePath);
    }

    [Fact]
    public async Task Skips_a_browser_whose_command_is_not_an_exe()
    {
        var registry = RegistryWithBrowser(executablePath: @"C:\Program Files\Weird\browser.bat");

        var browsers = await Create(registry).GetInstalledBrowsers();

        // A disconfigured entry is skipped, not fatal.
        Assert.Empty(browsers);
    }

    [Fact]
    public async Task Skips_a_browser_with_no_shell_open_command_key()
    {
        var registry = new FakeRegistry();
        registry.CurrentUser.WithSubKey($@"{StartMenuInternet}\broken.exe", browser => browser.WithDefault("Broken"));

        var browsers = await Create(registry).GetInstalledBrowsers();

        Assert.Empty(browsers);
    }

    [Fact]
    public async Task One_disconfigured_browser_does_not_hide_the_others()
    {
        var registry = RegistryWithBrowser();
        registry.CurrentUser.WithSubKey($@"{StartMenuInternet}\broken.exe", browser => browser.WithDefault("Broken"));

        var browsers = await Create(registry).GetInstalledBrowsers();

        Assert.Equal("Firefox", Assert.Single(browsers).Name);
    }

    [Fact]
    public async Task Parses_the_icon_index_out_of_the_DefaultIcon_value()
    {
        var registry = RegistryWithBrowser(iconPath: @"C:\Program Files\Firefox\firefox.exe,3");

        var browser = Assert.Single(await Create(registry).GetInstalledBrowsers());

        Assert.Equal(@"C:\Program Files\Firefox\firefox.exe", browser.IconPath);
        Assert.Equal(3, browser.IconIndex);
    }

    [Fact]
    public async Task Falls_back_to_the_executable_when_the_icon_is_not_an_exe_or_ico()
    {
        var registry = RegistryWithBrowser(iconPath: @"C:\Program Files\Firefox\logo.png,0");

        var browser = Assert.Single(await Create(registry).GetInstalledBrowsers());

        Assert.Equal(@"C:\Program Files\Firefox\firefox.exe", browser.IconPath);
        Assert.Equal(0, browser.IconIndex);
    }

    [Fact]
    public async Task Does_not_list_the_same_browser_twice_when_it_is_in_both_hives()
    {
        var registry = RegistryWithBrowser();
        registry.LocalMachine.WithSubKey($@"{StartMenuInternet}\firefox.exe", browser =>
        {
            browser.WithDefault("Firefox (machine)");
            browser.WithSubKey(@"shell\open\command", c => c.WithDefault(@"C:\Program Files\Firefox\firefox.exe"));
            browser.WithSubKey("DefaultIcon", i => i.WithDefault(@"C:\Program Files\Firefox\firefox.exe,0"));
        });

        var browsers = await Create(registry).GetInstalledBrowsers();

        // HKCU is read first and wins.
        Assert.Equal("Firefox", Assert.Single(browsers).Name);
    }

    [Fact]
    public async Task Adds_legacy_Edge_when_its_folder_and_executable_are_present()
    {
        var fileSystem = new FakeBrowserFileSystem();
        fileSystem.Directories.Add(@"C:\Windows\SystemApps\");
        fileSystem.Directories.Add(@"C:\Windows\SystemApps\Microsoft.MicrosoftEdge_8wekyb3d8bbwe");
        fileSystem.Files.Add(@"C:\Windows\SystemApps\Microsoft.MicrosoftEdge_8wekyb3d8bbwe\MicrosoftEdge.exe");

        var browsers = await Create(new FakeRegistry(), fileSystem).GetInstalledBrowsers();

        var edge = Assert.Single(browsers);
        Assert.Equal("Microsoft Edge", edge.Name);
        Assert.Equal(EBrowserSource.HardCoded, edge.Source);
        Assert.Contains(".htm", edge.FileAssociations.Keys);
    }

    [Fact]
    public async Task Does_not_add_legacy_Edge_when_the_executable_is_missing()
    {
        var fileSystem = new FakeBrowserFileSystem();
        fileSystem.Directories.Add(@"C:\Windows\SystemApps\");
        fileSystem.Directories.Add(@"C:\Windows\SystemApps\Microsoft.MicrosoftEdge_8wekyb3d8bbwe");

        var browsers = await Create(new FakeRegistry(), fileSystem).GetInstalledBrowsers();

        Assert.Empty(browsers);
    }

    [Fact]
    public async Task Includes_packaged_browsers_and_asks_for_the_web_extensions()
    {
        var packagedBrowser = new Browser { Name = "Store Browser", Source = EBrowserSource.PackageManager };
        var packaged = new FakePackagedBrowserCatalog(packagedBrowser);

        var browsers = await Create(new FakeRegistry(), packaged: packaged).GetInstalledBrowsers();

        Assert.Equal("Store Browser", Assert.Single(browsers).Name);
        Assert.Equal([".htm", ".html", ".pdf"], packaged.RequestedExtensions);
    }

    [Fact]
    public async Task Finds_the_default_browser_by_its_url_association()
    {
        var registry = RegistryWithBrowser();
        registry.CurrentUser.WithSubKey(@"SOFTWARE\Microsoft\Windows\Shell\Associations\URLAssociations\http\UserChoice",
            choice => choice.With("ProgId", "FirefoxURL"));

        var platformBrowser = Create(registry);
        var browsers = await platformBrowser.GetInstalledBrowsers();

        var defaultBrowser = await platformBrowser.GetDefaultBrowser(browsers, EProtocolType.Http);

        Assert.Equal("Firefox", defaultBrowser?.Name);
    }

    [Fact]
    public async Task Throws_when_the_protocol_has_no_url_association_at_all()
    {
        var registry = RegistryWithBrowser();
        registry.CurrentUser.WithSubKey(@"SOFTWARE\Microsoft\Windows\Shell\Associations\URLAssociations\ftp");

        var platformBrowser = Create(registry);
        var browsers = await platformBrowser.GetInstalledBrowsers();

        await Assert.ThrowsAsync<BrowserException>(
            () => platformBrowser.GetDefaultBrowser(browsers, EProtocolType.Http));
    }

    [Fact]
    public async Task Returns_null_rather_than_throwing_when_the_ProgId_has_no_Shell_open_key()
    {
        // Regression: this dereferenced the ClassesRoot key unguarded and threw
        // NullReferenceException out of a method whose return type is already nullable.
        var registry = RegistryWithBrowser(httpProgId: "SomeOtherBrowserURL");
        registry.CurrentUser.WithSubKey(@"SOFTWARE\Microsoft\Windows\Shell\Associations\URLAssociations\http\UserChoice",
            choice => choice.With("ProgId", "ProgIdWithNoClassesRootEntry"));

        var platformBrowser = Create(registry);
        var browsers = await platformBrowser.GetInstalledBrowsers();

        Assert.Null(await platformBrowser.GetDefaultBrowser(browsers, EProtocolType.Http));
    }

    [Fact]
    public async Task Matches_the_default_browser_through_its_AppUserModelID()
    {
        var registry = RegistryWithBrowser(httpProgId: "AppUserModelIdOfTheBrowser");
        registry.CurrentUser.WithSubKey(@"SOFTWARE\Microsoft\Windows\Shell\Associations\URLAssociations\http\UserChoice",
            choice => choice.With("ProgId", "PackagedBrowserProgId"));
        registry.ClassesRoot.WithSubKey(@"PackagedBrowserProgId\Shell\open",
            open => open.With("AppUserModelID", "AppUserModelIdOfTheBrowser"));

        var platformBrowser = Create(registry);
        var browsers = await platformBrowser.GetInstalledBrowsers();

        Assert.Equal("Firefox", (await platformBrowser.GetDefaultBrowser(browsers, EProtocolType.Http))?.Name);
    }

    [Fact]
    public async Task Maps_the_legacy_Edge_ProgIds_onto_the_hard_coded_Edge_entry()
    {
        var fileSystem = new FakeBrowserFileSystem();
        fileSystem.Directories.Add(@"C:\Windows\SystemApps\");
        fileSystem.Directories.Add(@"C:\Windows\SystemApps\Microsoft.MicrosoftEdge_x");
        fileSystem.Files.Add(@"C:\Windows\SystemApps\Microsoft.MicrosoftEdge_x\MicrosoftEdge.exe");

        var registry = new FakeRegistry();
        registry.CurrentUser.WithSubKey(@"SOFTWARE\Microsoft\Windows\Shell\Associations\URLAssociations\http\UserChoice",
            choice => choice.With("ProgId", "AppX4hxtad77fbk3jkkeerkrm0ze94wjf3s9"));

        var platformBrowser = Create(registry, fileSystem);
        var browsers = await platformBrowser.GetInstalledBrowsers();

        Assert.Equal("Microsoft Edge", (await platformBrowser.GetDefaultBrowser(browsers, EProtocolType.Http))?.Name);
    }

    [Fact]
    public async Task Finds_the_default_browser_for_a_file_type()
    {
        var registry = RegistryWithBrowser();
        registry.CurrentUser.WithSubKey($@"{StartMenuInternet}\firefox.exe\Capabilities\FileAssociations",
            file => file.With(".html", "FirefoxHTML"));
        registry.CurrentUser.WithSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\FileExts\.html\UserChoice",
            choice => choice.With("ProgId", "FirefoxHTML"));

        var platformBrowser = Create(registry);
        var browsers = await platformBrowser.GetInstalledBrowsers();

        Assert.Equal("Firefox", (await platformBrowser.GetDefaultBrowser(browsers, EFileType.html))?.Name);
    }

    [Fact]
    public async Task Throws_when_the_file_type_is_not_registered()
    {
        var platformBrowser = Create(RegistryWithBrowser());
        var browsers = await platformBrowser.GetInstalledBrowsers();

        await Assert.ThrowsAsync<BrowserException>(
            () => platformBrowser.GetDefaultBrowser(browsers, EFileType.html));
    }
}
