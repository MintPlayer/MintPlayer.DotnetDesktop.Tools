// Hand-rolled regression harness (no third-party test framework — BCL only, like the spikes).
// Run: dotnet run --project tests/MintPlayer.ThreeDee.Tests   (exit code 0 = all pass)

using System.Numerics;
using MintPlayer.ThreeDee;
using MintPlayer.ThreeDee.Commands;
using MintPlayer.ThreeDee.Geometry;
using MintPlayer.ThreeDee.Inference;
using MintPlayer.ThreeDee.Rendering;
using MintPlayer.ThreeDee.Tools;

int fails = 0;

int Check(string name, bool ok, object detail)
{
    Console.WriteLine($"  [{(ok ? "PASS" : "FAIL")}] {name}  (got {detail})");
    return ok ? 0 : 1;
}

// === Geometry: closed box built via MeshBuilder (F3 half-edge invariants) ===
{
    Console.WriteLine("=== MeshBuilder.Box -> closed cube ===");
    var mesh = new Mesh();
    MeshBuilder.Box(mesh, Vector3.Zero, new Vector3(2, 2, 2), new Vector3(200, 200, 200));
    mesh.Twin();

    fails += Check("verts == 8", mesh.Vertices.Count == 8, mesh.Vertices.Count);
    fails += Check("faces == 6", mesh.Faces.Count == 6, mesh.Faces.Count);
    fails += Check("edges == 12", mesh.EdgeCount() == 12, mesh.EdgeCount());
    fails += Check("closed (0 boundary edges)", mesh.IsClosed(), mesh.BoundaryEdgeCount());
    fails += Check("manifold", mesh.IsManifold(), mesh.IsManifold());
    int euler = mesh.Vertices.Count - mesh.EdgeCount() + mesh.Faces.Count;
    fails += Check("Euler V-E+F == 2", euler == 2, euler);
    fails += Check("all normals outward", mesh.AllNormalsOutward(out int bad), bad);
    Console.WriteLine();
}

// === Geometry: extrude a quad -> box (Push/Pull kernel, mirrors spike S5) ===
{
    Console.WriteLine("=== Extrude quad -> box ===");
    var mesh = new Mesh();
    var f = mesh.MakeFace([new(0, 0, 0), new(1, 0, 0), new(1, 1, 0), new(0, 1, 0)]);
    fails += Check("source quad is open (4 boundary edges)", mesh.BoundaryEdgeCount() == 4, mesh.BoundaryEdgeCount());
    mesh.Extrude(f, 1.0f);

    fails += Check("verts == 8", mesh.Vertices.Count == 8, mesh.Vertices.Count);
    fails += Check("faces == 6", mesh.Faces.Count == 6, mesh.Faces.Count);
    fails += Check("edges == 12", mesh.EdgeCount() == 12, mesh.EdgeCount());
    fails += Check("closed", mesh.IsClosed(), mesh.BoundaryEdgeCount());
    fails += Check("manifold", mesh.IsManifold(), mesh.IsManifold());
    fails += Check("all normals outward", mesh.AllNormalsOutward(out int bad), bad);
    Console.WriteLine();
}

// === Geometry: extrude a triangle -> prism ===
{
    Console.WriteLine("=== Extrude triangle -> prism ===");
    var mesh = new Mesh();
    var f = mesh.MakeFace([new(0, 0, 0), new(1, 0, 0), new(0, 1, 0)]);
    mesh.Extrude(f, 2.0f);

    fails += Check("verts == 6", mesh.Vertices.Count == 6, mesh.Vertices.Count);
    fails += Check("faces == 5", mesh.Faces.Count == 5, mesh.Faces.Count);
    fails += Check("edges == 9", mesh.EdgeCount() == 9, mesh.EdgeCount());
    fails += Check("closed", mesh.IsClosed(), mesh.BoundaryEdgeCount());
    fails += Check("all normals outward", mesh.AllNormalsOutward(out int bad), bad);
    Console.WriteLine();
}

