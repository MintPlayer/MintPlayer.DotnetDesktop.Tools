using System.Numerics;

namespace MintPlayer.ThreeDee.Geometry;

/// <summary>
/// A half-edge mesh (fork F3, validated by spike S5). Holds a single global pool of vertices,
/// half-edges and faces — possibly several disconnected manifold components. Faces are added
/// from rings of shared vertices, then <see cref="Twin"/> stitches opposite half-edges by
/// geometry. Includes the validated Push/Pull <see cref="Extrude"/> op (wired up by a tool in M5).
/// </summary>
public sealed class Mesh
{
    const float Eps = 1e-4f;

    public List<Vertex> Vertices { get; } = [];
    public List<HalfEdge> HalfEdges { get; } = [];
    public List<Face> Faces { get; } = [];

    /// <summary>Standalone "wire" edges that do not (yet) bound a face — the Line tool's output.</summary>
    public List<(Vertex A, Vertex B)> Wires { get; } = [];

    public Vertex AddVertex(Vector3 p)
    {
        var v = new Vertex(p);
        Vertices.Add(v);
        return v;
    }

    public void Clear() { Vertices.Clear(); HalfEdges.Clear(); Faces.Clear(); Wires.Clear(); }

    /// <summary>Reuse an existing vertex within <paramref name="eps"/>, else add one (welds drawn geometry).</summary>
    public Vertex FindOrAddVertex(Vector3 p, float eps, out bool created)
    {
        float e2 = eps * eps;
        foreach (var v in Vertices)
            if (Vector3.DistanceSquared(v.P, p) <= e2) { created = false; return v; }
        created = true;
        return AddVertex(p);
    }

    public Vertex? FindVertex(Vector3 p, float eps)
    {
        float e2 = eps * eps;
        foreach (var v in Vertices) if (Vector3.DistanceSquared(v.P, p) <= e2) return v;
        return null;
    }

    public void AddWire(Vertex a, Vertex b) => Wires.Add((a, b));

    public void RemoveWire(Vertex a, Vertex b) =>
        Wires.RemoveAll(w => (w.A == a && w.B == b) || (w.A == b && w.B == a));

    /// <summary>Is a vertex still referenced by any half-edge or wire? (Guards undo vertex removal.)</summary>
    public bool IsVertexUsed(Vertex v) =>
        HalfEdges.Any(h => h.Origin == v) || Wires.Any(w => w.A == v || w.B == v);

    /// <summary>
    /// Add a face from an ordered ring of existing vertices (CCW when viewed from outside).
    /// Creates only the interior half-edges; call <see cref="Twin"/> once after all faces are
    /// added to stitch shared edges (unmatched edges remain boundary half-edges).
    /// </summary>
    public Face AddFace(IReadOnlyList<Vertex> ring, Vector3? color = null)
    {
        int n = ring.Count;
        var face = new Face();
        if (color is { } c) face.Color = c;
        Faces.Add(face);

        var he = new HalfEdge[n];
        for (int i = 0; i < n; i++)
        {
            he[i] = new HalfEdge { Origin = ring[i], Face = face };
            HalfEdges.Add(he[i]);
        }
        for (int i = 0; i < n; i++) he[i].Next = he[(i + 1) % n];
        face.Edge = he[0];
        for (int i = 0; i < n; i++) ring[i].Out ??= he[i];
        return face;
    }

    /// <summary>Remove a face and its half-edges (used by undo of face-creation).</summary>
    public void RemoveFace(Face f)
    {
        foreach (var h in f.Loop().ToList()) HalfEdges.Remove(h);
        Faces.Remove(f);
    }

    public void RemoveVertices(IEnumerable<Vertex> verts)
    {
        var set = verts as HashSet<Vertex> ?? [.. verts];
        Vertices.RemoveAll(set.Contains);
    }

