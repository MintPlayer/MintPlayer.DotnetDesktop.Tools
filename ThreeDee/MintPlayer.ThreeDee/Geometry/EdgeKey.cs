using System.Runtime.CompilerServices;

namespace MintPlayer.ThreeDee.Geometry;

/// <summary>
/// Identity for an undirected edge: an unordered pair of vertices. Used as a stable key for
/// selection/hover regardless of which half-edge direction surfaced it.
/// </summary>
public readonly struct EdgeKey : IEquatable<EdgeKey>
{
    public readonly Vertex A;
    public readonly Vertex B;

    public EdgeKey(Vertex a, Vertex b)
    {
        if (RuntimeHelpers.GetHashCode(a) <= RuntimeHelpers.GetHashCode(b)) { A = a; B = b; }
        else { A = b; B = a; }
    }

    public bool Equals(EdgeKey o) => ReferenceEquals(A, o.A) && ReferenceEquals(B, o.B);
    public override bool Equals(object? o) => o is EdgeKey k && Equals(k);
    public override int GetHashCode() =>
        RuntimeHelpers.GetHashCode(A) * 397 ^ RuntimeHelpers.GetHashCode(B);
}
