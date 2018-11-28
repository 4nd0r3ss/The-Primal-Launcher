using Launcher.users;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Launcher
{
    public partial class MainWindow : Form
    {
        private Preferences Config { get; set; } = Preferences.Instance;
        private Log _log { get; set; } = Log.Instance;
          
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [System.Runtime.InteropServices.DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();


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
                    if (_log.HasLogMessages())
                        this.LbxOutput.Items.Insert(0, _log.GetLogMessage());                
            });

            _log.Warning("Welcome to the Seventh Astral Server app!");

            //Get paths from configuration file.  
            txtGameInstallPath.Text = Config.Options.GameInstallPath;
            txtPatchPath.Text = Config.Options.PatchDownloadPath;

            
            
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
            LobbyServer lobby;
            Task.Run(() => { UpdateServer.Initialize(); });            
            Task.Run(() => { lobby = new LobbyServer(); });
        }

        private void Button1_Click(object sender, EventArgs e) =>        
            Task.Run(() => { Patcher.PatchExecutableFiles(); });        

        private void Lbxoutput_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.DrawBackground();
            e.DrawFocusRectangle();
            e.Graphics.DrawString(
                this.LbxOutput.Items[e.Index].ToString(), 
                new Font(FontFamily.GenericMonospace, 8, FontStyle.Regular), 
                new SolidBrush(_log.GetMessageColor(this.LbxOutput.Items[e.Index].ToString())), e.Bounds);
        }

        private void Lbxoutput_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void TopFrame_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void btnClose_Click(object sender, EventArgs e) => Application.Exit();

        private void btnMinimize_Click(object sender, EventArgs e) => this.WindowState = FormWindowState.Minimized;

        private void panel3_Paint(object sender, PaintEventArgs e)
        {

        }
    }
}
