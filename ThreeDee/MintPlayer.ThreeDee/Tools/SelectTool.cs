using System.Numerics;
using MintPlayer.ThreeDee.Geometry;
using MintPlayer.ThreeDee.Rendering;

namespace MintPlayer.ThreeDee.Tools;

/// <summary>Default tool: hover highlight, click-select, Shift/Ctrl toggle, box select, double-click face.</summary>
public sealed class SelectTool : ToolBase
{
    const int BoxThreshold = 3;

    PickResult _hover = PickResult.None;
    bool _leftDown, _boxing;
    Point _start, _end;

    public override string Name => "Select";
    public override string Hint => "Click to select; Shift to add/toggle; drag a box; double-click a face.";

    public override void Activate(ToolContext ctx) { _hover = PickResult.None; _leftDown = _boxing = false; }

    public override void OnMouseDown(ToolContext ctx, MouseButtons button, Point p, Keys modifiers)
    {
        if (button == MouseButtons.Left) { _leftDown = true; _start = p; _boxing = false; }
    }

    public override void OnMouseMove(ToolContext ctx, Point p, Keys modifiers)
    {
        if (_leftDown)
        {
            if (!_boxing && (Math.Abs(p.X - _start.X) > BoxThreshold || Math.Abs(p.Y - _start.Y) > BoxThreshold))
                _boxing = true;
            if (_boxing) { _end = p; ctx.Invalidate(); }
        }
        else
        {
            var pick = ctx.Picker.Pick(ctx.Ray(p), ctx.Camera);
            if (!Same(pick, _hover)) { _hover = pick; ctx.Invalidate(); }
        }
    }

    public override void OnMouseUp(ToolContext ctx, MouseButtons button, Point p, Keys modifiers)
    {
        if (button != MouseButtons.Left) return;
        bool toggle = (modifiers & (Keys.Shift | Keys.Control)) != 0;
        if (_boxing) BoxSelect(ctx, _start, p, toggle);
        else ctx.Selection.Apply(ctx.Picker.Pick(ctx.Ray(p), ctx.Camera), toggle);
        _leftDown = _boxing = false;
        ctx.Invalidate();
    }

    public override void OnDoubleClick(ToolContext ctx, Point p, Keys modifiers)
    {
        var pick = ctx.Picker.Pick(ctx.Ray(p), ctx.Camera);
        bool additive = (modifiers & (Keys.Shift | Keys.Control)) != 0;

        if (pick.Kind == PickKind.Edge)
        {
            // Double-click an edge → select the whole connected curve / edge path.
            if (!additive) ctx.Selection.Clear();
            foreach (var e in ctx.Scene.Mesh.ConnectedEdgePath(pick.Edge)) ctx.Selection.AddEdge(e);
            ctx.Invalidate();
            return;
        }

        var face = pick.Kind == PickKind.Face ? pick.Face : ctx.Picker.PickFace(ctx.Ray(p), out _, out _);
        if (face is null) return;
        if (!additive) ctx.Selection.Clear();
        ctx.Selection.AddFace(face);
        foreach (var h in face.Loop()) ctx.Selection.AddEdge(new EdgeKey(h.Origin, h.To));
        ctx.Invalidate();
    }

    public override void OnKeyDown(ToolContext ctx, Keys key)
    {
        if (key == Keys.Escape) { ctx.Selection.Clear(); ctx.Invalidate(); }
    }

    public override void DrawOverlay(Graphics g, ToolContext ctx)
    {
        SelectionOverlay.DrawHover(g, ctx.Camera, _hover, ctx.Selection);
        if (_boxing) SelectionOverlay.DrawBox(g, _start, _end);
    }

    static bool Same(PickResult a, PickResult b) =>
        a.Kind == b.Kind && ReferenceEquals(a.Face, b.Face) && a.Edge.Equals(b.Edge);

    void BoxSelect(ToolContext ctx, Point a, Point b, bool additive)
    {
        var rect = Rectangle.FromLTRB(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Max(a.X, b.X), Math.Max(a.Y, b.Y));
        bool crossing = b.X < a.X; // right-to-left = crossing (any vertex inside), else window (all inside)
        if (!additive) ctx.Selection.Clear();

        foreach (var f in ctx.Scene.Mesh.Faces)
        {
            int inside = 0, total = 0;
            foreach (var v in f.Vertices()) { total++; if (InRect(ctx, v.P, rect)) inside++; }
            if (crossing ? inside > 0 : inside == total && total > 0) ctx.Selection.AddFace(f);
        }
        foreach (var (va, vb) in ctx.Scene.Mesh.UniqueEdges())
        {
            bool ia = InRect(ctx, va.P, rect), ib = InRect(ctx, vb.P, rect);
            if (crossing ? (ia || ib) : (ia && ib)) ctx.Selection.AddEdge(new EdgeKey(va, vb));
        }
    }

    static bool InRect(ToolContext ctx, Vector3 world, Rectangle rect) =>
        ctx.Camera.Project(world, out Vector2 s) && rect.Contains((int)s.X, (int)s.Y);
}
