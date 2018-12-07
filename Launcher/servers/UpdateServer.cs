using Launcher.Characters;
using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Launcher
{
    class UpdateServer : Server
    {       
        public const int PORT = 54996;       

        public UpdateServer()
        {
            LaunchGame();
            Start("Update", PORT);            
        }

        private static void LaunchGame()
        {
            _log.Message("Launching ffxivboot.exe.");
            Process.Start(new ProcessStartInfo { FileName = Preferences.Instance.Options.GameInstallPath + @"\ffxivboot.exe" });
        }

        public override void ProcessIncoming(StateObject state)
        {
            string stringData = Encoding.ASCII.GetString(state.buffer);           

            //Boot version check response
            if (stringData.IndexOf("boot") > 0)
                state.workSocket.Send(Updater.CheckBootVer());

            //Game version check response
            if (stringData.IndexOf("game") > 0)
            {
                state.workSocket.Send(Updater.CheckGameVer());
                ServerShutDown(); //sending shutdown after server's final task.
            }                           
        }

        public override void ServerTransition()
        {
            if(Preferences.Instance.Options.UseExternalHttpServer)
            {                              
                _log.Warning("Using external HTTP server. You can change this in the options pane.");               
                User user = UserRepository.Instance.GetUser("FFXIVPlayer", "FFXIVPlayer");  //if using external http server, load default user.
                Task.Run(() => { LobbyServer http = new LobbyServer(user); });                   
            }           
            else
            {
                Task.Run(() => { HttpServer http = new HttpServer(); });
            }    
        }
    }
}
