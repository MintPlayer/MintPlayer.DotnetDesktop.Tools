using System.Drawing.Drawing2D;
using System.Numerics;

namespace MintPlayer.ThreeDee.Tools;

/// <summary>
/// Tape Measure: click two points to read the distance between them (inference-snapped). The
/// measurement persists on screen until you start a new one. Read-only — creates no geometry.
/// </summary>
public sealed class TapeMeasureTool : ToolBase
{
    Vector3? _p1;
    Vector3 _p2, _cursor;
    bool _measured, _hasCursor;

    public override string Name => "Tape Measure";
    public override string Hint => "Click two points to measure the distance between them.";

    public override string? Measurement =>
        _measured ? Num.Fmt(Vector3.Distance(_p1!.Value, _p2))
        : _p1 is { } a && _hasCursor ? Num.Fmt(Vector3.Distance(a, _cursor))
        : null;

    public override void Activate(ToolContext ctx) { _p1 = null; _measured = false; _hasCursor = false; }

    public override void OnMouseDown(ToolContext ctx, MouseButtons button, Point p, Keys modifiers)
    {
        if (button != MouseButtons.Left) return;
        Vector3 pt = ctx.Infer(p, null, ctx.WorkPlane, drawing: false).WorldPoint;
        if (_p1 is null || _measured) { _p1 = pt; _measured = false; }
        else { _p2 = pt; _measured = true; }
        ctx.Invalidate();
    }

    public override void OnMouseMove(ToolContext ctx, Point p, Keys modifiers)
    {
        _cursor = ctx.Infer(p, null, ctx.WorkPlane, drawing: false).WorldPoint;
        _hasCursor = true;
        ctx.Invalidate();
    }

    public override void OnKeyDown(ToolContext ctx, Keys key)
    {
        if (key == Keys.Escape) { _p1 = null; _measured = false; ctx.Invalidate(); }
    }

    public override void DrawOverlay(Graphics g, ToolContext ctx)
    {
        if (_p1 is not { } a) return;
        Vector3 b = _measured ? _p2 : _cursor;
        using var pen = new Pen(Color.FromArgb(255, 250, 230, 120), 1.4f) { DashStyle = DashStyle.Dash };
        SelectionOverlay.Line(g, ctx.Camera, a, b, pen);

        float len = Vector3.Distance(a, b);
        if (SelectionOverlay.TryProject(ctx.Camera, (a + b) * 0.5f, out var mid))
            using (var f = new Font("Segoe UI", 8f))
            using (var br = new SolidBrush(Color.FromArgb(255, 250, 230, 120)))
                g.DrawString(Num.Fmt(len), f, br, mid.X + 6, mid.Y - 8);
    }
}
