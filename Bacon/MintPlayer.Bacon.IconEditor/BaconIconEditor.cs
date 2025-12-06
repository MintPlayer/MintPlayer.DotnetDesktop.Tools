using System.ComponentModel;
using System.Drawing;
using MintPlayer.Bacon.Icons;
using MintPlayer.Bacon.Images;
using MintPlayer.Bacon.ImageEditor;

namespace MintPlayer.Bacon.IconEditor;

/// <summary>
/// A UserControl for editing BaconIcon files.
/// Displays a list of images on the left and the image editor on the right.
/// </summary>
public class BaconIconEditor : UserControl
{
    private BaconIcon? _icon;
    private readonly SplitContainer _splitContainer;
    private readonly ImageThumbnailListBox _imageList;
    private readonly BaconImageEditor _imageEditor;
    private readonly Panel _listPanel;
    private readonly Label _emptyLabel;

    /// <summary>
    /// The icon being edited.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public BaconIcon? Icon
    {
        get => _icon;
        set
        {
            _icon = value;
            RefreshImageList();
            IconChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// The currently selected image.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public BaconImage? SelectedImage
    {
        get => _imageList.SelectedItem as BaconImage;
        set
        {
            if (value != null && _imageList.Items.Contains(value))
            {
                _imageList.SelectedItem = value;
            }
        }
    }

    /// <summary>
    /// The image editor control.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public BaconImageEditor ImageEditor => _imageEditor;

    /// <summary>
    /// Event fired when the icon changes.
    /// </summary>
    public event EventHandler? IconChanged;

    /// <summary>
    /// Event fired when the selected image changes.
    /// </summary>
    public event EventHandler? SelectedImageChanged;

    /// <summary>
    /// Event fired when an image is modified.
    /// </summary>
    public event EventHandler? ImageModified;

    public BaconIconEditor()
    {
        // Set a minimum size to ensure the split container has enough space
        MinimumSize = new Size(400, 300);
        Size = new Size(600, 400);

        // Create main split container
        _splitContainer = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            SplitterWidth = 5,
            // Set min sizes to 0 initially to avoid SplitterDistance validation errors
            Panel1MinSize = 0,
            Panel2MinSize = 0
        };

        // Create list panel
        _listPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(4)
        };

        // Create image list
        _imageList = new ImageThumbnailListBox
        {
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.FixedSingle
        };
        _imageList.SelectedIndexChanged += OnImageListSelectedIndexChanged;

        // Create empty state label
        _emptyLabel = new Label
        {
            Text = "No images in icon.\nAdd images using the context menu.",
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Fill,
            ForeColor = SystemColors.GrayText,
            Visible = true
        };

        // Create image editor
        _imageEditor = new BaconImageEditor
        {
            Dock = DockStyle.Fill
        };
        _imageEditor.ImageModified += OnImageEditorModified;

        // Assembly hierarchy
        _listPanel.Controls.Add(_imageList);
        _listPanel.Controls.Add(_emptyLabel);
        _emptyLabel.BringToFront();

        _splitContainer.Panel1.Controls.Add(_listPanel);
        _splitContainer.Panel2.Controls.Add(_imageEditor);

        Controls.Add(_splitContainer);

        // Setup context menu
        SetupContextMenu();

        // Set splitter distance after load to avoid validation errors
        Load += OnLoad;
    }

    private void OnLoad(object? sender, EventArgs e)
    {
        // Now that the control has a proper size, set the panel min sizes and splitter distance
        if (_splitContainer.Width > 250)
        {
            _splitContainer.Panel1MinSize = 100;
            _splitContainer.Panel2MinSize = 150;
            _splitContainer.SplitterDistance = 150;
        }
    }

    private void SetupContextMenu()
    {
        var contextMenu = new ContextMenuStrip();

        var addImageItem = new ToolStripMenuItem("Add Image...", null, OnAddImage);
        var addCommonSizesItem = new ToolStripMenuItem("Add Common Sizes", null, OnAddCommonSizes);
        var removeImageItem = new ToolStripMenuItem("Remove Image", null, OnRemoveImage);
        var duplicateImageItem = new ToolStripMenuItem("Duplicate Image", null, OnDuplicateImage);

        contextMenu.Opening += (s, e) =>
        {
            var hasSelection = _imageList.SelectedItem != null;
            removeImageItem.Enabled = hasSelection;
            duplicateImageItem.Enabled = hasSelection;
        };

        contextMenu.Items.AddRange(new ToolStripItem[]
        {
            addImageItem,
            addCommonSizesItem,
            new ToolStripSeparator(),
            duplicateImageItem,
            removeImageItem
        });

        _imageList.ContextMenuStrip = contextMenu;
    }

    private void OnAddImage(object? sender, EventArgs e)
    {
        if (_icon == null) return;

        using var dialog = new AddImageDialog();
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            var image = _icon.AddImage(dialog.ImageWidth, dialog.ImageHeight);
            RefreshImageList();
            _imageList.SelectedItem = image;
            ImageModified?.Invoke(this, EventArgs.Empty);
        }
    }

    private void OnAddCommonSizes(object? sender, EventArgs e)
    {
        if (_icon == null) return;

        _icon.AddCommonSizes();
        RefreshImageList();
        ImageModified?.Invoke(this, EventArgs.Empty);
    }

    private void OnRemoveImage(object? sender, EventArgs e)
    {
        if (_icon == null || _imageList.SelectedItem is not BaconImage image) return;

        var result = MessageBox.Show(
            $"Are you sure you want to remove the {image.Width}x{image.Height} image?",
            "Remove Image",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (result == DialogResult.Yes)
        {
            _icon.RemoveImage(image);
            RefreshImageList();
            ImageModified?.Invoke(this, EventArgs.Empty);
        }
    }

    private void OnDuplicateImage(object? sender, EventArgs e)
    {
        if (_icon == null || _imageList.SelectedItem is not BaconImage image) return;

        var clone = image.Clone();
        clone.Name = $"{image.Name ?? $"{image.Width}x{image.Height}"} (copy)";
        _icon.Images.Add(clone);
        RefreshImageList();
        _imageList.SelectedItem = clone;
        ImageModified?.Invoke(this, EventArgs.Empty);
    }

    private void RefreshImageList()
    {
        _imageList.Items.Clear();

        if (_icon == null || _icon.Images.Count == 0)
        {
            _emptyLabel.Visible = true;
            _imageEditor.Image = null;
            return;
        }

        _emptyLabel.Visible = false;

        foreach (var image in _icon.Images)
        {
            _imageList.Items.Add(image);
        }

        if (_imageList.Items.Count > 0)
        {
            _imageList.SelectedIndex = 0;
        }
    }

    private void OnImageListSelectedIndexChanged(object? sender, EventArgs e)
    {
        var selectedImage = _imageList.SelectedItem as BaconImage;
        _imageEditor.Image = selectedImage;
        SelectedImageChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnImageEditorModified(object? sender, EventArgs e)
    {
        // Refresh the thumbnail when the image is modified
        _imageList.Invalidate();
        ImageModified?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Refresh the thumbnail list.
    /// </summary>
    public void RefreshThumbnails()
    {
        _imageList.Invalidate();
    }
}
