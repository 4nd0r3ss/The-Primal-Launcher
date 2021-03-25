using MaterialSkin;
using MaterialSkin.Controls;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Launcher
{
    public partial class MainWindow : MaterialForm
    {
        public MainWindow()
        {
            InitializeComponent();

            var materialSkinManager = MaterialSkinManager.Instance;
            materialSkinManager.AddFormToManage(this);
            materialSkinManager.Theme = MaterialSkinManager.Themes.LIGHT;
            materialSkinManager.ColorScheme = new ColorScheme((Primary)0x800101, (Primary)0x630101, (Primary)0x800101, (Accent)0xf5d800, TextShade.WHITE);


            //first thing is to check game installation.
            if (true)
            {
                //ucLogWindow.Instance.BringToFront();

                //default user control button color
                //btnLogWindow.BackColor = Color.White;
                //btnLogWindow.ForeColor = Color.Maroon;
            }
            else
            {
                //load game update user control
                //pnlMain.Controls.Add(ucGameUpdate.Instance);
                ucGameUpdate.Instance.Dock = DockStyle.Fill;
                ucGameUpdate.Instance.BringToFront();

                //default user control button color
                //btnGameUpdate.BackColor = Color.White;
                //btnGameUpdate.ForeColor = Color.Maroon;

                //disable launch button
                btnLaunchGame.Enabled = false;
            }

            

        }

        private void BtnLaunchGame_Click(object sender, EventArgs e)
        {
            Task.Run(() => { new UpdateServer();});
            Task.Run(() => { new HttpServer(); });          
        }
        
        private void Launch()
        {          
            string sessionId = "1";
            uint ticks = (uint)DateTime.Now.Ticks;
            string commandLine = string.Format(" T ={0} /LANG =en-us /REGION =2 /SERVER_UTC =1356916742 /SESSION_ID ={1}", ticks, sessionId);
                        
            //Blowfish fb = new Blowfish(
            //    Blowfish.GenerateKey(
            //        (ticks & ~0xffff).ToString(),
            //        BitConverter.ToUInt32(new byte[] { 1 }, 0)
            //    )
            //);

            //Process.Start(Preferences.Instance.Options.GameInstallPath + "ffxivgame.exe", "  sqex0002" + commandLine + "!////");
        }       

        private void MainWindow_Load(object sender, EventArgs e)
        {
            Log.Instance.Info("Welcome to Primal Launcher!");
            Task.Run(() => { new GameServer(); });
            Task.Run(() => { new LobbyServer(); });
        }

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

        //public void PrintPlayerPosition(Position position, byte[] unknown)
        //{
        //    lblPCPositionZone.Text = position.ZoneId.ToString();
        //    lblPCPositionX.Text = position.X.ToString();
        //    lblPCPositionY.Text = position.Y.ToString();
        //    lblPCPositionZ.Text = position.Z.ToString();
        //    lblPCPositionR.Text = position.R.ToString();




        //    //"x: " + position.X + ", y: " + position.Y + ", z: " + position.Z + ", r: " + position.R + ", region id: " + position.ZoneId + ", unknown: " +
        //    //unknown[0].ToString("X2") + " " + unknown[1].ToString("X2") + " " + unknown[2].ToString("X2") + " " +
        //    //unknown[3].ToString("X2") + " " + unknown[4].ToString("X2") + " " + unknown[5].ToString("X2");

        //}
    }
}
