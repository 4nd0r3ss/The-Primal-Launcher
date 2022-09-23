
namespace PrimalLauncher
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
            this.lblSeparator1 = new System.Windows.Forms.Label();
            this.panelMain = new System.Windows.Forms.Panel();
            this.lblClock = new PrimalLauncher.OutlinedFontLabel();
            this.lblUpdate = new PrimalLauncher.OutlinedFontLabel();
            this.lblOptions = new PrimalLauncher.OutlinedFontLabel();
            this.lblLog = new PrimalLauncher.OutlinedFontLabel();
            this.btnLaunch = new System.Windows.Forms.Button();
            this.panelMain.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblSeparator1
            // 
            this.lblSeparator1.BackColor = System.Drawing.SystemColors.ButtonFace;
            this.lblSeparator1.Location = new System.Drawing.Point(34, 525);
            this.lblSeparator1.Name = "lblSeparator1";
            this.lblSeparator1.Size = new System.Drawing.Size(730, 2);
            this.lblSeparator1.TabIndex = 15;
            // 
            // panelMain
            // 
            this.panelMain.BackColor = System.Drawing.Color.Transparent;
            this.panelMain.Location = new System.Drawing.Point(37, 72);
            this.panelMain.Name = "panelMain";
            this.panelMain.Size = new System.Drawing.Size(725, 434);
            this.panelMain.TabIndex = 13;
            // 
            // lblClock
            // 
            this.lblClock.AutoSize = true;
            this.lblClock.BackColor = System.Drawing.Color.Transparent;
            this.lblClock.Font = new System.Drawing.Font("Cambria", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblClock.Location = new System.Drawing.Point(37, 545);
            this.lblClock.Name = "lblClock";
            this.lblClock.OutlineForeColor = System.Drawing.Color.Black;
            this.lblClock.OutlineWidth = 2F;
            this.lblClock.Size = new System.Drawing.Size(172, 19);
            this.lblClock.TabIndex = 23;
            this.lblClock.Text = "Eorzea time: 00:00 AM";
            // 
            // lblUpdate
            // 
            this.lblUpdate.AutoSize = true;
            this.lblUpdate.BackColor = System.Drawing.Color.Transparent;
            this.lblUpdate.Font = new System.Drawing.Font("Cambria", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblUpdate.Location = new System.Drawing.Point(184, 32);
            this.lblUpdate.Name = "lblUpdate";
            this.lblUpdate.OutlineForeColor = System.Drawing.Color.Black;
            this.lblUpdate.OutlineWidth = 2F;
            this.lblUpdate.Size = new System.Drawing.Size(134, 23);
            this.lblUpdate.TabIndex = 22;
            this.lblUpdate.Text = "Game Update";
            this.lblUpdate.Click += new System.EventHandler(this.lblUpdate_Click);
            // 
            // lblOptions
            // 
            this.lblOptions.AutoSize = true;
            this.lblOptions.BackColor = System.Drawing.Color.Transparent;
            this.lblOptions.Font = new System.Drawing.Font("Cambria", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblOptions.Location = new System.Drawing.Point(92, 32);
            this.lblOptions.Name = "lblOptions";
            this.lblOptions.OutlineForeColor = System.Drawing.Color.Black;
            this.lblOptions.OutlineWidth = 2F;
            this.lblOptions.Size = new System.Drawing.Size(81, 23);
            this.lblOptions.TabIndex = 21;
            this.lblOptions.Text = "Options";
            this.lblOptions.Click += new System.EventHandler(this.lblOptions_Click);
            // 
            // lblLog
            // 
            this.lblLog.AutoSize = true;
            this.lblLog.BackColor = System.Drawing.Color.Transparent;
            this.lblLog.Font = new System.Drawing.Font("Cambria", 15F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLog.Location = new System.Drawing.Point(38, 32);
            this.lblLog.Name = "lblLog";
            this.lblLog.OutlineForeColor = System.Drawing.Color.Black;
            this.lblLog.OutlineWidth = 2F;
            this.lblLog.Size = new System.Drawing.Size(42, 23);
            this.lblLog.TabIndex = 20;
            this.lblLog.Text = "Log";
            this.lblLog.Click += new System.EventHandler(this.lblLog_Click);
            // 
            // btnLaunch
            // 
            this.btnLaunch.BackColor = System.Drawing.Color.Transparent;
            this.btnLaunch.BackgroundImage = global::PrimalLauncher.Properties.Resources.button_off;
            this.btnLaunch.FlatAppearance.BorderSize = 0;
            this.btnLaunch.FlatAppearance.MouseDownBackColor = System.Drawing.Color.Transparent;
            this.btnLaunch.FlatAppearance.MouseOverBackColor = System.Drawing.Color.Transparent;
            this.btnLaunch.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnLaunch.Font = new System.Drawing.Font("Cambria", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnLaunch.ForeColor = System.Drawing.SystemColors.Control;
            this.btnLaunch.Location = new System.Drawing.Point(636, 545);
            this.btnLaunch.Name = "btnLaunch";
            this.btnLaunch.Size = new System.Drawing.Size(124, 26);
            this.btnLaunch.TabIndex = 18;
            this.btnLaunch.Text = "Launch Game";
            this.btnLaunch.UseVisualStyleBackColor = false;
            this.btnLaunch.Click += new System.EventHandler(this.btnLaunch_Click);
            this.btnLaunch.MouseLeave += new System.EventHandler(this.btnLaunch_MouseLeave);
            this.btnLaunch.MouseHover += new System.EventHandler(this.btnLaunch_MouseHover);
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackgroundImage = global::PrimalLauncher.Properties.Resources.background;
            this.ClientSize = new System.Drawing.Size(799, 601);
            this.Controls.Add(this.lblClock);
            this.Controls.Add(this.lblUpdate);
            this.Controls.Add(this.lblOptions);
            this.Controls.Add(this.lblLog);
            this.Controls.Add(this.btnLaunch);
            this.Controls.Add(this.lblSeparator1);
            this.Controls.Add(this.panelMain);
            this.DoubleBuffered = true;
            this.Font = new System.Drawing.Font("Cambria", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ForeColor = System.Drawing.SystemColors.Control;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.MaximizeBox = false;
            this.Name = "MainWindow";
            this.Text = "The Primal Launcher v1.0";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainWindow_FormClosing);
            this.Shown += new System.EventHandler(this.MainWindow_Shown);
            this.panelMain.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Label lblSeparator1;
        private System.Windows.Forms.Panel panelMain;
        private OutlinedFontLabel lblLog;
        private OutlinedFontLabel lblOptions;
        private OutlinedFontLabel lblUpdate;
        private OutlinedFontLabel lblClock;
        private System.Windows.Forms.Button btnLaunch;
        private ucLog ucLog1;
    }
}

