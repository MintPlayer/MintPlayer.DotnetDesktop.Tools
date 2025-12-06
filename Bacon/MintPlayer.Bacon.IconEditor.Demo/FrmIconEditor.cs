using System.Drawing;
using System.Drawing.Imaging;
using MintPlayer.Bacon.Icons;
using MintPlayer.Bacon.IconParser;
using MintPlayer.Bacon.IconParser.Enums;
using MintPlayer.Bacon.Images;
using MintPlayer.Bacon.Images.Layers;

namespace MintPlayer.Bacon.IconEditor.Demo;

public class FrmIconEditor : Form
{
    private readonly BaconIconEditor _iconEditor;
    private readonly MenuStrip _menuStrip;
    private readonly ToolStrip _toolStrip;
    private readonly StatusStrip _statusStrip;
    private readonly ToolStripStatusLabel _statusLabel;

    private string? _currentFilePath;
    private bool _isModified;

    public bool IsModified => _isModified;

    public FrmIconEditor()
    {
        Text = "Bacon Icon Editor";
        Size = new Size(1024, 768);
        StartPosition = FormStartPosition.CenterScreen;

        // Create menu strip
        _menuStrip = CreateMenuStrip();

        // Create tool strip
        _toolStrip = CreateToolStrip();

        // Create status strip
        _statusStrip = new StatusStrip();
        _statusLabel = new ToolStripStatusLabel("Ready");
        _statusStrip.Items.Add(_statusLabel);

        // Create icon editor
        _iconEditor = new BaconIconEditor
        {
            Dock = DockStyle.Fill
        };
        _iconEditor.IconChanged += (s, e) => UpdateTitle();
        _iconEditor.ImageModified += (s, e) =>
        {
            _isModified = true;
            UpdateTitle();
        };
        _iconEditor.SelectedImageChanged += (s, e) => UpdateStatusBar();

        // Layout
        Controls.Add(_iconEditor);
        Controls.Add(_toolStrip);
        Controls.Add(_menuStrip);
        Controls.Add(_statusStrip);

        MainMenuStrip = _menuStrip;

        // Start with a new icon
        NewIcon();
    }

    private MenuStrip CreateMenuStrip()
    {
        var menuStrip = new MenuStrip();

        // File menu
        var fileMenu = new ToolStripMenuItem("&File");
        fileMenu.DropDownItems.Add("&New", null, (s, e) => NewIcon());
        fileMenu.DropDownItems.Add("&Open...", null, (s, e) => OpenFileDialog());
        fileMenu.DropDownItems.Add("Open &ICO...", null, (s, e) => OpenIcoFileDialog());
        fileMenu.DropDownItems.Add(new ToolStripSeparator());
        fileMenu.DropDownItems.Add("&Save", null, (s, e) => SaveFile());
        fileMenu.DropDownItems.Add("Save &As...", null, (s, e) => SaveFileAs());
        fileMenu.DropDownItems.Add(new ToolStripSeparator());
        fileMenu.DropDownItems.Add("&Export to ICO...", null, (s, e) => ExportToIco());
        fileMenu.DropDownItems.Add("Export Selected to &PNG...", null, (s, e) => ExportSelectedToPng());
        fileMenu.DropDownItems.Add(new ToolStripSeparator());
        fileMenu.DropDownItems.Add("E&xit", null, (s, e) => Close());

        // Edit menu
        var editMenu = new ToolStripMenuItem("&Edit");
        editMenu.DropDownItems.Add("&Add Image...", null, (s, e) => AddImage());
        editMenu.DropDownItems.Add("Add &Common Sizes", null, (s, e) => AddCommonSizes());
        editMenu.DropDownItems.Add(new ToolStripSeparator());
        editMenu.DropDownItems.Add("&Remove Selected Image", null, (s, e) => RemoveSelectedImage());

        // View menu
        var viewMenu = new ToolStripMenuItem("&View");
        viewMenu.DropDownItems.Add("Zoom &In", null, (s, e) => _iconEditor.ImageEditor.Zoom *= 1.25f);
        viewMenu.DropDownItems.Add("Zoom &Out", null, (s, e) => _iconEditor.ImageEditor.Zoom *= 0.8f);
        viewMenu.DropDownItems.Add("&Fit to Window", null, (s, e) => _iconEditor.ImageEditor.ZoomToFit());
        viewMenu.DropDownItems.Add("&Reset Zoom", null, (s, e) =>
        {
            _iconEditor.ImageEditor.Zoom = 1.0f;
            _iconEditor.ImageEditor.CenterImage();
        });

        // Help menu
        var helpMenu = new ToolStripMenuItem("&Help");
        helpMenu.DropDownItems.Add("&About", null, (s, e) => ShowAbout());

        menuStrip.Items.AddRange(new ToolStripItem[] { fileMenu, editMenu, viewMenu, helpMenu });

        return menuStrip;
    }