// === Scene: render-mesh emission matches face topology ===
{
    Console.WriteLine("=== Scene.BuildRenderMeshes ===");
    var scene = DemoScene.Build();
    var meshes = scene.BuildRenderMeshes();
    fails += Check("one render mesh per face", meshes.Count == scene.Mesh.Faces.Count, $"{meshes.Count} vs {scene.Mesh.Faces.Count}");

    bool fanOk = true, edgeOk = true;
    foreach (var rm in meshes)
    {
        int n = rm.Vertices.Length;
        if (rm.Triangles.Length != (n - 2) * 3) fanOk = false;
        if (rm.Edges is null || rm.Edges.Length != n * 2) edgeOk = false;
    }
    fails += Check("each face fan-triangulated to (n-2) tris", fanOk, fanOk);
    fails += Check("each face emits an n-edge boundary ring", edgeOk, edgeOk);
    fails += Check("scene bounds non-empty", !scene.Bounds().IsEmpty, scene.Bounds().IsEmpty);
    Console.WriteLine();
}

// === Picking (F4): ray-cast against a cube ===
{
    Console.WriteLine("=== Picker: ray-triangle face pick ===");
    var mesh = new Mesh();
    MeshBuilder.Box(mesh, Vector3.Zero, new Vector3(2, 2, 2), new Vector3(200, 200, 200));
    mesh.Twin();
    var picker = new MintPlayer.ThreeDee.Rendering.Picker(mesh);

    // Ray straight down -Z from +Z hits the +Z face (z=1) at t=4, point (0,0,1).
    var ray = new Ray(new Vector3(0, 0, 5), new Vector3(0, 0, -1));
    var face = picker.PickFace(ray, out Vector3 p, out float t);
    fails += Check("ray hits a face", face is not null, face is not null);
    fails += Check("hit point on +Z face", Vector3.Distance(p, new Vector3(0, 0, 1)) < 1e-3f, p);
    fails += Check("hit distance t == 4", MathF.Abs(t - 4f) < 1e-3f, t);

    var miss = picker.PickFace(new Ray(new Vector3(5, 5, 5), new Vector3(1, 1, 1)), out _, out _);
    fails += Check("ray pointing away misses", miss is null, miss is null);
    Console.WriteLine();
}

// === Picking via camera round-trip: edge vs face discrimination ===
{
    Console.WriteLine("=== Picker: camera unproject -> edge / face ===");
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
    fails += Check("midpoint ray picks an edge", ePick.Kind == MintPlayer.ThreeDee.Rendering.PickKind.Edge, ePick.Kind);
    fails += Check("picks the targeted edge", ePick.Edge.Equals(new EdgeKey(ea, eb)), "edge match");

    // Aiming through a face centroid should pick that face.
    var top = mesh.Faces.First(f => f.Normal().Y > 0.9f); // +Y top face
    cam.Project(top.Centroid(), out var fs);
    var fPick = picker.Pick(cam.Unproject(fs), cam);
    fails += Check("centroid ray picks a face", fPick.Kind == MintPlayer.ThreeDee.Rendering.PickKind.Face, fPick.Kind);
    fails += Check("picks the targeted face", ReferenceEquals(fPick.Face, top), "face match");
    Console.WriteLine();
}

// === Selection model ===
{
    Console.WriteLine("=== Selection apply/toggle ===");
    var mesh = new Mesh();
    MeshBuilder.Box(mesh, Vector3.Zero, new Vector3(2, 2, 2), new Vector3(200, 200, 200));
    mesh.Twin();
    var sel = new Selection();
    var f0 = mesh.Faces[0];

    sel.Apply(MintPlayer.ThreeDee.Rendering.PickResult.OnFace(f0, Vector3.Zero, 1f), toggle: false);
    fails += Check("replace-select adds one face", sel.Count == 1 && sel.Contains(f0), sel.Count);
    sel.Apply(MintPlayer.ThreeDee.Rendering.PickResult.OnFace(f0, Vector3.Zero, 1f), toggle: true);
    fails += Check("toggle removes it", sel.IsEmpty, sel.Count);
    Console.WriteLine();
}

