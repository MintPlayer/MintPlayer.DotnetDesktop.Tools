using System.Drawing;
using System.Text.Json.Serialization;
using MintPlayer.Bacon.Images.Data;

namespace MintPlayer.Bacon.Images.Shapes;

/// <summary>
/// A circle defined by center and a point on the edge.
/// </summary>
public class Circle : Plane
{
    public SerializablePoint Center { get; set; } = new(50, 50);
    public SerializablePoint EdgePoint { get; set; } = new(100, 50);

    [JsonIgnore]
    public float Radius => (float)Math.Sqrt(
        Math.Pow(EdgePoint.X - Center.X, 2) +
        Math.Pow(EdgePoint.Y - Center.Y, 2));

    [JsonIgnore]
    public override IReadOnlyList<SerializablePoint> ControlPoints => [Center, EdgePoint];

    public override void SetControlPoint(int index, PointF newPosition)
    {
        switch (index)
        {
            case 0:
                // When moving center, also move edge point to maintain radius
                var dx = newPosition.X - Center.X;
                var dy = newPosition.Y - Center.Y;
                Center = newPosition;
                EdgePoint = new SerializablePoint(EdgePoint.X + dx, EdgePoint.Y + dy);
                break;
            case 1:
                EdgePoint = newPosition;
                break;
        }
    }

    public override void Draw(Graphics g)
    {
        if (!IsVisible) return;

        var radius = Radius;
        var rect = new RectangleF(Center.X - radius, Center.Y - radius, radius * 2, radius * 2);

        if (Fill != null)
        {
            using var brush = Fill.Brush;
            g.FillEllipse(brush, rect);
        }

        if (Pen != null)
        {
            using var pen = Pen.Pen;
            g.DrawEllipse(pen, rect);
        }
    }

    [JsonIgnore]
    public override RectangleF Bounds
    {
        get
        {
            var radius = Radius;
            return new RectangleF(Center.X - radius, Center.Y - radius, radius * 2, radius * 2);
        }
    }
}
