namespace MintPlayer.Bacon.IconEditor.Demo;

public class MainForm : Form
{
    private readonly MenuStrip _menuStrip;
    private readonly ToolStrip _toolStrip;
    private readonly StatusStrip _statusStrip;
    private readonly ToolStripStatusLabel _statusLabel;
    private int _childFormCount;

    public MainForm()
    {
        Text = "Bacon Icon Editor";
        Size = new Size(1280, 900);
        StartPosition = FormStartPosition.CenterScreen;
        IsMdiContainer = true;

        // Create menu strip
        _menuStrip = CreateMenuStrip();

        // Create tool strip
        _toolStrip = CreateToolStrip();

        // Create status strip
        _statusStrip = new StatusStrip();
        _statusLabel = new ToolStripStatusLabel("Ready");
        _statusStrip.Items.Add(_statusLabel);

        // Layout
        Controls.Add(_toolStrip);
        Controls.Add(_menuStrip);
        Controls.Add(_statusStrip);

        MainMenuStrip = _menuStrip;

        // Handle MDI child activation
        MdiChildActivate += OnMdiChildActivate;
    }

    private MenuStrip CreateMenuStrip()
    {
        var menuStrip = new MenuStrip();

        // File menu
        var fileMenu = new ToolStripMenuItem("&File");
        fileMenu.DropDownItems.Add("&New Icon", null, (s, e) => NewIconEditor());
        fileMenu.DropDownItems.Add("&Open...", null, (s, e) => OpenFile());
        fileMenu.DropDownItems.Add(new ToolStripSeparator());
        fileMenu.DropDownItems.Add("&Save", null, (s, e) => ActiveIconEditor?.SaveFile());
        fileMenu.DropDownItems.Add("Save &As...", null, (s, e) => ActiveIconEditor?.SaveFileAs());
        fileMenu.DropDownItems.Add(new ToolStripSeparator());
        fileMenu.DropDownItems.Add("&Close", null, (s, e) => ActiveMdiChild?.Close());
        fileMenu.DropDownItems.Add("Close A&ll", null, (s, e) => CloseAllChildren());
        fileMenu.DropDownItems.Add(new ToolStripSeparator());
        fileMenu.DropDownItems.Add("E&xit", null, (s, e) => Close());

        // Window menu
        var windowMenu = new ToolStripMenuItem("&Window");
        windowMenu.DropDownItems.Add("&Cascade", null, (s, e) => LayoutMdi(MdiLayout.Cascade));
        windowMenu.DropDownItems.Add("Tile &Horizontally", null, (s, e) => LayoutMdi(MdiLayout.TileHorizontal));
        windowMenu.DropDownItems.Add("Tile &Vertically", null, (s, e) => LayoutMdi(MdiLayout.TileVertical));
        windowMenu.DropDownItems.Add("&Arrange Icons", null, (s, e) => LayoutMdi(MdiLayout.ArrangeIcons));

        // Help menu
        var helpMenu = new ToolStripMenuItem("&Help");
        helpMenu.DropDownItems.Add("&About", null, (s, e) => ShowAbout());

        menuStrip.Items.AddRange(new ToolStripItem[] { fileMenu, windowMenu, helpMenu });
        menuStrip.MdiWindowListItem = windowMenu;

        return menuStrip;
    }

    private ToolStrip CreateToolStrip()
    {
        var toolStrip = new ToolStrip();

        toolStrip.Items.Add(new ToolStripButton("New", null, (s, e) => NewIconEditor()) { ToolTipText = "New Icon Editor" });
        toolStrip.Items.Add(new ToolStripButton("Open", null, (s, e) => OpenFile()) { ToolTipText = "Open File" });
        toolStrip.Items.Add(new ToolStripButton("Save", null, (s, e) => ActiveIconEditor?.SaveFile()) { ToolTipText = "Save File" });
        toolStrip.Items.Add(new ToolStripSeparator());
        toolStrip.Items.Add(new ToolStripButton("Cascade", null, (s, e) => LayoutMdi(MdiLayout.Cascade)) { ToolTipText = "Cascade Windows" });
        toolStrip.Items.Add(new ToolStripButton("Tile H", null, (s, e) => LayoutMdi(MdiLayout.TileHorizontal)) { ToolTipText = "Tile Horizontally" });
        toolStrip.Items.Add(new ToolStripButton("Tile V", null, (s, e) => LayoutMdi(MdiLayout.TileVertical)) { ToolTipText = "Tile Vertically" });

        return toolStrip;
    }

    private FrmIconEditor? ActiveIconEditor => ActiveMdiChild as FrmIconEditor;

    private void NewIconEditor()
    {
        _childFormCount++;
        var childForm = new FrmIconEditor
        {
            MdiParent = this,
            Text = $"Icon {_childFormCount}"
        };
        childForm.Show();
        UpdateStatusBar();
    }

    private void OpenFile()
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "Bacon Icon Files (*.bicon)|*.bicon|Icon Files (*.ico)|*.ico|All Files (*.*)|*.*",
            Title = "Open Icon File"
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            _childFormCount++;
            var childForm = new FrmIconEditor
            {
                MdiParent = this
            };

            if (Path.GetExtension(dialog.FileName).Equals(".ico", StringComparison.OrdinalIgnoreCase))
            {
                childForm.OpenIcoFile(dialog.FileName);
            }
            else
            {
                childForm.OpenFile(dialog.FileName);
            }

            childForm.Show();
            UpdateStatusBar();
        }
    }

    private void CloseAllChildren()
    {
        foreach (var child in MdiChildren.ToArray())
        {
            child.Close();
        }
    }

    private void OnMdiChildActivate(object? sender, EventArgs e)
    {
        UpdateStatusBar();
    }

    private void UpdateStatusBar()
    {
        var childCount = MdiChildren.Length;
        if (childCount == 0)
        {
            _statusLabel.Text = "Ready - No documents open";
        }
        else
        {
            var activeChild = ActiveMdiChild;
            _statusLabel.Text = $"Documents: {childCount} | Active: {activeChild?.Text ?? "None"}";
        }
    }

    private void ShowAbout()
    {
        MessageBox.Show(
            "Bacon Icon Editor\n\n" +
            "A flexible icon editor for creating and editing multi-resolution icons.\n\n" +
            "Part of MintPlayer.DotnetDesktop.Tools",
            "About",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        // Check if any child forms have unsaved changes
        foreach (var child in MdiChildren)
        {
            if (child is FrmIconEditor iconEditor && iconEditor.IsModified)
            {
                var result = MessageBox.Show(
                    $"Save changes to {iconEditor.Text}?",
                    "Save Changes",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                switch (result)
                {
                    case DialogResult.Yes:
                        iconEditor.SaveFile();
                        if (iconEditor.IsModified)
                        {
                            e.Cancel = true;
                            return;
                        }
                        break;
                    case DialogResult.Cancel:
                        e.Cancel = true;
                        return;
                }
            }
        }

        base.OnFormClosing(e);
    }
}
