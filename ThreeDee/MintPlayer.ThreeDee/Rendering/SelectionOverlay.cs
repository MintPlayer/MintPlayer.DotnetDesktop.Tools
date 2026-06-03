using System.Drawing.Drawing2D;
using System.Numerics;
using MintPlayer.ThreeDee.Geometry;
using MintPlayer.ThreeDee.Inference;
using MintPlayer.ThreeDee.Rendering;

namespace MintPlayer.ThreeDee;

/// <summary>
/// 2D GDI+ overlay primitives drawn on top of the rasterized frame (GDI+ gives us alpha for
/// translucent fills). Used for selection/hover highlights and tool previews. Not depth-tested
/// against the scene — acceptable because the entities shown are the front-most under the cursor.
/// </summary>
public static class SelectionOverlay
{
    static readonly Color SelFill = Color.FromArgb(70, 80, 150, 240);
    static readonly Color SelEdge = Color.FromArgb(255, 90, 160, 255);
    static readonly Color HoverFill = Color.FromArgb(45, 130, 195, 255);
    static readonly Color HoverEdge = Color.FromArgb(255, 150, 220, 255);

    public static void DrawSelection(Graphics g, Camera cam, Selection sel)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using var fill = new SolidBrush(SelFill);
        using var pen = new Pen(SelEdge, 2.5f) { LineJoin = LineJoin.Round };
        foreach (var f in sel.Faces) FillFace(g, cam, f, fill, pen);
        foreach (var e in sel.Edges) Line(g, cam, e.A.P, e.B.P, pen);
    }

    public static void DrawHover(Graphics g, Camera cam, PickResult hover, Selection sel)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        using var fill = new SolidBrush(HoverFill);
        using var pen = new Pen(HoverEdge, 2.5f) { LineJoin = LineJoin.Round };
        if (hover.Kind == PickKind.Face && hover.Face is { } f && !sel.Contains(f))
            FillFace(g, cam, f, fill, pen);
        else if (hover.Kind == PickKind.Edge && !sel.Contains(hover.Edge))
            Line(g, cam, hover.Edge.A.P, hover.Edge.B.P, pen);
    }

    public static void DrawBox(Graphics g, Point a, Point b)
    {
        var rect = Rectangle.FromLTRB(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
        bool crossing = b.X < a.X;
        using var pen = new Pen(crossing ? Color.FromArgb(220, 120, 220, 120) : Color.FromArgb(220, 150, 190, 255), 1f)
        {
            DashStyle = crossing ? DashStyle.Dash : DashStyle.Solid,
        };
        g.DrawRectangle(pen, rect);
    }

    // ---- reusable primitives (also used by tool previews) ----

    public static bool TryProject(Camera cam, Vector3 w, out PointF p)
    {
        if (cam.Project(w, out Vector2 s)) { p = new PointF(s.X, s.Y); return true; }
        p = default; return false;
    }

    public static void Line(Graphics g, Camera cam, Vector3 a, Vector3 b, Pen pen)
    {
        if (TryProject(cam, a, out var pa) && TryProject(cam, b, out var pb)) g.DrawLine(pen, pa, pb);
    }

    public static void Polyline(Graphics g, Camera cam, IReadOnlyList<Vector3> pts, Pen pen, bool close)
    {
        for (int i = 0; i + 1 < pts.Count; i++) Line(g, cam, pts[i], pts[i + 1], pen);
        if (close && pts.Count > 2) Line(g, cam, pts[^1], pts[0], pen);
    }

    public static void Marker(Graphics g, Camera cam, Vector3 w, Brush b, float size = 6f)
    {
        if (TryProject(cam, w, out var p))
            g.FillRectangle(b, p.X - size / 2, p.Y - size / 2, size, size);
    }

    /// <summary>Colour used for an axis direction (SketchUp red/green/blue).</summary>
    public static Color AxisColor(Vector3 dir)
    {
        Vector3 a = new(MathF.Abs(dir.X), MathF.Abs(dir.Y), MathF.Abs(dir.Z));
        if (a.X >= a.Y && a.X >= a.Z) return Color.FromArgb(230, 80, 80);    // X red
        if (a.Y >= a.X && a.Y >= a.Z) return Color.FromArgb(90, 200, 90);    // Y green
        return Color.FromArgb(90, 150, 255);                                 // Z blue
    }

    static Color SnapColor(InferenceResult s) => s.Type switch
    {
        SnapType.Endpoint => Color.FromArgb(70, 220, 110),
        SnapType.Midpoint => Color.FromArgb(120, 230, 230),
        SnapType.OnEdge => Color.FromArgb(235, 90, 90),
        SnapType.OnFace => Color.FromArgb(120, 160, 255),
        SnapType.AxisLock => AxisColor(s.Direction),
        _ => Color.FromArgb(230, 230, 235),
    };

    /// <summary>Draw the snap indicator (a coloured marker + label) for an inference result.</summary>
    public static void DrawSnap(Graphics g, Camera cam, InferenceResult s)
    {
        if (s.Type == SnapType.Free || !TryProject(cam, s.WorldPoint, out var p)) return;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        var color = SnapColor(s);
        using var brush = new SolidBrush(color);
        using var pen = new Pen(color, 2f);
        float r = 5f;
        // diamond for points, square for on-face/plane
        if (s.Type is SnapType.Endpoint or SnapType.Midpoint)
            g.FillPolygon(brush, new PointF[] { new(p.X, p.Y - r), new(p.X + r, p.Y), new(p.X, p.Y + r), new(p.X - r, p.Y) });
        else
            g.DrawRectangle(pen, p.X - r, p.Y - r, 2 * r, 2 * r);
        if (s.Label.Length > 0)
            using (var f = new Font("Segoe UI", 7.5f))
            using (var tb = new SolidBrush(color))
                g.DrawString(s.Label, f, tb, p.X + 8, p.Y - 6);
    }

    static void FillFace(Graphics g, Camera cam, Face f, Brush fill, Pen pen)
    {
        var pts = ProjectLoop(cam, f.Vertices().Select(v => v.P));
        if (pts is null) return;
        g.FillPolygon(fill, pts);
        g.DrawPolygon(pen, pts);
    }

    static PointF[]? ProjectLoop(Camera cam, IEnumerable<Vector3> verts)
    {
        var list = new List<PointF>();
        foreach (var v in verts)
        {
            if (!TryProject(cam, v, out var p)) return null;
            list.Add(p);
        }
        return list.Count >= 3 ? list.ToArray() : null;
    }
}
