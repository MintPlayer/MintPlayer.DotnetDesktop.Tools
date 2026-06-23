using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;

namespace MintPlayer.ThreeDee.Rendering;

/// <summary>
/// Hand-written software 3D rasterizer (fork F1a) with a per-pixel Z-buffer + LockBits
/// framebuffer (fork F2). Lifted and generalized from spike S1 (137 FPS @ 9.6k tris).
/// Draws shaded triangles and depth-tested lines (mesh feature edges + world grid/axes).
/// </summary>
public sealed class SoftwareRasterizer : IRenderer
{
    int _w, _h;
    Bitmap? _bmp;
    int[] _color = [];
    float[] _zbuf = [];

    const int ClearArgb = unchecked((int)0xFF20_2830);
    const int EdgeArgb = unchecked((int)0xFFD0_D4D8); // light: reads on dark bg and on shaded faces
    // Mesh feature edges lie exactly on face boundaries, so they need a small depth bias to
    // win the z-test against their own faces. World lines (grid/axes) never coincide with
    // geometry, so they use a strict test (bias 0) to avoid bleeding through faces.
    const float EdgeDepthBias = 3e-4f;

    public Vector3 SunDirection { get; set; } = Vector3.Normalize(new Vector3(0.4f, 0.8f, 0.55f));

    /// <summary>SketchUp-style tint applied to faces seen from behind (e.g. open single-sided faces).</summary>
    public Vector3 BackColor { get; set; } = new(95, 115, 150);

    /// <summary>When true, back faces are skipped entirely instead of tinted (perf; default off).</summary>
    public bool CullBackFaces { get; set; } = false;

    public void Resize(int widthPx, int heightPx)
    {
        widthPx = Math.Max(1, widthPx);
        heightPx = Math.Max(1, heightPx);
        if (widthPx == _w && heightPx == _h && _bmp is not null) return;
        _w = widthPx; _h = heightPx;
        _bmp?.Dispose();
        _bmp = new Bitmap(_w, _h, PixelFormat.Format32bppArgb);
        _color = new int[_w * _h];
        _zbuf = new float[_w * _h];
    }

    public Bitmap RenderFrame(IReadOnlyList<RenderMesh> meshes, IReadOnlyList<WorldLine> lines, Camera camera, RenderStyle style)
    {
        if (_bmp is null) Resize(camera.ViewportWidth, camera.ViewportHeight);
        Clear();

        Matrix4x4 vp = camera.ViewProjection;
        bool fill = style is RenderStyle.Shaded or RenderStyle.ShadedEdges or RenderStyle.HiddenLine;
        bool shade = style is RenderStyle.Shaded or RenderStyle.ShadedEdges;
        bool drawEdges = style is RenderStyle.Wireframe or RenderStyle.HiddenLine or RenderStyle.ShadedEdges;
        bool depthTestLines = style is not RenderStyle.Wireframe;

        foreach (var mesh in meshes)
        {
            Matrix4x4 mvp = mesh.Model * vp;
            int n = mesh.Vertices.Length;
            var pv = new PV[n];
            for (int i = 0; i < n; i++) pv[i] = Project(mesh.Vertices[i], mvp);

            if (fill)
            {
                int[] tri = mesh.Triangles;
                for (int t = 0; t < tri.Length; t += 3)
                {
                    int a = tri[t], b = tri[t + 1], c = tri[t + 2];
                    if (!pv[a].Ok || !pv[b].Ok || !pv[c].Ok) continue;
                    float area = SignedArea(pv[a], pv[b], pv[c]);
                    if (MathF.Abs(area) < 1e-7f) continue;     // degenerate
                    bool back = area > 0;                      // front faces are CW in screen space => negative
                    if (back && CullBackFaces) continue;

                    int col;
                    if (shade)
                    {
                        Vector3 wn = FaceNormal(mesh.Vertices[a], mesh.Vertices[b], mesh.Vertices[c], mesh.Model);
                        if (back) wn = -wn;                    // face the viewer for shading
                        col = Shading.Shade(back ? BackColor : mesh.Color, Shading.Diffuse(wn, SunDirection));
                    }
                    else col = ClearArgb; // HiddenLine: faces occlude but are invisible
                    FillTriangle(pv[a], pv[b], pv[c], area, col);
                }
            }

            if (drawEdges)
                foreach (var (i0, i1) in MeshEdges(mesh))
                    DrawLine(pv[i0], pv[i1], EdgeArgb, depthTestLines, EdgeDepthBias);
        }

        // Ground grid + axis tripod, strictly depth-tested against the scene.
        foreach (var ln in lines)
            DrawLine(Project(ln.A, vp), Project(ln.B, vp), ln.ColorArgb, depthTestLines, 0f);

        Blit();
        return _bmp!;
    }

