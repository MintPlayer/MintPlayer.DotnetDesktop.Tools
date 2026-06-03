using System.Numerics;

namespace MintPlayer.ThreeDee.Rendering;

/// <summary>
/// The renderer's input primitive: a triangle mesh in (or transformable into) world space,
/// plus an optional list of "feature" edges to draw in edged styles.
/// In M2 the half-edge <c>Scene</c> will emit these; for M1 they are built by hand.
/// </summary>
public sealed class RenderMesh
{
    /// <summary>Vertex positions in model space.</summary>
    public required Vector3[] Vertices { get; init; }

    /// <summary>Triangle list (every 3 indices = one triangle), wound CCW when viewed from outside.</summary>
    public required int[] Triangles { get; init; }

    /// <summary>
    /// Optional feature-edge list (index pairs) drawn in HiddenLine/ShadedEdges/Wireframe.
    /// When null, edges are derived from triangle edges (which exposes triangulation diagonals).
    /// </summary>
    public int[]? Edges { get; init; }

    /// <summary>Flat base color, components in 0..255.</summary>
    public Vector3 Color { get; init; } = new(180, 180, 185);

    /// <summary>Model-to-world transform (identity for M1 world-space meshes).</summary>
    public Matrix4x4 Model { get; init; } = Matrix4x4.Identity;

    /// <summary>Accumulate this mesh's world-space vertices into a bounding box.</summary>
    public void AddToBounds(ref BoundingBox box)
    {
        foreach (var v in Vertices)
            box.Add(Vector3.Transform(v, Model));
    }
}
