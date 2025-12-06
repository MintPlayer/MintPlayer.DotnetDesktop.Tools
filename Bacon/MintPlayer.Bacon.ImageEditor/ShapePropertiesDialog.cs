using System.Drawing;
using System.Drawing.Drawing2D;
using MintPlayer.Bacon.Images.Data;
using MintPlayer.Bacon.Images.Shapes;

namespace MintPlayer.Bacon.ImageEditor;

/// <summary>
/// Dialog for editing shape Pen and Brush properties.
/// </summary>
public class ShapePropertiesDialog : Form
{
    private readonly Shape _shape;

    // Pen controls
    private readonly CheckBox _hasPenCheckbox;
    private readonly Panel _penPanel;
    private readonly Button _penColorButton;
    private readonly NumericUpDown _penWidthInput;
    private readonly ComboBox _dashStyleCombo;
    private readonly ComboBox _startCapCombo;
    private readonly ComboBox _endCapCombo;
    private readonly Panel _penColorPreview;

    // Fill controls
    private readonly CheckBox _hasFillCheckbox;
    private readonly Panel _fillPanel;
    private readonly ComboBox _fillTypeCombo;
    private readonly Button _fillColorButton;
    private readonly Panel _fillColorPreview;
    private readonly Button _gradientColor1Button;
    private readonly Button _gradientColor2Button;
    private readonly Panel _gradientColor1Preview;
    private readonly Panel _gradientColor2Preview;
    private readonly Panel _solidFillPanel;
    private readonly Panel _gradientFillPanel;

    // Shape preview
    private readonly Panel _previewPanel;

    // Buttons
    private readonly Button _okButton;
    private readonly Button _cancelButton;

    // Current values
    private Color _penColor = Color.Black;
    private Color _fillColor = Color.White;
    private Color _gradientColor1 = Color.White;
    private Color _gradientColor2 = Color.Black;

