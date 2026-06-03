using System.Numerics;
using MintPlayer.ThreeDee.Geometry;

namespace MintPlayer.ThreeDee.Commands;

/// <summary>
/// Follow-Me: sweep a profile face along a path polyline, building a solid (the SketchUp lathe/sweep).
/// At each path station a cross-section ring is placed perpendicular to the path tangent, oriented by
/// a parallel-transport frame (minimizes twist); rings are stitched with side quads and the open ends
/// are capped. The original profile face is left in place. Undoable.
/// </summary>
public sealed class FollowMeCommand(Mesh mesh, Face profile, IReadOnlyList<Vector3> path) : ICommand
{
    readonly List<Vertex> _verts = [];
    readonly List<Face> _faces = [];

    public string Name => "Follow Me";

    public void Do()
    {
        _verts.Clear(); _faces.Clear();

        var prof = profile.Vertices().Select(v => v.P).ToArray();
        int n = prof.Length;
        if (n < 3) return;
        Vector3 normal = profile.Normal();

        var pts = path.ToList();
        bool closed = pts.Count > 2 && Vector3.Distance(pts[0], pts[^1]) < 1e-4f;
        if (closed) pts.RemoveAt(pts.Count - 1);
        int m = pts.Count;
        if (m < 2) return;

        Vector3 Tangent(int i)
        {
            Vector3 t = closed ? pts[(i + 1) % m] - pts[(i - 1 + m) % m]
                : i == 0 ? pts[1] - pts[0] : i == m - 1 ? pts[m - 1] - pts[m - 2] : pts[i + 1] - pts[i - 1];
            return Vector3.Normalize(t);
        }

        var ring = new Vertex[m][];
        for (int i = 0; i < m; i++) ring[i] = new Vertex[n];

        if (closed && TryPathPlane(pts, out Vector3 center, out Vector3 axis))
        {
            // Exact surface of revolution about the path's axis (PRD R11.1-2): perfect sphere/lathe.
            Vector3 r0 = Vector3.Normalize(pts[0] - center);
            Vector3 r0perp = Vector3.Normalize(Vector3.Cross(axis, r0));
            for (int i = 0; i < m; i++)
            {
                Vector3 ri = pts[i] - center;
                float ang = MathF.Atan2(Vector3.Dot(ri, r0perp), Vector3.Dot(ri, r0));
                var rot = Quaternion.CreateFromAxisAngle(axis, ang);
                for (int j = 0; j < n; j++)
                {
                    ring[i][j] = mesh.AddVertex(center + Vector3.Transform(prof[j] - center, rot));
                    _verts.Add(ring[i][j]);
                }
            }
        }
        else
        {
            // Moving-frame (parallel-transport) sweep (PRD R11.1-1): preserves the profile's offset
            // from the path. The WHOLE frame (T,U,V) is transported, so the radial offset is carried.
            Vector3 t0 = Tangent(0);
            Tessellation.PlaneBasis(normal, out Vector3 u0, out Vector3 v0);
            var qStart = ShortestArc(normal, t0);
            Vector3 startU = Vector3.Transform(u0, qStart), startV = Vector3.Transform(v0, qStart);

            var l1 = new float[n]; var l2 = new float[n]; var l3 = new float[n];
            for (int j = 0; j < n; j++)
            {
                Vector3 d = prof[j] - pts[0];
                l1[j] = Vector3.Dot(d, t0); l2[j] = Vector3.Dot(d, startU); l3[j] = Vector3.Dot(d, startV);
            }

            var T = new Vector3[m]; var U = new Vector3[m]; var V = new Vector3[m];
            T[0] = t0; U[0] = startU; V[0] = startV;
            for (int i = 1; i < m; i++)
            {
                var qi = ShortestArc(Tangent(i - 1), Tangent(i));
                T[i] = Vector3.Transform(T[i - 1], qi); U[i] = Vector3.Transform(U[i - 1], qi); V[i] = Vector3.Transform(V[i - 1], qi);
            }
            for (int i = 0; i < m; i++)
                for (int j = 0; j < n; j++)
                {
                    ring[i][j] = mesh.AddVertex(pts[i] + l1[j] * T[i] + l2[j] * U[i] + l3[j] * V[i]);
                    _verts.Add(ring[i][j]);
                }
        }

        int stations = closed ? m : m - 1;
        for (int i = 0; i < stations; i++)
        {
            int i2 = (i + 1) % m;
            for (int j = 0; j < n; j++)
            {
                int j2 = (j + 1) % n;
                _faces.Add(mesh.AddFace([ring[i][j], ring[i][j2], ring[i2][j2], ring[i2][j]], profile.Color));
            }
        }

        if (!closed)
        {
            _faces.Add(mesh.AddFace([.. ring[0].Reverse()], profile.Color)); // start cap faces back
            _faces.Add(mesh.AddFace(ring[m - 1], profile.Color));            // end cap faces forward
        }

        mesh.Twin();
    }

    public void Undo()
    {
        foreach (var f in _faces) mesh.RemoveFace(f);
        mesh.RemoveVertices(_verts);
        mesh.Twin();
    }

    /// <summary>If the path is (near-)planar, return its centroid + best-fit normal (revolution axis).</summary>
    static bool TryPathPlane(List<Vector3> pts, out Vector3 center, out Vector3 axis)
    {
        center = Vector3.Zero; axis = Vector3.UnitY;
        foreach (var p in pts) center += p;
        center /= pts.Count;

        Vector3 nrm = Vector3.Zero; // Newell
        for (int i = 0; i < pts.Count; i++)
        {
            Vector3 a = pts[i], b = pts[(i + 1) % pts.Count];
            nrm.X += (a.Y - b.Y) * (a.Z + b.Z);
            nrm.Y += (a.Z - b.Z) * (a.X + b.X);
            nrm.Z += (a.X - b.X) * (a.Y + b.Y);
        }
        if (nrm.LengthSquared() < 1e-12f) return false;
        axis = Vector3.Normalize(nrm);

        float maxDev = 0f, extent = 1e-6f;
        foreach (var p in pts)
        {
            maxDev = MathF.Max(maxDev, MathF.Abs(Vector3.Dot(p - center, axis)));
            extent = MathF.Max(extent, (p - center).Length());
        }
        return maxDev / extent < 1e-3f; // planar
    }

    static Quaternion ShortestArc(Vector3 from, Vector3 to)
    {
        from = Vector3.Normalize(from); to = Vector3.Normalize(to);
        float d = Vector3.Dot(from, to);
        if (d > 0.99999f) return Quaternion.Identity;
        if (d < -0.99999f)
        {
            Vector3 perp = MathF.Abs(from.X) < 0.9f ? Vector3.UnitX : Vector3.UnitY;
            return Quaternion.CreateFromAxisAngle(Vector3.Normalize(Vector3.Cross(from, perp)), MathF.PI);
        }
        return Quaternion.CreateFromAxisAngle(Vector3.Normalize(Vector3.Cross(from, to)), MathF.Acos(Math.Clamp(d, -1f, 1f)));
    }
}
