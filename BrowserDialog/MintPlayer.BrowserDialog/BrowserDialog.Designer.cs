using System;

namespace MintPlayer.BrowserDialog
{
    partial class BrowserDialog
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
            btnOK = new Button();
            btnCancel = new Button();
            pnlParent = new Panel();
            lvBrowsers = new ListView();
            pnlLoading = new Panel();
            progressBar3 = new ProgressBar();
            pnlParent.SuspendLayout();
            pnlLoading.SuspendLayout();
            SuspendLayout();
            // 
            // btnOK
            // 
            btnOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnOK.DialogResult = DialogResult.OK;
            btnOK.Enabled = false;
            btnOK.Location = new Point(268, 270);
            btnOK.Margin = new Padding(4, 3, 4, 3);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(88, 29);
            btnOK.TabIndex = 0;
            btnOK.Text = "OK";
            btnOK.UseVisualStyleBackColor = true;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(362, 270);
            btnCancel.Margin = new Padding(4, 3, 4, 3);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(88, 29);
            btnCancel.TabIndex = 1;
            btnCancel.Text = "Annuleren";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // pnlParent
            // 
            pnlParent.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            pnlParent.Controls.Add(lvBrowsers);
            pnlParent.Controls.Add(pnlLoading);
            pnlParent.Location = new Point(12, 12);
            pnlParent.Name = "pnlParent";
            pnlParent.Size = new Size(440, 252);
            pnlParent.TabIndex = 3;
            // 
            // lvBrowsers
            // 
            lvBrowsers.Dock = DockStyle.Fill;
            lvBrowsers.Location = new Point(0, 0);
            lvBrowsers.Margin = new Padding(4, 3, 4, 3);
            lvBrowsers.MultiSelect = false;
            lvBrowsers.Name = "lvBrowsers";
            lvBrowsers.Size = new Size(440, 252);
            lvBrowsers.TabIndex = 2;
            lvBrowsers.UseCompatibleStateImageBehavior = false;
            lvBrowsers.SelectedIndexChanged += LvBrowsers_SelectedIndexChanged;
            // 
            // pnlLoading
            // 
            pnlLoading.Controls.Add(progressBar3);
            pnlLoading.Dock = DockStyle.Fill;
            pnlLoading.Location = new Point(0, 0);
            pnlLoading.Name = "pnlLoading";
            pnlLoading.Size = new Size(440, 252);
            pnlLoading.TabIndex = 0;
            // 
            // progressBar3
            // 
            progressBar3.Anchor = AnchorStyles.None;
            progressBar3.Location = new Point(145, 119);
            progressBar3.Name = "progressBar3";
            progressBar3.Size = new Size(150, 14);
            progressBar3.Style = ProgressBarStyle.Marquee;
            progressBar3.TabIndex = 5;
            progressBar3.Value = 50;
            // 
            // BrowserDialog
            // 
            AcceptButton = btnOK;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(464, 313);
            Controls.Add(pnlParent);
            Controls.Add(btnOK);
            Controls.Add(btnCancel);
            Margin = new Padding(4, 3, 4, 3);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "BrowserDialog";
            Text = "Pick a browser";
            Load += BrowserDialog_Load;
            Shown += BrowserDialog_Shown;
            pnlParent.ResumeLayout(false);
            pnlLoading.ResumeLayout(false);
            ResumeLayout(false);
        }

        #endregion

        private Button btnOK;
        private Button btnCancel;
        private Panel pnlLoading;
        private ListView lvBrowsers;
        private Panel pnlParent;
        private ProgressBar progressBar1;
        private ProgressBar progressBar3;
        private ProgressBar progressBar2;
    }
}

