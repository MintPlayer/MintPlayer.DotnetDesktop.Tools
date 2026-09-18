namespace MintPlayer.PlatformBrowser.Test;

static class Program
{
    static async Task Main(string[] args)
    {
        var platformBrowser = new PlatformBrowser();
        var browsers = await platformBrowser.GetInstalledBrowsers();
        foreach (var browser in browsers)
        {
            Console.WriteLine($"Browser: {browser.Name}");
            Console.WriteLine($"Executable: {browser.ExecutablePath}");
            Console.WriteLine($"Version: {browser.Version}");
            Console.WriteLine($"Icon path: {browser.IconPath}");
            Console.WriteLine($"Icon index: {browser.IconIndex}");
            Console.WriteLine();
        }

        try
        {
            var httpDefaultBrowser = await platformBrowser.GetDefaultBrowser(browsers, Enums.EProtocolType.Http);
            Console.WriteLine($"HTTP default browser: {httpDefaultBrowser?.Name}");
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(ex.Message);
            Console.ResetColor();
        }

        try
        {
            var htmlDefaultBrowser = await platformBrowser.GetDefaultBrowser(browsers, Enums.EFileType.html);
            Console.WriteLine($"HTML default browser: {htmlDefaultBrowser?.Name}");
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(ex.Message);
            Console.ResetColor();
        }
        Console.ReadKey();
    }
}
