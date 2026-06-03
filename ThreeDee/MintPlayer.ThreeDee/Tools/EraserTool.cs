using MintPlayer.ThreeDee.Commands;
using MintPlayer.ThreeDee.Geometry;
using MintPlayer.ThreeDee.Rendering;

namespace MintPlayer.ThreeDee.Tools;

/// <summary>Click a face to delete it, or a wire edge to delete it. Hover highlights the target.</summary>
public sealed class EraserTool : ToolBase
{
    PickResult _hover = PickResult.None;

    public override string Name => "Eraser";
    public override string Hint => "Click a face or a wire edge to delete it.";

    public override void Activate(ToolContext ctx) => _hover = PickResult.None;

    public override void OnMouseMove(ToolContext ctx, Point p, Keys modifiers)
    {
        var pick = ctx.Picker.Pick(ctx.Ray(p), ctx.Camera);
        if (pick.Kind != _hover.Kind || !ReferenceEquals(pick.Face, _hover.Face) || !pick.Edge.Equals(_hover.Edge))
        { _hover = pick; ctx.Invalidate(); }
    }

    public override void OnMouseDown(ToolContext ctx, MouseButtons button, Point p, Keys modifiers)
    {
        if (button != MouseButtons.Left) return;
        var pick = ctx.Picker.Pick(ctx.Ray(p), ctx.Camera);
        if (pick.Kind == PickKind.Face)
            ctx.Commit(new DeleteCommand(ctx.Scene.Mesh, [pick.Face!], []));
        else if (pick.Kind == PickKind.Edge && IsWire(ctx.Scene.Mesh, pick.Edge))
            ctx.Commit(new DeleteCommand(ctx.Scene.Mesh, [], [pick.Edge]));
        ctx.Invalidate();
    }

    static bool IsWire(Mesh mesh, EdgeKey e) =>
        mesh.Wires.Any(w => (w.A == e.A && w.B == e.B) || (w.A == e.B && w.B == e.A));

    public override void DrawOverlay(Graphics g, ToolContext ctx) =>
        SelectionOverlay.DrawHover(g, ctx.Camera, _hover, ctx.Selection);
}
