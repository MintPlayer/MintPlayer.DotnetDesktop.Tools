using System.Numerics;
using System.Text.Json;
using MintPlayer.ThreeDee.Geometry;
using MintPlayer.ThreeDee.Rendering;

namespace MintPlayer.ThreeDee;

/// <summary>
/// Native scene serialization (PRD §5.7) — JSON via System.Text.Json, no third-party libs.
/// The document is fully described by vertex positions, faces (vertex-index loops + color),
/// wire edges (index pairs), and the camera, so save/load is a plain round-trip.
/// </summary>
public static class SceneIO
{
    public const string Extension = ".3dee";

    sealed record Doc(float[][] Vertices, FaceDto[] Faces, int[][] Wires, CamDto? Camera);
    sealed record FaceDto(int[] Indices, float[] Color);
    sealed record CamDto(float Azimuth, float Elevation, float Distance, float[] Target, bool Orthographic);

    static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    public static string Save(Scene scene, Camera cam)
    {
        var mesh = scene.Mesh;
        var index = new Dictionary<Vertex, int>(mesh.Vertices.Count);
        for (int i = 0; i < mesh.Vertices.Count; i++) index[mesh.Vertices[i]] = i;

        var v = mesh.Vertices.Select(x => new[] { x.P.X, x.P.Y, x.P.Z }).ToArray();
        var f = mesh.Faces.Select(face =>
            new FaceDto([.. face.Vertices().Select(x => index[x])], [face.Color.X, face.Color.Y, face.Color.Z])).ToArray();
        var w = mesh.Wires.Select(e => new[] { index[e.A], index[e.B] }).ToArray();
        var camDto = new CamDto(cam.AzimuthDeg, cam.ElevationDeg, cam.Distance, [cam.Target.X, cam.Target.Y, cam.Target.Z], cam.Orthographic);

        return JsonSerializer.Serialize(new Doc(v, f, w, camDto), Options);
    }

    public static void Load(string json, Scene scene, Camera cam)
    {
        var doc = JsonSerializer.Deserialize<Doc>(json) ?? throw new InvalidDataException("Empty scene document.");
        var mesh = scene.Mesh;
        mesh.Clear();

        var verts = doc.Vertices.Select(a => mesh.AddVertex(new Vector3(a[0], a[1], a[2]))).ToArray();
        foreach (var f in doc.Faces)
            mesh.AddFace([.. f.Indices.Select(i => verts[i])], new Vector3(f.Color[0], f.Color[1], f.Color[2]));
        foreach (var e in doc.Wires)
            mesh.AddWire(verts[e[0]], verts[e[1]]);
        mesh.Twin();

        if (doc.Camera is { } c)
        {
            cam.AzimuthDeg = c.Azimuth;
            cam.ElevationDeg = c.Elevation;
            cam.Distance = c.Distance;
            cam.Target = new Vector3(c.Target[0], c.Target[1], c.Target[2]);
            cam.Orthographic = c.Orthographic;
        }
    }
}
