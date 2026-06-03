namespace MintPlayer.ThreeDee.Geometry;

/// <summary>Records what an <see cref="Mesh.Extrude"/> created, so it can be precisely undone.</summary>
public sealed class ExtrudeResult
{
    public required Face Source { get; init; }
    public required Face Cap { get; init; }
    public required List<Face> Walls { get; init; }
    public required List<Vertex> CapVertices { get; init; }
}
