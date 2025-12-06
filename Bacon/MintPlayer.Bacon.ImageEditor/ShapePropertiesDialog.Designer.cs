namespace MintPlayer.Bacon.ImageEditor;

partial class ShapePropertiesDialog
{
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        var padding = 12;
        var labelWidth = 80;
        var inputWidth = 120;
        var currentY = padding;

        SuspendLayout();

        // ===== PEN GROUP BOX =====
        _penGroupBox = new GroupBox();
        _penGroupBox.Text = "Stroke (Pen)";
        _penGroupBox.Location = new Point(padding, currentY);
        _penGroupBox.Size = new Size(Size.Width - padding * 2 - 16, 180);

        _hasPenCheckbox = new CheckBox();
        _hasPenCheckbox.Text = "Has stroke";
        _hasPenCheckbox.Location = new Point(10, 20);
        _hasPenCheckbox.Size = new Size(100, 20);

        _penPanel = new Panel();
        _penPanel.Location = new Point(10, 45);
        _penPanel.Size = new Size(_penGroupBox.Width - 20, 125);

        // Pen color
        _penColorLabel = new Label();
        _penColorLabel.Text = "Color:";
        _penColorLabel.Location = new Point(0, 5);
        _penColorLabel.Size = new Size(labelWidth, 23);
        _penColorLabel.TextAlign = ContentAlignment.MiddleLeft;

        _penColorPreview = new Panel();
        _penColorPreview.Location = new Point(labelWidth, 5);
        _penColorPreview.Size = new Size(25, 23);
        _penColorPreview.BorderStyle = BorderStyle.FixedSingle;

        _penColorButton = new Button();
        _penColorButton.Text = "Choose...";
        _penColorButton.Location = new Point(labelWidth + 30, 5);
        _penColorButton.Size = new Size(inputWidth - 30, 23);

        // Pen width
        _penWidthLabel = new Label();
        _penWidthLabel.Text = "Width:";
        _penWidthLabel.Location = new Point(0, 35);
        _penWidthLabel.Size = new Size(labelWidth, 23);
        _penWidthLabel.TextAlign = ContentAlignment.MiddleLeft;

        _penWidthInput = new NumericUpDown();
        _penWidthInput.Location = new Point(labelWidth, 35);
        _penWidthInput.Size = new Size(inputWidth, 23);
        _penWidthInput.Minimum = 0.1m;
        _penWidthInput.Maximum = 100;
        _penWidthInput.DecimalPlaces = 1;
        _penWidthInput.Value = 1;

        // Dash style
        _dashStyleLabel = new Label();
        _dashStyleLabel.Text = "Dash Style:";
        _dashStyleLabel.Location = new Point(0, 65);
        _dashStyleLabel.Size = new Size(labelWidth, 23);
        _dashStyleLabel.TextAlign = ContentAlignment.MiddleLeft;

        _dashStyleCombo = new ComboBox();
        _dashStyleCombo.Location = new Point(labelWidth, 65);
        _dashStyleCombo.Size = new Size(inputWidth, 23);
        _dashStyleCombo.DropDownStyle = ComboBoxStyle.DropDownList;

        // Start cap
        _startCapLabel = new Label();
        _startCapLabel.Text = "Start Cap:";
        _startCapLabel.Location = new Point(200, 35);
        _startCapLabel.Size = new Size(labelWidth, 23);
        _startCapLabel.TextAlign = ContentAlignment.MiddleLeft;

        _startCapCombo = new ComboBox();
        _startCapCombo.Location = new Point(280, 35);
        _startCapCombo.Size = new Size(90, 23);
        _startCapCombo.DropDownStyle = ComboBoxStyle.DropDownList;

        // End cap
        _endCapLabel = new Label();
        _endCapLabel.Text = "End Cap:";
        _endCapLabel.Location = new Point(200, 65);
        _endCapLabel.Size = new Size(labelWidth, 23);
        _endCapLabel.TextAlign = ContentAlignment.MiddleLeft;

        _endCapCombo = new ComboBox();
        _endCapCombo.Location = new Point(280, 65);
        _endCapCombo.Size = new Size(90, 23);
        _endCapCombo.DropDownStyle = ComboBoxStyle.DropDownList;

