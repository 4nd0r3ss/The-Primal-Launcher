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
            this.materialTabSelector1 = new MaterialSkin.Controls.MaterialTabSelector();
            this.materialTabControl1 = new MaterialSkin.Controls.MaterialTabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.materialCard2 = new MaterialSkin.Controls.MaterialCard();
            this.materialCard1 = new MaterialSkin.Controls.MaterialCard();
            this.LbxOutput = new System.Windows.Forms.ListBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tabPage3 = new System.Windows.Forms.TabPage();
            this.tabPage4 = new System.Windows.Forms.TabPage();
            this.tabPage5 = new System.Windows.Forms.TabPage();
            this.tabPage6 = new System.Windows.Forms.TabPage();
            this.btnLaunchGame = new MaterialSkin.Controls.MaterialButton();
            this.materialLabel1 = new MaterialSkin.Controls.MaterialLabel();
            this.lblEorzeaTime = new MaterialSkin.Controls.MaterialLabel();
            this.LbxOutput3 = new System.Windows.Forms.ListBox();
            this.lblPCPositionZone = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lblPCPositionX = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.lblPCPositionY = new System.Windows.Forms.Label();
            this.lblPCPositionZ = new System.Windows.Forms.Label();
            this.lblPCPositionR = new System.Windows.Forms.Label();
            this.materialLabel2 = new MaterialSkin.Controls.MaterialLabel();
            this.materialDivider1 = new MaterialSkin.Controls.MaterialDivider();
            this.materialTabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.materialCard1.SuspendLayout();
            this.SuspendLayout();
            // 
            // materialTabSelector1
            // 
            this.materialTabSelector1.BaseTabControl = this.materialTabControl1;
            this.materialTabSelector1.Depth = 0;
            this.materialTabSelector1.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.materialTabSelector1.Location = new System.Drawing.Point(-51, 64);
            this.materialTabSelector1.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialTabSelector1.Name = "materialTabSelector1";
            this.materialTabSelector1.Size = new System.Drawing.Size(875, 48);
            this.materialTabSelector1.TabIndex = 16;
            this.materialTabSelector1.Text = "materialTabSelector1";
            // 
            // materialTabControl1
            // 
            this.materialTabControl1.Controls.Add(this.tabPage1);
            this.materialTabControl1.Controls.Add(this.tabPage2);
            this.materialTabControl1.Controls.Add(this.tabPage3);
            this.materialTabControl1.Controls.Add(this.tabPage4);
            this.materialTabControl1.Controls.Add(this.tabPage5);
            this.materialTabControl1.Controls.Add(this.tabPage6);
            this.materialTabControl1.Depth = 0;
            this.materialTabControl1.Location = new System.Drawing.Point(3, 118);
            this.materialTabControl1.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialTabControl1.Multiline = true;
            this.materialTabControl1.Name = "materialTabControl1";
            this.materialTabControl1.SelectedIndex = 0;
            this.materialTabControl1.Size = new System.Drawing.Size(821, 428);
            this.materialTabControl1.TabIndex = 17;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.materialCard2);
            this.tabPage1.Controls.Add(this.materialCard1);
            this.tabPage1.ForeColor = System.Drawing.Color.Black;
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(813, 402);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Log";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // materialCard2
            // 
            this.materialCard2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.materialCard2.Depth = 0;
            this.materialCard2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.materialCard2.Location = new System.Drawing.Point(17, 17);
            this.materialCard2.Margin = new System.Windows.Forms.Padding(14);
            this.materialCard2.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialCard2.Name = "materialCard2";
            this.materialCard2.Padding = new System.Windows.Forms.Padding(14);
            this.materialCard2.Size = new System.Drawing.Size(222, 117);
            this.materialCard2.TabIndex = 26;
            // 
            // materialCard1
            // 
            this.materialCard1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.materialCard1.Controls.Add(this.LbxOutput);
            this.materialCard1.Depth = 0;
            this.materialCard1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(222)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.materialCard1.Location = new System.Drawing.Point(17, 151);
            this.materialCard1.Margin = new System.Windows.Forms.Padding(14);
            this.materialCard1.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialCard1.Name = "materialCard1";
            this.materialCard1.Padding = new System.Windows.Forms.Padding(14);
            this.materialCard1.Size = new System.Drawing.Size(783, 251);
            this.materialCard1.TabIndex = 25;
            // 
            // LbxOutput
            // 
            this.LbxOutput.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.LbxOutput.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.LbxOutput.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.LbxOutput.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.LbxOutput.FormattingEnabled = true;
            this.LbxOutput.HorizontalScrollbar = true;
            this.LbxOutput.Location = new System.Drawing.Point(17, 8);
            this.LbxOutput.Name = "LbxOutput";
            this.LbxOutput.Size = new System.Drawing.Size(749, 234);
            this.LbxOutput.TabIndex = 24;
            this.LbxOutput.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.Lbxoutput_DrawItem);
            // 
            // tabPage2
            // 
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(813, 402);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Options";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // tabPage3
            // 
            this.tabPage3.Location = new System.Drawing.Point(4, 22);
            this.tabPage3.Name = "tabPage3";
            this.tabPage3.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage3.Size = new System.Drawing.Size(813, 402);
            this.tabPage3.TabIndex = 2;
            this.tabPage3.Text = "Game Update";
            this.tabPage3.UseVisualStyleBackColor = true;
            // 
            // tabPage4
            // 
            this.tabPage4.Location = new System.Drawing.Point(4, 22);
            this.tabPage4.Name = "tabPage4";
            this.tabPage4.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage4.Size = new System.Drawing.Size(813, 402);
            this.tabPage4.TabIndex = 3;
            this.tabPage4.Text = "Help";
            this.tabPage4.UseVisualStyleBackColor = true;
            // 
            // tabPage5
            // 
            this.tabPage5.Location = new System.Drawing.Point(4, 22);
            this.tabPage5.Name = "tabPage5";
            this.tabPage5.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage5.Size = new System.Drawing.Size(813, 402);
            this.tabPage5.TabIndex = 4;
            this.tabPage5.Text = "Website";
            this.tabPage5.UseVisualStyleBackColor = true;
            // 
            // tabPage6
            // 
            this.tabPage6.Location = new System.Drawing.Point(4, 22);
            this.tabPage6.Name = "tabPage6";
            this.tabPage6.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage6.Size = new System.Drawing.Size(813, 402);
            this.tabPage6.TabIndex = 5;
            this.tabPage6.Text = "About";
            this.tabPage6.UseVisualStyleBackColor = true;
            // 
            // btnLaunchGame
            // 
            this.btnLaunchGame.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.btnLaunchGame.Depth = 0;
            this.btnLaunchGame.DrawShadows = true;
            this.btnLaunchGame.HighEmphasis = true;
            this.btnLaunchGame.Icon = null;
            this.btnLaunchGame.Location = new System.Drawing.Point(681, 551);
            this.btnLaunchGame.Margin = new System.Windows.Forms.Padding(4, 6, 4, 6);
            this.btnLaunchGame.MouseState = MaterialSkin.MouseState.HOVER;
            this.btnLaunchGame.Name = "btnLaunchGame";
            this.btnLaunchGame.Size = new System.Drawing.Size(124, 36);
            this.btnLaunchGame.TabIndex = 18;
            this.btnLaunchGame.Text = "Launch Game";
            this.btnLaunchGame.Type = MaterialSkin.Controls.MaterialButton.MaterialButtonType.Contained;
            this.btnLaunchGame.UseAccentColor = false;
            this.btnLaunchGame.UseVisualStyleBackColor = true;
            this.btnLaunchGame.Click += new System.EventHandler(this.BtnLaunchGame_Click);
            // 
            // materialLabel1
            // 
            this.materialLabel1.AutoSize = true;
            this.materialLabel1.Depth = 0;
            this.materialLabel1.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.materialLabel1.Location = new System.Drawing.Point(26, 559);
            this.materialLabel1.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialLabel1.Name = "materialLabel1";
            this.materialLabel1.Size = new System.Drawing.Size(92, 19);
            this.materialLabel1.TabIndex = 19;
            this.materialLabel1.Text = "Eorzea time: ";
            // 
            // lblEorzeaTime
            // 
            this.lblEorzeaTime.AutoSize = true;
            this.lblEorzeaTime.Depth = 0;
            this.lblEorzeaTime.Font = new System.Drawing.Font("Roboto", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.lblEorzeaTime.Location = new System.Drawing.Point(124, 559);
            this.lblEorzeaTime.MouseState = MaterialSkin.MouseState.HOVER;
            this.lblEorzeaTime.Name = "lblEorzeaTime";
            this.lblEorzeaTime.Size = new System.Drawing.Size(69, 19);
            this.lblEorzeaTime.TabIndex = 20;
            this.lblEorzeaTime.Text = "00:00 AM";
            // 
            // LbxOutput3
            // 
            this.LbxOutput3.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.LbxOutput3.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.LbxOutput3.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.LbxOutput3.ForeColor = System.Drawing.SystemColors.InactiveCaptionText;
            this.LbxOutput3.FormattingEnabled = true;
            this.LbxOutput3.HorizontalScrollbar = true;
            this.LbxOutput3.Location = new System.Drawing.Point(160, 17);
            this.LbxOutput3.Name = "LbxOutput3";
            this.LbxOutput3.Size = new System.Drawing.Size(907, 221);
            this.LbxOutput3.TabIndex = 25;
            this.LbxOutput3.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.Lbxoutput_DrawItem);
            // 
            // lblPCPositionZone
            // 
            this.lblPCPositionZone.AutoSize = true;
            this.lblPCPositionZone.ForeColor = System.Drawing.Color.Black;
            this.lblPCPositionZone.Location = new System.Drawing.Point(83, 41);
            this.lblPCPositionZone.Name = "lblPCPositionZone";
            this.lblPCPositionZone.Size = new System.Drawing.Size(25, 13);
            this.lblPCPositionZone.TabIndex = 23;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.ForeColor = System.Drawing.Color.Black;
            this.label2.Location = new System.Drawing.Point(15, 41);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(51, 13);
            this.label2.TabIndex = 24;
            // 
            // lblPCPositionX
            // 
            this.lblPCPositionX.AutoSize = true;
            this.lblPCPositionX.ForeColor = System.Drawing.Color.Black;
            this.lblPCPositionX.Location = new System.Drawing.Point(57, 64);
            this.lblPCPositionX.Name = "lblPCPositionX";
            this.lblPCPositionX.Size = new System.Drawing.Size(34, 13);
            this.lblPCPositionX.TabIndex = 25;
            this.lblPCPositionX.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.ForeColor = System.Drawing.Color.Black;
            this.label3.Location = new System.Drawing.Point(16, 64);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(17, 13);
            this.label3.TabIndex = 26;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.ForeColor = System.Drawing.Color.Black;
            this.label4.Location = new System.Drawing.Point(106, 64);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(17, 13);
            this.label4.TabIndex = 27;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.ForeColor = System.Drawing.Color.Black;
            this.label5.Location = new System.Drawing.Point(16, 83);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(17, 13);
            this.label5.TabIndex = 28;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.ForeColor = System.Drawing.Color.Black;
            this.label6.Location = new System.Drawing.Point(106, 83);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(18, 13);
            this.label6.TabIndex = 29;
            // 
            // lblPCPositionY
            // 
            this.lblPCPositionY.AutoSize = true;
            this.lblPCPositionY.ForeColor = System.Drawing.Color.Black;
            this.lblPCPositionY.Location = new System.Drawing.Point(166, 64);
            this.lblPCPositionY.Name = "lblPCPositionY";
            this.lblPCPositionY.Size = new System.Drawing.Size(34, 13);
            this.lblPCPositionY.TabIndex = 30;
            this.lblPCPositionY.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblPCPositionZ
            // 
            this.lblPCPositionZ.AutoSize = true;
            this.lblPCPositionZ.ForeColor = System.Drawing.Color.Black;
            this.lblPCPositionZ.Location = new System.Drawing.Point(58, 83);
            this.lblPCPositionZ.Name = "lblPCPositionZ";
            this.lblPCPositionZ.Size = new System.Drawing.Size(34, 13);
            this.lblPCPositionZ.TabIndex = 31;
            this.lblPCPositionZ.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // lblPCPositionR
            // 
            this.lblPCPositionR.AutoSize = true;
            this.lblPCPositionR.ForeColor = System.Drawing.Color.Black;
            this.lblPCPositionR.Location = new System.Drawing.Point(167, 83);
            this.lblPCPositionR.Name = "lblPCPositionR";
            this.lblPCPositionR.Size = new System.Drawing.Size(34, 13);
            this.lblPCPositionR.TabIndex = 32;
            this.lblPCPositionR.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // materialLabel2
            // 
            this.materialLabel2.AutoSize = true;
            this.materialLabel2.Depth = 0;
            this.materialLabel2.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.materialLabel2.Location = new System.Drawing.Point(15, 9);
            this.materialLabel2.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialLabel2.Name = "materialLabel2";
            this.materialLabel2.Size = new System.Drawing.Size(106, 19);
            this.materialLabel2.TabIndex = 33;
            // 
            // materialDivider1
            // 
            this.materialDivider1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(30)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))));
            this.materialDivider1.Depth = 0;
            this.materialDivider1.Location = new System.Drawing.Point(16, 27);
            this.materialDivider1.MouseState = MaterialSkin.MouseState.HOVER;
            this.materialDivider1.Name = "materialDivider1";
            this.materialDivider1.Size = new System.Drawing.Size(190, 1);
            this.materialDivider1.TabIndex = 34;
            // 
            // MainWindow
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.Maroon;
            this.ClientSize = new System.Drawing.Size(824, 599);
            this.Controls.Add(this.lblEorzeaTime);
            this.Controls.Add(this.materialLabel1);
            this.Controls.Add(this.btnLaunchGame);
            this.Controls.Add(this.materialTabControl1);
            this.Controls.Add(this.materialTabSelector1);
            this.ForeColor = System.Drawing.Color.Maroon;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "MainWindow";
            this.Text = "Primal Launcher";
            this.Load += new System.EventHandler(this.MainWindow_Load);
            this.materialTabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.materialCard1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private MaterialSkin.Controls.MaterialTabSelector materialTabSelector1;
        private MaterialSkin.Controls.MaterialTabControl materialTabControl1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.TabPage tabPage3;
        private System.Windows.Forms.TabPage tabPage4;
        private System.Windows.Forms.TabPage tabPage5;
        private System.Windows.Forms.TabPage tabPage6;
        private MaterialSkin.Controls.MaterialButton btnLaunchGame;
        private MaterialSkin.Controls.MaterialLabel materialLabel1;
        private MaterialSkin.Controls.MaterialLabel lblEorzeaTime;
        private System.Windows.Forms.TabPage tabPage1;
        private MaterialSkin.Controls.MaterialCard materialCard2;
        private MaterialSkin.Controls.MaterialCard materialCard1;
        private System.Windows.Forms.ListBox LbxOutput3;
        private System.Windows.Forms.Label lblPCPositionZone;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label lblPCPositionX;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label lblPCPositionY;
        private System.Windows.Forms.Label lblPCPositionZ;
        private System.Windows.Forms.Label lblPCPositionR;
        private MaterialSkin.Controls.MaterialLabel materialLabel2;
        private MaterialSkin.Controls.MaterialDivider materialDivider1;
        private System.Windows.Forms.ListBox LbxOutput;
    }
}

