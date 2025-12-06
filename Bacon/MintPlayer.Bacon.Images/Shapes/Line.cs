using MintPlayer.Bacon.Images.Data;

namespace MintPlayer.Bacon.Images.Shapes;

/// <summary>
/// Abstract base class for line-based shapes (no fill, only stroke).
/// </summary>
public abstract class Line : Shape
{
    /// <summary>
    /// The pen used to draw the line.
    /// </summary>
    public SerializablePen Pen { get; set; } = new(System.Drawing.Color.Black, 1f);
}
