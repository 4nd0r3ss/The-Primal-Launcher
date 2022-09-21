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

using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace PrimalLauncher
{
    class UpdateServer : Server
    {              
        public UpdateServer()
        {
            Task.Run(() => { ProcessIncoming(); });
            LaunchGame();
            Start("Update", 54996);            
        }

        private static void LaunchGame()
        {
            Log.Instance.Info("Launching ffxivboot.exe.");
            string path = Preferences.Instance.Options.GameInstallPath;
            Process.Start(new ProcessStartInfo { FileName = path + @"\ffxivboot.exe" });
        }       

        public override void ProcessIncoming()
        {  
            while(_listening)
            {
                if (_connection.bufferQueue.Count > 0)
                {
                    string stringData = Encoding.ASCII.GetString(_connection.bufferQueue.Dequeue());

                    //Boot version check response
                    if (stringData.IndexOf("boot") > 0)
                        _connection.Send(GameInstallationChecker.CheckBootVer());

                    //Game version check response
                    if (stringData.IndexOf("game") > 0)
                    {
                        _connection.Send(GameInstallationChecker.CheckGameVer());
                        ServerShutDown(); //sending shutdown after server's final task.
                    }
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