    private ToolStrip CreateToolStrip()
    {
        var toolStrip = new ToolStrip();

        toolStrip.Items.Add(new ToolStripButton("New", null, (s, e) => NewIcon()) { ToolTipText = "New Icon" });
        toolStrip.Items.Add(new ToolStripButton("Open", null, (s, e) => OpenFileDialog()) { ToolTipText = "Open File" });
        toolStrip.Items.Add(new ToolStripButton("Save", null, (s, e) => SaveFile()) { ToolTipText = "Save File" });
        toolStrip.Items.Add(new ToolStripSeparator());
        toolStrip.Items.Add(new ToolStripButton("Add", null, (s, e) => AddImage()) { ToolTipText = "Add Image" });
        toolStrip.Items.Add(new ToolStripButton("Export PNG", null, (s, e) => ExportSelectedToPng()) { ToolTipText = "Export Selected to PNG" });
        toolStrip.Items.Add(new ToolStripSeparator());
        toolStrip.Items.Add(new ToolStripButton("Zoom In", null, (s, e) => _iconEditor.ImageEditor.Zoom *= 1.25f));
        toolStrip.Items.Add(new ToolStripButton("Zoom Out", null, (s, e) => _iconEditor.ImageEditor.Zoom *= 0.8f));
        toolStrip.Items.Add(new ToolStripButton("Fit", null, (s, e) => _iconEditor.ImageEditor.ZoomToFit()));

        return toolStrip;
    }

    private void NewIcon()
    {
        if (!CheckSaveChanges()) return;

        _iconEditor.Icon = new BaconIcon { Name = "New Icon" };
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

    public void OpenFile(string filePath)
    {
        try
        {
            var icon = BaconIcon.LoadFromFile(filePath);
            if (icon != null)
            {
                _iconEditor.Icon = icon;
                _currentFilePath = filePath;
                _isModified = false;
                UpdateTitle();
                _statusLabel.Text = $"Opened: {Path.GetFileName(filePath)}";
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error opening file: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
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

                // Create a paint layer with the imported bitmap
                var paintLayer = new PaintLayer
                {
                    Name = "Imported",
                    Bitmap = (Bitmap)imageWithType.Image.Clone()
                };
                baconImage.Layers.Add(paintLayer);

                icon.Images.Add(baconImage);
                imageWithType.Dispose();
            }

            _iconEditor.Icon = icon;
            _currentFilePath = null; // ICO files are converted, not directly edited
            _isModified = true;
            UpdateTitle();
            _statusLabel.Text = $"Imported {images.Length} images from ICO file";
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

    private void SaveToFile(string path)
    {
        if (_iconEditor.Icon == null) return;

        try
        {
            _iconEditor.Icon.SaveToFile(path);
            _currentFilePath = path;
            _isModified = false;
            UpdateTitle();
            _statusLabel.Text = $"Saved: {Path.GetFileName(path)}";
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving file: {ex.Message}", "Error",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void ExportToIco()
    {
        if (_iconEditor.Icon == null || _iconEditor.Icon.Images.Count == 0)
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

                foreach (var image in _iconEditor.Icon.Images)
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

                _statusLabel.Text = $"Exported to: {Path.GetFileName(dialog.FileName)}";
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
        var selectedImage = _iconEditor.SelectedImage;
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
                _statusLabel.Text = $"Exported to: {Path.GetFileName(dialog.FileName)}";
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
        if (_iconEditor.Icon == null) return;

        using var dialog = new AddImageDialog();
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            var image = _iconEditor.Icon.AddImage(dialog.ImageWidth, dialog.ImageHeight);
            _iconEditor.RefreshThumbnails();
            _iconEditor.SelectedImage = image;
            _isModified = true;
            UpdateTitle();
        }
    }

    private void AddCommonSizes()
    {
        if (_iconEditor.Icon == null) return;

        _iconEditor.Icon.AddCommonSizes();
        _iconEditor.RefreshThumbnails();
        _isModified = true;
        UpdateTitle();
    }

    private void RemoveSelectedImage()
    {
        var selectedImage = _iconEditor.SelectedImage;
        if (selectedImage == null || _iconEditor.Icon == null) return;

        var result = MessageBox.Show(
            $"Are you sure you want to remove the {selectedImage.Width}x{selectedImage.Height} image?",
            "Remove Image",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (result == DialogResult.Yes)
        {
            _iconEditor.Icon.RemoveImage(selectedImage);
            _iconEditor.RefreshThumbnails();
            _isModified = true;
            UpdateTitle();
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
                return !_isModified; // Return true if save succeeded
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
        var selectedImage = _iconEditor.SelectedImage;
        if (selectedImage != null)
        {
            _statusLabel.Text = $"Selected: {selectedImage.Width} x {selectedImage.Height} | Layers: {selectedImage.Layers.Count}";
        }
        else
        {
            _statusLabel.Text = "Ready";
        }
    }

    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        if (!CheckSaveChanges())
        {
            e.Cancel = true;
        }
        base.OnFormClosing(e);
    }
}
