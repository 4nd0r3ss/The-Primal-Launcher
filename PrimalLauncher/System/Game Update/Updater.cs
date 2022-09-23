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

using Ionic.Zlib;
using System;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace PrimalLauncher
{
    /// <summary>
    /// The following code is a direct translation from C++ code found in Seventh Umbral Server.
    /// All credit goes to the original author.
    /// Seventh Umbral Server website: http://http://seventhumbral.org/
    /// </summary>
    public static class Updater
    {     
        public static void StartGameUpdate()
        {     
            ucUpdate.Instance.UpdateProgreeBar.Value = 0;
            ucUpdate.Instance.LblUpdate.Text = "Starting game update...";
            string filesPath = Preferences.Instance.AppFolder + @"\";

            if (!GameInstallationChecker.HasWritePermission())
            {
                GameInstallationChecker.AskForAdminPermissions();
                return;
            }                

            foreach (var file in Downloader.UpdateFiles)
            {
                string currentFilePath = filesPath + file.Key.Replace('/', '\\');
                ucUpdate.Instance.LblUpdate.Text = "Applying file " + file.Key.Replace('/', '\\');                

                //try
                //{
                using (FileStream fs = File.Open(currentFilePath, FileMode.Open))
                {
                    fs.Seek(0, SeekOrigin.Begin);
                    byte[] buffer = new byte[0x10];
                    fs.Read(buffer, 0, buffer.Length);
                    string fileType = Encoding.ASCII.GetString(buffer);

                    if (fileType.IndexOf("ZIPATCH") < 0)
                        throw new Exception("Bad file header. Download update files again.");

                    fs.Seek(0x10, SeekOrigin.Begin);

                    using (MemoryStream ms = new MemoryStream())
                    {
                        fs.CopyTo(ms);
                        ms.Seek(0, SeekOrigin.Begin);

                        BinaryReader br = new BinaryReader(ms);

                        //while there are bytes to read
                        while (br.BaseStream.Position != br.BaseStream.Length)
                        {
                            string operation = Encoding.ASCII.GetString(BitConverter.GetBytes(br.ReadUInt32()));

                            switch (operation)
                            {
                                case "ETRY":
                                    ProcessETRY(ref br);
                                    break;
                                case "FHDR":
                                    ProcessFHDR(ref br);
                                    break;
                                case "HIST":
                                    ProcessHIST(ref br);
                                    break;
                                case "APLY":
                                    ProcessAPLY(ref br);
                                    break;
                                case "ADIR":
                                    ProcessADIR(ref br);
                                    break;
                                case "DIFF":
                                    ProcessDIFF(ref br);
                                    break;
                                case "DELD":
                                    ProcessDELD(ref br);
                                    break;
                            }
                        }

                        ucUpdate.Instance.UpdateProgreeBar.Increment(2);
                    }
                }
                //break;
                //}catch(Exception e){ throw e; }       
            }

            ucUpdate.Instance.LblUpdate.Text = "Game is updated to version 1.23b.";

            if (!ucUpdate.Instance.ChkKeepFiles.Checked)
            {
                DirectoryInfo dir = new DirectoryInfo(Preferences.Instance.Options.PatchDownloadPath);

                foreach (DirectoryInfo subDir in dir.GetDirectories())
                {
                    subDir.Delete(true);
                }
            }

            ucUpdate.Instance.ToggleUpdate(false);
            ucUpdate.Instance.TogglePatching(true);
        }

        private static uint SwapEndian(uint number)
        {
            byte[] data = BitConverter.GetBytes(number);
            return (uint)(data[0] << 24 | data[1] << 16 | data[2] << 8 | data[3]);
        }

        private static string ReadDirectory(ref BinaryReader br)
        {
            uint pathSize = SwapEndian(br.ReadUInt32());
            string pathToCreate = Encoding.ASCII.GetString(br.ReadBytes((int)pathSize));

            br.ReadUInt64(); //unknown

            if ((br.BaseStream.Length - br.BaseStream.Position) >= 0x08) //bug fix
                br.ReadUInt64(); //unknown

            return Preferences.Instance.Options.GameInstallPath + pathToCreate;
        }

        private static void ProcessETRY(ref BinaryReader br)
        {
            uint pathSize = SwapEndian(br.ReadUInt32());
            string filePath = Encoding.ASCII.GetString(br.ReadBytes((int)pathSize));
            int itemCount = (int)SwapEndian(br.ReadUInt32());

            for (int i = 0; i < itemCount; i++)
            {
                uint hashMode = SwapEndian(br.ReadUInt32());

                byte[] srcFileHash = br.ReadBytes(0x14);
                byte[] dstFileHash = br.ReadBytes(0x14);

                bool isCompressed = br.ReadUInt32() == 0x5a;

                uint compressedFileSize = SwapEndian(br.ReadUInt32());
                uint previousFileSize = SwapEndian(br.ReadUInt32());
                uint newFileSize = SwapEndian(br.ReadUInt32());

                byte[] fileData = br.ReadBytes((int)compressedFileSize);

                //unzip if zipped
                if (isCompressed)
                    fileData = ZlibStream.UncompressBuffer(fileData);

                //destination path with filename
                string fileDestinationPath = Preferences.Instance.Options.GameInstallPath + filePath.Replace('/', '\\');

                try
                {
                    //destination path without filename               
                    string destinationPath = fileDestinationPath.Substring(0, fileDestinationPath.LastIndexOf('\\'));

                    //if curent file destination folder does not exist, create it.
                    if (!Directory.Exists(destinationPath))
                        Directory.CreateDirectory(destinationPath);

                    //delete file if exists
                    if (File.Exists(fileDestinationPath))
                        File.Delete(fileDestinationPath);

                    //Save file.
                    File.WriteAllBytes(fileDestinationPath, fileData);
                }
                catch (Exception e) { throw e; }
            }
        }

        private static void ProcessFHDR(ref BinaryReader br) => br.ReadInt32();

        private static void ProcessHIST(ref BinaryReader br)
        {
            br.ReadInt32();  //unknown
            br.ReadUInt64(); //unknown
            br.ReadUInt32(); //unknown
            br.ReadUInt32(); //unknown
        }

        private static void ProcessAPLY(ref BinaryReader br)
        {
            br.ReadInt32();  //unknown
            br.ReadUInt64(); //unknown
            br.ReadUInt32(); //unknown
            br.ReadUInt32(); //unknown
        }

        private static void ProcessADIR(ref BinaryReader br)
        {
            string fileDestinationPath = ReadDirectory(ref br);

            if (!Directory.Exists(fileDestinationPath))
                Directory.CreateDirectory(fileDestinationPath);
        }

        private static void ProcessDIFF(ref BinaryReader br)
        {
            br.ReadInt32();  //unknown
            br.ReadUInt64(); //unknown
            br.ReadUInt32(); //unknown
            br.ReadUInt32(); //unknown
        }

        private static void ProcessDELD(ref BinaryReader br)
        {
            string fileDestinationPath = ReadDirectory(ref br);

            if (Directory.Exists(fileDestinationPath))
                ClearFolder(fileDestinationPath);
        }

        private static void ClearFolder(string folderName)
        {
            DirectoryInfo dir = new DirectoryInfo(folderName);

            foreach (FileInfo fi in dir.GetFiles())
                fi.Delete();

            foreach (DirectoryInfo di in dir.GetDirectories())
            {
                ClearFolder(di.FullName);
                di.Delete();
            }
        }
    }
}
