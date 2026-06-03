using System.Numerics;
using MintPlayer.ThreeDee.Rendering;

namespace MintPlayer.ThreeDee;

public sealed class MainForm : Form
{
    readonly ViewportControl _viewport = new() { Dock = DockStyle.Fill };
    readonly ToolStripStatusLabel _fpsLabel = new() { AutoSize = true };
    readonly ToolStripStatusLabel _projLabel = new() { AutoSize = true };
    readonly ToolStripStatusLabel _styleLabel = new() { AutoSize = true };
    readonly ToolStripStatusLabel _selLabel = new() { AutoSize = true, Text = "Selected: 0 face(s), 0 edge(s)" };
    readonly ToolStripStatusLabel _hintLabel = new()
    {
        Spring = true,
        TextAlign = ContentAlignment.MiddleRight,
        Text = "Navigate: toolbar buttons (hold to repeat) · MMB orbit · Shift+MMB pan · wheel zoom · F1 for help",
    };
    readonly ToolStripStatusLabel _vcbLabel = new() { Text = "Measurements", AutoSize = true };
    readonly ToolStripTextBox _vcb = new() { Width = 90, ToolTipText = "Type a length / distance, then Enter" };

    const string Filter = "ThreeDee scene (*.3dee)|*.3dee|JSON (*.json)|*.json|All files (*.*)|*.*";
    readonly Dictionary<RenderStyle, ToolStripMenuItem> _styleItems = new();
    readonly Dictionary<string, ToolStripMenuItem> _toolItems = new();
    readonly List<(ToolStripMenuItem Item, Vector3 Normal)> _planeItems = [];
    ToolStripMenuItem _projItem = null!, _undoItem = null!, _redoItem = null!;
    string? _currentPath;

    // toolbar mirrors of the checkable menu state, so menu + toolbar stay in sync
    readonly Dictionary<string, ToolStripButton> _toolButtons = new();
    readonly Dictionary<RenderStyle, ToolStripButton> _styleButtons = new();
    ToolStripButton _projButton = null!, _tbUndo = null!, _tbRedo = null!;

    // click-and-hold navigation (for laptops without a middle mouse button or scroll wheel)
    readonly System.Windows.Forms.Timer _navTimer = new() { Interval = 55 };
    Action? _navStep;
    const float OrbitStep = 11f;  // pixels fed to Camera.Orbit (~0.4°/px)
    const float PanStep = 20f;
    const float ZoomStep = 0.10f;

    public MainForm()
    {
        Text = "ThreeDee — Untitled";
        ClientSize = new Size(1280, 760);
        StartPosition = FormStartPosition.CenterScreen;
        BackColor = Color.FromArgb(0x20, 0x28, 0x30);
        KeyPreview = true; // bare-key shortcuts (SketchUp feel) are handled here, not as menu ShortcutKeys

        // Dock order matters: add the fill control first, then inner-to-outer for each edge.
        // Top edge is processed last-added-first, so add the toolbar before the menu to stack
        // them as: menu (top) → toolbar → viewport → status bar.
        Controls.Add(_viewport);
        Controls.Add(BuildStatusBar());
        Controls.Add(BuildToolbar());
        var menu = BuildMenu();
        Controls.Add(menu);
        MainMenuStrip = menu;

        _navTimer.Tick += (_, _) => _navStep?.Invoke();

        _viewport.FrameRendered += ms =>
        {
            _fpsLabel.Text = $"{1000.0 / Math.Max(ms, 0.001):F0} FPS ({ms:F1} ms)";
            UpdateStateLabels();
        };
        _viewport.StatusChanged += s => _selLabel.Text = s;
        _viewport.MeasurementChanged += m => { if (!_vcb.Focused) _vcb.Text = m ?? ""; };
        _viewport.History.Changed += UpdateEditMenu;
        _vcb.KeyDown += (_, e) =>
        {
            if (e.KeyCode != Keys.Enter) return;
            _viewport.ApplyToolValue(_vcb.Text);
            _vcb.Clear();
            _viewport.Focus();
            e.Handled = e.SuppressKeyPress = true;
        };
        UpdateStateLabels();
        UpdateEditMenu();
        UpdateToolChecks("Select");
    }

