using System.Numerics;

namespace MintPlayer.ThreeDee;

/// <summary>A ray in world space: origin + normalized direction. (Validated in spike S3/S4.)</summary>
public readonly struct Ray
{
    public readonly Vector3 Origin;
    public readonly Vector3 Direction; // normalized

    public Ray(Vector3 origin, Vector3 direction)
    {
        Origin = origin;
        Direction = Vector3.Normalize(direction);
    }

    /// <summary>Point at parameter t along the ray.</summary>
    public Vector3 At(float t) => Origin + Direction * t;

    /// <summary>Shortest distance from a world point to this (infinite) ray.</summary>
    public float DistanceToPoint(Vector3 p)
    {
        Vector3 w = p - Origin;
        float t = Vector3.Dot(w, Direction);
        return Vector3.Distance(Origin + Direction * t, p);
    }

    /// <summary>
    /// Intersect with the plane through <paramref name="planePoint"/> with the given
    /// (not necessarily unit) normal. Returns false if the ray is parallel to the plane
    /// or the hit is behind the origin.
    /// </summary>
    public bool IntersectPlane(Vector3 planePoint, Vector3 planeNormal, out Vector3 hit)
    {
        float denom = Vector3.Dot(Direction, planeNormal);
        if (MathF.Abs(denom) < 1e-9f) { hit = default; return false; }
        float t = Vector3.Dot(planePoint - Origin, planeNormal) / denom;
        if (t < 0) { hit = default; return false; }
        hit = Origin + Direction * t;
        return true;
    }
}
