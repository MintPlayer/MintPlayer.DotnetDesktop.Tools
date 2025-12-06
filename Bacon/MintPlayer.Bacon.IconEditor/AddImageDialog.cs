using System.Drawing;

namespace MintPlayer.Bacon.IconEditor;

/// <summary>
/// Dialog for adding a new image to an icon.
/// </summary>
public class AddImageDialog : Form
{
    private readonly NumericUpDown _widthInput;
    private readonly NumericUpDown _heightInput;
    private readonly CheckBox _linkDimensionsCheckbox;
    private readonly ComboBox _presetsCombo;
    private readonly Button _okButton;
    private readonly Button _cancelButton;

    /// <summary>
    /// The width of the image to create.
    /// </summary>
    public int ImageWidth => (int)_widthInput.Value;

    /// <summary>
    /// The height of the image to create.
    /// </summary>
    public int ImageHeight => (int)_heightInput.Value;

    public AddImageDialog()
    {
        Text = "Add Image";
        Size = new Size(300, 220);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        StartPosition = FormStartPosition.CenterParent;
        MaximizeBox = false;
        MinimizeBox = false;

        var padding = 12;
        var labelWidth = 60;
        var inputWidth = 100;

        // Preset combo
        var presetLabel = new Label
        {
            Text = "Preset:",
            Location = new Point(padding, padding),
            Size = new Size(labelWidth, 23),
            TextAlign = ContentAlignment.MiddleLeft
        };

        _presetsCombo = new ComboBox
        {
            Location = new Point(padding + labelWidth, padding),
            Size = new Size(inputWidth + 80, 23),
            DropDownStyle = ComboBoxStyle.DropDownList
        };
        _presetsCombo.Items.AddRange(new object[]
        {
            "Custom",
            "16 x 16",
            "24 x 24",
            "32 x 32",
            "48 x 48",
            "64 x 64",
            "128 x 128",
            "256 x 256"
        });
        _presetsCombo.SelectedIndex = 0;
        _presetsCombo.SelectedIndexChanged += OnPresetChanged;

        // Width input
        var widthLabel = new Label
        {
            Text = "Width:",
            Location = new Point(padding, padding + 35),
            Size = new Size(labelWidth, 23),
            TextAlign = ContentAlignment.MiddleLeft
        };

        _widthInput = new NumericUpDown
        {
            Location = new Point(padding + labelWidth, padding + 35),
            Size = new Size(inputWidth, 23),
            Minimum = 1,
            Maximum = 1024,
            Value = 32
        };
        _widthInput.ValueChanged += OnWidthChanged;

        // Height input
        var heightLabel = new Label
        {
            Text = "Height:",
            Location = new Point(padding, padding + 65),
            Size = new Size(labelWidth, 23),
            TextAlign = ContentAlignment.MiddleLeft
        };

        _heightInput = new NumericUpDown
        {
            Location = new Point(padding + labelWidth, padding + 65),
            Size = new Size(inputWidth, 23),
            Minimum = 1,
            Maximum = 1024,
            Value = 32
        };
        _heightInput.ValueChanged += OnHeightChanged;

        // Link dimensions checkbox
        _linkDimensionsCheckbox = new CheckBox
        {
            Text = "Link dimensions (square)",
            Location = new Point(padding, padding + 95),
            Size = new Size(200, 23),
            Checked = true
        };

        // Buttons
        _okButton = new Button
        {
            Text = "OK",
            Location = new Point(Size.Width - 180, Size.Height - 75),
            Size = new Size(75, 25),
            DialogResult = DialogResult.OK
        };

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
            presetLabel, _presetsCombo,
            widthLabel, _widthInput,
            heightLabel, _heightInput,
            _linkDimensionsCheckbox,
            _okButton, _cancelButton
        });
    }

    private void OnPresetChanged(object? sender, EventArgs e)
    {
        if (_presetsCombo.SelectedIndex <= 0) return;

        var presetSizes = new[] { 16, 24, 32, 48, 64, 128, 256 };
        var size = presetSizes[_presetsCombo.SelectedIndex - 1];

        _widthInput.ValueChanged -= OnWidthChanged;
        _heightInput.ValueChanged -= OnHeightChanged;

        _widthInput.Value = size;
        _heightInput.Value = size;

        _widthInput.ValueChanged += OnWidthChanged;
        _heightInput.ValueChanged += OnHeightChanged;
    }

    private void OnWidthChanged(object? sender, EventArgs e)
    {
        _presetsCombo.SelectedIndex = 0; // Custom

        if (_linkDimensionsCheckbox.Checked)
        {
            _heightInput.ValueChanged -= OnHeightChanged;
            _heightInput.Value = _widthInput.Value;
            _heightInput.ValueChanged += OnHeightChanged;
        }
    }

    private void OnHeightChanged(object? sender, EventArgs e)
    {
        _presetsCombo.SelectedIndex = 0; // Custom

        if (_linkDimensionsCheckbox.Checked)
        {
            _widthInput.ValueChanged -= OnWidthChanged;
            _widthInput.Value = _heightInput.Value;
            _widthInput.ValueChanged += OnWidthChanged;
        }
    }
}
