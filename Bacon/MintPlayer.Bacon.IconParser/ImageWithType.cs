using System.Drawing;
using MintPlayer.Bacon.IconParser.Enums;

namespace MintPlayer.Bacon.IconParser;

/// <summary>
/// Represents an image with its type information from an ICO file.
/// </summary>
public class ImageWithType : IDisposable
{
    /// <summary>
    /// The image bitmap.
    /// </summary>
    public Bitmap Image { get; set; }

    /// <summary>
    /// The type of the image (BMP or PNG).
    /// </summary>
    public ImageType Type { get; set; }

    /// <summary>
    /// Creates a new ImageWithType with the specified image and type.
    /// </summary>
    public ImageWithType(Bitmap image, ImageType type)
    {
        Image = image;
        Type = type;
    }

    /// <summary>
    /// Width of the image.
    /// </summary>
    public int Width => Image.Width;

    /// <summary>
    /// Height of the image.
    /// </summary>
    public int Height => Image.Height;

    public void Dispose()
    {
        Image?.Dispose();
    }
}