// === Commands: CreateFaceCommand + History undo/redo ===
{
    Console.WriteLine("=== CreateFaceCommand + History ===");
    var scene = new Scene();
    var history = new History();
    // A quad on the ground; CreateFaceCommand should wind it so the normal matches +Y.
    Vector3[] pts = [new(0, 0, 0), new(2, 0, 0), new(2, 0, 2), new(0, 0, 2)];
    history.Execute(new CreateFaceCommand(scene.Mesh, pts, new Vector3(200, 200, 200), Vector3.UnitY));

    fails += Check("face created", scene.Mesh.Faces.Count == 1, scene.Mesh.Faces.Count);
    fails += Check("4 vertices added", scene.Mesh.Vertices.Count == 4, scene.Mesh.Vertices.Count);
    fails += Check("normal matches intended +Y", Vector3.Dot(scene.Mesh.Faces[0].Normal(), Vector3.UnitY) > 0.99f, scene.Mesh.Faces[0].Normal());

    history.Undo();
    fails += Check("undo removes face + verts", scene.Mesh.Faces.Count == 0 && scene.Mesh.Vertices.Count == 0, $"{scene.Mesh.Faces.Count}/{scene.Mesh.Vertices.Count}");
    history.Redo();
    fails += Check("redo restores face", scene.Mesh.Faces.Count == 1 && scene.Mesh.Vertices.Count == 4, scene.Mesh.Faces.Count);
    fails += Check("CanUndo true, CanRedo false", history.CanUndo && !history.CanRedo, $"{history.CanUndo}/{history.CanRedo}");
    Console.WriteLine();
}

// === Tool integration: drive RectangleTool through a ToolContext (no window) ===
{
    Console.WriteLine("=== RectangleTool end-to-end ===");
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

    fails += Check("rectangle created a face", scene.Mesh.Faces.Count == 1, scene.Mesh.Faces.Count);
    fails += Check("face is a quad", scene.Mesh.Faces.Count == 1 && scene.Mesh.Faces[0].SideCount() == 4, scene.Mesh.Faces.Count == 1 ? scene.Mesh.Faces[0].SideCount() : -1);
    fails += Check("face lies on the ground plane", scene.Mesh.Faces.Count == 1 && scene.Mesh.Faces[0].Vertices().All(v => MathF.Abs(v.P.Y) < 1e-3f), "y≈0");
    fails += Check("recorded on the undo stack", ctx.History.CanUndo, ctx.History.CanUndo);
    Console.WriteLine();
}

// === Wire edges: AddEdgeCommand welds + undo ===
{
    Console.WriteLine("=== AddEdgeCommand (wire edges) ===");
    var mesh = new Mesh();
    var c1 = new AddEdgeCommand(mesh, new Vector3(0, 0, 0), new Vector3(1, 0, 0));
    var c2 = new AddEdgeCommand(mesh, new Vector3(1, 0, 0), new Vector3(2, 0, 0));
    c1.Do(); c2.Do();
    fails += Check("two wires created", mesh.Wires.Count == 2, mesh.Wires.Count);
    fails += Check("shared endpoint welded (3 verts)", mesh.Vertices.Count == 3, mesh.Vertices.Count);
    fails += Check("UniqueEdges counts 2", mesh.UniqueEdges().Count() == 2, mesh.UniqueEdges().Count());

    c2.Undo();
    fails += Check("undo c2 leaves 1 wire", mesh.Wires.Count == 1, mesh.Wires.Count);
    fails += Check("shared vertex kept (2 verts)", mesh.Vertices.Count == 2, mesh.Vertices.Count);
    c1.Undo();
    fails += Check("undo c1 empties mesh", mesh.Wires.Count == 0 && mesh.Vertices.Count == 0, $"{mesh.Wires.Count}/{mesh.Vertices.Count}");
    Console.WriteLine();
}

// === LineTool end-to-end: draw a triangle and close it into a face ===
{
    Console.WriteLine("=== LineTool draw + close ===");
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

    fails += Check("two wire edges drawn", scene.Mesh.Wires.Count == 2, scene.Mesh.Wires.Count);
    fails += Check("loop closed into one face", scene.Mesh.Faces.Count == 1, scene.Mesh.Faces.Count);
    fails += Check("face is a triangle", scene.Mesh.Faces.Count == 1 && scene.Mesh.Faces[0].SideCount() == 3, scene.Mesh.Faces.Count == 1 ? scene.Mesh.Faces[0].SideCount() : -1);
    fails += Check("vertices welded (3 total)", scene.Mesh.Vertices.Count == 3, scene.Mesh.Vertices.Count);
    fails += Check("UniqueEdges dedups wire+face (3)", scene.Mesh.UniqueEdges().Count() == 3, scene.Mesh.UniqueEdges().Count());

    // Switching away mid-run keeps committed edges (the original bug report).
    tool.Activate(ctx);
    var t2 = new LineTool(); t2.Activate(ctx);
    t2.OnMouseDown(ctx, MouseButtons.Left, Screen(new Vector3(-2, 0, -2)), Keys.None);
    t2.OnMouseDown(ctx, MouseButtons.Left, Screen(new Vector3(-2, 0, 2)), Keys.None); // one edge committed
    int wiresBefore = scene.Mesh.Wires.Count;
    new SelectTool().Activate(ctx); // "switch to Select"
    fails += Check("switching tools keeps drawn edges", scene.Mesh.Wires.Count == wiresBefore && wiresBefore == 3, scene.Mesh.Wires.Count);
    Console.WriteLine();
}

