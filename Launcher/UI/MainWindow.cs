using MaterialSkin;
using MaterialSkin.Controls;
using System;
using System.Collections.Generic;
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
            Preferences.Instance.LoadConfigFile();

            //first thing is to check game installation.
            if (true)
            {
                //show log tab
            }
            else
            {
                //show update tab
            }

            //List<Zone> myList = ZoneRepository.GetZones();
            //List<Actor> acto = ActorRepository.Instance.GetZoneNpcs(166);
            //Quest quest = QuestRepository.GetInitialQuest(1);

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
    }
}
