// Copyright (C) 2022 Andreus Faria
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY

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
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PrimalLauncher
{
    public partial class MainWindow : Form
    {
        [DllImport("kernel32.dll")]
        public static extern UInt64 GetTickCount64();

        [DllImport("kernel32.dll")]
        public static extern uint GetTickCount();

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
            Log.Instance.Info("Welcome to Primal Launcher!");            

            //if (File.Exists(Preferences.Instance.AppDataFile))
            //    File.Delete(Preferences.Instance.AppDataFile);

            //if (File.Exists(Preferences.Instance.AppUserFile))
            //    File.Delete(Preferences.Instance.AppUserFile);

            //File.Delete("packet_output.txt");

            //List<Actor> acto = ActorRepository.GetZoneNpcs(0xce);            
            //var z = ZoneRepository.GetInstance("man0g1_1");
            //var z =ZoneRepository.GetZones();
            //ZoneInstance z = ZoneRepository.GetInstance(12);           
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
            if (false)
            {
                LaunchGame(1);
            }
            else
            {
                Task.Run(() => { new UpdateServer(); });
                Task.Run(() => { new HttpServer(); });
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

        private void LaunchGame(int sessionId)
        {
            //generate encryption key and initialize blowfish
            uint tickcount = (uint)Environment.TickCount;
            string encryptionKeyStr = string.Format("{0}", (tickcount & ~0xFFFF).ToString("X2")).ToLower();
            byte[] encryptionKey = new byte[encryptionKeyStr.Length];

            for (int i = 0; i < encryptionKeyStr.Length; i++)
                encryptionKey[i] = (byte)encryptionKeyStr[i];

            Blowfish bf = new Blowfish(encryptionKey);

            //get command line arguments chars/bytes
            string commandLineStr = string.Format(" T ={0} /LANG =en-us /REGION =2 /SERVER_UTC =1356916742 /SESSION_ID ={1}", tickcount, sessionId);
            char[] commandLineC = new char[commandLineStr.Length];
            byte[] commandLine = new byte[commandLineStr.Length];

            for (int i = 0; i < commandLineStr.Length; i++)
            {
                commandLine[i] = (byte)commandLineStr[i];
                commandLineC[i] = commandLineStr[i];
            }

            //encrypt command line arguments
            int commandLineSize = commandLine.Length + 1;

            for (int i = 0; i < (commandLineSize & ~0x7); i += 8)
            {
                uint xl = BitConverter.ToUInt32(commandLine, i);
                uint xr = BitConverter.ToUInt32(commandLine, i + 4);

                bf.BlowfishEncipher(ref xl, ref xr);

                Buffer.BlockCopy(BitConverter.GetBytes(xl), 0, commandLine, i, sizeof(uint));
                Buffer.BlockCopy(BitConverter.GetBytes(xr), 0, commandLine, i + 4, sizeof(uint));
            }

            //base 64 encode command line encrypted arguments        
            string encodedCommandLine = Convert.ToBase64String(commandLine);
            encodedCommandLine = encodedCommandLine.Replace("+", "-").Replace("/", "_");

            //format command line and arguments
            string completeCommandLine = string.Format("{0}\\ffxivgame.exe", Preferences.Instance.Options.GameInstallPath);
            string arguments = string.Format("sqex0002{0}!////", encodedCommandLine);

            //open game
            Process p = new Process();
            p.StartInfo.FileName = completeCommandLine;
            p.StartInfo.Arguments = arguments;
            p.Start();
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
            }          
        }
    }
}
