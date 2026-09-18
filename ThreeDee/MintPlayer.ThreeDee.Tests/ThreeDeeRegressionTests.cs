using System.Numerics;
using MintPlayer.ThreeDee;
using MintPlayer.ThreeDee.Commands;
using MintPlayer.ThreeDee.Geometry;
using MintPlayer.ThreeDee.Inference;
using MintPlayer.ThreeDee.Rendering;
using MintPlayer.ThreeDee.Tools;

namespace MintPlayer.ThreeDee.Tests;

/// <summary>
/// Ported from the hand-rolled regression harness that used to live in
/// MintPlayer.ThreeDee.Test/Program.cs. Each "=== ... ===" section became one [Fact];
/// every assertion is the one the harness made, with its message preserved, so the
/// port cannot have quietly weakened a check.
/// </summary>
public class ThreeDeeRegressionTests
{
    /// <summary>MeshBuilder.Box -> closed cube</summary>
    [Fact]
    public void MeshBuilder_Box_closed_cube()
    {
    var mesh = new Mesh();
    MeshBuilder.Box(mesh, Vector3.Zero, new Vector3(2, 2, 2), new Vector3(200, 200, 200));
    mesh.Twin();

    Assert.True(mesh.Vertices.Count == 8, $"verts == 8 (got {mesh.Vertices.Count})");
    Assert.True(mesh.Faces.Count == 6, $"faces == 6 (got {mesh.Faces.Count})");
    Assert.True(mesh.EdgeCount() == 12, $"edges == 12 (got {mesh.EdgeCount()})");
    Assert.True(mesh.IsClosed(), $"closed (0 boundary edges) (got {mesh.BoundaryEdgeCount()})");
    Assert.True(mesh.IsManifold(), $"manifold (got {mesh.IsManifold()})");
    int euler = mesh.Vertices.Count - mesh.EdgeCount() + mesh.Faces.Count;
    Assert.True(euler == 2, $"Euler V-E+F == 2 (got {euler})");
    Assert.True(mesh.AllNormalsOutward(out int bad), $"all normals outward (got {bad})");
    }

    /// <summary>Extrude quad -> box</summary>
    [Fact]
    public void Extrude_quad_box()
    {
    var mesh = new Mesh();
    var f = mesh.MakeFace([new(0, 0, 0), new(1, 0, 0), new(1, 1, 0), new(0, 1, 0)]);
    Assert.True(mesh.BoundaryEdgeCount() == 4, $"source quad is open (4 boundary edges) (got {mesh.BoundaryEdgeCount()})");
    mesh.Extrude(f, 1.0f);

    Assert.True(mesh.Vertices.Count == 8, $"verts == 8 (got {mesh.Vertices.Count})");
    Assert.True(mesh.Faces.Count == 6, $"faces == 6 (got {mesh.Faces.Count})");
    Assert.True(mesh.EdgeCount() == 12, $"edges == 12 (got {mesh.EdgeCount()})");
    Assert.True(mesh.IsClosed(), $"closed (got {mesh.BoundaryEdgeCount()})");
    Assert.True(mesh.IsManifold(), $"manifold (got {mesh.IsManifold()})");
    Assert.True(mesh.AllNormalsOutward(out int bad), $"all normals outward (got {bad})");
    }

    /// <summary>Extrude triangle -> prism</summary>
    [Fact]
    public void Extrude_triangle_prism()
    {
    var mesh = new Mesh();
    var f = mesh.MakeFace([new(0, 0, 0), new(1, 0, 0), new(0, 1, 0)]);
    mesh.Extrude(f, 2.0f);

    Assert.True(mesh.Vertices.Count == 6, $"verts == 6 (got {mesh.Vertices.Count})");
    Assert.True(mesh.Faces.Count == 5, $"faces == 5 (got {mesh.Faces.Count})");
    Assert.True(mesh.EdgeCount() == 9, $"edges == 9 (got {mesh.EdgeCount()})");
    Assert.True(mesh.IsClosed(), $"closed (got {mesh.BoundaryEdgeCount()})");
    Assert.True(mesh.AllNormalsOutward(out int bad), $"all normals outward (got {bad})");
    }

    /// <summary>Scene.BuildRenderMeshes</summary>
    [Fact]
    public void Scene_BuildRenderMeshes()
    {
    var scene = TestScene.Build();
    var meshes = scene.BuildRenderMeshes();
    Assert.True(meshes.Count == scene.Mesh.Faces.Count, $"one render mesh per face (got {$"{meshes.Count} vs {scene.Mesh.Faces.Count}"})");

    bool fanOk = true, edgeOk = true;
    foreach (var rm in meshes)
    {
        int n = rm.Vertices.Length;
        if (rm.Triangles.Length != (n - 2) * 3) fanOk = false;
        if (rm.Edges is null || rm.Edges.Length != n * 2) edgeOk = false;
    }
    Assert.True(fanOk, $"each face fan-triangulated to (n-2) tris (got {fanOk})");
    Assert.True(edgeOk, $"each face emits an n-edge boundary ring (got {edgeOk})");
    Assert.True(!scene.Bounds().IsEmpty, $"scene bounds non-empty (got {scene.Bounds().IsEmpty})");
    }

