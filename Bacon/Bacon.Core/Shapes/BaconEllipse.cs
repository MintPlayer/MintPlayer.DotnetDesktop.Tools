using System.Drawing;

namespace MintPlayer.Bacon.Shapes;

public class BaconEllipse : BaconPlane
{
    public Rectangle Bounds { get; set; }
    public override void Draw(Graphics g)
    {
        if (!Visible) return;
        if (Brush is not SolidBrush { Color: var c } || c.A > 0)
        {
            g.FillEllipse(Brush, Bounds);
        }
        g.DrawEllipse(Pen, Bounds);
        if (Selected)
        {
            foreach (var cp in GetControlPoints()) DrawControlPoint(g, cp);
        }
    }
    public override bool HitTest(Point p)
    {
        if (!Bounds.Contains(p)) return false;
        // ellipse equation normalized
        double rx = Bounds.Width / 2.0;
        double ry = Bounds.Height / 2.0;
        double cx = Bounds.Left + rx;
        double cy = Bounds.Top + ry;
        double dx = (p.X - cx) / rx;
        double dy = (p.Y - cy) / ry;
        return dx * dx + dy * dy <= 1.0;
    }
    public override IEnumerable<Point> GetControlPoints() => new[] { Bounds.Location, new Point(Bounds.Right, Bounds.Top), new Point(Bounds.Right, Bounds.Bottom), new Point(Bounds.Left, Bounds.Bottom) };
    public override void SetControlPoint(int index, Point value)
    {
        int left = Bounds.Left; int top = Bounds.Top; int right = Bounds.Right; int bottom = Bounds.Bottom;
        switch (index)
        {
            case 0: left = value.X; top = value.Y; break;
            case 1: right = value.X; top = value.Y; break;
            case 2: right = value.X; bottom = value.Y; break;
            case 3: left = value.X; bottom = value.Y; break;
            default: throw new ArgumentOutOfRangeException(nameof(index));
        }
        Bounds = Rectangle.FromLTRB(Math.Min(left, right), Math.Min(top, bottom), Math.Max(left, right), Math.Max(top, bottom));
    }
    private void DrawControlPoint(Graphics g, Point pt)
    {
        var r = new Rectangle(pt.X - 3, pt.Y - 3, 6, 6);
        g.FillRectangle(Brushes.White, r);
        g.DrawRectangle(Pens.Black, r);
    }
}
