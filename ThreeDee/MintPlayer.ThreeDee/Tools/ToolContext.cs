using System.Numerics;
using MintPlayer.ThreeDee.Commands;
using MintPlayer.ThreeDee.Geometry;
using MintPlayer.ThreeDee.Inference;
using MintPlayer.ThreeDee.Rendering;

namespace MintPlayer.ThreeDee.Tools;

/// <summary>
/// Services a tool needs: the document, camera, picker, selection, undo history, repaint/rebuild
/// hooks, and basic point inference (vertex snap + face/ground-plane projection). The full
/// inference engine arrives in M6; this is the minimal "where in 3D did the cursor land?" needed
/// to draw on the ground or on an existing face.
/// </summary>
public sealed class ToolContext(
    Scene scene, Camera camera, Picker picker, Selection selection, History history,
    Action invalidate, Action rebuildScene)
{
    public Scene Scene => scene;
    public Camera Camera => camera;
    public Picker Picker => picker;
    public Selection Selection => selection;
    public History History => history;

    public float PixelTolerance => 9f;

    /// <summary>The active construction plane used when a click doesn't land on existing geometry
    /// (default = ground/XZ). Lets tools draw on the XY/YZ planes too.</summary>
    public (Vector3 Point, Vector3 Normal) WorkPlane { get; set; } = (Vector3.Zero, Vector3.UnitY);

    readonly InferenceEngine _engine = new();

    /// <summary>Run the snapping engine for a cursor pixel (back-projecting the pixel tolerance).</summary>
    public InferenceResult Infer(Point p, Vector3? fromPoint, (Vector3 Point, Vector3 Normal)? plane, bool drawing)
    {
        float tol = PixelTolerance * camera.WorldPerPixelAt(camera.Distance);
        return _engine.Infer(scene.Mesh, Ray(p), fromPoint, tol, plane, drawing);
    }

    public void Invalidate() => invalidate();

    /// <summary>Run a command through the undo stack (rebuild + repaint follow via History.Changed).</summary>
    public void Commit(ICommand command) => history.Execute(command);

    /// <summary>Rebuild render data from the mesh (after a non-command edit, if ever needed).</summary>
    public void RebuildScene() => rebuildScene();

    public Ray Ray(Point p) => camera.Unproject(new Vector2(p.X, p.Y));

    /// <summary>Snap to the nearest existing vertex within the pixel tolerance.</summary>
    public bool SnapVertex(Point p, out Vector3 world)
    {
        world = default;
        float best = PixelTolerance;
        bool found = false;
        foreach (var v in scene.Mesh.Vertices)
            if (camera.Project(v.P, out Vector2 s))
            {
                float d = MathF.Sqrt((s.X - p.X) * (s.X - p.X) + (s.Y - p.Y) * (s.Y - p.Y));
                if (d <= best) { best = d; world = v.P; found = true; }
            }
        return found;
    }

    /// <summary>
    /// Resolve the first click of a drawing op into a 3D point + work-plane normal:
    /// vertex snap → existing face plane → ground plane (y=0).
    /// </summary>
    public bool PickStartPoint(Point p, out Vector3 world, out Vector3 normal)
    {
        normal = Vector3.UnitY;
        if (SnapVertex(p, out world)) return true;
        Ray ray = Ray(p);
        var f = picker.PickFace(ray, out Vector3 hit, out _);
        if (f is not null) { world = hit; normal = f.Normal(); return true; }
        return ray.IntersectPlane(Vector3.Zero, Vector3.UnitY, out world);
    }

    /// <summary>Resolve a subsequent click onto the locked work plane (with vertex snap override).</summary>
    public bool PickOnPlane(Point p, Vector3 planePoint, Vector3 planeNormal, out Vector3 world)
    {
        if (SnapVertex(p, out world)) return true;
        return Ray(p).IntersectPlane(planePoint, planeNormal, out world);
    }
}
