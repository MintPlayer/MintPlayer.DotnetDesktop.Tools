using System.Drawing.Drawing2D;
using System.Numerics;
using MintPlayer.ThreeDee.Commands;
using MintPlayer.ThreeDee.Inference;

namespace MintPlayer.ThreeDee.Tools;

/// <summary>3-point arc: click start, click end, then move/click to set the bulge. Output is a wire polyline.</summary>
public sealed class ArcTool : ToolBase
{
    const int Segments = 32;

    int _stage; // 0 = pick start, 1 = pick end, 2 = pick bulge
    Vector3 _planeNormal = Vector3.UnitY, _start, _end, _cursor;
    bool _hasCursor, _semicircle;
    InferenceResult _snap;

    public override string Name => "Arc";
    public override string Hint => "Click the start point, the end point, then move/click to set the bulge.";
    public override string? Measurement => _stage == 2 && _semicircle ? "Half circle" : null;

    public override void Activate(ToolContext ctx) { _stage = 0; _hasCursor = false; _semicircle = false; _snap = default; }

    (Vector3, Vector3) Plane(ToolContext ctx) => _stage == 0 ? ctx.WorkPlane : (_start, _planeNormal);

    public override void OnMouseDown(ToolContext ctx, MouseButtons button, Point p, Keys modifiers)
    {
        if (button != MouseButtons.Left) return;
        var r = ctx.Infer(p, null, Plane(ctx), drawing: false);
        if (_stage == 0) { _planeNormal = r.Type == SnapType.OnFace ? r.Normal : ctx.WorkPlane.Normal; _start = r.WorldPoint; _stage = 1; }
        else if (_stage == 1) { _end = r.WorldPoint; _stage = 2; }
        else
        {
            Vector3 bulge = SnapSemicircle(ctx, r.WorldPoint, out _);
            if (Tessellation.Arc3Point(_start, bulge, _end, Segments, out var pts))
                ctx.Commit(new AddPolylineCommand(ctx.Scene.Mesh, pts, closed: false));
            _stage = 0;
        }
        _snap = r;
        ctx.Invalidate();
    }

    public override void OnMouseMove(ToolContext ctx, Point p, Keys modifiers)
    {
        _snap = ctx.Infer(p, null, Plane(ctx), drawing: false);
        _cursor = _stage == 2 ? SnapSemicircle(ctx, _snap.WorldPoint, out _semicircle) : _snap.WorldPoint;
        _hasCursor = true;
        ctx.Invalidate();
    }

    public override void OnKeyDown(ToolContext ctx, Keys key)
    {
        if (key == Keys.Escape) { _stage = 0; ctx.Invalidate(); }
    }

    public override void DrawOverlay(Graphics g, ToolContext ctx)
    {
        using var chord = new Pen(Color.FromArgb(180, 180, 200, 255), 1.3f) { DashStyle = DashStyle.Dash };
        using var arc = new Pen(Color.FromArgb(255, 235, 235, 240), 1.6f);
        // Cyan, like SketchUp's half-circle inference, when the bulge snaps to a semicircle.
        using var semi = new Pen(Color.FromArgb(255, 120, 230, 235), 2f);
        if (_stage == 1 && _hasCursor) SelectionOverlay.Line(g, ctx.Camera, _start, _cursor, chord);
        if (_stage == 2 && _hasCursor)
        {
            SelectionOverlay.Line(g, ctx.Camera, _start, _end, chord);
            if (Tessellation.Arc3Point(_start, _cursor, _end, Segments, out var pts))
                SelectionOverlay.Polyline(g, ctx.Camera, pts, _semicircle ? semi : arc, close: false);
        }
        if (_hasCursor) SelectionOverlay.DrawSnap(g, ctx.Camera, _snap);
    }

    Vector3 SnapSemicircle(ToolContext ctx, Vector3 cursor, out bool semi) =>
        SnapSemicircle(_start, _end, _planeNormal, cursor,
            ctx.PixelTolerance * ctx.Camera.WorldPerPixelAt(ctx.Camera.Distance), out semi);

    /// <summary>
    /// If the bulge point is near a half-circle (chord = diameter), snap it to an exact semicircle.
    /// A semicircle's bulge height (perpendicular from the chord midpoint) equals the radius, which
    /// is half the chord length. Snaps when within <paramref name="tol"/>, preserving the side.
    /// </summary>
    public static Vector3 SnapSemicircle(Vector3 start, Vector3 end, Vector3 planeNormal, Vector3 cursor, float tol, out bool semi)
    {
        semi = false;
        Vector3 chord = end - start;
        float chordLen = chord.Length();
        Vector3 perp = Vector3.Cross(planeNormal, chord);
        if (chordLen < 1e-6f || perp.Length() < 1e-9f) return cursor;
        perp = Vector3.Normalize(perp);
        Vector3 mid = 0.5f * (start + end);
        float h = Vector3.Dot(cursor - mid, perp);   // signed bulge height
        float half = 0.5f * chordLen;                // semicircle bulge = radius = half the chord
        if (MathF.Abs(MathF.Abs(h) - half) <= tol) { semi = true; return mid + MathF.CopySign(half, h) * perp; }
        return cursor;
    }
}
