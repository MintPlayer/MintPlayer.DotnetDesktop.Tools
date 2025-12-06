using System.Drawing;
using System.Drawing.Drawing2D;
using MintPlayer.Bacon.Images.Data;
using MintPlayer.Bacon.Images.Shapes;

namespace MintPlayer.Bacon.ImageEditor;

/// <summary>
/// Dialog for editing shape Pen and Brush properties.
/// </summary>
public partial class ShapePropertiesDialog : Form
{
    private readonly Shape _shape;

    // Current values
    private Color _penColor = Color.Black;
    private Color _fillColor = Color.White;
    private Color _gradientColor1 = Color.White;
    private Color _gradientColor2 = Color.Black;

    public ShapePropertiesDialog(Shape shape)
    {
        _shape = shape;

        InitializeComponent();

        Text = $"Properties - {shape.Name ?? shape.GetType().Name}";

        // Setup combo box items
        _dashStyleCombo.Items.AddRange(Enum.GetNames(typeof(DashStyle)).Where(n => n != "Custom").ToArray());
        _startCapCombo.Items.AddRange(new[] { "Flat", "Round", "Square", "Triangle" });
        _middleCapCombo.Items.AddRange(new[] { "Flat", "Round", "Triangle" });
        _endCapCombo.Items.AddRange(new[] { "Flat", "Round", "Square", "Triangle" });
        _fillTypeCombo.Items.AddRange(new[] { "Solid", "Linear Gradient" });

        // Wire up events
        _hasPenCheckbox.CheckedChanged += OnHasPenChanged;
        _hasFillCheckbox.CheckedChanged += OnHasFillChanged;
        _penColorButton.Click += OnPenColorClick;
        _penWidthInput.ValueChanged += OnPenPropertyChanged;
        _dashStyleCombo.SelectedIndexChanged += OnPenPropertyChanged;
        _startCapCombo.SelectedIndexChanged += OnPenPropertyChanged;
        _middleCapCombo.SelectedIndexChanged += OnPenPropertyChanged;
        _endCapCombo.SelectedIndexChanged += OnPenPropertyChanged;
        _fillTypeCombo.SelectedIndexChanged += OnFillTypeChanged;
        _fillColorButton.Click += OnFillColorClick;
        _gradientColor1Button.Click += OnGradientColor1Click;
        _gradientColor2Button.Click += OnGradientColor2Click;
        _previewPanel.Paint += OnPreviewPaint;
        _okButton.Click += OnOkClick;

        // Initialize values from shape
        InitializeFromShape();

        // Set initial enabled state for fill section based on shape type
        var isPlane = _shape is Plane;
        _fillGroupBox.Enabled = isPlane;
        if (!isPlane)
        {
            _hasFillCheckbox.Checked = false;
        }
    }

    private SerializablePen? GetShapePen()
    {
        return _shape switch
        {
            Line line => line.Pen,
            Plane plane => plane.Pen,
            _ => null
        };
    }

    private SerializableBrush? GetShapeFill()
    {
        return _shape is Plane plane ? plane.Fill : null;
    }

    private void InitializeFromShape()
    {
        // Initialize pen values
        var pen = GetShapePen();
        if (pen != null)
        {
            _hasPenCheckbox.Checked = true;
            _penColor = pen.Color;
            _penWidthInput.Value = (decimal)pen.Width;
            _dashStyleCombo.SelectedItem = pen.DashStyle.ToString();
            _startCapCombo.SelectedItem = GetCapName(pen.StartCap);
            _middleCapCombo.SelectedItem = GetDashCapName(pen.DashCap);
            _endCapCombo.SelectedItem = GetCapName(pen.EndCap);
        }
        else
        {
            _hasPenCheckbox.Checked = false;
            _dashStyleCombo.SelectedIndex = 0;
            _startCapCombo.SelectedIndex = 0;
            _middleCapCombo.SelectedIndex = 0;
            _endCapCombo.SelectedIndex = 0;
        }

        // Initialize fill values
        var fill = GetShapeFill();
        if (fill is SerializableSolidBrush solidBrush)
        {
            _hasFillCheckbox.Checked = true;
            _fillTypeCombo.SelectedIndex = 0;
            _fillColor = solidBrush.Color;
        }
        else if (fill is SerializableLinearGradientBrush gradientBrush)
        {
            _hasFillCheckbox.Checked = true;
            _fillTypeCombo.SelectedIndex = 1;
            _gradientColor1 = gradientBrush.Color1;
            _gradientColor2 = gradientBrush.Color2;
        }
        else
        {
            _hasFillCheckbox.Checked = false;
            _fillTypeCombo.SelectedIndex = 0;
        }

        UpdateColorPreviews();
        UpdatePanelEnabled();
    }

