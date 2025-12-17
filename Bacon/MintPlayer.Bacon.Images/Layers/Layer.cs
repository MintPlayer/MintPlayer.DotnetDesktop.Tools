using System.Drawing;
using System.Text.Json.Serialization;

namespace MintPlayer.Bacon.Images.Layers;

/// <summary>
/// Abstract base class for all layers.
/// </summary>
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(ShapeLayer), "shapeLayer")]
[JsonDerivedType(typeof(PaintLayer), "paintLayer")]
public abstract class Layer
{
    /// <summary>
    /// Name of the layer.
    /// </summary>
    public string Name { get; set; } = "Layer";

    /// <summary>
    /// Whether the layer is visible.
    /// </summary>
    public bool IsVisible { get; set; } = true;

    /// <summary>
    /// Whether the layer is locked (cannot be edited).
    /// </summary>
    public bool IsLocked { get; set; } = false;

    /// <summary>
    /// Opacity of the layer (0-1).
    /// </summary>
    public float Opacity { get; set; } = 1.0f;

    /// <summary>
    /// Draw the layer on the graphics surface.
    /// </summary>
    public abstract void Draw(Graphics g, int width, int height);
}
