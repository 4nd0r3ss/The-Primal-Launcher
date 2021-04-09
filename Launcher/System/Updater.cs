﻿

using System;
using System.IO;
using System.Windows.Forms;

namespace Launcher
{
    class Updater
    {
        private static Log _log = Log.Instance;

        private static HttpPacket BootUpToDate { get; } = new HttpPacket()
        {
            HttpResponse = "204 No Content",
            ContentLocation = "ffxiv/2d2a390f/vercheck.dat",
            XRepository = "ffxiv/win32/release/boot",
            XPatchModule = "ZiPatch",
            XProtocol = "torrent",
            XInfoUrl = "http://www.example.com",
            XLatestVersion = "2010.09.18.0000",
            ContentLength = "0",
            KeepAlive = "timeout=5, max=100",
            Connection = "Keep-Alive",
            ContentType = "text/plain"
        };

        private static HttpPacket BootOutdated { get; } = new HttpPacket
        {
            HttpResponse = "200 OK",
            ContentLocation = "ffxiv/48eca647/vercheck.dat",
            ContentType = "multipart/mixed; boundary=477D80B1_38BC_41d4_8B48_5273ADB89CAC",
            XRepository = "ffxiv/win32/release/boot",
            XPatchModule = "ZiPatch",
            XProtocol = "torrent",
            XInfoUrl = "http://www.example.com",
            XLatestVersion = "2012.05.20.0000.0001",
            Connection = "keep-alive"
        };

        private static HttpPacket GameUpToDate { get; } = new HttpPacket
        {
            HttpResponse = "204 No Content",
            ContentLocation = "ffxiv/48eca647/vercheck.dat",
            XRepository = "ffxiv/win32/release/game",
            XPatchModule = "ZiPatch",
            XProtocol = "torrent",
            XInfoUrl = "http://www.example.com/",
            XLatestVersion = "2010.07.10.0000",
            ContentLength = "0",
            KeepAlive = "timeout=5, max=99",
            Connection = "Keep-Alive",
            ContentType = "text/plain"
        };

        private static HttpPacket GameOutdated { get; } = new HttpPacket();

        public static byte[] CheckBootVer()
        {
            bool upToDate = true;
            _log.Info("Checking ffxivboot.exe version...");

            /* TODO: ffxivboot.exe version check */

            if (upToDate)
            {
                _log.Success("ffxivboot.exe is up-to-date!");
                return BootUpToDate.ToBytes();
            }
            else
            {
                _log.Warning("ffxivboot.exe is outdated! Starting update...");
                return BootOutdated.ToBytes();
            }
        }        

        public static byte[] CheckGameVer()
        {
            bool upToDate = true;
            _log.Info("Checking game version...");

            /* TODO: ffxivgame.exe version check */

            if (upToDate)
            {
                _log.Success("You have the latest verion (1.23b).");
                return GameUpToDate.ToBytes();
            }
            else
            {
                _log.Warning("The game is outdated! Starting update...");
                return GameOutdated.ToBytes();
            }
        }

        /// <summary>
        /// Searches for a valid game installation in the system. If not found, terminate the app.
        /// In case a game installation is found, verifies the game version. This method is called every time the app is started.
        /// </summary>
        /// <returns>True if game installation exists AND it is up-to-date.</returns>
        public static bool GameInstallationOk()
        {
            string gameInstallPath = Preferences.Instance.Options.GameInstallPath;

            if (gameInstallPath.Equals("") || gameInstallPath == null) //if game installation was not found
            {
                MessageBox.Show("Primal Launcher could not find a Final Fantasy XIV 1.X game installation. Please install the game and run this application again.", "Primal Launcher");
                Environment.Exit(0); //there is not much left to do, so terminate app.
            }
            else
            {
               //game installation exists, check version files.
               string gameVersion = File.ReadAllText(gameInstallPath + @"\game.ver");
               string bootVersion = File.ReadAllText(gameInstallPath + @"\boot.ver");
                
               //if (gameVersion.Equals("2010.07.10.0000") || bootVersion.Equals("2010.07.10.0000"))               
                    //return false; //game is outdated   
            }

            return true; //game is updated      
        }
    }
}