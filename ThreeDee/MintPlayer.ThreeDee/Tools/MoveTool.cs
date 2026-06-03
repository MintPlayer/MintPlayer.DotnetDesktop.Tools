using System.Drawing.Drawing2D;
using System.Numerics;
using MintPlayer.ThreeDee.Commands;
using MintPlayer.ThreeDee.Geometry;

namespace MintPlayer.ThreeDee.Tools;

/// <summary>Move the selection: click a base point then a destination (or type a distance along the direction).</summary>
public sealed class MoveTool : ToolBase
{
    bool _hasBase, _hasCursor;
    Vector3 _base, _cursor;
    Vertex[] _verts = [];

    public override string Name => "Move";
    public override string Hint => "Select geometry first, then click a base point and a destination (or type a distance).";
    public override bool WantsValue => _hasBase;
    public override string? Measurement => _hasBase && _hasCursor ? Num.Fmt((_cursor - _base).Length()) : null;

    public override void Activate(ToolContext ctx) { _hasBase = _hasCursor = false; }

    public override void OnMouseDown(ToolContext ctx, MouseButtons button, Point p, Keys modifiers)
    {
        if (button != MouseButtons.Left) return;
        if (!_hasBase)
        {
            if (ctx.Selection.IsEmpty) return;
            _base = ctx.Infer(p, null, ctx.WorkPlane, drawing: false).WorldPoint;
            _verts = ctx.Selection.Vertices();
            _hasBase = true;
        }
        else
        {
            Vector3 dst = ctx.Infer(p, _base, (_base, ctx.WorkPlane.Normal), drawing: true).WorldPoint;
            ctx.Commit(new MoveCommand(_verts, dst - _base));
            _hasBase = false;
        }
        ctx.Invalidate();
    }

    public override void OnMouseMove(ToolContext ctx, Point p, Keys modifiers)
    {
        _cursor = _hasBase
            ? ctx.Infer(p, _base, (_base, ctx.WorkPlane.Normal), drawing: true).WorldPoint
            : ctx.Infer(p, null, ctx.WorkPlane, drawing: false).WorldPoint;
        _hasCursor = true;
        ctx.Invalidate();
    }

    public override bool TryApplyValue(ToolContext ctx, string text)
    {
        if (!_hasBase || !_hasCursor || !Num.TryParse(text, out float dist) || dist <= 0) return false;
        Vector3 dir = _cursor - _base;
        if (dir.LengthSquared() < 1e-9f) return false;
        ctx.Commit(new MoveCommand(_verts, Vector3.Normalize(dir) * dist));
        _hasBase = false;
        ctx.Invalidate();
        return true;
    }

    public override void OnKeyDown(ToolContext ctx, Keys key)
    {
        if (key == Keys.Escape) { _hasBase = false; ctx.Invalidate(); }
    }

    public override void DrawOverlay(Graphics g, ToolContext ctx)
    {
        if (!_hasBase || !_hasCursor) return;
        Vector3 delta = _cursor - _base;
        using var ghost = new Pen(Color.FromArgb(220, 120, 200, 255), 1.5f);
        using var vec = new Pen(Color.FromArgb(200, 235, 235, 240), 1.2f) { DashStyle = DashStyle.Dash };
        ToolPreview.DrawSelection(g, ctx, p => p + delta, ghost);
        SelectionOverlay.Line(g, ctx.Camera, _base, _cursor, vec);
    }
}
