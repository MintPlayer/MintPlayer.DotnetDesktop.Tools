using System.Drawing;

namespace MintPlayer.Bacon.Shapes;

public class BaconPolygon : BaconPlane
{
    public List<Point> Points { get; set; } = new();
    public override void Draw(Graphics g)
    {
        if (!Visible) return;
        if (Points.Count < 2) return;
        if (Brush is not SolidBrush { Color: var c } || c.A > 0)
        {
            g.FillPolygon(Brush, Points.ToArray());
        }
        g.DrawPolygon(Pen, Points.ToArray());
        if (Selected)
        {
            foreach (var cp in Points) DrawControlPoint(g, cp);
        }
    }

    public override bool HitTest(Point p)
    {
        // ray casting algorithm
        bool inside = false;
        for (int i = 0, j = Points.Count - 1; i < Points.Count; j = i++)
        {
            var pi = Points[i];
            var pj = Points[j];
            bool intersect = ((pi.Y > p.Y) != (pj.Y > p.Y)) &&
                             (p.X < (pj.X - pi.X) * (p.Y - pi.Y) / (double)(pj.Y - pi.Y) + pi.X);
            if (intersect) inside = !inside;
        }
        return inside;
    }
    public override IEnumerable<Point> GetControlPoints() => Points;
    public override void SetControlPoint(int index, Point value)
    {
        if (index < 0 || index >= Points.Count) throw new ArgumentOutOfRangeException(nameof(index));
        Points[index] = value;
    }
    private void DrawControlPoint(Graphics g, Point pt)
    {
        var r = new Rectangle(pt.X - 3, pt.Y - 3, 6, 6);
        g.FillRectangle(Brushes.White, r);
        g.DrawRectangle(Pens.Black, r);
    }
}
