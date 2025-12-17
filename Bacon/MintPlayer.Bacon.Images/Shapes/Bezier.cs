using System.Drawing;
using System.Text.Json.Serialization;
using MintPlayer.Bacon.Images.Data;

namespace MintPlayer.Bacon.Images.Shapes;

/// <summary>
/// A Bezier curve defined by start, end, and control points.
/// </summary>
public class Bezier : Line
{
    public SerializablePoint Start { get; set; } = new(0, 50);
    public SerializablePoint Control1 { get; set; } = new(25, 0);
    public SerializablePoint Control2 { get; set; } = new(75, 100);
    public SerializablePoint End { get; set; } = new(100, 50);

    [JsonIgnore]
    public override IReadOnlyList<SerializablePoint> ControlPoints => [Start, Control1, Control2, End];

    public override void SetControlPoint(int index, PointF newPosition)
    {
        switch (index)
        {
            case 0:
                Start = newPosition;
                break;
            case 1:
                Control1 = newPosition;
                break;
            case 2:
                Control2 = newPosition;
                break;
            case 3:
                End = newPosition;
                break;
        }
    }

    public override void Draw(Graphics g)
    {
        if (!IsVisible) return;
        using var pen = Pen.Pen;
        g.DrawBezier(pen, Start, Control1, Control2, End);
    }

    [JsonIgnore]
    public override RectangleF Bounds
    {
        get
        {
            var minX = Math.Min(Math.Min(Start.X, Control1.X), Math.Min(Control2.X, End.X));
            var minY = Math.Min(Math.Min(Start.Y, Control1.Y), Math.Min(Control2.Y, End.Y));
            var maxX = Math.Max(Math.Max(Start.X, Control1.X), Math.Max(Control2.X, End.X));
            var maxY = Math.Max(Math.Max(Start.Y, Control1.Y), Math.Max(Control2.Y, End.Y));
            return new RectangleF(minX, minY, maxX - minX, maxY - minY);
        }
    }
}
