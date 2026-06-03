using System.Numerics;
using MintPlayer.ThreeDee.Rendering;

namespace MintPlayer.ThreeDee;

/// <summary>View scaffolding shared across milestones: the ground grid and the axis tripod.</summary>
public static class DemoContent
{
    /// <summary>Ground grid on y=0 plus a colored X/Y/Z axis tripod.</summary>
    public static List<WorldLine> BuildGridAndAxes(int half = 12, float step = 1f)
    {
        const int gridArgb = unchecked((int)0xFF3A_4048);
        const int gridMajor = unchecked((int)0xFF4E_5660);
        var lines = new List<WorldLine>();
        float ext = half * step;
        for (int i = -half; i <= half; i++)
        {
            float p = i * step;
            int c = (i % 5 == 0) ? gridMajor : gridArgb;
            lines.Add(new WorldLine(new Vector3(p, 0, -ext), new Vector3(p, 0, ext), c));
            lines.Add(new WorldLine(new Vector3(-ext, 0, p), new Vector3(ext, 0, p), c));
        }
        // Axis tripod: X red, Y green (up), Z blue.
        const float L = 2.5f;
        lines.Add(new WorldLine(Vector3.Zero, new Vector3(L, 0, 0), unchecked((int)0xFFE0_4040)));
        lines.Add(new WorldLine(Vector3.Zero, new Vector3(0, L, 0), unchecked((int)0xFF40_C040)));
        lines.Add(new WorldLine(Vector3.Zero, new Vector3(0, 0, L), unchecked((int)0xFF4878_FF)));
        return lines;
    }
}
