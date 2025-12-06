using System.Drawing;
using System.Drawing.Imaging;
using System.Text.Json.Serialization;

namespace MintPlayer.Bacon.Images.Layers;

/// <summary>
/// A layer that contains a bitmap for freehand drawing.
/// </summary>
public class PaintLayer : Layer
{
    /// <summary>
    /// The bitmap data encoded as Base64 PNG.
    /// </summary>
    public string? BitmapData { get; set; }

    /// <summary>
    /// Width of the bitmap.
    /// </summary>
    public int BitmapWidth { get; set; }

    /// <summary>
    /// Height of the bitmap.
    /// </summary>
    public int BitmapHeight { get; set; }

    [JsonIgnore]
    private Bitmap? _bitmap;

    /// <summary>
    /// Get or set the bitmap directly (not serialized, use BitmapData for serialization).
    /// </summary>
    [JsonIgnore]
    public Bitmap? Bitmap
    {
        get
        {
            if (_bitmap == null && !string.IsNullOrEmpty(BitmapData))
            {
                LoadBitmapFromData();
            }
            return _bitmap;
        }
        set
        {
            _bitmap?.Dispose();
            _bitmap = value;
            if (value != null)
            {
                BitmapWidth = value.Width;
                BitmapHeight = value.Height;
                SaveBitmapToData();
            }
            else
            {
                BitmapData = null;
                BitmapWidth = 0;
                BitmapHeight = 0;
            }
        }
    }

    /// <summary>
    /// Create or resize the bitmap to the specified dimensions.
    /// </summary>
    public void EnsureBitmap(int width, int height)
    {
        if (_bitmap != null && _bitmap.Width == width && _bitmap.Height == height)
            return;

        var newBitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb);

        // Copy existing content if present
        if (_bitmap != null)
        {
            using var g = Graphics.FromImage(newBitmap);
            g.DrawImage(_bitmap, 0, 0);
            _bitmap.Dispose();
        }

        _bitmap = newBitmap;
        BitmapWidth = width;
        BitmapHeight = height;
    }

    /// <summary>
    /// Save the bitmap to the BitmapData property as Base64 PNG.
    /// </summary>
    public void SaveBitmapToData()
    {
        if (_bitmap == null)
        {
            BitmapData = null;
            return;
        }

        using var ms = new MemoryStream();
        _bitmap.Save(ms, ImageFormat.Png);
        BitmapData = Convert.ToBase64String(ms.ToArray());
    }

    private void LoadBitmapFromData()
    {
        if (string.IsNullOrEmpty(BitmapData)) return;

        try
        {
            var bytes = Convert.FromBase64String(BitmapData);
            using var ms = new MemoryStream(bytes);
            _bitmap = new Bitmap(ms);
        }
        catch
        {
            _bitmap = null;
        }
    }

    public override void Draw(Graphics g, int width, int height)
    {
        if (!IsVisible || Bitmap == null) return;

        if (Opacity < 1.0f)
        {
            var colorMatrix = new ColorMatrix
            {
                Matrix33 = Opacity
            };

            using var imageAttributes = new ImageAttributes();
            imageAttributes.SetColorMatrix(colorMatrix);

            g.DrawImage(Bitmap,
                new System.Drawing.Rectangle(0, 0, Bitmap.Width, Bitmap.Height),
                0, 0, Bitmap.Width, Bitmap.Height,
                GraphicsUnit.Pixel,
                imageAttributes);
        }
        else
        {
            g.DrawImage(Bitmap, 0, 0);
        }
    }

    /// <summary>
    /// Get the graphics object for drawing on the paint layer.
    /// </summary>
    public Graphics? GetGraphics()
    {
        if (Bitmap == null) return null;
        return Graphics.FromImage(Bitmap);
    }
}
