using System.Drawing.Imaging;
using System.Numerics;
using MintPlayer.ThreeDee.Rendering;

namespace MintPlayer.ThreeDee;

internal static class Program
{
    [STAThread]
    static int Main(string[] args)
    {
        // Headless verification: `ThreeDee --screenshot <path>` renders one frame and exits.
        int idx = Array.IndexOf(args, "--screenshot");
        if (idx >= 0 && idx + 1 < args.Length)
        {
            var style = idx + 2 < args.Length && Enum.TryParse<RenderStyle>(args[idx + 2], out var s)
                ? s : RenderStyle.ShadedEdges;
            // Optional camera overrides: --screenshot <path> <style> <azimuth> <elevation>
            (float, float)? angle = idx + 4 < args.Length
                && float.TryParse(args[idx + 3], out var az) && float.TryParse(args[idx + 4], out var el)
                ? (az, el) : null;
            return Screenshot(args[idx + 1], style, angle);
        }

        int pidx = Array.IndexOf(args, "--pickshot");
        if (pidx >= 0 && pidx + 1 < args.Length)
            return PickShot(args[pidx + 1]);

        int didx = Array.IndexOf(args, "--drawshot");
        if (didx >= 0 && didx + 1 < args.Length)
            return DrawShot(args[didx + 1]);

        int uidx = Array.IndexOf(args, "--pullshot");
        if (uidx >= 0 && uidx + 1 < args.Length)
            return PullShot(args[uidx + 1]);

        int iidx = Array.IndexOf(args, "--infershot");
        if (iidx >= 0 && iidx + 1 < args.Length)
            return InferShot(args[iidx + 1]);

        int midx = Array.IndexOf(args, "--m7shot");
        if (midx >= 0 && midx + 1 < args.Length)
            return M7Shot(args[midx + 1]);

        int plidx = Array.IndexOf(args, "--planeshot");
        if (plidx >= 0 && plidx + 1 < args.Length)
            return PlaneShot(args[plidx + 1]);

        int loidx = Array.IndexOf(args, "--loopshot");
        if (loidx >= 0 && loidx + 1 < args.Length)
            return LoopShot(args[loidx + 1]);

        int fmidx = Array.IndexOf(args, "--followshot");
        if (fmidx >= 0 && fmidx + 1 < args.Length)
            return FollowShot(args[fmidx + 1]);

        int spidx = Array.IndexOf(args, "--sphereshot");
        if (spidx >= 0 && spidx + 1 < args.Length)
            return SphereShot(args[spidx + 1]);

        int fidx = Array.IndexOf(args, "--formshot");
        if (fidx >= 0 && fidx + 1 < args.Length)
            return FormShot(args[fidx + 1]);

        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
        return 0;
    }

    // Verification: capture the whole main window (menu + toolbar + viewport) to a PNG.
    static int FormShot(string path)
    {
        ApplicationConfiguration.Initialize();
        using var form = new MainForm
        {
            StartPosition = FormStartPosition.Manual,
            Location = new Point(-4000, -4000), // realize off-screen so it doesn't steal focus
            ShowInTaskbar = false,
        };
        form.Show();
        Application.DoEvents(); // let the menu/toolbar/viewport lay out and paint once
        using var bmp = new Bitmap(form.Width, form.Height);
        form.DrawToBitmap(bmp, new Rectangle(0, 0, form.Width, form.Height));
        bmp.Save(path, ImageFormat.Png);
        form.Close();
        Console.WriteLine($"Wrote {path} ({bmp.Width}x{bmp.Height})");
        return 0;
    }

