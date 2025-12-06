using System.Text.Json;
using System.Text.Json.Serialization;
using MintPlayer.Bacon.Images;

namespace MintPlayer.Bacon.Icons;

/// <summary>
/// Represents an icon containing multiple BaconImages at different resolutions.
/// </summary>
public class BaconIcon
{
    /// <summary>
    /// Name of the icon.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// The images in this icon (typically at different resolutions).
    /// </summary>
    public List<BaconImage> Images { get; set; } = [];

    /// <summary>
    /// Add a new image with the specified size.
    /// </summary>
    public BaconImage AddImage(int width, int height)
    {
        var image = new BaconImage
        {
            Width = width,
            Height = height,
            Name = $"{width}x{height}"
        };
        Images.Add(image);
        return image;
    }

    /// <summary>
    /// Add common icon sizes (16, 24, 32, 48, 64, 128, 256).
    /// </summary>
    public void AddCommonSizes()
    {
        var sizes = new[] { 16, 24, 32, 48, 64, 128, 256 };
        foreach (var size in sizes)
        {
            AddImage(size, size);
        }
    }

    /// <summary>
    /// Get an image by its size.
    /// </summary>
    public BaconImage? GetImage(int width, int height)
    {
        return Images.FirstOrDefault(i => i.Width == width && i.Height == height);
    }

    /// <summary>
    /// Get the image closest to the requested size.
    /// </summary>
    public BaconImage? GetClosestImage(int size)
    {
        if (Images.Count == 0) return null;

        return Images
            .OrderBy(i => Math.Abs(Math.Max(i.Width, i.Height) - size))
            .First();
    }

    /// <summary>
    /// Remove an image from the icon.
    /// </summary>
    public bool RemoveImage(BaconImage image)
    {
        return Images.Remove(image);
    }

    /// <summary>
    /// Sort images by size (smallest first).
    /// </summary>
    public void SortBySize()
    {
        Images = Images.OrderBy(i => Math.Max(i.Width, i.Height)).ToList();
    }

    /// <summary>
    /// Serialize the icon to JSON.
    /// </summary>
    public string ToJson(bool indented = false)
    {
        var options = new JsonSerializerOptions
        {
            WriteIndented = indented,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        return JsonSerializer.Serialize(this, options);
    }

    /// <summary>
    /// Deserialize an icon from JSON.
    /// </summary>
    public static BaconIcon? FromJson(string json)
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        return JsonSerializer.Deserialize<BaconIcon>(json, options);
    }

    /// <summary>
    /// Save the icon to a JSON file.
    /// </summary>
    public void SaveToFile(string path)
    {
        var json = ToJson(true);
        File.WriteAllText(path, json);
    }

    /// <summary>
    /// Load an icon from a JSON file.
    /// </summary>
    public static BaconIcon? LoadFromFile(string path)
    {
        var json = File.ReadAllText(path);
        return FromJson(json);
    }

    /// <summary>
    /// Create a copy of this icon.
    /// </summary>
    public BaconIcon Clone()
    {
        var json = ToJson();
        return FromJson(json) ?? new BaconIcon();
    }
}
