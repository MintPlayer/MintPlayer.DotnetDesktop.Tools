using System.Drawing.Drawing2D;
using System.Numerics;
using MintPlayer.ThreeDee.Commands;
using MintPlayer.ThreeDee.Inference;

namespace MintPlayer.ThreeDee.Tools;

/// <summary>
/// Rectangle tool: click two opposite corners on a work plane (inference-snapped). Type "w" (square)
/// or "w;d" to set exact dimensions. The rectangle is axis-aligned to the plane basis.
/// </summary>
public sealed class RectangleTool : ToolBase
{
    static readonly Vector3 FaceColor = new(205, 205, 212);

    bool _hasP0, _hasCursor;
    Vector3 _p0, _planeNormal = Vector3.UnitY, _u, _v, _cursor;
    InferenceResult _snap;

    public override string Name => "Rectangle";
    public override string Hint => "Click two opposite corners (or type w;d for exact size).";
    public override bool WantsValue => _hasP0;

    public override string? Measurement
    {
        get
        {
            if (!_hasP0 || !_hasCursor) return null;
            Vector3 dd = _cursor - _p0;
            return $"{Num.Fmt(MathF.Abs(Vector3.Dot(dd, _u)))}; {Num.Fmt(MathF.Abs(Vector3.Dot(dd, _v)))}";
        }
    }

    public override void Activate(ToolContext ctx) { _hasP0 = _hasCursor = false; _snap = default; }

    public override void OnMouseDown(ToolContext ctx, MouseButtons button, Point p, Keys modifiers)
    {
        if (button != MouseButtons.Left) return;
        if (!_hasP0)
        {
            var r = ctx.Infer(p, null, ctx.WorkPlane, drawing: false);
            _planeNormal = r.Type == SnapType.OnFace ? r.Normal : ctx.WorkPlane.Normal;
            _p0 = r.WorldPoint;
            Basis(_planeNormal, out _u, out _v);
            _hasP0 = true; _snap = r;
        }
        else
        {
            var r = ctx.Infer(p, null, (_p0, _planeNormal), drawing: false);
            Commit(ctx, r.WorldPoint);
        }
        ctx.Invalidate();
    }

    public override void OnMouseMove(ToolContext ctx, Point p, Keys modifiers)
    {
        _snap = ctx.Infer(p, null, _hasP0 ? (_p0, _planeNormal) : ctx.WorkPlane, drawing: false);
        _cursor = _snap.WorldPoint;
        _hasCursor = true;
        ctx.Invalidate();
    }

    public override void OnKeyDown(ToolContext ctx, Keys key)
    {
        if (key == Keys.Escape) { _hasP0 = _hasCursor = false; ctx.Invalidate(); }
    }

    public override bool TryApplyValue(ToolContext ctx, string text)
    {
        if (!_hasP0) return false;
        var parts = text.Split([';', 'x', '*'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (parts.Length == 0 || !Num.TryParse(parts[0], out float w)) return false;
        float d = parts.Length >= 2 && Num.TryParse(parts[1], out float dd) ? dd : w;
        if (w <= 0 || d <= 0) return false;

        Vector3 diag = _hasCursor ? _cursor - _p0 : _u + _v;
        float su = Vector3.Dot(diag, _u) < 0 ? -1 : 1, sv = Vector3.Dot(diag, _v) < 0 ? -1 : 1;
        Commit(ctx, _p0 + _u * (su * w) + _v * (sv * d));
        ctx.Invalidate();
        return true;
    }

    void Commit(ToolContext ctx, Vector3 p1)
    {
        ctx.Commit(new CreateFaceCommand(ctx.Scene.Mesh, Corners(p1), FaceColor, _planeNormal));
        _hasP0 = _hasCursor = false;
    }

    Vector3[] Corners(Vector3 p1)
    {
        float du = Vector3.Dot(p1 - _p0, _u), dv = Vector3.Dot(p1 - _p0, _v);
        return [_p0, _p0 + _u * du, _p0 + _u * du + _v * dv, _p0 + _v * dv];
    }

    static void Basis(Vector3 n, out Vector3 u, out Vector3 v)
    {
        u = MathF.Abs(n.Y) < 0.99f ? Vector3.Normalize(Vector3.Cross(Vector3.UnitY, n)) : Vector3.UnitX;
        v = Vector3.Normalize(Vector3.Cross(n, u));
    }

    public override void DrawOverlay(Graphics g, ToolContext ctx)
    {
        if (_hasP0 && _hasCursor)
        {
            using var pen = new Pen(Color.FromArgb(255, 235, 235, 240), 1.6f) { LineJoin = LineJoin.Round };
            SelectionOverlay.Polyline(g, ctx.Camera, Corners(_cursor), pen, close: true);
        }
        if (_hasCursor) SelectionOverlay.DrawSnap(g, ctx.Camera, _snap);
    }
}
