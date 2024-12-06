namespace MintPlayer.BrowserDialog.Test
{
    partial class MainForm
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
            this.btnPickBrowser = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // btnKiesBrowser
            // 
            this.btnPickBrowser.Location = new System.Drawing.Point(12, 12);
            this.btnPickBrowser.Name = "btnKiesBrowser";
            this.btnPickBrowser.Size = new System.Drawing.Size(115, 25);
            this.btnPickBrowser.TabIndex = 0;
            this.btnPickBrowser.Text = "Kies een browser";
            this.btnPickBrowser.UseVisualStyleBackColor = true;
            this.btnPickBrowser.Click += new System.EventHandler(this.btnPickBrowser_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Controls.Add(this.btnPickBrowser);
            this.Name = "MainForm";
            this.Text = "Demo BrowserDialog";
            this.ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Button btnPickBrowser;
    }
}

