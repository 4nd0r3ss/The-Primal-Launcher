namespace Launcher
{
    partial class MainWindow
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            this.label1 = new System.Windows.Forms.Label();
            this.txtGameInstallPath = new System.Windows.Forms.TextBox();
            this.btnChangeGameInstallPath = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.txtPatchPath = new System.Windows.Forms.TextBox();
            this.btnChangePatchPath = new System.Windows.Forms.Button();
            this.btnSaveConfigChanges = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.button2 = new System.Windows.Forms.Button();
            this.BtnPatchExe = new System.Windows.Forms.Button();
            this.btnLaunchGame = new System.Windows.Forms.Button();
            this.LbxOutput = new System.Windows.Forms.ListBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.LblDownloadStatus = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.label3 = new System.Windows.Forms.Label();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(26, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(114, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Game installation path:";
            // 
            // txtGameInstallPath
            // 
            this.txtGameInstallPath.Enabled = false;
            this.txtGameInstallPath.Location = new System.Drawing.Point(28, 31);
            this.txtGameInstallPath.Name = "txtGameInstallPath";
            this.txtGameInstallPath.Size = new System.Drawing.Size(519, 20);
            this.txtGameInstallPath.TabIndex = 1;
            // 
            // btnChangeGameInstallPath
            // 
            this.btnChangeGameInstallPath.Location = new System.Drawing.Point(570, 29);
            this.btnChangeGameInstallPath.Name = "btnChangeGameInstallPath";
            this.btnChangeGameInstallPath.Size = new System.Drawing.Size(58, 23);
            this.btnChangeGameInstallPath.TabIndex = 2;
            this.btnChangeGameInstallPath.Text = "Change";
            this.btnChangeGameInstallPath.UseVisualStyleBackColor = true;
            this.btnChangeGameInstallPath.Click += new System.EventHandler(this.BtnChangeGameInstallPath_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 27);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(82, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "Download path:";
            // 
            // txtPatchPath
            // 
            this.txtPatchPath.Enabled = false;
            this.txtPatchPath.Location = new System.Drawing.Point(9, 42);
            this.txtPatchPath.Name = "txtPatchPath";
            this.txtPatchPath.Size = new System.Drawing.Size(216, 20);
            this.txtPatchPath.TabIndex = 4;
            // 
            // btnChangePatchPath
            // 
            this.btnChangePatchPath.Location = new System.Drawing.Point(231, 40);
            this.btnChangePatchPath.Name = "btnChangePatchPath";
            this.btnChangePatchPath.Size = new System.Drawing.Size(58, 23);
            this.btnChangePatchPath.TabIndex = 5;
            this.btnChangePatchPath.Text = "Change";
            this.btnChangePatchPath.UseVisualStyleBackColor = true;
            this.btnChangePatchPath.Click += new System.EventHandler(this.BtnChangePatchPath_Click);
            // 
            // btnSaveConfigChanges
            // 
            this.btnSaveConfigChanges.Location = new System.Drawing.Point(541, 415);
            this.btnSaveConfigChanges.Name = "btnSaveConfigChanges";
            this.btnSaveConfigChanges.Size = new System.Drawing.Size(107, 23);
            this.btnSaveConfigChanges.TabIndex = 6;
            this.btnSaveConfigChanges.Text = "Save changes";
            this.btnSaveConfigChanges.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.button2);
            this.groupBox1.Controls.Add(this.BtnPatchExe);
            this.groupBox1.Location = new System.Drawing.Point(403, 63);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(245, 130);
            this.groupBox1.TabIndex = 7;
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
            this.BtnPatchExe.Click += new System.EventHandler(this.Button1_Click);
            // 
            // btnLaunchGame
            // 
            this.btnLaunchGame.Location = new System.Drawing.Point(18, 415);
            this.btnLaunchGame.Name = "btnLaunchGame";
            this.btnLaunchGame.Size = new System.Drawing.Size(163, 23);
            this.btnLaunchGame.TabIndex = 8;
            this.btnLaunchGame.Text = "Launch game";
            this.btnLaunchGame.UseVisualStyleBackColor = true;
            this.btnLaunchGame.Click += new System.EventHandler(this.BtnLaunchGame_Click);
            // 
            // LbxOutput
            // 
            this.LbxOutput.BackColor = System.Drawing.SystemColors.WindowText;
            this.LbxOutput.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.LbxOutput.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.LbxOutput.ForeColor = System.Drawing.SystemColors.Window;
            this.LbxOutput.FormattingEnabled = true;
            this.LbxOutput.Location = new System.Drawing.Point(19, 217);
            this.LbxOutput.Name = "LbxOutput";
            this.LbxOutput.Size = new System.Drawing.Size(629, 182);
            this.LbxOutput.TabIndex = 9;
            this.LbxOutput.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.Lbxoutput_DrawItem);
            this.LbxOutput.SelectedIndexChanged += new System.EventHandler(this.Lbxoutput_SelectedIndexChanged);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.LblDownloadStatus);
            this.groupBox2.Controls.Add(this.button1);
            this.groupBox2.Controls.Add(this.progressBar1);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.btnChangePatchPath);
            this.groupBox2.Controls.Add(this.txtPatchPath);
            this.groupBox2.Location = new System.Drawing.Point(19, 63);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(366, 130);
            this.groupBox2.TabIndex = 10;
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
            this.progressBar1.Size = new System.Drawing.Size(197, 23);
            this.progressBar1.TabIndex = 6;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(18, 202);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(73, 13);
            this.label3.TabIndex = 11;
            this.label3.Text = "Server Output";
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(666, 450);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.LbxOutput);
            this.Controls.Add(this.btnLaunchGame);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.btnSaveConfigChanges);
            this.Controls.Add(this.btnChangeGameInstallPath);
            this.Controls.Add(this.txtGameInstallPath);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainWindow";
            this.Text = "Seventh Astral Launcher";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtGameInstallPath;
        private System.Windows.Forms.Button btnChangeGameInstallPath;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtPatchPath;
        private System.Windows.Forms.Button btnChangePatchPath;
        private System.Windows.Forms.Button btnSaveConfigChanges;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnLaunchGame;
        private System.Windows.Forms.ListBox LbxOutput;
        private System.Windows.Forms.Button BtnPatchExe;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label LblDownloadStatus;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ProgressBar progressBar1;
    }
}

