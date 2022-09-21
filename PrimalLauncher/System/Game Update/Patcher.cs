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

using System.IO;

namespace PrimalLauncher
{
    public static class Patcher
    {       
        private static string GameInstallPath { get; set; } = Preferences.Instance.Options.GameInstallPath;
        
        private static readonly byte[] loginServerAddress = { 0xCA, 0xCF, 0xB8, 0xA2, 0x96, 0x96, 0x14, 0xCC, 0xF2, 0x9C, 0x9A, 0x8, 0x55, 0xAC, 0x7E, 0x35, 0x99, 0xB9, 0x57, }; //127.0.0.1/login (encoded string)
        private static readonly byte[] lobbyServerAddress = { 0x31, 0x32, 0x37, 0x2e, 0x30, 0x2e, 0x30, 0x2e, 0x31, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }; //[lobby01.ffxiv.com] -> [127.0.0.1        ] size: 0x11
        private static readonly byte[] updateServerAddress = { 0x31, 0x32, 0x37, 0x2e, 0x30, 0x2e, 0x30, 0x2e, 0x31, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }; //[lobby01.ffxiv.com] -> [127.0.0.1        ] size: 0x09
        private static readonly byte[] lobbyServerPort = { 0x35, 0x34, 0x39, 0x39, 0x37 }; //54997
        
        #region Executable patching methods
        private static bool PatchLoginExe()
        {
            try
            {
                //Load binary into memory              
                byte[] exeDataLogin = File.ReadAllBytes(GameInstallPath + @"/ffxivlogin.exe");

                //Save backup               
                File.WriteAllBytes(GameInstallPath + @"/ffxivlogin.exe.bak", exeDataLogin);

                //Write encoded localhost address to offset 0x53EA0 (old login server string offset)               
                using (MemoryStream memStream = new MemoryStream(exeDataLogin))
                {
                    using (BinaryWriter binaryWriter = new BinaryWriter(memStream))
                    {
                        binaryWriter.BaseStream.Seek(0x53EA0, SeekOrigin.Begin);
                        binaryWriter.Write(loginServerAddress);
                    }
                }

                //Save patched file                
                File.WriteAllBytes(GameInstallPath + @"/ffxivlogin.exe", exeDataLogin);

                return true;
            }
            catch
            {
                return false;
            }
            
        }

        private static bool PatchBootExe()
        {
            try
            {
                //Load binary into memory 
                byte[] exeDataBoot = File.ReadAllBytes(GameInstallPath + @"/ffxivboot.exe");

                //Save binary backup
                File.WriteAllBytes(GameInstallPath + @"/ffxivboot.exe.bak", exeDataBoot);

                using (MemoryStream memStream = new MemoryStream(exeDataBoot))
                {
                    using (BinaryWriter binaryWriter = new BinaryWriter(memStream))
                    {
                        //Change lobby server address
                        binaryWriter.BaseStream.Seek(0x00965d08, SeekOrigin.Begin);
                        binaryWriter.Write(lobbyServerAddress);

                        //Change update server address
                        binaryWriter.BaseStream.Seek(0x00966404, SeekOrigin.Begin);
                        binaryWriter.Write(updateServerAddress);
                    }
                }

                //Save patched file                
                File.WriteAllBytes(GameInstallPath + @"/ffxivboot.exe", exeDataBoot);

                return true;
            }
            catch 
            {
                return false;
            }            
        }

        private static bool PatchGameExe()
        {
            try
            {
                //Load binary into memory 
                byte[] exeDataGame = File.ReadAllBytes(GameInstallPath + @"/ffxivgame.exe");

                //Save binary backup
                File.WriteAllBytes(GameInstallPath + @"/ffxivgame.exe.bak", exeDataGame);

                using (MemoryStream memStream = new MemoryStream(exeDataGame))
                {
                    using (BinaryWriter binaryWriter = new BinaryWriter(memStream))
                    {
                        //Change lobby server address
                        binaryWriter.BaseStream.Seek(0x00b90110, SeekOrigin.Begin);
                        binaryWriter.Write(lobbyServerAddress);

                        //Change lobby server port
                        binaryWriter.BaseStream.Seek(0x00c50424, SeekOrigin.Begin);
                        binaryWriter.Write(lobbyServerPort);
                    }
                }

                //Save patched file                
                File.WriteAllBytes(GameInstallPath + @"/ffxivgame.exe", exeDataGame);

                return true;
            }
            catch
            {
                return false;
            }            
        }
        #endregion

        private static void WritePatchMessages()
        {
            //all update files were processed
            File.AppendAllText(Preferences.Instance.Options.GameInstallPath + "/game.ver", " [Patched by Primal Launcher]");
            File.AppendAllText(Preferences.Instance.Options.GameInstallPath + "/boot.ver", " [Patched by Primal Launcher]");
        }

        /// <summary>
        /// Patch executables
        /// </summary>
        public static void PatchBinaries()
        {
            if (GameInstallationChecker.HasWritePermission())
            {
                PatchBootExe();
                PatchLoginExe();
                PatchGameExe();
                WritePatchMessages();

                if (GameInstallationChecker.GameIsPatched())
                {
                    ucUpdate.Instance.TogglePatching(false);
                }
            }
            else
            {
                GameInstallationChecker.AskForAdminPermissions();
                return;
            }
        }      
    }
}