    static IEnumerable<(int, int)> MeshEdges(RenderMesh mesh)
    {
        if (mesh.Edges is { } e)
        {
            for (int k = 0; k + 1 < e.Length; k += 2) yield return (e[k], e[k + 1]);
        }
        else
        {
            int[] tri = mesh.Triangles;
            for (int t = 0; t < tri.Length; t += 3)
            {
                yield return (tri[t], tri[t + 1]);
                yield return (tri[t + 1], tri[t + 2]);
                yield return (tri[t + 2], tri[t]);
            }
        }
    }

    // ---- pixel pipeline ----

    readonly struct PV(float x, float y, float z, bool ok)
    {
        public readonly float X = x, Y = y, Z = z;
        public readonly bool Ok = ok;
    }

    PV Project(Vector3 v, Matrix4x4 mvp)
    {
        var clip = Vector4.Transform(new Vector4(v, 1f), mvp);
        if (clip.W <= 1e-4f) return new PV(0, 0, 0, false);
        float invw = 1f / clip.W;
        float sx = (clip.X * invw * 0.5f + 0.5f) * _w;
        float sy = (1f - (clip.Y * invw * 0.5f + 0.5f)) * _h;
        return new PV(sx, sy, clip.Z * invw, true);
    }

    static float SignedArea(PV a, PV b, PV c) =>
        (b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X);

    static Vector3 FaceNormal(Vector3 a, Vector3 b, Vector3 c, Matrix4x4 model)
    {
        Vector3 wa = Vector3.Transform(a, model), wb = Vector3.Transform(b, model), wc = Vector3.Transform(c, model);
        return Vector3.Normalize(Vector3.Cross(wb - wa, wc - wa));
    }

    void Clear()
    {
        Array.Fill(_color, ClearArgb);
        Array.Fill(_zbuf, float.PositiveInfinity);
    }

    void FillTriangle(PV pa, PV pb, PV pc, float area, int col)
    {
        int minX = Math.Max(0, (int)MathF.Floor(MathF.Min(pa.X, MathF.Min(pb.X, pc.X))));
        int maxX = Math.Min(_w - 1, (int)MathF.Ceiling(MathF.Max(pa.X, MathF.Max(pb.X, pc.X))));
        int minY = Math.Max(0, (int)MathF.Floor(MathF.Min(pa.Y, MathF.Min(pb.Y, pc.Y))));
        int maxY = Math.Min(_h - 1, (int)MathF.Ceiling(MathF.Max(pa.Y, MathF.Max(pb.Y, pc.Y))));
        if (minX > maxX || minY > maxY) return;
        float inv = 1f / area;
        for (int y = minY; y <= maxY; y++)
        {
            int row = y * _w;
            for (int x = minX; x <= maxX; x++)
            {
                float px = x + 0.5f, py = y + 0.5f;
                float w0 = ((pb.X - px) * (pc.Y - py) - (pb.Y - py) * (pc.X - px)) * inv;
                float w1 = ((pc.X - px) * (pa.Y - py) - (pc.Y - py) * (pa.X - px)) * inv;
                float w2 = 1f - w0 - w1;
                if (w0 < 0 || w1 < 0 || w2 < 0) continue;
                float z = w0 * pa.Z + w1 * pb.Z + w2 * pc.Z;
                int pi = row + x;
                if (z < _zbuf[pi]) { _zbuf[pi] = z; _color[pi] = col; }
            }
        }
    }