    /// <summary>Picker: ray-triangle face pick</summary>
    [Fact]
    public void Picker_ray_triangle_face_pick()
    {
    var mesh = new Mesh();
    MeshBuilder.Box(mesh, Vector3.Zero, new Vector3(2, 2, 2), new Vector3(200, 200, 200));
    mesh.Twin();
    var picker = new MintPlayer.ThreeDee.Rendering.Picker(mesh);

    // Ray straight down -Z from +Z hits the +Z face (z=1) at t=4, point (0,0,1).
    var ray = new Ray(new Vector3(0, 0, 5), new Vector3(0, 0, -1));
    var face = picker.PickFace(ray, out Vector3 p, out float t);
    Assert.True(face is not null, $"ray hits a face (got {face is not null})");
    Assert.True(Vector3.Distance(p, new Vector3(0, 0, 1)) < 1e-3f, $"hit point on +Z face (got {p})");
    Assert.True(MathF.Abs(t - 4f) < 1e-3f, $"hit distance t == 4 (got {t})");

    var miss = picker.PickFace(new Ray(new Vector3(5, 5, 5), new Vector3(1, 1, 1)), out _, out _);
    Assert.True(miss is null, $"ray pointing away misses (got {miss is null})");
    }

    /// <summary>Picker: camera unproject -> edge / face</summary>
    [Fact]
    public void Picker_camera_unproject_edge_face()
    {
    var mesh = new Mesh();
    MeshBuilder.Box(mesh, Vector3.Zero, new Vector3(2, 2, 2), new Vector3(200, 200, 200));
    mesh.Twin();
    var picker = new MintPlayer.ThreeDee.Rendering.Picker(mesh);
    var cam = new MintPlayer.ThreeDee.Rendering.Camera { ViewportWidth = 1000, ViewportHeight = 1000 };
    cam.SetStandardView(MintPlayer.ThreeDee.Rendering.StandardView.Iso, mesh.Bounds());

    // Aiming through an edge midpoint should pick that edge.
    var (ea, eb) = mesh.UniqueEdges().First();
    Vector3 mid = (ea.P + eb.P) * 0.5f;
    cam.Project(mid, out var es);
    var ePick = picker.Pick(cam.Unproject(es), cam);
    Assert.True(ePick.Kind == MintPlayer.ThreeDee.Rendering.PickKind.Edge, $"midpoint ray picks an edge (got {ePick.Kind})");
    Assert.True(ePick.Edge.Equals(new EdgeKey(ea, eb)), $"picks the targeted edge (got {"edge match"})");

    // Aiming through a face centroid should pick that face.
    var top = mesh.Faces.First(f => f.Normal().Y > 0.9f); // +Y top face
    cam.Project(top.Centroid(), out var fs);
    var fPick = picker.Pick(cam.Unproject(fs), cam);
    Assert.True(fPick.Kind == MintPlayer.ThreeDee.Rendering.PickKind.Face, $"centroid ray picks a face (got {fPick.Kind})");
    Assert.True(ReferenceEquals(fPick.Face, top), $"picks the targeted face (got {"face match"})");
    }

    /// <summary>Selection apply/toggle</summary>
    [Fact]
    public void Selection_apply_toggle()
    {
    var mesh = new Mesh();
    MeshBuilder.Box(mesh, Vector3.Zero, new Vector3(2, 2, 2), new Vector3(200, 200, 200));
    mesh.Twin();
    var sel = new Selection();
    var f0 = mesh.Faces[0];

    sel.Apply(MintPlayer.ThreeDee.Rendering.PickResult.OnFace(f0, Vector3.Zero, 1f), toggle: false);
    Assert.True(sel.Count == 1 && sel.Contains(f0), $"replace-select adds one face (got {sel.Count})");
    sel.Apply(MintPlayer.ThreeDee.Rendering.PickResult.OnFace(f0, Vector3.Zero, 1f), toggle: true);
    Assert.True(sel.IsEmpty, $"toggle removes it (got {sel.Count})");
    }

    /// <summary>CreateFaceCommand + History</summary>
    [Fact]
    public void CreateFaceCommand_with_History()
    {
    var scene = new Scene();
    var history = new History();
    // A quad on the ground; CreateFaceCommand should wind it so the normal matches +Y.
    Vector3[] pts = [new(0, 0, 0), new(2, 0, 0), new(2, 0, 2), new(0, 0, 2)];
    history.Execute(new CreateFaceCommand(scene.Mesh, pts, new Vector3(200, 200, 200), Vector3.UnitY));

    Assert.True(scene.Mesh.Faces.Count == 1, $"face created (got {scene.Mesh.Faces.Count})");
    Assert.True(scene.Mesh.Vertices.Count == 4, $"4 vertices added (got {scene.Mesh.Vertices.Count})");
    Assert.True(Vector3.Dot(scene.Mesh.Faces[0].Normal(), Vector3.UnitY) > 0.99f, $"normal matches intended +Y (got {scene.Mesh.Faces[0].Normal()})");

    history.Undo();
    Assert.True(scene.Mesh.Faces.Count == 0 && scene.Mesh.Vertices.Count == 0, $"undo removes face + verts (got {$"{scene.Mesh.Faces.Count}/{scene.Mesh.Vertices.Count}"})");
    history.Redo();
    Assert.True(scene.Mesh.Faces.Count == 1 && scene.Mesh.Vertices.Count == 4, $"redo restores face (got {scene.Mesh.Faces.Count})");
    Assert.True(history.CanUndo && !history.CanRedo, $"CanUndo true, CanRedo false (got {$"{history.CanUndo}/{history.CanRedo}"})");
    }

