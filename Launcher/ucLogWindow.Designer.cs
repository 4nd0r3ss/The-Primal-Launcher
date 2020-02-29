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
            this.label1 = new System.Windows.Forms.Label();
            this.lblPCPositionZone = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.lblPCPositionX = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.lblPCPositionY = new System.Windows.Forms.Label();
            this.lblPCPositionZ = new System.Windows.Forms.Label();
            this.lblPCPositionR = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.lblEorzeaTime = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
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
            this.LbxOutput.Location = new System.Drawing.Point(14, 204);
            this.LbxOutput.Name = "LbxOutput";
            this.LbxOutput.Size = new System.Drawing.Size(578, 208);
            this.LbxOutput.TabIndex = 9;
            this.LbxOutput.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.Lbxoutput_DrawItem);
            // 
            // lblEorzeaTimeText
            // 
            this.lblEorzeaTimeText.AutoSize = true;
            this.lblEorzeaTimeText.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblEorzeaTimeText.ForeColor = System.Drawing.SystemColors.ActiveCaptionText;
            this.lblEorzeaTimeText.Location = new System.Drawing.Point(335, 27);
            this.lblEorzeaTimeText.Name = "lblEorzeaTimeText";
            this.lblEorzeaTimeText.Size = new System.Drawing.Size(79, 16);
            this.lblEorzeaTimeText.TabIndex = 10;
            this.lblEorzeaTimeText.Text = "Eorzea time";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.SystemColors.ControlText;
            this.label1.Location = new System.Drawing.Point(13, 27);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(71, 16);
            this.label1.TabIndex = 12;
            this.label1.Text = "Player Info";
            // 
            // lblPCPositionZone
            // 
            this.lblPCPositionZone.AutoSize = true;
            this.lblPCPositionZone.ForeColor = System.Drawing.Color.Black;
            this.lblPCPositionZone.Location = new System.Drawing.Point(65, 11);
            this.lblPCPositionZone.Name = "lblPCPositionZone";
            this.lblPCPositionZone.Size = new System.Drawing.Size(38, 13);
            this.lblPCPositionZone.TabIndex = 13;
            this.lblPCPositionZone.Text = "zoneid";
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.panel1.Controls.Add(this.lblPCPositionR);
            this.panel1.Controls.Add(this.lblPCPositionZ);
            this.panel1.Controls.Add(this.lblPCPositionY);
            this.panel1.Controls.Add(this.label6);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.lblPCPositionX);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.lblPCPositionZone);
            this.panel1.Location = new System.Drawing.Point(15, 47);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(269, 115);
            this.panel1.TabIndex = 14;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.Color.Black;
            this.label2.Location = new System.Drawing.Point(8, 11);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(51, 13);
            this.label2.TabIndex = 14;
            this.label2.Text = "Location:";
            // 
            // lblPCPositionX
            // 
            this.lblPCPositionX.AutoSize = true;
            this.lblPCPositionX.ForeColor = System.Drawing.Color.Black;
            this.lblPCPositionX.Location = new System.Drawing.Point(68, 33);
            this.lblPCPositionX.Name = "lblPCPositionX";
            this.lblPCPositionX.Size = new System.Drawing.Size(34, 13);
            this.lblPCPositionX.TabIndex = 15;
            this.lblPCPositionX.Text = "0.000";
            this.lblPCPositionX.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ForeColor = System.Drawing.Color.Black;
            this.label3.Location = new System.Drawing.Point(8, 33);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(17, 13);
            this.label3.TabIndex = 16;
            this.label3.Text = "X:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.ForeColor = System.Drawing.Color.Black;
            this.label4.Location = new System.Drawing.Point(8, 51);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(17, 13);
            this.label4.TabIndex = 17;
            this.label4.Text = "Y:";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.ForeColor = System.Drawing.Color.Black;
            this.label5.Location = new System.Drawing.Point(8, 70);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(17, 13);
            this.label5.TabIndex = 18;
            this.label5.Text = "Z:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.ForeColor = System.Drawing.Color.Black;
            this.label6.Location = new System.Drawing.Point(7, 88);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(50, 13);
            this.label6.TabIndex = 19;
            this.label6.Text = "Rotation:";
            // 
            // lblPCPositionY
            // 
            this.lblPCPositionY.AutoSize = true;
            this.lblPCPositionY.ForeColor = System.Drawing.Color.Black;
            this.lblPCPositionY.Location = new System.Drawing.Point(68, 51);
            this.lblPCPositionY.Name = "lblPCPositionY";
            this.lblPCPositionY.Size = new System.Drawing.Size(34, 13);
            this.lblPCPositionY.TabIndex = 20;
            this.lblPCPositionY.Text = "0.000";
            this.lblPCPositionY.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblPCPositionZ
            // 
            this.lblPCPositionZ.AutoSize = true;
            this.lblPCPositionZ.ForeColor = System.Drawing.Color.Black;
            this.lblPCPositionZ.Location = new System.Drawing.Point(68, 70);
            this.lblPCPositionZ.Name = "lblPCPositionZ";
            this.lblPCPositionZ.Size = new System.Drawing.Size(34, 13);
            this.lblPCPositionZ.TabIndex = 21;
            this.lblPCPositionZ.Text = "0.000";
            this.lblPCPositionZ.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblPCPositionR
            // 
            this.lblPCPositionR.AutoSize = true;
            this.lblPCPositionR.ForeColor = System.Drawing.Color.Black;
            this.lblPCPositionR.Location = new System.Drawing.Point(68, 88);
            this.lblPCPositionR.Name = "lblPCPositionR";
            this.lblPCPositionR.Size = new System.Drawing.Size(34, 13);
            this.lblPCPositionR.TabIndex = 22;
            this.lblPCPositionR.Text = "0.000";
            this.lblPCPositionR.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.ForeColor = System.Drawing.Color.Black;
            this.label7.Location = new System.Drawing.Point(12, 183);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(89, 16);
            this.label7.TabIndex = 15;
            this.label7.Text = "Server Output";
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.flowLayoutPanel1.Controls.Add(this.lblEorzeaTime);
            this.flowLayoutPanel1.Location = new System.Drawing.Point(338, 47);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Padding = new System.Windows.Forms.Padding(5);
            this.flowLayoutPanel1.Size = new System.Drawing.Size(137, 66);
            this.flowLayoutPanel1.TabIndex = 16;
            this.flowLayoutPanel1.WrapContents = false;
            // 
            // lblEorzeaTime
            // 
            this.lblEorzeaTime.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.lblEorzeaTime.AutoSize = true;
            this.lblEorzeaTime.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblEorzeaTime.ForeColor = System.Drawing.Color.Black;
            this.lblEorzeaTime.Location = new System.Drawing.Point(8, 5);
            this.lblEorzeaTime.Name = "lblEorzeaTime";
            this.lblEorzeaTime.Size = new System.Drawing.Size(62, 16);
            this.lblEorzeaTime.TabIndex = 12;
            this.lblEorzeaTime.Text = "00:00 AM";
            this.lblEorzeaTime.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ucLogWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.Controls.Add(this.flowLayoutPanel1);
            this.Controls.Add(this.label7);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.lblEorzeaTimeText);
            this.Controls.Add(this.LbxOutput);
            this.Name = "ucLogWindow";
            this.Size = new System.Drawing.Size(605, 427);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.flowLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox LbxOutput;
        private System.Windows.Forms.Label lblEorzeaTimeText;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblPCPositionZone;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblPCPositionR;
        private System.Windows.Forms.Label lblPCPositionZ;
        private System.Windows.Forms.Label lblPCPositionY;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblPCPositionX;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Label lblEorzeaTime;
    }
}
