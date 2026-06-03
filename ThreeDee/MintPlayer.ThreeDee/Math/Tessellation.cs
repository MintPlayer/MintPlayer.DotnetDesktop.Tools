using System.Numerics;

namespace MintPlayer.ThreeDee;

/// <summary>Curve → polyline tessellation (validated by spike S7). BCL-only.</summary>
public static class Tessellation
{
    // ---- cubic Bézier (adaptive flattening within a chord tolerance) ----

    public static Vector3 BezierPoint(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float t)
    {
        float u = 1f - t;
        return u * u * u * p0 + 3f * u * u * t * p1 + 3f * u * t * t * p2 + t * t * t * p3;
    }

    public static List<Vector3> FlattenBezier(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float tol)
    {
        var pts = new List<Vector3> { p0 };
        Subdivide(p0, p1, p2, p3, MathF.Max(tol, 1e-5f), 0, pts);
        return pts;
    }

    static void Subdivide(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, float tol, int depth, List<Vector3> outPts)
    {
        if (depth >= 24 || (DistPointSeg(p1, p0, p3) <= tol && DistPointSeg(p2, p0, p3) <= tol))
        {
            outPts.Add(p3);
            return;
        }
        Vector3 p01 = 0.5f * (p0 + p1), p12 = 0.5f * (p1 + p2), p23 = 0.5f * (p2 + p3);
        Vector3 p012 = 0.5f * (p01 + p12), p123 = 0.5f * (p12 + p23), mid = 0.5f * (p012 + p123);
        Subdivide(p0, p01, p012, mid, tol, depth + 1, outPts);
        Subdivide(mid, p123, p23, p3, tol, depth + 1, outPts);
    }

    static float DistPointSeg(Vector3 p, Vector3 a, Vector3 b)
    {
        Vector3 ab = b - a;
        float len2 = ab.LengthSquared();
        if (len2 < 1e-20f) return Vector3.Distance(p, a);
        float t = Math.Clamp(Vector3.Dot(p - a, ab) / len2, 0f, 1f);
        return Vector3.Distance(p, a + t * ab);
    }

    // ---- circle / regular N-gon ----

    /// <summary>N vertices evenly spaced on a circle in the plane spanned by orthonormal (u, v).</summary>
    public static List<Vector3> Polygon(Vector3 center, Vector3 u, Vector3 v, float radius, int n)
    {
        var pts = new List<Vector3>(n);
        for (int i = 0; i < n; i++)
        {
            float a = MathF.Tau * i / n;
            pts.Add(center + radius * (MathF.Cos(a) * u + MathF.Sin(a) * v));
        }
        return pts;
    }

    // ---- arc ----

    public static List<Vector3> TessellateArc(Vector3 center, Vector3 u, Vector3 v, float radius, float startAngle, float sweep, int n)
    {
        var pts = new List<Vector3>(n + 1);
        for (int i = 0; i <= n; i++)
        {
            float a = startAngle + sweep * i / n;
            pts.Add(center + radius * (MathF.Cos(a) * u + MathF.Sin(a) * v));
        }
        return pts;
    }

    /// <summary>Arc through 3 points (a → b → c). Returns false if the points are collinear.</summary>
    public static bool Arc3Point(Vector3 a, Vector3 b, Vector3 c, int n, out List<Vector3> pts)
    {
        pts = [];
        Vector3 normal = Vector3.Cross(b - a, c - a);
        if (normal.Length() < 1e-9f) return false;
        normal = Vector3.Normalize(normal);

        Vector3 ab = b - a, ac = c - a;
        Vector3 cross = Vector3.Cross(ab, ac);
        float cross2 = cross.LengthSquared();
        if (cross2 < 1e-12f) return false;
        Vector3 toCenter = (Vector3.Cross(cross, ab) * ac.LengthSquared() + Vector3.Cross(ac, cross) * ab.LengthSquared()) / (2f * cross2);
        Vector3 center = a + toCenter;
        float radius = toCenter.Length();

        Vector3 u = Vector3.Normalize(a - center);
        Vector3 v = Vector3.Normalize(Vector3.Cross(normal, u));

        float Angle(Vector3 p) { Vector3 d = p - center; return MathF.Atan2(Vector3.Dot(d, v), Vector3.Dot(d, u)); }
        static float Norm2Pi(float x) { while (x < 0) x += MathF.Tau; while (x >= MathF.Tau) x -= MathF.Tau; return x; }

        float bRel = Norm2Pi(Angle(b)), cRel = Norm2Pi(Angle(c));
        float sweep = bRel < cRel ? cRel : cRel - MathF.Tau;
        pts = TessellateArc(center, u, v, radius, 0f, sweep, n);
        return true;
    }

    /// <summary>An in-plane orthonormal basis for a plane normal (matches the tools' convention).</summary>
    public static void PlaneBasis(Vector3 normal, out Vector3 u, out Vector3 v)
    {
        u = MathF.Abs(normal.Y) < 0.99f ? Vector3.Normalize(Vector3.Cross(Vector3.UnitY, normal)) : Vector3.UnitX;
        v = Vector3.Normalize(Vector3.Cross(normal, u));
    }
}
