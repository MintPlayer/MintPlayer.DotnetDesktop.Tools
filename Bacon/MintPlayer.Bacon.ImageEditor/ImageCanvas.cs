using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using MintPlayer.Bacon.Images;
using MintPlayer.Bacon.Images.Data;
using MintPlayer.Bacon.Images.Layers;
using MintPlayer.Bacon.Images.Shapes;

namespace MintPlayer.Bacon.ImageEditor;

/// <summary>
/// The canvas control for rendering and editing BaconImage content.
/// </summary>
internal class ImageCanvas : UserControl
{
    private BaconImage? _image;
    private float _zoom = 1.0f;
    private PointF _panOffset = PointF.Empty;
    private bool _isPanning;
    private Point _lastMousePos;
    private readonly List<Shape> _selectedShapes = [];
    private int _selectedControlPointIndex = -1;
    private bool _isDraggingControlPoint;
    private bool _isDraggingShapes;
    private bool _isHoveringMoveHandle;
    private PointF _dragStartImagePoint;

    private const int CheckerboardSize = 8;
    private static readonly Color CheckerColor1 = Color.FromArgb(204, 204, 204);
    private static readonly Color CheckerColor2 = Color.FromArgb(255, 255, 255);
    private static readonly Color BackgroundColor = Color.FromArgb(173, 216, 230); // Light blue
    private const float ControlPointRadius = 5f;
    private const float MoveHandleSize = 12f;
    private static readonly Color ControlPointColor = Color.FromArgb(0, 120, 215);
    private static readonly Color ControlPointHoverColor = Color.FromArgb(255, 165, 0);
    private static readonly Color SelectedShapeColor = Color.FromArgb(0, 120, 215);
    private static readonly Color MoveHandleColor = Color.FromArgb(0, 120, 215);
    private static readonly Color MoveHandleHoverColor = Color.FromArgb(255, 165, 0);

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public BaconImage? Image
    {
        get => _image;
        set
        {
            _image = value;
            _selectedShapes.Clear();
            _selectedControlPointIndex = -1;
            CenterImage();
            Invalidate();
        }
    }

    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public float Zoom
    {
        get => _zoom;
        set
        {
            _zoom = Math.Max(0.1f, Math.Min(10f, value));
            Invalidate();
            ZoomChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Gets the first selected shape (for backward compatibility).
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Shape? SelectedShape
    {
        get => _selectedShapes.FirstOrDefault();
        set
        {
            _selectedShapes.Clear();
            if (value != null)
            {
                _selectedShapes.Add(value);
            }
            Invalidate();
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Gets all selected shapes.
    /// </summary>
    [Browsable(false)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IReadOnlyList<Shape> SelectedShapes => _selectedShapes;

    public event EventHandler? ZoomChanged;
    public event EventHandler? SelectionChanged;
    public event EventHandler? ImageModified;

    public ImageCanvas()
    {
        SetStyle(ControlStyles.AllPaintingInWmPaint |
                 ControlStyles.UserPaint |
                 ControlStyles.DoubleBuffer |
                 ControlStyles.ResizeRedraw, true);

        BackColor = BackgroundColor;
    }

    /// <summary>
    /// Add a shape to the selection.
    /// </summary>
    public void AddToSelection(Shape shape)
    {
        if (!_selectedShapes.Contains(shape))
        {
            _selectedShapes.Add(shape);
            Invalidate();
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Remove a shape from the selection.
    /// </summary>
    public void RemoveFromSelection(Shape shape)
    {
        if (_selectedShapes.Remove(shape))
        {
            Invalidate();
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Clear the selection.
    /// </summary>
    public void ClearSelection()
    {
        if (_selectedShapes.Count > 0)
        {
            _selectedShapes.Clear();
            Invalidate();
            SelectionChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>
    /// Check if a shape is selected.
    /// </summary>
    public bool IsSelected(Shape shape) => _selectedShapes.Contains(shape);

    public void CenterImage()
    {
        if (_image == null) return;

        var imageWidth = _image.Width * _zoom;
        var imageHeight = _image.Height * _zoom;

        _panOffset = new PointF(
            (Width - imageWidth) / 2,
            (Height - imageHeight) / 2);

        Invalidate();
    }

    public void ZoomToFit()
    {
        if (_image == null) return;

        var zoomX = (Width - 40) / (float)_image.Width;
        var zoomY = (Height - 40) / (float)_image.Height;
        Zoom = Math.Min(zoomX, zoomY);
        CenterImage();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.InterpolationMode = InterpolationMode.NearestNeighbor;

        g.Clear(BackgroundColor);

        if (_image == null) return;

        var imageBounds = new RectangleF(
            _panOffset.X, _panOffset.Y,
            _image.Width * _zoom, _image.Height * _zoom);

        DrawCheckerboard(g, imageBounds);

        var oldTransform = g.Transform;
        g.TranslateTransform(_panOffset.X, _panOffset.Y);
        g.ScaleTransform(_zoom, _zoom);

        foreach (var layer in _image.Layers)
        {
            layer.Draw(g, _image.Width, _image.Height);
        }

        DrawSelectionAndControlPoints(g);

        g.Transform = oldTransform;

        using var borderPen = new Pen(Color.Gray, 1);
        g.DrawRectangle(borderPen, imageBounds.X, imageBounds.Y, imageBounds.Width, imageBounds.Height);
    }

    private void DrawCheckerboard(Graphics g, RectangleF bounds)
    {
        var checkerSize = CheckerboardSize * _zoom;

        using var brush1 = new SolidBrush(CheckerColor1);
        using var brush2 = new SolidBrush(CheckerColor2);

        var oldClip = g.Clip;
        g.SetClip(bounds);

        var startX = (int)Math.Floor(bounds.X / checkerSize) * (int)checkerSize;
        var startY = (int)Math.Floor(bounds.Y / checkerSize) * (int)checkerSize;

        for (float y = startY; y < bounds.Bottom; y += checkerSize)
        {
            for (float x = startX; x < bounds.Right; x += checkerSize)
            {
                var isEven = ((int)((x - startX) / checkerSize) + (int)((y - startY) / checkerSize)) % 2 == 0;
                var brush = isEven ? brush1 : brush2;
                g.FillRectangle(brush, x, y, checkerSize, checkerSize);
            }
        }

        g.Clip = oldClip;
    }

    private void DrawSelectionAndControlPoints(Graphics g)
    {
        if (_image == null) return;

        foreach (var layer in _image.Layers.OfType<ShapeLayer>())
        {
            if (!layer.IsVisible) continue;

            foreach (var shape in layer.Shapes)
            {
                if (!shape.IsVisible || shape.IsLocked) continue;

                var isSelected = _selectedShapes.Contains(shape);
                var controlPoints = shape.ControlPoints;

                if (isSelected)
                {
                    // Draw selection rectangle
                    using var selectionPen = new Pen(SelectedShapeColor, 1.5f / _zoom)
                    {
                        DashStyle = DashStyle.Dash
                    };
                    var bounds = shape.Bounds;
                    bounds.Inflate(3 / _zoom, 3 / _zoom);
                    g.DrawRectangle(selectionPen, bounds.X, bounds.Y, bounds.Width, bounds.Height);

                    // Draw move handle at top-right of bounds
                    DrawMoveHandle(g, bounds);
                }

                // Draw control points for selected shapes
                if (isSelected)
                {
                    for (int i = 0; i < controlPoints.Count; i++)
                    {
                        var pt = controlPoints[i];
                        var isPrimarySelected = shape == _selectedShapes.FirstOrDefault();
                        var isHovered = isPrimarySelected && i == _selectedControlPointIndex;
                        var color = isHovered ? ControlPointHoverColor : ControlPointColor;
                        var radius = ControlPointRadius / _zoom;

                        using var brush = new SolidBrush(color);
                        using var pen = new Pen(Color.White, 1f / _zoom);

                        g.FillEllipse(brush, pt.X - radius, pt.Y - radius, radius * 2, radius * 2);
                        g.DrawEllipse(pen, pt.X - radius, pt.Y - radius, radius * 2, radius * 2);
                    }
                }
            }
        }
    }

    private void DrawMoveHandle(Graphics g, RectangleF shapeBounds)
    {
        var handleSize = MoveHandleSize / _zoom;
        var offset = 20 / _zoom;
        var handleX = shapeBounds.Right + offset - handleSize / 2;
        var handleY = shapeBounds.Top - offset - handleSize / 2;
        var handleRect = new RectangleF(handleX, handleY, handleSize, handleSize);

        var color = _isHoveringMoveHandle ? MoveHandleHoverColor : MoveHandleColor;

        using var brush = new SolidBrush(color);
        using var pen = new Pen(Color.White, 1f / _zoom);

        // Draw rounded rectangle
        g.FillEllipse(brush, handleRect);
        g.DrawEllipse(pen, handleRect);

        // Draw move arrows icon
        using var iconPen = new Pen(Color.White, 1.5f / _zoom);
        var centerX = handleRect.X + handleRect.Width / 2;
        var centerY = handleRect.Y + handleRect.Height / 2;
        var arrowSize = handleSize / 4;

        // Horizontal arrows
        g.DrawLine(iconPen, centerX - arrowSize, centerY, centerX + arrowSize, centerY);
        // Vertical arrows
        g.DrawLine(iconPen, centerX, centerY - arrowSize, centerX, centerY + arrowSize);
        // Arrow heads
        var headSize = arrowSize / 2;
        // Right
        g.DrawLine(iconPen, centerX + arrowSize, centerY, centerX + arrowSize - headSize, centerY - headSize);
        g.DrawLine(iconPen, centerX + arrowSize, centerY, centerX + arrowSize - headSize, centerY + headSize);
        // Left
        g.DrawLine(iconPen, centerX - arrowSize, centerY, centerX - arrowSize + headSize, centerY - headSize);
        g.DrawLine(iconPen, centerX - arrowSize, centerY, centerX - arrowSize + headSize, centerY + headSize);
        // Up
        g.DrawLine(iconPen, centerX, centerY - arrowSize, centerX - headSize, centerY - arrowSize + headSize);
        g.DrawLine(iconPen, centerX, centerY - arrowSize, centerX + headSize, centerY - arrowSize + headSize);
        // Down
        g.DrawLine(iconPen, centerX, centerY + arrowSize, centerX - headSize, centerY + arrowSize - headSize);
        g.DrawLine(iconPen, centerX, centerY + arrowSize, centerX + headSize, centerY + arrowSize - headSize);
    }

    private RectangleF GetMoveHandleRect(RectangleF shapeBounds)
    {
        var handleSize = MoveHandleSize / _zoom;
        var offset = 20 / _zoom;
        var handleX = shapeBounds.Right + offset - handleSize / 2;
        var handleY = shapeBounds.Top - offset - handleSize / 2;
        return new RectangleF(handleX, handleY, handleSize, handleSize);
    }

    private bool HitTestMoveHandle(PointF imagePoint)
    {
        foreach (var shape in _selectedShapes)
        {
            var bounds = shape.Bounds;
            bounds.Inflate(3 / _zoom, 3 / _zoom);
            var handleRect = GetMoveHandleRect(bounds);
            if (handleRect.Contains(imagePoint))
            {
                return true;
            }
        }
        return false;
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);

        if (e.Button == MouseButtons.Middle)
        {
            _isPanning = true;
            _lastMousePos = e.Location;
            Cursor = Cursors.Hand;
            return;
        }

        if (e.Button == MouseButtons.Left && _image != null)
        {
            var imagePoint = ScreenToImage(e.Location);
            var isCtrlPressed = ModifierKeys.HasFlag(Keys.Control);

            // Check if clicking on move handle
            if (_selectedShapes.Count > 0 && HitTestMoveHandle(imagePoint))
            {
                _isDraggingShapes = true;
                _dragStartImagePoint = imagePoint;
                _lastMousePos = e.Location;
                Cursor = Cursors.SizeAll;
                return;
            }

            // Check if clicking on a control point of the primary selected shape
            var primaryShape = _selectedShapes.FirstOrDefault();
            if (primaryShape != null && !primaryShape.IsLocked)
            {
                var hitIndex = primaryShape.HitTestControlPoint(imagePoint, 8 / _zoom);
                if (hitIndex >= 0)
                {
                    _selectedControlPointIndex = hitIndex;
                    _isDraggingControlPoint = true;
                    _lastMousePos = e.Location;
                    return;
                }
            }

            // Check if clicking on any shape
            foreach (var layer in _image.Layers.OfType<ShapeLayer>())
            {
                if (!layer.IsVisible || layer.IsLocked) continue;

                for (int i = layer.Shapes.Count - 1; i >= 0; i--)
                {
                    var shape = layer.Shapes[i];
                    if (!shape.IsVisible || shape.IsLocked) continue;

                    var hitIndex = shape.HitTestControlPoint(imagePoint, 8 / _zoom);
                    if (hitIndex >= 0 || shape.Bounds.Contains(imagePoint))
                    {
                        if (isCtrlPressed)
                        {
                            // Toggle selection with Ctrl
                            if (_selectedShapes.Contains(shape))
                            {
                                RemoveFromSelection(shape);
                            }
                            else
                            {
                                AddToSelection(shape);
                            }
                        }
                        else
                        {
                            // Replace selection without Ctrl
                            if (!_selectedShapes.Contains(shape))
                            {
                                _selectedShapes.Clear();
                                _selectedShapes.Add(shape);
                                SelectionChanged?.Invoke(this, EventArgs.Empty);
                            }
                        }

                        if (hitIndex >= 0)
                        {
                            _selectedControlPointIndex = hitIndex;
                            _isDraggingControlPoint = true;
                        }
                        else
                        {
                            _selectedControlPointIndex = -1;
                        }

                        _lastMousePos = e.Location;
                        Invalidate();
                        return;
                    }
                }
            }

            // Clicked on nothing
            if (!isCtrlPressed)
            {
                ClearSelection();
            }
            _selectedControlPointIndex = -1;
        }
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);

        if (_isPanning)
        {
            var dx = e.X - _lastMousePos.X;
            var dy = e.Y - _lastMousePos.Y;
            _panOffset = new PointF(_panOffset.X + dx, _panOffset.Y + dy);
            _lastMousePos = e.Location;
            Refresh();
            return;
        }

        if (_isDraggingShapes && _selectedShapes.Count > 0)
        {
            var imagePoint = ScreenToImage(e.Location);
            var dx = imagePoint.X - _dragStartImagePoint.X;
            var dy = imagePoint.Y - _dragStartImagePoint.Y;

            foreach (var shape in _selectedShapes)
            {
                MoveShape(shape, dx, dy);
            }

            _dragStartImagePoint = imagePoint;
            _lastMousePos = e.Location;
            Refresh();
            ImageModified?.Invoke(this, EventArgs.Empty);
            return;
        }

        if (_isDraggingControlPoint && _selectedShapes.Count > 0 && _selectedControlPointIndex >= 0)
        {
            var primaryShape = _selectedShapes.FirstOrDefault();
            if (primaryShape != null)
            {
                var imagePoint = ScreenToImage(e.Location);
                primaryShape.SetControlPoint(_selectedControlPointIndex, imagePoint);
                _lastMousePos = e.Location;
                Refresh();
                ImageModified?.Invoke(this, EventArgs.Empty);
            }
            return;
        }

        // Update cursor and hover state
        if (_image != null)
        {
            var imagePoint = ScreenToImage(e.Location);

            // Check move handle hover
            var wasHoveringMoveHandle = _isHoveringMoveHandle;
            _isHoveringMoveHandle = _selectedShapes.Count > 0 && HitTestMoveHandle(imagePoint);

            if (_isHoveringMoveHandle)
            {
                Cursor = Cursors.SizeAll;
                if (!wasHoveringMoveHandle) Invalidate();
                return;
            }
            else if (wasHoveringMoveHandle)
            {
                Invalidate();
            }

            // Check control point hover
            var primaryShape = _selectedShapes.FirstOrDefault();
            if (primaryShape != null)
            {
                var hitIndex = primaryShape.HitTestControlPoint(imagePoint, 8 / _zoom);

                if (hitIndex >= 0 && hitIndex != _selectedControlPointIndex)
                {
                    _selectedControlPointIndex = hitIndex;
                    Invalidate();
                }
                else if (hitIndex < 0 && _selectedControlPointIndex >= 0)
                {
                    _selectedControlPointIndex = -1;
                    Invalidate();
                }

                Cursor = hitIndex >= 0 ? Cursors.Cross : Cursors.Default;
            }
            else
            {
                Cursor = Cursors.Default;
            }
        }
    }

    private void MoveShape(Shape shape, float dx, float dy)
    {
        var controlPoints = shape.ControlPoints;
        for (int i = 0; i < controlPoints.Count; i++)
        {
            var pt = controlPoints[i];
            shape.SetControlPoint(i, new PointF(pt.X + dx, pt.Y + dy));
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);

        if (_isPanning)
        {
            _isPanning = false;
            Cursor = Cursors.Default;
        }

        _isDraggingControlPoint = false;
        _isDraggingShapes = false;

        // Update cursor based on current position
        if (_image != null)
        {
            var imagePoint = ScreenToImage(e.Location);
            _isHoveringMoveHandle = _selectedShapes.Count > 0 && HitTestMoveHandle(imagePoint);
            Cursor = _isHoveringMoveHandle ? Cursors.SizeAll : Cursors.Default;
        }
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);

        if (ModifierKeys.HasFlag(Keys.Control))
        {
            var mouseImagePosBefore = ScreenToImage(e.Location);

            var zoomFactor = e.Delta > 0 ? 1.1f : 0.9f;
            Zoom *= zoomFactor;

            var mouseImagePosAfter = ScreenToImage(e.Location);
            var dx = (mouseImagePosAfter.X - mouseImagePosBefore.X) * _zoom;
            var dy = (mouseImagePosAfter.Y - mouseImagePosBefore.Y) * _zoom;
            _panOffset = new PointF(_panOffset.X + dx, _panOffset.Y + dy);

            Refresh();
        }
    }

    private PointF ScreenToImage(Point screenPoint)
    {
        return new PointF(
            (screenPoint.X - _panOffset.X) / _zoom,
            (screenPoint.Y - _panOffset.Y) / _zoom);
    }

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);
        Invalidate();
    }
}
