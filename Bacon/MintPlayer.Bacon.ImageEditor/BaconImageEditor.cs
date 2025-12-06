using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using MintPlayer.Bacon.Images;
using MintPlayer.Bacon.Images.Data;
using MintPlayer.Bacon.Images.Layers;
using MintPlayer.Bacon.Images.Shapes;

namespace MintPlayer.Bacon.ImageEditor;

/// <summary>
/// A UserControl for editing BaconImage files.
/// Contains a canvas for drawing and a layers panel on the right.
/// </summary>
public class BaconImageEditor : UserControl
{
    private BaconImage? _image;
    private readonly SplitContainer _splitContainer;
    private readonly ImageCanvas _canvas;
    private readonly Panel _layersPanel;
    private readonly LayerListBox _layersList;
    private readonly Label _layersLabel;
    private readonly Label _emptyLayersLabel;
    private int _selectedLayerIndex = -1;

    /// <summary>
    /// The image being edited.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public BaconImage? Image
    {
        get => _image;
        set
        {
            _image = value;
            _canvas.Image = value;
            _selectedLayerIndex = -1;
            RefreshLayersList();
            ImageChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Current zoom level.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public float Zoom
    {
        get => _canvas.Zoom;
        set => _canvas.Zoom = value;
    }

    /// <summary>
    /// Currently selected shape.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Shape? SelectedShape
    {
        get => _canvas.SelectedShape;
        set => _canvas.SelectedShape = value;
    }

    /// <summary>
    /// Currently selected layer index.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public int SelectedLayerIndex
    {
        get => _selectedLayerIndex;
        set
        {
            _selectedLayerIndex = value;
            if (value >= 0 && value < _layersList.Items.Count)
            {
                _layersList.SelectedIndex = value;
            }
        }
    }

    /// <summary>
    /// The currently selected layer.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Layer? SelectedLayer => _layersList.SelectedItem as Layer;

    /// <summary>
    /// Event fired when the image changes.
    /// </summary>
    public event EventHandler? ImageChanged;

    /// <summary>
    /// Event fired when the zoom level changes.
    /// </summary>
    public event EventHandler? ZoomChanged;

    /// <summary>
    /// Event fired when the selection changes.
    /// </summary>
    public event EventHandler? SelectionChanged;

    /// <summary>
    /// Event fired when the image content is modified.
    /// </summary>
    public event EventHandler? ImageModified;

    /// <summary>
    /// Event fired when the selected layer changes.
    /// </summary>
    public event EventHandler? SelectedLayerChanged;

    public BaconImageEditor()
    {
        MinimumSize = new Size(400, 300);
        Size = new Size(600, 400);

        // Create split container
        _splitContainer = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            SplitterWidth = 5,
            Panel1MinSize = 0,
            Panel2MinSize = 0
        };

        // Create canvas
        _canvas = new ImageCanvas
        {
            Dock = DockStyle.Fill
        };
        _canvas.ZoomChanged += (s, e) => ZoomChanged?.Invoke(this, e);
        _canvas.SelectionChanged += (s, e) => SelectionChanged?.Invoke(this, e);
        _canvas.ImageModified += (s, e) =>
        {
            RefreshLayersList();
            ImageModified?.Invoke(this, e);
        };

        // Create layers panel
        _layersPanel = new Panel
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(4)
        };

        // Create layers label
        _layersLabel = new Label
        {
            Text = "Layers",
            Dock = DockStyle.Top,
            Height = 20,
            Font = new Font(Font.FontFamily, 9, FontStyle.Bold),
            TextAlign = ContentAlignment.MiddleLeft
        };

        // Create layers list
        _layersList = new LayerListBox
        {
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.FixedSingle
        };
        _layersList.SelectedIndexChanged += OnLayerSelectionChanged;

        // Create empty state label
        _emptyLayersLabel = new Label
        {
            Text = "No layers.\nRight-click to add.",
            TextAlign = ContentAlignment.MiddleCenter,
            Dock = DockStyle.Fill,
            ForeColor = SystemColors.GrayText,
            Visible = true
        };

        // Assemble layers panel
        _layersPanel.Controls.Add(_layersList);
        _layersPanel.Controls.Add(_emptyLayersLabel);
        _layersPanel.Controls.Add(_layersLabel);
        _emptyLayersLabel.BringToFront();

        // Assemble split container
        _splitContainer.Panel1.Controls.Add(_canvas);
        _splitContainer.Panel2.Controls.Add(_layersPanel);

        Controls.Add(_splitContainer);

        // Setup context menus
        SetupCanvasContextMenu();
        SetupLayersContextMenu();

        // Set splitter distance after load
        Load += OnLoad;
    }