    /// <summary>RectangleTool end-to-end</summary>
    [Fact]
    public void RectangleTool_end_to_end()
    {
    var scene = new Scene();
    var cam = new Camera { ViewportWidth = 1000, ViewportHeight = 1000 };
    cam.Target = Vector3.Zero; cam.Distance = 20; cam.AzimuthDeg = 45; cam.ElevationDeg = 35;
    var ctx = new ToolContext(scene, cam, new Picker(scene.Mesh), new Selection(), new History(), () => { }, () => { });

    var tool = new RectangleTool();
    tool.Activate(ctx);
    cam.Project(new Vector3(-2, 0, -2), out var s0);
    cam.Project(new Vector3(2, 0, 2), out var s1);
    tool.OnMouseDown(ctx, MouseButtons.Left, new Point((int)s0.X, (int)s0.Y), Keys.None);
    tool.OnMouseDown(ctx, MouseButtons.Left, new Point((int)s1.X, (int)s1.Y), Keys.None);

    Assert.True(scene.Mesh.Faces.Count == 1, $"rectangle created a face (got {scene.Mesh.Faces.Count})");
    Assert.True(scene.Mesh.Faces.Count == 1 && scene.Mesh.Faces[0].SideCount() == 4, $"face is a quad (got {(scene.Mesh.Faces.Count == 1 ? scene.Mesh.Faces[0].SideCount() : -1)})");
    Assert.True(scene.Mesh.Faces.Count == 1 && scene.Mesh.Faces[0].Vertices().All(v => MathF.Abs(v.P.Y) < 1e-3f), $"face lies on the ground plane (got {"y≈0"})");
    Assert.True(ctx.History.CanUndo, $"recorded on the undo stack (got {ctx.History.CanUndo})");
    }

    /// <summary>AddEdgeCommand (wire edges)</summary>
    [Fact]
    public void AddEdgeCommand_wire_edges()
    {
    var mesh = new Mesh();
    var c1 = new AddEdgeCommand(mesh, new Vector3(0, 0, 0), new Vector3(1, 0, 0));
    var c2 = new AddEdgeCommand(mesh, new Vector3(1, 0, 0), new Vector3(2, 0, 0));
    c1.Do(); c2.Do();
    Assert.True(mesh.Wires.Count == 2, $"two wires created (got {mesh.Wires.Count})");
    Assert.True(mesh.Vertices.Count == 3, $"shared endpoint welded (3 verts) (got {mesh.Vertices.Count})");
    Assert.True(mesh.UniqueEdges().Count() == 2, $"UniqueEdges counts 2 (got {mesh.UniqueEdges().Count()})");

    c2.Undo();
    Assert.True(mesh.Wires.Count == 1, $"undo c2 leaves 1 wire (got {mesh.Wires.Count})");
    Assert.True(mesh.Vertices.Count == 2, $"shared vertex kept (2 verts) (got {mesh.Vertices.Count})");
    c1.Undo();
    Assert.True(mesh.Wires.Count == 0 && mesh.Vertices.Count == 0, $"undo c1 empties mesh (got {$"{mesh.Wires.Count}/{mesh.Vertices.Count}"})");
    }

    /// <summary>LineTool draw + close</summary>
    [Fact]
    public void LineTool_draw_close()
    {
    var scene = new Scene();
    var cam = new Camera { ViewportWidth = 1000, ViewportHeight = 1000 };
    cam.Target = Vector3.Zero; cam.Distance = 20; cam.AzimuthDeg = 45; cam.ElevationDeg = 35;
    var ctx = new ToolContext(scene, cam, new Picker(scene.Mesh), new Selection(), new History(), () => { }, () => { });

    var tool = new LineTool();
    tool.Activate(ctx);
    Point Screen(Vector3 w) { cam.Project(w, out var s); return new Point((int)s.X, (int)s.Y); }
    Vector3 a = new(0, 0, 0), b = new(3, 0, 0), d = new(0, 0, 3);
    tool.OnMouseDown(ctx, MouseButtons.Left, Screen(a), Keys.None); // start
    tool.OnMouseDown(ctx, MouseButtons.Left, Screen(b), Keys.None); // edge a-b
    tool.OnMouseDown(ctx, MouseButtons.Left, Screen(d), Keys.None); // edge b-d
    tool.OnMouseDown(ctx, MouseButtons.Left, Screen(a), Keys.None); // click start -> close into a face

    Assert.True(scene.Mesh.Wires.Count == 2, $"two wire edges drawn (got {scene.Mesh.Wires.Count})");
    Assert.True(scene.Mesh.Faces.Count == 1, $"loop closed into one face (got {scene.Mesh.Faces.Count})");
    Assert.True(scene.Mesh.Faces.Count == 1 && scene.Mesh.Faces[0].SideCount() == 3, $"face is a triangle (got {(scene.Mesh.Faces.Count == 1 ? scene.Mesh.Faces[0].SideCount() : -1)})");
    Assert.True(scene.Mesh.Vertices.Count == 3, $"vertices welded (3 total) (got {scene.Mesh.Vertices.Count})");
    Assert.True(scene.Mesh.UniqueEdges().Count() == 3, $"UniqueEdges dedups wire+face (3) (got {scene.Mesh.UniqueEdges().Count()})");

    // Switching away mid-run keeps committed edges (the original bug report).
    tool.Activate(ctx);
    var t2 = new LineTool(); t2.Activate(ctx);
    t2.OnMouseDown(ctx, MouseButtons.Left, Screen(new Vector3(-2, 0, -2)), Keys.None);
    t2.OnMouseDown(ctx, MouseButtons.Left, Screen(new Vector3(-2, 0, 2)), Keys.None); // one edge committed
    int wiresBefore = scene.Mesh.Wires.Count;
    new SelectTool().Activate(ctx); // "switch to Select"
    Assert.True(scene.Mesh.Wires.Count == wiresBefore && wiresBefore == 3, $"switching tools keeps drawn edges (got {scene.Mesh.Wires.Count})");
    }

