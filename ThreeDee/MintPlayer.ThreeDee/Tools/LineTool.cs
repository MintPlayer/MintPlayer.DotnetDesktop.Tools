using System.Drawing.Drawing2D;
using System.Numerics;
using MintPlayer.ThreeDee.Commands;
using MintPlayer.ThreeDee.Inference;

namespace MintPlayer.ThreeDee.Tools;

/// <summary>
/// SketchUp-style Line tool with inference + VCB. Each click commits a wire edge (snapped via the
/// inference engine: endpoints/midpoints/edges, and axis lock with red/green/blue feedback). Click
/// the start point (≥3 pts) to close into a face; Enter/double-click/Esc end the run. Type a length
/// to place the next point at an exact distance along the current direction.
/// </summary>
public sealed class LineTool : ToolBase
{
    static readonly Vector3 FaceColor = new(205, 205, 212);

    readonly List<Vector3> _chain = [];
    Vector3 _planePoint, _planeNormal = Vector3.UnitY;
    Vector3 _last, _cursor;
    bool _active, _hasCursor, _overStart;
    InferenceResult _snap;

    public override string Name => "Line";
    public override string Hint => "Click to place points; click the start point to close into a face. Enter/Esc ends.";
    public override bool WantsValue => _active && _chain.Count >= 1 && _hasCursor;
    public override string? Measurement => _active && _hasCursor ? Num.Fmt((_cursor - _last).Length()) : null;

    public override void Activate(ToolContext ctx) => EndRun();

    public override void OnMouseDown(ToolContext ctx, MouseButtons button, Point p, Keys modifiers)
    {
        if (button != MouseButtons.Left) return;
        if (!_active)
        {
            var r = ctx.Infer(p, null, ctx.WorkPlane, drawing: false);
            _planeNormal = r.Type == SnapType.OnFace ? r.Normal : ctx.WorkPlane.Normal;
            _planePoint = r.WorldPoint;
            _last = r.WorldPoint;
            _chain.Clear(); _chain.Add(_last);
            _active = true; _snap = r;
        }
        else if (_chain.Count >= 3 && OverStart(ctx, p))
        {
            ctx.Commit(new CreateFaceCommand(ctx.Scene.Mesh, [.. _chain], FaceColor, _planeNormal));
            EndRun();
        }
        else
        {
            var r = ctx.Infer(p, _last, (_planePoint, _planeNormal), drawing: true);
            AddSegment(ctx, r.WorldPoint);
        }
        ctx.Invalidate();
    }

    public override void OnMouseMove(ToolContext ctx, Point p, Keys modifiers)
    {
        _snap = _active
            ? ctx.Infer(p, _last, (_planePoint, _planeNormal), drawing: true)
            : ctx.Infer(p, null, ctx.WorkPlane, drawing: false);
        _cursor = _snap.WorldPoint;
        _hasCursor = true;
        _overStart = _active && _chain.Count >= 3 && OverStart(ctx, p);
        ctx.Invalidate();
    }

    public override void OnDoubleClick(ToolContext ctx, Point p, Keys modifiers) { EndRun(); ctx.Invalidate(); }

    public override void OnKeyDown(ToolContext ctx, Keys key)
    {
        if (key is Keys.Return or Keys.Escape) { EndRun(); ctx.Invalidate(); }
    }

    public override bool TryApplyValue(ToolContext ctx, string text)
    {
        if (!_active || _chain.Count < 1 || !_hasCursor || !Num.TryParse(text, out float len) || len <= 0) return false;
        Vector3 dir = _cursor - _last;
        if (dir.LengthSquared() < 1e-9f) return false;
        AddSegment(ctx, _last + Vector3.Normalize(dir) * len);
        ctx.Invalidate();
        return true;
    }

    void AddSegment(ToolContext ctx, Vector3 pt)
    {
        Vector3 from = _last;
        ctx.Commit(new AddEdgeCommand(ctx.Scene.Mesh, from, pt));
        _last = pt;
        _chain.Add(pt);
        TryFillLoop(ctx, from, pt);
    }

    // SketchUp auto-face: if the new edge completed a planar wire loop, fill it.
    static void TryFillLoop(ToolContext ctx, Vector3 fromPos, Vector3 toPos)
    {
        var mesh = ctx.Scene.Mesh;
        var a = mesh.FindVertex(fromPos, 1e-4f);
        var b = mesh.FindVertex(toPos, 1e-4f);
        if (a is null || b is null) return;
        if (Geometry.LoopFinder.TryFindFillableLoop(mesh, a, b, out var loop, out var normal))
            ctx.Commit(new CreateFaceCommand(mesh, [.. loop.Select(v => v.P)], FaceColor, normal));
    }

    void EndRun() { _active = false; _chain.Clear(); _hasCursor = false; _overStart = false; _snap = default; }

    bool OverStart(ToolContext ctx, Point p) =>
        ctx.Camera.Project(_chain[0], out Vector2 s)
        && MathF.Sqrt((s.X - p.X) * (s.X - p.X) + (s.Y - p.Y) * (s.Y - p.Y)) <= ctx.PixelTolerance;

    public override void DrawOverlay(Graphics g, ToolContext ctx)
    {
        if (_active && _hasCursor)
        {
            Color c = _snap.Type == SnapType.AxisLock ? SelectionOverlay.AxisColor(_snap.Direction) : Color.FromArgb(220, 210, 215, 255);
            using var rubber = new Pen(c, 1.6f) { DashStyle = DashStyle.Dash };
            SelectionOverlay.Line(g, ctx.Camera, _last, _cursor, rubber);
            if (_overStart)
                using (var close = new SolidBrush(Color.FromArgb(255, 90, 210, 120)))
                    SelectionOverlay.Marker(g, ctx.Camera, _chain[0], close, 11f);
        }
        if (_hasCursor) SelectionOverlay.DrawSnap(g, ctx.Camera, _snap);
    }
}