    private static string GetCapName(LineCap cap)
    {
        return cap switch
        {
            LineCap.Flat => "Flat",
            LineCap.Round => "Round",
            LineCap.Square => "Square",
            LineCap.Triangle => "Triangle",
            _ => "Flat"
        };
    }

    private static LineCap GetCapFromName(string name)
    {
        return name switch
        {
            "Flat" => LineCap.Flat,
            "Round" => LineCap.Round,
            "Square" => LineCap.Square,
            "Triangle" => LineCap.Triangle,
            _ => LineCap.Flat
        };
    }

    private static string GetDashCapName(DashCap cap)
    {
        return cap switch
        {
            DashCap.Flat => "Flat",
            DashCap.Round => "Round",
            DashCap.Triangle => "Triangle",
            _ => "Flat"
        };
    }

    private static DashCap GetDashCapFromName(string name)
    {
        return name switch
        {
            "Flat" => DashCap.Flat,
            "Round" => DashCap.Round,
            "Triangle" => DashCap.Triangle,
            _ => DashCap.Flat
        };
    }

    private void UpdateColorPreviews()
    {
        _penColorPreview.BackColor = _penColor;
        _fillColorPreview.BackColor = _fillColor;
        _gradientColor1Preview.BackColor = _gradientColor1;
        _gradientColor2Preview.BackColor = _gradientColor2;
        _previewPanel.Invalidate();
    }

    private void UpdatePanelEnabled()
    {
        _penPanel.Enabled = _hasPenCheckbox.Checked;
        _fillPanel.Enabled = _hasFillCheckbox.Checked;
        _previewPanel.Invalidate();
    }

    private void OnHasPenChanged(object? sender, EventArgs e)
    {
        UpdatePanelEnabled();
    }

    private void OnHasFillChanged(object? sender, EventArgs e)
    {
        UpdatePanelEnabled();
    }

    private void OnPenPropertyChanged(object? sender, EventArgs e)
    {
        _previewPanel.Invalidate();
    }

    private void OnFillTypeChanged(object? sender, EventArgs e)
    {
        var isGradient = _fillTypeCombo.SelectedIndex == 1;
        _solidFillPanel.Visible = !isGradient;
        _gradientFillPanel.Visible = isGradient;
        _previewPanel.Invalidate();
    }

