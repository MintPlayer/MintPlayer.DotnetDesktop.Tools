using System.Numerics;

namespace MintPlayer.ThreeDee.Geometry;

/// <summary>
/// Finds a fillable closed loop through a just-drawn edge (SketchUp auto-face behavior): the
/// smallest cycle of wire edges through (a,b) that is planar and doesn't already bound a face.
/// </summary>
public static class LoopFinder
{
    public static bool TryFindFillableLoop(Mesh mesh, Vertex a, Vertex b, out List<Vertex> loop, out Vector3 normal)
    {
        loop = []; normal = Vector3.UnitY;
        var cycle = MinimalCycle(mesh, a, b);
        if (cycle is null || cycle.Count < 3) return false;
        if (!IsPlanar(cycle, out normal)) return false;
        if (FaceAlreadyExists(mesh, cycle)) return false;
        loop = cycle;
        return true;
    }

    /// <summary>Shortest cycle through edge (a,b): BFS from b back to a over the wire graph minus that edge.</summary>
    static List<Vertex>? MinimalCycle(Mesh mesh, Vertex a, Vertex b)
    {
        var adj = new Dictionary<Vertex, List<Vertex>>();
        void Link(Vertex x, Vertex y) { (adj.TryGetValue(x, out var l) ? l : adj[x] = []).Add(y); }
        foreach (var (u, v) in mesh.Wires)
        {
            if ((u == a && v == b) || (u == b && v == a)) continue; // exclude the just-added edge
            Link(u, v); Link(v, u);
        }
        if (!adj.ContainsKey(b) || !adj.ContainsKey(a)) return null;

        var prev = new Dictionary<Vertex, Vertex>();
        var seen = new HashSet<Vertex> { b };
        var queue = new Queue<Vertex>();
        queue.Enqueue(b);
        while (queue.Count > 0)
        {
            var cur = queue.Dequeue();
            if (cur == a)
            {
                var path = new List<Vertex>();
                for (var v = a; ; v = prev[v]) { path.Add(v); if (v == b) break; }
                path.Reverse();                  // b → ... → a
                var cycle = new List<Vertex> { a };
                cycle.AddRange(path.Take(path.Count - 1)); // a, b, ... (drop trailing a)
                return cycle;
            }
            foreach (var nxt in adj[cur])
                if (seen.Add(nxt)) { prev[nxt] = cur; queue.Enqueue(nxt); }
        }
        return null;
    }

    static bool IsPlanar(List<Vertex> loop, out Vector3 normal)
    {
        normal = Vector3.Zero;
        for (int i = 0; i < loop.Count; i++)
        {
            Vector3 p = loop[i].P, q = loop[(i + 1) % loop.Count].P;
            normal.X += (p.Y - q.Y) * (p.Z + q.Z);
            normal.Y += (p.Z - q.Z) * (p.X + q.X);
            normal.Z += (p.X - q.X) * (p.Y + q.Y);
        }
        if (normal.LengthSquared() < 1e-12f) return false;
        normal = Vector3.Normalize(normal);

        // Deterministic orientation: make the dominant axis positive (ground loops face up, etc.).
        Vector3 m = new(MathF.Abs(normal.X), MathF.Abs(normal.Y), MathF.Abs(normal.Z));
        float dom = m.X >= m.Y && m.X >= m.Z ? normal.X : m.Y >= m.Z ? normal.Y : normal.Z;
        if (dom < 0) normal = -normal;

        Vector3 c = Vector3.Zero;
        foreach (var v in loop) c += v.P;
        c /= loop.Count;
        foreach (var v in loop)
            if (MathF.Abs(Vector3.Dot(v.P - c, normal)) > 1e-3f) return false;
        return true;
    }

    static bool FaceAlreadyExists(Mesh mesh, List<Vertex> loop)
    {
        var set = new HashSet<Vertex>(loop);
        foreach (var f in mesh.Faces)
        {
            var fset = new HashSet<Vertex>(f.Vertices());
            if (fset.Count == set.Count && fset.SetEquals(set)) return true;
        }
        return false;
    }
}
