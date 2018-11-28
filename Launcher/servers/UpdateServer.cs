using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Launcher
{
    public class UpdateServer
    {
        private static string FFXIVBootFilePath { get; } = Preferences.Instance.Options.GameInstallPath + @"\ffxivboot.exe";
        static bool _updateActive = false;       

        #region Class properties
        private static Log _log { get; set; } = Log.Instance;
        private static TcpListener UpdateListener { get; } = new TcpListener(IPAddress.Parse("127.0.0.1"), 54996);
        #endregion

        #region Initialize & Shutdown
        public static void Initialize()
        {
            if (!_updateActive)
            {
                _log.Message("Update server started @ port 54996.");
                UpdateListener.Start();
                _updateActive = true;
                UpdateDaemon();
            }               
        }      

        public static void Shutdown()
        {
            if (_updateActive)
            {
                _log.Message("Shutting down update server.");
                try{ UpdateListener.Stop(); }
                catch (Exception){}
                finally{                    
                    //HttpServer.Initialize();    
                }

                _updateActive = false;
            }
        }        
        #endregion

        #region Update server
        private static void UpdateDaemon()
        {           
            //if  ffxivboot.exe is not there for some reason, abort.
            if (File.Exists(FFXIVBootFilePath))
            {
                LaunchGame();
                
                while (_updateActive)
                {
                    try
                    {
                        using (TcpClient client = UpdateListener.AcceptTcpClient())
                        {
                            NetworkStream ns = client.GetStream();
                            string stringData = Packet.ToText(ns);

                            //Boot version check response
                            if (stringData.IndexOf("boot") > 0)
                                Packet.Send(ns, Updater.CheckBootVer());

                            //Game version check response
                            if (stringData.IndexOf("game") > 0)
                            {
                                Packet.Send(ns, Updater.CheckGameVer());
                                Shutdown();
                            }
                        }
                    }
                    catch (Exception) {
                        _log.Warning("Something went wrong with the update server. Please try again.");
                    }
                }
            }
            else
            {
                _log.Error("ffixvboot.exe not found! Please reinstall the game.");                
            }
            return;
        }
        #endregion

        private static void LaunchGame()
        {
            _log.Message("Launching ffxivboot.exe.");
            Process.Start(new ProcessStartInfo { FileName = FFXIVBootFilePath });
        }
    }
}
