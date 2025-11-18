using System.Drawing;

namespace MintPlayer.Bacon.Shapes;

public class BaconRectangle : BaconPlane
{
    public Rectangle Rectangle { get; set; }

    public override void Draw(Graphics g)
    {
        if (!Visible) return;
        if (Brush is not SolidBrush { Color: var c } || c.A > 0)
        {
            g.FillRectangle(Brush, Rectangle);
        }
        g.DrawRectangle(Pen, Rectangle);
        if (Selected)
        {
            foreach (var cp in GetControlPoints()) DrawControlPoint(g, cp);
        }
    }

    public override bool HitTest(Point p) => Rectangle.Contains(p);
    public override IEnumerable<Point> GetControlPoints() => new[] { Rectangle.Location, new Point(Rectangle.Right, Rectangle.Top), new Point(Rectangle.Right, Rectangle.Bottom), new Point(Rectangle.Left, Rectangle.Bottom) };
    public override void SetControlPoint(int index, Point value)
    {
        var pts = GetControlPoints().ToList();
        if (index < 0 || index >= pts.Count) throw new ArgumentOutOfRangeException(nameof(index));
        // Resize rectangle based on which corner was moved
        int left = Rectangle.Left; int top = Rectangle.Top; int right = Rectangle.Right; int bottom = Rectangle.Bottom;
        switch (index)
        {
            case 0: left = value.X; top = value.Y; break;
            case 1: right = value.X; top = value.Y; break;
            case 2: right = value.X; bottom = value.Y; break;
            case 3: left = value.X; bottom = value.Y; break;
        }
        Rectangle = Rectangle.FromLTRB(Math.Min(left, right), Math.Min(top, bottom), Math.Max(left, right), Math.Max(top, bottom));
    }
    private void DrawControlPoint(Graphics g, Point pt)
    {
        var r = new Rectangle(pt.X - 3, pt.Y - 3, 6, 6);
        g.FillRectangle(Brushes.White, r);
        g.DrawRectangle(Pens.Black, r);
    }
}