    // Verification: drive the Line tool headlessly to draw an open polyline + a closed face.
    static int DrawShot(string path)
    {
        var scene = new Scene();
        var cam = new Camera { ViewportWidth = 1280, ViewportHeight = 720, Target = Vector3.Zero, Distance = 18, AzimuthDeg = 45, ElevationDeg = 30 };
        var ctx = new Tools.ToolContext(scene, cam, new Picker(scene.Mesh), new Selection(), new Commands.History(), () => { }, () => { });
        var line = new Tools.LineTool();
        Point S(Vector3 w) { cam.Project(w, out var s); return new Point((int)s.X, (int)s.Y); }

        line.Activate(ctx); // closed quad on the ground
        foreach (var w in new[] { new Vector3(-4, 0, -2), new(-1, 0, -2), new(-1, 0, 1), new(-4, 0, 1), new(-4, 0, -2) })
            line.OnMouseDown(ctx, MouseButtons.Left, S(w), Keys.None);

        line.Activate(ctx); // open polyline
        foreach (var w in new[] { new Vector3(2, 0, -2), new(4, 0, 0), new(3, 0, 2) })
            line.OnMouseDown(ctx, MouseButtons.Left, S(w), Keys.None);
        line.OnKeyDown(ctx, Keys.Return);

        var lines = new List<WorldLine>(DemoContent.BuildGridAndAxes());
        lines.AddRange(scene.WireLines());
        using var r = new SoftwareRasterizer();
        r.Resize(cam.ViewportWidth, cam.ViewportHeight);
        var bmp = r.RenderFrame(scene.BuildRenderMeshes(), lines, cam, RenderStyle.ShadedEdges);
        bmp.Save(path, ImageFormat.Png);
        Console.WriteLine($"Wrote {path}: {scene.Mesh.Faces.Count} face(s), {scene.Mesh.Wires.Count} wire(s)");
        return 0;
    }

    // Verification: draw a rectangle on the ground, then Push/Pull it into a box.
    static int PullShot(string path)
    {
        var scene = new Scene();
        var cam = new Camera { ViewportWidth = 1280, ViewportHeight = 720, Target = Vector3.Zero, Distance = 16, AzimuthDeg = 50, ElevationDeg = 28 };
        var ctx = new Tools.ToolContext(scene, cam, new Picker(scene.Mesh), new Selection(), new Commands.History(), () => { }, () => { });
        Point S(Vector3 w) { cam.Project(w, out var s); return new Point((int)s.X, (int)s.Y); }

        var rect = new Tools.RectangleTool();
        rect.Activate(ctx);
        rect.OnMouseDown(ctx, MouseButtons.Left, S(new Vector3(-2, 0, -2)), Keys.None);
        rect.OnMouseDown(ctx, MouseButtons.Left, S(new Vector3(2, 0, 2)), Keys.None);

        ctx.Commit(new Commands.PushPullCommand(scene.Mesh, scene.Mesh.Faces[^1], 3.5f));

        var lines = new List<WorldLine>(DemoContent.BuildGridAndAxes());
        lines.AddRange(scene.WireLines());
        using var r = new SoftwareRasterizer();
        r.Resize(cam.ViewportWidth, cam.ViewportHeight);
        var bmp = r.RenderFrame(scene.BuildRenderMeshes(), lines, cam, RenderStyle.ShadedEdges);
        bmp.Save(path, ImageFormat.Png);
        Console.WriteLine($"Wrote {path}: {scene.Mesh.Faces.Count} faces, {scene.Mesh.Vertices.Count} verts");
        return 0;
    }

    // Verification: Follow-Me revolution — circle profile + circle path -> a sphere.
    static int SphereShot(string path)
    {
        const float R = 2.5f;
        var scene = new Scene();
        var cam = new Camera { ViewportWidth = 1280, ViewportHeight = 720, Target = Vector3.Zero, Distance = 12, AzimuthDeg = 35, ElevationDeg = 22 };
        var prof = scene.Mesh.MakeFace(Tessellation.Polygon(Vector3.Zero, Vector3.UnitX, Vector3.UnitY, R, 24));
        var circlePath = Tessellation.Polygon(Vector3.Zero, Vector3.UnitX, Vector3.UnitZ, R, 48);
        circlePath.Add(circlePath[0]);
        new Commands.FollowMeCommand(scene.Mesh, prof, circlePath).Do();
        scene.Mesh.RemoveFace(prof);

        using var r = new SoftwareRasterizer();
        r.Resize(cam.ViewportWidth, cam.ViewportHeight);
        var bmp = r.RenderFrame(scene.BuildRenderMeshes(), DemoContent.BuildGridAndAxes(), cam, RenderStyle.Shaded);
        bmp.Save(path, ImageFormat.Png);
        Console.WriteLine($"Wrote {path}: {scene.Mesh.Faces.Count} faces");
        return 0;
    }

