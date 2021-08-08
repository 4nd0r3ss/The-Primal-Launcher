using MaterialSkin;
using MaterialSkin.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PrimalLauncher
{
    public partial class MainWindow : MaterialForm
    {
        public string DownloadStatus
        {
            get { return lblDownloadStat.Text; }
            set { lblDownloadStat.Text = value; }
        }

        public string UpdateStatus
        {
            get { return lblUpdate.Text; }
            set { lblUpdate.Text = value; }
        }

        public bool ChkKeepUpdatefiles
        {
            get { return chkKeepFiles.Checked; }
        }

        public int DownloadProgressValue
        {
            get { return progressBar.Value;  }
            set { progressBar.Value = value; }
        }

        public int UpdateProgressValue
        {
            get { return progressBar1.Value; }
            set { progressBar1.Value = value; }
        }

        public ProgressBar UpdateProgressBar
        {
            get { return progressBar1; }
        }

        public bool UpdateBtnEnabled
        {
            get { return btnGameUpdate.Enabled; }
            set { btnGameUpdate.Enabled = value; }
        }

        public bool DownloadStartBtnEnabled
        {
            get { return btnDownload.Enabled; }
            set { btnDownload.Enabled = value; }
        }

        public bool DownloadCancelBtnEnabled
        {
            get { return btnDownloadCancel.Enabled; }
            set { btnDownloadCancel.Enabled = value; }
        }

        public MaterialTabSelector TabSelector
        {
            get { return materialTabSelector1;  }
        }

        public MaterialButton BtnLaunchGame
        {
            get { return btnLaunchGame; }
        }

        public MainWindow()
        {
            CheckForIllegalCrossThreadCalls = false;
            InitializeComponent();

            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;            
            materialSkinManager.ColorScheme = new ColorScheme((Primary)0x800101, (Primary)0x630101, (Primary)0x800101, (Accent)0xf5d800, TextShade.WHITE);         
        }

        private void BtnLaunchGame_Click(object sender, EventArgs e)
        {
            //Launch();
            Task.Run(() => { new UpdateServer();});
            Task.Run(() => { new HttpServer(); });
        }

        public void SelectTab(int tabIndex)
        {
            materialTabControl1.SelectTab(tabIndex);
        }


        /// <summary>
        /// Launch the game through command line. This is a direct translation of SU code from C++ to C#.
        /// http://seventhumbral.org/downloads.php
        /// </summary>
        private void Launch()
        {          
            string sessionId = "1";
            uint ticks = (uint)Environment.TickCount;
            string command = string.Format(" T ={0} /LANG =en-us /REGION =2 /SERVER_UTC =1356916742 /SESSION_ID ={1}", ticks, sessionId);
            char[] encriptionKey = (ticks & ~0xFFFF).ToString("X8").ToArray();

            char[] commandLine = new char[0x400];
            Buffer.BlockCopy(Encoding.ASCII.GetBytes(command), 0, commandLine, 0, command.Length);           

            Blowfish bf = new Blowfish(Encoding.ASCII.GetBytes(encriptionKey));

            for (int i = 0; i < commandLine.Length; i++)
            {
                uint xl = commandLine[i];
                uint xr = commandLine[i + 4];
                bf.BlowfishEncipher(ref xl, ref xr);               
            }

            string encodedCommandLine = Convert.ToBase64String(Encoding.ASCII.GetBytes(commandLine));
            encodedCommandLine.Replace('+', '-');
            encodedCommandLine.Replace('/', '_');      

            Process.Start(Preferences.Instance.Options.GameInstallPath + "ffxivgame.exe", "  sqex0002" + commandLine + "!////");
        }       

        private void MainWindow_Load(object sender, EventArgs e)
        {
            Preferences.Instance.LoadConfigFile();           
            
            SetGameUpdateNavigation();

            //List<Zone> myList = ZoneRepository.GetZones();
            //List<Actor> acto = ActorRepository.Instance.GetZoneNpcs(206);
            //Quest quest = QuestRepository.GetInitialQuest(1);
            //List<Actor> acto = ActorRepository.Instance.GetCompanyWarp(206);

            Log.Instance.Info("Welcome to Primal Launcher!");
            Task.Run(() => { new GameServer(); });
            Task.Run(() => { new LobbyServer(); });
        }

        public void SetGameUpdateNavigation()
        {
            bool filesDownloaded = GameUpdate.Instance.CheckUpdateFiles();
            bool gameUpdated = Updater.GameIsUpdated();
            bool gamePatched = Patcher.GameIsPatched();

            //if ()
            //{
                btnLaunchGame.Enabled = true;
            //}
                
            
        }

        #region Lock/Unlock contols
        private void ToggleDownloadUI(bool enable)
        {
            btnDownload.Enabled = enable;
            btnDownloadCancel.Enabled = enable;    
        }

        private void ToggleUpdateUI(bool enable)
        {

        }

        #endregion

        public void WriteLogMessage(string message)
        {
            LbxOutput.Invoke((MethodInvoker)(() =>
            {
                LbxOutput.Items.Insert(0, message);
            }));
        }

        private void Lbxoutput_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            e.DrawFocusRectangle();
            e.Graphics.DrawString(
                LbxOutput.Items[e.Index].ToString(),
                new Font(FontFamily.GenericMonospace, 8, FontStyle.Regular),
                new SolidBrush(Log.Instance.GetMessageColor(LbxOutput.Items[e.Index].ToString())), e.Bounds);
        }

        private void clock_ticker_Tick(object sender, EventArgs e)
        {
            lblEorzeaTime.Text = Clock.Instance.StringTime;

            if(User.Instance.Character != null)
            {
                Position pos = User.Instance.Character.Position;
                lblLocation.Text = pos.ZoneId.ToString();
                lblx.Text = string.Format("{0:0.0}", pos.X);
                lbly.Text = string.Format("{0:0.0}", pos.Y);
                lblz.Text = string.Format("{0:0.0}", pos.Z);
                lblr.Text = string.Format("{0:0.0}", pos.R);

                //User.Instance.Character.UpdatePlayTime();
                //lblPlaytime.Text = User.Instance.Character.TotalPlaytime.ToString();
            }            
        } 
        
        public static MainWindow Get()
        {
            foreach (Form frm in Application.OpenForms)
                if (frm.GetType() == typeof(MainWindow))
                    return (MainWindow)frm;
                else
                    continue;

            return null;            
        }

        private void btnDownload_Click(object sender, EventArgs e)
        {
            GameUpdate.Instance.DownloadFile();
        }

        private void btnDownloadCancel_Click(object sender, EventArgs e)
        {
            GameUpdate.Instance.DownloadCancel();
        }

        private void btnGameUpdate_Click(object sender, EventArgs e)
        {
            //run as admin code from: https://stackoverflow.com/questions/6412896/giving-application-elevated-uac
            WindowsPrincipal pricipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            bool hasAdministrativeRight = pricipal.IsInRole(WindowsBuiltInRole.Administrator);

            if (!hasAdministrativeRight)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.UseShellExecute = true;
                startInfo.WorkingDirectory = Environment.CurrentDirectory;
                startInfo.FileName = Application.ExecutablePath;
                startInfo.Verb = "runas";
                try
                {
                    Process p = Process.Start(startInfo);
                    Application.Exit();
                }
                catch (System.ComponentModel.Win32Exception ex)
                {
                    MessageBox.Show("This utility requires elevated priviledges to complete correctly.", "Error: UAC Authorization Required", MessageBoxButtons.OK);
                    Log.Instance.Error(ex.Message);
                    return;
                }
            }
            else
            {
                Task.Run(() => { GameUpdate.Instance.StartGameUpdate(); });
            }
        }

        private void chkKeepFiles_CheckedChanged(object sender, EventArgs e)
        {

        }

        private void btnPatch_Click(object sender, EventArgs e)
        {
            GameUpdate.Instance.Patch();
        }
    }
}
