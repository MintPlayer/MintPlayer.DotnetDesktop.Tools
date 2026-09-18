namespace MintPlayer.PlatformBrowser;

/// <summary>
/// Resolves the web browsers installed on the machine, and which of them is the default
/// for a given protocol or file type.
/// </summary>
public interface IPlatformBrowser
{
    /// <summary>Retrieves the list of installed browsers.</summary>
    Task<ReadOnlyCollection<Browser>> GetInstalledBrowsers();

    /// <summary>Gets the default browser for a protocol (HTTP, HTTPS, FTP, MailTo, ...).</summary>
    /// <param name="browsers">An already-resolved browser list, to avoid a second scan.</param>
    /// <param name="protocolType">The protocol to look up.</param>
    Task<Browser?> GetDefaultBrowser(IEnumerable<Browser> browsers, EProtocolType protocolType);

    /// <summary>Gets the default browser for a protocol, resolving the browser list first.</summary>
    Task<Browser?> GetDefaultBrowser(EProtocolType protocolType);

    /// <summary>Gets the default browser for the HTTP protocol.</summary>
    Task<Browser?> GetDefaultBrowser();

    /// <summary>Gets the default browser for a file type (html, pdf, svg, ...).</summary>
    /// <param name="browsers">An already-resolved browser list, to avoid a second scan.</param>
    /// <param name="fileType">The file type to look up.</param>
    Task<Browser?> GetDefaultBrowser(IEnumerable<Browser> browsers, EFileType fileType);

    /// <summary>Gets the default browser for a file type, resolving the browser list first.</summary>
    Task<Browser?> GetDefaultBrowser(EFileType fileType);
}