    /// <summary>PushPull extrude (standalone face -> box)</summary>
    [Fact]
    public void PushPull_extrude_standalone_face_box()
    {
    var mesh = new Mesh();
    var f = mesh.MakeFace([new(0, 0, 0), new(2, 0, 0), new(2, 0, 2), new(0, 0, 2)]);
    Assert.True(Mesh.IsStandalone(f), $"starts standalone (got {Mesh.IsStandalone(f)})");

    var cmd = new PushPullCommand(mesh, f, 3f);
    cmd.Do();
    Assert.True(mesh.Faces.Count == 6, $"becomes a closed box (6 faces) (got {mesh.Faces.Count})");
    Assert.True(mesh.Vertices.Count == 8, $"8 vertices (got {mesh.Vertices.Count})");
    Assert.True(mesh.IsClosed() && mesh.IsManifold(), $"closed + manifold (got {$"{mesh.IsClosed()}/{mesh.IsManifold()}"})");
    Assert.True(mesh.AllNormalsOutward(out int bad), $"all normals outward (got {bad})");
    int euler = mesh.Vertices.Count - mesh.EdgeCount() + mesh.Faces.Count;
    Assert.True(euler == 2, $"Euler == 2 (got {euler})");

    cmd.Undo();
    Assert.True(mesh.Faces.Count == 1 && mesh.Vertices.Count == 4, $"undo restores the flat face (got {$"{mesh.Faces.Count}/{mesh.Vertices.Count}"})");
    }

    /// <summary>PushPull stretch (solid face)</summary>
    [Fact]
    public void PushPull_stretch_solid_face()
    {
    var mesh = new Mesh();
    MeshBuilder.Box(mesh, new Vector3(0, 1, 0), new Vector3(2, 2, 2), new Vector3(200, 200, 200));
    mesh.Twin();
    var top = mesh.Faces.First(f => f.Normal().Y > 0.9f);
    Assert.True(!Mesh.IsStandalone(top), $"top face is not standalone (got {Mesh.IsStandalone(top)})");
    float yBefore = top.Vertices().First().P.Y;

    var cmd = new PushPullCommand(mesh, top, 1.5f);
    cmd.Do();
    Assert.True(mesh.Faces.Count == 6, $"face count unchanged (stretch, not extrude) (got {mesh.Faces.Count})");
    Assert.True(top.Vertices().All(v => MathF.Abs(v.P.Y - (yBefore + 1.5f)) < 1e-4f), $"top moved up by 1.5 (got {top.Vertices().First().P.Y})");
    Assert.True(mesh.IsClosed() && mesh.IsManifold(), $"still closed + manifold (got {$"{mesh.IsClosed()}/{mesh.IsManifold()}"})");

    cmd.Undo();
    Assert.True(top.Vertices().All(v => MathF.Abs(v.P.Y - yBefore) < 1e-4f), $"undo restores height (got {top.Vertices().First().P.Y})");
    }

