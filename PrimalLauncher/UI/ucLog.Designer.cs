
namespace PrimalLauncher
{
    partial class ucLog
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
            this.LbxServerLog = new System.Windows.Forms.ListBox();
            this.SuspendLayout();
            // 
            // LbxServerLog
            // 
            this.LbxServerLog.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(12)))), ((int)(((byte)(12)))), ((int)(((byte)(31)))));
            this.LbxServerLog.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.LbxServerLog.Font = new System.Drawing.Font("Cascadia Mono SemiBold", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.LbxServerLog.ForeColor = System.Drawing.SystemColors.Menu;
            this.LbxServerLog.FormattingEnabled = true;
            this.LbxServerLog.ItemHeight = 16;
            this.LbxServerLog.Location = new System.Drawing.Point(3, 7);
            this.LbxServerLog.Name = "LbxServerLog";
            this.LbxServerLog.ScrollAlwaysVisible = true;
            this.LbxServerLog.SelectionMode = System.Windows.Forms.SelectionMode.None;
            this.LbxServerLog.Size = new System.Drawing.Size(540, 368);
            this.LbxServerLog.TabIndex = 0;
            // 
            // ucLog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.LbxServerLog);
            this.DoubleBuffered = true;
            this.Name = "ucLog";
            this.Size = new System.Drawing.Size(589, 434);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListBox LbxServerLog;
    }
}
