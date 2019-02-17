namespace Launcher
{
    partial class ucLogWindow
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
            this.LbxOutput = new System.Windows.Forms.ListBox();
            this.lblEorzeaTimeText = new System.Windows.Forms.Label();
            this.lblEorzeaTime = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // LbxOutput
            // 
            this.LbxOutput.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.LbxOutput.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.LbxOutput.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.LbxOutput.ForeColor = System.Drawing.SystemColors.Window;
            this.LbxOutput.FormattingEnabled = true;
            this.LbxOutput.HorizontalScrollbar = true;
            this.LbxOutput.Location = new System.Drawing.Point(14, 35);
            this.LbxOutput.Name = "LbxOutput";
            this.LbxOutput.Size = new System.Drawing.Size(578, 377);
            this.LbxOutput.TabIndex = 9;
            this.LbxOutput.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.Lbxoutput_DrawItem);
            // 
            // lblEorzeaTimeText
            // 
            this.lblEorzeaTimeText.AutoSize = true;
            this.lblEorzeaTimeText.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.lblEorzeaTimeText.Location = new System.Drawing.Point(14, 10);
            this.lblEorzeaTimeText.Name = "lblEorzeaTimeText";
            this.lblEorzeaTimeText.Size = new System.Drawing.Size(68, 13);
            this.lblEorzeaTimeText.TabIndex = 10;
            this.lblEorzeaTimeText.Text = "Eorzea time: ";
            // 
            // lblEorzeaTime
            // 
            this.lblEorzeaTime.AutoSize = true;
            this.lblEorzeaTime.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.lblEorzeaTime.Location = new System.Drawing.Point(75, 10);
            this.lblEorzeaTime.Name = "lblEorzeaTime";
            this.lblEorzeaTime.Size = new System.Drawing.Size(53, 13);
            this.lblEorzeaTime.TabIndex = 11;
            this.lblEorzeaTime.Text = "00:00 AM";
            // 
            // ucLogWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lblEorzeaTime);
            this.Controls.Add(this.lblEorzeaTimeText);
            this.Controls.Add(this.LbxOutput);
            this.Name = "ucLogWindow";
            this.Size = new System.Drawing.Size(605, 427);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox LbxOutput;
        private System.Windows.Forms.Label lblEorzeaTimeText;
        private System.Windows.Forms.Label lblEorzeaTime;
    }
}
