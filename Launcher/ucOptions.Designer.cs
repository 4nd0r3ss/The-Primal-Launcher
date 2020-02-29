namespace Launcher
{
    partial class ucOptions
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.txtGameInstallPath = new System.Windows.Forms.TextBox();
            this.btnChangeGameInstallPath = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.btnChangePatchPath = new System.Windows.Forms.Button();
            this.txtPatchPath = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.radioButton3 = new System.Windows.Forms.RadioButton();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtGameInstallPath
            // 
            this.txtGameInstallPath.Enabled = false;
            this.txtGameInstallPath.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.txtGameInstallPath.Location = new System.Drawing.Point(27, 26);
            this.txtGameInstallPath.Name = "txtGameInstallPath";
            this.txtGameInstallPath.Size = new System.Drawing.Size(488, 20);
            this.txtGameInstallPath.TabIndex = 11;
            // 
            // btnChangeGameInstallPath
            // 
            this.btnChangeGameInstallPath.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnChangeGameInstallPath.Location = new System.Drawing.Point(521, 24);
            this.btnChangeGameInstallPath.Name = "btnChangeGameInstallPath";
            this.btnChangeGameInstallPath.Size = new System.Drawing.Size(58, 23);
            this.btnChangeGameInstallPath.TabIndex = 12;
            this.btnChangeGameInstallPath.Text = "Change";
            this.btnChangeGameInstallPath.UseVisualStyleBackColor = true;
            this.btnChangeGameInstallPath.Click += new System.EventHandler(this.btnChangeGameInstallPath_Click_1);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.label2.Location = new System.Drawing.Point(27, 59);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(107, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Application files path:";
            // 
            // btnChangePatchPath
            // 
            this.btnChangePatchPath.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnChangePatchPath.Location = new System.Drawing.Point(521, 73);
            this.btnChangePatchPath.Name = "btnChangePatchPath";
            this.btnChangePatchPath.Size = new System.Drawing.Size(58, 23);
            this.btnChangePatchPath.TabIndex = 5;
            this.btnChangePatchPath.Text = "Change";
            this.btnChangePatchPath.UseVisualStyleBackColor = true;
            this.btnChangePatchPath.Click += new System.EventHandler(this.btnChangePatchPath_Click_1);
            // 
            // txtPatchPath
            // 
            this.txtPatchPath.Enabled = false;
            this.txtPatchPath.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.txtPatchPath.Location = new System.Drawing.Point(27, 75);
            this.txtPatchPath.Name = "txtPatchPath";
            this.txtPatchPath.Size = new System.Drawing.Size(488, 20);
            this.txtPatchPath.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.label1.Location = new System.Drawing.Point(27, 7);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(114, 13);
            this.label1.TabIndex = 15;
            this.label1.Text = "Game installation path:";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.radioButton3);
            this.groupBox3.Controls.Add(this.radioButton2);
            this.groupBox3.Controls.Add(this.radioButton1);
            this.groupBox3.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.groupBox3.Location = new System.Drawing.Point(30, 117);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(289, 100);
            this.groupBox3.TabIndex = 16;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Web server options";
            // 
            // radioButton3
            // 
            this.radioButton3.AutoSize = true;
            this.radioButton3.Location = new System.Drawing.Point(13, 71);
            this.radioButton3.Name = "radioButton3";
            this.radioButton3.Size = new System.Drawing.Size(267, 17);
            this.radioButton3.TabIndex = 2;
            this.radioButton3.TabStop = true;
            this.radioButton3.Text = "Use an http server installed in this machine (unsafe)";
            this.radioButton3.UseVisualStyleBackColor = true;
            // 
            // radioButton2
            // 
            this.radioButton2.AutoSize = true;
            this.radioButton2.Location = new System.Drawing.Point(13, 47);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(204, 17);
            this.radioButton2.TabIndex = 1;
            this.radioButton2.TabStop = true;
            this.radioButton2.Text = "Log-in automatically using default user";
            this.radioButton2.UseVisualStyleBackColor = true;
            this.radioButton2.CheckedChanged += new System.EventHandler(this.radioButton2_CheckedChanged);
            // 
            // radioButton1
            // 
            this.radioButton1.AutoSize = true;
            this.radioButton1.Location = new System.Drawing.Point(13, 23);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(208, 17);
            this.radioButton1.TabIndex = 0;
            this.radioButton1.TabStop = true;
            this.radioButton1.Text = "Use built-in web server (recommended)";
            this.radioButton1.UseVisualStyleBackColor = true;
            // 
            // ucOptions
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.txtGameInstallPath);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.btnChangePatchPath);
            this.Controls.Add(this.btnChangeGameInstallPath);
            this.Controls.Add(this.txtPatchPath);
            this.Name = "ucOptions";
            this.Size = new System.Drawing.Size(605, 427);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtGameInstallPath;
        private System.Windows.Forms.Button btnChangeGameInstallPath;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnChangePatchPath;
        private System.Windows.Forms.TextBox txtPatchPath;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.RadioButton radioButton3;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.RadioButton radioButton1;
    }
}
