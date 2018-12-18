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
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.LblDownloadStatus = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.label2 = new System.Windows.Forms.Label();
            this.btnChangePatchPath = new System.Windows.Forms.Button();
            this.txtPatchPath = new System.Windows.Forms.TextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.button2 = new System.Windows.Forms.Button();
            this.BtnPatchExe = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.radioButton3 = new System.Windows.Forms.RadioButton();
            this.groupBox2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtGameInstallPath
            // 
            this.txtGameInstallPath.Enabled = false;
            this.txtGameInstallPath.Location = new System.Drawing.Point(27, 26);
            this.txtGameInstallPath.Name = "txtGameInstallPath";
            this.txtGameInstallPath.Size = new System.Drawing.Size(488, 20);
            this.txtGameInstallPath.TabIndex = 11;
            // 
            // btnChangeGameInstallPath
            // 
            this.btnChangeGameInstallPath.Location = new System.Drawing.Point(521, 24);
            this.btnChangeGameInstallPath.Name = "btnChangeGameInstallPath";
            this.btnChangeGameInstallPath.Size = new System.Drawing.Size(58, 23);
            this.btnChangeGameInstallPath.TabIndex = 12;
            this.btnChangeGameInstallPath.Text = "Change";
            this.btnChangeGameInstallPath.UseVisualStyleBackColor = true;
            this.btnChangeGameInstallPath.Click += new System.EventHandler(this.btnChangeGameInstallPath_Click_1);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.LblDownloadStatus);
            this.groupBox2.Controls.Add(this.button1);
            this.groupBox2.Controls.Add(this.progressBar1);
            this.groupBox2.Location = new System.Drawing.Point(30, 261);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(306, 130);
            this.groupBox2.TabIndex = 14;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Game Update";
            // 
            // LblDownloadStatus
            // 
            this.LblDownloadStatus.AutoSize = true;
            this.LblDownloadStatus.Location = new System.Drawing.Point(10, 111);
            this.LblDownloadStatus.Name = "LblDownloadStatus";
            this.LblDownloadStatus.Size = new System.Drawing.Size(35, 13);
            this.LblDownloadStatus.TabIndex = 8;
            this.LblDownloadStatus.Text = "label4";
            // 
            // button1
            // 
            this.button1.Location = new System.Drawing.Point(10, 78);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(132, 23);
            this.button1.TabIndex = 7;
            this.button1.Text = "Download game update";
            this.button1.UseVisualStyleBackColor = true;
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(148, 78);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(141, 23);
            this.progressBar1.TabIndex = 6;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(27, 59);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(107, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Application files path:";
            // 
            // btnChangePatchPath
            // 
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
            this.txtPatchPath.Location = new System.Drawing.Point(27, 75);
            this.txtPatchPath.Name = "txtPatchPath";
            this.txtPatchPath.Size = new System.Drawing.Size(488, 20);
            this.txtPatchPath.TabIndex = 4;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.button2);
            this.groupBox1.Controls.Add(this.BtnPatchExe);
            this.groupBox1.Location = new System.Drawing.Point(342, 261);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(245, 130);
            this.groupBox1.TabIndex = 13;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Patches";
            // 
            // button2
            // 
            this.button2.Location = new System.Drawing.Point(30, 78);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(185, 23);
            this.button2.TabIndex = 7;
            this.button2.Text = "Restore original files";
            this.button2.UseVisualStyleBackColor = true;
            // 
            // BtnPatchExe
            // 
            this.BtnPatchExe.Location = new System.Drawing.Point(30, 39);
            this.BtnPatchExe.Name = "BtnPatchExe";
            this.BtnPatchExe.Size = new System.Drawing.Size(185, 23);
            this.BtnPatchExe.TabIndex = 6;
            this.BtnPatchExe.Text = "Patch game files";
            this.BtnPatchExe.UseVisualStyleBackColor = true;
            this.BtnPatchExe.Click += new System.EventHandler(this.BtnPatchExe_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
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
            this.groupBox3.Location = new System.Drawing.Point(30, 117);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(289, 100);
            this.groupBox3.TabIndex = 16;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Web server options";
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
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "ucOptions";
            this.Size = new System.Drawing.Size(605, 427);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtGameInstallPath;
        private System.Windows.Forms.Button btnChangeGameInstallPath;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label LblDownloadStatus;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnChangePatchPath;
        private System.Windows.Forms.TextBox txtPatchPath;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button BtnPatchExe;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.RadioButton radioButton3;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.RadioButton radioButton1;
    }
}
