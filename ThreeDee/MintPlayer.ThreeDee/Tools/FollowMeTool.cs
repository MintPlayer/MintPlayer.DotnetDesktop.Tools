using System.Numerics;
using MintPlayer.ThreeDee.Commands;
using MintPlayer.ThreeDee.Geometry;
using MintPlayer.ThreeDee.Rendering;

namespace MintPlayer.ThreeDee.Tools;

/// <summary>
/// Follow-Me: select the path edges first (Select tool), then activate this tool and click the
/// profile face to sweep it along that path. Hover highlights the face under the cursor.
/// </summary>
public sealed class FollowMeTool : ToolBase
{
    Face? _hover;

    public override string Name => "Follow Me";
    public override string Hint => "Select the path edge(s) with the Select tool first, then click the profile face to sweep it.";

    public override void Activate(ToolContext ctx) => _hover = null;

    public override void OnMouseMove(ToolContext ctx, Point p, Keys modifiers)
    {
        var f = ctx.Picker.PickFace(ctx.Ray(p), out _, out _);
        if (!ReferenceEquals(f, _hover)) { _hover = f; ctx.Invalidate(); }
    }

    public override void OnMouseDown(ToolContext ctx, MouseButtons button, Point p, Keys modifiers)
    {
        if (button != MouseButtons.Left) return;
        var face = ctx.Picker.PickFace(ctx.Ray(p), out _, out _);
        if (face is null) return;
        var path = OrderPath(ctx.Selection);
        if (path.Count >= 2) ctx.Commit(new FollowMeCommand(ctx.Scene.Mesh, face, path));
        ctx.Invalidate();
    }

    public override void DrawOverlay(Graphics g, ToolContext ctx)
    {
        if (_hover is not null)
            SelectionOverlay.DrawHover(g, ctx.Camera, PickResult.OnFace(_hover, _hover.Centroid(), 0f), ctx.Selection);
    }

    /// <summary>Order the selected edges into a single polyline (start at an endpoint if open).</summary>
    static List<Vector3> OrderPath(Selection sel)
    {
        var adj = new Dictionary<Vertex, List<Vertex>>();
        void Link(Vertex x, Vertex y) { (adj.TryGetValue(x, out var l) ? l : adj[x] = []).Add(y); }
        foreach (var e in sel.Edges) { Link(e.A, e.B); Link(e.B, e.A); }
        if (adj.Count == 0) return [];

        Vertex start = adj.FirstOrDefault(kv => kv.Value.Count == 1).Key ?? adj.Keys.First();
        var order = new List<Vertex>();
        var visited = new HashSet<Vertex>();
        Vertex? cur = start, prev = null;
        while (cur is not null && visited.Add(cur))
        {
            order.Add(cur);
            Vertex? next = null;
            foreach (var nb in adj[cur]) if (!ReferenceEquals(nb, prev) && !visited.Contains(nb)) { next = nb; break; }
            prev = cur; cur = next;
        }

        var pathPts = order.Select(v => v.P).ToList();
        // Closed loop: the last vertex connects back to the start → repeat it so the sweep revolves/closes.
        if (order.Count > 2 && adj[order[^1]].Any(v => ReferenceEquals(v, order[0])))
            pathPts.Add(order[0].P);
        return pathPts;
    }
}
