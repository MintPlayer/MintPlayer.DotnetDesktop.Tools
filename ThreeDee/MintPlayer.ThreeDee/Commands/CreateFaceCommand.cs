using System.Numerics;
using MintPlayer.ThreeDee.Geometry;

namespace MintPlayer.ThreeDee.Commands;

/// <summary>
/// Create a single planar face from a ring of world points (the result of the Line/Rectangle
/// tools). The ring is wound so the face normal matches <paramref name="intendedNormal"/>
/// (the work-plane normal), so the new face is front-facing toward the viewer.
///
/// M4 scope: the face is a standalone component (fresh vertices). Merging into / splitting
/// existing geometry is deferred (it needs the edge-dissolve / face-split ops noted in S5).
/// </summary>
public sealed class CreateFaceCommand(Mesh mesh, IReadOnlyList<Vector3> points, Vector3 color, Vector3 intendedNormal) : ICommand
{
    const float Eps = 1e-4f;

    Vertex[] _verts = [];
    bool[] _created = [];
    Face? _face;

    public string Name => "Create Face";

    public void Do()
    {
        var ring = points.ToList();
        if (Vector3.Dot(PolygonNormal(ring), intendedNormal) < 0) ring.Reverse();

        // Weld to existing vertices so closing a drawn loop reuses its wire endpoints.
        _verts = new Vertex[ring.Count];
        _created = new bool[ring.Count];
        for (int i = 0; i < ring.Count; i++) _verts[i] = mesh.FindOrAddVertex(ring[i], Eps, out _created[i]);

        _face = mesh.AddFace(_verts, color);
        mesh.Twin();
    }

    public void Undo()
    {
        if (_face is not null) mesh.RemoveFace(_face);
        for (int i = _verts.Length - 1; i >= 0; i--)
            if (_created[i] && !mesh.IsVertexUsed(_verts[i])) mesh.Vertices.Remove(_verts[i]);
        mesh.Twin();
        _face = null;
    }

    /// <summary>Newell's method over raw points (no topology required).</summary>
    static Vector3 PolygonNormal(IReadOnlyList<Vector3> pts)
    {
        Vector3 n = Vector3.Zero;
        for (int i = 0; i < pts.Count; i++)
        {
            Vector3 a = pts[i], b = pts[(i + 1) % pts.Count];
            n.X += (a.Y - b.Y) * (a.Z + b.Z);
            n.Y += (a.Z - b.Z) * (a.X + b.X);
            n.Z += (a.X - b.X) * (a.Y + b.Y);
        }
        return n.LengthSquared() > 1e-18f ? Vector3.Normalize(n) : Vector3.UnitY;
    }
}