    MenuStrip BuildMenu()
    {
        var menu = new MenuStrip();

        var file = new ToolStripMenuItem("&File");
        file.DropDownItems.Add(ItemKey("New", Keys.Control | Keys.N, NewFile, "new"));
        file.DropDownItems.Add(ItemKey("Open…", Keys.Control | Keys.O, OpenFile, "open"));
        file.DropDownItems.Add(ItemKey("Save", Keys.Control | Keys.S, SaveFile, "save"));
        file.DropDownItems.Add(ItemKey("Save As…", Keys.Control | Keys.Shift | Keys.S, SaveFileAs, "saveas"));
        file.DropDownItems.Add(new ToolStripSeparator());
        var export = new ToolStripMenuItem("Export") { Image = ToolbarIcons.Get("export") };
        export.DropDownItems.Add(Item("Wavefront OBJ…", "", () => Export("OBJ (*.obj)|*.obj", "obj", _viewport.ExportObj), "export"));
        export.DropDownItems.Add(Item("STL (ASCII)…", "", () => Export("STL (*.stl)|*.stl", "stl", _viewport.ExportStl), "export"));
        file.DropDownItems.Add(export);
        file.DropDownItems.Add(new ToolStripSeparator());
        file.DropDownItems.Add(Item("Exit", "", Close, "exit"));

        var edit = new ToolStripMenuItem("&Edit");
        _undoItem = Item("Undo", "Ctrl+Z", _viewport.Undo, "undo");
        _redoItem = Item("Redo", "Ctrl+Y", _viewport.Redo, "redo");
        edit.DropDownItems.Add(_undoItem);
        edit.DropDownItems.Add(_redoItem);
        edit.DropDownItems.Add(new ToolStripSeparator());
        edit.DropDownItems.Add(Item("Select All", "Ctrl+A", _viewport.SelectAll, "selectall"));
        edit.DropDownItems.Add(Item("Clear Selection", "Esc", _viewport.ClearSelection, "clearsel"));

        var draw = new ToolStripMenuItem("&Draw");
        AddToolItem(draw, "Select", "Space", _viewport.UseSelectTool, "select");
        AddToolItem(draw, "Line", "L", _viewport.UseLineTool, "line");
        AddToolItem(draw, "Rectangle", "R", _viewport.UseRectangleTool, "rect");
        AddToolItem(draw, "Circle", "C", _viewport.UseCircleTool, "circle");
        AddToolItem(draw, "Arc", "A", _viewport.UseArcTool, "arc");
        AddToolItem(draw, "Bezier", "B", _viewport.UseBezierTool, "bezier");
        AddToolItem(draw, "Push/Pull", "P", _viewport.UsePushPullTool, "pushpull");
        draw.DropDownItems.Add(new ToolStripSeparator());
        var planeMenu = new ToolStripMenuItem("Drawing Plane") { Image = ToolbarIcons.Get("plane") };
        AddPlaneItem(planeMenu, "Ground  (XZ)", Vector3.UnitY);
        AddPlaneItem(planeMenu, "Front  (XY)", Vector3.UnitZ);
        AddPlaneItem(planeMenu, "Side  (YZ)", Vector3.UnitX);
        draw.DropDownItems.Add(planeMenu);

        var modify = new ToolStripMenuItem("&Modify");
        AddToolItem(modify, "Move", "M", _viewport.UseMoveTool, "move");
        AddToolItem(modify, "Rotate", "Q", _viewport.UseRotateTool, "rotate");
        AddToolItem(modify, "Follow Me", "", _viewport.UseFollowMeTool, "followme");
        AddToolItem(modify, "Eraser", "E", _viewport.UseEraserTool, "eraser");
        modify.DropDownItems.Add(new ToolStripSeparator());
        modify.DropDownItems.Add(Item("Delete Selection", "Del", _viewport.DeleteSelection, "delete"));
        AddToolItem(modify, "Tape Measure", "D", _viewport.UseTapeMeasureTool, "tape");

        var view = new ToolStripMenuItem("&View");
        view.DropDownItems.Add(Item("Isometric", "I", () => SetView(StandardView.Iso), "iso"));
        view.DropDownItems.Add(Item("Top", "T", () => SetView(StandardView.Top), "top"));
        view.DropDownItems.Add(Item("Front", "F", () => SetView(StandardView.Front), "front"));
        view.DropDownItems.Add(Item("Right", "", () => SetView(StandardView.Right), "right"));
        view.DropDownItems.Add(new ToolStripSeparator());
        view.DropDownItems.Add(Item("Zoom Extents", "Z", _viewport.ZoomExtents, "zoomext"));
        view.DropDownItems.Add(BuildNavigateMenu());
        view.DropDownItems.Add(new ToolStripSeparator());
        _projItem = Item("Parallel Projection", "", ToggleProjection, "projection");
        view.DropDownItems.Add(_projItem);

        var style = new ToolStripMenuItem("&Style");
        AddStyleItem(style, "Wireframe", RenderStyle.Wireframe, "1", "wire");
        AddStyleItem(style, "Hidden Line", RenderStyle.HiddenLine, "2", "hidden");
        AddStyleItem(style, "Shaded", RenderStyle.Shaded, "3", "shaded");
        AddStyleItem(style, "Shaded + Edges", RenderStyle.ShadedEdges, "4", "shadededges");

        var help = new ToolStripMenuItem("&Help");
        var controls = new ToolStripMenuItem("Controls / Navigation…") { ShortcutKeys = Keys.F1, Image = ToolbarIcons.Get("help") };
        controls.Click += (_, _) => ShowControls();
        help.DropDownItems.Add(controls);

        menu.Items.AddRange([file, edit, draw, modify, view, style, help]);
        return menu;
    }

