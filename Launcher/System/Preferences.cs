using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Launcher
{
    [Serializable]
    public struct Options
    {
        public string GameInstallPath { get; set; }
        public string PatchDownloadPath { get; set; }
        public string ServerAddress { get; set; }
        public bool UseExternalHttpServer { get; set; }
        public bool ShowLoginPage { get; set; }
        public bool ChooseGameAccount { get; set; }
        public string UserFilesPath { get; set; }
        public string ServerRegion { get; set; }
    }

    public class Preferences
    {
        #region Class fields
        private static Preferences _instance = null;
        private readonly string _configFile = @"app_data.dat";
        public static string AppFolder = @"\Primal Launcher User Files\";
        #endregion
       
        private Preferences() { }       

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
        public string SearchGameInstallPath()
        {
            const string GAME_INSTALL_REGKEY = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall\{F2C4E6E0-EB78-4824-A212-6DF6AF0E8E82}";
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(GAME_INSTALL_REGKEY))
            {
                return key.GetValue("InstallLocation").ToString() + @"\" + key.GetValue("DisplayName").ToString() + @"\";
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
                    Options = (Options)bFormatter.Deserialize(fileStream);
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
        private void SaveConfigFile()
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
                GameInstallPath = SearchGameInstallPath(),
                PatchDownloadPath = @"patches\",
                ServerAddress = "127.0.0.1",
                UseExternalHttpServer = false,
                ShowLoginPage = true,
                ChooseGameAccount = true,
                UserFilesPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
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