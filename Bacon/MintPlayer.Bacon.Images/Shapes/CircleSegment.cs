using System.Drawing;
using System.Text.Json.Serialization;
using MintPlayer.Bacon.Images.Data;

namespace MintPlayer.Bacon.Images.Shapes;

/// <summary>
/// An arc (circle segment) defined by three points on the circle.
/// </summary>
public class CircleSegment : Line
{
    public SerializablePoint Point1 { get; set; } = new(0, 50);
    public SerializablePoint Point2 { get; set; } = new(50, 0);
    public SerializablePoint Point3 { get; set; } = new(100, 50);

    [JsonIgnore]
    public override IReadOnlyList<SerializablePoint> ControlPoints => [Point1, Point2, Point3];

    public override void SetControlPoint(int index, PointF newPosition)
    {
        switch (index)
        {
            case 0:
                Point1 = newPosition;
                break;
            case 1:
                Point2 = newPosition;
                break;
            case 2:
                Point3 = newPosition;
                break;
        }
    }

    public override void Draw(Graphics g)
    {
        if (!IsVisible) return;

        // Calculate circle center and radius from 3 points
        if (!TryGetCircleFromThreePoints(Point1, Point2, Point3, out var center, out var radius))
        {
            // Points are collinear, draw a straight line instead
            using var pen = Pen.Pen;
            g.DrawLine(pen, Point1, Point3);
            return;
        }

        // Calculate angles for the arc
        var angle1 = Math.Atan2(Point1.Y - center.Y, Point1.X - center.X);
        var angle2 = Math.Atan2(Point2.Y - center.Y, Point2.X - center.X);
        var angle3 = Math.Atan2(Point3.Y - center.Y, Point3.X - center.X);

        // Determine sweep direction
        var startAngle = (float)(angle1 * 180 / Math.PI);
        var sweepAngle = GetSweepAngle(angle1, angle2, angle3);

        using var pen2 = Pen.Pen;
        g.DrawArc(pen2,
            center.X - radius, center.Y - radius,
            radius * 2, radius * 2,
            startAngle, sweepAngle);
    }

    private static bool TryGetCircleFromThreePoints(PointF p1, PointF p2, PointF p3,
        out PointF center, out float radius)
    {
        center = PointF.Empty;
        radius = 0;

        var ax = p1.X;
        var ay = p1.Y;
        var bx = p2.X;
        var by = p2.Y;
        var cx = p3.X;
        var cy = p3.Y;

        var d = 2 * (ax * (by - cy) + bx * (cy - ay) + cx * (ay - by));
        if (Math.Abs(d) < 1e-10)
            return false;

        var ux = ((ax * ax + ay * ay) * (by - cy) + (bx * bx + by * by) * (cy - ay) + (cx * cx + cy * cy) * (ay - by)) / d;
        var uy = ((ax * ax + ay * ay) * (cx - bx) + (bx * bx + by * by) * (ax - cx) + (cx * cx + cy * cy) * (bx - ax)) / d;

        center = new PointF(ux, uy);
        radius = (float)Math.Sqrt((ax - ux) * (ax - ux) + (ay - uy) * (ay - uy));
        return true;
    }

    private static float GetSweepAngle(double angle1, double angle2, double angle3)
    {
        // Normalize angles to [0, 2*PI]
        angle1 = (angle1 + 2 * Math.PI) % (2 * Math.PI);
        angle2 = (angle2 + 2 * Math.PI) % (2 * Math.PI);
        angle3 = (angle3 + 2 * Math.PI) % (2 * Math.PI);

        // Calculate sweep, ensuring we go through point2
        var sweep = angle3 - angle1;
        if (sweep < 0) sweep += 2 * Math.PI;

        // Check if we need to go the other way
        var mid = (angle1 + sweep / 2) % (2 * Math.PI);
        var diff = Math.Abs(mid - angle2);
        if (diff > Math.PI) diff = 2 * Math.PI - diff;

        if (diff > 0.1) // If mid-point doesn't match angle2, go the other way
        {
            sweep = -(2 * Math.PI - sweep);
        }

        return (float)(sweep * 180 / Math.PI);
    }

    [JsonIgnore]
    public override RectangleF Bounds
    {
        get
        {
            if (!TryGetCircleFromThreePoints(Point1, Point2, Point3, out var center, out var radius))
            {
                var minX = Math.Min(Math.Min(Point1.X, Point2.X), Point3.X);
                var minY = Math.Min(Math.Min(Point1.Y, Point2.Y), Point3.Y);
                var maxX = Math.Max(Math.Max(Point1.X, Point2.X), Point3.X);
                var maxY = Math.Max(Math.Max(Point1.Y, Point2.Y), Point3.Y);
                return new RectangleF(minX, minY, maxX - minX, maxY - minY);
            }

            return new RectangleF(center.X - radius, center.Y - radius, radius * 2, radius * 2);
        }
    }
}
