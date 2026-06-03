using System.Numerics;
using MintPlayer.ThreeDee.Geometry;

namespace MintPlayer.ThreeDee.Commands;

/// <summary>
/// Add a single wire edge between two world points, welding each endpoint to an existing vertex
/// within tolerance (so a chain shares vertices and connects to existing geometry). Undo removes
/// the wire and any vertex this command created that is no longer referenced.
/// </summary>
public sealed class AddEdgeCommand(Mesh mesh, Vector3 from, Vector3 to) : ICommand
{
    const float Eps = 1e-4f;

    Vertex? _a, _b;
    bool _createdA, _createdB;

    public string Name => "Draw Edge";

    public void Do()
    {
        _a = mesh.FindOrAddVertex(from, Eps, out _createdA);
        _b = mesh.FindOrAddVertex(to, Eps, out _createdB);
        mesh.AddWire(_a, _b);
    }

    public void Undo()
    {
        if (_a is null || _b is null) return;
        mesh.RemoveWire(_a, _b);
        if (_createdB && !mesh.IsVertexUsed(_b)) mesh.Vertices.Remove(_b);
        if (_createdA && !mesh.IsVertexUsed(_a)) mesh.Vertices.Remove(_a);
    }
}
