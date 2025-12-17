using MintPlayer.Bacon.Icons;
using MintPlayer.Bacon.IconParser;
using MintPlayer.Bacon.IconParser.Enums;
using MintPlayer.Bacon.Images;
using MintPlayer.Bacon.Images.Layers;

namespace MintPlayer.Bacon.IconEditor.Demo;

public partial class FrmIconEditor : Form
{
    private string? _currentFilePath;
    private bool _isModified;

    public bool IsModified => _isModified;

    public FrmIconEditor()
    {
        InitializeComponent();
    }

    #region Form Events

    private void FrmIconEditor_Load(object? sender, EventArgs e)
    {
        NewIcon();
    }

    private void FrmIconEditor_FormClosing(object? sender, FormClosingEventArgs e)
    {
        if (!CheckSaveChanges())
        {
            e.Cancel = true;
        }
    }

    #endregion

    #region Icon Editor Events

    private void IconEditor_IconChanged(object? sender, EventArgs e)
    {
        UpdateTitle();
    }

    private void IconEditor_ImageModified(object? sender, EventArgs e)
    {
        _isModified = true;
        UpdateTitle();
    }

    private void IconEditor_SelectedImageChanged(object? sender, EventArgs e)
    {
        UpdateStatusBar();
    }

    #endregion

    #region File Menu

