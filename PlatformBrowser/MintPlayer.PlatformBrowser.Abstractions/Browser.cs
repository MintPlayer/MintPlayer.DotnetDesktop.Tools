using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MintPlayer.PlatformBrowser;

public enum EBrowserSource
{
    /// <summary>Indicates that the browser was found in the Windows Registry.</summary>
    Registry,
    /// <summary>Indicates that the browser is installed as an UWP app, and was found in the <code>Windows.Management.Deployment.PackageManager</code>.</summary>
    PackageManager,
    /// <summary>Indicates that this library did other checks and added this browser manually to the list.</summary>
    HardCoded,
}

public class Browser
{
    // Widened from internal to public when this type moved into the abstractions package.
    // The implementation now lives in a different assembly and has to be able to construct
    // and classify a Browser, and a fake catalog in a test does too. Widening is not a
    // breaking change for existing consumers, and both members are genuinely useful: where
    // a browser was found decides how it is launched and grouped.
    public Browser()
    {
    }

    /// <summary>Where this browser was found: the registry, the UWP package manager, or a hard-coded probe.</summary>
    public EBrowserSource Source { get; set; }

    /// <summary>The registry key name this browser was read from, or null for browsers that did not come from the registry.</summary>
    public string? KeyName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ExecutablePath { get; set; } = string.Empty;
    public string IconPath { get; set; } = string.Empty;
    public int IconIndex { get; set; }
    public string? Version { get; set; }

    /// <summary>List of file types (.html, .xhtml, ...) that are supported by this webbrowser</summary>
    public ReadOnlyDictionary<string, object> FileAssociations { get; set; } = (new object[0]).ToDictionary(x => string.Empty, x => x).AsReadOnly();
    /// <summary>List of web protocols (HTTP, FTP, ...) that are supported by this webbrowser</summary>
    public ReadOnlyDictionary<string, object> UrlAssociations { get; set; } = (new object[0]).ToDictionary(x => string.Empty, x => x).AsReadOnly();

    public override string ToString()
    {
        return Name;
    }
}
