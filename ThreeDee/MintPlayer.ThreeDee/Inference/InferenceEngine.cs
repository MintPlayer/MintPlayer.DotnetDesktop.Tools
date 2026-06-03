using System.Numerics;
using MintPlayer.ThreeDee.Geometry;

namespace MintPlayer.ThreeDee.Inference;

/// <summary>
/// SketchUp-style snapping (validated by spike S6), adapted to the half-edge mesh. Given a cursor
/// ray it returns the best snap in priority order. Two modes:
///  • hover/first-point (no work plane): Endpoint &gt; Midpoint &gt; OnEdge &gt; OnFace &gt; Free.
///  • drawing on a locked work plane: Endpoint &gt; Midpoint &gt; OnEdge &gt; AxisLock &gt; OnPlane.
/// Tolerances are world-space (the caller back-projects a pixel radius for them).
/// </summary>
public sealed class InferenceEngine
{
    public float AxisLockDegrees = 4f;

    public InferenceResult Infer(Mesh mesh, Ray ray, Vector3? fromPoint, float pointTol,
        (Vector3 Point, Vector3 Normal)? plane, bool drawing)
    {
        float edgeTol = pointTol * 0.7f;

        // 1) Endpoint — nearest vertex to the ray.
        {
            float best = float.MaxValue; Vector3 bp = default;
            foreach (var v in mesh.Vertices)
            {
                float d = PointRayDistance(v.P, ray);
                if (d < best) { best = d; bp = v.P; }
            }
            if (best <= pointTol) return new InferenceResult(SnapType.Endpoint, bp, "Endpoint");
        }

        // 2) Midpoint — nearest edge midpoint.
        {
            float best = float.MaxValue; Vector3 bp = default;
            foreach (var (a, b) in mesh.UniqueEdges())
            {
                Vector3 m = (a.P + b.P) * 0.5f;
                float d = PointRayDistance(m, ray);
                if (d < best) { best = d; bp = m; }
            }
            if (best <= pointTol) return new InferenceResult(SnapType.Midpoint, bp, "Midpoint");
        }

        // 3) On-edge — nearest point along an edge.
        {
            float best = float.MaxValue; Vector3 bp = default;
            foreach (var (a, b) in mesh.UniqueEdges())
            {
                float d = RaySegmentDistance(ray, a.P, b.P, out Vector3 onSeg);
                if (d < best) { best = d; bp = onSeg; }
            }
            if (best <= edgeTol) return new InferenceResult(SnapType.OnEdge, bp, "On Edge");
        }

        if (!drawing)
        {
            // 4) On-face — ray crosses a face inside its polygon (nearest).
            float bestT = float.MaxValue; Vector3 bp = default; Vector3 bn = default; bool hit = false;
            foreach (var f in mesh.Faces)
            {
                Vector3 n = f.Normal();
                if (RayPlane(ray, n, f.Centroid(), out Vector3 p, out float t) && t < bestT && PointInFace(p, f, n))
                { bestT = t; bp = p; bn = n; hit = true; }
            }
            if (hit) return new InferenceResult(SnapType.OnFace, bp, "On Face", normal: bn);

            // Free: ground/work plane if given, else a point one unit down the ray.
            if (plane is { } pl && ray.IntersectPlane(pl.Point, pl.Normal, out Vector3 gp))
                return new InferenceResult(SnapType.OnPlane, gp, "On Plane", normal: pl.Normal);
            return new InferenceResult(SnapType.Free, ray.At(1f), "");
        }

        // Drawing on a locked plane: resolve the plane point, then try axis lock from the reference.
        if (plane is { } wp && ray.IntersectPlane(wp.Point, wp.Normal, out Vector3 planePt))
        {
            if (fromPoint is Vector3 origin)
            {
                Vector3 dir = planePt - origin;
                if (dir.LengthSquared() > 1e-9f)
                {
                    dir = Vector3.Normalize(dir);
                    Span<Vector3> axes = [Vector3.UnitX, -Vector3.UnitX, Vector3.UnitY, -Vector3.UnitY, Vector3.UnitZ, -Vector3.UnitZ];
                    float bestAng = float.MaxValue; Vector3 bestAxis = default;
                    foreach (var ax in axes)
                    {
                        float ang = MathF.Acos(Math.Clamp(Vector3.Dot(dir, ax), -1f, 1f)) * 180f / MathF.PI;
                        if (ang < bestAng) { bestAng = ang; bestAxis = ax; }
                    }
                    if (bestAng <= AxisLockDegrees)
                    {
                        float len = Vector3.Dot(planePt - origin, bestAxis);
                        return new InferenceResult(SnapType.AxisLock, origin + bestAxis * len, "On Axis", direction: bestAxis);
                    }
                }
            }
            return new InferenceResult(SnapType.OnPlane, planePt, "On Plane", normal: wp.Normal);
        }

        return new InferenceResult(SnapType.Free, ray.At(1f), "");
    }

    // ---- geometry helpers (from spike S6) ----

    static float PointRayDistance(Vector3 p, Ray r)
    {
        float t = MathF.Max(0f, Vector3.Dot(p - r.Origin, r.Direction));
        return Vector3.Distance(p, r.Origin + r.Direction * t);
    }

    static float RaySegmentDistance(Ray r, Vector3 a, Vector3 b, out Vector3 onSeg)
    {
        Vector3 d2 = b - a, ro = r.Origin - a;
        float E = Vector3.Dot(d2, d2);
        float sc;
        if (E <= 1e-12f) sc = 0f;
        else
        {
            float B = Vector3.Dot(r.Direction, d2), C = Vector3.Dot(r.Direction, ro), F = Vector3.Dot(d2, ro);
            float denom = E - B * B;
            float t = MathF.Abs(denom) > 1e-12f ? MathF.Max(0f, (B * F - C * E) / denom) : 0f;
            sc = Math.Clamp((B * t + F) / E, 0f, 1f);
        }
        onSeg = a + d2 * sc;
        float tt = MathF.Max(0f, Vector3.Dot(onSeg - r.Origin, r.Direction));
        return Vector3.Distance(r.Origin + r.Direction * tt, onSeg);
    }

    static bool RayPlane(Ray r, Vector3 n, Vector3 pointOnPlane, out Vector3 hit, out float t)
    {
        hit = default; t = 0f;
        float denom = Vector3.Dot(n, r.Direction);
        if (MathF.Abs(denom) < 1e-9f) return false;
        t = Vector3.Dot(n, pointOnPlane - r.Origin) / denom;
        if (t < 0f) return false;
        hit = r.At(t);
        return true;
    }

    static bool PointInFace(Vector3 p, Face f, Vector3 n)
    {
        var vs = f.Vertices().Select(v => v.P).ToArray();
        for (int i = 0; i < vs.Length; i++)
        {
            Vector3 a = vs[i], b = vs[(i + 1) % vs.Length];
            if (Vector3.Dot(Vector3.Cross(b - a, p - a), n) < -1e-4f) return false;
        }
        return true;
    }
}