    /// <summary>InferenceEngine</summary>
    [Fact]
    public void InferenceEngine_snaps_to_references()
    {
    var mesh = new Mesh();
    MeshBuilder.Box(mesh, Vector3.Zero, new Vector3(2, 2, 2), new Vector3(200, 200, 200)); // corners at +/-1
    mesh.Twin();
    var eng = new InferenceEngine();
    const float tol = 0.25f;

    // Endpoint: aim at the corner (1,1,1).
    var rEnd = eng.Infer(mesh, new Ray(new Vector3(1.05f, 1f, 5f), new Vector3(1, 1, 1) - new Vector3(1.05f, 1f, 5f)), null, tol, null, false);
    Assert.True(rEnd.Type == SnapType.Endpoint && Vector3.Distance(rEnd.WorldPoint, new Vector3(1, 1, 1)) < 1e-3f, $"near corner -> Endpoint (got {rEnd.Type})");

    // Midpoint: aim horizontally at (1,1,0), the mid of the vertical edge (1,1,-1)-(1,1,1).
    var rMid = eng.Infer(mesh, new Ray(new Vector3(5, 1, 0), new Vector3(-1, 0, 0)), null, tol, null, false);
    Assert.True(rMid.Type == SnapType.Midpoint && Vector3.Distance(rMid.WorldPoint, new Vector3(1, 1, 0)) < 1e-3f, $"near edge mid -> Midpoint (got {rMid.Type})");

    // OnFace: aim at the +X face center (1,0,0).
    var rFace = eng.Infer(mesh, new Ray(new Vector3(5, 0, 0), new Vector3(-1, 0, 0)), null, tol, null, false);
    Assert.True(rFace.Type == SnapType.OnFace && Vector3.Distance(rFace.WorldPoint, new Vector3(1, 0, 0)) < 1e-3f, $"through face center -> OnFace (got {rFace.Type})");

    // AxisLock: empty mesh, drawing on ground, direction ~1deg off +X from origin.
    var empty = new Mesh();
    var rAxis = eng.Infer(empty, new Ray(new Vector3(5, 5, 0.1f), new Vector3(0, -1, 0)), Vector3.Zero, tol, (Vector3.Zero, Vector3.UnitY), true);
    Assert.True(rAxis.Type == SnapType.AxisLock && MathF.Abs(rAxis.WorldPoint.Z) < 1e-3f && rAxis.Direction.X > 0.9f, $"near +X axis -> AxisLock (got {rAxis.Type})");
    }

    /// <summary>VCB typed values</summary>
    [Fact]
    public void VCB_typed_values()
    {
    // Push/Pull a standalone face by an exact distance.
    var scene = new Scene();
    var cam = new Camera { ViewportWidth = 1000, ViewportHeight = 1000, Target = new Vector3(1, 0, 1), Distance = 12, AzimuthDeg = 45, ElevationDeg = 60 };
    var ctx = new ToolContext(scene, cam, new Picker(scene.Mesh), new Selection(), new History(), () => { }, () => { });
    var face = scene.Mesh.MakeFace([new(0, 0, 0), new(2, 0, 0), new(2, 0, 2), new(0, 0, 2)]);
    var pp = new PushPullTool();
    pp.Activate(ctx);
    cam.Project(face.Centroid(), out var fs);
    pp.OnMouseDown(ctx, MouseButtons.Left, new Point((int)fs.X, (int)fs.Y), Keys.None); // start on the face
    Assert.True(pp.WantsValue, $"push/pull wants a value (got {pp.WantsValue})");
    bool applied = pp.TryApplyValue(ctx, "3");
    Assert.True(applied && scene.Mesh.Faces.Count == 6, $"typed distance extrudes into a box (got {$"{applied}/{scene.Mesh.Faces.Count}"})");

    // Line tool: type a length to place an exact-distance point.
    var s2 = new Scene();
    var c2 = new Camera { ViewportWidth = 1000, ViewportHeight = 1000, Target = Vector3.Zero, Distance = 12, AzimuthDeg = 40, ElevationDeg = 55 };
    var ctx2 = new ToolContext(s2, c2, new Picker(s2.Mesh), new Selection(), new History(), () => { }, () => { });
    var line = new LineTool();
    line.Activate(ctx2);
    Point P(Vector3 w) { c2.Project(w, out var s); return new Point((int)s.X, (int)s.Y); }
    line.OnMouseDown(ctx2, MouseButtons.Left, P(Vector3.Zero), Keys.None); // start at origin
    line.OnMouseMove(ctx2, P(new Vector3(3, 0, 0)), Keys.None);            // aim down +X
    bool lineApplied = line.TryApplyValue(ctx2, "5");
    bool len5 = s2.Mesh.Wires.Count == 1
        && MathF.Abs(Vector3.Distance(s2.Mesh.Wires[0].A.P, s2.Mesh.Wires[0].B.P) - 5f) < 1e-3f;
    Assert.True(lineApplied && len5, $"typed length makes an exact 5-unit edge (got {$"{lineApplied}/{s2.Mesh.Wires.Count}"})");
    }