    /// <summary>Build a single planar face from a ring of points (allocates fresh vertices).</summary>
    public Face MakeFace(IReadOnlyList<Vector3> pts, Vector3? color = null)
    {
        var ring = new Vertex[pts.Count];
        for (int i = 0; i < pts.Count; i++) ring[i] = AddVertex(pts[i]);
        var face = AddFace(ring, color);
        Twin();
        return face;
    }

    /// <summary>Stitch twins: for each directed edge (a→b) find its opposite (b→a).</summary>
    public void Twin()
    {
        var map = new Dictionary<(Vertex, Vertex), HalfEdge>(HalfEdges.Count);
        foreach (var h in HalfEdges) map[(h.Origin, h.To)] = h;
        foreach (var h in HalfEdges)
            h.Twin = map.TryGetValue((h.To, h.Origin), out var t) ? t : null;
    }

    /// <summary>
    /// Push/Pull: extrude <paramref name="face"/> along its outward normal by <paramref name="distance"/>.
    /// The source face is flipped in place (its old normal now points into the solid), a translated
    /// cap face is added, and one side-wall quad is stitched per boundary edge so all normals point
    /// outward. (Verbatim S5 algorithm — the headline feature's kernel.)
    /// </summary>
    public ExtrudeResult Extrude(Face face, float distance)
    {
        Vector3 dir = face.Normal() * distance;
        var loop = face.Loop().ToList();
        int n = loop.Count;
        var src = loop.Select(h => h.Origin).ToArray();
        var cap = src.Select(v => AddVertex(v.P + dir)).ToArray();

        // Any old boundary placeholders for this loop become interior; drop them.
        foreach (var h in loop)
            if (h.Twin is { Face: null } b) HalfEdges.Remove(b);

        ReverseFaceLoop(face); // flip source: its normal becomes -N (the near cap)

        var capFace = new Face { Color = face.Color };
        Faces.Add(capFace);
        var capHE = new HalfEdge[n];
        for (int i = 0; i < n; i++) { capHE[i] = new HalfEdge { Origin = cap[i], Face = capFace }; HalfEdges.Add(capHE[i]); }
        for (int i = 0; i < n; i++) capHE[i].Next = capHE[(i + 1) % n];
        capFace.Edge = capHE[0];

        var walls = new List<Face>(n);
        for (int i = 0; i < n; i++) // side walls: b[i] → b[i+1] → t[i+1] → t[i]
        {
            int j = (i + 1) % n;
            var sf = new Face { Color = face.Color };
            Faces.Add(sf);
            var e0 = new HalfEdge { Origin = src[i], Face = sf };
            var e1 = new HalfEdge { Origin = src[j], Face = sf };
            var e2 = new HalfEdge { Origin = cap[j], Face = sf };
            var e3 = new HalfEdge { Origin = cap[i], Face = sf };
            e0.Next = e1; e1.Next = e2; e2.Next = e3; e3.Next = e0;
            HalfEdges.AddRange([e0, e1, e2, e3]);
            sf.Edge = e0;
            walls.Add(sf);
        }

        Twin();
        RepointVertexOut();
        return new ExtrudeResult { Source = face, Cap = capFace, Walls = walls, CapVertices = [.. cap] };
    }

    /// <summary>Precisely reverse an <see cref="Extrude"/> (for undo).</summary>
    public void UnExtrude(ExtrudeResult r)
    {
        foreach (var w in r.Walls) RemoveFace(w);
        RemoveFace(r.Cap);
        RemoveVertices(r.CapVertices);
        ReverseFaceLoop(r.Source); // restore the source face's original orientation
        Twin();
        RepointVertexOut();
    }

    /// <summary>A face is "free" if none of its boundary edges is shared with another face.</summary>
    public static bool IsStandalone(Face face) =>
        face.Loop().All(h => h.Twin is null || h.Twin.Face is null);

    static void ReverseFaceLoop(Face face)
    {
        var hes = face.Loop().ToList();
        int n = hes.Count;
        var origins = hes.Select(h => h.Origin).ToArray();
        for (int i = 0; i < n; i++) hes[i].Origin = origins[(i + 1) % n];
        for (int i = 0; i < n; i++) hes[i].Next = hes[(i - 1 + n) % n];
    }

