namespace MintPlayer.Bacon.IconEditor.Demo;

partial class MainForm
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
        components = new System.ComponentModel.Container();
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
        menuStrip = new MenuStrip();
        fileMenu = new ToolStripMenuItem();
        newToolStripMenuItem = new ToolStripMenuItem();
        openToolStripMenuItem = new ToolStripMenuItem();
        toolStripSeparator3 = new ToolStripSeparator();
        saveToolStripMenuItem = new ToolStripMenuItem();
        saveAsToolStripMenuItem = new ToolStripMenuItem();
        toolStripSeparator4 = new ToolStripSeparator();
        closeToolStripMenuItem = new ToolStripMenuItem();
        closeAllToolStripMenuItem = new ToolStripMenuItem();
        toolStripSeparator5 = new ToolStripSeparator();
        exitToolStripMenuItem = new ToolStripMenuItem();
        editMenu = new ToolStripMenuItem();
        undoToolStripMenuItem = new ToolStripMenuItem();
        redoToolStripMenuItem = new ToolStripMenuItem();
        toolStripSeparator6 = new ToolStripSeparator();
        cutToolStripMenuItem = new ToolStripMenuItem();
        copyToolStripMenuItem = new ToolStripMenuItem();
        pasteToolStripMenuItem = new ToolStripMenuItem();
        toolStripSeparator7 = new ToolStripSeparator();
        selectAllToolStripMenuItem = new ToolStripMenuItem();
        viewMenu = new ToolStripMenuItem();
        toolBarToolStripMenuItem = new ToolStripMenuItem();
        statusBarToolStripMenuItem = new ToolStripMenuItem();
        windowsMenu = new ToolStripMenuItem();
        newWindowToolStripMenuItem = new ToolStripMenuItem();
        cascadeToolStripMenuItem = new ToolStripMenuItem();
        tileVerticalToolStripMenuItem = new ToolStripMenuItem();
        tileHorizontalToolStripMenuItem = new ToolStripMenuItem();
        closeAllWindowsToolStripMenuItem = new ToolStripMenuItem();
        arrangeIconsToolStripMenuItem = new ToolStripMenuItem();
        helpMenu = new ToolStripMenuItem();
        contentsToolStripMenuItem = new ToolStripMenuItem();
        indexToolStripMenuItem = new ToolStripMenuItem();
        searchToolStripMenuItem = new ToolStripMenuItem();
        toolStripSeparator8 = new ToolStripSeparator();
        aboutToolStripMenuItem = new ToolStripMenuItem();
        toolStrip = new ToolStrip();
        newToolStripButton = new ToolStripButton();
        openToolStripButton = new ToolStripButton();
        saveToolStripButton = new ToolStripButton();
        toolStripSeparator1 = new ToolStripSeparator();
        printToolStripButton = new ToolStripButton();
        printPreviewToolStripButton = new ToolStripButton();
        toolStripSeparator2 = new ToolStripSeparator();
        helpToolStripButton = new ToolStripButton();
        statusStrip = new StatusStrip();
        toolStripStatusLabel = new ToolStripStatusLabel();
        toolTip = new ToolTip(components);
        cutToolStripButton = new ToolStripButton();
        copyToolStripButton = new ToolStripButton();
        pasteToolStripButton = new ToolStripButton();
        toolStripSeparator9 = new ToolStripSeparator();
        menuStrip.SuspendLayout();
        toolStrip.SuspendLayout();
        statusStrip.SuspendLayout();
        SuspendLayout();
        // 
        // menuStrip
        // 
        menuStrip.Items.AddRange(new ToolStripItem[] { fileMenu, editMenu, viewMenu, windowsMenu, helpMenu });
        menuStrip.Location = new Point(0, 0);
        menuStrip.MdiWindowListItem = windowsMenu;
        menuStrip.Name = "menuStrip";
        menuStrip.Size = new Size(1280, 24);
        menuStrip.TabIndex = 0;
        menuStrip.Text = "MenuStrip";
        // 
        // fileMenu
        // 
        fileMenu.DropDownItems.AddRange(new ToolStripItem[] { newToolStripMenuItem, openToolStripMenuItem, toolStripSeparator3, saveToolStripMenuItem, saveAsToolStripMenuItem, toolStripSeparator4, closeToolStripMenuItem, closeAllToolStripMenuItem, toolStripSeparator5, exitToolStripMenuItem });
        fileMenu.ImageTransparentColor = SystemColors.ActiveBorder;
        fileMenu.Name = "fileMenu";
        fileMenu.Size = new Size(37, 20);
        fileMenu.Text = "&File";
        // 
        // newToolStripMenuItem
        // 
        newToolStripMenuItem.Image = (Image)resources.GetObject("newToolStripMenuItem.Image");
        newToolStripMenuItem.ImageTransparentColor = Color.Black;
        newToolStripMenuItem.Name = "newToolStripMenuItem";
        newToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.N;
        newToolStripMenuItem.Size = new Size(195, 22);
        newToolStripMenuItem.Text = "&New";
        newToolStripMenuItem.Click += NewToolStripMenuItem_Click;
        // 
        // openToolStripMenuItem
        // 
        openToolStripMenuItem.Image = (Image)resources.GetObject("openToolStripMenuItem.Image");
        openToolStripMenuItem.ImageTransparentColor = Color.Black;
        openToolStripMenuItem.Name = "openToolStripMenuItem";
        openToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.O;
        openToolStripMenuItem.Size = new Size(195, 22);
        openToolStripMenuItem.Text = "&Open...";
        openToolStripMenuItem.Click += OpenToolStripMenuItem_Click;
        // 
        // toolStripSeparator3
        // 
        toolStripSeparator3.Name = "toolStripSeparator3";
        toolStripSeparator3.Size = new Size(192, 6);
        // 
        // saveToolStripMenuItem
        // 
        saveToolStripMenuItem.Image = (Image)resources.GetObject("saveToolStripMenuItem.Image");
        saveToolStripMenuItem.ImageTransparentColor = Color.Black;
        saveToolStripMenuItem.Name = "saveToolStripMenuItem";
        saveToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.S;
        saveToolStripMenuItem.Size = new Size(195, 22);
        saveToolStripMenuItem.Text = "&Save";
        saveToolStripMenuItem.Click += SaveToolStripMenuItem_Click;
        // 
        // saveAsToolStripMenuItem
        // 
        saveAsToolStripMenuItem.Name = "saveAsToolStripMenuItem";
        saveAsToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Shift | Keys.S;
        saveAsToolStripMenuItem.Size = new Size(195, 22);
        saveAsToolStripMenuItem.Text = "Save &As...";
        saveAsToolStripMenuItem.Click += SaveAsToolStripMenuItem_Click;
        // 
        // toolStripSeparator4
        // 
        toolStripSeparator4.Name = "toolStripSeparator4";
        toolStripSeparator4.Size = new Size(192, 6);
        // 
        // closeToolStripMenuItem
        // 
        closeToolStripMenuItem.Name = "closeToolStripMenuItem";
        closeToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.W;
        closeToolStripMenuItem.Size = new Size(195, 22);
        closeToolStripMenuItem.Text = "&Close";
        closeToolStripMenuItem.Click += CloseToolStripMenuItem_Click;
        // 
        // closeAllToolStripMenuItem
        // 
        closeAllToolStripMenuItem.Name = "closeAllToolStripMenuItem";
        closeAllToolStripMenuItem.Size = new Size(195, 22);
        closeAllToolStripMenuItem.Text = "Close A&ll";
        closeAllToolStripMenuItem.Click += CloseAllToolStripMenuItem_Click;
        // 
        // toolStripSeparator5
        // 
        toolStripSeparator5.Name = "toolStripSeparator5";
        toolStripSeparator5.Size = new Size(192, 6);
        // 
        // exitToolStripMenuItem
        // 
        exitToolStripMenuItem.Name = "exitToolStripMenuItem";
        exitToolStripMenuItem.ShortcutKeys = Keys.Alt | Keys.F4;
        exitToolStripMenuItem.Size = new Size(195, 22);
        exitToolStripMenuItem.Text = "E&xit";
        exitToolStripMenuItem.Click += ExitToolStripMenuItem_Click;
        // 
        // editMenu
        // 
        editMenu.DropDownItems.AddRange(new ToolStripItem[] { undoToolStripMenuItem, redoToolStripMenuItem, toolStripSeparator6, cutToolStripMenuItem, copyToolStripMenuItem, pasteToolStripMenuItem, toolStripSeparator7, selectAllToolStripMenuItem });
        editMenu.Name = "editMenu";
        editMenu.Size = new Size(39, 20);
        editMenu.Text = "&Edit";
        // 
        // undoToolStripMenuItem
        // 
        undoToolStripMenuItem.Image = (Image)resources.GetObject("undoToolStripMenuItem.Image");
        undoToolStripMenuItem.ImageTransparentColor = Color.Black;
        undoToolStripMenuItem.Name = "undoToolStripMenuItem";
        undoToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Z;
        undoToolStripMenuItem.Size = new Size(164, 22);
        undoToolStripMenuItem.Text = "&Undo";
        undoToolStripMenuItem.Click += UndoToolStripMenuItem_Click;
        // 
        // redoToolStripMenuItem
        // 
        redoToolStripMenuItem.Image = (Image)resources.GetObject("redoToolStripMenuItem.Image");
        redoToolStripMenuItem.ImageTransparentColor = Color.Black;
        redoToolStripMenuItem.Name = "redoToolStripMenuItem";
        redoToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.Y;
        redoToolStripMenuItem.Size = new Size(164, 22);
        redoToolStripMenuItem.Text = "&Redo";
        redoToolStripMenuItem.Click += RedoToolStripMenuItem_Click;
        // 
        // toolStripSeparator6
        // 
        toolStripSeparator6.Name = "toolStripSeparator6";
        toolStripSeparator6.Size = new Size(161, 6);
        // 
        // cutToolStripMenuItem
        // 
        cutToolStripMenuItem.Image = (Image)resources.GetObject("cutToolStripMenuItem.Image");
        cutToolStripMenuItem.ImageTransparentColor = Color.Black;
        cutToolStripMenuItem.Name = "cutToolStripMenuItem";
        cutToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.X;
        cutToolStripMenuItem.Size = new Size(164, 22);
        cutToolStripMenuItem.Text = "Cu&t";
        cutToolStripMenuItem.Click += CutToolStripMenuItem_Click;
        // 
        // copyToolStripMenuItem
        // 
        copyToolStripMenuItem.Image = (Image)resources.GetObject("copyToolStripMenuItem.Image");
        copyToolStripMenuItem.ImageTransparentColor = Color.Black;
        copyToolStripMenuItem.Name = "copyToolStripMenuItem";
        copyToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.C;
        copyToolStripMenuItem.Size = new Size(164, 22);
        copyToolStripMenuItem.Text = "&Copy";
        copyToolStripMenuItem.Click += CopyToolStripMenuItem_Click;
        // 
        // pasteToolStripMenuItem
        // 
        pasteToolStripMenuItem.Image = (Image)resources.GetObject("pasteToolStripMenuItem.Image");
        pasteToolStripMenuItem.ImageTransparentColor = Color.Black;
        pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
        pasteToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.V;
        pasteToolStripMenuItem.Size = new Size(164, 22);
        pasteToolStripMenuItem.Text = "&Paste";
        pasteToolStripMenuItem.Click += PasteToolStripMenuItem_Click;
        // 
        // toolStripSeparator7
        // 
        toolStripSeparator7.Name = "toolStripSeparator7";
        toolStripSeparator7.Size = new Size(161, 6);
        // 
        // selectAllToolStripMenuItem
        // 
        selectAllToolStripMenuItem.Name = "selectAllToolStripMenuItem";
        selectAllToolStripMenuItem.ShortcutKeys = Keys.Control | Keys.A;
        selectAllToolStripMenuItem.Size = new Size(164, 22);
        selectAllToolStripMenuItem.Text = "Select &All";
        selectAllToolStripMenuItem.Click += SelectAllToolStripMenuItem_Click;
        // 
        // viewMenu
        // 
        viewMenu.DropDownItems.AddRange(new ToolStripItem[] { toolBarToolStripMenuItem, statusBarToolStripMenuItem });
        viewMenu.Name = "viewMenu";
        viewMenu.Size = new Size(44, 20);
        viewMenu.Text = "&View";
        // 
        // toolBarToolStripMenuItem
        // 
        toolBarToolStripMenuItem.Checked = true;
        toolBarToolStripMenuItem.CheckOnClick = true;
        toolBarToolStripMenuItem.CheckState = CheckState.Checked;
        toolBarToolStripMenuItem.Name = "toolBarToolStripMenuItem";
        toolBarToolStripMenuItem.Size = new Size(126, 22);
        toolBarToolStripMenuItem.Text = "&Toolbar";
        toolBarToolStripMenuItem.Click += ToolBarToolStripMenuItem_Click;
        // 
        // statusBarToolStripMenuItem
        // 
        statusBarToolStripMenuItem.Checked = true;
        statusBarToolStripMenuItem.CheckOnClick = true;
        statusBarToolStripMenuItem.CheckState = CheckState.Checked;
        statusBarToolStripMenuItem.Name = "statusBarToolStripMenuItem";
        statusBarToolStripMenuItem.Size = new Size(126, 22);
        statusBarToolStripMenuItem.Text = "&Status Bar";
        statusBarToolStripMenuItem.Click += StatusBarToolStripMenuItem_Click;
        // 
        // windowsMenu
        // 
        windowsMenu.DropDownItems.AddRange(new ToolStripItem[] { newWindowToolStripMenuItem, cascadeToolStripMenuItem, tileVerticalToolStripMenuItem, tileHorizontalToolStripMenuItem, closeAllWindowsToolStripMenuItem, arrangeIconsToolStripMenuItem });
        windowsMenu.Name = "windowsMenu";
        windowsMenu.Size = new Size(68, 20);
        windowsMenu.Text = "&Windows";
        // 
        // newWindowToolStripMenuItem
        // 
        newWindowToolStripMenuItem.Name = "newWindowToolStripMenuItem";
        newWindowToolStripMenuItem.Size = new Size(151, 22);
        newWindowToolStripMenuItem.Text = "&New Window";
        newWindowToolStripMenuItem.Click += NewWindowToolStripMenuItem_Click;
        // 
        // cascadeToolStripMenuItem
        // 
        cascadeToolStripMenuItem.Name = "cascadeToolStripMenuItem";
        cascadeToolStripMenuItem.Size = new Size(151, 22);
        cascadeToolStripMenuItem.Text = "&Cascade";
        cascadeToolStripMenuItem.Click += CascadeToolStripMenuItem_Click;
        // 
        // tileVerticalToolStripMenuItem
        // 
        tileVerticalToolStripMenuItem.Name = "tileVerticalToolStripMenuItem";
        tileVerticalToolStripMenuItem.Size = new Size(151, 22);
        tileVerticalToolStripMenuItem.Text = "Tile &Vertical";
        tileVerticalToolStripMenuItem.Click += TileVerticalToolStripMenuItem_Click;
        // 
        // tileHorizontalToolStripMenuItem
        // 
        tileHorizontalToolStripMenuItem.Name = "tileHorizontalToolStripMenuItem";
        tileHorizontalToolStripMenuItem.Size = new Size(151, 22);
        tileHorizontalToolStripMenuItem.Text = "Tile &Horizontal";
        tileHorizontalToolStripMenuItem.Click += TileHorizontalToolStripMenuItem_Click;
        // 
        // closeAllWindowsToolStripMenuItem
        // 
        closeAllWindowsToolStripMenuItem.Name = "closeAllWindowsToolStripMenuItem";
        closeAllWindowsToolStripMenuItem.Size = new Size(151, 22);
        closeAllWindowsToolStripMenuItem.Text = "C&lose All";
        closeAllWindowsToolStripMenuItem.Click += CloseAllWindowsToolStripMenuItem_Click;
        // 
        // arrangeIconsToolStripMenuItem
        // 
        arrangeIconsToolStripMenuItem.Name = "arrangeIconsToolStripMenuItem";
        arrangeIconsToolStripMenuItem.Size = new Size(151, 22);
        arrangeIconsToolStripMenuItem.Text = "&Arrange Icons";
        arrangeIconsToolStripMenuItem.Click += ArrangeIconsToolStripMenuItem_Click;
        // 
        // helpMenu
        // 
        helpMenu.DropDownItems.AddRange(new ToolStripItem[] { contentsToolStripMenuItem, indexToolStripMenuItem, searchToolStripMenuItem, toolStripSeparator8, aboutToolStripMenuItem });
        helpMenu.Name = "helpMenu";
        helpMenu.Size = new Size(44, 20);
        helpMenu.Text = "&Help";
        // 
        // contentsToolStripMenuItem
        // 
        contentsToolStripMenuItem.Name = "contentsToolStripMenuItem";
        contentsToolStripMenuItem.ShortcutKeys = Keys.F1;
        contentsToolStripMenuItem.Size = new Size(141, 22);
        contentsToolStripMenuItem.Text = "&Contents";
        contentsToolStripMenuItem.Click += ContentsToolStripMenuItem_Click;
        // 
        // indexToolStripMenuItem
        // 
        indexToolStripMenuItem.Image = (Image)resources.GetObject("indexToolStripMenuItem.Image");
        indexToolStripMenuItem.ImageTransparentColor = Color.Black;
        indexToolStripMenuItem.Name = "indexToolStripMenuItem";
        indexToolStripMenuItem.Size = new Size(141, 22);
        indexToolStripMenuItem.Text = "&Index";
        indexToolStripMenuItem.Click += IndexToolStripMenuItem_Click;
        // 
        // searchToolStripMenuItem
        // 
        searchToolStripMenuItem.Image = (Image)resources.GetObject("searchToolStripMenuItem.Image");
        searchToolStripMenuItem.ImageTransparentColor = Color.Black;
        searchToolStripMenuItem.Name = "searchToolStripMenuItem";
        searchToolStripMenuItem.Size = new Size(141, 22);
        searchToolStripMenuItem.Text = "&Search";
        searchToolStripMenuItem.Click += SearchToolStripMenuItem_Click;
        // 
        // toolStripSeparator8
        // 
        toolStripSeparator8.Name = "toolStripSeparator8";
        toolStripSeparator8.Size = new Size(138, 6);
        // 
        // aboutToolStripMenuItem
        // 
        aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
        aboutToolStripMenuItem.Size = new Size(141, 22);
        aboutToolStripMenuItem.Text = "&About...";
        aboutToolStripMenuItem.Click += AboutToolStripMenuItem_Click;
        // 
        // toolStrip
        // 
        toolStrip.Items.AddRange(new ToolStripItem[] { newToolStripButton, openToolStripButton, saveToolStripButton, toolStripSeparator1, printToolStripButton, printPreviewToolStripButton, toolStripSeparator2, cutToolStripButton, copyToolStripButton, pasteToolStripButton, toolStripSeparator9, helpToolStripButton });
        toolStrip.Location = new Point(0, 24);
        toolStrip.Name = "toolStrip";
        toolStrip.Size = new Size(1280, 25);
        toolStrip.TabIndex = 1;
        toolStrip.Text = "ToolStrip";
        // 
        // newToolStripButton
        //
        newToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
        newToolStripButton.Image = (Image)resources.GetObject("newToolStripButton1.Image");
        newToolStripButton.ImageTransparentColor = Color.Magenta;
        newToolStripButton.Name = "newToolStripButton";
        newToolStripButton.Size = new Size(23, 22);
        newToolStripButton.Text = "New";
        newToolStripButton.Click += NewToolStripMenuItem_Click;
        // 
        // openToolStripButton
        //
        openToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
        openToolStripButton.Image = (Image)resources.GetObject("openToolStripButton1.Image");
        openToolStripButton.ImageTransparentColor = Color.Magenta;
        openToolStripButton.Name = "openToolStripButton";
        openToolStripButton.Size = new Size(23, 22);
        openToolStripButton.Text = "Open";
        openToolStripButton.Click += OpenToolStripMenuItem_Click;
        // 
        // saveToolStripButton
        //
        saveToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
        saveToolStripButton.Image = (Image)resources.GetObject("saveToolStripButton1.Image");
        saveToolStripButton.ImageTransparentColor = Color.Magenta;
        saveToolStripButton.Name = "saveToolStripButton";
        saveToolStripButton.Size = new Size(23, 22);
        saveToolStripButton.Text = "Save";
        saveToolStripButton.Click += SaveToolStripMenuItem_Click;
        // 
        // toolStripSeparator1
        // 
        toolStripSeparator1.Name = "toolStripSeparator1";
        toolStripSeparator1.Size = new Size(6, 25);
        // 
        // printToolStripButton
        //
        printToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
        printToolStripButton.Image = (Image)resources.GetObject("printToolStripButton1.Image");
        printToolStripButton.ImageTransparentColor = Color.Magenta;
        printToolStripButton.Name = "printToolStripButton";
        printToolStripButton.Size = new Size(23, 22);
        printToolStripButton.Text = "Print";
        printToolStripButton.Click += PrintToolStripButton_Click;
        // 
        // printPreviewToolStripButton
        //
        printPreviewToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
        printPreviewToolStripButton.Image = (Image)resources.GetObject("printToolStripButton1.Image");
        printPreviewToolStripButton.ImageTransparentColor = Color.Magenta;
        printPreviewToolStripButton.Name = "printPreviewToolStripButton";
        printPreviewToolStripButton.Size = new Size(23, 22);
        printPreviewToolStripButton.Text = "Print Preview";
        printPreviewToolStripButton.Click += PrintPreviewToolStripButton_Click;
        // 
        // toolStripSeparator2
        // 
        toolStripSeparator2.Name = "toolStripSeparator2";
        toolStripSeparator2.Size = new Size(6, 25);
        // 
        // helpToolStripButton
        //
        helpToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
        helpToolStripButton.Image = (Image)resources.GetObject("helpToolStripButton1.Image");
        helpToolStripButton.ImageTransparentColor = Color.Magenta;
        helpToolStripButton.Name = "helpToolStripButton";
        helpToolStripButton.Size = new Size(23, 22);
        helpToolStripButton.Text = "Help";
        helpToolStripButton.Click += ContentsToolStripMenuItem_Click;
        // 
        // statusStrip
        // 
        statusStrip.Items.AddRange(new ToolStripItem[] { toolStripStatusLabel });
        statusStrip.Location = new Point(0, 878);
        statusStrip.Name = "statusStrip";
        statusStrip.Size = new Size(1280, 22);
        statusStrip.TabIndex = 2;
        statusStrip.Text = "StatusStrip";
        // 
        // toolStripStatusLabel
        // 
        toolStripStatusLabel.Name = "toolStripStatusLabel";
        toolStripStatusLabel.Size = new Size(39, 17);
        toolStripStatusLabel.Text = "Ready";
        //
        // cutToolStripButton
        //
        cutToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
        cutToolStripButton.Image = (Image)resources.GetObject("cutToolStripButton.Image");
        cutToolStripButton.ImageTransparentColor = Color.Magenta;
        cutToolStripButton.Name = "cutToolStripButton";
        cutToolStripButton.Size = new Size(23, 22);
        cutToolStripButton.Text = "Cut";
        cutToolStripButton.Click += CutToolStripMenuItem_Click;
        // 
        // copyToolStripButton
        //
        copyToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
        copyToolStripButton.Image = (Image)resources.GetObject("copyToolStripButton.Image");
        copyToolStripButton.ImageTransparentColor = Color.Magenta;
        copyToolStripButton.Name = "copyToolStripButton";
        copyToolStripButton.Size = new Size(23, 22);
        copyToolStripButton.Text = "Copy";
        copyToolStripButton.Click += CopyToolStripMenuItem_Click;
        // 
        // pasteToolStripButton
        //
        pasteToolStripButton.DisplayStyle = ToolStripItemDisplayStyle.Image;
        pasteToolStripButton.Image = (Image)resources.GetObject("pasteToolStripButton.Image");
        pasteToolStripButton.ImageTransparentColor = Color.Magenta;
        pasteToolStripButton.Name = "pasteToolStripButton";
        pasteToolStripButton.Size = new Size(23, 22);
        pasteToolStripButton.Text = "Paste";
        pasteToolStripButton.Click += PasteToolStripMenuItem_Click;
        // 
        // toolStripSeparator9
        // 
        toolStripSeparator9.Name = "toolStripSeparator9";
        toolStripSeparator9.Size = new Size(6, 25);
        //
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(1280, 900);
        Controls.Add(statusStrip);
        Controls.Add(toolStrip);
        Controls.Add(menuStrip);
        IsMdiContainer = true;
        MainMenuStrip = menuStrip;
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Bacon Icon Editor";
        FormClosing += MainForm_FormClosing;
        MdiChildActivate += MainForm_MdiChildActivate;
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
    private ToolStrip toolStrip;
    private StatusStrip statusStrip;
    private ToolStripSeparator toolStripSeparator1;
    private ToolStripSeparator toolStripSeparator2;
    private ToolStripSeparator toolStripSeparator3;
    private ToolStripSeparator toolStripSeparator4;
    private ToolStripSeparator toolStripSeparator5;
    private ToolStripSeparator toolStripSeparator6;
    private ToolStripSeparator toolStripSeparator7;
    private ToolStripSeparator toolStripSeparator8;
    private ToolStripStatusLabel toolStripStatusLabel;
    private ToolStripMenuItem aboutToolStripMenuItem;
    private ToolStripMenuItem tileHorizontalToolStripMenuItem;
    private ToolStripMenuItem fileMenu;
    private ToolStripMenuItem newToolStripMenuItem;
    private ToolStripMenuItem openToolStripMenuItem;
    private ToolStripMenuItem saveToolStripMenuItem;
    private ToolStripMenuItem saveAsToolStripMenuItem;
    private ToolStripMenuItem exitToolStripMenuItem;
    private ToolStripMenuItem editMenu;
    private ToolStripMenuItem undoToolStripMenuItem;
    private ToolStripMenuItem redoToolStripMenuItem;
    private ToolStripMenuItem cutToolStripMenuItem;
    private ToolStripMenuItem copyToolStripMenuItem;
    private ToolStripMenuItem pasteToolStripMenuItem;
    private ToolStripMenuItem selectAllToolStripMenuItem;
    private ToolStripMenuItem viewMenu;
    private ToolStripMenuItem toolBarToolStripMenuItem;
    private ToolStripMenuItem statusBarToolStripMenuItem;
    private ToolStripMenuItem windowsMenu;
    private ToolStripMenuItem newWindowToolStripMenuItem;
    private ToolStripMenuItem cascadeToolStripMenuItem;
    private ToolStripMenuItem tileVerticalToolStripMenuItem;
    private ToolStripMenuItem closeAllWindowsToolStripMenuItem;
    private ToolStripMenuItem arrangeIconsToolStripMenuItem;
    private ToolStripMenuItem helpMenu;
    private ToolStripMenuItem contentsToolStripMenuItem;
    private ToolStripMenuItem indexToolStripMenuItem;
    private ToolStripMenuItem searchToolStripMenuItem;
    private ToolStripButton newToolStripButton;
    private ToolStripButton openToolStripButton;
    private ToolStripButton saveToolStripButton;
    private ToolStripButton printToolStripButton;
    private ToolStripButton printPreviewToolStripButton;
    private ToolStripButton helpToolStripButton;
    private ToolStripMenuItem closeToolStripMenuItem;
    private ToolStripMenuItem closeAllToolStripMenuItem;
    private ToolTip toolTip;
    private ToolStripButton cutToolStripButton;
    private ToolStripButton copyToolStripButton;
    private ToolStripButton pasteToolStripButton;
    private ToolStripSeparator toolStripSeparator9;
}