    /// <summary>Tessellation</summary>
    [Fact]
    public void Tessellation_triangulates_polygons()
    {
    Vector3 p0 = new(0, 0, 0), p1 = new(0, 10, 5), p2 = new(10, 10, -5), p3 = new(10, 0, 0);
    var bez = Tessellation.FlattenBezier(p0, p1, p2, p3, 0.05f);
    Assert.True(Vector3.Distance(bez[0], p0) < 1e-4f && Vector3.Distance(bez[^1], p3) < 1e-4f, $"bezier starts/ends at endpoints (got {bez.Count})");
    var bezTight = Tessellation.FlattenBezier(p0, p1, p2, p3, 0.001f);
    Assert.True(bezTight.Count > bez.Count, $"tighter tolerance -> more segments (got {$"{bez.Count} -> {bezTight.Count}"})");

    bool arcOk = Tessellation.Arc3Point(new(1, 0, 0), new(0, 1, 1), new(-1, 0, 2), 64, out var arc);
    Assert.True(arcOk && Vector3.Distance(arc[0], new(1, 0, 0)) < 1e-3f && Vector3.Distance(arc[^1], new(-1, 0, 2)) < 1e-3f, $"3-point arc hits endpoints (got {arc.Count})");
    bool passesB = arc.Any(q => Vector3.Distance(q, new Vector3(0, 1, 1)) < 0.1f);
    Assert.True(passesB, $"3-point arc passes through the middle point (got {passesB})");

    var ngon = Tessellation.Polygon(Vector3.Zero, Vector3.UnitX, Vector3.UnitZ, 5f, 24);
    Assert.True(ngon.All(q => MathF.Abs(q.Length() - 5f) < 1e-3f) && ngon.Count == 24, $"N-gon vertices on radius (got {ngon.Count})");

    // Arc tool: half-circle inference snaps the bulge so the chord becomes a diameter.
    Vector3 aS = new(-2, 0, 0), aE = new(2, 0, 0), nrm = Vector3.UnitY;
    // A near-semicircle bulge (height ~1.95, exact radius = 2) snaps; the snapped point gives a true semicircle.
    var snapped = ArcTool.SnapSemicircle(aS, aE, nrm, new Vector3(0, 0, 1.95f), tol: 0.2f, out bool semi);
    Assert.True(semi, $"half-circle inference fires near a semicircle (got {semi})");
    Tessellation.Arc3Point(aS, snapped, aE, 64, out var semiArc);
    Vector3 ctr = 0.5f * (aS + aE);
    float r = Vector3.Distance(aS, ctr);
    bool onCircle = semiArc.All(q => MathF.Abs(Vector3.Distance(q, ctr) - r) < 1e-3f);
    Assert.True(onCircle, $"snapped semicircle: chord midpoint is the circle center (got {$"r={r:F3}"})");
    // Far-off bulge (height 0.5, far from radius 2) does not snap and is returned unchanged.
    var unsnapped = ArcTool.SnapSemicircle(aS, aE, nrm, new Vector3(0, 0, 0.5f), tol: 0.2f, out bool semi2);
    Assert.True(!semi2 && Vector3.Distance(unsnapped, new Vector3(0, 0, 0.5f)) < 1e-6f, $"no half-circle snap when far from a semicircle (got {semi2})");
    }

    /// <summary>Edit commands</summary>
    [Fact]
    public void Edit_commands()
    {
    var mesh = new Mesh();
    var v = mesh.AddVertex(new Vector3(1, 0, 0));
    var move = new MoveCommand([v], new Vector3(0, 2, 0));
    move.Do();
    Assert.True(Vector3.Distance(v.P, new Vector3(1, 2, 0)) < 1e-4f, $"move translates (got {v.P})");
    move.Undo();
    Assert.True(Vector3.Distance(v.P, new Vector3(1, 0, 0)) < 1e-4f, $"move undo restores (got {v.P})");

    var rot = new RotateCommand([v], Vector3.Zero, Vector3.UnitY, MathF.PI / 2);
    rot.Do();
    Assert.True(MathF.Abs(v.P.Length() - 1f) < 1e-4f && Vector3.Distance(v.P, new Vector3(1, 0, 0)) > 0.5f, $"rotate keeps radius, moves point (got {v.P})");
    rot.Undo();
    Assert.True(Vector3.Distance(v.P, new Vector3(1, 0, 0)) < 1e-4f, $"rotate undo restores (got {v.P})");

    var box = new Mesh();
    MeshBuilder.Box(box, Vector3.Zero, new Vector3(2, 2, 2), new Vector3(200, 200, 200));
    box.Twin();
    var del = new DeleteCommand(box, [box.Faces[0]], []);
    del.Do();
    Assert.True(box.Faces.Count == 5, $"delete removes a face (6->5) (got {box.Faces.Count})");
    del.Undo();
    Assert.True(box.Faces.Count == 6, $"delete undo restores (->6) (got {box.Faces.Count})");
    del.Do();
    Assert.True(box.Faces.Count == 5, $"delete redo works (->5) (got {box.Faces.Count})");

    var wm = new Mesh();
    var poly = new AddPolylineCommand(wm, [new(0, 0, 0), new(1, 0, 0), new(2, 0, 0)], closed: false);
    poly.Do();
    Assert.True(wm.Wires.Count == 2, $"polyline adds 2 wires (got {wm.Wires.Count})");
    poly.Undo();
    Assert.True(wm.Wires.Count == 0 && wm.Vertices.Count == 0, $"polyline undo clears (got {$"{wm.Wires.Count}/{wm.Vertices.Count}"})");
    }

