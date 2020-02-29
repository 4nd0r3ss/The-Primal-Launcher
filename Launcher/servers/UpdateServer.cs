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
            Task.Run(() =>
            {
                while (_listening)               
                    ProcessIncoming(ref _connection);                
            });

            LaunchGame();
            Start("Update", PORT);            
        }

        private static void LaunchGame()
        {
            _log.Info("Launching ffxivboot.exe.");
            Process.Start(new ProcessStartInfo { FileName = Preferences.Instance.Options.GameInstallPath + @"\ffxivboot.exe" });
        }       

        public override void ProcessIncoming(ref StateObject _connection)
        {  
            while(_connection.bufferQueue.Count > 0)
            {
                string stringData = Encoding.ASCII.GetString(_connection.bufferQueue.Dequeue());

                //Boot version check response
                if (stringData.IndexOf("boot") > 0)
                    _connection.Send(Updater.CheckBootVer());

                //Game version check response
                if (stringData.IndexOf("game") > 0)
                {
                    _connection.Send(Updater.CheckGameVer());
                    ServerShutDown(); //sending shutdown after server's final task.
                }
            }                            
        }

        public override void ServerTransition()
        {
            if(Preferences.Instance.Options.UseExternalHttpServer)
            {                              
                _log.Warning("Using external HTTP server. You can change this in the options pane.");               
                UserFactory.Instance.LoadUser("FFXIVPlayer", "FFXIVPlayer");  //if using external http server, load default user.
                Task.Run(() => { new LobbyServer(); });                   
            }           
            else
            {
                Task.Run(() => { new HttpServer(); });
            }    
        }
    }
}
