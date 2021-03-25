using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Launcher
{
    public partial class ucOptions : UserControl
    {       
        private static ucOptions _instance;
        public static ucOptions Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ucOptions();
                return _instance;
            }
        }
        private ucOptions()
        {
            InitializeComponent();

            //Get paths from configuration file.  
            txtGameInstallPath.Text = Preferences.Instance.Options.GameInstallPath;
            txtPatchPath.Text = Preferences.Instance.Options.UserFilesPath + Preferences.AppFolder;
        }            
        
        private void btnChangeGameInstallPath_Click_1(object sender, EventArgs e)
        {
            string path = Preferences.Instance.SelectFolder();

            if (path != null)
                txtGameInstallPath.Text = path;
        }

        private void btnChangePatchPath_Click_1(object sender, EventArgs e)
        {
            string path = Preferences.Instance.SelectFolder();

            if (path != null)
                txtPatchPath.Text = path;
        }

        private void BtnPatchExe_Click(object sender, EventArgs e) => Task.Run(() => { Patcher.PatchExecutableFiles(); });
    }
}
