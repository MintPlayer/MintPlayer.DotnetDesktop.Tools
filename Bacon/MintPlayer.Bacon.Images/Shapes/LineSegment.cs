using System.Drawing;
using System.Text.Json.Serialization;
using MintPlayer.Bacon.Images.Data;

namespace MintPlayer.Bacon.Images.Shapes;

/// <summary>
/// A straight line between two points.
/// </summary>
public class LineSegment : Line
{
    public SerializablePoint Start { get; set; } = new(0, 0);
    public SerializablePoint End { get; set; } = new(100, 100);

    [JsonIgnore]
    public override IReadOnlyList<SerializablePoint> ControlPoints => [Start, End];

    public override void SetControlPoint(int index, PointF newPosition)
    {
        switch (index)
        {
            case 0:
                Start = newPosition;
                break;
            case 1:
                End = newPosition;
                break;
        }
    }

    public override void Draw(Graphics g)
    {
        if (!IsVisible) return;
        using var pen = Pen.Pen;
        g.DrawLine(pen, Start, End);
    }

    [JsonIgnore]
    public override RectangleF Bounds
    {
        get
        {
            var minX = Math.Min(Start.X, End.X);
            var minY = Math.Min(Start.Y, End.Y);
            var maxX = Math.Max(Start.X, End.X);
            var maxY = Math.Max(Start.Y, End.Y);
            return new RectangleF(minX, minY, maxX - minX, maxY - minY);
        }
    }
}
