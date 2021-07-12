namespace PrimalLauncher
{
    partial class Downloader
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Downloader));
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.label1 = new System.Windows.Forms.Label();
            this.btnDownload = new System.Windows.Forms.Button();
            this.btnDownloadCancel = new System.Windows.Forms.Button();
            this.lblDownloadStat = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // progressBar
            // 
            this.progressBar.Location = new System.Drawing.Point(12, 86);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(543, 23);
            this.progressBar.TabIndex = 0;
            this.progressBar.Click += new System.EventHandler(this.progressBar_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(13, 13);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(0, 13);
            this.label1.TabIndex = 1;
            this.label1.Click += new System.EventHandler(this.label1_Click);
            // 
            // btnDownload
            // 
            this.btnDownload.Location = new System.Drawing.Point(11, 121);
            this.btnDownload.Name = "btnDownload";
            this.btnDownload.Size = new System.Drawing.Size(136, 23);
            this.btnDownload.TabIndex = 3;
            this.btnDownload.Text = "Download";
            this.btnDownload.UseVisualStyleBackColor = true;
            this.btnDownload.Click += new System.EventHandler(this.btnDownload_Click);
            // 
            // btnDownloadCancel
            // 
            this.btnDownloadCancel.Location = new System.Drawing.Point(462, 121);
            this.btnDownloadCancel.Name = "btnDownloadCancel";
            this.btnDownloadCancel.Size = new System.Drawing.Size(93, 23);
            this.btnDownloadCancel.TabIndex = 4;
            this.btnDownloadCancel.Text = "Cancel";
            this.btnDownloadCancel.UseVisualStyleBackColor = true;
            this.btnDownloadCancel.Click += new System.EventHandler(this.btnDownloadCancel_Click);
            // 
            // lblDownloadStat
            // 
            this.lblDownloadStat.AutoSize = true;
            this.lblDownloadStat.Location = new System.Drawing.Point(13, 67);
            this.lblDownloadStat.Name = "lblDownloadStat";
            this.lblDownloadStat.Size = new System.Drawing.Size(0, 13);
            this.lblDownloadStat.TabIndex = 5;
            this.lblDownloadStat.Click += new System.EventHandler(this.lblDownloadStat_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(11, 13);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(543, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "Your game installation is outdated. In order to update to the latest version (1.2" +
    "3b), you must download update files ";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(13, 30);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(420, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "totalizing 5.87GB. When download resumes, the update process will start automatic" +
    "ally. ";
            this.label3.Click += new System.EventHandler(this.label3_Click);
            // 
            // Downloader
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(567, 158);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.lblDownloadStat);
            this.Controls.Add(this.btnDownloadCancel);
            this.Controls.Add(this.btnDownload);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.progressBar);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "Downloader";
            this.Text = "Primal Launcher Downloader";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ProgressBar progressBar;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnDownload;
        private System.Windows.Forms.Button btnDownloadCancel;
        private System.Windows.Forms.Label lblDownloadStat;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
    }
}