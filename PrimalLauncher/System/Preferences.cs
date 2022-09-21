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

using Microsoft.Win32;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;

namespace PrimalLauncher
{
    [Serializable]
    public class Options
    {
        public string GameInstallPath { get; set; }
        public string PatchDownloadPath { get; set; }
        public string ServerAddress { get; set; }
        public bool UseExternalHttpServer { get; set; }
        public bool ShowLoginPage { get; set; }
        public bool ChooseGameAccount { get; set; }
        public string UserFilesPath { get; set; }
        public string ServerRegion { get; set; }
        public byte LobbyOption { get; set; }
        public bool PrintPacketsToFile { get; set; }
    }

    public class Preferences
    {
        #region Class fields
        private static Preferences _instance = null;
        private readonly string _configFile = @"app_data.dat";
        public static string AppFolder = @"\Primal Launcher User Files\";
        #endregion
       
        private Preferences()
        {
            Configure();
        }      

        public static Preferences Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Preferences();
                }
                return _instance;
            }
        }
        public Options Options { get; set; }       

        #region Search game installation path in registry
        public static string GetGameInstallPath()
        {
            const string GAME_INSTALL_REGKEY = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{F2C4E6E0-EB78-4824-A212-6DF6AF0E8E82}";
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(GAME_INSTALL_REGKEY))
            {
                if(key != null)
                {
                    return key.GetValue("InstallLocation").ToString() + @"\" + key.GetValue("DisplayName").ToString() + @"\";
                }
                else
                {
                   return null;
                }                
            }                       
        }
        #endregion

        #region Load configuration file       
        public void LoadConfigFile()
        {               
            if (File.Exists(Options.UserFilesPath + AppFolder + _configFile))
            {               
                using (var fileStream = new FileStream(Options.UserFilesPath + AppFolder + _configFile, FileMode.Open))
                {
                    var bFormatter = new BinaryFormatter();

                    try
                    {
                        Options = (Options)bFormatter.Deserialize(fileStream);
                    }
                    catch
                    {
                        Log.Instance.Error("Options file deserialization error. Delete the file and try again.");
                    }
                    
                }
            }
            else //there is no config file
            {                
                Configure();                
                SaveConfigFile();
            }
        }
        #endregion

        #region Save options object to configuration file
        public void SaveConfigFile()
        {
            if (Directory.Exists(Options.UserFilesPath + AppFolder))
            {
                using (var fileStream = new FileStream(Options.UserFilesPath + AppFolder + _configFile, FileMode.Create))
                {
                    var bFormatter = new BinaryFormatter();
                    bFormatter.Serialize(fileStream, Options);
                }
            }
            else
            {
                Directory.CreateDirectory(Options.UserFilesPath + AppFolder);
                SaveConfigFile();
            }   

        }
        #endregion

        #region Fill out options object with default values
        private void Configure()
        {
            Options = new Options
            {
                GameInstallPath = GetGameInstallPath(),
                PatchDownloadPath = @"patches\",
                ServerAddress = "127.0.0.1",
                UseExternalHttpServer = false,
                ShowLoginPage = true,
                ChooseGameAccount = true,
                UserFilesPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                ServerRegion = "NA",
                PrintPacketsToFile = true,
                LobbyOption = 0
            };            
        }
        #endregion

        #region Show file select dialog and return selected path
        public string SelectFolder()
        {
            using (var fbd = new FolderBrowserDialog())
            {
                DialogResult folder = fbd.ShowDialog();  
                
                if (folder == DialogResult.OK && !string.IsNullOrWhiteSpace(fbd.SelectedPath))
                    return fbd.SelectedPath;               
                else               
                    return null;               
            }
        }
        #endregion
    }
}
