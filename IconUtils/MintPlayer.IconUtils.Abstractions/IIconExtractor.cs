namespace MintPlayer.IconUtils;

/// <summary>Extracts the individual images out of icon files and executables.</summary>
public interface IIconExtractor
{
    /// <summary>Splits up the different images in an icon file or executable.</summary>
    /// <param name="filename">Path to a .exe, .ico or .cur file.</param>
    Task<List<Icon>> Split(string filename);

    /// <summary>Splits a multi-image icon into one icon per image.</summary>
    Task<List<Icon>> ExtractImagesFromIcon(Icon icon);
}