        _penPanel.Controls.AddRange(new Control[]
        {
            _penColorLabel, _penColorPreview, _penColorButton,
            _penWidthLabel, _penWidthInput,
            _dashStyleLabel, _dashStyleCombo,
            _startCapLabel, _startCapCombo,
            _endCapLabel, _endCapCombo
        });

        _penGroupBox.Controls.Add(_hasPenCheckbox);
        _penGroupBox.Controls.Add(_penPanel);
        currentY += _penGroupBox.Height + padding;

        // ===== FILL GROUP BOX =====
        _fillGroupBox = new GroupBox();
        _fillGroupBox.Text = "Fill (Brush)";
        _fillGroupBox.Location = new Point(padding, currentY);
        _fillGroupBox.Size = new Size(Size.Width - padding * 2 - 16, 170);

        _hasFillCheckbox = new CheckBox();
        _hasFillCheckbox.Text = "Has fill";
        _hasFillCheckbox.Location = new Point(10, 20);
        _hasFillCheckbox.Size = new Size(100, 20);

        _fillPanel = new Panel();
        _fillPanel.Location = new Point(10, 45);
        _fillPanel.Size = new Size(_fillGroupBox.Width - 20, 115);

        // Fill type
        _fillTypeLabel = new Label();
        _fillTypeLabel.Text = "Type:";
        _fillTypeLabel.Location = new Point(0, 5);
        _fillTypeLabel.Size = new Size(labelWidth, 23);
        _fillTypeLabel.TextAlign = ContentAlignment.MiddleLeft;

        _fillTypeCombo = new ComboBox();
        _fillTypeCombo.Location = new Point(labelWidth, 5);
        _fillTypeCombo.Size = new Size(inputWidth, 23);
        _fillTypeCombo.DropDownStyle = ComboBoxStyle.DropDownList;

        // Solid fill panel
        _solidFillPanel = new Panel();
        _solidFillPanel.Location = new Point(0, 35);
        _solidFillPanel.Size = new Size(350, 30);

        _solidColorLabel = new Label();
        _solidColorLabel.Text = "Color:";
        _solidColorLabel.Location = new Point(0, 5);
        _solidColorLabel.Size = new Size(labelWidth, 23);
        _solidColorLabel.TextAlign = ContentAlignment.MiddleLeft;

        _fillColorPreview = new Panel();
        _fillColorPreview.Location = new Point(labelWidth, 5);
        _fillColorPreview.Size = new Size(25, 23);
        _fillColorPreview.BorderStyle = BorderStyle.FixedSingle;

        _fillColorButton = new Button();
        _fillColorButton.Text = "Choose...";
        _fillColorButton.Location = new Point(labelWidth + 30, 5);
        _fillColorButton.Size = new Size(inputWidth - 30, 23);

        _solidFillPanel.Controls.AddRange(new Control[]
        {
            _solidColorLabel, _fillColorPreview, _fillColorButton
        });

        // Gradient fill panel
        _gradientFillPanel = new Panel();
        _gradientFillPanel.Location = new Point(0, 35);
        _gradientFillPanel.Size = new Size(350, 70);
        _gradientFillPanel.Visible = false;

        _gradient1Label = new Label();
        _gradient1Label.Text = "Color 1:";
        _gradient1Label.Location = new Point(0, 5);
        _gradient1Label.Size = new Size(labelWidth, 23);
        _gradient1Label.TextAlign = ContentAlignment.MiddleLeft;

        _gradientColor1Preview = new Panel();
        _gradientColor1Preview.Location = new Point(labelWidth, 5);
        _gradientColor1Preview.Size = new Size(25, 23);
        _gradientColor1Preview.BorderStyle = BorderStyle.FixedSingle;

        _gradientColor1Button = new Button();
        _gradientColor1Button.Text = "Choose...";
        _gradientColor1Button.Location = new Point(labelWidth + 30, 5);
        _gradientColor1Button.Size = new Size(inputWidth - 30, 23);

        _gradient2Label = new Label();
        _gradient2Label.Text = "Color 2:";
        _gradient2Label.Location = new Point(0, 35);
        _gradient2Label.Size = new Size(labelWidth, 23);
        _gradient2Label.TextAlign = ContentAlignment.MiddleLeft;

