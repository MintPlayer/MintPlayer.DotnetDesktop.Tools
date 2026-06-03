using MintPlayer.ThreeDee.Geometry;
using MintPlayer.ThreeDee.Rendering;

namespace MintPlayer.ThreeDee;

/// <summary>The current selection: sets of faces and edges (PRD §5.5).</summary>
public sealed class Selection
{
    public HashSet<Face> Faces { get; } = [];
    public HashSet<EdgeKey> Edges { get; } = [];

    public bool IsEmpty => Faces.Count == 0 && Edges.Count == 0;
    public int Count => Faces.Count + Edges.Count;

    public bool Contains(Face f) => Faces.Contains(f);
    public bool Contains(EdgeKey e) => Edges.Contains(e);

    /// <summary>Distinct vertices touched by the selection (for Move/Rotate).</summary>
    public Vertex[] Vertices()
    {
        var set = new HashSet<Vertex>();
        foreach (var f in Faces) foreach (var v in f.Vertices()) set.Add(v);
        foreach (var e in Edges) { set.Add(e.A); set.Add(e.B); }
        return [.. set];
    }

    public void Clear() { Faces.Clear(); Edges.Clear(); }

    /// <summary>Drop selected entities no longer in the mesh (after undo/redo restructures topology).</summary>
    public void Prune(Geometry.Mesh mesh)
    {
        var faces = new HashSet<Face>(mesh.Faces);
        var verts = new HashSet<Vertex>(mesh.Vertices);
        Faces.RemoveWhere(f => !faces.Contains(f));
        Edges.RemoveWhere(e => !verts.Contains(e.A) || !verts.Contains(e.B));
    }

    public void AddFace(Face f) => Faces.Add(f);
    public void AddEdge(EdgeKey e) => Edges.Add(e);

    public void ToggleFace(Face f) { if (!Faces.Remove(f)) Faces.Add(f); }
    public void ToggleEdge(EdgeKey e) { if (!Edges.Remove(e)) Edges.Add(e); }

    /// <summary>Apply a single pick result, honoring an additive/toggle modifier.</summary>
    public void Apply(PickResult pick, bool toggle)
    {
        if (!toggle) Clear();
        switch (pick.Kind)
        {
            case PickKind.Face: if (toggle) ToggleFace(pick.Face!); else AddFace(pick.Face!); break;
            case PickKind.Edge: if (toggle) ToggleEdge(pick.Edge); else AddEdge(pick.Edge); break;
        }
    }
}
