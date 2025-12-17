using System.Drawing;
using System.Text.Json.Serialization;
using MintPlayer.Bacon.Images.Data;

namespace MintPlayer.Bacon.Images.Shapes;

/// <summary>
/// An arc defined by a bounding rectangle and angles.
/// </summary>
public class Arc : Line
{
    public SerializablePoint TopLeft { get; set; } = new(0, 0);
    public SerializablePoint BottomRight { get; set; } = new(100, 100);
    public float StartAngle { get; set; } = 0;
    public float SweepAngle { get; set; } = 90;

    [JsonIgnore]
    public override IReadOnlyList<SerializablePoint> ControlPoints => [TopLeft, BottomRight];

    public override void SetControlPoint(int index, PointF newPosition)
    {
        switch (index)
        {
            case 0:
                TopLeft = newPosition;
                break;
            case 1:
                BottomRight = newPosition;
                break;
        }
    }

    public override void Draw(Graphics g)
    {
        if (!IsVisible) return;
        var rect = GetRectangle();
        if (rect.Width <= 0 || rect.Height <= 0) return;

        using var pen = Pen.Pen;
        g.DrawArc(pen, rect, StartAngle, SweepAngle);
    }

    private RectangleF GetRectangle()
    {
        var x = Math.Min(TopLeft.X, BottomRight.X);
        var y = Math.Min(TopLeft.Y, BottomRight.Y);
        var w = Math.Abs(BottomRight.X - TopLeft.X);
        var h = Math.Abs(BottomRight.Y - TopLeft.Y);
        return new RectangleF(x, y, w, h);
    }

    [JsonIgnore]
    public override RectangleF Bounds => GetRectangle();
}
