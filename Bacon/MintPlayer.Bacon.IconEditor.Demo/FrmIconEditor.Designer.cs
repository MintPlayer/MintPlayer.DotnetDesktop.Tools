namespace MintPlayer.Bacon.IconEditor.Demo;

partial class FrmIconEditor
{
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        menuStrip = new MenuStrip();
        fileMenu = new ToolStripMenuItem();
        newToolStripMenuItem = new ToolStripMenuItem();
        openToolStripMenuItem = new ToolStripMenuItem();
        openIcoToolStripMenuItem = new ToolStripMenuItem();
        toolStripSeparator1 = new ToolStripSeparator();
        saveToolStripMenuItem = new ToolStripMenuItem();
        saveAsToolStripMenuItem = new ToolStripMenuItem();
        toolStripSeparator2 = new ToolStripSeparator();
        exportToIcoToolStripMenuItem = new ToolStripMenuItem();
        exportToPngToolStripMenuItem = new ToolStripMenuItem();
        toolStripSeparator3 = new ToolStripSeparator();
        exitToolStripMenuItem = new ToolStripMenuItem();
        editMenu = new ToolStripMenuItem();
        addImageToolStripMenuItem = new ToolStripMenuItem();
        addCommonSizesToolStripMenuItem = new ToolStripMenuItem();
        toolStripSeparator4 = new ToolStripSeparator();
        removeSelectedToolStripMenuItem = new ToolStripMenuItem();
        viewMenu = new ToolStripMenuItem();
        zoomInToolStripMenuItem = new ToolStripMenuItem();
        zoomOutToolStripMenuItem = new ToolStripMenuItem();
        fitToWindowToolStripMenuItem = new ToolStripMenuItem();
        resetZoomToolStripMenuItem = new ToolStripMenuItem();
        helpMenu = new ToolStripMenuItem();
        aboutToolStripMenuItem = new ToolStripMenuItem();
        toolStrip = new ToolStrip();
        addToolStripButton = new ToolStripButton();
        exportPngToolStripButton = new ToolStripButton();
        toolStripSeparator6 = new ToolStripSeparator();
        zoomInToolStripButton = new ToolStripButton();
        zoomOutToolStripButton = new ToolStripButton();
        fitToolStripButton = new ToolStripButton();
        statusStrip = new StatusStrip();
        toolStripStatusLabel = new ToolStripStatusLabel();
        iconEditor = new BaconIconEditor();
        menuStrip.SuspendLayout();
        toolStrip.SuspendLayout();
        statusStrip.SuspendLayout();
        SuspendLayout();
        // 
        // menuStrip
        // 
        menuStrip.Items.AddRange(new ToolStripItem[] { fileMenu, editMenu, viewMenu, helpMenu });
        menuStrip.Location = new Point(0, 0);
        menuStrip.Name = "menuStrip";
        menuStrip.Size = new Size(1024, 24);
        menuStrip.TabIndex = 0;
        menuStrip.Text = "MenuStrip";
        menuStrip.Visible = false;
        // 
        // fileMenu
        // 
        fileMenu.DropDownItems.AddRange(new ToolStripItem[] { newToolStripMenuItem, openToolStripMenuItem, openIcoToolStripMenuItem, toolStripSeparator1, saveToolStripMenuItem, saveAsToolStripMenuItem, toolStripSeparator2, exportToIcoToolStripMenuItem, exportToPngToolStripMenuItem, toolStripSeparator3, exitToolStripMenuItem });
        fileMenu.Name = "fileMenu";
        fileMenu.Size = new Size(37, 20);
        fileMenu.Text = "&File";
        // 
        // newToolStripMenuItem
        // 
        newToolStripMenuItem.Name = "newToolStripMenuItem";
        newToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.N;
        newToolStripMenuItem.Size = new Size(204, 22);
        newToolStripMenuItem.Text = "&New";
        newToolStripMenuItem.Click += NewToolStripMenuItem_Click;
        // 
        // openToolStripMenuItem
        // 
        openToolStripMenuItem.Name = "openToolStripMenuItem";
        openToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.O;
        openToolStripMenuItem.Size = new Size(204, 22);
        openToolStripMenuItem.Text = "&Open...";
        openToolStripMenuItem.Click += OpenToolStripMenuItem_Click;
        // 
        // openIcoToolStripMenuItem
        // 
        openIcoToolStripMenuItem.Name = "openIcoToolStripMenuItem";
        openIcoToolStripMenuItem.Size = new Size(204, 22);
        openIcoToolStripMenuItem.Text = "Open &ICO...";
        openIcoToolStripMenuItem.Click += OpenIcoToolStripMenuItem_Click;
        // 
        // toolStripSeparator1
        // 
        toolStripSeparator1.Name = "toolStripSeparator1";
        toolStripSeparator1.Size = new Size(201, 6);
        // 
        // saveToolStripMenuItem
        // 
        saveToolStripMenuItem.Name = "saveToolStripMenuItem";
        saveToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.S;
        saveToolStripMenuItem.Size = new Size(204, 22);
        saveToolStripMenuItem.Text = "&Save";
        saveToolStripMenuItem.Click += SaveToolStripMenuItem_Click;
        // 
        // saveAsToolStripMenuItem
        // 
        saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
        saveAsToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Shift | Keys.S;
        saveAsToolStripMenuItem.Size = new Size(204, 22);
        saveAsToolStripMenuItem.Text = "Save &As...";
        saveAsToolStripMenuItem.Click += SaveAsToolStripMenuItem_Click;
        // 
        // toolStripSeparator2
        // 
        toolStripSeparator2.Name = "toolStripSeparator2";
        toolStripSeparator2.Size = new Size(201, 6);
        // 
        // exportToIcoToolStripMenuItem
        // 
        exportToIcoToolStripMenuItem.Name = "exportToIcoToolStripMenuItem";
        exportToIcoToolStripMenuItem.Size = new Size(204, 22);
        exportToIcoToolStripMenuItem.Text = "&Export to ICO...";
        exportToIcoToolStripMenuItem.Click += ExportToIcoToolStripMenuItem_Click;
        // 
        // exportToPngToolStripMenuItem
        // 
        exportToPngToolStripMenuItem.Name = "exportToPngToolStripMenuItem";
        exportToPngToolStripMenuItem.Size = new Size(204, 22);
        exportToPngToolStripMenuItem.Text = "Export Selected to &PNG...";
        exportToPngToolStripMenuItem.Click += ExportToPngToolStripMenuItem_Click;
        // 
        // toolStripSeparator3
        // 
        toolStripSeparator3.Name = "toolStripSeparator3";
        toolStripSeparator3.Size = new Size(201, 6);
        // 
        // exitToolStripMenuItem
        // 
        exitToolStripMenuItem.Name = "exitToolStripMenuItem";
        exitToolStripMenuItem.Size = new Size(204, 22);
        exitToolStripMenuItem.Text = "E&xit";
        exitToolStripMenuItem.Click += ExitToolStripMenuItem_Click;
        // 
        // editMenu
        // 
        editMenu.DropDownItems.AddRange(new ToolStripItem[] { addImageToolStripMenuItem, addCommonSizesToolStripMenuItem, toolStripSeparator4, removeSelectedToolStripMenuItem });
        editMenu.Name = "editMenu";
        editMenu.Size = new Size(39, 20);
        editMenu.Text = "&Edit";
        // 
        // addImageToolStripMenuItem
        // 
        addImageToolStripMenuItem.Name = "addImageToolStripMenuItem";
        addImageToolStripMenuItem.Size = new Size(224, 22);
        addImageToolStripMenuItem.Text = "&Add Image...";
        addImageToolStripMenuItem.Click += AddImageToolStripMenuItem_Click;
        // 
        // addCommonSizesToolStripMenuItem
        // 
        addCommonSizesToolStripMenuItem.Name = "addCommonSizesToolStripMenuItem";
        addCommonSizesToolStripMenuItem.Size = new Size(224, 22);
        addCommonSizesToolStripMenuItem.Text = "Add &Common Sizes";
        addCommonSizesToolStripMenuItem.Click += AddCommonSizesToolStripMenuItem_Click;
        // 
        // toolStripSeparator4
        // 
        toolStripSeparator4.Name = "toolStripSeparator4";
        toolStripSeparator4.Size = new Size(221, 6);
        // 
        // removeSelectedToolStripMenuItem
        // 
        removeSelectedToolStripMenuItem.Name = "removeSelectedToolStripMenuItem";
        removeSelectedToolStripMenuItem.ShortcutKeys = Keys.Delete;
        removeSelectedToolStripMenuItem.Size = new Size(224, 22);
        removeSelectedToolStripMenuItem.Text = "&Remove Selected Image";
        removeSelectedToolStripMenuItem.Click += RemoveSelectedToolStripMenuItem_Click;
        // 
        // viewMenu
        // 
        viewMenu.DropDownItems.AddRange(new ToolStripItem[] { zoomInToolStripMenuItem, zoomOutToolStripMenuItem, fitToWindowToolStripMenuItem, resetZoomToolStripMenuItem });
        viewMenu.Name = "viewMenu";
        viewMenu.Size = new Size(44, 20);
        viewMenu.Text = "&View";
        // 
        // zoomInToolStripMenuItem
        // 
        zoomInToolStripMenuItem.Name = "zoomInToolStripMenuItem";
        zoomInToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Oemplus;
        zoomInToolStripMenuItem.Size = new Size(222, 22);
        zoomInToolStripMenuItem.Text = "Zoom &In";
        zoomInToolStripMenuItem.Click += ZoomInToolStripMenuItem_Click;
        // 
        // zoomOutToolStripMenuItem
        // 
        zoomOutToolStripMenuItem.Name = "zoomOutToolStripMenuItem";
        zoomOutToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.OemMinus;
        zoomOutToolStripMenuItem.Size = new Size(222, 22);
        zoomOutToolStripMenuItem.Text = "Zoom &Out";
        zoomOutToolStripMenuItem.Click += ZoomOutToolStripMenuItem_Click;
        // 
        // fitToWindowToolStripMenuItem
        // 
        fitToWindowToolStripMenuItem.Name = "fitToWindowToolStripMenuItem";
        fitToWindowToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.D0;
        fitToWindowToolStripMenuItem.Size = new Size(222, 22);
        fitToWindowToolStripMenuItem.Text = "&Fit to Window";
        fitToWindowToolStripMenuItem.Click += FitToWindowToolStripMenuItem_Click;
        // 
        // resetZoomToolStripMenuItem
        // 
        resetZoomToolStripMenuItem.Name = "resetZoomToolStripMenuItem";
        resetZoomToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.D1;
        resetZoomToolStripMenuItem.Size = new Size(222, 22);
        resetZoomToolStripMenuItem.Text = "&Reset Zoom";
        resetZoomToolStripMenuItem.Click += ResetZoomToolStripMenuItem_Click;
        // 
        // helpMenu
        // 
        helpMenu.DropDownItems.AddRange(new ToolStripItem[] { aboutToolStripMenuItem });
        helpMenu.Name = "helpMenu";
        helpMenu.Size = new Size(44, 20);
        helpMenu.Text = "&Help";
        // 
        // aboutToolStripMenuItem
        // 
        aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
        aboutToolStripMenuItem.Size = new Size(116, 22);
        aboutToolStripMenuItem.Text = "&About...";
        aboutToolStripMenuItem.Click += AboutToolStripMenuItem_Click;
        // 
        // toolStrip
        // 
        toolStrip.Items.AddRange(new ToolStripItem[] { addToolStripButton, exportPngToolStripButton, toolStripSeparator6, zoomInToolStripButton, zoomOutToolStripButton, fitToolStripButton });
        toolStrip.Location = new Point(0, 24);
        toolStrip.Name = "toolStrip";
        toolStrip.Size = new Size(1024, 25);
        toolStrip.TabIndex = 1;
        toolStrip.Text = "ToolStrip";
        // 
        // addToolStripButton
        // 
        addToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
        addToolStripButton.Name = "addToolStripButton";
        addToolStripButton.Size = new Size(33, 22);
        addToolStripButton.Text = "Add";
        addToolStripButton.ToolTipText = "Add Image";
        addToolStripButton.Click += AddImageToolStripMenuItem_Click;
        // 
        // exportPngToolStripButton
        // 
        exportPngToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
        exportPngToolStripButton.Name = "exportPngToolStripButton";
        exportPngToolStripButton.Size = new Size(71, 22);
        exportPngToolStripButton.Text = "Export PNG";
        exportPngToolStripButton.ToolTipText = "Export Selected to PNG";
        exportPngToolStripButton.Click += ExportToPngToolStripMenuItem_Click;
        // 
        // toolStripSeparator6
        // 
        toolStripSeparator6.Name = "toolStripSeparator6";
        toolStripSeparator6.Size = new Size(6, 25);
        // 
        // zoomInToolStripButton
        // 
        zoomInToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
        zoomInToolStripButton.Name = "zoomInToolStripButton";
        zoomInToolStripButton.Size = new Size(56, 22);
        zoomInToolStripButton.Text = "Zoom In";
        zoomInToolStripButton.Click += ZoomInToolStripMenuItem_Click;
        // 
        // zoomOutToolStripButton
        // 
        zoomOutToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
        zoomOutToolStripButton.Name = "zoomOutToolStripButton";
        zoomOutToolStripButton.Size = new Size(66, 22);
        zoomOutToolStripButton.Text = "Zoom Out";
        zoomOutToolStripButton.Click += ZoomOutToolStripMenuItem_Click;
        // 
        // fitToolStripButton
        // 
        fitToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Text;
        fitToolStripButton.Name = "fitToolStripButton";
        fitToolStripButton.Size = new Size(24, 22);
        fitToolStripButton.Text = "Fit";
        fitToolStripButton.Click += FitToWindowToolStripMenuItem_Click;
        // 
        // statusStrip
        // 
        statusStrip.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel });
        statusStrip.Location = new Point(0, 746);
        statusStrip.Name = "statusStrip";
        statusStrip.Size = new Size(1024, 22);
        statusStrip.TabIndex = 2;
        statusStrip.Text = "StatusStrip";
        // 
        // toolStripStatusLabel
        // 
        toolStripStatusLabel.Name = "toolStripStatusLabel";
        toolStripStatusLabel.Size = new Size(39, 17);
        toolStripStatusLabel.Text = "Ready";
        // 
        // iconEditor
        // 
        iconEditor.Dock = DockStyle.Fill;
        iconEditor.Location = new Point(0, 49);
        iconEditor.MinimumSize = new Size(400, 300);
        iconEditor.Name = "iconEditor";
        iconEditor.Size = new Size(1024, 697);
        iconEditor.TabIndex = 3;
        iconEditor.IconChanged += IconEditor_IconChanged;
        iconEditor.SelectedImageChanged += IconEditor_SelectedImageChanged;
        iconEditor.ImageModified += IconEditor_ImageModified;
        // 
        // FrmIconEditor
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1024, 768);
        Controls.Add(iconEditor);
        Controls.Add(statusStrip);
        Controls.Add(toolStrip);
        Controls.Add(menuStrip);
        MainMenuStrip = menuStrip;
        Name = "FrmIconEditor";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Bacon Icon Editor";
        FormClosing += FrmIconEditor_FormClosing;
        Load += FrmIconEditor_Load;
        menuStrip.ResumeLayout(false);
        menuStrip.PerformLayout();
        toolStrip.ResumeLayout(false);
        toolStrip.PerformLayout();
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private MenuStrip menuStrip;
    private ToolStripMenuItem fileMenu;
    private ToolStripMenuItem newToolStripMenuItem;
    private ToolStripMenuItem openToolStripMenuItem;
    private ToolStripMenuItem openIcoToolStripMenuItem;
    private ToolStripSeparator toolStripSeparator1;
    private ToolStripMenuItem saveToolStripMenuItem;
    private ToolStripMenuItem saveAsToolStripMenuItem;
    private ToolStripSeparator toolStripSeparator2;
    private ToolStripMenuItem exportToIcoToolStripMenuItem;
    private ToolStripMenuItem exportToPngToolStripMenuItem;
    private ToolStripSeparator toolStripSeparator3;
    private ToolStripMenuItem exitToolStripMenuItem;
    private ToolStripMenuItem editMenu;
    private ToolStripMenuItem addImageToolStripMenuItem;
    private ToolStripMenuItem addCommonSizesToolStripMenuItem;
    private ToolStripSeparator toolStripSeparator4;
    private ToolStripMenuItem removeSelectedToolStripMenuItem;
    private ToolStripMenuItem viewMenu;
    private ToolStripMenuItem zoomInToolStripMenuItem;
    private ToolStripMenuItem zoomOutToolStripMenuItem;
    private ToolStripMenuItem fitToWindowToolStripMenuItem;
    private ToolStripMenuItem resetZoomToolStripMenuItem;
    private ToolStripMenuItem helpMenu;
    private ToolStripMenuItem aboutToolStripMenuItem;
    private ToolStrip toolStrip;
    private ToolStripButton newToolStripButton;
    private ToolStripButton openToolStripButton;
    private ToolStripButton saveToolStripButton;
    private ToolStripSeparator toolStripSeparator5;
    private ToolStripButton addToolStripButton;
    private ToolStripButton exportPngToolStripButton;
    private ToolStripSeparator toolStripSeparator6;
    private ToolStripButton zoomInToolStripButton;
    private ToolStripButton zoomOutToolStripButton;
    private ToolStripButton fitToolStripButton;
    private StatusStrip statusStrip;
    private ToolStripStatusLabel toolStripStatusLabel;
    private BaconIconEditor iconEditor;
}
