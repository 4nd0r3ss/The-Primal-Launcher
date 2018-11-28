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
            this.btnWebsite = new System.Windows.Forms.Panel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.label4 = new System.Windows.Forms.Label();
            this.button3 = new System.Windows.Forms.Button();
            this.btnAbout = new System.Windows.Forms.Button();
            this.btnHelp = new System.Windows.Forms.Button();
            this.btnMisc = new System.Windows.Forms.Button();
            this.btnUsers = new System.Windows.Forms.Button();
            this.btnOptions = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.btnMinimize = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.btnWebsite.SuspendLayout();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.panel2.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel3.SuspendLayout();
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
            this.txtGameInstallPath.Location = new System.Drawing.Point(12, 16);
            this.txtGameInstallPath.Name = "txtGameInstallPath";
            this.txtGameInstallPath.Size = new System.Drawing.Size(504, 20);
            this.txtGameInstallPath.TabIndex = 1;
            // 
            // btnChangeGameInstallPath
            // 
            this.btnChangeGameInstallPath.Location = new System.Drawing.Point(536, 14);
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
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.button2);
            this.groupBox1.Controls.Add(this.BtnPatchExe);
            this.groupBox1.Location = new System.Drawing.Point(349, 56);
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
            this.btnLaunchGame.BackColor = System.Drawing.Color.Maroon;
            this.btnLaunchGame.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnLaunchGame.FlatAppearance.BorderSize = 0;
            this.btnLaunchGame.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLaunchGame.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLaunchGame.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(192)))), ((int)(((byte)(0)))));
            this.btnLaunchGame.Location = new System.Drawing.Point(0, 427);
            this.btnLaunchGame.Name = "btnLaunchGame";
            this.btnLaunchGame.Size = new System.Drawing.Size(165, 39);
            this.btnLaunchGame.TabIndex = 8;
            this.btnLaunchGame.Text = "Launch game";
            this.btnLaunchGame.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnLaunchGame.UseVisualStyleBackColor = false;
            this.btnLaunchGame.Click += new System.EventHandler(this.BtnLaunchGame_Click);
            // 
            // LbxOutput
            // 
            this.LbxOutput.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.LbxOutput.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.LbxOutput.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.LbxOutput.ForeColor = System.Drawing.SystemColors.Window;
            this.LbxOutput.FormattingEnabled = true;
            this.LbxOutput.Location = new System.Drawing.Point(12, 1);
            this.LbxOutput.Name = "LbxOutput";
            this.LbxOutput.Size = new System.Drawing.Size(593, 234);
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
            this.groupBox2.Location = new System.Drawing.Point(21, 56);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(306, 130);
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
            this.progressBar1.Size = new System.Drawing.Size(141, 23);
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
            // btnWebsite
            // 
            this.btnWebsite.BackColor = System.Drawing.Color.Maroon;
            this.btnWebsite.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnWebsite.Controls.Add(this.panel1);
            this.btnWebsite.Controls.Add(this.button3);
            this.btnWebsite.Controls.Add(this.btnAbout);
            this.btnWebsite.Controls.Add(this.btnHelp);
            this.btnWebsite.Controls.Add(this.btnMisc);
            this.btnWebsite.Controls.Add(this.btnUsers);
            this.btnWebsite.Controls.Add(this.btnOptions);
            this.btnWebsite.Controls.Add(this.btnLaunchGame);
            this.btnWebsite.Dock = System.Windows.Forms.DockStyle.Left;
            this.btnWebsite.Location = new System.Drawing.Point(0, 0);
            this.btnWebsite.Name = "btnWebsite";
            this.btnWebsite.Size = new System.Drawing.Size(165, 466);
            this.btnWebsite.TabIndex = 12;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.Maroon;
            this.panel1.Controls.Add(this.pictureBox1);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Location = new System.Drawing.Point(-1, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(166, 128);
            this.panel1.TabIndex = 14;
            // 
            // pictureBox1
            // 
            this.pictureBox1.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("pictureBox1.BackgroundImage")));
            this.pictureBox1.Location = new System.Drawing.Point(10, 3);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(155, 102);
            this.pictureBox1.TabIndex = 14;
            this.pictureBox1.TabStop = false;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Bahnschrift Condensed", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.label4.Location = new System.Drawing.Point(10, 108);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(151, 18);
            this.label4.TabIndex = 0;
            this.label4.Text = "FFXIV1.0 Primal Launcher v0.1";
            this.label4.Click += new System.EventHandler(this.label4_Click);
            // 
            // button3
            // 
            this.button3.BackColor = System.Drawing.Color.DarkRed;
            this.button3.FlatAppearance.BorderSize = 0;
            this.button3.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button3.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.button3.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.button3.Location = new System.Drawing.Point(0, 331);
            this.button3.Name = "button3";
            this.button3.Size = new System.Drawing.Size(165, 47);
            this.button3.TabIndex = 14;
            this.button3.Text = "Website";
            this.button3.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.button3.UseVisualStyleBackColor = false;
            // 
            // btnAbout
            // 
            this.btnAbout.BackColor = System.Drawing.Color.DarkRed;
            this.btnAbout.FlatAppearance.BorderSize = 0;
            this.btnAbout.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnAbout.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAbout.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.btnAbout.Location = new System.Drawing.Point(0, 379);
            this.btnAbout.Name = "btnAbout";
            this.btnAbout.Size = new System.Drawing.Size(165, 47);
            this.btnAbout.TabIndex = 13;
            this.btnAbout.Text = "About";
            this.btnAbout.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnAbout.UseVisualStyleBackColor = false;
            // 
            // btnHelp
            // 
            this.btnHelp.BackColor = System.Drawing.Color.DarkRed;
            this.btnHelp.FlatAppearance.BorderSize = 0;
            this.btnHelp.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnHelp.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnHelp.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.btnHelp.Location = new System.Drawing.Point(0, 283);
            this.btnHelp.Name = "btnHelp";
            this.btnHelp.Size = new System.Drawing.Size(165, 47);
            this.btnHelp.TabIndex = 12;
            this.btnHelp.Text = "Help";
            this.btnHelp.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnHelp.UseVisualStyleBackColor = false;
            // 
            // btnMisc
            // 
            this.btnMisc.BackColor = System.Drawing.Color.DarkRed;
            this.btnMisc.FlatAppearance.BorderSize = 0;
            this.btnMisc.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMisc.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnMisc.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.btnMisc.Location = new System.Drawing.Point(0, 235);
            this.btnMisc.Name = "btnMisc";
            this.btnMisc.Size = new System.Drawing.Size(165, 47);
            this.btnMisc.TabIndex = 11;
            this.btnMisc.Text = "Other";
            this.btnMisc.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnMisc.UseVisualStyleBackColor = false;
            // 
            // btnUsers
            // 
            this.btnUsers.BackColor = System.Drawing.Color.DarkRed;
            this.btnUsers.FlatAppearance.BorderSize = 0;
            this.btnUsers.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnUsers.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnUsers.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.btnUsers.Location = new System.Drawing.Point(0, 185);
            this.btnUsers.Name = "btnUsers";
            this.btnUsers.Size = new System.Drawing.Size(165, 49);
            this.btnUsers.TabIndex = 10;
            this.btnUsers.Text = "Players";
            this.btnUsers.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnUsers.UseVisualStyleBackColor = false;
            // 
            // btnOptions
            // 
            this.btnOptions.BackColor = System.Drawing.Color.DarkRed;
            this.btnOptions.FlatAppearance.BorderSize = 0;
            this.btnOptions.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnOptions.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnOptions.ForeColor = System.Drawing.SystemColors.ButtonFace;
            this.btnOptions.Location = new System.Drawing.Point(0, 137);
            this.btnOptions.Name = "btnOptions";
            this.btnOptions.Size = new System.Drawing.Size(165, 47);
            this.btnOptions.TabIndex = 9;
            this.btnOptions.Text = "Options";
            this.btnOptions.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.btnOptions.UseVisualStyleBackColor = false;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.White;
            this.panel2.Controls.Add(this.panel4);
            this.panel2.Controls.Add(this.txtGameInstallPath);
            this.panel2.Controls.Add(this.btnChangeGameInstallPath);
            this.panel2.Controls.Add(this.groupBox2);
            this.panel2.Controls.Add(this.groupBox1);
            this.panel2.Location = new System.Drawing.Point(165, 38);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(605, 427);
            this.panel2.TabIndex = 13;
            // 
            // panel4
            // 
            this.panel4.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.panel4.Controls.Add(this.LbxOutput);
            this.panel4.Location = new System.Drawing.Point(0, 192);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(605, 236);
            this.panel4.TabIndex = 12;
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.White;
            this.panel3.Controls.Add(this.btnMinimize);
            this.panel3.Controls.Add(this.btnClose);
            this.panel3.Location = new System.Drawing.Point(165, 1);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(605, 40);
            this.panel3.TabIndex = 14;
            this.panel3.Paint += new System.Windows.Forms.PaintEventHandler(this.panel3_Paint);
            this.panel3.MouseDown += new System.Windows.Forms.MouseEventHandler(this.TopFrame_MouseDown);
            // 
            // btnMinimize
            // 
            this.btnMinimize.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnMinimize.FlatAppearance.BorderSize = 0;
            this.btnMinimize.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White;
            this.btnMinimize.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
            this.btnMinimize.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnMinimize.Font = new System.Drawing.Font("Segoe UI", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnMinimize.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.btnMinimize.Location = new System.Drawing.Point(546, -11);
            this.btnMinimize.Name = "btnMinimize";
            this.btnMinimize.Size = new System.Drawing.Size(34, 37);
            this.btnMinimize.TabIndex = 1;
            this.btnMinimize.Text = "_";
            this.btnMinimize.TextAlign = System.Drawing.ContentAlignment.TopCenter;
            this.btnMinimize.UseVisualStyleBackColor = true;
            this.btnMinimize.Click += new System.EventHandler(this.btnMinimize_Click);
            // 
            // btnClose
            // 
            this.btnClose.BackgroundImageLayout = System.Windows.Forms.ImageLayout.None;
            this.btnClose.FlatAppearance.BorderSize = 0;
            this.btnClose.FlatAppearance.MouseDownBackColor = System.Drawing.Color.White;
            this.btnClose.FlatAppearance.MouseOverBackColor = System.Drawing.Color.White;
            this.btnClose.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnClose.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnClose.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.btnClose.Location = new System.Drawing.Point(571, -2);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(39, 31);
            this.btnClose.TabIndex = 0;
            this.btnClose.Text = "X";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // MainWindow
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.Maroon;
            this.ClientSize = new System.Drawing.Size(771, 466);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.btnWebsite);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label1);
            this.ForeColor = System.Drawing.Color.Maroon;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainWindow";
            this.Text = "Seventh Astral Launcher";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.btnWebsite.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
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
        private System.Windows.Forms.Panel btnWebsite;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btnMisc;
        private System.Windows.Forms.Button btnUsers;
        private System.Windows.Forms.Button btnOptions;
        private System.Windows.Forms.Button button3;
        private System.Windows.Forms.Button btnAbout;
        private System.Windows.Forms.Button btnHelp;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Button btnMinimize;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.Panel panel4;
    }
}

