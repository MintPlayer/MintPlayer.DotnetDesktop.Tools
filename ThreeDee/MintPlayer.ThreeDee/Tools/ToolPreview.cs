using System.Numerics;

namespace MintPlayer.ThreeDee.Tools;

/// <summary>Draws a transformed ghost of the current selection (Move/Rotate previews).</summary>
static class ToolPreview
{
    public static void DrawSelection(Graphics g, ToolContext ctx, Func<Vector3, Vector3> transform, Pen pen)
    {
        foreach (var f in ctx.Selection.Faces)
            SelectionOverlay.Polyline(g, ctx.Camera, [.. f.Vertices().Select(v => transform(v.P))], pen, close: true);
        foreach (var e in ctx.Selection.Edges)
            SelectionOverlay.Line(g, ctx.Camera, transform(e.A.P), transform(e.B.P), pen);
    }
}
