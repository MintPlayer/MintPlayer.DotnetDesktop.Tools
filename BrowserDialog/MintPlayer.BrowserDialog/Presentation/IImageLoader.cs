using System.Drawing;

namespace MintPlayer.BrowserDialog.Presentation;

/// <summary>Loads an image file from disk. The last file-system edge of the presenter.</summary>
public interface IImageLoader
{
    /// <summary>Loads an image, or returns null when it cannot be read.</summary>
    Image? Load(string path);
}

/// <summary>Loads images with <see cref="Image.FromFile(string)"/>. Used by default.</summary>
public sealed class ImageLoader : IImageLoader
{
    /// <inheritdoc />
    public Image? Load(string path)
    {
        // A browser advertising an icon path that is missing or is not an image is a
        // misconfigured install, not a reason to fail the dialog.
        try
        {
            return Image.FromFile(path);
        }
        catch (FileNotFoundException)
        {
            return null;
        }
        catch (OutOfMemoryException)
        {
            // What GDI+ throws for "this file is not an image it understands".
            return null;
        }
        catch (ArgumentException)
        {
            return null;
        }
    }
}
