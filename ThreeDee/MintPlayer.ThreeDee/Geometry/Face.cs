using System.Numerics;

namespace MintPlayer.ThreeDee.Geometry;

/// <summary>A planar polygon face, defined by its CCW (outward) half-edge loop.</summary>
public sealed class Face
{
    public HalfEdge Edge = null!;

    /// <summary>Flat base color, components 0..255 (no textures — PRD non-goal).</summary>
    public Vector3 Color = new(235, 235, 238);

    /// <summary>Walk the half-edges around this face, starting at <see cref="Edge"/>.</summary>
    public IEnumerable<HalfEdge> Loop()
    {
        var h = Edge;
        do { yield return h; h = h.Next; } while (h != Edge);
    }

    /// <summary>Vertices around the loop, in order.</summary>
    public IEnumerable<Vertex> Vertices()
    {
        foreach (var h in Loop()) yield return h.Origin;
    }

    public int SideCount()
    {
        int n = 0;
        foreach (var _ in Loop()) n++;
        return n;
    }

    public Vector3 Centroid()
    {
        Vector3 c = Vector3.Zero; int n = 0;
        foreach (var h in Loop()) { c += h.Origin.P; n++; }
        return n == 0 ? c : c / n;
    }

    /// <summary>Newell's method: a robust normal for any planar polygon (≥3 sides, convex or not).</summary>
    public Vector3 Normal()
    {
        Vector3 n = Vector3.Zero;
        foreach (var h in Loop())
        {
            Vector3 a = h.Origin.P, b = h.To.P;
            n.X += (a.Y - b.Y) * (a.Z + b.Z);
            n.Y += (a.Z - b.Z) * (a.X + b.X);
            n.Z += (a.X - b.X) * (a.Y + b.Y);
        }
        return n.LengthSquared() > 1e-18f ? Vector3.Normalize(n) : n;
    }
}
