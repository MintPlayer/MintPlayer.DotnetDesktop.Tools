using System.Diagnostics;
using System.Numerics;
using MintPlayer.ThreeDee.Commands;
using MintPlayer.ThreeDee.Rendering;
using MintPlayer.ThreeDee.Tools;

namespace MintPlayer.ThreeDee;

/// <summary>
/// The 3D viewport: owns the renderer, camera, scene, picker, selection and undo history. Navigation
/// (MMB orbit, Shift+MMB pan, wheel zoom) is always available; all other input is routed to the
/// active <see cref="ITool"/>. Each frame blits the rasterized bitmap, the selection highlight and
/// the active tool's overlay.
/// </summary>
public sealed class ViewportControl : Control
{
    readonly IRenderer _renderer = new SoftwareRasterizer();
    readonly Camera _camera = new();
    readonly Scene _scene = DemoScene.Build();
    readonly Selection _selection = new();
    readonly History _history = new();
    readonly Picker _picker;
    readonly ToolContext _ctx;
    readonly List<RenderMesh> _meshes = [];
    readonly List<WorldLine> _gridAxes = DemoContent.BuildGridAndAxes();
    readonly List<WorldLine> _lines = [];
    BoundingBox _bounds;

    ITool _tool = new SelectTool();
    Point _lastMouse;
    bool _orbiting;

    // manual double-click detection (the WinForms event is unreliable on this UserPaint control)
    long _lastLeftDownTick;
    Point _lastLeftDownPos;
    bool _suppressLeftUp;

    public Camera Camera => _camera;
    public History History => _history;
    public string ActiveToolName => _tool.Name;

