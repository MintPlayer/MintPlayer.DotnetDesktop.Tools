using System.Numerics;
using MintPlayer.ThreeDee.Geometry;

namespace MintPlayer.ThreeDee.Rendering;

public enum PickKind { None, Face, Edge }

/// <summary>What the cursor ray hit: a face, an edge, or nothing — with the world hit point.</summary>
public readonly struct PickResult
{
    public readonly PickKind Kind;
    public readonly Face? Face;
    public readonly EdgeKey Edge;
    public readonly Vector3 Point;
    public readonly float T; // distance along the ray

    PickResult(PickKind kind, Face? face, EdgeKey edge, Vector3 point, float t)
    {
        Kind = kind; Face = face; Edge = edge; Point = point; T = t;
    }

    public static readonly PickResult None = new(PickKind.None, null, default, default, 0);
    public static PickResult OnFace(Face f, Vector3 p, float t) => new(PickKind.Face, f, default, p, t);
    public static PickResult OnEdge(EdgeKey e, Vector3 p, float t) => new(PickKind.Edge, null, e, p, t);
}

/// <summary>
/// CPU ray-cast picker (fork F4, validated by spike S4): Möller–Trumbore ray-triangle for faces
/// and ray-segment closest-approach for edges. Edges win when the cursor is within a back-projected
/// pixel radius and the edge isn't significantly behind the nearest face (edges sit on faces).
/// Brute force — S4 measured 0.15 ms over 10k tris, so no spatial index is needed yet.
/// </summary>
public sealed class Picker(Mesh mesh)
{
    const float Eps = 1e-7f;
    const float DepthSlack = 1e-2f; // edge may be this far behind the front face and still win

    public PickResult Pick(Ray ray, Camera cam, float pixelRadius = 6f)
    {
        Face? face = PickFace(ray, out Vector3 fPoint, out float fT);
        bool edge = PickEdge(ray, cam, pixelRadius, out EdgeKey eKey, out Vector3 ePoint, out float eT);

        if (edge && (face is null || eT <= fT + DepthSlack)) return PickResult.OnEdge(eKey, ePoint, eT);
        if (face is not null) return PickResult.OnFace(face, fPoint, fT);
        return PickResult.None;
    }

    /// <summary>Nearest face along the ray (back faces included — a modeler must click faces seen from behind).</summary>
    public Face? PickFace(Ray ray, out Vector3 point, out float t)
    {
        Face? best = null;
        float bestT = float.PositiveInfinity;
        foreach (var f in mesh.Faces)
        {
            var v = f.Vertices().Select(x => x.P).ToArray();
            for (int i = 1; i + 1 < v.Length; i++) // convex fan
                if (RayTriangle(ray, v[0], v[i], v[i + 1], out float ht) && ht < bestT)
                {
                    bestT = ht; best = f;
                }
        }
        t = bestT;
        point = best is null ? default : ray.At(bestT);
        return best;
    }

    bool PickEdge(Ray ray, Camera cam, float pixelRadius, out EdgeKey edge, out Vector3 point, out float t)
    {
        edge = default; point = default; t = float.PositiveInfinity;
        bool found = false;
        foreach (var (a, b) in mesh.UniqueEdges())
        {
            float dist = RaySegmentDistance(ray, a.P, b.P, out float tRay);
            float worldTol = pixelRadius * cam.WorldPerPixelAt(MathF.Max(tRay, 0f));
            if (dist <= worldTol && tRay < t)
            {
                t = tRay; edge = new EdgeKey(a, b); point = ray.At(tRay); found = true;
            }
        }
        return found;
    }

    // ---- intersection kernels (lifted verbatim from spike S4) ----

    static bool RayTriangle(in Ray ray, Vector3 a, Vector3 b, Vector3 c, out float t)
    {
        t = 0f;
        Vector3 e1 = b - a, e2 = c - a;
        Vector3 pvec = Vector3.Cross(ray.Direction, e2);
        float det = Vector3.Dot(e1, pvec);
        if (MathF.Abs(det) < Eps) return false; // parallel
        float invDet = 1f / det;
        Vector3 tvec = ray.Origin - a;
        float u = Vector3.Dot(tvec, pvec) * invDet;
        if (u < 0f || u > 1f) return false;
        Vector3 qvec = Vector3.Cross(tvec, e1);
        float v = Vector3.Dot(ray.Direction, qvec) * invDet;
        if (v < 0f || u + v > 1f) return false;
        float tt = Vector3.Dot(e2, qvec) * invDet;
        if (tt <= Eps) return false; // behind the origin
        t = tt;
        return true;
    }

    static float RaySegmentDistance(in Ray ray, Vector3 p0, Vector3 p1, out float tRay)
    {
        Vector3 u = ray.Direction, v = p1 - p0, w0 = ray.Origin - p0;
        float b = Vector3.Dot(u, v), c = Vector3.Dot(v, v), d = Vector3.Dot(u, w0), e = Vector3.Dot(v, w0);
        float denom = c - b * b; // a = 1 (u normalized)

        float s = denom < Eps ? (c > Eps ? e / c : 0f) : (e - b * d) / denom;
        s = Math.Clamp(s, 0f, 1f);
        float t = b * s - d; // ray param for that s
        if (t < 0f)
        {
            t = 0f;
            s = c > Eps ? Math.Clamp(Vector3.Dot(v, ray.Origin - p0) / c, 0f, 1f) : 0f;
        }
        tRay = t;
        return Vector3.Distance(ray.Origin + t * u, p0 + s * v);
    }
}