    // Verification: Follow-Me — sweep a square profile along an arc path into a curved tube.
    static int FollowShot(string path)
    {
        var scene = new Scene();
        var cam = new Camera { ViewportWidth = 1280, ViewportHeight = 720, Target = new Vector3(0, 0.5f, 0), Distance = 18, AzimuthDeg = 45, ElevationDeg = 30 };

        var prof = scene.Mesh.MakeFace([new(-0.5f, -0.5f, 0), new(0.5f, -0.5f, 0), new(0.5f, 0.5f, 0), new(-0.5f, 0.5f, 0)]);
        Tessellation.Arc3Point(new Vector3(-4, 0, 0), new Vector3(0, 0, 4), new Vector3(4, 0, 0), 24, out var arcPath);
        new Commands.FollowMeCommand(scene.Mesh, prof, arcPath).Do();
        scene.Mesh.RemoveFace(prof); // hide the construction profile for the shot

        var lines = new List<WorldLine>(DemoContent.BuildGridAndAxes());
        lines.AddRange(scene.WireLines());
        using var r = new SoftwareRasterizer();
        r.Resize(cam.ViewportWidth, cam.ViewportHeight);
        var bmp = r.RenderFrame(scene.BuildRenderMeshes(), lines, cam, RenderStyle.ShadedEdges);
        bmp.Save(path, ImageFormat.Png);
        Console.WriteLine($"Wrote {path}: {scene.Mesh.Faces.Count} faces");
        return 0;
    }

    // Verification: draw an arc, close it with a line -> the loop auto-fills a face.
    static int LoopShot(string path)
    {
        var scene = new Scene();
        var cam = new Camera { ViewportWidth = 1280, ViewportHeight = 720, Target = Vector3.Zero, Distance = 12, AzimuthDeg = 40, ElevationDeg = 45 };
        var ctx = new Tools.ToolContext(scene, cam, new Picker(scene.Mesh), new Selection(), new Commands.History(), () => { }, () => { });
        Point S(Vector3 w) { cam.Project(w, out var s); return new Point((int)s.X, (int)s.Y); }

        var arc = new Tools.ArcTool(); arc.Activate(ctx);
        arc.OnMouseDown(ctx, MouseButtons.Left, S(new Vector3(-3, 0, 0)), Keys.None);
        arc.OnMouseDown(ctx, MouseButtons.Left, S(new Vector3(3, 0, 0)), Keys.None);
        arc.OnMouseDown(ctx, MouseButtons.Left, S(new Vector3(0, 0, 3)), Keys.None);
        var line = new Tools.LineTool(); line.Activate(ctx);
        line.OnMouseDown(ctx, MouseButtons.Left, S(new Vector3(-3, 0, 0)), Keys.None);
        line.OnMouseDown(ctx, MouseButtons.Left, S(new Vector3(3, 0, 0)), Keys.None);

        var lines = new List<WorldLine>(DemoContent.BuildGridAndAxes());
        lines.AddRange(scene.WireLines());
        using var r = new SoftwareRasterizer();
        r.Resize(cam.ViewportWidth, cam.ViewportHeight);
        var bmp = r.RenderFrame(scene.BuildRenderMeshes(), lines, cam, RenderStyle.ShadedEdges);
        bmp.Save(path, ImageFormat.Png);
        Console.WriteLine($"Wrote {path}: {scene.Mesh.Faces.Count} faces");
        return 0;
    }

