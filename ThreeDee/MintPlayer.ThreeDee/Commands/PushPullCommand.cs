using System.Numerics;
using MintPlayer.ThreeDee.Geometry;

namespace MintPlayer.ThreeDee.Commands;

/// <summary>
/// Push/Pull a face along its normal by <paramref name="distance"/> (the headline feature).
///
/// Two behaviours, chosen by the face:
///  • A free/standalone face (a drawn rectangle or closed loop) is <b>extruded</b> — a cap and
///    side walls are created, turning the flat face into a solid (the validated S5 kernel).
///  • A face already part of a solid is <b>stretched</b> by translating its (shared) vertices along
///    the normal, so the adjacent side faces grow with it (e.g. making a box taller).
/// Both are precisely reversible.
/// </summary>
public sealed class PushPullCommand(Mesh mesh, Face face, float distance) : ICommand
{
    bool _extruded;
    ExtrudeResult? _extrude;          // extrude case
    Vertex[] _moved = [];             // stretch case
    Vector3 _delta;

    public string Name => "Push/Pull";

    public void Do()
    {
        _delta = face.Normal() * distance;
        _extruded = Mesh.IsStandalone(face);
        if (_extruded)
        {
            _extrude = mesh.Extrude(face, distance);
        }
        else
        {
            _moved = face.Vertices().ToArray();
            foreach (var v in _moved) v.P += _delta;
        }
    }

    public void Undo()
    {
        if (_extruded)
        {
            if (_extrude is not null) mesh.UnExtrude(_extrude);
        }
        else
        {
            foreach (var v in _moved) v.P -= _delta;
        }
    }
}
