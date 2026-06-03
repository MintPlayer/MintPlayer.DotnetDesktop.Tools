using System.Drawing;
using System.Numerics;

namespace MintPlayer.ThreeDee.Rendering;

/// <summary>
/// The render-backend swap point (architecture fork F1). The software rasterizer is the
/// chosen implementation; a hosted-WPF backend could satisfy the same contract.
///
/// Note (refines PRD §6): the renderer consumes triangle meshes + depth-tested world lines
/// rather than the <c>Scene</c> directly, keeping it dumb and backend-agnostic — the Scene
/// will produce these inputs in M2. Picking and Camera live deliberately outside this interface.
/// </summary>
public interface IRenderer : IDisposable
{
    /// <summary>(Re)allocate the framebuffer + depth buffer for a pixel size.</summary>
    void Resize(int widthPx, int heightPx);

    /// <summary>Render one frame and return the backing bitmap to blit to screen.</summary>
    Bitmap RenderFrame(
        IReadOnlyList<RenderMesh> meshes,
        IReadOnlyList<WorldLine> lines,
        Camera camera,
        RenderStyle style);

    /// <summary>Direction the flat-shading "sun" points toward (will be normalized).</summary>
    Vector3 SunDirection { get; set; }
}