// === Push/Pull: extrude a standalone face into a solid (+ undo) ===
{
    Console.WriteLine("=== PushPull extrude (standalone face -> box) ===");
    var mesh = new Mesh();
    var f = mesh.MakeFace([new(0, 0, 0), new(2, 0, 0), new(2, 0, 2), new(0, 0, 2)]);
    fails += Check("starts standalone", Mesh.IsStandalone(f), Mesh.IsStandalone(f));

    var cmd = new PushPullCommand(mesh, f, 3f);
    cmd.Do();
    fails += Check("becomes a closed box (6 faces)", mesh.Faces.Count == 6, mesh.Faces.Count);
    fails += Check("8 vertices", mesh.Vertices.Count == 8, mesh.Vertices.Count);
    fails += Check("closed + manifold", mesh.IsClosed() && mesh.IsManifold(), $"{mesh.IsClosed()}/{mesh.IsManifold()}");
    fails += Check("all normals outward", mesh.AllNormalsOutward(out int bad), bad);
    int euler = mesh.Vertices.Count - mesh.EdgeCount() + mesh.Faces.Count;
    fails += Check("Euler == 2", euler == 2, euler);

    cmd.Undo();
    fails += Check("undo restores the flat face", mesh.Faces.Count == 1 && mesh.Vertices.Count == 4, $"{mesh.Faces.Count}/{mesh.Vertices.Count}");
    Console.WriteLine();
}

// === Push/Pull: stretch a face that's part of a solid (+ undo) ===
{
    Console.WriteLine("=== PushPull stretch (solid face) ===");
    var mesh = new Mesh();
    MeshBuilder.Box(mesh, new Vector3(0, 1, 0), new Vector3(2, 2, 2), new Vector3(200, 200, 200));
    mesh.Twin();
    var top = mesh.Faces.First(f => f.Normal().Y > 0.9f);
    fails += Check("top face is not standalone", !Mesh.IsStandalone(top), Mesh.IsStandalone(top));
    float yBefore = top.Vertices().First().P.Y;

    var cmd = new PushPullCommand(mesh, top, 1.5f);
    cmd.Do();
    fails += Check("face count unchanged (stretch, not extrude)", mesh.Faces.Count == 6, mesh.Faces.Count);
    fails += Check("top moved up by 1.5", top.Vertices().All(v => MathF.Abs(v.P.Y - (yBefore + 1.5f)) < 1e-4f), top.Vertices().First().P.Y);
    fails += Check("still closed + manifold", mesh.IsClosed() && mesh.IsManifold(), $"{mesh.IsClosed()}/{mesh.IsManifold()}");

    cmd.Undo();
    fails += Check("undo restores height", top.Vertices().All(v => MathF.Abs(v.P.Y - yBefore) < 1e-4f), top.Vertices().First().P.Y);
    Console.WriteLine();
}

// === Inference engine: snap classification ===
{
    Console.WriteLine("=== InferenceEngine ===");
    var mesh = new Mesh();
    MeshBuilder.Box(mesh, Vector3.Zero, new Vector3(2, 2, 2), new Vector3(200, 200, 200)); // corners at +/-1
    mesh.Twin();
    var eng = new InferenceEngine();
    const float tol = 0.25f;

    // Endpoint: aim at the corner (1,1,1).
    var rEnd = eng.Infer(mesh, new Ray(new Vector3(1.05f, 1f, 5f), new Vector3(1, 1, 1) - new Vector3(1.05f, 1f, 5f)), null, tol, null, false);
    fails += Check("near corner -> Endpoint", rEnd.Type == SnapType.Endpoint && Vector3.Distance(rEnd.WorldPoint, new Vector3(1, 1, 1)) < 1e-3f, rEnd.Type);

    // Midpoint: aim horizontally at (1,1,0), the mid of the vertical edge (1,1,-1)-(1,1,1).
    var rMid = eng.Infer(mesh, new Ray(new Vector3(5, 1, 0), new Vector3(-1, 0, 0)), null, tol, null, false);
    fails += Check("near edge mid -> Midpoint", rMid.Type == SnapType.Midpoint && Vector3.Distance(rMid.WorldPoint, new Vector3(1, 1, 0)) < 1e-3f, rMid.Type);

    // OnFace: aim at the +X face center (1,0,0).
    var rFace = eng.Infer(mesh, new Ray(new Vector3(5, 0, 0), new Vector3(-1, 0, 0)), null, tol, null, false);
    fails += Check("through face center -> OnFace", rFace.Type == SnapType.OnFace && Vector3.Distance(rFace.WorldPoint, new Vector3(1, 0, 0)) < 1e-3f, rFace.Type);

    // AxisLock: empty mesh, drawing on ground, direction ~1deg off +X from origin.
    var empty = new Mesh();
    var rAxis = eng.Infer(empty, new Ray(new Vector3(5, 5, 0.1f), new Vector3(0, -1, 0)), Vector3.Zero, tol, (Vector3.Zero, Vector3.UnitY), true);
    fails += Check("near +X axis -> AxisLock", rAxis.Type == SnapType.AxisLock && MathF.Abs(rAxis.WorldPoint.Z) < 1e-3f && rAxis.Direction.X > 0.9f, rAxis.Type);
    Console.WriteLine();
}