    void RepointVertexOut()
    {
        foreach (var v in Vertices) v.Out = null;
        foreach (var h in HalfEdges)
            if (h.Face != null) h.Origin.Out ??= h;
    }

    // ---- queries ----

    /// <summary>Each undirected edge once (face boundaries + wires, deduplicated), as an (a, b) pair.</summary>
    public IEnumerable<(Vertex A, Vertex B)> UniqueEdges()
    {
        var seen = new HashSet<EdgeKey>();
        foreach (var h in HalfEdges)
            if (seen.Add(new EdgeKey(h.Origin, h.To))) yield return (h.Origin, h.To);
        foreach (var w in Wires)
            if (seen.Add(new EdgeKey(w.A, w.B))) yield return (w.A, w.B);
    }

    /// <summary>
    /// All edges in the connected chain through <paramref name="seed"/> (a whole tessellated curve /
    /// edge path), for double-click selection. Traversal stops at junction vertices (incident degree ≠ 2)
    /// so it doesn't bleed across where a curve meets a face or another curve (SketchUp behavior).
    /// </summary>
    public IEnumerable<EdgeKey> ConnectedEdgePath(EdgeKey seed)
    {
        var inc = new Dictionary<Vertex, List<EdgeKey>>();
        void Add(Vertex v, EdgeKey k) => (inc.TryGetValue(v, out var l) ? l : inc[v] = []).Add(k);
        foreach (var (a, b) in UniqueEdges()) { var k = new EdgeKey(a, b); Add(a, k); Add(b, k); }

        var result = new HashSet<EdgeKey> { seed };
        var stack = new Stack<EdgeKey>();
        stack.Push(seed);
        while (stack.Count > 0)
        {
            var e = stack.Pop();
            foreach (var v in (Vertex[])[e.A, e.B])
            {
                if (!inc.TryGetValue(v, out var edges) || edges.Count != 2) continue; // endpoint/junction
                foreach (var nb in edges) if (result.Add(nb)) stack.Push(nb);
            }
        }
        return result;
    }

    public int EdgeCount()
    {
        var idx = new Dictionary<Vertex, int>();
        for (int i = 0; i < Vertices.Count; i++) idx[Vertices[i]] = i;
        var set = new HashSet<(int, int)>();
        foreach (var h in HalfEdges)
        {
            int a = idx[h.Origin], b = idx[h.To];
            set.Add(a < b ? (a, b) : (b, a));
        }
        return set.Count;
    }

    /// <summary>Boundary edges have no twin (faces are built without explicit boundary placeholders).</summary>
    public int BoundaryEdgeCount() => HalfEdges.Count(h => h.Twin == null);

    public bool IsClosed() => BoundaryEdgeCount() == 0;

    /// <summary>Manifold ⇔ no directed edge (a→b) is shared by more than one face.</summary>
    public bool IsManifold()
    {
        var seen = new HashSet<(Vertex, Vertex)>();
        foreach (var h in HalfEdges)
            if (!seen.Add((h.Origin, h.To))) return false;
        return true;
    }

    public Vector3 Center()
    {
        if (Vertices.Count == 0) return Vector3.Zero;
        Vector3 c = Vector3.Zero;
        foreach (var v in Vertices) c += v.P;
        return c / Vertices.Count;
    }

    /// <summary>Convex-solid sanity check: each face normal points away from the mesh centroid.</summary>
    public bool AllNormalsOutward(out int bad)
    {
        bad = 0;
        var center = Center();
        foreach (var f in Faces)
            if (Vector3.Dot(f.Normal(), f.Centroid() - center) <= Eps) bad++;
        return bad == 0;
    }

    public BoundingBox Bounds()
    {
        var box = BoundingBox.Empty;
        foreach (var v in Vertices) box.Add(v.P);
        return box;
    }
}
