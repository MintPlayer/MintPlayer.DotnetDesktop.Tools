using System.Numerics;

namespace MintPlayer.ThreeDee.Rendering;

/// <summary>Flat per-face directional ("sun") shading. No textures (PRD non-goal).</summary>
public static class Shading
{
    public const float Ambient = 0.25f;

    /// <summary>Diffuse term for a world-space face normal under a (normalized) light direction.</summary>
    public static float Diffuse(Vector3 worldNormal, Vector3 sunDir) =>
        MathF.Max(Ambient, Vector3.Dot(worldNormal, sunDir));

    /// <summary>Pack an 0..255 RGB base color scaled by an intensity into a 32bpp ARGB int.</summary>
    public static int Shade(Vector3 baseColor, float intensity)
    {
        int r = (int)Math.Clamp(baseColor.X * intensity, 0, 255);
        int g = (int)Math.Clamp(baseColor.Y * intensity, 0, 255);
        int b = (int)Math.Clamp(baseColor.Z * intensity, 0, 255);
        return unchecked((int)0xFF000000) | (r << 16) | (g << 8) | b;
    }
}
