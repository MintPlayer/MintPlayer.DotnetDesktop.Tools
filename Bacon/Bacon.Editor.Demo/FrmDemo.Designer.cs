namespace MintPlayer.Bacon.Editor.Demo;

partial class FrmDemo
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
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
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.editor = new MintPlayer.Bacon.Editor.Controls.BaconEditorControl();
        this.btnAddImage = new System.Windows.Forms.Button();
        this.btnAddRect = new System.Windows.Forms.Button();
        this.btnAddEllipse = new System.Windows.Forms.Button();
        this.btnAddPolygon = new System.Windows.Forms.Button();
        this.pnlTools = new System.Windows.Forms.FlowLayoutPanel();
        this.btnExportIco = new System.Windows.Forms.Button();
        this.pnlTools.SuspendLayout();
        this.SuspendLayout();
        // 
        // editor
        // 
        this.editor.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
        this.editor.Location = new System.Drawing.Point(12, 47);
        this.editor.Name = "editor";
        this.editor.Size = new System.Drawing.Size(776, 391);
        this.editor.TabIndex = 0;
        // 
        // btnAddImage
        // 
        this.btnAddImage.Location = new System.Drawing.Point(3, 3);
        this.btnAddImage.Name = "btnAddImage";
        this.btnAddImage.Size = new System.Drawing.Size(90, 30);
        this.btnAddImage.TabIndex = 1;
        this.btnAddImage.Text = "Add 64x64";
        this.btnAddImage.UseVisualStyleBackColor = true;
        this.btnAddImage.Click += new System.EventHandler(this.btnAddImage_Click);
        // 
        // btnAddRect
        // 
        this.btnAddRect.Location = new System.Drawing.Point(99, 3);
        this.btnAddRect.Name = "btnAddRect";
        this.btnAddRect.Size = new System.Drawing.Size(90, 30);
        this.btnAddRect.TabIndex = 2;
        this.btnAddRect.Text = "Rectangle";
        this.btnAddRect.UseVisualStyleBackColor = true;
        this.btnAddRect.Click += new System.EventHandler(this.btnAddRect_Click);
        // 
        // btnAddEllipse
        // 
        this.btnAddEllipse.Location = new System.Drawing.Point(195, 3);
        this.btnAddEllipse.Name = "btnAddEllipse";
        this.btnAddEllipse.Size = new System.Drawing.Size(90, 30);
        this.btnAddEllipse.TabIndex = 3;
        this.btnAddEllipse.Text = "Ellipse";
        this.btnAddEllipse.UseVisualStyleBackColor = true;
        this.btnAddEllipse.Click += new System.EventHandler(this.btnAddEllipse_Click);
        // 
        // btnAddPolygon
        // 
        this.btnAddPolygon.Location = new System.Drawing.Point(291, 3);
        this.btnAddPolygon.Name = "btnAddPolygon";
        this.btnAddPolygon.Size = new System.Drawing.Size(90, 30);
        this.btnAddPolygon.TabIndex = 4;
        this.btnAddPolygon.Text = "Triangle";
        this.btnAddPolygon.UseVisualStyleBackColor = true;
        this.btnAddPolygon.Click += new System.EventHandler(this.btnAddPolygon_Click);
        // 
        // pnlTools
        // 
        this.pnlTools.AutoSize = true;
        this.pnlTools.Controls.Add(this.btnAddImage);
        this.pnlTools.Controls.Add(this.btnAddRect);
        this.pnlTools.Controls.Add(this.btnAddEllipse);
        this.pnlTools.Controls.Add(this.btnAddPolygon);
        this.pnlTools.Controls.Add(this.btnExportIco);
        this.pnlTools.Location = new System.Drawing.Point(12, 12);
        this.pnlTools.Name = "pnlTools";
        this.pnlTools.Size = new System.Drawing.Size(488, 36);
        this.pnlTools.TabIndex = 5;
        // 
        // btnExportIco
        // 
        this.btnExportIco.Location = new System.Drawing.Point(387, 3);
        this.btnExportIco.Name = "btnExportIco";
        this.btnExportIco.Size = new System.Drawing.Size(98, 30);
        this.btnExportIco.TabIndex = 5;
        this.btnExportIco.Text = "Export ICO";
        this.btnExportIco.UseVisualStyleBackColor = true;
        this.btnExportIco.Click += new System.EventHandler(this.btnExportIco_Click);
        // 
        // FrmDemo
        // 
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(800, 450);
        this.Controls.Add(this.pnlTools);
        this.Controls.Add(this.editor);
        this.Name = "FrmDemo";
        this.Text = "Bacon Icon Editor Demo";
        this.Load += new System.EventHandler(this.FrmDemo_Load);
        this.pnlTools.ResumeLayout(false);
        this.ResumeLayout(false);
        this.PerformLayout();

    }

    #endregion

    private MintPlayer.Bacon.Editor.Controls.BaconEditorControl editor;
    private System.Windows.Forms.Button btnAddImage;
    private System.Windows.Forms.Button btnAddRect;
    private System.Windows.Forms.Button btnAddEllipse;
    private System.Windows.Forms.Button btnAddPolygon;
    private System.Windows.Forms.FlowLayoutPanel pnlTools;
    private System.Windows.Forms.Button btnExportIco;
}