    private void OnLoad(object? sender, EventArgs e)
    {
        if (_splitContainer.Width > 250)
        {
            _splitContainer.Panel1MinSize = 150;
            _splitContainer.Panel2MinSize = 100;
            _splitContainer.SplitterDistance = _splitContainer.Width - 180;
        }
    }

    private void SetupCanvasContextMenu()
    {
        var contextMenu = new ContextMenuStrip();

        var addLayerMenu = new ToolStripMenuItem("Add Layer");
        addLayerMenu.DropDownItems.Add("Shape Layer", null, OnAddShapeLayer);
        addLayerMenu.DropDownItems.Add("Paint Layer", null, OnAddPaintLayer);

        var addShapeMenu = new ToolStripMenuItem("Add Shape");
        addShapeMenu.DropDownItems.Add("Line Segment", null, (s, e) => OnAddShape("LineSegment"));
        addShapeMenu.DropDownItems.Add("Curve", null, (s, e) => OnAddShape("Curve"));
        addShapeMenu.DropDownItems.Add("Circle Segment", null, (s, e) => OnAddShape("CircleSegment"));
        addShapeMenu.DropDownItems.Add(new ToolStripSeparator());
        addShapeMenu.DropDownItems.Add("Rectangle", null, (s, e) => OnAddShape("Polygon"));
        addShapeMenu.DropDownItems.Add("Circle", null, (s, e) => OnAddShape("Circle"));
        addShapeMenu.DropDownItems.Add("Closed Curve", null, (s, e) => OnAddShape("ClosedCurve"));

        var deleteShapeItem = new ToolStripMenuItem("Delete Selected Shape", null, OnDeleteSelectedShape);

        contextMenu.Opening += (s, e) =>
        {
            addLayerMenu.Enabled = _image != null;
            addShapeMenu.Enabled = _image?.Layers.OfType<ShapeLayer>().Any() == true;
            deleteShapeItem.Enabled = _canvas.SelectedShape != null;
        };

        contextMenu.Items.AddRange(new ToolStripItem[]
        {
            addLayerMenu,
            addShapeMenu,
            new ToolStripSeparator(),
            deleteShapeItem
        });

        _canvas.ContextMenuStrip = contextMenu;
    }

    private void SetupLayersContextMenu()
    {
        var contextMenu = new ContextMenuStrip();

        var addShapeLayerItem = new ToolStripMenuItem("Add Shape Layer", null, OnAddShapeLayer);
        var addPaintLayerItem = new ToolStripMenuItem("Add Paint Layer", null, OnAddPaintLayer);
        var duplicateLayerItem = new ToolStripMenuItem("Duplicate Layer", null, OnDuplicateLayer);
        var deleteLayerItem = new ToolStripMenuItem("Delete Layer", null, OnDeleteLayer);
        var moveUpItem = new ToolStripMenuItem("Move Up", null, OnMoveLayerUp);
        var moveDownItem = new ToolStripMenuItem("Move Down", null, OnMoveLayerDown);
        var toggleVisibilityItem = new ToolStripMenuItem("Toggle Visibility", null, OnToggleLayerVisibility);
        var toggleLockItem = new ToolStripMenuItem("Toggle Lock", null, OnToggleLayerLock);
        var renameLayerItem = new ToolStripMenuItem("Rename...", null, OnRenameLayer);

        contextMenu.Opening += (s, e) =>
        {
            var hasSelection = _layersList.SelectedItem != null;
            var hasImage = _image != null;
            var selectedIndex = _layersList.SelectedIndex;

            addShapeLayerItem.Enabled = hasImage;
            addPaintLayerItem.Enabled = hasImage;
            duplicateLayerItem.Enabled = hasSelection;
            deleteLayerItem.Enabled = hasSelection;
            moveUpItem.Enabled = hasSelection && selectedIndex > 0;
            moveDownItem.Enabled = hasSelection && hasImage && selectedIndex < _image!.Layers.Count - 1;
            toggleVisibilityItem.Enabled = hasSelection;
            toggleLockItem.Enabled = hasSelection;
            renameLayerItem.Enabled = hasSelection;
        };

        contextMenu.Items.AddRange(new ToolStripItem[]
        {
            addShapeLayerItem,
            addPaintLayerItem,
            new ToolStripSeparator(),
            duplicateLayerItem,
            deleteLayerItem,
            new ToolStripSeparator(),
            moveUpItem,
            moveDownItem,
            new ToolStripSeparator(),
            toggleVisibilityItem,
            toggleLockItem,
            renameLayerItem
        });

        _layersList.ContextMenuStrip = contextMenu;
    }