    // Verification: rectangle on the Front (XY) plane + circle on the Side (YZ) plane.
    static int PlaneShot(string path)
    {
        var scene = new Scene();
        var cam = new Camera { ViewportWidth = 1280, ViewportHeight = 720, Target = new Vector3(0, 1.5f, 0), Distance = 16, AzimuthDeg = 35, ElevationDeg = 22 };
        var ctx = new Tools.ToolContext(scene, cam, new Picker(scene.Mesh), new Selection(), new Commands.History(), () => { }, () => { });
        Point S(Vector3 w) { cam.Project(w, out var s); return new Point((int)s.X, (int)s.Y); }
        void Click(Tools.ITool t, Vector3 w) => t.OnMouseDown(ctx, MouseButtons.Left, S(w), Keys.None);

        ctx.WorkPlane = (Vector3.Zero, Vector3.UnitZ); // Front / XY
        var rect = new Tools.RectangleTool(); rect.Activate(ctx);
        Click(rect, new Vector3(-3, 0, 0)); Click(rect, new Vector3(0, 3, 0));

        ctx.WorkPlane = (Vector3.Zero, Vector3.UnitX); // Side / YZ
        var circle = new Tools.CircleTool(); circle.Activate(ctx);
        Click(circle, new Vector3(0, 1.5f, -3)); Click(circle, new Vector3(0, 3f, -3));

        var lines = new List<WorldLine>(DemoContent.BuildGridAndAxes());
        lines.AddRange(scene.WireLines());
        using var r = new SoftwareRasterizer();
        r.Resize(cam.ViewportWidth, cam.ViewportHeight);
        var bmp = r.RenderFrame(scene.BuildRenderMeshes(), lines, cam, RenderStyle.ShadedEdges);
        bmp.Save(path, ImageFormat.Png);
        Console.WriteLine($"Wrote {path}: {scene.Mesh.Faces.Count} faces");
        return 0;
    }

    // Verification: a circle face, a 3-point arc, and a cubic Bézier drawn via the tools.
    static int M7Shot(string path)
    {
        var scene = new Scene();
        var cam = new Camera { ViewportWidth = 1280, ViewportHeight = 720, Target = new Vector3(1, 0, 0), Distance = 18, AzimuthDeg = 50, ElevationDeg = 35 };
        var ctx = new Tools.ToolContext(scene, cam, new Picker(scene.Mesh), new Selection(), new Commands.History(), () => { }, () => { });
        Point S(Vector3 w) { cam.Project(w, out var s); return new Point((int)s.X, (int)s.Y); }
        void Click(Tools.ITool t, Vector3 w) => t.OnMouseDown(ctx, MouseButtons.Left, S(w), Keys.None);

        var circle = new Tools.CircleTool(); circle.Activate(ctx);
        Click(circle, new Vector3(-4, 0, 0)); Click(circle, new Vector3(-4, 0, 2));

        var arc = new Tools.ArcTool(); arc.Activate(ctx);
        Click(arc, new Vector3(0, 0, -3)); Click(arc, new Vector3(4, 0, -3)); Click(arc, new Vector3(2, 0, -1));

        var bez = new Tools.BezierTool(); bez.Activate(ctx);
        foreach (var w in new[] { new Vector3(0, 0, 2), new(2, 0, 5), new(5, 0, 5), new(6, 0, 2) }) Click(bez, w);

        var lines = new List<WorldLine>(DemoContent.BuildGridAndAxes());
        lines.AddRange(scene.WireLines());
        using var r = new SoftwareRasterizer();
        r.Resize(cam.ViewportWidth, cam.ViewportHeight);
        var bmp = r.RenderFrame(scene.BuildRenderMeshes(), lines, cam, RenderStyle.ShadedEdges);
        bmp.Save(path, ImageFormat.Png);
        Console.WriteLine($"Wrote {path}: {scene.Mesh.Faces.Count} faces, {scene.Mesh.Wires.Count} wires");
        return 0;
    }