    [System.ComponentModel.Browsable(false)]
    [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
    public RenderStyle Style { get; set; } = RenderStyle.ShadedEdges;

    public event Action<double>? FrameRendered;
    public event Action<string>? StatusChanged;
    public event Action<string?>? MeasurementChanged;

    public bool ToolWantsValue => _tool.WantsValue;

    public ViewportControl()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.Opaque
               | ControlStyles.UserPaint | ControlStyles.OptimizedDoubleBuffer, true);
        TabStop = true;
        _picker = new Picker(_scene.Mesh);
        _ctx = new ToolContext(_scene, _camera, _picker, _selection, _history, Invalidate, RebuildScene);
        _history.Changed += () => { RebuildScene(); _selection.Prune(_scene.Mesh); RaiseStatus(); Invalidate(); };
        RebuildScene();
        _camera.SetStandardView(StandardView.Iso, _bounds);
        _tool.Activate(_ctx);
    }

    void RebuildScene()
    {
        _meshes.Clear();
        _meshes.AddRange(_scene.BuildRenderMeshes());
        _lines.Clear();
        _lines.AddRange(_gridAxes);          // grid + axes first…
        _lines.AddRange(_scene.WireLines());  // …then wires, so they win at coincident depths (y=0)
        _bounds = _scene.Bounds();
    }

    protected override void OnHandleCreated(EventArgs e) { base.OnHandleCreated(e); ApplyViewportSize(); }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        ApplyViewportSize();
        Invalidate();
    }

    void ApplyViewportSize()
    {
        _camera.ViewportWidth = Math.Max(1, Width);
        _camera.ViewportHeight = Math.Max(1, Height);
        _renderer.Resize(Width, Height);
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        var sw = Stopwatch.StartNew();
        var bmp = _renderer.RenderFrame(_meshes, _lines, _camera, Style);
        sw.Stop();
        e.Graphics.DrawImageUnscaled(bmp, 0, 0);

        SelectionOverlay.DrawSelection(e.Graphics, _camera, _selection);
        _tool.DrawOverlay(e.Graphics, _ctx);

        FrameRendered?.Invoke(sw.Elapsed.TotalMilliseconds);
    }

    // ---- input: MMB/wheel = navigation; everything else -> active tool ----

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        Focus();
        if (e.Button == MouseButtons.Middle) { _orbiting = true; _lastMouse = e.Location; return; }

        // Manual double-click detection — reliable for a custom UserPaint control (the framework's
        // OnMouseDoubleClick is unreliable here). A detected double-click routes to the tool and
        // suppresses the trailing mouse-up so it can't overwrite the selection.
        if (e.Button == MouseButtons.Left)
        {
            long now = Environment.TickCount64;
            var sz = SystemInformation.DoubleClickSize;
            bool dbl = now - _lastLeftDownTick <= SystemInformation.DoubleClickTime
                       && Math.Abs(e.X - _lastLeftDownPos.X) <= sz.Width
                       && Math.Abs(e.Y - _lastLeftDownPos.Y) <= sz.Height;
            _lastLeftDownTick = dbl ? 0 : now;
            _lastLeftDownPos = e.Location;
            if (dbl)
            {
                _suppressLeftUp = true;
                _tool.OnDoubleClick(_ctx, e.Location, ModifierKeys);
                RaiseStatus();
                return;
            }
        }
        _tool.OnMouseDown(_ctx, e.Button, e.Location, ModifierKeys);
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (_orbiting)
        {
            int dx = e.X - _lastMouse.X, dy = e.Y - _lastMouse.Y;
            _lastMouse = e.Location;
            if ((ModifierKeys & Keys.Shift) != 0) _camera.Pan(dx, dy); else _camera.Orbit(dx, dy);
            Invalidate();
        }
        else { _tool.OnMouseMove(_ctx, e.Location, ModifierKeys); MeasurementChanged?.Invoke(_tool.Measurement); }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (e.Button == MouseButtons.Middle) { _orbiting = false; return; }
        if (e.Button == MouseButtons.Left && _suppressLeftUp) { _suppressLeftUp = false; return; }
        _tool.OnMouseUp(_ctx, e.Button, e.Location, ModifierKeys);
        RaiseStatus();
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);
        _camera.ZoomAtCursor(new Vector2(e.X, e.Y), (e.Delta > 0 ? 1 : -1) * 0.12f);
        Invalidate();
    }

    void RaiseStatus()
    {
        string sel = $"Selected: {_selection.Faces.Count} face(s), {_selection.Edges.Count} edge(s)";
        string hint = _tool.Hint;
        StatusChanged?.Invoke(hint.Length > 0 ? $"{_tool.Name} — {hint}   ·   {sel}" : $"{sel}  ·  {_tool.Name} tool");
    }

    // ---- commands invoked from the menu / keyboard ----

    public void SetTool(ITool tool) { _tool = tool; _tool.Activate(_ctx); RaiseStatus(); MeasurementChanged?.Invoke(null); Invalidate(); }

    /// <summary>Apply a typed VCB value to the active tool.</summary>
    public void ApplyToolValue(string text)
    {
        _tool.TryApplyValue(_ctx, text);
        RaiseStatus();
        MeasurementChanged?.Invoke(_tool.Measurement);
        Invalidate();
    }
    public void UseSelectTool() => SetTool(new SelectTool());
    public void UseLineTool() => SetTool(new LineTool());
    public void UseRectangleTool() => SetTool(new RectangleTool());
    public void UsePushPullTool() => SetTool(new PushPullTool());
    public void UseCircleTool() => SetTool(new CircleTool());
    public void UseArcTool() => SetTool(new ArcTool());
    public void UseBezierTool() => SetTool(new BezierTool());
    public void UseMoveTool() => SetTool(new MoveTool());
    public void UseRotateTool() => SetTool(new RotateTool());
    public void UseEraserTool() => SetTool(new EraserTool());
    public void UseTapeMeasureTool() => SetTool(new TapeMeasureTool());
    public void UseFollowMeTool() => SetTool(new FollowMeTool());

    public void ExportObj(string path) => File.WriteAllText(path, MeshExport.ToObj(_scene));
    public void ExportStl(string path) => File.WriteAllText(path, MeshExport.ToStl(_scene));

    /// <summary>Delete the current selection (Delete key).</summary>
    public void DeleteSelection()
    {
        if (_selection.IsEmpty) return;
        _history.Execute(new Commands.DeleteCommand(_scene.Mesh, [.. _selection.Faces], [.. _selection.Edges]));
        _selection.Clear();
    }
    public void ForwardKey(Keys key) => _tool.OnKeyDown(_ctx, key);

    public void Undo() => _history.Undo();
    public void Redo() => _history.Redo();

    public void SetView(StandardView view) { _camera.SetStandardView(view, _bounds); Invalidate(); }
    public void ZoomExtents() { _camera.ZoomExtents(_bounds); Invalidate(); }

    // ---- button/menu navigation: same camera ops as MMB-orbit / Shift+MMB-pan / wheel-zoom,
    //      exposed for machines without a middle mouse button or scroll wheel (laptops, trackpads). ----
    public void OrbitBy(float dxPixels, float dyPixels) { _camera.Orbit(dxPixels, dyPixels); Invalidate(); }
    public void PanBy(float dxPixels, float dyPixels) { _camera.Pan(dxPixels, dyPixels); Invalidate(); }
    public void ZoomBy(float factor) { _camera.Zoom(factor); Invalidate(); }
    public void ToggleProjection() { _camera.Orthographic = !_camera.Orthographic; Invalidate(); }
    public void SetRenderStyle(RenderStyle style) { Style = style; Invalidate(); }

    /// <summary>Active construction plane normal (XZ ground = +Y, XY front = +Z, YZ side = +X).</summary>
    public Vector3 WorkPlaneNormal => _ctx.WorkPlane.Normal;
    public void SetWorkPlane(Vector3 normal) { _ctx.WorkPlane = (Vector3.Zero, normal); RaiseStatus(); Invalidate(); }

    public void ClearSelection() { _selection.Clear(); RaiseStatus(); Invalidate(); }

    // ---- document (save/load) ----

    public void NewScene()
    {
        _scene.Mesh.Clear();
        _selection.Clear();
        _history.Clear();
        RebuildScene();
        RaiseStatus();
        Invalidate();
    }

    public void SaveScene(string path) => File.WriteAllText(path, SceneIO.Save(_scene, _camera));

    public void LoadScene(string path)
    {
        SceneIO.Load(File.ReadAllText(path), _scene, _camera);
        _selection.Clear();
        _history.Clear();
        RebuildScene();
        _camera.ZoomExtents(_bounds);
        RaiseStatus();
        Invalidate();
    }

    public void SelectAll()
    {
        _selection.Clear();
        foreach (var f in _scene.Mesh.Faces) _selection.AddFace(f);
        foreach (var (a, b) in _scene.Mesh.UniqueEdges()) _selection.AddEdge(new Geometry.EdgeKey(a, b));
        RaiseStatus();
        Invalidate();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _renderer.Dispose();
        base.Dispose(disposing);
    }
}