    private void OnLayerSelectionChanged(object? sender, EventArgs e)
    {
        _selectedLayerIndex = _layersList.SelectedIndex;
        SelectedLayerChanged?.Invoke(this, EventArgs.Empty);
    }

    private void OnAddShapeLayer(object? sender, EventArgs e)
    {
        if (_image == null) return;

        var layer = new ShapeLayer
        {
            Name = $"Shape Layer {_image.Layers.Count + 1}"
        };
        _image.Layers.Add(layer);
        RefreshLayersList();
        _layersList.SelectedItem = layer;
        ImageModified?.Invoke(this, EventArgs.Empty);
    }

    private void OnAddPaintLayer(object? sender, EventArgs e)
    {
        if (_image == null) return;

        var layer = new PaintLayer
        {
            Name = $"Paint Layer {_image.Layers.Count + 1}"
        };
        _image.Layers.Add(layer);
        RefreshLayersList();
        _layersList.SelectedItem = layer;
        ImageModified?.Invoke(this, EventArgs.Empty);
    }

    private void OnDuplicateLayer(object? sender, EventArgs e)
    {
        if (_image == null || _layersList.SelectedItem is not Layer layer) return;

        Layer newLayer;
        if (layer is ShapeLayer shapeLayer)
        {
            var clone = new ShapeLayer
            {
                Name = $"{layer.Name} (copy)",
                IsVisible = layer.IsVisible,
                IsLocked = false,
                Opacity = layer.Opacity
            };
            // Clone shapes would require deep copy - for now just create empty
            newLayer = clone;
        }
        else if (layer is PaintLayer paintLayer)
        {
            var clone = new PaintLayer
            {
                Name = $"{layer.Name} (copy)",
                IsVisible = layer.IsVisible,
                IsLocked = false,
                Opacity = layer.Opacity,
                Bitmap = paintLayer.Bitmap != null ? new Bitmap(paintLayer.Bitmap) : null
            };
            newLayer = clone;
        }
        else
        {
            return;
        }

        var index = _image.Layers.IndexOf(layer);
        _image.Layers.Insert(index + 1, newLayer);
        RefreshLayersList();
        _layersList.SelectedItem = newLayer;
        ImageModified?.Invoke(this, EventArgs.Empty);
    }