    /// <summary>SceneIO round-trip</summary>
    [Fact]
    public void SceneIO_round_trip()
    {
    var scene = new Scene();
    MeshBuilder.Box(scene.Mesh, new Vector3(0, 1, 0), new Vector3(2, 2, 2), new Vector3(200, 120, 90));
    scene.Mesh.AddWire(scene.Mesh.AddVertex(new Vector3(5, 0, 0)), scene.Mesh.AddVertex(new Vector3(6, 0, 0)));
    scene.Mesh.Twin();
    var cam = new Camera { AzimuthDeg = 33, ElevationDeg = 22, Distance = 15, Target = new Vector3(1, 2, 3), Orthographic = true };

    string json = SceneIO.Save(scene, cam);
    var s2 = new Scene();
    var c2 = new Camera();
    SceneIO.Load(json, s2, c2);

    Assert.True(s2.Mesh.Vertices.Count == 10, $"vertices preserved (got {s2.Mesh.Vertices.Count})");
    Assert.True(s2.Mesh.Faces.Count == 6, $"faces preserved (got {s2.Mesh.Faces.Count})");
    Assert.True(s2.Mesh.Wires.Count == 1, $"wires preserved (got {s2.Mesh.Wires.Count})");
    Assert.True(s2.Mesh.IsClosed() && s2.Mesh.IsManifold(), $"box still closed + manifold (got {$"{s2.Mesh.IsClosed()}/{s2.Mesh.IsManifold()}"})");
    Assert.True(Vector3.Distance(s2.Mesh.Faces[0].Color, new Vector3(200, 120, 90)) < 1e-3f, $"face color preserved (got {s2.Mesh.Faces[0].Color})");
    Assert.True(MathF.Abs(c2.AzimuthDeg - 33) < 1e-3f && MathF.Abs(c2.Distance - 15) < 1e-3f && c2.Orthographic && Vector3.Distance(c2.Target, new Vector3(1, 2, 3)) < 1e-3f, $"camera preserved (got {$"{c2.AzimuthDeg}/{c2.Distance}/{c2.Orthographic}"})");
    }

    /// <summary>MeshExport</summary>
    [Fact]
    public void MeshExport_writes_obj_and_stl()
    {
    var scene = new Scene();
    MeshBuilder.Box(scene.Mesh, new Vector3(0.5f, 0.25f, 0), new Vector3(2, 1.5f, 2), new Vector3(200, 200, 200));
    scene.Mesh.Twin();

    var obj = MeshExport.ToObj(scene).Split('\n');
    Assert.True(obj.Count(l => l.StartsWith("v ")) == 8, $"OBJ has 8 vertices (got {obj.Count(l => l.StartsWith("v "))})");
    Assert.True(obj.Count(l => l.StartsWith("f ")) == 6, $"OBJ has 6 faces (got {obj.Count(l => l.StartsWith("f "))})");
    Assert.True(!string.Join("\n", obj).Contains(','), $"OBJ uses invariant '.' decimals (no commas) (got {"no commas"})");

    var stl = MeshExport.ToStl(scene);
    int facets = stl.Split("facet normal").Length - 1;
    Assert.True(facets == 12, $"STL triangulates 6 quads into 12 facets (got {facets})");
    Assert.True(stl.StartsWith("solid") && stl.TrimEnd().EndsWith("endsolid ThreeDee"), $"STL is well-formed (got {"solid…endsolid"})");
    }

    /// <summary>LoopFinder</summary>
    [Fact]
    public void LoopFinder_closes_wire_loops()
    {
    var m = new Mesh();
    var v0 = m.AddVertex(new Vector3(0, 0, 0));
    var v1 = m.AddVertex(new Vector3(2, 0, 0));
    var v2 = m.AddVertex(new Vector3(1, 0, 2));
    m.AddWire(v0, v1); m.AddWire(v1, v2); m.AddWire(v2, v0);
    bool ok = LoopFinder.TryFindFillableLoop(m, v2, v0, out var loop, out _);
    Assert.True(ok && loop.Count == 3, $"triangle of wires yields a 3-vertex loop (got {(ok ? loop.Count : -1)})");

    // An open chain (no closing edge) is not fillable.
    var m2 = new Mesh();
    var a = m2.AddVertex(new Vector3(0, 0, 0));
    var b = m2.AddVertex(new Vector3(1, 0, 0));
    m2.AddWire(a, b);
    Assert.True(!LoopFinder.TryFindFillableLoop(m2, a, b, out _, out _), $"open chain is not fillable (got {"no loop"})");
    }

    /// <summary>Arc + line -> auto face</summary>
    [Fact]
    public void Arc_line_auto_face()
    {
    var scene = new Scene();
    var cam = new Camera { ViewportWidth = 1000, ViewportHeight = 1000, Target = Vector3.Zero, Distance = 12, AzimuthDeg = 40, ElevationDeg = 50 };
    var ctx = new ToolContext(scene, cam, new Picker(scene.Mesh), new Selection(), new History(), () => { }, () => { });
    Point P(Vector3 w) { cam.Project(w, out var s); return new Point((int)s.X, (int)s.Y); }

    var arc = new ArcTool(); arc.Activate(ctx);
    arc.OnMouseDown(ctx, MouseButtons.Left, P(new Vector3(-2, 0, 0)), Keys.None);
    arc.OnMouseDown(ctx, MouseButtons.Left, P(new Vector3(2, 0, 0)), Keys.None);
    arc.OnMouseDown(ctx, MouseButtons.Left, P(new Vector3(0, 0, 2)), Keys.None); // bulge -> arc committed
    Assert.True(scene.Mesh.Faces.Count == 0 && scene.Mesh.Wires.Count > 0, $"arc made wires, no face yet (got {$"{scene.Mesh.Faces.Count}/{scene.Mesh.Wires.Count}"})");

    var line = new LineTool(); line.Activate(ctx);
    line.OnMouseDown(ctx, MouseButtons.Left, P(new Vector3(-2, 0, 0)), Keys.None); // snap to arc start
    line.OnMouseDown(ctx, MouseButtons.Left, P(new Vector3(2, 0, 0)), Keys.None);  // close -> fill
    Assert.True(scene.Mesh.Faces.Count == 1, $"closing line fills a face (got {scene.Mesh.Faces.Count})");
    }