    private void OnPenColorClick(object? sender, EventArgs e)
    {
        using var dialog = new ColorDialog { Color = _penColor, FullOpen = true };
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _penColor = dialog.Color;
            UpdateColorPreviews();
        }
    }

    private void OnFillColorClick(object? sender, EventArgs e)
    {
        using var dialog = new ColorDialog { Color = _fillColor, FullOpen = true };
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _fillColor = dialog.Color;
            UpdateColorPreviews();
        }
    }

    private void OnGradientColor1Click(object? sender, EventArgs e)
    {
        using var dialog = new ColorDialog { Color = _gradientColor1, FullOpen = true };
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _gradientColor1 = dialog.Color;
            UpdateColorPreviews();
        }
    }

    private void OnGradientColor2Click(object? sender, EventArgs e)
    {
        using var dialog = new ColorDialog { Color = _gradientColor2, FullOpen = true };
        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _gradientColor2 = dialog.Color;
            UpdateColorPreviews();
        }
    }

    private void OnPreviewPaint(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(SystemColors.Control);

        var bounds = _previewPanel.ClientRectangle;
        var previewRect = new RectangleF(
            bounds.X + 20,
            bounds.Y + 10,
            bounds.Width - 40,
            bounds.Height - 20);

        // Draw fill
        if (_hasFillCheckbox.Checked && _shape is Plane)
        {
            Brush brush;
            if (_fillTypeCombo.SelectedIndex == 1)
            {
                brush = new LinearGradientBrush(
                    new PointF(previewRect.Left, previewRect.Top),
                    new PointF(previewRect.Right, previewRect.Bottom),
                    _gradientColor1,
                    _gradientColor2);
            }
            else
            {
                brush = new SolidBrush(_fillColor);
            }

            g.FillEllipse(brush, previewRect);
            brush.Dispose();
        }

        // Draw stroke
        if (_hasPenCheckbox.Checked)
        {
            var pen = new Pen(_penColor, (float)_penWidthInput.Value);
            if (_dashStyleCombo.SelectedItem is string dashName &&
                Enum.TryParse<DashStyle>(dashName, out var dashStyle))
            {
                pen.DashStyle = dashStyle;
            }
            if (_startCapCombo.SelectedItem is string startCapName)
            {
                pen.StartCap = GetCapFromName(startCapName);
            }
            if (_middleCapCombo.SelectedItem is string middleCapName)
            {
                pen.DashCap = GetDashCapFromName(middleCapName);
            }
            if (_endCapCombo.SelectedItem is string endCapName)
            {
                pen.EndCap = GetCapFromName(endCapName);
            }

            if (_shape is Plane)
            {
                g.DrawEllipse(pen, previewRect);
            }
            else
            {
                g.DrawLine(pen, previewRect.Left, previewRect.Top + previewRect.Height / 2,
                    previewRect.Right, previewRect.Top + previewRect.Height / 2);
            }

            pen.Dispose();
        }
    }

    private void OnOkClick(object? sender, EventArgs e)
    {
        ApplyToShape();
    }

    private void ApplyToShape()
    {
        // Apply pen
        if (_shape is Line line)
        {
            line.Pen = new SerializablePen
            {
                Color = _penColor,
                Width = (float)_penWidthInput.Value,
                DashStyle = Enum.TryParse<DashStyle>(_dashStyleCombo.SelectedItem?.ToString(), out var ds) ? ds : DashStyle.Solid,
                DashCap = GetDashCapFromName(_middleCapCombo.SelectedItem?.ToString() ?? "Flat"),
                StartCap = GetCapFromName(_startCapCombo.SelectedItem?.ToString() ?? "Flat"),
                EndCap = GetCapFromName(_endCapCombo.SelectedItem?.ToString() ?? "Flat")
            };
        }
        else if (_shape is Plane plane)
        {
            // Apply pen
            if (_hasPenCheckbox.Checked)
            {
                plane.Pen = new SerializablePen
                {
                    Color = _penColor,
                    Width = (float)_penWidthInput.Value,
                    DashStyle = Enum.TryParse<DashStyle>(_dashStyleCombo.SelectedItem?.ToString(), out var ds) ? ds : DashStyle.Solid,
                    DashCap = GetDashCapFromName(_middleCapCombo.SelectedItem?.ToString() ?? "Flat"),
                    StartCap = GetCapFromName(_startCapCombo.SelectedItem?.ToString() ?? "Flat"),
                    EndCap = GetCapFromName(_endCapCombo.SelectedItem?.ToString() ?? "Flat")
                };
            }
            else
            {
                plane.Pen = null;
            }

            // Apply fill
            if (_hasFillCheckbox.Checked)
            {
                if (_fillTypeCombo.SelectedIndex == 1)
                {
                    var shapeBounds = _shape.Bounds;
                    plane.Fill = new SerializableLinearGradientBrush(
                        new PointF(shapeBounds.Left, shapeBounds.Top),
                        new PointF(shapeBounds.Right, shapeBounds.Bottom),
                        _gradientColor1,
                        _gradientColor2);
                }
                else
                {
                    plane.Fill = new SerializableSolidBrush(_fillColor);
                }
            }
            else
            {
                plane.Fill = null;
            }
        }
    }
}