    static void ShowControls() => MessageBox.Show(
        """
        Navigation
          Orbit    Middle mouse button + drag   — or the toolbar orbit buttons
          Pan      Shift + Middle mouse + drag   — or the toolbar pan buttons
          Zoom     Mouse wheel                   — or the toolbar zoom +/- buttons
          No middle button or wheel? Use the toolbar "Nav:" cluster or View > Navigate.
          Toolbar nav buttons auto-repeat while held down.

        Draw tools
          Space Select  L Line  R Rectangle  C Circle  A Arc  B Bezier  P Push/Pull
          Line: click edges; click the start point to close into a face.
          Circle/Rectangle: click center/corner then radius/corner.
          Arc: click start, end, then the bulge.   Bezier: 4 control points.
          Type a value (length / w;d / radius / distance) then Enter for exact sizes.

        Modify
          M Move   Q Rotate   E Eraser   Del Delete selection
          Move/Rotate act on the current selection (pick a base/center first).
          Follow Me: select the path edges, then click the profile face to sweep it.

        Drawing plane (Draw > Drawing Plane): Ground (XZ) / Front (XY) / Side (YZ)
          Lets you draw shapes on vertical planes too. Draw an arc and close it
          with a line to auto-fill the enclosed face.

        Selection (Select tool)
          Click / Shift+Click   Select / toggle
          Drag L→R / R→L        Window / crossing box select
          Double-click          Face + its edges
          Ctrl+Z / Ctrl+Y       Undo / redo       Ctrl+A  Select all

        Views:  I Iso · T Top · F Front · Z Zoom Extents
        Styles: 1 Wireframe · 2 Hidden Line · 3 Shaded · 4 Shaded+Edges
        """,
        "ThreeDee — Controls", MessageBoxButtons.OK, MessageBoxIcon.Information);

    static ToolStripMenuItem Item(string text, string shortcutHint, Action onClick, string? icon = null)
    {
        var item = new ToolStripMenuItem(text) { ShortcutKeyDisplayString = shortcutHint };
        if (icon is not null) item.Image = ToolbarIcons.Get(icon);
        item.Click += (_, _) => onClick();
        return item;
    }

    static ToolStripMenuItem ItemKey(string text, Keys keys, Action onClick, string? icon = null)
    {
        var item = new ToolStripMenuItem(text) { ShortcutKeys = keys };
        if (icon is not null) item.Image = ToolbarIcons.Get(icon);
        item.Click += (_, _) => onClick();
        return item;
    }

    // ---- File ----

    void NewFile() { _viewport.NewScene(); _currentPath = null; UpdateTitle(); }

