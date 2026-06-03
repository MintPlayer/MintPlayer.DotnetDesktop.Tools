using System.Numerics;
using MintPlayer.ThreeDee.Geometry;
using MintPlayer.ThreeDee.Rendering;

namespace MintPlayer.ThreeDee;

/// <summary>
/// The document model: a single global half-edge <see cref="Mesh"/> (the open-questions answer
/// in ARCHITECTURE.md §5). Emits the renderer's inputs — one <see cref="RenderMesh"/> per face
/// (fan-triangulated, with its boundary ring as feature edges) — keeping the renderer dumb.
/// </summary>
public sealed class Scene
{
    public Mesh Mesh { get; } = new();

    public BoundingBox Bounds() => Mesh.Bounds();

    /// <summary>Standalone wire edges as depth-tested world lines (drawn after the grid so they win at y=0).</summary>
    public IEnumerable<WorldLine> WireLines()
    {
        const int edgeArgb = unchecked((int)0xFFD0_D4D8);
        foreach (var (a, b) in Mesh.Wires)
            yield return new WorldLine(a.P, b.P, edgeArgb);
    }

    /// <summary>Fan-triangulate every face into a render mesh carrying that face's color + edges.</summary>
    public List<RenderMesh> BuildRenderMeshes()
    {
        var result = new List<RenderMesh>(Mesh.Faces.Count);
        foreach (var face in Mesh.Faces)
        {
            var verts = face.Vertices().Select(v => v.P).ToArray();
            int n = verts.Length;
            if (n < 3) continue;

            var tris = new int[(n - 2) * 3];
            for (int i = 0, t = 0; i < n - 2; i++)
            {
                tris[t++] = 0; tris[t++] = i + 1; tris[t++] = i + 2; // convex fan
            }

            var edges = new int[n * 2];
            for (int i = 0, e = 0; i < n; i++) { edges[e++] = i; edges[e++] = (i + 1) % n; }

            result.Add(new RenderMesh { Vertices = verts, Triangles = tris, Edges = edges, Color = face.Color });
        }
        return result;
    }
}
