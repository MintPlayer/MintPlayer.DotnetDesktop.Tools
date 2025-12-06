using System.Drawing;
using System.Text.Json.Serialization;
using MintPlayer.Bacon.Images.Data;

namespace MintPlayer.Bacon.Images.Shapes;

/// <summary>
/// A curved line defined by control points (cardinal spline).
/// </summary>
public class Curve : Line
{
    public List<SerializablePoint> Points { get; set; } = [];

    /// <summary>
    /// Tension of the curve (0 = straight lines, 0.5 = default, 1 = very curved).
    /// </summary>
    public float Tension { get; set; } = 0.5f;

    [JsonIgnore]
    public override IReadOnlyList<SerializablePoint> ControlPoints => Points;

    public override void SetControlPoint(int index, PointF newPosition)
    {
        if (index >= 0 && index < Points.Count)
        {
            Points[index] = newPosition;
        }
    }

    public override void Draw(Graphics g)
    {
        if (!IsVisible || Points.Count < 2) return;
        using var pen = Pen.Pen;
        var pointFs = Points.Select(p => p.Point).ToArray();
        g.DrawCurve(pen, pointFs, Tension);
    }

    [JsonIgnore]
    public override RectangleF Bounds
    {
        get
        {
            if (Points.Count == 0) return RectangleF.Empty;
            var minX = Points.Min(p => p.X);
            var minY = Points.Min(p => p.Y);
            var maxX = Points.Max(p => p.X);
            var maxY = Points.Max(p => p.Y);
            return new RectangleF(minX, minY, maxX - minX, maxY - minY);
        }
    }
}
