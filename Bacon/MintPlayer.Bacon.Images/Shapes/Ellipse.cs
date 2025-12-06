using System.Drawing;
using System.Text.Json.Serialization;
using MintPlayer.Bacon.Images.Data;

namespace MintPlayer.Bacon.Images.Shapes;

/// <summary>
/// An ellipse defined by a bounding rectangle.
/// </summary>
public class Ellipse : Plane
{
    public SerializablePoint TopLeft { get; set; } = new(0, 0);
    public SerializablePoint BottomRight { get; set; } = new(100, 80);

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
