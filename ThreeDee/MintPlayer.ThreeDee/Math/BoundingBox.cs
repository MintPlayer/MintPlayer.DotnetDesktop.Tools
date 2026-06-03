using System.Numerics;

namespace MintPlayer.ThreeDee;

/// <summary>Axis-aligned bounding box used for Zoom Extents and standard-view framing.</summary>
public struct BoundingBox
{
    public Vector3 Min;
    public Vector3 Max;
    public bool IsEmpty;

    public static BoundingBox Empty => new() { Min = new Vector3(float.MaxValue), Max = new Vector3(float.MinValue), IsEmpty = true };

    public void Add(Vector3 p)
    {
        Min = Vector3.Min(Min, p);
        Max = Vector3.Max(Max, p);
        IsEmpty = false;
    }

    public readonly Vector3 Center => (Min + Max) * 0.5f;

    /// <summary>Half the length of the space diagonal (the bounding-sphere radius).</summary>
    public readonly float Radius => IsEmpty ? 1f : (Max - Min).Length() * 0.5f;
}