// === VCB: typed values drive tools ===
{
    Console.WriteLine("=== VCB typed values ===");
    // Push/Pull a standalone face by an exact distance.
    var scene = new Scene();
    var cam = new Camera { ViewportWidth = 1000, ViewportHeight = 1000, Target = new Vector3(1, 0, 1), Distance = 12, AzimuthDeg = 45, ElevationDeg = 60 };
    var ctx = new ToolContext(scene, cam, new Picker(scene.Mesh), new Selection(), new History(), () => { }, () => { });
    var face = scene.Mesh.MakeFace([new(0, 0, 0), new(2, 0, 0), new(2, 0, 2), new(0, 0, 2)]);
    var pp = new PushPullTool();
    pp.Activate(ctx);
    cam.Project(face.Centroid(), out var fs);
    pp.OnMouseDown(ctx, MouseButtons.Left, new Point((int)fs.X, (int)fs.Y), Keys.None); // start on the face
    fails += Check("push/pull wants a value", pp.WantsValue, pp.WantsValue);
    bool applied = pp.TryApplyValue(ctx, "3");
    fails += Check("typed distance extrudes into a box", applied && scene.Mesh.Faces.Count == 6, $"{applied}/{scene.Mesh.Faces.Count}");

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
    fails += Check("typed length makes an exact 5-unit edge", lineApplied && len5, $"{lineApplied}/{s2.Mesh.Wires.Count}");
    Console.WriteLine();
}

// === Tessellation (S7) ===
{
    Console.WriteLine("=== Tessellation ===");
    Vector3 p0 = new(0, 0, 0), p1 = new(0, 10, 5), p2 = new(10, 10, -5), p3 = new(10, 0, 0);
    var bez = Tessellation.FlattenBezier(p0, p1, p2, p3, 0.05f);
    fails += Check("bezier starts/ends at endpoints", Vector3.Distance(bez[0], p0) < 1e-4f && Vector3.Distance(bez[^1], p3) < 1e-4f, bez.Count);
    var bezTight = Tessellation.FlattenBezier(p0, p1, p2, p3, 0.001f);
    fails += Check("tighter tolerance -> more segments", bezTight.Count > bez.Count, $"{bez.Count} -> {bezTight.Count}");

    bool arcOk = Tessellation.Arc3Point(new(1, 0, 0), new(0, 1, 1), new(-1, 0, 2), 64, out var arc);
    fails += Check("3-point arc hits endpoints", arcOk && Vector3.Distance(arc[0], new(1, 0, 0)) < 1e-3f && Vector3.Distance(arc[^1], new(-1, 0, 2)) < 1e-3f, arc.Count);
    bool passesB = arc.Any(q => Vector3.Distance(q, new Vector3(0, 1, 1)) < 0.1f);
    fails += Check("3-point arc passes through the middle point", passesB, passesB);

    var ngon = Tessellation.Polygon(Vector3.Zero, Vector3.UnitX, Vector3.UnitZ, 5f, 24);
    fails += Check("N-gon vertices on radius", ngon.All(q => MathF.Abs(q.Length() - 5f) < 1e-3f) && ngon.Count == 24, ngon.Count);

    // Arc tool: half-circle inference snaps the bulge so the chord becomes a diameter.
    Vector3 aS = new(-2, 0, 0), aE = new(2, 0, 0), nrm = Vector3.UnitY;
    // A near-semicircle bulge (height ~1.95, exact radius = 2) snaps; the snapped point gives a true semicircle.
    var snapped = ArcTool.SnapSemicircle(aS, aE, nrm, new Vector3(0, 0, 1.95f), tol: 0.2f, out bool semi);
    fails += Check("half-circle inference fires near a semicircle", semi, semi);
    Tessellation.Arc3Point(aS, snapped, aE, 64, out var semiArc);
    Vector3 ctr = 0.5f * (aS + aE);
    float r = Vector3.Distance(aS, ctr);
    bool onCircle = semiArc.All(q => MathF.Abs(Vector3.Distance(q, ctr) - r) < 1e-3f);
    fails += Check("snapped semicircle: chord midpoint is the circle center", onCircle, $"r={r:F3}");
    // Far-off bulge (height 0.5, far from radius 2) does not snap and is returned unchanged.
    var unsnapped = ArcTool.SnapSemicircle(aS, aE, nrm, new Vector3(0, 0, 0.5f), tol: 0.2f, out bool semi2);
    fails += Check("no half-circle snap when far from a semicircle", !semi2 && Vector3.Distance(unsnapped, new Vector3(0, 0, 0.5f)) < 1e-6f, semi2);
    Console.WriteLine();
}

