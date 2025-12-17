using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Text.Json;
using System.Text.Json.Serialization;
using MintPlayer.Bacon.Images.Layers;

namespace MintPlayer.Bacon.Images;

/// <summary>
/// Represents a flexible image with multiple layers.
/// </summary>
public class BaconImage
{
    /// <summary>
    /// Width of the image in pixels.
    /// </summary>
    public int Width { get; set; } = 256;

    /// <summary>
    /// Height of the image in pixels.
    /// </summary>
    public int Height { get; set; } = 256;

    /// <summary>
    /// Name of the image.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// The layers in this image.
    /// </summary>
    public List<Layer> Layers { get; set; } = [];

    /// <summary>
    /// Render the image to a bitmap.
    /// </summary>
    public Bitmap Render()
    {
        var bitmap = new Bitmap(Width, Height, PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(bitmap);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;

        // Start with transparent background
        g.Clear(Color.Transparent);

        // Draw each layer
        foreach (var layer in Layers)
        {
            layer.Draw(g, Width, Height);
        }

        return bitmap;
    }

    /// <summary>
    /// Render the image to a bitmap with the specified size.
    /// </summary>
    public Bitmap Render(int width, int height)
    {
        using var original = Render();
        var scaled = new Bitmap(width, height, PixelFormat.Format32bppArgb);
        using var g = Graphics.FromImage(scaled);
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
        g.DrawImage(original, 0, 0, width, height);
        return scaled;
    }

    /// <summary>
    /// Serialize the image to JSON.
    /// </summary>
    public string ToJson(bool indented = false)
    {
        // Before serializing, ensure all paint layers have their bitmap data saved
        foreach (var layer in Layers.OfType<PaintLayer>())
        {
            layer.SaveBitmapToData();
        }

        var options = new JsonSerializerOptions
        {
            WriteIndented = indented,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        return JsonSerializer.Serialize(this, options);
    }

    /// <summary>
    /// Deserialize an image from JSON.
    /// </summary>
    public static BaconImage? FromJson(string json)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        return JsonSerializer.Deserialize<BaconImage>(json, options);
    }

    /// <summary>
    /// Save the image to a JSON file.
    /// </summary>
    public void SaveToFile(string path)
    {
        var json = ToJson(true);
        File.WriteAllText(path, json);
    }

    /// <summary>
    /// Load an image from a JSON file.
    /// </summary>
    public static BaconImage? LoadFromFile(string path)
    {
        var json = File.ReadAllText(path);
        return FromJson(json);
    }

    /// <summary>
    /// Export the rendered image to a file.
    /// </summary>
    public void ExportToPng(string path)
    {
        using var bitmap = Render();
        bitmap.Save(path, ImageFormat.Png);
    }

    /// <summary>
    /// Export the rendered image to a file with specified size.
    /// </summary>
    public void ExportToPng(string path, int width, int height)
    {
        using var bitmap = Render(width, height);
        bitmap.Save(path, ImageFormat.Png);
    }

    /// <summary>
    /// Create a copy of this image.
    /// </summary>
    public BaconImage Clone()
    {
        var json = ToJson();
        return FromJson(json) ?? new BaconImage();
    }
}
