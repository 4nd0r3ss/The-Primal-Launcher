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
    }

    public class Preferences
    {
        #region Class fields
        private static Preferences _instance = null;
        private readonly string _configFile = @"app_data.dat";
        private Options _options;
        #endregion

        #region Constructor
        private Preferences() => LoadConfigFile();
        #endregion

        #region Class properties
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
        public Options Options => _options;
        #endregion

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
            if (File.Exists(_configFile))
            {               
                using (var fileStream = new FileStream(_configFile, FileMode.Open))
                {
                    var bFormatter = new BinaryFormatter();
                    _options = (Options)bFormatter.Deserialize(fileStream);
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
            using (var fileStream = new FileStream(_configFile, FileMode.Create))
            {
                var bFormatter = new BinaryFormatter();
                bFormatter.Serialize(fileStream, _options);
            }
        }
        #endregion

        #region Fills up options object with default values
        private void Configure()
        {
            _options.GameInstallPath = SearchGameInstallPath();
            _options.PatchDownloadPath = @"patches\";
            _options.ServerAddress = "127.0.0.1";
            _options.UseExternalHttpServer = false;
            _options.ShowLoginPage = true;
            _options.ChooseGameAccount = true;
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