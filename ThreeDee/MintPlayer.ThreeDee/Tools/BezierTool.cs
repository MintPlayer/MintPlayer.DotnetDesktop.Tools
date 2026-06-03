using System.Drawing.Drawing2D;
using System.Numerics;
using MintPlayer.ThreeDee.Commands;
using MintPlayer.ThreeDee.Inference;

namespace MintPlayer.ThreeDee.Tools;

/// <summary>Cubic Bézier: click 4 control points; the curve is flattened to a wire polyline.</summary>
public sealed class BezierTool : ToolBase
{
    const float ChordTol = 0.05f;

    readonly List<Vector3> _ctrl = [];
    Vector3 _planeNormal = Vector3.UnitY, _cursor;
    bool _hasCursor;
    InferenceResult _snap;

    public override string Name => "Bezier";
    public override string Hint => "Click 4 control points to draw a cubic Bézier curve.";

    public override void Activate(ToolContext ctx) { _ctrl.Clear(); _hasCursor = false; _snap = default; }

    (Vector3, Vector3) Plane(ToolContext ctx) => _ctrl.Count == 0 ? ctx.WorkPlane : (_ctrl[0], _planeNormal);

    public override void OnMouseDown(ToolContext ctx, MouseButtons button, Point p, Keys modifiers)
    {
        if (button != MouseButtons.Left) return;
        var r = ctx.Infer(p, null, Plane(ctx), drawing: false);
        if (_ctrl.Count == 0) _planeNormal = r.Type == SnapType.OnFace ? r.Normal : ctx.WorkPlane.Normal;
        _ctrl.Add(r.WorldPoint); _snap = r;
        if (_ctrl.Count == 4)
        {
            ctx.Commit(new AddPolylineCommand(ctx.Scene.Mesh, Tessellation.FlattenBezier(_ctrl[0], _ctrl[1], _ctrl[2], _ctrl[3], ChordTol), closed: false));
            _ctrl.Clear();
        }
        ctx.Invalidate();
    }

    public override void OnMouseMove(ToolContext ctx, Point p, Keys modifiers)
    {
        _snap = ctx.Infer(p, null, Plane(ctx), drawing: false);
        _cursor = _snap.WorldPoint; _hasCursor = true;
        ctx.Invalidate();
    }

    public override void OnKeyDown(ToolContext ctx, Keys key)
    {
        if (key == Keys.Escape) { _ctrl.Clear(); ctx.Invalidate(); }
    }

    public override void DrawOverlay(Graphics g, ToolContext ctx)
    {
        using var hull = new Pen(Color.FromArgb(160, 180, 200, 255), 1.2f) { DashStyle = DashStyle.Dash };
        using var curve = new Pen(Color.FromArgb(255, 235, 235, 240), 1.6f);
        using var marker = new SolidBrush(Color.FromArgb(255, 120, 200, 120));

        var hullPts = _hasCursor ? [.. _ctrl, _cursor] : _ctrl.ToList();
        SelectionOverlay.Polyline(g, ctx.Camera, hullPts, hull, close: false);
        foreach (var c in _ctrl) SelectionOverlay.Marker(g, ctx.Camera, c, marker);

        if (hullPts.Count >= 4)
            SelectionOverlay.Polyline(g, ctx.Camera, Tessellation.FlattenBezier(hullPts[0], hullPts[1], hullPts[2], hullPts[3], ChordTol), curve, close: false);
        if (_hasCursor) SelectionOverlay.DrawSnap(g, ctx.Camera, _snap);
    }
}
