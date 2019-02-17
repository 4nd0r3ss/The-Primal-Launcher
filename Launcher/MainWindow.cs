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
        private Log _log = Log.Instance;

        #region Main window click & drag stuff
        public const int WM_NCLBUTTONDOWN = 0xA1;
        public const int HT_CAPTION = 0x2;

        [System.Runtime.InteropServices.DllImportAttribute("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
        [System.Runtime.InteropServices.DllImportAttribute("user32.dll")]
        public static extern bool ReleaseCapture();
        #endregion

        public MainWindow()
        {
            InitializeComponent();

            //load default user control (log window)
            pnlMain.Controls.Add(ucLogWindow.Instance);
            ucLogWindow.Instance.Dock = DockStyle.Fill;
            ucLogWindow.Instance.BringToFront();

            //default user control button color
            btnLogWindow.BackColor = Color.White;
            btnLogWindow.ForeColor = Color.Maroon;

            _log.Info("Welcome to Primal Launcher!");
        }       

        private void BtnLaunchGame_Click(object sender, EventArgs e)
            => Task.Run(() => { UpdateServer update = new UpdateServer(); });    

        private void TopFrame_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                ReleaseCapture();
                SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
            }
        }

        private void btnClose_Click(object sender, EventArgs e) => Application.Exit();

        private void btnMinimize_Click(object sender, EventArgs e) => this.WindowState = FormWindowState.Minimized;

        private void ButtonColorsSetup(object sender)
        {
            Button btn = sender as Button;

            foreach (Button b in pnlButtons.Controls)
            {
                b.BackColor = Color.DarkRed;
                b.ForeColor = Color.White;
            }

            btn.BackColor = Color.White;
            btn.ForeColor = Color.Maroon;
        }

        private void btnLogWindow_Click(object sender, EventArgs e)
        {            
            ucLogWindow.Instance.BringToFront();
            ButtonColorsSetup(sender);
        }      

        private void btnOptions_Click(object sender, EventArgs e)
        {
            if (!pnlMain.Controls.Contains(ucOptions.Instance))
            {
                pnlMain.Controls.Add(ucOptions.Instance);
                ucOptions.Instance.Dock = DockStyle.Fill;
            }
            
            ucOptions.Instance.BringToFront();
            ButtonColorsSetup(sender);
        }

        private void btnAbout_Click(object sender, EventArgs e)
        {
            if (!pnlMain.Controls.Contains(ucAbout.Instance))
            {
                pnlMain.Controls.Add(ucAbout.Instance);
                ucAbout.Instance.Dock = DockStyle.Fill;
            }
            ucAbout.Instance.BringToFront();
            ButtonColorsSetup(sender);
        }
    }
}
