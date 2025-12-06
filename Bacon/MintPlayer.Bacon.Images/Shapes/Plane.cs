using MintPlayer.Bacon.Images.Data;

namespace MintPlayer.Bacon.Images.Shapes;

/// <summary>
/// Abstract base class for plane-based shapes (can have both fill and stroke).
/// </summary>
public abstract class Plane : Shape
{
    /// <summary>
    /// The pen used to draw the outline.
    /// </summary>
    public SerializablePen? Pen { get; set; } = new(System.Drawing.Color.Black, 1f);

    /// <summary>
    /// The brush used to fill the shape.
    /// </summary>
    public SerializableBrush? Fill { get; set; } = new SerializableSolidBrush(System.Drawing.Color.White);
}
