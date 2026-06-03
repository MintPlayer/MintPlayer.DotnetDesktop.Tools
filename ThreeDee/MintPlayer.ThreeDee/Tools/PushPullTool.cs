using System.Drawing.Drawing2D;
using System.Numerics;
using MintPlayer.ThreeDee.Commands;
using MintPlayer.ThreeDee.Geometry;
using MintPlayer.ThreeDee.Rendering;

namespace MintPlayer.ThreeDee.Tools;

/// <summary>
/// Push/Pull (the headline tool). Hover a face to highlight it, click to start, move the mouse to
/// set the extrusion distance along the face normal (the cursor is projected onto the pull axis),
/// then click to commit a <see cref="PushPullCommand"/>. Esc cancels.
/// </summary>
public sealed class PushPullTool : ToolBase
{
    Face? _hover;
    bool _active;
    Face? _face;
    Vector3 _axisOrigin, _normal;
    float _distance;

    public override string Name => "Push/Pull";
    public override string Hint => "Click a face, move to set the distance, click to finish (or type a distance).";
    public override bool WantsValue => _active && _face is not null;
    public override string? Measurement => _active ? Num.Fmt(_distance) : null;

    public override bool TryApplyValue(ToolContext ctx, string text)
    {
        if (!_active || _face is null || !Num.TryParse(text, out float dist) || dist == 0) return false;
        float sign = _distance < 0 ? -1f : 1f; // keep the direction the user was dragging
        ctx.Commit(new PushPullCommand(ctx.Scene.Mesh, _face, sign * MathF.Abs(dist)));
        _active = false; _face = null;
        ctx.Invalidate();
        return true;
    }

    public override void Activate(ToolContext ctx) { _active = false; _face = null; _hover = null; _distance = 0; }

    public override void OnMouseDown(ToolContext ctx, MouseButtons button, Point p, Keys modifiers)
    {
        if (button != MouseButtons.Left) return;
        if (!_active)
        {
            var f = ctx.Picker.PickFace(ctx.Ray(p), out _, out _);
            if (f is null) return;
            _face = f; _normal = f.Normal(); _axisOrigin = f.Centroid(); _distance = 0; _active = true;
        }
        else
        {
            if (MathF.Abs(_distance) > 1e-4f && _face is not null)
                ctx.Commit(new PushPullCommand(ctx.Scene.Mesh, _face, _distance));
            _active = false; _face = null;
        }
        ctx.Invalidate();
    }

    public override void OnMouseMove(ToolContext ctx, Point p, Keys modifiers)
    {
        if (_active)
        {
            _distance = DistanceAlongAxis(ctx, p);
            ctx.Invalidate();
        }
        else
        {
            var f = ctx.Picker.PickFace(ctx.Ray(p), out _, out _);
            if (!ReferenceEquals(f, _hover)) { _hover = f; ctx.Invalidate(); }
        }
    }

    public override void OnKeyDown(ToolContext ctx, Keys key)
    {
        if (key == Keys.Escape) { _active = false; _face = null; ctx.Invalidate(); }
    }

    float DistanceAlongAxis(ToolContext ctx, Point p)
    {
        // Closest parameter on the pull axis (origin + t*normal) to the cursor ray.
        Ray ray = ctx.Ray(p);
        Vector3 u = _normal, v = ray.Direction, w0 = _axisOrigin - ray.Origin;
        float b = Vector3.Dot(u, v), d = Vector3.Dot(u, w0), e = Vector3.Dot(v, w0);
        float denom = 1f - b * b;
        return MathF.Abs(denom) < 1e-6f ? _distance : (b * e - d) / denom;
    }

    public override void DrawOverlay(Graphics g, ToolContext ctx)
    {
        if (_active && _face is not null)
        {
            using var pen = new Pen(Color.FromArgb(255, 235, 235, 240), 1.6f) { LineJoin = LineJoin.Round };
            using var connector = new Pen(Color.FromArgb(200, 180, 200, 255), 1.4f) { DashStyle = DashStyle.Dash };
            var orig = _face.Vertices().Select(v => v.P).ToArray();
            var moved = orig.Select(p => p + _normal * _distance).ToArray();
            SelectionOverlay.Polyline(g, ctx.Camera, moved, pen, close: true);
            for (int i = 0; i < orig.Length; i++)
                SelectionOverlay.Line(g, ctx.Camera, orig[i], moved[i], connector);
        }
        else if (_hover is not null)
        {
            SelectionOverlay.DrawHover(g, ctx.Camera, PickResult.OnFace(_hover, _hover.Centroid(), 0f), ctx.Selection);
        }
    }
}
