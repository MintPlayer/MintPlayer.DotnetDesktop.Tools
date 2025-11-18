using System.Drawing;
using MintPlayer.ObservableCollection; // adjust if namespace differs

namespace MintPlayer.Bacon.Shapes;

public abstract class BaconShape
{
    public bool Visible { get; set; } = true;
    public bool Selected { get; set; }
    public virtual IEnumerable<Point> GetControlPoints() => Enumerable.Empty<Point>();
    public abstract void SetControlPoint(int index, Point value);
    public abstract void Draw(Graphics g);
    public virtual bool HitTest(Point p) => GetBounds().Contains(p);
    public virtual Rectangle GetBounds()
    {
        var pts = GetControlPoints().ToList();
        if (!pts.Any()) return Rectangle.Empty;
        int minX = pts.Min(p => p.X); int maxX = pts.Max(p => p.X);
        int minY = pts.Min(p => p.Y); int maxY = pts.Max(p => p.Y);
        return Rectangle.FromLTRB(minX, minY, maxX, maxY);
    }
    public virtual void Offset(int dx, int dy)
    {
        var pts = GetControlPoints().ToList();
        for (int i = 0; i < pts.Count; i++)
        {
            SetControlPoint(i, new Point(pts[i].X + dx, pts[i].Y + dy));
        }
    }
}
