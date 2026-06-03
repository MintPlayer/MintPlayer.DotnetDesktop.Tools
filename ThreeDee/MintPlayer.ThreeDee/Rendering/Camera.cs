using System.Numerics;

namespace MintPlayer.ThreeDee.Rendering;

/// <summary>
/// Orbit camera in spherical coordinates (azimuth, elevation, distance) about a Target.
/// World is right-handed with +Y up. Math (project/unproject, Y-flip, D3D [0,1] depth)
/// is the verbatim S3 spike result; orbit/pan/zoom/framing are layered on top.
/// </summary>
public sealed class Camera
{
    // ---- orbit parameters ----
    public Vector3 Target = Vector3.Zero;
    public float AzimuthDeg = 45f;
    public float ElevationDeg = 30f;
    public float Distance = 12f;

    // ---- lens ----
    public float FovYDeg = 55f;
    public float NearPlane = 0.1f;
    public float FarPlane = 1000f;
    public bool Orthographic = false;

    // ---- viewport (pixels) ----
    public int ViewportWidth = 1280;
    public int ViewportHeight = 720;

    const float MinDistance = 0.05f;
    const float MaxElevation = 89f;

    public Vector3 Up => Vector3.UnitY;
    float FovYRad => FovYDeg * (MathF.PI / 180f);
    float Aspect => ViewportHeight <= 0 ? 1f : (float)ViewportWidth / ViewportHeight;

    /// <summary>Eye position derived from spherical coords about Target.</summary>
    public Vector3 Eye
    {
        get
        {
            float az = AzimuthDeg * (MathF.PI / 180f);
            float el = ElevationDeg * (MathF.PI / 180f);
            float cosEl = MathF.Cos(el);
            return Target + new Vector3(
                Distance * cosEl * MathF.Cos(az),
                Distance * MathF.Sin(el),
                Distance * cosEl * MathF.Sin(az));
        }
    }

    public Vector3 Forward => Vector3.Normalize(Target - Eye);

    public Matrix4x4 ViewMatrix => Matrix4x4.CreateLookAt(Eye, Target, Up);

    public Matrix4x4 ProjectionMatrix
    {
        get
        {
            if (Orthographic)
            {
                // Match the perspective framing at the target depth so the toggle is seamless.
                float h = 2f * Distance * MathF.Tan(FovYRad * 0.5f);
                return Matrix4x4.CreateOrthographic(h * Aspect, h, NearPlane, FarPlane);
            }
            return Matrix4x4.CreatePerspectiveFieldOfView(FovYRad, Aspect, NearPlane, FarPlane);
        }
    }

    public Matrix4x4 ViewProjection => ViewMatrix * ProjectionMatrix;

    // ---- NDC <-> screen (explicit WinForms Y-DOWN flip, isolated per S3) ----

    Vector2 NdcToScreen(Vector2 ndc) => new(
        (ndc.X * 0.5f + 0.5f) * ViewportWidth,
        (1f - (ndc.Y * 0.5f + 0.5f)) * ViewportHeight);

    Vector2 ScreenToNdc(Vector2 s) => new(
        (s.X / ViewportWidth) * 2f - 1f,
        (1f - s.Y / ViewportHeight) * 2f - 1f);

    /// <summary>World → screen pixel. Returns false if the point is behind the camera.</summary>
    public bool Project(Vector3 world, out Vector2 screen)
    {
        Vector4 clip = Vector4.Transform(new Vector4(world, 1f), ViewProjection);
        if (clip.W <= 1e-7f) { screen = default; return false; }
        screen = NdcToScreen(new Vector2(clip.X / clip.W, clip.Y / clip.W));
        return true;
    }

    /// <summary>Screen pixel → world Ray (through the near and far planes).</summary>
    public Ray Unproject(Vector2 screen)
    {
        Vector2 ndc = ScreenToNdc(screen);
        if (!Matrix4x4.Invert(ViewProjection, out Matrix4x4 inv))
            throw new InvalidOperationException("ViewProjection not invertible.");
        Vector3 near = UnprojectNdc(new Vector3(ndc, 0f), inv); // D3D depth: near at z=0
        Vector3 far = UnprojectNdc(new Vector3(ndc, 1f), inv);  //            far  at z=1
        return new Ray(near, far - near);
    }

