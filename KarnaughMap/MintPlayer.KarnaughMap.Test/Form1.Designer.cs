namespace KarnaughMap.Test
{
    public partial class Form1
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
            karnaughMap1 = new MintPlayer.KarnaughMap.KarnaughMap();
            btnRandomFill = new System.Windows.Forms.Button();
            btnSolve = new System.Windows.Forms.Button();
            btnSolveSelection = new System.Windows.Forms.Button();
            lstLoopOnes = new System.Windows.Forms.ListBox();
            lstLoopZeros = new System.Windows.Forms.ListBox();
            cmbMode = new System.Windows.Forms.ComboBox();
            SuspendLayout();
            // 
            // karnaughMap1
            // 
            karnaughMap1.Location = new System.Drawing.Point(250, 51);
            karnaughMap1.Name = "karnaughMap1";
            this.karnaughMap1.OutputVariable = "X";
            karnaughMap1.Size = new System.Drawing.Size(697, 377);
            karnaughMap1.TabIndex = 0;
            karnaughMap1.KarnaughMapSolved += KarnaughMap1_KarnaughMapSolved;
            karnaughMap1.KarnaughLoopAdded += KarnaughMap1_KarnaughLoopAdded;
            karnaughMap1.ModeChanging += KarnaughMap1_ModeChanging;
            karnaughMap1.InputVariables.AddRange(new string[] { "A", "B", "C", "D", "E", "F", "G" });
            // 
            // btnRandomFill
            // 
            btnRandomFill.Location = new System.Drawing.Point(13, 13);
            btnRandomFill.Name = "btnRandomFill";
            btnRandomFill.Size = new System.Drawing.Size(150, 25);
            btnRandomFill.TabIndex = 1;
            btnRandomFill.Text = "Random fill";
            btnRandomFill.UseVisualStyleBackColor = true;
            btnRandomFill.Click += BtnRandomFill_Click;
            // 
            // btnSolve
            // 
            btnSolve.Location = new System.Drawing.Point(183, 13);
            btnSolve.Name = "btnSolve";
            btnSolve.Size = new System.Drawing.Size(150, 25);
            btnSolve.TabIndex = 2;
            btnSolve.Text = "Solve";
            btnSolve.UseVisualStyleBackColor = true;
            btnSolve.Click += BtnSolve_Click;
            // 
            // btnSolveSelection
            // 
            btnSolveSelection.Location = new System.Drawing.Point(353, 13);
            btnSolveSelection.Name = "btnSolveSelection";
            btnSolveSelection.Size = new System.Drawing.Size(150, 25);
            btnSolveSelection.TabIndex = 3;
            btnSolveSelection.Text = "Solve selection";
            btnSolveSelection.UseVisualStyleBackColor = true;
            btnSolveSelection.Click += BtnSolveSelection_Click;
            // 
            // lstLoopOnes
            // 
            lstLoopOnes.Location = new System.Drawing.Point(20, 50);
            lstLoopOnes.Name = "lstLoopOnes";
            lstLoopOnes.Size = new System.Drawing.Size(200, 289);
            lstLoopOnes.TabIndex = 4;
            lstLoopOnes.SelectedIndexChanged += LstLoopOnes_SelectedIndexChanged;
            // 
            // lstLoopZeros
            // 
            lstLoopZeros.Location = new System.Drawing.Point(20, 365);
            lstLoopZeros.Name = "lstLoopZeros";
            lstLoopZeros.Size = new System.Drawing.Size(200, 289);
            lstLoopZeros.TabIndex = 5;
            lstLoopZeros.SelectedIndexChanged += LstLoopZeros_SelectedIndexChanged;
            // 
            // cmbMode
            // 
            cmbMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cmbMode.Items.AddRange(new object[] { "Edit", "Solve" });
            cmbMode.Location = new System.Drawing.Point(543, 14);
            cmbMode.Name = "cmbMode";
            cmbMode.Size = new System.Drawing.Size(200, 23);
            cmbMode.TabIndex = 6;
            cmbMode.SelectedIndexChanged += cmbMode_SelectedIndexChanged;
            // 
            // Form1
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            BackColor = System.Drawing.SystemColors.Window;
            ClientSize = new System.Drawing.Size(800, 667);
            Controls.Add(karnaughMap1);
            Controls.Add(btnRandomFill);
            Controls.Add(btnSolve);
            Controls.Add(btnSolveSelection);
            Controls.Add(lstLoopOnes);
            Controls.Add(lstLoopZeros);
            Controls.Add(cmbMode);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
        }
        #endregion

        private MintPlayer.KarnaughMap.KarnaughMap karnaughMap1;
        private System.Windows.Forms.Button btnRandomFill;
        private System.Windows.Forms.Button btnSolve;
        private System.Windows.Forms.Button btnSolveSelection;
        private System.Windows.Forms.ListBox lstLoopOnes;
        private System.Windows.Forms.ListBox lstLoopZeros;
        private System.Windows.Forms.ComboBox cmbMode;
    }
}