    private void NewToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        NewIcon();
    }

    private void OpenToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        OpenFileDialog();
    }

    private void OpenIcoToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        OpenIcoFileDialog();
    }

    private void SaveToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        SaveFile();
    }

    private void SaveAsToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        SaveFileAs();
    }

    private void ExportToIcoToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        ExportToIco();
    }

    private void ExportToPngToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        ExportSelectedToPng();
    }

    private void ExitToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        Close();
    }

    #endregion

    #region Edit Menu

    private void AddImageToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        AddImage();
    }

    private void AddCommonSizesToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        AddCommonSizes();
    }

    private void RemoveSelectedToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        RemoveSelectedImage();
    }

    #endregion

    #region View Menu

    private void ZoomInToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        iconEditor.ImageEditor.Zoom *= 1.25f;
    }

    private void ZoomOutToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        iconEditor.ImageEditor.Zoom *= 0.8f;
    }

    private void FitToWindowToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        iconEditor.ImageEditor.ZoomToFit();
    }

    private void ResetZoomToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        iconEditor.ImageEditor.Zoom = 1.0f;
        iconEditor.ImageEditor.CenterImage();
    }

    #endregion

    #region Help Menu

    private void AboutToolStripMenuItem_Click(object? sender, EventArgs e)
    {
        ShowAbout();
    }

    #endregion

    #region Public Methods

    public void OpenFile(string filePath)
    {
        try
        {
            var icon = BaconIcon.LoadFromFile(filePath);
            if (icon != null)
            {
                iconEditor.Icon = icon;
                _currentFilePath = filePath;
                _isModified = false;
                UpdateTitle();
                toolStripStatusLabel.Text = $"Opened: {Path.GetFileName(filePath)}";
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error opening file: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public void OpenIcoFile(string filePath)
    {
        try
        {
            var images = IcoParser.Read(filePath);
            var icon = new BaconIcon
            {
                Name = Path.GetFileNameWithoutExtension(filePath)
            };

            foreach (var imageWithType in images)
            {
                var baconImage = new BaconImage
                {
                    Width = imageWithType.Width,
                    Height = imageWithType.Height,
                    Name = $"{imageWithType.Width}x{imageWithType.Height}"
                };

                var paintLayer = new PaintLayer
                {
                    Name = "Imported",
                    Bitmap = (Bitmap)imageWithType.Image.Clone()
                };
                baconImage.Layers.Add(paintLayer);

                icon.Images.Add(baconImage);
                imageWithType.Dispose();
            }

            iconEditor.Icon = icon;
            _currentFilePath = null;
            _isModified = true;
            UpdateTitle();
            toolStripStatusLabel.Text = $"Imported {images.Length} images from ICO file";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error opening ICO file: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    public void SaveFile()
    {
        if (string.IsNullOrEmpty(_currentFilePath))
        {
            SaveFileAs();
        }
        else
        {
            SaveToFile(_currentFilePath);
        }
    }

    public void SaveFileAs()
    {
        using var dialog = new SaveFileDialog
        {
            Filter = "Bacon Icon Files (*.bicon)|*.bicon|All Files (*.*)|*.*",
            Title = "Save Bacon Icon"
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            SaveToFile(dialog.FileName);
        }
    }

    #endregion

    #region Private Methods

    private void NewIcon()
    {
        if (!CheckSaveChanges()) return;

        iconEditor.Icon = new BaconIcon { Name = "New Icon" };
        _currentFilePath = null;
        _isModified = false;
        UpdateTitle();
        UpdateStatusBar();
    }

    private void OpenFileDialog()
    {
        if (!CheckSaveChanges()) return;

        using var dialog = new OpenFileDialog
        {
            Filter = "Bacon Icon Files (*.bicon)|*.bicon|All Files (*.*)|*.*",
            Title = "Open Bacon Icon"
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            OpenFile(dialog.FileName);
        }
    }

    private void OpenIcoFileDialog()
    {
        if (!CheckSaveChanges()) return;

        using var dialog = new OpenFileDialog
        {
            Filter = "Icon Files (*.ico)|*.ico|All Files (*.*)|*.*",
            Title = "Open ICO File"
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            OpenIcoFile(dialog.FileName);
        }
    }

    private void SaveToFile(string path)
    {
        if (iconEditor.Icon == null) return;

        try
        {
            iconEditor.Icon.SaveToFile(path);
            _currentFilePath = path;
            _isModified = false;
            UpdateTitle();
            toolStripStatusLabel.Text = $"Saved: {Path.GetFileName(path)}";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving file: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ExportToIco()
    {
        if (iconEditor.Icon == null || iconEditor.Icon.Images.Count == 0)
        {
            MessageBox.Show("No images to export.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var dialog = new SaveFileDialog
        {
            Filter = "Icon Files (*.ico)|*.ico",
            Title = "Export to ICO"
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            try
            {
                var imagesWithType = new List<ImageWithType>();

                foreach (var image in iconEditor.Icon.Images)
                {
                    using var rendered = image.Render();
                    var bitmap = new Bitmap(rendered);
                    imagesWithType.Add(new ImageWithType(bitmap, ImageType.Png));
                }

                IcoParser.Write(dialog.FileName, imagesWithType.ToArray());

                foreach (var img in imagesWithType)
                {
                    img.Dispose();
                }

                toolStripStatusLabel.Text = $"Exported to: {Path.GetFileName(dialog.FileName)}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting to ICO: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void ExportSelectedToPng()
    {
        var selectedImage = iconEditor.SelectedImage;
        if (selectedImage == null)
        {
            MessageBox.Show("No image selected.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        using var dialog = new SaveFileDialog
        {
            Filter = "PNG Files (*.png)|*.png",
            Title = "Export to PNG",
            FileName = $"{selectedImage.Name ?? $"{selectedImage.Width}x{selectedImage.Height}"}.png"
        };

        if (dialog.ShowDialog() == DialogResult.OK)
        {
            try
            {
                selectedImage.ExportToPng(dialog.FileName);
                toolStripStatusLabel.Text = $"Exported to: {Path.GetFileName(dialog.FileName)}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error exporting to PNG: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

    private void AddImage()
    {
        if (iconEditor.Icon == null) return;

        using var dialog = new AddImageDialog();
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            var image = iconEditor.Icon.AddImage(dialog.ImageWidth, dialog.ImageHeight);
            iconEditor.RefreshThumbnails();
            iconEditor.SelectedImage = image;
            _isModified = true;
            UpdateTitle();
        }
    }

    private void AddCommonSizes()
    {
        if (iconEditor.Icon == null) return;

        iconEditor.Icon.AddCommonSizes();
        iconEditor.RefreshThumbnails();
        _isModified = true;
        UpdateTitle();
    }

    private void RemoveSelectedImage()
    {
        var selectedImage = iconEditor.SelectedImage;
        if (selectedImage == null || iconEditor.Icon == null) return;

        var result = MessageBox.Show(
            $"Are you sure you want to remove the {selectedImage.Width}x{selectedImage.Height} image?",
            "Remove Image",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (result == DialogResult.Yes)
        {
            iconEditor.Icon.RemoveImage(selectedImage);
            iconEditor.RefreshThumbnails();
            _isModified = true;
            UpdateTitle();
        }
    }

    private static void ShowAbout()
    {
        MessageBox.Show(
            "Bacon Icon Editor\n\n" +
            "A flexible icon editor for creating and editing multi-resolution icons.\n\n" +
            "Part of MintPlayer.DotnetDesktop.Tools",
            "About",
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private bool CheckSaveChanges()
    {
        if (!_isModified) return true;

        var result = MessageBox.Show(
            "Do you want to save changes to the current icon?",
            "Save Changes",
            MessageBoxButtons.YesNoCancel,
            MessageBoxIcon.Question);

        switch (result)
        {
            case DialogResult.Yes:
                SaveFile();
                return !_isModified;
            case DialogResult.No:
                return true;
            default:
                return false;
        }
    }

    private void UpdateTitle()
    {
        var fileName = string.IsNullOrEmpty(_currentFilePath)
            ? "Untitled"
            : Path.GetFileName(_currentFilePath);

        var modified = _isModified ? " *" : "";

        Text = $"{fileName}{modified}";
    }

    private void UpdateStatusBar()
    {
        var selectedImage = iconEditor.SelectedImage;
        if (selectedImage != null)
        {
            toolStripStatusLabel.Text = $"Selected: {selectedImage.Width} x {selectedImage.Height} | Layers: {selectedImage.Layers.Count}";
        }
        else
        {
            toolStripStatusLabel.Text = "Ready";
        }
    }

    #endregion
}