// === Edit commands (Move / Rotate / Delete / Polyline) ===
{
    Console.WriteLine("=== Edit commands ===");
    var mesh = new Mesh();
    var v = mesh.AddVertex(new Vector3(1, 0, 0));
    var move = new MoveCommand([v], new Vector3(0, 2, 0));
    move.Do();
    fails += Check("move translates", Vector3.Distance(v.P, new Vector3(1, 2, 0)) < 1e-4f, v.P);
    move.Undo();
    fails += Check("move undo restores", Vector3.Distance(v.P, new Vector3(1, 0, 0)) < 1e-4f, v.P);

    var rot = new RotateCommand([v], Vector3.Zero, Vector3.UnitY, MathF.PI / 2);
    rot.Do();
    fails += Check("rotate keeps radius, moves point", MathF.Abs(v.P.Length() - 1f) < 1e-4f && Vector3.Distance(v.P, new Vector3(1, 0, 0)) > 0.5f, v.P);
    rot.Undo();
    fails += Check("rotate undo restores", Vector3.Distance(v.P, new Vector3(1, 0, 0)) < 1e-4f, v.P);

    var box = new Mesh();
    MeshBuilder.Box(box, Vector3.Zero, new Vector3(2, 2, 2), new Vector3(200, 200, 200));
    box.Twin();
    var del = new DeleteCommand(box, [box.Faces[0]], []);
    del.Do();
    fails += Check("delete removes a face (6->5)", box.Faces.Count == 5, box.Faces.Count);
    del.Undo();
    fails += Check("delete undo restores (->6)", box.Faces.Count == 6, box.Faces.Count);
    del.Do();
    fails += Check("delete redo works (->5)", box.Faces.Count == 5, box.Faces.Count);

    var wm = new Mesh();
    var poly = new AddPolylineCommand(wm, [new(0, 0, 0), new(1, 0, 0), new(2, 0, 0)], closed: false);
    poly.Do();
    fails += Check("polyline adds 2 wires", wm.Wires.Count == 2, wm.Wires.Count);
    poly.Undo();
    fails += Check("polyline undo clears", wm.Wires.Count == 0 && wm.Vertices.Count == 0, $"{wm.Wires.Count}/{wm.Vertices.Count}");
    Console.WriteLine();
}