    public ShapePropertiesDialog(Shape shape)
    {
        _shape = shape;

        Text = $"Properties - {shape.Name ?? shape.GetType().Name}";
        Size = new Size(420, 520);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;

        var padding = 12;
        var labelWidth = 80;
        var inputWidth = 120;
        var currentY = padding;

        // ===== PEN SECTION =====
        var penGroupBox = new GroupBox
        {
            Text = "Stroke (Pen)",
            Location = new Point(padding, currentY),
            Size = new Size(Size.Width - padding * 2 - 16, 180)
        };

        _hasPenCheckbox = new CheckBox
        {
            Text = "Has stroke",
            Location = new Point(10, 20),
            Size = new Size(100, 20),
            Checked = GetShapePen() != null
        };
        _hasPenCheckbox.CheckedChanged += OnHasPenChanged;

        _penPanel = new Panel
        {
            Location = new Point(10, 45),
            Size = new Size(penGroupBox.Width - 20, 125)
        };

        // Pen color
        var penColorLabel = new Label
        {
            Text = "Color:",
            Location = new Point(0, 5),
            Size = new Size(labelWidth, 23),
            TextAlign = ContentAlignment.MiddleLeft
        };

        _penColorPreview = new Panel
        {
            Location = new Point(labelWidth, 5),
            Size = new Size(25, 23),
            BorderStyle = BorderStyle.FixedSingle
        };

        _penColorButton = new Button
        {
            Text = "Choose...",
            Location = new Point(labelWidth + 30, 5),
            Size = new Size(inputWidth - 30, 23)
        };
        _penColorButton.Click += OnPenColorClick;

        // Pen width
        var penWidthLabel = new Label
        {
            Text = "Width:",
            Location = new Point(0, 35),
            Size = new Size(labelWidth, 23),
            TextAlign = ContentAlignment.MiddleLeft
        };

        _penWidthInput = new NumericUpDown
        {
            Location = new Point(labelWidth, 35),
            Size = new Size(inputWidth, 23),
            Minimum = 0.1m,
            Maximum = 100,
            DecimalPlaces = 1,
            Value = 1
        };
        _penWidthInput.ValueChanged += OnPenPropertyChanged;

        // Dash style
        var dashStyleLabel = new Label
        {
            Text = "Dash Style:",
            Location = new Point(0, 65),
            Size = new Size(labelWidth, 23),
            TextAlign = ContentAlignment.MiddleLeft
        };

        _dashStyleCombo = new ComboBox
        {
            Location = new Point(labelWidth, 65),
            Size = new Size(inputWidth, 23),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _dashStyleCombo.Items.AddRange(Enum.GetNames(typeof(DashStyle)).Where(n => n != "Custom").ToArray());
        _dashStyleCombo.SelectedIndexChanged += OnPenPropertyChanged;

        // Start cap
        var startCapLabel = new Label
        {
            Text = "Start Cap:",
            Location = new Point(200, 35),
            Size = new Size(labelWidth, 23),
            TextAlign = ContentAlignment.MiddleLeft
        };

        _startCapCombo = new ComboBox
        {
            Location = new Point(280, 35),
            Size = new Size(90, 23),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _startCapCombo.Items.AddRange(new[] { "Flat", "Round", "Square", "Triangle" });
        _startCapCombo.SelectedIndexChanged += OnPenPropertyChanged;

        // End cap
        var endCapLabel = new Label
        {
            Text = "End Cap:",
            Location = new Point(200, 65),
            Size = new Size(labelWidth, 23),
            TextAlign = ContentAlignment.MiddleLeft
        };

        _endCapCombo = new ComboBox
        {
            Location = new Point(280, 65),
            Size = new Size(90, 23),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _endCapCombo.Items.AddRange(new[] { "Flat", "Round", "Square", "Triangle" });
        _endCapCombo.SelectedIndexChanged += OnPenPropertyChanged;

        _penPanel.Controls.AddRange(new Control[]
        {
            penColorLabel, _penColorPreview, _penColorButton,
            penWidthLabel, _penWidthInput,
            dashStyleLabel, _dashStyleCombo,
            startCapLabel, _startCapCombo,
            endCapLabel, _endCapCombo
        });

        penGroupBox.Controls.Add(_hasPenCheckbox);
        penGroupBox.Controls.Add(_penPanel);
        currentY += penGroupBox.Height + padding;

        // ===== FILL SECTION =====
        var fillGroupBox = new GroupBox
        {
            Text = "Fill (Brush)",
            Location = new Point(padding, currentY),
            Size = new Size(Size.Width - padding * 2 - 16, 170)
        };

        _hasFillCheckbox = new CheckBox
        {
            Text = "Has fill",
            Location = new Point(10, 20),
            Size = new Size(100, 20),
            Checked = GetShapeFill() != null
        };
        _hasFillCheckbox.CheckedChanged += OnHasFillChanged;

        _fillPanel = new Panel
        {
            Location = new Point(10, 45),
            Size = new Size(fillGroupBox.Width - 20, 115)
        };

        // Fill type
        var fillTypeLabel = new Label
        {
            Text = "Type:",
            Location = new Point(0, 5),
            Size = new Size(labelWidth, 23),
            TextAlign = ContentAlignment.MiddleLeft
        };

        _fillTypeCombo = new ComboBox
        {
            Location = new Point(labelWidth, 5),
            Size = new Size(inputWidth, 23),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _fillTypeCombo.Items.AddRange(new[] { "Solid", "Linear Gradient" });
        _fillTypeCombo.SelectedIndexChanged += OnFillTypeChanged;

        // Solid fill panel
        _solidFillPanel = new Panel
        {
            Location = new Point(0, 35),
            Size = new Size(350, 30)
        };

        var solidColorLabel = new Label
        {
            Text = "Color:",
            Location = new Point(0, 5),
            Size = new Size(labelWidth, 23),
            TextAlign = ContentAlignment.MiddleLeft
        };

        _fillColorPreview = new Panel
        {
            Location = new Point(labelWidth, 5),
            Size = new Size(25, 23),
            BorderStyle = BorderStyle.FixedSingle
        };

        _fillColorButton = new Button
        {
            Text = "Choose...",
            Location = new Point(labelWidth + 30, 5),
            Size = new Size(inputWidth - 30, 23)
        };
        _fillColorButton.Click += OnFillColorClick;

        _solidFillPanel.Controls.AddRange(new Control[]
        {
            solidColorLabel, _fillColorPreview, _fillColorButton
        });

        // Gradient fill panel
        _gradientFillPanel = new Panel
        {
            Location = new Point(0, 35),
            Size = new Size(350, 70),
            Visible = false
        };

        var gradient1Label = new Label
        {
            Text = "Color 1:",
            Location = new Point(0, 5),
            Size = new Size(labelWidth, 23),
            TextAlign = ContentAlignment.MiddleLeft
        };

        _gradientColor1Preview = new Panel
        {
            Location = new Point(labelWidth, 5),
            Size = new Size(25, 23),
            BorderStyle = BorderStyle.FixedSingle
        };

        _gradientColor1Button = new Button
        {
            Text = "Choose...",
            Location = new Point(labelWidth + 30, 5),
            Size = new Size(inputWidth - 30, 23)
        };
        _gradientColor1Button.Click += OnGradientColor1Click;

        var gradient2Label = new Label
        {
            Text = "Color 2:",
            Location = new Point(0, 35),
            Size = new Size(labelWidth, 23),
            TextAlign = ContentAlignment.MiddleLeft
        };

        _gradientColor2Preview = new Panel
        {
            Location = new Point(labelWidth, 35),
            Size = new Size(25, 23),
            BorderStyle = BorderStyle.FixedSingle
        };

        _gradientColor2Button = new Button
        {
            Text = "Choose...",
            Location = new Point(labelWidth + 30, 35),
            Size = new Size(inputWidth - 30, 23)
        };
        _gradientColor2Button.Click += OnGradientColor2Click;

        _gradientFillPanel.Controls.AddRange(new Control[]
        {
            gradient1Label, _gradientColor1Preview, _gradientColor1Button,
            gradient2Label, _gradientColor2Preview, _gradientColor2Button
        });

        _fillPanel.Controls.AddRange(new Control[]
        {
            fillTypeLabel, _fillTypeCombo,
            _solidFillPanel, _gradientFillPanel
        });

        fillGroupBox.Controls.Add(_hasFillCheckbox);
        fillGroupBox.Controls.Add(_fillPanel);
        currentY += fillGroupBox.Height + padding;

        // ===== PREVIEW SECTION =====
        var previewGroupBox = new GroupBox
        {
            Text = "Preview",
            Location = new Point(padding, currentY),
            Size = new Size(Size.Width - padding * 2 - 16, 80)
        };

        _previewPanel = new Panel
        {
            Location = new Point(10, 20),
            Size = new Size(previewGroupBox.Width - 20, 50),
            BorderStyle = BorderStyle.FixedSingle
        };
        _previewPanel.Paint += OnPreviewPaint;

        previewGroupBox.Controls.Add(_previewPanel);
        currentY += previewGroupBox.Height + padding;

        // ===== BUTTONS =====
        _okButton = new Button
        {
            Text = "OK",
            Location = new Point(Size.Width - 180, Size.Height - 75),
            Size = new Size(75, 25),
            DialogResult = DialogResult.OK
        };
        _okButton.Click += OnOkClick;

        _cancelButton = new Button
        {
            Text = "Cancel",
            Location = new Point(Size.Width - 95, Size.Height - 75),
            Size = new Size(75, 25),
            DialogResult = DialogResult.Cancel
        };

        AcceptButton = _okButton;
        CancelButton = _cancelButton;

        Controls.AddRange(new Control[]
        {
            penGroupBox,
            fillGroupBox,
            previewGroupBox,
            _okButton,
            _cancelButton
        });

        // Initialize values from shape
        InitializeFromShape();

        // Set initial enabled state for fill section based on shape type
        var isPlane = _shape is Plane;
        fillGroupBox.Enabled = isPlane;
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
            _endCapCombo.SelectedItem = GetCapName(pen.EndCap);
        }
        else
        {
            _hasPenCheckbox.Checked = false;
            _dashStyleCombo.SelectedIndex = 0;
            _startCapCombo.SelectedIndex = 0;
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

    private string GetCapName(LineCap cap)
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

    private LineCap GetCapFromName(string name)
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
                    var bounds = _shape.Bounds;
                    plane.Fill = new SerializableLinearGradientBrush(
                        new PointF(bounds.Left, bounds.Top),
                        new PointF(bounds.Right, bounds.Bottom),
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
