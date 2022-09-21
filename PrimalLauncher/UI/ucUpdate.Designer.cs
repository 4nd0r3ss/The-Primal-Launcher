
namespace PrimalLauncher
{
    partial class ucUpdate
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
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.btnDownload = new System.Windows.Forms.Button();
            this.btnDownloadCancel = new System.Windows.Forms.Button();
            this.btnGameUpdate = new System.Windows.Forms.Button();
            this.chkKeepFiles = new System.Windows.Forms.CheckBox();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.btnPatch = new System.Windows.Forms.Button();
            this.lblUpdateSeparator1 = new System.Windows.Forms.Label();
            this.lblUpdateSeparator2 = new System.Windows.Forms.Label();
            this.lblUpdateSeparator3 = new System.Windows.Forms.Label();
            this.lblUpdate = new PrimalLauncher.OutlinedFontLabel();
            this.lblDownloadStat = new PrimalLauncher.OutlinedFontLabel();
            this.outlinedFontLabel3 = new PrimalLauncher.OutlinedFontLabel();
            this.lblPatch = new PrimalLauncher.OutlinedFontLabel();
            this.outlinedFontLabel2 = new PrimalLauncher.OutlinedFontLabel();
            this.outlinedFontLabel1 = new PrimalLauncher.OutlinedFontLabel();
            this.outlinedFontLabel8 = new PrimalLauncher.OutlinedFontLabel();
            this.SuspendLayout();
            // 
            // progressBar
            // 
            this.progressBar.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.progressBar.Location = new System.Drawing.Point(20, 74);
            this.progressBar.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(680, 15);
            this.progressBar.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBar.TabIndex = 16;
            // 
            // btnDownload
            // 
            this.btnDownload.BackColor = System.Drawing.Color.Transparent;
            this.btnDownload.BackgroundImage = global::PrimalLauncher.Properties.Resources.button_off;
            this.btnDownload.FlatAppearance.BorderSize = 0;
            this.btnDownload.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btnDownload.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDownload.ForeColor = System.Drawing.SystemColors.Control;
            this.btnDownload.Location = new System.Drawing.Point(20, 98);
            this.btnDownload.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnDownload.Name = "btnDownload";
            this.btnDownload.Size = new System.Drawing.Size(124, 25);
            this.btnDownload.TabIndex = 17;
            this.btnDownload.Text = "Download";
            this.btnDownload.UseVisualStyleBackColor = false;
            this.btnDownload.Click += new System.EventHandler(this.btnDownload_Click);
            this.btnDownload.MouseLeave += new System.EventHandler(this.btnDownload_MouseLeave);
            this.btnDownload.MouseHover += new System.EventHandler(this.btnDownload_MouseHover);
            // 
            // btnDownloadCancel
            // 
            this.btnDownloadCancel.BackgroundImage = global::PrimalLauncher.Properties.Resources.button_off;
            this.btnDownloadCancel.Enabled = false;
            this.btnDownloadCancel.FlatAppearance.BorderSize = 0;
            this.btnDownloadCancel.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btnDownloadCancel.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDownloadCancel.ForeColor = System.Drawing.SystemColors.Control;
            this.btnDownloadCancel.Location = new System.Drawing.Point(576, 98);
            this.btnDownloadCancel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnDownloadCancel.Name = "btnDownloadCancel";
            this.btnDownloadCancel.Size = new System.Drawing.Size(124, 25);
            this.btnDownloadCancel.TabIndex = 18;
            this.btnDownloadCancel.Text = "Stop";
            this.btnDownloadCancel.UseVisualStyleBackColor = false;
            this.btnDownloadCancel.MouseClick += new System.Windows.Forms.MouseEventHandler(this.btnDownloadCancel_MouseClick);
            this.btnDownloadCancel.MouseLeave += new System.EventHandler(this.btnDownloadCancel_MouseLeave);
            this.btnDownloadCancel.MouseHover += new System.EventHandler(this.btnDownloadCancel_MouseHover);
            // 
            // btnGameUpdate
            // 
            this.btnGameUpdate.BackgroundImage = global::PrimalLauncher.Properties.Resources.button_off;
            this.btnGameUpdate.Enabled = false;
            this.btnGameUpdate.FlatAppearance.BorderSize = 0;
            this.btnGameUpdate.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btnGameUpdate.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGameUpdate.ForeColor = System.Drawing.SystemColors.Control;
            this.btnGameUpdate.Location = new System.Drawing.Point(20, 252);
            this.btnGameUpdate.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnGameUpdate.Name = "btnGameUpdate";
            this.btnGameUpdate.Size = new System.Drawing.Size(124, 25);
            this.btnGameUpdate.TabIndex = 23;
            this.btnGameUpdate.Text = "Update game";
            this.btnGameUpdate.UseVisualStyleBackColor = false;
            this.btnGameUpdate.Click += new System.EventHandler(this.btnGameUpdate_Click);
            this.btnGameUpdate.MouseLeave += new System.EventHandler(this.btnGameUpdate_MouseLeave);
            this.btnGameUpdate.MouseHover += new System.EventHandler(this.btnGameUpdate_MouseHover);
            // 
            // chkKeepFiles
            // 
            this.chkKeepFiles.AutoSize = true;
            this.chkKeepFiles.Checked = true;
            this.chkKeepFiles.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkKeepFiles.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.chkKeepFiles.Location = new System.Drawing.Point(440, 260);
            this.chkKeepFiles.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.chkKeepFiles.Name = "chkKeepFiles";
            this.chkKeepFiles.Size = new System.Drawing.Size(15, 14);
            this.chkKeepFiles.TabIndex = 21;
            this.chkKeepFiles.UseVisualStyleBackColor = true;
            this.chkKeepFiles.CheckedChanged += new System.EventHandler(this.chkKeepFiles_CheckedChanged);
            this.chkKeepFiles.Click += new System.EventHandler(this.btnGameUpdate_Click);
            // 
            // progressBar1
            // 
            this.progressBar1.BackColor = System.Drawing.SystemColors.GradientActiveCaption;
            this.progressBar1.Location = new System.Drawing.Point(20, 226);
            this.progressBar1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(680, 15);
            this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBar1.TabIndex = 20;
            // 
            // btnPatch
            // 
            this.btnPatch.BackgroundImage = global::PrimalLauncher.Properties.Resources.button_off;
            this.btnPatch.Enabled = false;
            this.btnPatch.FlatAppearance.BorderSize = 0;
            this.btnPatch.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btnPatch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnPatch.ForeColor = System.Drawing.SystemColors.Control;
            this.btnPatch.Location = new System.Drawing.Point(20, 391);
            this.btnPatch.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnPatch.Name = "btnPatch";
            this.btnPatch.Size = new System.Drawing.Size(124, 25);
            this.btnPatch.TabIndex = 24;
            this.btnPatch.Text = "Patch binaries";
            this.btnPatch.UseVisualStyleBackColor = false;
            this.btnPatch.Click += new System.EventHandler(this.btnPatch_Click);
            this.btnPatch.MouseLeave += new System.EventHandler(this.btnPatch_MouseLeave);
            this.btnPatch.MouseHover += new System.EventHandler(this.btnPatch_MouseHover);
            // 
            // lblUpdateSeparator1
            // 
            this.lblUpdateSeparator1.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.lblUpdateSeparator1.Location = new System.Drawing.Point(0, 44);
            this.lblUpdateSeparator1.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.lblUpdateSeparator1.Name = "lblUpdateSeparator1";
            this.lblUpdateSeparator1.Size = new System.Drawing.Size(724, 2);
            this.lblUpdateSeparator1.TabIndex = 31;
            // 
            // lblUpdateSeparator2
            // 
            this.lblUpdateSeparator2.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.lblUpdateSeparator2.Location = new System.Drawing.Point(0, 192);
            this.lblUpdateSeparator2.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.lblUpdateSeparator2.Name = "lblUpdateSeparator2";
            this.lblUpdateSeparator2.Size = new System.Drawing.Size(724, 2);
            this.lblUpdateSeparator2.TabIndex = 33;
            // 
            // lblUpdateSeparator3
            // 
            this.lblUpdateSeparator3.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.lblUpdateSeparator3.Location = new System.Drawing.Point(0, 344);
            this.lblUpdateSeparator3.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.lblUpdateSeparator3.Name = "lblUpdateSeparator3";
            this.lblUpdateSeparator3.Size = new System.Drawing.Size(724, 2);
            this.lblUpdateSeparator3.TabIndex = 35;
            // 
            // lblUpdate
            // 
            this.lblUpdate.AutoSize = true;
            this.lblUpdate.Location = new System.Drawing.Point(17, 204);
            this.lblUpdate.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblUpdate.Name = "lblUpdate";
            this.lblUpdate.OutlineForeColor = System.Drawing.Color.Black;
            this.lblUpdate.OutlineWidth = 2F;
            this.lblUpdate.Size = new System.Drawing.Size(159, 15);
            this.lblUpdate.TabIndex = 40;
            this.lblUpdate.Text = "The game is not updated.";
            // 
            // lblDownloadStat
            // 
            this.lblDownloadStat.AutoSize = true;
            this.lblDownloadStat.Location = new System.Drawing.Point(17, 52);
            this.lblDownloadStat.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblDownloadStat.Name = "lblDownloadStat";
            this.lblDownloadStat.OutlineForeColor = System.Drawing.Color.Black;
            this.lblDownloadStat.OutlineWidth = 2F;
            this.lblDownloadStat.Size = new System.Drawing.Size(215, 15);
            this.lblDownloadStat.TabIndex = 39;
            this.lblDownloadStat.Text = "Click the download button to start.";
            // 
            // outlinedFontLabel3
            // 
            this.outlinedFontLabel3.AutoSize = true;
            this.outlinedFontLabel3.Location = new System.Drawing.Point(460, 258);
            this.outlinedFontLabel3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.outlinedFontLabel3.Name = "outlinedFontLabel3";
            this.outlinedFontLabel3.OutlineForeColor = System.Drawing.Color.Black;
            this.outlinedFontLabel3.OutlineWidth = 2F;
            this.outlinedFontLabel3.Size = new System.Drawing.Size(248, 15);
            this.outlinedFontLabel3.TabIndex = 38;
            this.outlinedFontLabel3.Text = "Keep files  after update (recommended)";
            this.outlinedFontLabel3.TextAlign = System.Drawing.ContentAlignment.TopRight;
            this.outlinedFontLabel3.Click += new System.EventHandler(this.btnGameUpdate_Click);
            // 
            // lblPatch
            // 
            this.lblPatch.AutoSize = true;
            this.lblPatch.Location = new System.Drawing.Point(17, 361);
            this.lblPatch.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblPatch.Name = "lblPatch";
            this.lblPatch.OutlineForeColor = System.Drawing.Color.Black;
            this.lblPatch.OutlineWidth = 2F;
            this.lblPatch.Size = new System.Drawing.Size(526, 15);
            this.lblPatch.TabIndex = 37;
            this.lblPatch.Text = "The patching process will only be available once the game is updated to version 1" +
    ".23b.";
            // 
            // outlinedFontLabel2
            // 
            this.outlinedFontLabel2.AutoSize = true;
            this.outlinedFontLabel2.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.outlinedFontLabel2.ForeColor = System.Drawing.Color.Moccasin;
            this.outlinedFontLabel2.Location = new System.Drawing.Point(0, 318);
            this.outlinedFontLabel2.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.outlinedFontLabel2.Name = "outlinedFontLabel2";
            this.outlinedFontLabel2.OutlineForeColor = System.Drawing.Color.Black;
            this.outlinedFontLabel2.OutlineWidth = 2F;
            this.outlinedFontLabel2.Size = new System.Drawing.Size(128, 17);
            this.outlinedFontLabel2.TabIndex = 36;
            this.outlinedFontLabel2.Text = "Patch binary files";
            // 
            // outlinedFontLabel1
            // 
            this.outlinedFontLabel1.AutoSize = true;
            this.outlinedFontLabel1.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.outlinedFontLabel1.ForeColor = System.Drawing.Color.Moccasin;
            this.outlinedFontLabel1.Location = new System.Drawing.Point(0, 167);
            this.outlinedFontLabel1.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.outlinedFontLabel1.Name = "outlinedFontLabel1";
            this.outlinedFontLabel1.OutlineForeColor = System.Drawing.Color.Black;
            this.outlinedFontLabel1.OutlineWidth = 2F;
            this.outlinedFontLabel1.Size = new System.Drawing.Size(156, 17);
            this.outlinedFontLabel1.TabIndex = 34;
            this.outlinedFontLabel1.Text = "Update game to 1.23b";
            // 
            // outlinedFontLabel8
            // 
            this.outlinedFontLabel8.AutoSize = true;
            this.outlinedFontLabel8.Font = new System.Drawing.Font("Cambria", 11.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.outlinedFontLabel8.ForeColor = System.Drawing.Color.Moccasin;
            this.outlinedFontLabel8.Location = new System.Drawing.Point(0, 18);
            this.outlinedFontLabel8.Margin = new System.Windows.Forms.Padding(5, 0, 5, 0);
            this.outlinedFontLabel8.Name = "outlinedFontLabel8";
            this.outlinedFontLabel8.OutlineForeColor = System.Drawing.Color.Black;
            this.outlinedFontLabel8.OutlineWidth = 2F;
            this.outlinedFontLabel8.Size = new System.Drawing.Size(164, 17);
            this.outlinedFontLabel8.TabIndex = 32;
            this.outlinedFontLabel8.Text = "Download update files";
            // 
            // ucUpdate
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.lblUpdate);
            this.Controls.Add(this.lblDownloadStat);
            this.Controls.Add(this.outlinedFontLabel3);
            this.Controls.Add(this.lblPatch);
            this.Controls.Add(this.outlinedFontLabel2);
            this.Controls.Add(this.lblUpdateSeparator3);
            this.Controls.Add(this.outlinedFontLabel1);
            this.Controls.Add(this.lblUpdateSeparator2);
            this.Controls.Add(this.outlinedFontLabel8);
            this.Controls.Add(this.lblUpdateSeparator1);
            this.Controls.Add(this.btnPatch);
            this.Controls.Add(this.btnGameUpdate);
            this.Controls.Add(this.chkKeepFiles);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.btnDownload);
            this.Controls.Add(this.btnDownloadCancel);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Cambria", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.SystemColors.Control;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "ucUpdate";
            this.Size = new System.Drawing.Size(729, 431);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Button btnDownload;
        private System.Windows.Forms.Button btnDownloadCancel;
        private System.Windows.Forms.Button btnGameUpdate;
        private System.Windows.Forms.CheckBox chkKeepFiles;
        private System.Windows.Forms.ProgressBar progressBar1;
        private System.Windows.Forms.Button btnPatch;
        private OutlinedFontLabel outlinedFontLabel8;
        private System.Windows.Forms.Label lblUpdateSeparator1;
        private OutlinedFontLabel outlinedFontLabel1;
        private System.Windows.Forms.Label lblUpdateSeparator2;
        private OutlinedFontLabel outlinedFontLabel2;
        private System.Windows.Forms.Label lblUpdateSeparator3;
        private OutlinedFontLabel lblPatch;
        private OutlinedFontLabel outlinedFontLabel3;
        private OutlinedFontLabel lblDownloadStat;
        private OutlinedFontLabel lblUpdate;
    }
}
