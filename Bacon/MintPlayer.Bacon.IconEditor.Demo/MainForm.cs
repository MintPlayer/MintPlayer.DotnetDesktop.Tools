namespace MintPlayer.Bacon.IconEditor.Demo;

public partial class MainForm : Form
{
    private int _childFormCount;

    public MainForm()
    {
        InitializeComponent();
    }

    private FrmIconEditor? ActiveIconEditor => ActiveMdiChild as FrmIconEditor;

    #region File Menu

    private void NewToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        NewIconEditor();
    }

    private void OpenToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        OpenFile();
    }

    private void SaveToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        ActiveIconEditor?.SaveFile();
    }

    private void SaveAsToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        ActiveIconEditor?.SaveFileAs();
    }

    private void CloseToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        ActiveMdiChild?.Close();
    }

    private void CloseAllToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        CloseAllChildren();
    }

    private void ExitToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        Close();
    }

    #endregion

    #region Edit Menu

    private void UndoToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        // TODO: Implement undo
    }

    private void RedoToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        // TODO: Implement redo
    }

    private void CutToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        // TODO: Implement cut
    }

    private void CopyToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        // TODO: Implement copy
    }

    private void PasteToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        // TODO: Implement paste
    }

    private void SelectAllToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        // TODO: Implement select all
    }

    #endregion

    #region View Menu

    private void ToolBarToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        toolStrip.Visible = toolBarToolStripMenuItem.Checked;
    }

    private void StatusBarToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        statusStrip.Visible = statusBarToolStripMenuItem.Checked;
    }

    #endregion

    #region Windows Menu

    private void NewWindowToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        NewIconEditor();
    }

    private void CascadeToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        LayoutMdi(MdiLayout.Cascade);
    }

    private void TileVerticalToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        LayoutMdi(MdiLayout.TileVertical);
    }

    private void TileHorizontalToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        LayoutMdi(MdiLayout.TileHorizontal);
    }

    private void CloseAllWindowsToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        CloseAllChildren();
    }

    private void ArrangeIconsToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        LayoutMdi(MdiLayout.ArrangeIcons);
    }

    #endregion

    #region Help Menu

    private void ContentsToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        // TODO: Show help contents
    }

    private void IndexToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        // TODO: Show help index
    }

    private void SearchToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        // TODO: Show help search
    }

    private void AboutToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        ShowAbout();
    }

    #endregion

    #region Toolbar

    private void PrintToolStripButton_Click(object? sender, EventArgs e)
    {
        // TODO: Implement print
    }

    private void PrintPreviewToolStripButton_Click(object? sender, EventArgs e)
    {
        // TODO: Implement print preview
    }

    #endregion

    #region Form Events

    private void MainForm_MdiChildActivate(object? sender, EventArgs e)
    {
        UpdateStatusBar();
    }

    private void MainForm_FormClosing(object? sender, FormClosingEventArgs e)
    {
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
    }

    #endregion

    #region Private Methods

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

    private void UpdateStatusBar()
    {
        var childCount = MdiChildren.Length;
        if (childCount == 0)
        {
            toolStripStatusLabel.Text = "Ready - No documents open";
        }
        else
        {
            var activeChild = ActiveMdiChild;
            toolStripStatusLabel.Text = $"Documents: {childCount} | Active: {activeChild?.Text ?? "None"}";
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

    #endregion
}
