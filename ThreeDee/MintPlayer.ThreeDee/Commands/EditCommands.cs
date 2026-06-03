using System.Numerics;
using MintPlayer.ThreeDee.Geometry;

namespace MintPlayer.ThreeDee.Commands;

/// <summary>Translate a fixed set of vertices by a delta (Move tool).</summary>
public sealed class MoveCommand(Vertex[] verts, Vector3 delta) : ICommand
{
    public string Name => "Move";
    public void Do() { foreach (var v in verts) v.P += delta; }
    public void Undo() { foreach (var v in verts) v.P -= delta; }
}

/// <summary>Rotate a fixed set of vertices about an axis through a center (Rotate tool).</summary>
public sealed class RotateCommand(Vertex[] verts, Vector3 center, Vector3 axis, float angleRad) : ICommand
{
    public string Name => "Rotate";
    public void Do() => Apply(angleRad);
    public void Undo() => Apply(-angleRad);

    void Apply(float a)
    {
        var q = Matrix4x4.CreateFromAxisAngle(Vector3.Normalize(axis), a);
        foreach (var v in verts) v.P = center + Vector3.Transform(v.P - center, q);
    }
}

/// <summary>Delete faces and wire edges (Eraser / Delete). Vertices are left in place.</summary>
public sealed class DeleteCommand(Mesh mesh, IReadOnlyList<Face> faces, IReadOnlyList<EdgeKey> wires) : ICommand
{
    (Vertex[] Ring, Vector3 Color)[]? _faceData;
    (Vertex, Vertex)[] _wirePairs = [];
    Face[] _live = [];

    public string Name => "Delete";

    public void Do()
    {
        if (_faceData is null)
        {
            _faceData = [.. faces.Select(f => (f.Vertices().ToArray(), f.Color))];
            _wirePairs = [.. wires.Select(w => (w.A, w.B))];
            _live = [.. faces];
        }
        foreach (var f in _live) mesh.RemoveFace(f);
        foreach (var (a, b) in _wirePairs) mesh.RemoveWire(a, b);
        mesh.Twin();
    }

    public void Undo()
    {
        _live = [.. _faceData!.Select(d => mesh.AddFace(d.Ring, d.Color))];
        foreach (var (a, b) in _wirePairs) mesh.AddWire(a, b);
        mesh.Twin();
    }
}

/// <summary>Add a welded wire polyline (Arc / Bézier output), optionally closed.</summary>
public sealed class AddPolylineCommand(Mesh mesh, IReadOnlyList<Vector3> points, bool closed) : ICommand
{
    const float Eps = 1e-4f;
    readonly List<Vertex> _created = [];
    readonly List<(Vertex A, Vertex B)> _wires = [];

    public string Name => "Draw Curve";

    public void Do()
    {
        _created.Clear(); _wires.Clear();
        var vs = new Vertex[points.Count];
        for (int i = 0; i < points.Count; i++)
        {
            vs[i] = mesh.FindOrAddVertex(points[i], Eps, out bool c);
            if (c) _created.Add(vs[i]);
        }
        for (int i = 0; i + 1 < vs.Length; i++) { mesh.AddWire(vs[i], vs[i + 1]); _wires.Add((vs[i], vs[i + 1])); }
        if (closed && vs.Length > 2) { mesh.AddWire(vs[^1], vs[0]); _wires.Add((vs[^1], vs[0])); }
    }

    public void Undo()
    {
        foreach (var (a, b) in _wires) mesh.RemoveWire(a, b);
        foreach (var v in _created) if (!mesh.IsVertexUsed(v)) mesh.Vertices.Remove(v);
    }
}
