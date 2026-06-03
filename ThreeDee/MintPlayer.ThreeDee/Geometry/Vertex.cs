using System.Numerics;

namespace MintPlayer.ThreeDee.Geometry;

/// <summary>A mesh vertex. <see cref="Out"/> is one outgoing (interior) half-edge.</summary>
public sealed class Vertex(Vector3 position)
{
    public Vector3 P = position;
    public HalfEdge? Out;
}
