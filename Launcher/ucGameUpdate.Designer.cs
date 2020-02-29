namespace Launcher
{
    partial class ucGameUpdate
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
            this.lblDownloadStat = new System.Windows.Forms.Label();
            this.btnDownloadCancel = new System.Windows.Forms.Button();
            this.btnDownload = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.btnPatch = new System.Windows.Forms.Button();
            this.chkKeepFiles = new System.Windows.Forms.CheckBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.btnGameUpdate = new System.Windows.Forms.Button();
            this.lblUpdate = new System.Windows.Forms.Label();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblDownloadStat
            // 
            this.lblDownloadStat.AutoSize = true;
            this.lblDownloadStat.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.lblDownloadStat.Location = new System.Drawing.Point(16, 28);
            this.lblDownloadStat.Name = "lblDownloadStat";
            this.lblDownloadStat.Size = new System.Drawing.Size(270, 13);
            this.lblDownloadStat.TabIndex = 9;
            this.lblDownloadStat.Text = "Click the download button to start downloading the files.";
            // 
            // btnDownloadCancel
            // 
            this.btnDownloadCancel.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnDownloadCancel.Location = new System.Drawing.Point(433, 82);
            this.btnDownloadCancel.Name = "btnDownloadCancel";
            this.btnDownloadCancel.Size = new System.Drawing.Size(93, 23);
            this.btnDownloadCancel.TabIndex = 8;
            this.btnDownloadCancel.Text = "Stop";
            this.btnDownloadCancel.UseVisualStyleBackColor = true;
            this.btnDownloadCancel.Click += new System.EventHandler(this.btnDownloadCancel_Click);
            // 
            // btnDownload
            // 
            this.btnDownload.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnDownload.Location = new System.Drawing.Point(16, 82);
            this.btnDownload.Name = "btnDownload";
            this.btnDownload.Size = new System.Drawing.Size(136, 23);
            this.btnDownload.TabIndex = 7;
            this.btnDownload.Text = "Download";
            this.btnDownload.UseVisualStyleBackColor = true;
            this.btnDownload.Click += new System.EventHandler(this.btnDownload_Click);
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(16, 49);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(510, 23);
            this.progressBar.TabIndex = 6;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.progressBar);
            this.groupBox1.Controls.Add(this.lblDownloadStat);
            this.groupBox1.Controls.Add(this.btnDownload);
            this.groupBox1.Controls.Add(this.btnDownloadCancel);
            this.groupBox1.Location = new System.Drawing.Point(30, 15);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(544, 121);
            this.groupBox1.TabIndex = 10;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Update files";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Controls.Add(this.label1);
            this.groupBox2.Controls.Add(this.btnPatch);
            this.groupBox2.Location = new System.Drawing.Point(30, 267);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(544, 141);
            this.groupBox2.TabIndex = 11;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Binaries patch";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(17, 84);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(359, 13);
            this.label4.TabIndex = 5;
            this.label4.Text = "where the game is installed. The game will work only after this setp is done.";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(16, 67);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(510, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Primal Launcher can comunicate with the game. It may ask you for administrator pe" +
    "rmissions depending on ";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 50);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(508, 13);
            this.label2.TabIndex = 3;
            this.label2.Text = "This will replace the original servers IP addresses in the game binaries with you" +
    "r computer\'s localhost IP so ";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.label1.Location = new System.Drawing.Point(16, 27);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(417, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "The patching process will only be available once the game is updated to version 1" +
    ".23b.";
            // 
            // btnPatch
            // 
            this.btnPatch.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnPatch.Location = new System.Drawing.Point(16, 107);
            this.btnPatch.Name = "btnPatch";
            this.btnPatch.Size = new System.Drawing.Size(136, 23);
            this.btnPatch.TabIndex = 0;
            this.btnPatch.Text = "Patch game binaries";
            this.btnPatch.UseVisualStyleBackColor = true;
            this.btnPatch.Click += new System.EventHandler(this.btnPatch_Click);
            // 
            // chkKeepFiles
            // 
            this.chkKeepFiles.AutoSize = true;
            this.chkKeepFiles.Checked = true;
            this.chkKeepFiles.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkKeepFiles.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.chkKeepFiles.Location = new System.Drawing.Point(337, 85);
            this.chkKeepFiles.Name = "chkKeepFiles";
            this.chkKeepFiles.Size = new System.Drawing.Size(192, 17);
            this.chkKeepFiles.TabIndex = 1;
            this.chkKeepFiles.Text = "Keep update files  (Recommended)";
            this.chkKeepFiles.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.btnGameUpdate);
            this.groupBox3.Controls.Add(this.chkKeepFiles);
            this.groupBox3.Controls.Add(this.lblUpdate);
            this.groupBox3.Controls.Add(this.progressBar1);
            this.groupBox3.Location = new System.Drawing.Point(30, 142);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(544, 119);
            this.groupBox3.TabIndex = 12;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Game update";
            // 
            // btnGameUpdate
            // 
            this.btnGameUpdate.Enabled = false;
            this.btnGameUpdate.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.btnGameUpdate.Location = new System.Drawing.Point(16, 78);
            this.btnGameUpdate.Name = "btnGameUpdate";
            this.btnGameUpdate.Size = new System.Drawing.Size(136, 23);
            this.btnGameUpdate.TabIndex = 2;
            this.btnGameUpdate.Text = "Start game update";
            this.btnGameUpdate.UseVisualStyleBackColor = true;
            this.btnGameUpdate.Click += new System.EventHandler(this.btnGameUpdate_Click);
            // 
            // lblUpdate
            // 
            this.lblUpdate.AutoSize = true;
            this.lblUpdate.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.lblUpdate.Location = new System.Drawing.Point(19, 28);
            this.lblUpdate.Name = "lblUpdate";
            this.lblUpdate.Size = new System.Drawing.Size(0, 13);
            this.lblUpdate.TabIndex = 1;
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(16, 44);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(510, 23);
            this.progressBar1.TabIndex = 0;
            // 
            // ucGameUpdate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "ucGameUpdate";
            this.Size = new System.Drawing.Size(605, 427);
            this.Load += new System.EventHandler(this.ucGameUpdate_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label lblDownloadStat;
        private System.Windows.Forms.Button btnDownloadCancel;
        private System.Windows.Forms.Button btnDownload;
        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox chkKeepFiles;
        private System.Windows.Forms.Button btnPatch;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button btnGameUpdate;
        private System.Windows.Forms.Label lblUpdate;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
    }
}
