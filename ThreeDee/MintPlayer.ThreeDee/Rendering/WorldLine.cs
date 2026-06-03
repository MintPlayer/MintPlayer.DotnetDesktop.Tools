using System.Numerics;

namespace MintPlayer.ThreeDee.Rendering;

/// <summary>A depth-tested world-space line segment (used for the ground grid and axis tripod).</summary>
public readonly struct WorldLine(Vector3 a, Vector3 b, int colorArgb)
{
    public readonly Vector3 A = a;
    public readonly Vector3 B = b;
    public readonly int ColorArgb = colorArgb;
}
