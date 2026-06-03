using System.Globalization;
using System.Numerics;
using System.Text;
using MintPlayer.ThreeDee.Geometry;

namespace MintPlayer.ThreeDee;

/// <summary>Exports the scene's faces to common interchange formats (stretch goal §8). BCL-only.</summary>
public static class MeshExport
{
    static readonly CultureInfo Inv = CultureInfo.InvariantCulture;
    static string F(float x) => x.ToString("0.######", Inv);

    /// <summary>Wavefront OBJ. Faces are written as polygons (OBJ supports n-gons natively).</summary>
    public static string ToObj(Scene scene)
    {
        var mesh = scene.Mesh;
        var index = new Dictionary<Vertex, int>(mesh.Vertices.Count);
        var sb = new StringBuilder();
        sb.AppendLine("# Exported by ThreeDee");

        for (int i = 0; i < mesh.Vertices.Count; i++)
        {
            var p = mesh.Vertices[i].P;
            index[mesh.Vertices[i]] = i + 1; // OBJ is 1-based
            sb.Append("v ").Append(F(p.X)).Append(' ').Append(F(p.Y)).Append(' ').Append(F(p.Z)).Append('\n');
        }
        foreach (var face in mesh.Faces)
        {
            sb.Append('f');
            foreach (var v in face.Vertices()) sb.Append(' ').Append(index[v]);
            sb.Append('\n');
        }
        return sb.ToString();
    }

    /// <summary>ASCII STL — every face fan-triangulated, with its outward normal.</summary>
    public static string ToStl(Scene scene)
    {
        var sb = new StringBuilder();
        sb.Append("solid ThreeDee\n");
        foreach (var face in scene.Mesh.Faces)
        {
            var v = face.Vertices().Select(x => x.P).ToArray();
            if (v.Length < 3) continue;
            Vector3 n = face.Normal();
            for (int i = 1; i + 1 < v.Length; i++)
                Facet(sb, n, v[0], v[i], v[i + 1]);
        }
        sb.Append("endsolid ThreeDee\n");
        return sb.ToString();
    }

    static void Facet(StringBuilder sb, Vector3 n, Vector3 a, Vector3 b, Vector3 c)
    {
        sb.Append("  facet normal ").Append(F(n.X)).Append(' ').Append(F(n.Y)).Append(' ').Append(F(n.Z)).Append('\n');
        sb.Append("    outer loop\n");
        Vert(sb, a); Vert(sb, b); Vert(sb, c);
        sb.Append("    endloop\n  endfacet\n");
    }

    static void Vert(StringBuilder sb, Vector3 p) =>
        sb.Append("      vertex ").Append(F(p.X)).Append(' ').Append(F(p.Y)).Append(' ').Append(F(p.Z)).Append('\n');
}
