using System.Drawing;

namespace MintPlayer.Bacon.Shapes;

public class BaconLineSegment : BaconLine
{
    public Point Start { get; set; }
    public Point End { get; set; }
    public override void Draw(Graphics g)
    {
        if (!Visible) return;
        g.DrawLine(Pen, Start, End);
        if (Selected)
        {
            DrawControlPoint(g, Start);
            DrawControlPoint(g, End);
        }
    }
    public override bool HitTest(Point p)
    {
        // Simple distance-to-segment check
        var dx = End.X - Start.X;
        var dy = End.Y - Start.Y;
        if (dx == 0 && dy == 0) return (Math.Abs(p.X - Start.X) <= 3 && Math.Abs(p.Y - Start.Y) <= 3);
        double t = ((p.X - Start.X) * dx + (p.Y - Start.Y) * dy) / (double)(dx * dx + dy * dy);
        t = Math.Max(0, Math.Min(1, t));
        double projX = Start.X + t * dx;
        double projY = Start.Y + t * dy;
        double dist = Math.Sqrt((p.X - projX) * (p.X - projX) + (p.Y - projY) * (p.Y - projY));
        return dist <= Pen.Width + 3;
    }
    public override IEnumerable<Point> GetControlPoints() => new[] { Start, End };
    public override void SetControlPoint(int index, Point value)
    {
        if (index == 0) Start = value; else if (index == 1) End = value; else throw new ArgumentOutOfRangeException(nameof(index));
    }
    private void DrawControlPoint(Graphics g, Point pt)
    {
        var r = new Rectangle(pt.X - 3, pt.Y - 3, 6, 6);
        g.FillRectangle(Brushes.White, r);
        g.DrawRectangle(Pens.Black, r);
    }
}
