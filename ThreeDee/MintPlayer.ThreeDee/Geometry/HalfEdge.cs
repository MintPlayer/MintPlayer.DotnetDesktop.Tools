namespace MintPlayer.ThreeDee.Geometry;

/// <summary>
/// A directed half-edge. <see cref="Twin"/> is the oppositely-directed half-edge across the
/// same undirected edge (null on a boundary). <see cref="Next"/> is the next half-edge around
/// <see cref="Face"/>'s loop. Fork F3 chose this representation (spike S5) because walking a
/// face loop and crossing to a neighbour are O(1) — the foundation for Push/Pull.
/// </summary>
public sealed class HalfEdge
{
    public Vertex Origin = null!;
    public HalfEdge? Twin;
    public HalfEdge Next = null!;
    public Face? Face;            // null => boundary half-edge

    public Vertex To => Next.Origin;
}
