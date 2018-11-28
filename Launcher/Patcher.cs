using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace Launcher
{
    public static class Patcher
    {
        private static Log _log { get; set; } = Log.Instance;
        private static string GameInstPath { get; set; } = Preferences.Instance.Options.GameInstallPath;

        #region Check patched files
        public static void CheckPatched()
        {
            bool bakFilesExist = true;

            if (bakFilesExist)
            {

            }
            else
            {

            }

            throw new NotImplementedException();
        }

        #endregion

        #region Executable patching methods
        private static void PatchLoginExe()
        {           
            _log.Message("Starting patch process for ffxivlogin.exe...");

            if (HasWritePermission())
            {
                //127.0.0.1/login
                byte[] encodedString = { 0xCA, 0xCF, 0xB8, 0xA2, 0x96, 0x96, 0x14, 0xCC, 0xF2, 0x9C, 0x9A, 0x8, 0x55, 0xAC, 0x7E, 0x35, 0x99, 0xB9, 0x57, };

                //Open executable for patching
                _log.Message("Copying executable bytes into memory...");
                byte[] exeDataLogin = File.ReadAllBytes(GameInstPath + @"/ffxivlogin.exe");

                //Save backup
                _log.Message("Backing up file as ffxivlogin.exe.bak...");
                File.WriteAllBytes(GameInstPath + @"/ffxivlogin.exe.bak", exeDataLogin);

                //Write encoded localhost address to offset 0x53EA0 (old login string offset)
                _log.Message("Patching (redirect requests to 127.0.0.1)...");
                using (MemoryStream memStream = new MemoryStream(exeDataLogin))
                {
                    using (BinaryWriter binaryWriter = new BinaryWriter(memStream))
                    {
                        binaryWriter.BaseStream.Seek(0x53EA0, SeekOrigin.Begin);
                        binaryWriter.Write(encodedString);
                    }
                }

                //Save patched file
                _log.Message("Saving patched file...");
                //File.WriteAllBytes(GameInstPath + @"/ffxivlogin.exe", exeDataLogin);

                _log.Message("Done!");
            }
            else
            {
                _log.Message("you need write permission in the game installation folder.");
                _log.Message("Please close this program, right-click the shortcut and select 'Run as administrator'.");
                _log.Message("This operation is required only once. Aborting patching operation.");
            }  
        }

        private static void PatchBootExe()
        {
           throw new NotImplementedException();
        }

        private static void PatchGameExe()
        {
            //Change server ip
            //Offset: 0x00b90110 to 0x00b90120
            //[lobby01.ffxiv.com] -> [127.0.0.1        ] size: 0x11
            //Change server port
            //Offset: 0x00c50424 to 0x00c50428
            //[54996] to [54997] size: 0x5

            throw new NotImplementedException();
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
        private static bool HasWritePermission()
        {
            try
            {
                string testFile = GameInstPath + @"/CheckPermission.dat";

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
