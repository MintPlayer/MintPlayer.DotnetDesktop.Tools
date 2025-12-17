namespace MintPlayer.Bacon.IconParser.Enums;

/// <summary>
/// The type of image stored in an ICO file entry.
/// </summary>
public enum ImageType
{
    /// <summary>
    /// BMP format (legacy, typically used for older icons).
    /// </summary>
    Bmp,

    /// <summary>
    /// PNG format (modern, supports transparency and compression).
    /// </summary>
    Png
}
