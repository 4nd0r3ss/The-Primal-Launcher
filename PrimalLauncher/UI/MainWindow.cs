/* 
Copyright (C) 2022 Andreus Faria

This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program.  If not, see <https://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace PrimalLauncher
{
    public partial class MainWindow : Form
    {
        public static MainWindow Window = null;
        private bool IsInstallationOk { get; set; }

        public string ClockTime { set { lblClock.Text = value; } }

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x2000000;
                return cp;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            Window = this;

            panelMain.Controls.Add(ucLog.Instance);
            ucLog.Instance.Dock = DockStyle.Fill;
            ucLog.Instance.BringToFront();
            ResetTabButtonColors();
            lblLog.ForeColor = Color.Moccasin;

            lblSeparator1.BackColor = Color.FromArgb(64, 128, 128, 128);
            Preferences.Instance.LoadConfigFile();
            Log.Instance.Info("Welcome to The Primal Launcher!");

            /* debug stuff */
            //var actors = ActorXmlLoader.GetZoneNpcs(0x9b);  
            //var zones = ZoneRepository.GetZones();
            //var instance = ZoneXmlLoader.GetInstance(8);           
            //var i = GuildLeveXmlLoader.GetGuildLevePackSet(21);
            //var monsters = ActorXmlLoader.GetZoneMonsters(0x96);
            //var instance = World.Instance.CreateInstance(6);
            //ItemGraphics.Instance.Load();
            //var gls = GuildLeveXmlLoader.GetPassiveGLs(155);   
            //var job = new Job(4, "testjob", 50);
            //job.LoadActions();
        }

        private void ResetTabButtonColors()
        {
            lblLog.ForeColor = Color.White;
            lblOptions.ForeColor = Color.White;
            lblUpdate.ForeColor = Color.White;
        }

        private void lblLog_Click(object sender, EventArgs e)
        {
            if (!panelMain.Controls.Contains(ucLog.Instance))
            {
                panelMain.Controls.Add(ucLog.Instance);
                ucLog.Instance.Dock = DockStyle.Fill;
            }

            ucLog.Instance.BringToFront();
            ResetTabButtonColors();
            lblLog.ForeColor = Color.Moccasin;
        }     

        private void lblOptions_Click(object sender, EventArgs e)
        {
            if (!panelMain.Controls.Contains(ucOptions.Instance))
            {
                panelMain.Controls.Add(ucOptions.Instance);
                ucOptions.Instance.Dock = DockStyle.Fill;
            }

            ucOptions.Instance.BringToFront();
            ResetTabButtonColors();
            lblOptions.ForeColor = Color.Moccasin;
        }       

        private void btnLaunch_MouseHover(object sender, EventArgs e)
        {
            btnLaunch.BackgroundImage = Properties.Resources.button_on;
            btnLaunch.Refresh();
        }

        private void btnLaunch_MouseLeave(object sender, EventArgs e)
        {
            btnLaunch.BackgroundImage = Properties.Resources.button_off;
            btnLaunch.Refresh();
        }

        private void btnLaunch_Click(object sender, EventArgs e)
        {


            
            if (Preferences.Instance.Options.ShowLoginPage)
            {
                Task.Run(() => { new UpdateServer(); });
                Task.Run(() => { new HttpServer(); });                
            }
            else
            {
                Launcher.Launch("1");
            }
        }

        private void lblUpdate_Click(object sender, EventArgs e)
        {
            if (!panelMain.Controls.Contains(ucUpdate.Instance))
            {
                panelMain.Controls.Add(ucUpdate.Instance);
                ucUpdate.Instance.Dock = DockStyle.Fill;
            }

            ucUpdate.Instance.BringToFront();
            ResetTabButtonColors();
            lblUpdate.ForeColor = Color.Moccasin;
        }

        private void ToggleTabSelector(bool enabled)
        {
            btnLaunch.Enabled = enabled;
            lblLog.Enabled = enabled;
            lblOptions.Enabled = enabled;
            lblUpdate.Enabled = !enabled;
            var controls = panelMain.Controls;
            UserControl control;

            if (lblUpdate.Enabled)
            {
                control = ucUpdate.Instance;
                lblUpdate.ForeColor = Color.Moccasin;
            }
            else
            {
                control = ucLog.Instance;
                lblLog.ForeColor = Color.Moccasin;
            }

            control.Dock = DockStyle.Fill;
            control.BringToFront();          
            controls.Add(control);
        }

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            LobbyServer.Instance.ServerShutDown();
            GameServer.Instance.ServerShutDown();

        }

        private void MainWindow_Shown(object sender, EventArgs e)
        {
            IsInstallationOk =  GameInstallationChecker.Check();

            if(IsInstallationOk)
            {               
                Task.Run(() => { try { LobbyServer.Instance.Initialize(); } catch (Exception ex) { throw ex; } });
                Task.Run(() => { try { GameServer.Instance.Initialize(); } catch (Exception ex) { throw ex; } });
            }
            else
            {
                ToggleTabSelector(IsInstallationOk);
                lblUpdate_Click(lblUpdate, EventArgs.Empty);
            }          
        }

        public void EnableLaunchGameBtn()
        {
            btnLaunch.Enabled = true;
        }

        public void FocusLogWindow()
        {
            ToggleTabSelector(true);
            lblUpdate.Enabled = true;
            lblLog_Click(lblUpdate, EventArgs.Empty);
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {

        }
    }
}
