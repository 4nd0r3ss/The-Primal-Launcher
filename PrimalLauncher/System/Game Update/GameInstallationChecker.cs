
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
using System;
using System.Diagnostics;
using System.IO;
using System.Security.Principal;
using System.Windows.Forms;

namespace PrimalLauncher
{
    internal class GameInstallationChecker
    {
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
            ContentLocation = "ffxiv/2d2a390f/vercheck.dat",
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
            XLatestVersion = "2012.09.19.0001",
            ContentLength = "0",
            KeepAlive = "timeout=5, max=99",
            Connection = "Keep-Alive",
            ContentType = "text/plain"
        };

        private static HttpPacket GameOutdated { get; } = new HttpPacket();

        public static byte[] CheckBootVer()
        {
            bool upToDate = true;
            Log.Instance.Info("Checking ffxivboot.exe version...");

            /* TODO: ffxivboot.exe version check */

            if (upToDate)
            {
                Log.Instance.Success("ffxivboot.exe is up-to-date!");
                return BootUpToDate.ToBytes();
            }
            else
            {
                Log.Instance.Warning("ffxivboot.exe is outdated! Starting update...");
                return BootOutdated.ToBytes();
            }
        }

        public static byte[] CheckGameVer()
        {
            bool upToDate = true;
            Log.Instance.Info("Checking game version...");

            /* TODO: ffxivgame.exe version check */

            if (upToDate)
            {
                Log.Instance.Success("You have the latest version (1.23b).");
                return GameUpToDate.ToBytes();
            }
            else
            {
                Log.Instance.Warning("The game is outdated! Starting update...");
                return GameOutdated.ToBytes();
            }
        }

        /// <summary>
        /// Searches for a valid game installation in the system. If not found, terminate the app.
        /// In case a game installation is found, verifies the game version. This method is called every time the app is started.
        /// </summary>
        /// <returns>True if game installation exists AND it is up-to-date.</returns>
        public static bool Check()
        {
            string gameInstallPath = Preferences.GetGameInstallPath();

            if (string.IsNullOrEmpty(gameInstallPath)) //if game installation was not found
            {
                MessageBox.Show("Could not find a FINAL FANTASY XIV 1.0 installation. Please install the game and try again.", "Primal Launcher");
                Environment.Exit(0); //there is not much left to do, so terminate app.
            }
            else
            {
                if (GameIsPatched())
                {
                    ucUpdate.Instance.ToggleUpdateSections(false, false, false);
                    return true;
                }
                else
                {
                    if (GameIsUpdated())
                        ucUpdate.Instance.ToggleUpdateSections(false, false, true);
                    else if (Downloader.Instance.CheckUpdateFiles())
                        ucUpdate.Instance.ToggleUpdateSections(false, true, false);
                    else
                        ucUpdate.Instance.ToggleUpdateSections(true, false, false);

                    return false;
                }      
            }

            return true; //game is updated      
        }

        

        /// <summary>
        /// Checks if the game installation was patched with local IP addresses.
        /// </summary>
        /// <returns></returns>
        public static bool GameIsPatched() => VersionFilesContainText("game", "[Patched by Primal Launcher]");

        /// <summary>
        /// 
        /// </summary>       
        /// <returns></returns>
        public static bool GameIsUpdated() => VersionFilesContainText("patch", "1.23b");

        /// <summary>
        /// Search for specific text in the version files.
        /// </summary>
        /// <param name="toSearch"></param>
        /// <returns></returns>
        private static bool VersionFilesContainText(string fileName, string toSearch)
        {
            string gameInstallPath = Preferences.Instance.Options.GameInstallPath;

            if (File.Exists(gameInstallPath + @"\" + fileName + ".ver"))
            {
                string fileContents = File.ReadAllText(gameInstallPath + @"\" + fileName + ".ver");

                if (fileContents.IndexOf(toSearch) < 0)
                    return false;
                else
                    return true; //found
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Game installation path write permission checker
        /// </summary>
        /// <returns></returns>
        public static bool HasWritePermission()
        {
            try
            {
                string testFile = Preferences.Instance.Options.GameInstallPath + @"/CheckPermission.dat";

                File.WriteAllText(testFile, "");

                if (File.Exists(testFile))
                    File.Delete(testFile);

                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }

        public static void AskForAdminPermissions()
        {
            //run as admin code from: https://stackoverflow.com/questions/6412896/giving-application-elevated-uac
            WindowsPrincipal pricipal = new WindowsPrincipal(WindowsIdentity.GetCurrent());
            bool hasAdministrativeRight = pricipal.IsInRole(WindowsBuiltInRole.Administrator);

            if (!hasAdministrativeRight)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.UseShellExecute = true;
                startInfo.WorkingDirectory = Environment.CurrentDirectory;
                startInfo.FileName = Application.ExecutablePath;
                startInfo.Verb = "runas";
                try
                {
                    Process p = Process.Start(startInfo);
                    Application.Exit();
                }
                catch (System.ComponentModel.Win32Exception ex)
                {
                    MessageBox.Show("This utility requires elevated priviledges to complete correctly.", "Error: UAC Authorization Required", MessageBoxButtons.OK);
                    Log.Instance.Error(ex.Message);
                    return;
                }
            }
        }
    }
}