    private void OnDeleteLayer(object? sender, EventArgs e)
    {
        if (_image == null || _layersList.SelectedItem is not Layer layer) return;

        var result = MessageBox.Show(
            $"Are you sure you want to delete the layer '{layer.Name}'?",
            "Delete Layer",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (result == DialogResult.Yes)
        {
            _image.Layers.Remove(layer);
            RefreshLayersList();
            _canvas.Invalidate();
            ImageModified?.Invoke(this, EventArgs.Empty);
        }
    }

    private void OnMoveLayerUp(object? sender, EventArgs e)
    {
        if (_image == null || _layersList.SelectedItem is not Layer layer) return;

        var index = _image.Layers.IndexOf(layer);
        if (index > 0)
        {
            _image.Layers.RemoveAt(index);
            _image.Layers.Insert(index - 1, layer);
            RefreshLayersList();
            _layersList.SelectedItem = layer;
            _canvas.Invalidate();
            ImageModified?.Invoke(this, EventArgs.Empty);
        }
    }

    private void OnMoveLayerDown(object? sender, EventArgs e)
    {
        if (_image == null || _layersList.SelectedItem is not Layer layer) return;

        var index = _image.Layers.IndexOf(layer);
        if (index < _image.Layers.Count - 1)
        {
            _image.Layers.RemoveAt(index);
            _image.Layers.Insert(index + 1, layer);
            RefreshLayersList();
            _layersList.SelectedItem = layer;
            _canvas.Invalidate();
            ImageModified?.Invoke(this, EventArgs.Empty);
        }
    }

    private void OnToggleLayerVisibility(object? sender, EventArgs e)
    {
        if (_layersList.SelectedItem is not Layer layer) return;

        layer.IsVisible = !layer.IsVisible;
        _layersList.Invalidate();
        _canvas.Invalidate();
        ImageModified?.Invoke(this, EventArgs.Empty);
    }

    private void OnToggleLayerLock(object? sender, EventArgs e)
    {
        if (_layersList.SelectedItem is not Layer layer) return;

        layer.IsLocked = !layer.IsLocked;
        _layersList.Invalidate();
        ImageModified?.Invoke(this, EventArgs.Empty);
    }

    private void OnRenameLayer(object? sender, EventArgs e)
    {
        if (_layersList.SelectedItem is not Layer layer) return;

        var newName = ShowInputDialog("Rename Layer", "Enter new name:", layer.Name ?? "");
        if (newName != null)
        {
            layer.Name = newName;
            _layersList.Invalidate();
            ImageModified?.Invoke(this, EventArgs.Empty);
        }
    }

    private string? ShowInputDialog(string title, string prompt, string defaultValue)
    {
        using var form = new Form
        {
            Text = title,
            Width = 300,
            Height = 150,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            StartPosition = FormStartPosition.CenterParent,
            MaximizeBox = false,
            MinimizeBox = false
        };

        var label = new Label { Left = 10, Top = 15, Text = prompt, Width = 260 };
        var textBox = new TextBox { Left = 10, Top = 40, Width = 260, Text = defaultValue };
        var okButton = new Button { Text = "OK", Left = 110, Top = 75, Width = 75, DialogResult = DialogResult.OK };
        var cancelButton = new Button { Text = "Cancel", Left = 195, Top = 75, Width = 75, DialogResult = DialogResult.Cancel };

        form.Controls.AddRange(new Control[] { label, textBox, okButton, cancelButton });
        form.AcceptButton = okButton;
        form.CancelButton = cancelButton;

        return form.ShowDialog(this) == DialogResult.OK ? textBox.Text : null;
    }

    private void OnAddShape(string shapeType)
    {
        if (_image == null) return;

        var shapeLayer = SelectedLayer as ShapeLayer ?? _image.Layers.OfType<ShapeLayer>().FirstOrDefault();
        if (shapeLayer == null)
        {
            shapeLayer = new ShapeLayer { Name = "Shape Layer 1" };
            _image.Layers.Add(shapeLayer);
            RefreshLayersList();
        }

        Shape shape;
        var centerX = _image.Width / 2f;
        var centerY = _image.Height / 2f;
        var size = Math.Min(_image.Width, _image.Height) / 3f;

        switch (shapeType)
        {
            case "LineSegment":
                shape = new LineSegment
                {
                    Start = new SerializablePoint(centerX - size, centerY),
                    End = new SerializablePoint(centerX + size, centerY),
                    Pen = new SerializablePen(Color.Black, 2)
                };
                break;
            case "Curve":
                shape = new Curve
                {
                    Points =
                    [
                        new SerializablePoint(centerX - size, centerY),
                        new SerializablePoint(centerX - size / 2, centerY - size),
                        new SerializablePoint(centerX + size / 2, centerY + size),
                        new SerializablePoint(centerX + size, centerY)
                    ],
                    Pen = new SerializablePen(Color.Black, 2)
                };
                break;
            case "CircleSegment":
                shape = new CircleSegment
                {
                    Point1 = new SerializablePoint(centerX - size, centerY),
                    Point2 = new SerializablePoint(centerX, centerY - size),
                    Point3 = new SerializablePoint(centerX + size, centerY),
                    Pen = new SerializablePen(Color.Black, 2)
                };
                break;
            case "Polygon":
                shape = new Polygon
                {
                    Points =
                    [
                        new SerializablePoint(centerX - size, centerY - size / 2),
                        new SerializablePoint(centerX + size, centerY - size / 2),
                        new SerializablePoint(centerX + size, centerY + size / 2),
                        new SerializablePoint(centerX - size, centerY + size / 2)
                    ],
                    Pen = new SerializablePen(Color.Black, 2),
                    Fill = new SerializableSolidBrush(Color.FromArgb(128, 100, 150, 200))
                };
                break;
            case "Circle":
                shape = new Circle
                {
                    Center = new SerializablePoint(centerX, centerY),
                    EdgePoint = new SerializablePoint(centerX + size, centerY),
                    Pen = new SerializablePen(Color.Black, 2),
                    Fill = new SerializableSolidBrush(Color.FromArgb(128, 100, 150, 200))
                };
                break;
            case "ClosedCurve":
                shape = new ClosedCurve
                {
                    Points =
                    [
                        new SerializablePoint(centerX, centerY - size),
                        new SerializablePoint(centerX + size, centerY),
                        new SerializablePoint(centerX, centerY + size),
                        new SerializablePoint(centerX - size, centerY)
                    ],
                    Pen = new SerializablePen(Color.Black, 2),
                    Fill = new SerializableSolidBrush(Color.FromArgb(128, 100, 150, 200))
                };
                break;
            default:
                return;
        }

        shapeLayer.Shapes.Add(shape);
        _canvas.SelectedShape = shape;
        RefreshLayersList();
        ImageModified?.Invoke(this, EventArgs.Empty);
    }

    private void OnDeleteSelectedShape(object? sender, EventArgs e)
    {
        if (_image == null || _canvas.SelectedShape == null) return;

        foreach (var layer in _image.Layers.OfType<ShapeLayer>())
        {
            if (layer.Shapes.Remove(_canvas.SelectedShape))
            {
                _canvas.SelectedShape = null;
                RefreshLayersList();
                ImageModified?.Invoke(this, EventArgs.Empty);
                break;
            }
        }
    }

    private void RefreshLayersList()
    {
        var selectedLayer = _layersList.SelectedItem as Layer;
        _layersList.Items.Clear();

        if (_image == null || _image.Layers.Count == 0)
        {
            _emptyLayersLabel.Visible = true;
            return;
        }

        _emptyLayersLabel.Visible = false;

        // Add layers in reverse order (top layer first in list)
        for (int i = _image.Layers.Count - 1; i >= 0; i--)
        {
            _layersList.Items.Add(_image.Layers[i]);
        }

        // Restore selection
        if (selectedLayer != null && _layersList.Items.Contains(selectedLayer))
        {
            _layersList.SelectedItem = selectedLayer;
        }
        else if (_layersList.Items.Count > 0)
        {
            _layersList.SelectedIndex = 0;
        }
    }

    /// <summary>
    /// Center the image in the view.
    /// </summary>
    public void CenterImage() => _canvas.CenterImage();

    /// <summary>
    /// Zoom to fit the image in the view.
    /// </summary>
    public void ZoomToFit() => _canvas.ZoomToFit();

    /// <summary>
    /// Refresh the canvas.
    /// </summary>
    public void RefreshCanvas() => _canvas.Invalidate();

    /// <summary>
    /// Refresh the layers list.
    /// </summary>
    public void RefreshLayers() => RefreshLayersList();
}
