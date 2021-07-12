using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace PrimalLauncher
{
    public static class Patcher
    {       
        private static string GameInstallPath { get; set; } = Preferences.Instance.Options.GameInstallPath;
        
        private static readonly byte[] loginServerAddress = { 0xCA, 0xCF, 0xB8, 0xA2, 0x96, 0x96, 0x14, 0xCC, 0xF2, 0x9C, 0x9A, 0x8, 0x55, 0xAC, 0x7E, 0x35, 0x99, 0xB9, 0x57, }; //127.0.0.1/login (encoded string)
        private static readonly byte[] lobbyServerAddress = { 0x31, 0x32, 0x37, 0x2e, 0x30, 0x2e, 0x30, 0x2e, 0x31, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }; //[lobby01.ffxiv.com] -> [127.0.0.1        ] size: 0x11
        private static readonly byte[] updateServerAddress = { 0x31, 0x32, 0x37, 0x2e, 0x30, 0x2e, 0x30, 0x2e, 0x31, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 }; //[lobby01.ffxiv.com] -> [127.0.0.1        ] size: 0x09
        private static readonly byte[] lobbyServerPort = { 0x35, 0x34, 0x39, 0x39, 0x37 }; //54997

        #region Check patched files
        public static bool GameIsPatched()
        {
            //TODO: change this method to verify the dates of the files instead.
            bool bakFilesExist = false;

            bakFilesExist = File.Exists(GameInstallPath + "/ffxivlogin.exe.bak");
            bakFilesExist = File.Exists(GameInstallPath + "/ffxivboot.exe.bak");
            bakFilesExist = File.Exists(GameInstallPath + "/ffxivgame.exe.bak");

            return bakFilesExist;
        }

        #endregion

        #region Executable patching methods
        private static void PatchLoginExe()
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
        }

        private static void PatchBootExe()
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
        }

        private static void PatchGameExe()
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
        }
        #endregion

        #region Patch process
        public static void PatchExecutableFiles()
        {
            PatchBootExe();
            PatchLoginExe();
            PatchGameExe();
        }
        #endregion

        #region Game installation path write permission checker
        public static bool HasWritePermission()
        {
            try
            {
                string testFile = GameInstallPath + @"/CheckPermission.dat";

                File.WriteAllText(testFile, "");

                if(File.Exists(testFile))
                    File.Delete(testFile);

                return true;
            }
            catch (UnauthorizedAccessException)
            {
                return false;
            }
        }
        #endregion
    }
}