    void OpenFile()
    {
        using var d = new OpenFileDialog { Filter = Filter };
        if (d.ShowDialog(this) != DialogResult.OK) return;
        try { _viewport.LoadScene(d.FileName); _currentPath = d.FileName; UpdateTitle(); }
        catch (Exception ex) { MessageBox.Show(this, ex.Message, "Open failed", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    void SaveFile()
    {
        if (_currentPath is null) { SaveFileAs(); return; }
        try { _viewport.SaveScene(_currentPath); }
        catch (Exception ex) { MessageBox.Show(this, ex.Message, "Save failed", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    void SaveFileAs()
    {
        using var d = new SaveFileDialog { Filter = Filter, DefaultExt = "3dee", FileName = "Untitled" + SceneIO.Extension };
        if (d.ShowDialog(this) != DialogResult.OK) return;
        try { _viewport.SaveScene(d.FileName); _currentPath = d.FileName; UpdateTitle(); }
        catch (Exception ex) { MessageBox.Show(this, ex.Message, "Save failed", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    void UpdateTitle() => Text = $"ThreeDee — {(_currentPath is null ? "Untitled" : Path.GetFileName(_currentPath))}";

    void Export(string filter, string ext, Action<string> write)
    {
        using var d = new SaveFileDialog { Filter = filter, DefaultExt = ext, FileName = "model." + ext };
        if (d.ShowDialog(this) != DialogResult.OK) return;
        try { write(d.FileName); }
        catch (Exception ex) { MessageBox.Show(this, ex.Message, "Export failed", MessageBoxButtons.OK, MessageBoxIcon.Error); }
    }

    void AddToolItem(ToolStripMenuItem parent, string name, string hint, Action use, string? icon = null)
    {
        var item = new ToolStripMenuItem(name) { ShortcutKeyDisplayString = hint };
        if (icon is not null) item.Image = ToolbarIcons.Get(icon);
        item.Click += (_, _) => { use(); UpdateToolChecks(name); };
        _toolItems[name] = item;
        parent.DropDownItems.Add(item);
    }

    void AddPlaneItem(ToolStripMenuItem parent, string text, Vector3 normal)
    {
        var item = new ToolStripMenuItem(text) { Checked = normal == _viewport.WorkPlaneNormal };
        item.Click += (_, _) => { _viewport.SetWorkPlane(normal); UpdatePlaneChecks(); };
        _planeItems.Add((item, normal));
        parent.DropDownItems.Add(item);
    }

    void UpdatePlaneChecks()
    {
        foreach (var (item, normal) in _planeItems) item.Checked = normal == _viewport.WorkPlaneNormal;
    }

    void AddStyleItem(ToolStripMenuItem parent, string text, RenderStyle style, string hint, string? icon = null)
    {
        var item = new ToolStripMenuItem(text) { ShortcutKeyDisplayString = hint, Checked = style == _viewport.Style };
        if (icon is not null) item.Image = ToolbarIcons.Get(icon);
        item.Click += (_, _) => SetStyle(style);
        _styleItems[style] = item;
        parent.DropDownItems.Add(item);
    }

    StatusStrip BuildStatusBar()
    {
        var bar = new StatusStrip();
        bar.Items.AddRange([_fpsLabel, _projLabel, _styleLabel, _selLabel, _hintLabel, _vcbLabel, _vcb]);
        return bar;
    }

    // ---- toolbar ----

    ToolStrip BuildToolbar()
    {
        var ts = new ToolStrip
        {
            GripStyle = ToolStripGripStyle.Hidden,
            ImageScalingSize = new Size(16, 16),
            Padding = new Padding(2, 1, 2, 1),
        };

        ts.Items.Add(Button("new", "New (Ctrl+N)", NewFile));
        ts.Items.Add(Button("open", "Open… (Ctrl+O)", OpenFile));
        ts.Items.Add(Button("save", "Save (Ctrl+S)", SaveFile));
        ts.Items.Add(new ToolStripSeparator());

        _tbUndo = Button("undo", "Undo (Ctrl+Z)", _viewport.Undo);
        _tbRedo = Button("redo", "Redo (Ctrl+Y)", _viewport.Redo);
        ts.Items.Add(_tbUndo);
        ts.Items.Add(_tbRedo);
        ts.Items.Add(new ToolStripSeparator());

        ToolButton(ts, "select", "Select (Space)", "Select", _viewport.UseSelectTool);
        ToolButton(ts, "line", "Line (L)", "Line", _viewport.UseLineTool);
        ToolButton(ts, "rect", "Rectangle (R)", "Rectangle", _viewport.UseRectangleTool);
        ToolButton(ts, "circle", "Circle (C)", "Circle", _viewport.UseCircleTool);
        ToolButton(ts, "arc", "Arc (A)", "Arc", _viewport.UseArcTool);
        ToolButton(ts, "bezier", "Bezier (B)", "Bezier", _viewport.UseBezierTool);
        ToolButton(ts, "pushpull", "Push/Pull (P)", "Push/Pull", _viewport.UsePushPullTool);
        ts.Items.Add(new ToolStripSeparator());

        ToolButton(ts, "move", "Move (M)", "Move", _viewport.UseMoveTool);
        ToolButton(ts, "rotate", "Rotate (Q)", "Rotate", _viewport.UseRotateTool);
        ToolButton(ts, "followme", "Follow Me", "Follow Me", _viewport.UseFollowMeTool);
        ToolButton(ts, "eraser", "Eraser (E)", "Eraser", _viewport.UseEraserTool);
        ToolButton(ts, "tape", "Tape Measure (D)", "Tape Measure", _viewport.UseTapeMeasureTool);
        ts.Items.Add(Button("delete", "Delete Selection (Del)", _viewport.DeleteSelection));
        ts.Items.Add(new ToolStripSeparator());

        ts.Items.Add(Button("iso", "Isometric (I)", () => SetView(StandardView.Iso)));
        ts.Items.Add(Button("top", "Top (T)", () => SetView(StandardView.Top)));
        ts.Items.Add(Button("front", "Front (F)", () => SetView(StandardView.Front)));
        ts.Items.Add(Button("right", "Right", () => SetView(StandardView.Right)));
        _projButton = Button("projection", "Parallel projection (toggle)", ToggleProjection);
        ts.Items.Add(_projButton);
        ts.Items.Add(new ToolStripSeparator());

        StyleButton(ts, "wire", "Wireframe (1)", RenderStyle.Wireframe);
        StyleButton(ts, "hidden", "Hidden Line (2)", RenderStyle.HiddenLine);
        StyleButton(ts, "shaded", "Shaded (3)", RenderStyle.Shaded);
        StyleButton(ts, "shadededges", "Shaded + Edges (4)", RenderStyle.ShadedEdges);
        ts.Items.Add(new ToolStripSeparator());

        // Navigation cluster — click, or click-and-hold to keep moving (no MMB / wheel needed).
        ts.Items.Add(new ToolStripLabel("Nav:"));
        ts.Items.Add(NavButton("orbitU", "Orbit up (hold)", () => _viewport.OrbitBy(0, OrbitStep)));
        ts.Items.Add(NavButton("orbitD", "Orbit down (hold)", () => _viewport.OrbitBy(0, -OrbitStep)));
        ts.Items.Add(NavButton("orbitL", "Orbit left (hold)", () => _viewport.OrbitBy(-OrbitStep, 0)));
        ts.Items.Add(NavButton("orbitR", "Orbit right (hold)", () => _viewport.OrbitBy(OrbitStep, 0)));
        ts.Items.Add(NavButton("panL", "Pan left (hold)", () => _viewport.PanBy(-PanStep, 0)));
        ts.Items.Add(NavButton("panR", "Pan right (hold)", () => _viewport.PanBy(PanStep, 0)));
        ts.Items.Add(NavButton("panU", "Pan up (hold)", () => _viewport.PanBy(0, -PanStep)));
        ts.Items.Add(NavButton("panD", "Pan down (hold)", () => _viewport.PanBy(0, PanStep)));
        ts.Items.Add(NavButton("zoomin", "Zoom in (hold)", () => _viewport.ZoomBy(ZoomStep)));
        ts.Items.Add(NavButton("zoomout", "Zoom out (hold)", () => _viewport.ZoomBy(-ZoomStep)));
        ts.Items.Add(Button("zoomext", "Zoom Extents (Z)", _viewport.ZoomExtents));

        return ts;
    }

    static ToolStripButton Button(string icon, string tip, Action onClick)
    {
        var b = new ToolStripButton
        {
            Image = ToolbarIcons.Get(icon),
            ToolTipText = tip,
            DisplayStyle = ToolStripItemDisplayStyle.Image,
        };
        b.Click += (_, _) => onClick();
        return b;
    }

    void ToolButton(ToolStrip ts, string icon, string tip, string name, Action use)
    {
        var b = new ToolStripButton
        {
            Image = ToolbarIcons.Get(icon),
            ToolTipText = tip,
            DisplayStyle = ToolStripItemDisplayStyle.Image,
        };
        b.Click += (_, _) => { use(); UpdateToolChecks(name); };
        _toolButtons[name] = b;
        ts.Items.Add(b);
    }

    void StyleButton(ToolStrip ts, string icon, string tip, RenderStyle style)
    {
        var b = new ToolStripButton
        {
            Image = ToolbarIcons.Get(icon),
            ToolTipText = tip,
            DisplayStyle = ToolStripItemDisplayStyle.Image,
            Checked = style == _viewport.Style,
        };
        b.Click += (_, _) => SetStyle(style);
        _styleButtons[style] = b;
        ts.Items.Add(b);
    }

    // A navigation button: steps once on press, then auto-repeats while held.
    ToolStripButton NavButton(string icon, string tip, Action step)
    {
        var b = new ToolStripButton
        {
            Image = ToolbarIcons.Get(icon),
            ToolTipText = tip,
            DisplayStyle = ToolStripItemDisplayStyle.Image,
        };
        b.MouseDown += (_, e) => { if (e.Button == MouseButtons.Left) { step(); _navStep = step; _navTimer.Start(); } };
        b.MouseUp += (_, _) => StopNav();
        b.MouseLeave += (_, _) => StopNav();
        return b;
    }

    void StopNav() { _navTimer.Stop(); _navStep = null; }

    ToolStripMenuItem BuildNavigateMenu()
    {
        var nav = new ToolStripMenuItem("Navigate") { Image = ToolbarIcons.Get("orbitR") };
        nav.DropDownItems.Add(Item("Orbit Up", "", () => _viewport.OrbitBy(0, OrbitStep * 2), "orbitU"));
        nav.DropDownItems.Add(Item("Orbit Down", "", () => _viewport.OrbitBy(0, -OrbitStep * 2), "orbitD"));
        nav.DropDownItems.Add(Item("Orbit Left", "", () => _viewport.OrbitBy(-OrbitStep * 2, 0), "orbitL"));
        nav.DropDownItems.Add(Item("Orbit Right", "", () => _viewport.OrbitBy(OrbitStep * 2, 0), "orbitR"));
        nav.DropDownItems.Add(new ToolStripSeparator());
        nav.DropDownItems.Add(Item("Pan Left", "", () => _viewport.PanBy(-PanStep * 2, 0), "panL"));
        nav.DropDownItems.Add(Item("Pan Right", "", () => _viewport.PanBy(PanStep * 2, 0), "panR"));
        nav.DropDownItems.Add(Item("Pan Up", "", () => _viewport.PanBy(0, -PanStep * 2), "panU"));
        nav.DropDownItems.Add(Item("Pan Down", "", () => _viewport.PanBy(0, PanStep * 2), "panD"));
        nav.DropDownItems.Add(new ToolStripSeparator());
        nav.DropDownItems.Add(Item("Zoom In", "", () => _viewport.ZoomBy(ZoomStep * 2), "zoomin"));
        nav.DropDownItems.Add(Item("Zoom Out", "", () => _viewport.ZoomBy(-ZoomStep * 2), "zoomout"));
        return nav;
    }

    // ---- actions (shared by menu + keyboard) ----

    void SetView(StandardView view) => _viewport.SetView(view);
    void ToggleProjection() { _viewport.ToggleProjection(); UpdateStateLabels(); }

    void SetStyle(RenderStyle style)
    {
        _viewport.SetRenderStyle(style);
        foreach (var kv in _styleItems) kv.Value.Checked = kv.Key == style;
        foreach (var kv in _styleButtons) kv.Value.Checked = kv.Key == style;
        UpdateStateLabels();
    }

    void UseTool(string name, Action use) { use(); UpdateToolChecks(name); }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (_vcb.Focused) return; // let the VCB handle its own typing/Enter

        // SketchUp feel: start typing a value to drive the VCB while a tool op is active.
        if (!e.Control && _viewport.ToolWantsValue && ValueChar(e) is char ch)
        {
            _vcb.Focus();
            _vcb.Text = ch.ToString();
            _vcb.SelectionStart = _vcb.TextLength;
            e.Handled = e.SuppressKeyPress = true;
            return;
        }

        if (e.Control)
        {
            switch (e.KeyCode)
            {
                case Keys.Z: _viewport.Undo(); e.Handled = true; return;
                case Keys.Y: _viewport.Redo(); e.Handled = true; return;
                case Keys.A: _viewport.SelectAll(); e.Handled = true; return;
                default: return;
            }
        }
        switch (e.KeyCode)
        {
            case Keys.Space: UseTool("Select", _viewport.UseSelectTool); break;
            case Keys.L: UseTool("Line", _viewport.UseLineTool); break;
            case Keys.R: UseTool("Rectangle", _viewport.UseRectangleTool); break;
            case Keys.C: UseTool("Circle", _viewport.UseCircleTool); break;
            case Keys.A: UseTool("Arc", _viewport.UseArcTool); break;
            case Keys.B: UseTool("Bezier", _viewport.UseBezierTool); break;
            case Keys.P: UseTool("Push/Pull", _viewport.UsePushPullTool); break;
            case Keys.M: UseTool("Move", _viewport.UseMoveTool); break;
            case Keys.Q: UseTool("Rotate", _viewport.UseRotateTool); break;
            case Keys.E: UseTool("Eraser", _viewport.UseEraserTool); break;
            case Keys.D: UseTool("Tape Measure", _viewport.UseTapeMeasureTool); break;
            case Keys.Delete: _viewport.DeleteSelection(); break;
            case Keys.Escape:
            case Keys.Return: _viewport.ForwardKey(e.KeyCode); break;
            case Keys.I: SetView(StandardView.Iso); break;
            case Keys.T: SetView(StandardView.Top); break;
            case Keys.F: SetView(StandardView.Front); break;
            case Keys.Z: _viewport.ZoomExtents(); break;
            case Keys.D1: SetStyle(RenderStyle.Wireframe); break;
            case Keys.D2: SetStyle(RenderStyle.HiddenLine); break;
            case Keys.D3: SetStyle(RenderStyle.Shaded); break;
            case Keys.D4: SetStyle(RenderStyle.ShadedEdges); break;
            default: return;
        }
        e.Handled = true;
    }

    static char? ValueChar(KeyEventArgs e)
    {
        if (!e.Shift && e.KeyCode >= Keys.D0 && e.KeyCode <= Keys.D9) return (char)('0' + (e.KeyCode - Keys.D0));
        if (e.KeyCode >= Keys.NumPad0 && e.KeyCode <= Keys.NumPad9) return (char)('0' + (e.KeyCode - Keys.NumPad0));
        return e.KeyCode switch
        {
            Keys.OemMinus or Keys.Subtract => '-',
            Keys.OemPeriod or Keys.Decimal => '.',
            Keys.Oemcomma => ',',
            Keys.OemSemicolon => ';',
            _ => null,
        };
    }

    void UpdateToolChecks(string active)
    {
        foreach (var kv in _toolItems) kv.Value.Checked = kv.Key == active;
        foreach (var kv in _toolButtons) kv.Value.Checked = kv.Key == active;
    }

    void UpdateEditMenu()
    {
        _undoItem.Enabled = _viewport.History.CanUndo;
        _redoItem.Enabled = _viewport.History.CanRedo;
        _undoItem.Text = _viewport.History.CanUndo ? $"Undo {_viewport.History.NextUndo}" : "Undo";
        _redoItem.Text = _viewport.History.CanRedo ? $"Redo {_viewport.History.NextRedo}" : "Redo";
        if (_tbUndo is not null) _tbUndo.Enabled = _viewport.History.CanUndo;
        if (_tbRedo is not null) _tbRedo.Enabled = _viewport.History.CanRedo;
    }

    void UpdateStateLabels()
    {
        _projLabel.Text = _viewport.Camera.Orthographic ? "Parallel" : "Perspective";
        _styleLabel.Text = _viewport.Style.ToString();
        if (_projItem is not null) _projItem.Checked = _viewport.Camera.Orthographic;
        if (_projButton is not null) _projButton.Checked = _viewport.Camera.Orthographic;
    }
}
