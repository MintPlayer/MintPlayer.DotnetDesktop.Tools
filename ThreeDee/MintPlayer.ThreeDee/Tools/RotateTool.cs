using System.Drawing.Drawing2D;
using System.Numerics;
using MintPlayer.ThreeDee.Commands;
using MintPlayer.ThreeDee.Geometry;

namespace MintPlayer.ThreeDee.Tools;

/// <summary>
/// Rotate the selection about a vertical axis: click the center, click a reference point, then
/// move/click to set the angle (or type degrees). Rotation axis is +Y for M7.
/// </summary>
public sealed class RotateTool : ToolBase
{
    int _stage; // 0 = center, 1 = reference, 2 = angle
    Vector3 _center, _ref, _cursor, _axis = Vector3.UnitY;
    bool _hasCursor;
    float _angle;
    Vertex[] _verts = [];

    public override string Name => "Rotate";
    public override string Hint => "Select geometry first, then click the center, a reference point, and the angle (or type degrees).";
    public override bool WantsValue => _stage >= 2;
    public override string? Measurement => _stage >= 2 ? Num.Fmt(_angle * 180f / MathF.PI) + "°" : null;

    public override void Activate(ToolContext ctx) { _stage = 0; _hasCursor = false; _angle = 0; }

    public override void OnMouseDown(ToolContext ctx, MouseButtons button, Point p, Keys modifiers)
    {
        if (button != MouseButtons.Left) return;
        if (_stage == 0)
        {
            if (ctx.Selection.IsEmpty) return;
            _center = ctx.Infer(p, null, (Vector3.Zero, Vector3.UnitY), drawing: false).WorldPoint;
            _axis = Vector3.UnitY;
            _verts = ctx.Selection.Vertices();
            _stage = 1;
        }
        else if (_stage == 1) { _ref = ctx.Infer(p, null, (_center, _axis), drawing: false).WorldPoint; _stage = 2; }
        else { ctx.Commit(new RotateCommand(_verts, _center, _axis, _angle)); _stage = 0; }
        ctx.Invalidate();
    }

    public override void OnMouseMove(ToolContext ctx, Point p, Keys modifiers)
    {
        var plane = _stage >= 1 ? (_center, _axis) : (Vector3.Zero, Vector3.UnitY);
        _cursor = ctx.Infer(p, null, plane, drawing: false).WorldPoint;
        if (_stage >= 2) _angle = SignedAngle(_ref - _center, _cursor - _center, _axis);
        _hasCursor = true;
        ctx.Invalidate();
    }

    public override bool TryApplyValue(ToolContext ctx, string text)
    {
        if (_stage < 2 || !Num.TryParse(text, out float deg)) return false;
        ctx.Commit(new RotateCommand(_verts, _center, _axis, deg * MathF.PI / 180f));
        _stage = 0;
        ctx.Invalidate();
        return true;
    }

    public override void OnKeyDown(ToolContext ctx, Keys key)
    {
        if (key == Keys.Escape) { _stage = 0; ctx.Invalidate(); }
    }

    static float SignedAngle(Vector3 a, Vector3 b, Vector3 axis)
    {
        if (a.LengthSquared() < 1e-9f || b.LengthSquared() < 1e-9f) return 0f;
        a = Vector3.Normalize(a); b = Vector3.Normalize(b);
        return MathF.Atan2(Vector3.Dot(Vector3.Cross(a, b), Vector3.Normalize(axis)), Vector3.Dot(a, b));
    }

    public override void DrawOverlay(Graphics g, ToolContext ctx)
    {
        using var spoke = new Pen(Color.FromArgb(200, 235, 235, 240), 1.2f) { DashStyle = DashStyle.Dash };
        using var ghost = new Pen(Color.FromArgb(220, 120, 200, 255), 1.5f);
        using var marker = new SolidBrush(Color.FromArgb(255, 120, 200, 120));

        if (_stage >= 1) SelectionOverlay.Marker(g, ctx.Camera, _center, marker);
        if (_stage >= 2 && _hasCursor)
        {
            SelectionOverlay.Line(g, ctx.Camera, _center, _ref, spoke);
            SelectionOverlay.Line(g, ctx.Camera, _center, _cursor, spoke);
            var q = Matrix4x4.CreateFromAxisAngle(Vector3.Normalize(_axis), _angle);
            ToolPreview.DrawSelection(g, ctx, p => _center + Vector3.Transform(p - _center, q), ghost);
        }
    }
}
