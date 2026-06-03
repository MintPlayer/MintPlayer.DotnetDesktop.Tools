using System.Numerics;

namespace MintPlayer.ThreeDee.Geometry;

/// <summary>Helpers that assemble primitive solids out of half-edge faces.</summary>
public static class MeshBuilder
{
    // Quad face rings (each CCW when viewed from outside), indexing the 8 box corners below.
    static readonly int[][] BoxFaces =
    [
        [1, 2, 6, 5], // +X
        [0, 4, 7, 3], // -X
        [3, 7, 6, 2], // +Y (top)
        [0, 1, 5, 4], // -Y (bottom)
        [4, 5, 6, 7], // +Z (front)
        [0, 3, 2, 1], // -Z (back)
    ];

    /// <summary>
    /// Add a closed box (8 shared vertices, 6 quad faces) to <paramref name="mesh"/>.
    /// The caller is responsible for calling <see cref="Mesh.Twin"/> once after adding all solids.
    /// </summary>
    public static Face[] Box(Mesh mesh, Vector3 center, Vector3 size, Vector3 color)
    {
        Vector3 m = center - size * 0.5f, x = center + size * 0.5f;
        var v = new[]
        {
            mesh.AddVertex(new Vector3(m.X, m.Y, m.Z)), mesh.AddVertex(new Vector3(x.X, m.Y, m.Z)),
            mesh.AddVertex(new Vector3(x.X, x.Y, m.Z)), mesh.AddVertex(new Vector3(m.X, x.Y, m.Z)),
            mesh.AddVertex(new Vector3(m.X, m.Y, x.Z)), mesh.AddVertex(new Vector3(x.X, m.Y, x.Z)),
            mesh.AddVertex(new Vector3(x.X, x.Y, x.Z)), mesh.AddVertex(new Vector3(m.X, x.Y, x.Z)),
        };
        var faces = new Face[BoxFaces.Length];
        for (int f = 0; f < BoxFaces.Length; f++)
        {
            var ring = BoxFaces[f].Select(i => v[i]).ToArray();
            faces[f] = mesh.AddFace(ring, color);
        }
        return faces;
    }
}
