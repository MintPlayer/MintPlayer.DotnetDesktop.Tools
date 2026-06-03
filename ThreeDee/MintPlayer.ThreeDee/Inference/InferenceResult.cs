using System.Numerics;

namespace MintPlayer.ThreeDee.Inference;

/// <summary>The kind of snap the inference engine found, in priority order.</summary>
public enum SnapType { Free, OnPlane, OnFace, OnEdge, Midpoint, Endpoint, AxisLock }

/// <summary>
/// A snap result (validated approach from spike S6): the world point to use, plus context for the
/// on-screen indicator — <see cref="Normal"/> for OnFace (work-plane locking) and
/// <see cref="Direction"/> for AxisLock (which axis, for red/green/blue feedback).
/// </summary>
public readonly struct InferenceResult(SnapType type, Vector3 point, string label, Vector3 normal = default, Vector3 direction = default)
{
    public readonly SnapType Type = type;
    public readonly Vector3 WorldPoint = point;
    public readonly string Label = label;
    public readonly Vector3 Normal = normal;       // face normal when Type == OnFace
    public readonly Vector3 Direction = direction;  // locked axis when Type == AxisLock
}
