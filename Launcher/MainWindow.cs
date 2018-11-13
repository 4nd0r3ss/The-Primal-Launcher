using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Launcher
{
    public partial class MainWindow : Form
    {
        private Preferences Config { get; set; } = Preferences.Instance;
        private Log LogMsg { get; set; } = Log.Instance;
        private UpdateServer Server { get; set; }

        public MainWindow()
        {          
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            //dirty way to get rid of cross-thread exception
            CheckForIllegalCrossThreadCalls = false;

            //Output logger thread
            Task.Run(() =>
            {
                while (true)                
                    if (LogMsg.HasLogMessages())
                        this.LbxOutput.Items.Insert(0, LogMsg.GetLogMessage());                
            });
            
            LogMsg.LogMessage(LogMsg.MSG, "Welcome to the Seventh Astral Server app!");

            //Get paths from configuration file.  
            txtGameInstallPath.Text = Config.Options.GameInstallPath;
            txtPatchPath.Text = Config.Options.PatchDownloadPath;

            LblDownloadStatus.Text = ServerUtilities.GetEpoch();
        }

        private void BtnChangeGameInstallPath_Click(object sender, EventArgs e)
        {
            string path = Config.SelectFolder();
            
            if (path != null)
                txtGameInstallPath.Text = path;            
        }

        private void BtnChangePatchPath_Click(object sender, EventArgs e)
        {
            string path = Config.SelectFolder();
            
            if (path != null)
                txtPatchPath.Text = path;
        }

        private void BtnLaunchGame_Click(object sender, EventArgs e)
        {
            
            Task.Run(() => { UpdateServer.Initialize(); });
            Task.Run(() => { LobbyServer.Initialize(); });
        }

        private void Button1_Click(object sender, EventArgs e) =>        
            Task.Run(() => { Patcher.PatchExecutableFiles(); });        

        private void Lbxoutput_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            e.DrawFocusRectangle();

            Color c = Color.White; //default color
            string str = this.LbxOutput.Items[e.Index].ToString();
            
            if (str.IndexOf(LogMsg.ERR) >= 0)            
                c = Color.Red;   
            else if (str.IndexOf(LogMsg.WNG) >= 0)
                c = Color.Yellow;
            else if (str.IndexOf(LogMsg.OK) >= 0)
                c = Color.DarkGreen;

            e.Graphics.DrawString(
                this.LbxOutput.Items[e.Index].ToString(), 
                new Font(FontFamily.GenericMonospace, 8, FontStyle.Regular), 
                new SolidBrush(c), e.Bounds);
        }

        private void Lbxoutput_SelectedIndexChanged(object sender, EventArgs e)
        {

        }
    }
}
