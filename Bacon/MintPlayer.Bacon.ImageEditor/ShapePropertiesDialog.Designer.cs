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
        _penGroupBox = new GroupBox();
        _hasPenCheckbox = new CheckBox();
        _penPanel = new Panel();
        _penColorLabel = new Label();
        _penColorPreview = new Panel();
        _penColorButton = new Button();
        _penWidthLabel = new Label();
        _penWidthInput = new NumericUpDown();
        _dashStyleLabel = new Label();
        _dashStyleCombo = new ComboBox();
        _startCapLabel = new Label();
        _startCapCombo = new ComboBox();
        _middleCapLabel = new Label();
        _middleCapCombo = new ComboBox();
        _endCapLabel = new Label();
        _endCapCombo = new ComboBox();
        _fillGroupBox = new GroupBox();
        _hasFillCheckbox = new CheckBox();
        _fillPanel = new Panel();
        _fillTypeLabel = new Label();
        _fillTypeCombo = new ComboBox();
        _solidFillPanel = new Panel();
        _solidColorLabel = new Label();
        _fillColorPreview = new Panel();
        _fillColorButton = new Button();
        _gradientFillPanel = new Panel();
        _gradient1Label = new Label();
        _gradientColor1Preview = new Panel();
        _gradientColor1Button = new Button();
        _gradient2Label = new Label();
        _gradientColor2Preview = new Panel();
        _gradientColor2Button = new Button();
        _previewGroupBox = new GroupBox();
        _previewPanel = new Panel();
        _okButton = new Button();
        _cancelButton = new Button();

        _penGroupBox.SuspendLayout();
        _penPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)_penWidthInput).BeginInit();
        _fillGroupBox.SuspendLayout();
        _fillPanel.SuspendLayout();
        _solidFillPanel.SuspendLayout();
        _gradientFillPanel.SuspendLayout();
        _previewGroupBox.SuspendLayout();
        SuspendLayout();

        //
        // _penGroupBox
        //
        _penGroupBox.Controls.Add(_penPanel);
        _penGroupBox.Controls.Add(_hasPenCheckbox);
        _penGroupBox.Location = new Point(12, 12);
        _penGroupBox.Name = "_penGroupBox";
        _penGroupBox.Size = new Size(380, 180);
        _penGroupBox.TabIndex = 0;
        _penGroupBox.TabStop = false;
        _penGroupBox.Text = "Stroke (Pen)";

        //
        // _hasPenCheckbox
        //
        _hasPenCheckbox.AutoSize = true;
        _hasPenCheckbox.Location = new Point(10, 20);
        _hasPenCheckbox.Name = "_hasPenCheckbox";
        _hasPenCheckbox.Size = new Size(82, 19);
        _hasPenCheckbox.TabIndex = 0;
        _hasPenCheckbox.Text = "Has stroke";
        _hasPenCheckbox.UseVisualStyleBackColor = true;

        //
        // _penPanel
        //
        _penPanel.Controls.Add(_endCapCombo);
        _penPanel.Controls.Add(_endCapLabel);
        _penPanel.Controls.Add(_middleCapCombo);
        _penPanel.Controls.Add(_middleCapLabel);
        _penPanel.Controls.Add(_startCapCombo);
        _penPanel.Controls.Add(_startCapLabel);
        _penPanel.Controls.Add(_dashStyleCombo);
        _penPanel.Controls.Add(_dashStyleLabel);
        _penPanel.Controls.Add(_penWidthInput);
        _penPanel.Controls.Add(_penWidthLabel);
        _penPanel.Controls.Add(_penColorButton);
        _penPanel.Controls.Add(_penColorPreview);
        _penPanel.Controls.Add(_penColorLabel);
        _penPanel.Location = new Point(10, 45);
        _penPanel.Name = "_penPanel";
        _penPanel.Size = new Size(360, 125);
        _penPanel.TabIndex = 1;

        //
        // _penColorLabel
        //
        _penColorLabel.Location = new Point(0, 5);
        _penColorLabel.Name = "_penColorLabel";
        _penColorLabel.Size = new Size(80, 23);
        _penColorLabel.TabIndex = 0;
        _penColorLabel.Text = "Color:";
        _penColorLabel.TextAlign = ContentAlignment.MiddleLeft;

        //
        // _penColorPreview
        //
        _penColorPreview.BorderStyle = BorderStyle.FixedSingle;
        _penColorPreview.Location = new Point(80, 5);
        _penColorPreview.Name = "_penColorPreview";
        _penColorPreview.Size = new Size(25, 23);
        _penColorPreview.TabIndex = 1;

        //
        // _penColorButton
        //
        _penColorButton.Location = new Point(110, 5);
        _penColorButton.Name = "_penColorButton";
        _penColorButton.Size = new Size(90, 23);
        _penColorButton.TabIndex = 2;
        _penColorButton.Text = "Choose...";
        _penColorButton.UseVisualStyleBackColor = true;

        //
        // _penWidthLabel
        //
        _penWidthLabel.Location = new Point(0, 35);
        _penWidthLabel.Name = "_penWidthLabel";
        _penWidthLabel.Size = new Size(80, 23);
        _penWidthLabel.TabIndex = 3;
        _penWidthLabel.Text = "Width:";
        _penWidthLabel.TextAlign = ContentAlignment.MiddleLeft;

        //
        // _penWidthInput
        //
        _penWidthInput.DecimalPlaces = 1;
        _penWidthInput.Location = new Point(80, 35);
        _penWidthInput.Maximum = new decimal(new int[] { 100, 0, 0, 0 });
        _penWidthInput.Minimum = new decimal(new int[] { 1, 0, 0, 65536 });
        _penWidthInput.Name = "_penWidthInput";
        _penWidthInput.Size = new Size(120, 23);
        _penWidthInput.TabIndex = 4;
        _penWidthInput.Value = new decimal(new int[] { 1, 0, 0, 0 });

        //
        // _dashStyleLabel
        //
        _dashStyleLabel.Location = new Point(0, 65);
        _dashStyleLabel.Name = "_dashStyleLabel";
        _dashStyleLabel.Size = new Size(80, 23);
        _dashStyleLabel.TabIndex = 5;
        _dashStyleLabel.Text = "Dash Style:";
        _dashStyleLabel.TextAlign = ContentAlignment.MiddleLeft;

        //
        // _dashStyleCombo
        //
        _dashStyleCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        _dashStyleCombo.FormattingEnabled = true;
        _dashStyleCombo.Location = new Point(80, 65);
        _dashStyleCombo.Name = "_dashStyleCombo";
        _dashStyleCombo.Size = new Size(120, 23);
        _dashStyleCombo.TabIndex = 6;

        //
        // _startCapLabel
        //
        _startCapLabel.Location = new Point(210, 5);
        _startCapLabel.Name = "_startCapLabel";
        _startCapLabel.Size = new Size(70, 23);
        _startCapLabel.TabIndex = 7;
        _startCapLabel.Text = "Start Cap:";
        _startCapLabel.TextAlign = ContentAlignment.MiddleLeft;

        //
        // _startCapCombo
        //
        _startCapCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        _startCapCombo.FormattingEnabled = true;
        _startCapCombo.Location = new Point(280, 5);
        _startCapCombo.Name = "_startCapCombo";
        _startCapCombo.Size = new Size(75, 23);
        _startCapCombo.TabIndex = 8;

        //
        // _middleCapLabel
        //
        _middleCapLabel.Location = new Point(210, 35);
        _middleCapLabel.Name = "_middleCapLabel";
        _middleCapLabel.Size = new Size(70, 23);
        _middleCapLabel.TabIndex = 9;
        _middleCapLabel.Text = "Middle Cap:";
        _middleCapLabel.TextAlign = ContentAlignment.MiddleLeft;

        //
        // _middleCapCombo
        //
        _middleCapCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        _middleCapCombo.FormattingEnabled = true;
        _middleCapCombo.Location = new Point(280, 35);
        _middleCapCombo.Name = "_middleCapCombo";
        _middleCapCombo.Size = new Size(75, 23);
        _middleCapCombo.TabIndex = 10;

        //
        // _endCapLabel
        //
        _endCapLabel.Location = new Point(210, 65);
        _endCapLabel.Name = "_endCapLabel";
        _endCapLabel.Size = new Size(70, 23);
        _endCapLabel.TabIndex = 11;
        _endCapLabel.Text = "End Cap:";
        _endCapLabel.TextAlign = ContentAlignment.MiddleLeft;

        //
        // _endCapCombo
        //
        _endCapCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        _endCapCombo.FormattingEnabled = true;
        _endCapCombo.Location = new Point(280, 65);
        _endCapCombo.Name = "_endCapCombo";
        _endCapCombo.Size = new Size(75, 23);
        _endCapCombo.TabIndex = 12;

        //
        // _fillGroupBox
        //
        _fillGroupBox.Controls.Add(_fillPanel);
        _fillGroupBox.Controls.Add(_hasFillCheckbox);
        _fillGroupBox.Location = new Point(12, 198);
        _fillGroupBox.Name = "_fillGroupBox";
        _fillGroupBox.Size = new Size(380, 170);
        _fillGroupBox.TabIndex = 1;
        _fillGroupBox.TabStop = false;
        _fillGroupBox.Text = "Fill (Brush)";

        //
        // _hasFillCheckbox
        //
        _hasFillCheckbox.AutoSize = true;
        _hasFillCheckbox.Location = new Point(10, 20);
        _hasFillCheckbox.Name = "_hasFillCheckbox";
        _hasFillCheckbox.Size = new Size(64, 19);
        _hasFillCheckbox.TabIndex = 0;
        _hasFillCheckbox.Text = "Has fill";
        _hasFillCheckbox.UseVisualStyleBackColor = true;

        //
        // _fillPanel
        //
        _fillPanel.Controls.Add(_gradientFillPanel);
        _fillPanel.Controls.Add(_solidFillPanel);
        _fillPanel.Controls.Add(_fillTypeCombo);
        _fillPanel.Controls.Add(_fillTypeLabel);
        _fillPanel.Location = new Point(10, 45);
        _fillPanel.Name = "_fillPanel";
        _fillPanel.Size = new Size(360, 115);
        _fillPanel.TabIndex = 1;

        //
        // _fillTypeLabel
        //
        _fillTypeLabel.Location = new Point(0, 5);
        _fillTypeLabel.Name = "_fillTypeLabel";
        _fillTypeLabel.Size = new Size(80, 23);
        _fillTypeLabel.TabIndex = 0;
        _fillTypeLabel.Text = "Type:";
        _fillTypeLabel.TextAlign = ContentAlignment.MiddleLeft;

        //
        // _fillTypeCombo
        //
        _fillTypeCombo.DropDownStyle = ComboBoxStyle.DropDownList;
        _fillTypeCombo.FormattingEnabled = true;
        _fillTypeCombo.Location = new Point(80, 5);
        _fillTypeCombo.Name = "_fillTypeCombo";
        _fillTypeCombo.Size = new Size(120, 23);
        _fillTypeCombo.TabIndex = 1;

        //
        // _solidFillPanel
        //
        _solidFillPanel.Controls.Add(_fillColorButton);
        _solidFillPanel.Controls.Add(_fillColorPreview);
        _solidFillPanel.Controls.Add(_solidColorLabel);
        _solidFillPanel.Location = new Point(0, 35);
        _solidFillPanel.Name = "_solidFillPanel";
        _solidFillPanel.Size = new Size(350, 30);
        _solidFillPanel.TabIndex = 2;

        //
        // _solidColorLabel
        //
        _solidColorLabel.Location = new Point(0, 5);
        _solidColorLabel.Name = "_solidColorLabel";
        _solidColorLabel.Size = new Size(80, 23);
        _solidColorLabel.TabIndex = 0;
        _solidColorLabel.Text = "Color:";
        _solidColorLabel.TextAlign = ContentAlignment.MiddleLeft;

        //
        // _fillColorPreview
        //
        _fillColorPreview.BorderStyle = BorderStyle.FixedSingle;
        _fillColorPreview.Location = new Point(80, 5);
        _fillColorPreview.Name = "_fillColorPreview";
        _fillColorPreview.Size = new Size(25, 23);
        _fillColorPreview.TabIndex = 1;

        //
        // _fillColorButton
        //
        _fillColorButton.Location = new Point(110, 5);
        _fillColorButton.Name = "_fillColorButton";
        _fillColorButton.Size = new Size(90, 23);
        _fillColorButton.TabIndex = 2;
        _fillColorButton.Text = "Choose...";
        _fillColorButton.UseVisualStyleBackColor = true;

        //
        // _gradientFillPanel
        //
        _gradientFillPanel.Controls.Add(_gradientColor2Button);
        _gradientFillPanel.Controls.Add(_gradientColor2Preview);
        _gradientFillPanel.Controls.Add(_gradient2Label);
        _gradientFillPanel.Controls.Add(_gradientColor1Button);
        _gradientFillPanel.Controls.Add(_gradientColor1Preview);
        _gradientFillPanel.Controls.Add(_gradient1Label);
        _gradientFillPanel.Location = new Point(0, 35);
        _gradientFillPanel.Name = "_gradientFillPanel";
        _gradientFillPanel.Size = new Size(350, 70);
        _gradientFillPanel.TabIndex = 3;
        _gradientFillPanel.Visible = false;

        //
        // _gradient1Label
        //
        _gradient1Label.Location = new Point(0, 5);
        _gradient1Label.Name = "_gradient1Label";
        _gradient1Label.Size = new Size(80, 23);
        _gradient1Label.TabIndex = 0;
        _gradient1Label.Text = "Color 1:";
        _gradient1Label.TextAlign = ContentAlignment.MiddleLeft;

        //
        // _gradientColor1Preview
        //
        _gradientColor1Preview.BorderStyle = BorderStyle.FixedSingle;
        _gradientColor1Preview.Location = new Point(80, 5);
        _gradientColor1Preview.Name = "_gradientColor1Preview";
        _gradientColor1Preview.Size = new Size(25, 23);
        _gradientColor1Preview.TabIndex = 1;

        //
        // _gradientColor1Button
        //
        _gradientColor1Button.Location = new Point(110, 5);
        _gradientColor1Button.Name = "_gradientColor1Button";
        _gradientColor1Button.Size = new Size(90, 23);
        _gradientColor1Button.TabIndex = 2;
        _gradientColor1Button.Text = "Choose...";
        _gradientColor1Button.UseVisualStyleBackColor = true;

        //
        // _gradient2Label
        //
        _gradient2Label.Location = new Point(0, 35);
        _gradient2Label.Name = "_gradient2Label";
        _gradient2Label.Size = new Size(80, 23);
        _gradient2Label.TabIndex = 3;
        _gradient2Label.Text = "Color 2:";
        _gradient2Label.TextAlign = ContentAlignment.MiddleLeft;

        //
        // _gradientColor2Preview
        //
        _gradientColor2Preview.BorderStyle = BorderStyle.FixedSingle;
        _gradientColor2Preview.Location = new Point(80, 35);
        _gradientColor2Preview.Name = "_gradientColor2Preview";
        _gradientColor2Preview.Size = new Size(25, 23);
        _gradientColor2Preview.TabIndex = 4;

        //
        // _gradientColor2Button
        //
        _gradientColor2Button.Location = new Point(110, 35);
        _gradientColor2Button.Name = "_gradientColor2Button";
        _gradientColor2Button.Size = new Size(90, 23);
        _gradientColor2Button.TabIndex = 5;
        _gradientColor2Button.Text = "Choose...";
        _gradientColor2Button.UseVisualStyleBackColor = true;

        //
        // _previewGroupBox
        //
        _previewGroupBox.Controls.Add(_previewPanel);
        _previewGroupBox.Location = new Point(12, 374);
        _previewGroupBox.Name = "_previewGroupBox";
        _previewGroupBox.Size = new Size(380, 80);
        _previewGroupBox.TabIndex = 2;
        _previewGroupBox.TabStop = false;
        _previewGroupBox.Text = "Preview";

        //
        // _previewPanel
        //
        _previewPanel.BorderStyle = BorderStyle.FixedSingle;
        _previewPanel.Location = new Point(10, 20);
        _previewPanel.Name = "_previewPanel";
        _previewPanel.Size = new Size(360, 50);
        _previewPanel.TabIndex = 0;

        //
        // _okButton
        //
        _okButton.DialogResult = DialogResult.OK;
        _okButton.Location = new Point(228, 462);
        _okButton.Name = "_okButton";
        _okButton.Size = new Size(75, 25);
        _okButton.TabIndex = 3;
        _okButton.Text = "OK";
        _okButton.UseVisualStyleBackColor = true;

        //
        // _cancelButton
        //
        _cancelButton.DialogResult = DialogResult.Cancel;
        _cancelButton.Location = new Point(317, 462);
        _cancelButton.Name = "_cancelButton";
        _cancelButton.Size = new Size(75, 25);
        _cancelButton.TabIndex = 4;
        _cancelButton.Text = "Cancel";
        _cancelButton.UseVisualStyleBackColor = true;

        //
        // ShapePropertiesDialog
        //
        AcceptButton = _okButton;
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        CancelButton = _cancelButton;
        ClientSize = new Size(404, 499);
        Controls.Add(_cancelButton);
        Controls.Add(_okButton);
        Controls.Add(_previewGroupBox);
        Controls.Add(_fillGroupBox);
        Controls.Add(_penGroupBox);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "ShapePropertiesDialog";
        StartPosition = FormStartPosition.CenterParent;
        Text = "Shape Properties";
        _penGroupBox.ResumeLayout(false);
        _penGroupBox.PerformLayout();
        _penPanel.ResumeLayout(false);
        ((System.ComponentModel.ISupportInitialize)_penWidthInput).EndInit();
        _fillGroupBox.ResumeLayout(false);
        _fillGroupBox.PerformLayout();
        _fillPanel.ResumeLayout(false);
        _solidFillPanel.ResumeLayout(false);
        _gradientFillPanel.ResumeLayout(false);
        _previewGroupBox.ResumeLayout(false);
        ResumeLayout(false);
    }

    #endregion

    private GroupBox _penGroupBox;
    private CheckBox _hasPenCheckbox;
    private Panel _penPanel;
    private Label _penColorLabel;
    private Panel _penColorPreview;
    private Button _penColorButton;
    private Label _penWidthLabel;
    private NumericUpDown _penWidthInput;
    private Label _dashStyleLabel;
    private ComboBox _dashStyleCombo;
    private Label _startCapLabel;
    private ComboBox _startCapCombo;
    private Label _middleCapLabel;
    private ComboBox _middleCapCombo;
    private Label _endCapLabel;
    private ComboBox _endCapCombo;
    private GroupBox _fillGroupBox;
    private CheckBox _hasFillCheckbox;
    private Panel _fillPanel;
    private Label _fillTypeLabel;
    private ComboBox _fillTypeCombo;
    private Panel _solidFillPanel;
    private Label _solidColorLabel;
    private Panel _fillColorPreview;
    private Button _fillColorButton;
    private Panel _gradientFillPanel;
    private Label _gradient1Label;
    private Panel _gradientColor1Preview;
    private Button _gradientColor1Button;
    private Label _gradient2Label;
    private Panel _gradientColor2Preview;
    private Button _gradientColor2Button;
    private GroupBox _previewGroupBox;
    private Panel _previewPanel;
    private Button _okButton;
    private Button _cancelButton;
}