// === Save / Load round-trip (M8) ===
{
    Console.WriteLine("=== SceneIO round-trip ===");
    var scene = new Scene();
    MeshBuilder.Box(scene.Mesh, new Vector3(0, 1, 0), new Vector3(2, 2, 2), new Vector3(200, 120, 90));
    scene.Mesh.AddWire(scene.Mesh.AddVertex(new Vector3(5, 0, 0)), scene.Mesh.AddVertex(new Vector3(6, 0, 0)));
    scene.Mesh.Twin();
    var cam = new Camera { AzimuthDeg = 33, ElevationDeg = 22, Distance = 15, Target = new Vector3(1, 2, 3), Orthographic = true };

    string json = SceneIO.Save(scene, cam);
    var s2 = new Scene();
    var c2 = new Camera();
    SceneIO.Load(json, s2, c2);

    fails += Check("vertices preserved", s2.Mesh.Vertices.Count == 10, s2.Mesh.Vertices.Count);
    fails += Check("faces preserved", s2.Mesh.Faces.Count == 6, s2.Mesh.Faces.Count);
    fails += Check("wires preserved", s2.Mesh.Wires.Count == 1, s2.Mesh.Wires.Count);
    fails += Check("box still closed + manifold", s2.Mesh.IsClosed() && s2.Mesh.IsManifold(), $"{s2.Mesh.IsClosed()}/{s2.Mesh.IsManifold()}");
    fails += Check("face color preserved", Vector3.Distance(s2.Mesh.Faces[0].Color, new Vector3(200, 120, 90)) < 1e-3f, s2.Mesh.Faces[0].Color);
    fails += Check("camera preserved", MathF.Abs(c2.AzimuthDeg - 33) < 1e-3f && MathF.Abs(c2.Distance - 15) < 1e-3f && c2.Orthographic && Vector3.Distance(c2.Target, new Vector3(1, 2, 3)) < 1e-3f, $"{c2.AzimuthDeg}/{c2.Distance}/{c2.Orthographic}");
    Console.WriteLine();
}

// === Export (M9): OBJ + STL ===
{
    Console.WriteLine("=== MeshExport ===");
    var scene = new Scene();
    MeshBuilder.Box(scene.Mesh, new Vector3(0.5f, 0.25f, 0), new Vector3(2, 1.5f, 2), new Vector3(200, 200, 200));
    scene.Mesh.Twin();

    var obj = MeshExport.ToObj(scene).Split('\n');
    fails += Check("OBJ has 8 vertices", obj.Count(l => l.StartsWith("v ")) == 8, obj.Count(l => l.StartsWith("v ")));
    fails += Check("OBJ has 6 faces", obj.Count(l => l.StartsWith("f ")) == 6, obj.Count(l => l.StartsWith("f ")));
    fails += Check("OBJ uses invariant '.' decimals (no commas)", !string.Join("\n", obj).Contains(','), "no commas");

    var stl = MeshExport.ToStl(scene);
    int facets = stl.Split("facet normal").Length - 1;
    fails += Check("STL triangulates 6 quads into 12 facets", facets == 12, facets);
    fails += Check("STL is well-formed", stl.StartsWith("solid") && stl.TrimEnd().EndsWith("endsolid ThreeDee"), "solid…endsolid");
    Console.WriteLine();
}

// === LoopFinder: closed planar wire loop is fillable ===
{
    Console.WriteLine("=== LoopFinder ===");
    var m = new Mesh();
    var v0 = m.AddVertex(new Vector3(0, 0, 0));
    var v1 = m.AddVertex(new Vector3(2, 0, 0));
    var v2 = m.AddVertex(new Vector3(1, 0, 2));
    m.AddWire(v0, v1); m.AddWire(v1, v2); m.AddWire(v2, v0);
    bool ok = LoopFinder.TryFindFillableLoop(m, v2, v0, out var loop, out _);
    fails += Check("triangle of wires yields a 3-vertex loop", ok && loop.Count == 3, ok ? loop.Count : -1);

    // An open chain (no closing edge) is not fillable.
    var m2 = new Mesh();
    var a = m2.AddVertex(new Vector3(0, 0, 0));
    var b = m2.AddVertex(new Vector3(1, 0, 0));
    m2.AddWire(a, b);
    fails += Check("open chain is not fillable", !LoopFinder.TryFindFillableLoop(m2, a, b, out _, out _), "no loop");
    Console.WriteLine();
}

