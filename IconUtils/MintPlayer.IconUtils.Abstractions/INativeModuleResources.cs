namespace MintPlayer.IconUtils;

/// <summary>
/// The Win32 seam: loads a PE file as a data-only module and reads its icon resources.
/// </summary>
/// <remarks>
/// The boundary is drawn deliberately low. Everything above it - reading GRPICONDIR,
/// sizing the output, assembling ICONDIR + ICONDIRENTRY + image data into a .ico byte
/// stream - is ordinary binary parsing, is where the bugs actually are, and is testable
/// against synthetic buffers. Only the seven kernel32 calls live below it, and mocking
/// those would only ever prove that the mock was called.
/// </remarks>
public interface INativeModuleResources
{
    /// <summary>Opens a file as a data-only module so its resources can be read.</summary>
    /// <exception cref="System.ComponentModel.Win32Exception">The module could not be loaded.</exception>
    IModuleHandle Open(string path);
}

/// <summary>An opened module. Disposing it releases the native handle.</summary>
public interface IModuleHandle : IDisposable
{
    /// <summary>Resource names of every RT_GROUP_ICON in the module, in enumeration order.</summary>
    IReadOnlyList<nint> EnumerateIconGroupNames();

    /// <summary>Raw RT_GROUP_ICON resource bytes (a GRPICONDIR followed by GRPICONDIRENTRYs).</summary>
    byte[] GetIconGroupData(nint name);

    /// <summary>Raw RT_ICON resource bytes for a single image within a group.</summary>
    byte[] GetIconImageData(nint id);
}