    static Vector3 UnprojectNdc(Vector3 ndc, Matrix4x4 invVp)
    {
        Vector4 h = Vector4.Transform(new Vector4(ndc, 1f), invVp);
        return new Vector3(h.X, h.Y, h.Z) / h.W;
    }

    // ---- interactive navigation (PRD §5.1) ----

    /// <summary>Orbit by pixel-drag deltas (degrees-per-pixel feel; elevation clamped).</summary>
    public void Orbit(float dxPixels, float dyPixels)
    {
        const float deg = 0.4f;
        AzimuthDeg += dxPixels * deg; // drag left → camera orbits so the model turns CW from top (SketchUp feel)
        ElevationDeg = Math.Clamp(ElevationDeg + dyPixels * deg, -MaxElevation, MaxElevation);
    }

    /// <summary>Pan: slide the target (and thus the eye) in the camera's view plane.</summary>
    public void Pan(float dxPixels, float dyPixels)
    {
        float worldPerPixel = (2f * Distance * MathF.Tan(FovYRad * 0.5f)) / ViewportHeight;
        Vector3 fwd = Forward;
        Vector3 right = Vector3.Normalize(Vector3.Cross(fwd, Up));
        Vector3 up = Vector3.Cross(right, fwd);
        Target += (-right * dxPixels + up * dyPixels) * worldPerPixel;
    }

    /// <summary>Plain dolly zoom (no cursor focus). factor &gt; 0 zooms in.</summary>
    public void Zoom(float factor)
    {
        Distance = MathF.Max(MinDistance, Distance * (1f - factor));
    }

    /// <summary>
    /// Zoom toward the cursor: dolly the eye toward the point under the cursor on the
    /// target's focal plane, translating the target by the same delta so the cursor
    /// point stays put (the SketchUp behaviour validated in spike S3).
    /// </summary>
    public void ZoomAtCursor(Vector2 cursorPx, float factor)
    {
        Ray ray = Unproject(cursorPx);
        if (!ray.IntersectPlane(Target, Forward, out Vector3 focus)) { Zoom(factor); return; }
        Vector3 eye = Eye;
        Vector3 delta = (focus - eye) * factor;
        if ((eye + delta - Target - delta).Length() < MinDistance) { Zoom(factor); return; }
        SetFromEyeTarget(eye + delta, Target + delta);
    }

    /// <summary>Frame a bounding box: center the target and back off to fit it on screen.</summary>
    public void ZoomExtents(BoundingBox box)
    {
        if (box.IsEmpty) return;
        Target = box.Center;
        float r = MathF.Max(box.Radius, 0.001f);
        float halfFovY = FovYRad * 0.5f;
        float halfFovX = MathF.Atan(MathF.Tan(halfFovY) * Aspect);
        float halfFov = MathF.Min(halfFovY, halfFovX);
        Distance = MathF.Max(MinDistance, (r / MathF.Sin(halfFov)) * 1.15f);
    }

    /// <summary>Snap to a standard view, then frame the supplied box.</summary>
    public void SetStandardView(StandardView view, BoundingBox box)
    {
        (AzimuthDeg, ElevationDeg) = view switch
        {
            StandardView.Top   => (-90f, MaxElevation),
            StandardView.Front => (90f, 0f),
            StandardView.Right => (0f, 0f),
            StandardView.Iso   => (45f, 35.264f),
            _                  => (AzimuthDeg, ElevationDeg),
        };
        ZoomExtents(box);
    }

    /// <summary>
    /// World size of one screen pixel at a given distance from the eye — used to convert a
    /// pixel-radius picking tolerance to world units (constant in parallel projection).
    /// </summary>
    public float WorldPerPixelAt(float viewDepth)
    {
        float h = 2f * MathF.Tan(FovYRad * 0.5f) * (Orthographic ? Distance : viewDepth);
        return h / MathF.Max(1, ViewportHeight);
    }

    /// <summary>Inverse of the Eye getter: derive spherical params from an explicit eye/target.</summary>
    public void SetFromEyeTarget(Vector3 eye, Vector3 target)
    {
        Vector3 off = eye - target;
        float dist = MathF.Max(off.Length(), MinDistance);
        Target = target;
        Distance = dist;
        ElevationDeg = MathF.Asin(Math.Clamp(off.Y / dist, -1f, 1f)) * (180f / MathF.PI);
        AzimuthDeg = MathF.Atan2(off.Z, off.X) * (180f / MathF.PI);
    }
}

public enum StandardView { Top, Front, Right, Iso }
