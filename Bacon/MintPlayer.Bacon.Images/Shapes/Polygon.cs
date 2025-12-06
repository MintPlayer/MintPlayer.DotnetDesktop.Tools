using System.Drawing;
using System.Text.Json.Serialization;
using MintPlayer.Bacon.Images.Data;

namespace MintPlayer.Bacon.Images.Shapes;

/// <summary>
/// A polygon defined by multiple vertices.
/// </summary>
public class Polygon : Plane
{
    public List<SerializablePoint> Points { get; set; } = [];

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

        var pointFs = Points.Select(p => p.Point).ToArray();

        if (Fill != null && Points.Count >= 3)
        {
            using var brush = Fill.Brush;
            g.FillPolygon(brush, pointFs);
        }

        if (Pen != null)
        {
            using var pen = Pen.Pen;
            g.DrawPolygon(pen, pointFs);
        }
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
