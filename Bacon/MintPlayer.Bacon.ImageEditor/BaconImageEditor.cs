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
    private readonly LayerTreeView _layersTree;
    private readonly Label _layersLabel;
    private readonly Label _emptyLayersLabel;

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
    /// Gets or sets whether shape control points are constrained to the image bounds when dragging.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool ConstrainShapesToImageBounds
    {
        get => _canvas.ConstrainShapesToImageBounds;
        set => _canvas.ConstrainShapesToImageBounds = value;
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
    /// All selected shapes.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IReadOnlyList<Shape> SelectedShapes => _canvas.SelectedShapes;

    /// <summary>
    /// The currently selected layer.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Layer? SelectedLayer => _layersTree.GetSelectedLayer();

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
        DoubleBuffered = true;

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
        _canvas.SelectionChanged += OnCanvasSelectionChanged;
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

        // Create layers tree
        _layersTree = new LayerTreeView
        {
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.FixedSingle
        };
        _layersTree.AfterSelect += OnLayerTreeSelectionChanged;

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
        _layersPanel.Controls.Add(_layersTree);
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

    private void OnCanvasSelectionChanged(object? sender, EventArgs e)
    {
        // Sync tree selection with canvas selection
        var selectedShape = _canvas.SelectedShape;
        if (selectedShape != null)
        {
            SelectNodeByTag(selectedShape);
        }
        SelectionChanged?.Invoke(this, e);
    }

    private void SelectNodeByTag(object tag)
    {
        foreach (TreeNode node in _layersTree.Nodes)
        {
            if (node.Tag == tag)
            {
                _layersTree.SelectedNode = node;
                return;
            }

            foreach (TreeNode childNode in node.Nodes)
            {
                if (childNode.Tag == tag)
                {
                    _layersTree.SelectedNode = childNode;
                    return;
                }
            }
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

        var deleteShapeItem = new ToolStripMenuItem("Delete Selected Shape(s)", null, OnDeleteSelectedShapes);

        contextMenu.Opening += (s, e) =>
        {
            addLayerMenu.Enabled = _image != null;
            addShapeMenu.Enabled = _image?.Layers.OfType<ShapeLayer>().Any() == true;
            deleteShapeItem.Enabled = _canvas.SelectedShapes.Count > 0;
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
        var addShapeMenu = new ToolStripMenuItem("Add Shape to Layer");
        addShapeMenu.DropDownItems.Add("Line Segment", null, (s, e) => OnAddShape("LineSegment"));
        addShapeMenu.DropDownItems.Add("Curve", null, (s, e) => OnAddShape("Curve"));
        addShapeMenu.DropDownItems.Add("Circle Segment", null, (s, e) => OnAddShape("CircleSegment"));
        addShapeMenu.DropDownItems.Add(new ToolStripSeparator());
        addShapeMenu.DropDownItems.Add("Rectangle", null, (s, e) => OnAddShape("Polygon"));
        addShapeMenu.DropDownItems.Add("Circle", null, (s, e) => OnAddShape("Circle"));
        addShapeMenu.DropDownItems.Add("Closed Curve", null, (s, e) => OnAddShape("ClosedCurve"));

        var duplicateItem = new ToolStripMenuItem("Duplicate", null, OnDuplicateSelected);
        var deleteItem = new ToolStripMenuItem("Delete", null, OnDeleteSelected);
        var moveUpItem = new ToolStripMenuItem("Move Up", null, OnMoveUp);
        var moveDownItem = new ToolStripMenuItem("Move Down", null, OnMoveDown);
        var toggleVisibilityItem = new ToolStripMenuItem("Toggle Visibility", null, OnToggleVisibility);
        var toggleLockItem = new ToolStripMenuItem("Toggle Lock", null, OnToggleLock);
        var renameItem = new ToolStripMenuItem("Rename...", null, OnRename);
        var propertiesItem = new ToolStripMenuItem("Properties...", null, OnShowProperties);

        contextMenu.Opening += (s, e) =>
        {
            var selectedNode = _layersTree.SelectedNode;
            var hasSelection = selectedNode != null;
            var isLayer = selectedNode?.Tag is Layer;
            var isShape = selectedNode?.Tag is Shape;
            var isShapeLayer = selectedNode?.Tag is ShapeLayer;
            var hasImage = _image != null;

            addShapeLayerItem.Enabled = hasImage;
            addPaintLayerItem.Enabled = hasImage;
            addShapeMenu.Enabled = isShapeLayer || (isShape && selectedNode?.Parent?.Tag is ShapeLayer);
            duplicateItem.Enabled = hasSelection;
            deleteItem.Enabled = hasSelection;
            moveUpItem.Enabled = hasSelection;
            moveDownItem.Enabled = hasSelection;
            toggleVisibilityItem.Enabled = hasSelection;
            toggleLockItem.Enabled = hasSelection;
            renameItem.Enabled = hasSelection;
            propertiesItem.Enabled = isShape;
        };

        contextMenu.Items.AddRange(new ToolStripItem[]
        {
            addShapeLayerItem,
            addPaintLayerItem,
            addShapeMenu,
            new ToolStripSeparator(),
            duplicateItem,
            deleteItem,
            new ToolStripSeparator(),
            moveUpItem,
            moveDownItem,
            new ToolStripSeparator(),
            toggleVisibilityItem,
            toggleLockItem,
            renameItem,
            new ToolStripSeparator(),
            propertiesItem
        });

        _layersTree.ContextMenuStrip = contextMenu;
    }

    private void OnLayerTreeSelectionChanged(object? sender, TreeViewEventArgs e)
    {
        // Sync canvas selection with tree selection
        if (e.Node?.Tag is Shape shape)
        {
            _canvas.SelectedShape = shape;
        }
        else if (e.Node?.Tag is Layer)
        {
            _canvas.ClearSelection();
        }
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
        SelectNodeByTag(layer);
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
        SelectNodeByTag(layer);
        ImageModified?.Invoke(this, EventArgs.Empty);
    }

    private void OnDuplicateSelected(object? sender, EventArgs e)
    {
        var selectedNode = _layersTree.SelectedNode;
        if (_image == null || selectedNode == null) return;

        if (selectedNode.Tag is Layer layer)
        {
            DuplicateLayer(layer);
        }
        else if (selectedNode.Tag is Shape shape)
        {
            DuplicateShape(shape);
        }
    }

    private void DuplicateLayer(Layer layer)
    {
        if (_image == null) return;

        Layer newLayer;
        if (layer is ShapeLayer)
        {
            newLayer = new ShapeLayer
            {
                Name = $"{layer.Name} (copy)",
                IsVisible = layer.IsVisible,
                IsLocked = false,
                Opacity = layer.Opacity
            };
        }
        else if (layer is PaintLayer paintLayer)
        {
            newLayer = new PaintLayer
            {
                Name = $"{layer.Name} (copy)",
                IsVisible = layer.IsVisible,
                IsLocked = false,
                Opacity = layer.Opacity,
                Bitmap = paintLayer.Bitmap != null ? new Bitmap(paintLayer.Bitmap) : null
            };
        }
        else
        {
            return;
        }

        var index = _image.Layers.IndexOf(layer);
        _image.Layers.Insert(index + 1, newLayer);
        RefreshLayersList();
        SelectNodeByTag(newLayer);
        ImageModified?.Invoke(this, EventArgs.Empty);
    }

    private void DuplicateShape(Shape shape)
    {
        if (_image == null) return;

        // Find the layer containing this shape
        foreach (var layer in _image.Layers.OfType<ShapeLayer>())
        {
            var index = layer.Shapes.IndexOf(shape);
            if (index >= 0)
            {
                // Create a simple copy by serializing and deserializing
                // For now, we'll just create a new shape at a slightly offset position
                // A proper implementation would need deep cloning
                // This is a simplified approach
                break;
            }
        }
    }

    private void OnDeleteSelected(object? sender, EventArgs e)
    {
        var selectedNode = _layersTree.SelectedNode;
        if (_image == null || selectedNode == null) return;

        if (selectedNode.Tag is Layer layer)
        {
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
        else if (selectedNode.Tag is Shape shape)
        {
            foreach (var shapeLayer in _image.Layers.OfType<ShapeLayer>())
            {
                if (shapeLayer.Shapes.Remove(shape))
                {
                    _canvas.ClearSelection();
                    RefreshLayersList();
                    ImageModified?.Invoke(this, EventArgs.Empty);
                    break;
                }
            }
        }
    }

    private void OnDeleteSelectedShapes(object? sender, EventArgs e)
    {
        if (_image == null || _canvas.SelectedShapes.Count == 0) return;

        var shapesToDelete = _canvas.SelectedShapes.ToList();
        foreach (var shape in shapesToDelete)
        {
            foreach (var layer in _image.Layers.OfType<ShapeLayer>())
            {
                if (layer.Shapes.Remove(shape))
                {
                    break;
                }
            }
        }

        _canvas.ClearSelection();
        RefreshLayersList();
        ImageModified?.Invoke(this, EventArgs.Empty);
    }

    private void OnMoveUp(object? sender, EventArgs e)
    {
        var selectedNode = _layersTree.SelectedNode;
        if (_image == null || selectedNode == null) return;

        if (selectedNode.Tag is Layer layer)
        {
            var index = _image.Layers.IndexOf(layer);
            if (index < _image.Layers.Count - 1) // Moving up means higher index (rendered later = on top)
            {
                _image.Layers.RemoveAt(index);
                _image.Layers.Insert(index + 1, layer);
                RefreshLayersList();
                SelectNodeByTag(layer);
                _canvas.Invalidate();
                ImageModified?.Invoke(this, EventArgs.Empty);
            }
        }
        else if (selectedNode.Tag is Shape shape && selectedNode.Parent?.Tag is ShapeLayer shapeLayer)
        {
            var index = shapeLayer.Shapes.IndexOf(shape);
            if (index < shapeLayer.Shapes.Count - 1)
            {
                shapeLayer.Shapes.RemoveAt(index);
                shapeLayer.Shapes.Insert(index + 1, shape);
                RefreshLayersList();
                SelectNodeByTag(shape);
                _canvas.Invalidate();
                ImageModified?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    private void OnMoveDown(object? sender, EventArgs e)
    {
        var selectedNode = _layersTree.SelectedNode;
        if (_image == null || selectedNode == null) return;

        if (selectedNode.Tag is Layer layer)
        {
            var index = _image.Layers.IndexOf(layer);
            if (index > 0) // Moving down means lower index (rendered earlier = behind)
            {
                _image.Layers.RemoveAt(index);
                _image.Layers.Insert(index - 1, layer);
                RefreshLayersList();
                SelectNodeByTag(layer);
                _canvas.Invalidate();
                ImageModified?.Invoke(this, EventArgs.Empty);
            }
        }
        else if (selectedNode.Tag is Shape shape && selectedNode.Parent?.Tag is ShapeLayer shapeLayer)
        {
            var index = shapeLayer.Shapes.IndexOf(shape);
            if (index > 0)
            {
                shapeLayer.Shapes.RemoveAt(index);
                shapeLayer.Shapes.Insert(index - 1, shape);
                RefreshLayersList();
                SelectNodeByTag(shape);
                _canvas.Invalidate();
                ImageModified?.Invoke(this, EventArgs.Empty);
            }
        }
    }

    private void OnToggleVisibility(object? sender, EventArgs e)
    {
        var selectedNode = _layersTree.SelectedNode;
        if (selectedNode == null) return;

        if (selectedNode.Tag is Layer layer)
        {
            layer.IsVisible = !layer.IsVisible;
        }
        else if (selectedNode.Tag is Shape shape)
        {
            shape.IsVisible = !shape.IsVisible;
        }

        RefreshLayersList();
        _canvas.Invalidate();
        ImageModified?.Invoke(this, EventArgs.Empty);
    }

    private void OnToggleLock(object? sender, EventArgs e)
    {
        var selectedNode = _layersTree.SelectedNode;
        if (selectedNode == null) return;

        if (selectedNode.Tag is Layer layer)
        {
            layer.IsLocked = !layer.IsLocked;
        }
        else if (selectedNode.Tag is Shape shape)
        {
            shape.IsLocked = !shape.IsLocked;
        }

        RefreshLayersList();
        ImageModified?.Invoke(this, EventArgs.Empty);
    }

    private void OnRename(object? sender, EventArgs e)
    {
        var selectedNode = _layersTree.SelectedNode;
        if (selectedNode == null) return;

        string? currentName = null;
        if (selectedNode.Tag is Layer layer)
        {
            currentName = layer.Name;
        }
        else if (selectedNode.Tag is Shape shape)
        {
            currentName = shape.Name;
        }

        var newName = ShowInputDialog("Rename", "Enter new name:", currentName ?? "");
        if (newName != null)
        {
            if (selectedNode.Tag is Layer l)
            {
                l.Name = newName;
            }
            else if (selectedNode.Tag is Shape s)
            {
                s.Name = newName;
            }

            RefreshLayersList();
            ImageModified?.Invoke(this, EventArgs.Empty);
        }
    }

    private void OnShowProperties(object? sender, EventArgs e)
    {
        var selectedNode = _layersTree.SelectedNode;
        if (selectedNode?.Tag is not Shape shape) return;

        using var dialog = new ShapePropertiesDialog(shape);
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _canvas.Invalidate();
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

        // Use selected shape layer or find the first one
        var shapeLayer = _layersTree.GetSelectedLayer() as ShapeLayer
            ?? _image.Layers.OfType<ShapeLayer>().FirstOrDefault();

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
        SelectNodeByTag(shape);
        ImageModified?.Invoke(this, EventArgs.Empty);
    }

    private void RefreshLayersList()
    {
        if (_image == null || _image.Layers.Count == 0)
        {
            _layersTree.Nodes.Clear();
            _emptyLayersLabel.Visible = true;
            return;
        }

        _emptyLayersLabel.Visible = false;
        _layersTree.PopulateLayers(_image.Layers);
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