    /// <summary>
    /// Xiaolin Wu anti-aliased line into the framebuffer, optionally depth-tested with a given
    /// bias. Each step plots the two pixels straddling the true line with complementary coverage,
    /// blending the line colour over the existing pixel. Like the old DDA path, lines read but
    /// never write the z-buffer, so partial-coverage pixels can't spuriously occlude geometry.
    /// </summary>
    void DrawLine(PV p0, PV p1, int col, bool depthTest, float bias)
    {
        if (!p0.Ok || !p1.Ok) return;
        float x0 = p0.X, y0 = p0.Y, z0 = p0.Z;
        float x1 = p1.X, y1 = p1.Y, z1 = p1.Z;

        // Iterate along the major axis so each column/row has exactly one straddling pair.
        bool steep = MathF.Abs(y1 - y0) > MathF.Abs(x1 - x0);
        if (steep) { (x0, y0) = (y0, x0); (x1, y1) = (y1, x1); }
        if (x0 > x1) { (x0, x1) = (x1, x0); (y0, y1) = (y1, y0); (z0, z1) = (z1, z0); }

        float dx = x1 - x0;
        float gradient = dx == 0f ? 0f : (y1 - y0) / dx;
        float zgrad = dx == 0f ? 0f : (z1 - z0) / dx;

        int xStart = (int)MathF.Round(x0);
        int xEnd = (int)MathF.Round(x1);
        float yAxis = y0 + gradient * (xStart - x0);
        for (int x = xStart; x <= xEnd; x++, yAxis += gradient)
        {
            float z = z0 + zgrad * (x - x0);
            int yFloor = (int)MathF.Floor(yAxis);
            float frac = yAxis - yFloor;
            if (steep)
            {
                Plot(yFloor, x, z, col, 1f - frac, depthTest, bias);
                Plot(yFloor + 1, x, z, col, frac, depthTest, bias);
            }
            else
            {
                Plot(x, yFloor, z, col, 1f - frac, depthTest, bias);
                Plot(x, yFloor + 1, z, col, frac, depthTest, bias);
            }
        }
    }

    void Plot(int x, int y, float z, int col, float coverage, bool depthTest, float bias)
    {
        if (coverage <= 0f) return;
        if ((uint)x >= (uint)_w || (uint)y >= (uint)_h) return;
        int pi = y * _w + x;
        if (depthTest && z > _zbuf[pi] + bias) return;
        _color[pi] = coverage >= 1f ? col : Blend(_color[pi], col, coverage);
    }

    /// <summary>Alpha-blends <paramref name="fg"/> over <paramref name="bg"/> (both opaque ARGB).</summary>
    static int Blend(int bg, int fg, float a)
    {
        float ia = 1f - a;
        int r = (int)(((fg >> 16) & 0xFF) * a + ((bg >> 16) & 0xFF) * ia);
        int g = (int)(((fg >> 8) & 0xFF) * a + ((bg >> 8) & 0xFF) * ia);
        int b = (int)((fg & 0xFF) * a + (bg & 0xFF) * ia);
        return unchecked((int)0xFF000000) | (r << 16) | (g << 8) | b;
    }

    unsafe void Blit()
    {
        var rect = new Rectangle(0, 0, _w, _h);
        var data = _bmp!.LockBits(rect, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);
        try
        {
            fixed (int* src = _color)
            {
                int* dst = (int*)data.Scan0;
                for (int y = 0; y < _h; y++)
                    Buffer.MemoryCopy(src + y * _w, dst + y * (data.Stride / 4), (long)_w * 4, (long)_w * 4);
            }
        }
        finally { _bmp!.UnlockBits(data); }
    }

    public void Dispose() => _bmp?.Dispose();
}