    /// <summary>FollowMeCommand</summary>
    [Fact]
    public void FollowMeCommand_sweeps_a_profile()
    {
    var scene = new Scene();
    var prof = scene.Mesh.MakeFace([new(0, -0.5f, -0.5f), new(0, 0.5f, -0.5f), new(0, 0.5f, 0.5f), new(0, -0.5f, 0.5f)]);
    var path = new List<Vector3> { new(0, 0, 0), new(3, 0, 0), new(3, 0, 3) }; // L-shaped, 2 segments

    var cmd = new FollowMeCommand(scene.Mesh, prof, path);
    cmd.Do();
    // 1 profile + (2 stations × 4) side faces + 2 caps = 11 faces; 4 + 3×4 = 16 vertices.
    Assert.True(scene.Mesh.Faces.Count == 11, $"sweep builds side + cap faces (got {scene.Mesh.Faces.Count})");
    Assert.True(scene.Mesh.Vertices.Count == 16, $"sweep builds ring vertices (got {scene.Mesh.Vertices.Count})");

    cmd.Undo();
    Assert.True(scene.Mesh.Faces.Count == 1 && scene.Mesh.Vertices.Count == 4, $"follow-me undo restores profile only (got {$"{scene.Mesh.Faces.Count}/{scene.Mesh.Vertices.Count}"})");
    }

    /// <summary>Follow-Me -> sphere</summary>
    [Fact]
    public void Follow_Me_sphere()
    {
    const float R = 2f;
    var scene = new Scene();
    // Profile: a full circle in the XY plane, centered at the origin (the revolution axis is a diameter).
    var prof = scene.Mesh.MakeFace(Tessellation.Polygon(Vector3.Zero, Vector3.UnitX, Vector3.UnitY, R, 24));
    // Path: a circle in the XZ plane (the equator), closed.
    var path = Tessellation.Polygon(Vector3.Zero, Vector3.UnitX, Vector3.UnitZ, R, 48);
    path.Add(path[0]); // mark closed

    new FollowMeCommand(scene.Mesh, prof, path).Do();

    float maxRelErr = 0f;
    foreach (var v in scene.Mesh.Vertices)
        maxRelErr = MathF.Max(maxRelErr, MathF.Abs(v.P.Length() - R) / R);
    Assert.True(maxRelErr < 1e-2f, $"revolution surface is a sphere (maxRelErr < 1e-2) (got {$"{maxRelErr:E3}"})");
    Assert.True(scene.Mesh.Faces.Count > 100, $"sweep produced faces (got {scene.Mesh.Faces.Count})");
    }

    /// <summary>Mesh.ConnectedEdgePath</summary>
    [Fact]
    public void Mesh_ConnectedEdgePath()
    {
    var m = new Mesh();
    new AddPolylineCommand(m, [new(0, 0, 0), new(1, 0, 0), new(2, 0, 0.5f), new(3, 0, 1), new(4, 0, 1.5f)], closed: false).Do();
    m.AddWire(m.AddVertex(new Vector3(10, 0, 0)), m.AddVertex(new Vector3(11, 0, 0))); // unrelated

    var seed = new EdgeKey(m.Vertices[1], m.Vertices[2]);
    int chain = m.ConnectedEdgePath(seed).Count();
    Assert.True(chain == 4, $"flood recovers the whole 4-edge chain (got {chain})");

    // Add a branch at vertex[2] -> it becomes a junction (valence 3); flood must stop there.
    m.AddWire(m.Vertices[2], m.AddVertex(new Vector3(2, 0, 3)));
    int capped = m.ConnectedEdgePath(new EdgeKey(m.Vertices[0], m.Vertices[1])).Count();
    Assert.True(capped == 2, $"flood stops at a junction (valence>2) (got {capped})");
    }

    /// <summary>SelectTool double-click whole curve</summary>
    [Fact]
    public void SelectTool_double_click_whole_curve()
    {
    var scene = new Scene();
    var cam = new Camera { ViewportWidth = 1000, ViewportHeight = 1000, Target = new Vector3(2, 0, 0), Distance = 12, AzimuthDeg = 40, ElevationDeg = 55 };
    var ctx = new ToolContext(scene, cam, new Picker(scene.Mesh), new Selection(), new History(), () => { }, () => { });
    Vector3[] arc = [new(0, 0, 0), new(1, 0, 0.4f), new(2, 0, 0.6f), new(3, 0, 0.4f), new(4, 0, 0)];
    new AddPolylineCommand(scene.Mesh, arc, false).Do(); // 4 wire edges

    var sel = new SelectTool();
    sel.Activate(ctx);
    Vector3 midEdge = (arc[1] + arc[2]) * 0.5f; // a point on the 2nd segment
    cam.Project(midEdge, out var s);
    sel.OnDoubleClick(ctx, new Point((int)s.X, (int)s.Y), Keys.None);
    Assert.True(ctx.Selection.Edges.Count == 4, $"double-click selects all 4 arc edges (got {ctx.Selection.Edges.Count})");
    }

}
