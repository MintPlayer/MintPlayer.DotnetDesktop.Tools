using System.Drawing.Drawing2D;
using System.Numerics;
using MintPlayer.ThreeDee.Commands;
using MintPlayer.ThreeDee.Inference;

namespace MintPlayer.ThreeDee.Tools;

/// <summary>Click a center then a radius point to create an N-gon face. Type a radius for exactness.</summary>
public sealed class CircleTool : ToolBase
{
    const int Segments = 24;
    static readonly Vector3 FaceColor = new(205, 205, 212);

    bool _hasCenter, _hasCursor;
    Vector3 _center, _planeNormal = Vector3.UnitY, _u, _v, _cursor;
    InferenceResult _snap;

    public override string Name => "Circle";
    public override string Hint => "Click the center, then a point on the radius (or type a radius).";
    public override bool WantsValue => _hasCenter;
    public override string? Measurement => _hasCenter && _hasCursor ? Num.Fmt((_cursor - _center).Length()) : null;

    public override void Activate(ToolContext ctx) { _hasCenter = _hasCursor = false; _snap = default; }

    public override void OnMouseDown(ToolContext ctx, MouseButtons button, Point p, Keys modifiers)
    {
        if (button != MouseButtons.Left) return;
        if (!_hasCenter)
        {
            var r = ctx.Infer(p, null, ctx.WorkPlane, drawing: false);
            _planeNormal = r.Type == SnapType.OnFace ? r.Normal : ctx.WorkPlane.Normal;
            _center = r.WorldPoint;
            Tessellation.PlaneBasis(_planeNormal, out _u, out _v);
            _hasCenter = true; _snap = r;
        }
        else
        {
            var r = ctx.Infer(p, null, (_center, _planeNormal), drawing: false);
            Commit(ctx, (r.WorldPoint - _center).Length());
        }
        ctx.Invalidate();
    }

    public override void OnMouseMove(ToolContext ctx, Point p, Keys modifiers)
    {
        _snap = ctx.Infer(p, null, _hasCenter ? (_center, _planeNormal) : ctx.WorkPlane, drawing: false);
        _cursor = _snap.WorldPoint; _hasCursor = true;
        ctx.Invalidate();
    }

    public override void OnKeyDown(ToolContext ctx, Keys key)
    {
        if (key == Keys.Escape) { _hasCenter = _hasCursor = false; ctx.Invalidate(); }
    }

    public override bool TryApplyValue(ToolContext ctx, string text)
    {
        if (!_hasCenter || !Num.TryParse(text, out float radius) || radius <= 0) return false;
        Commit(ctx, radius);
        ctx.Invalidate();
        return true;
    }

    void Commit(ToolContext ctx, float radius)
    {
        if (radius > 1e-4f)
            ctx.Commit(new CreateFaceCommand(ctx.Scene.Mesh, Tessellation.Polygon(_center, _u, _v, radius, Segments), FaceColor, _planeNormal));
        _hasCenter = _hasCursor = false;
    }

    public override void DrawOverlay(Graphics g, ToolContext ctx)
    {
        if (_hasCenter && _hasCursor)
        {
            using var pen = new Pen(Color.FromArgb(255, 235, 235, 240), 1.6f);
            SelectionOverlay.Polyline(g, ctx.Camera, Tessellation.Polygon(_center, _u, _v, (_cursor - _center).Length(), Segments), pen, close: true);
        }
        if (_hasCursor) SelectionOverlay.DrawSnap(g, ctx.Camera, _snap);
    }
}