// === Arc + closing line auto-fills a face (the reported scenario) ===
{
    Console.WriteLine("=== Arc + line -> auto face ===");
    var scene = new Scene();
    var cam = new Camera { ViewportWidth = 1000, ViewportHeight = 1000, Target = Vector3.Zero, Distance = 12, AzimuthDeg = 40, ElevationDeg = 50 };
    var ctx = new ToolContext(scene, cam, new Picker(scene.Mesh), new Selection(), new History(), () => { }, () => { });
    Point P(Vector3 w) { cam.Project(w, out var s); return new Point((int)s.X, (int)s.Y); }

    var arc = new ArcTool(); arc.Activate(ctx);
    arc.OnMouseDown(ctx, MouseButtons.Left, P(new Vector3(-2, 0, 0)), Keys.None);
    arc.OnMouseDown(ctx, MouseButtons.Left, P(new Vector3(2, 0, 0)), Keys.None);
    arc.OnMouseDown(ctx, MouseButtons.Left, P(new Vector3(0, 0, 2)), Keys.None); // bulge -> arc committed
    fails += Check("arc made wires, no face yet", scene.Mesh.Faces.Count == 0 && scene.Mesh.Wires.Count > 0, $"{scene.Mesh.Faces.Count}/{scene.Mesh.Wires.Count}");

    var line = new LineTool(); line.Activate(ctx);
    line.OnMouseDown(ctx, MouseButtons.Left, P(new Vector3(-2, 0, 0)), Keys.None); // snap to arc start
    line.OnMouseDown(ctx, MouseButtons.Left, P(new Vector3(2, 0, 0)), Keys.None);  // close -> fill
    fails += Check("closing line fills a face", scene.Mesh.Faces.Count == 1, scene.Mesh.Faces.Count);
    Console.WriteLine();
}

// === Follow-Me sweep ===
{
    Console.WriteLine("=== FollowMeCommand ===");
    var scene = new Scene();
    var prof = scene.Mesh.MakeFace([new(0, -0.5f, -0.5f), new(0, 0.5f, -0.5f), new(0, 0.5f, 0.5f), new(0, -0.5f, 0.5f)]);
    var path = new List<Vector3> { new(0, 0, 0), new(3, 0, 0), new(3, 0, 3) }; // L-shaped, 2 segments

    var cmd = new FollowMeCommand(scene.Mesh, prof, path);
    cmd.Do();
    // 1 profile + (2 stations × 4) side faces + 2 caps = 11 faces; 4 + 3×4 = 16 vertices.
    fails += Check("sweep builds side + cap faces", scene.Mesh.Faces.Count == 11, scene.Mesh.Faces.Count);
    fails += Check("sweep builds ring vertices", scene.Mesh.Vertices.Count == 16, scene.Mesh.Vertices.Count);

    cmd.Undo();
    fails += Check("follow-me undo restores profile only", scene.Mesh.Faces.Count == 1 && scene.Mesh.Vertices.Count == 4, $"{scene.Mesh.Faces.Count}/{scene.Mesh.Vertices.Count}");
    Console.WriteLine();
}

// === Follow-Me revolution makes a sphere (PRD R11.1) ===
{
    Console.WriteLine("=== Follow-Me -> sphere ===");
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
    fails += Check("revolution surface is a sphere (maxRelErr < 1e-2)", maxRelErr < 1e-2f, $"{maxRelErr:E3}");
    fails += Check("sweep produced faces", scene.Mesh.Faces.Count > 100, scene.Mesh.Faces.Count);
    Console.WriteLine();
}

// === Connected-edge-path (double-click whole-curve selection) ===
{
    Console.WriteLine("=== Mesh.ConnectedEdgePath ===");
    var m = new Mesh();
    new AddPolylineCommand(m, [new(0, 0, 0), new(1, 0, 0), new(2, 0, 0.5f), new(3, 0, 1), new(4, 0, 1.5f)], closed: false).Do();
    m.AddWire(m.AddVertex(new Vector3(10, 0, 0)), m.AddVertex(new Vector3(11, 0, 0))); // unrelated

    var seed = new EdgeKey(m.Vertices[1], m.Vertices[2]);
    int chain = m.ConnectedEdgePath(seed).Count();
    fails += Check("flood recovers the whole 4-edge chain", chain == 4, chain);

    // Add a branch at vertex[2] -> it becomes a junction (valence 3); flood must stop there.
    m.AddWire(m.Vertices[2], m.AddVertex(new Vector3(2, 0, 3)));
    int capped = m.ConnectedEdgePath(new EdgeKey(m.Vertices[0], m.Vertices[1])).Count();
    fails += Check("flood stops at a junction (valence>2)", capped == 2, capped);
    Console.WriteLine();
}

// === SelectTool double-click selects the whole curve (interactive path) ===
{
    Console.WriteLine("=== SelectTool double-click whole curve ===");
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
    fails += Check("double-click selects all 4 arc edges", ctx.Selection.Edges.Count == 4, ctx.Selection.Edges.Count);
    Console.WriteLine();
}

Console.WriteLine(fails == 0 ? "RESULT: PASS (all tests)" : $"RESULT: FAIL ({fails} assertion(s) failed)");
return fails == 0 ? 0 : 1;
