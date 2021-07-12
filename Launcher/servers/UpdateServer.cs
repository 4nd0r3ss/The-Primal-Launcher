using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PrimalLauncher
{
    class UpdateServer : Server
    {              
        public UpdateServer()
        {
            Task.Run(() =>
            {
                while (_listening)               
                    ProcessIncoming(ref _connection);                
            });

            LaunchGame();
            Start("Update", 54996);            
        }

        private static void LaunchGame()
        {
            Log.Instance.Info("Launching ffxivboot.exe.");
            string path = Preferences.Instance.Options.GameInstallPath;
            Process.Start(new ProcessStartInfo { FileName = path + @"\ffxivboot.exe" });
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
                    //ServerShutDown(); //sending shutdown after server's final task.
                }
            }
        }

        public override void ServerTransition()
        {
            //if(Preferences.Instance.Options.UseExternalHttpServer)
            //{                              
            //    Log.Instance.Warning("Using external HTTP server. You can change this in the options pane.");               
            //    UserRepository.Instance.LoadUser("FFXIVPlayer", "FFXIVPlayer");  //if using external http server, load default user.
            //    //Task.Run(() => { new LobbyServer(); });                   
            //}           
            //else
            //{
            //    //Task.Run(() => { new HttpServer(); });
            //}    
        }
    }
}