    // Verification: Line tool mid-draw, locked to the red +X axis, with a snap indicator.
    static int InferShot(string path)
    {
        var scene = new Scene();
        var cam = new Camera { ViewportWidth = 1280, ViewportHeight = 720, Target = new Vector3(2, 0, 0), Distance = 14, AzimuthDeg = 35, ElevationDeg = 28 };
        var ctx = new Tools.ToolContext(scene, cam, new Picker(scene.Mesh), new Selection(), new Commands.History(), () => { }, () => { });
        Point S(Vector3 w) { cam.Project(w, out var s); return new Point((int)s.X, (int)s.Y); }

        var line = new Tools.LineTool();
        line.Activate(ctx);
        line.OnMouseDown(ctx, MouseButtons.Left, S(Vector3.Zero), Keys.None);     // start at origin
        line.OnMouseMove(ctx, S(new Vector3(4, 0, 0.05f)), Keys.None);            // drag ~along +X -> axis lock

        var lines = new List<WorldLine>(DemoContent.BuildGridAndAxes());
        lines.AddRange(scene.WireLines());
        using var r = new SoftwareRasterizer();
        r.Resize(cam.ViewportWidth, cam.ViewportHeight);
        var bmp = r.RenderFrame(scene.BuildRenderMeshes(), lines, cam, RenderStyle.ShadedEdges);
        using (var g = Graphics.FromImage(bmp)) line.DrawOverlay(g, ctx);
        bmp.Save(path, ImageFormat.Png);
        Console.WriteLine($"Wrote {path}");
        return 0;
    }

    // Verification: render a frame with a selected face + its edges and a hovered face overlaid.
    static int PickShot(string path)
    {
        var scene = DemoScene.Build();
        var meshes = scene.BuildRenderMeshes();
        var lines = DemoContent.BuildGridAndAxes();
        var cam = new Camera { ViewportWidth = 1280, ViewportHeight = 720 };
        cam.SetStandardView(StandardView.Iso, scene.Bounds());
        using var r = new SoftwareRasterizer();
        r.Resize(cam.ViewportWidth, cam.ViewportHeight);
        var bmp = r.RenderFrame(meshes, lines, cam, RenderStyle.ShadedEdges);

        var sel = new Selection();
        var bodyTop = scene.Mesh.Faces[2];   // body box, +Y face
        sel.AddFace(bodyTop);
        foreach (var h in bodyTop.Loop()) sel.AddEdge(new MintPlayer.ThreeDee.Geometry.EdgeKey(h.Origin, h.To));
        var cubeTop = scene.Mesh.Faces[14];   // cube box, +Y face — shown as hover
        var hover = PickResult.OnFace(cubeTop, cubeTop.Centroid(), 0f);

        using (var g = Graphics.FromImage(bmp))
        {
            SelectionOverlay.DrawSelection(g, cam, sel);
            SelectionOverlay.DrawHover(g, cam, hover, sel);
        }
        bmp.Save(path, ImageFormat.Png);
        Console.WriteLine($"Wrote {path}");
        return 0;
    }

    static int Screenshot(string path, RenderStyle style, (float az, float el)? angle = null)
    {
        var scene = DemoScene.Build();
        var meshes = scene.BuildRenderMeshes();
        var lines = DemoContent.BuildGridAndAxes();
        var cam = new Camera { ViewportWidth = 1280, ViewportHeight = 720 };
        cam.SetStandardView(StandardView.Iso, scene.Bounds());
        if (angle is { } a) { cam.AzimuthDeg = a.az; cam.ElevationDeg = a.el; }
        using var r = new SoftwareRasterizer();
        r.Resize(cam.ViewportWidth, cam.ViewportHeight);
        var bmp = r.RenderFrame(meshes, lines, cam, style);
        bmp.Save(path, ImageFormat.Png);
        Console.WriteLine($"Wrote {path} ({bmp.Width}x{bmp.Height}, {style})");
        return 0;
    }
}