        _gradientColor2Preview = new Panel();
        _gradientColor2Preview.Location = new Point(labelWidth, 35);
        _gradientColor2Preview.Size = new Size(25, 23);
        _gradientColor2Preview.BorderStyle = BorderStyle.FixedSingle;

        _gradientColor2Button = new Button();
        _gradientColor2Button.Text = "Choose...";
        _gradientColor2Button.Location = new Point(labelWidth + 30, 35);
        _gradientColor2Button.Size = new Size(inputWidth - 30, 23);

        _gradientFillPanel.Controls.AddRange(new Control[]
        {
            _gradient1Label, _gradientColor1Preview, _gradientColor1Button,
            _gradient2Label, _gradientColor2Preview, _gradientColor2Button
        });

        _fillPanel.Controls.AddRange(new Control[]
        {
            _fillTypeLabel, _fillTypeCombo,
            _solidFillPanel, _gradientFillPanel
        });

        _fillGroupBox.Controls.Add(_hasFillCheckbox);
        _fillGroupBox.Controls.Add(_fillPanel);
        currentY += _fillGroupBox.Height + padding;

        // ===== PREVIEW GROUP BOX =====
        _previewGroupBox = new GroupBox();
        _previewGroupBox.Text = "Preview";
        _previewGroupBox.Location = new Point(padding, currentY);
        _previewGroupBox.Size = new Size(Size.Width - padding * 2 - 16, 80);

        _previewPanel = new Panel();
        _previewPanel.Location = new Point(10, 20);
        _previewPanel.Size = new Size(_previewGroupBox.Width - 20, 50);
        _previewPanel.BorderStyle = BorderStyle.FixedSingle;

        _previewGroupBox.Controls.Add(_previewPanel);

        // ===== BUTTONS =====
        _okButton = new Button();
        _okButton.Text = "OK";
        _okButton.Location = new Point(Size.Width - 180, Size.Height - 75);
        _okButton.Size = new Size(75, 25);
        _okButton.DialogResult = DialogResult.OK;

        _cancelButton = new Button();
        _cancelButton.Text = "Cancel";
        _cancelButton.Location = new Point(Size.Width - 95, Size.Height - 75);
        _cancelButton.Size = new Size(75, 25);
        _cancelButton.DialogResult = DialogResult.Cancel;

        // ===== FORM PROPERTIES =====
        AcceptButton = _okButton;
        CancelButton = _cancelButton;
        ClientSize = new Size(420, 520);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "ShapePropertiesDialog";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Shape Properties";

        Controls.AddRange(new Control[]
        {
            _penGroupBox,
            _fillGroupBox,
            _previewGroupBox,
            _okButton,
            _cancelButton
        });

        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    // Pen controls
    private GroupBox _penGroupBox;
    private CheckBox _hasPenCheckbox;
    private Panel _penPanel;
    private Label _penColorLabel;
    private Button _penColorButton;
    private NumericUpDown _penWidthInput;
    private Label _penWidthLabel;
    private Label _dashStyleLabel;
    private ComboBox _dashStyleCombo;
    private Label _startCapLabel;
    private ComboBox _startCapCombo;
    private Label _endCapLabel;
    private ComboBox _endCapCombo;
    private Panel _penColorPreview;

    // Fill controls
    private GroupBox _fillGroupBox;
    private CheckBox _hasFillCheckbox;
    private Panel _fillPanel;
    private Label _fillTypeLabel;
    private ComboBox _fillTypeCombo;
    private Panel _solidFillPanel;
    private Label _solidColorLabel;
    private Button _fillColorButton;
    private Panel _fillColorPreview;
    private Panel _gradientFillPanel;
    private Label _gradient1Label;
    private Button _gradientColor1Button;
    private Panel _gradientColor1Preview;
    private Label _gradient2Label;
    private Button _gradientColor2Button;
    private Panel _gradientColor2Preview;

    // Preview
    private GroupBox _previewGroupBox;
    private Panel _previewPanel;

    // Buttons
    private Button _okButton;
    private Button _cancelButton;
}
