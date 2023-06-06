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
using System.Drawing;
using System.Windows.Forms;

namespace PrimalLauncher
{
    public partial class ucUpdate : UserControl
    {
        public OutlinedFontLabel LblDownloadStat
        {
            get
            {
                return lblDownloadStat;
            }
        }

        public OutlinedFontLabel LblUpdate
        {
            get
            {
                return lblUpdate;
            }
        }

        public OutlinedFontLabel LblPatch
        {
            get
            {
                return lblPatch;
            }
        }

        public ProgressBar DownloadProgressBar
        {
            get
            {
                return progressBar;
            }
        }

        public ProgressBar UpdateProgreeBar
        {
            get
            {
                return progressBar1;
            }
        }

        public CheckBox ChkKeepFiles
        {
            get
            {
                return chkKeepFiles;
            }
        }

        public Button BtnDownloadCancel
        {
            get
            {
                return btnDownloadCancel;
            }
        }


        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= 0x2000000;
                return cp;
            }
        }

        private static ucUpdate _instance;
        public static ucUpdate Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ucUpdate();
                return _instance;
            }
        }        

        private ucUpdate()
        {
            InitializeComponent();
            lblUpdateSeparator1.BackColor = Color.FromArgb(64, 128, 128, 128);
            lblUpdateSeparator2.BackColor = Color.FromArgb(64, 128, 128, 128);
            lblUpdateSeparator3.BackColor = Color.FromArgb(64, 128, 128, 128);
        }

        public void TogglePatching(bool enable)
        {
            btnPatch.Enabled = enable;           

            if (!enable && GameInstallationChecker.GameIsPatched())
            {                
                lblPatch.Text = "Binaries are patched.";
            }                
        }

        public void ToggleUpdate(bool enable)
        {
            btnGameUpdate.Enabled = enable;
            chkKeepFiles.Enabled = enable;

            if (!enable)
                LblUpdate.Text = "Game is updated.";
        }

        public void ToggleDownload(bool enable)
        {
            btnDownload.Enabled = enable;
            //

            if (!enable)
            {
                lblDownloadStat.Text = "All update files downloaded.";
                btnDownloadCancel.Enabled = false;
            }                
        }

        public void ToggleUpdateSections(bool isDownloaded, bool isUpdated, bool isPatched)
        {
            Instance.ToggleDownload(isDownloaded);
            Instance.ToggleUpdate(isUpdated);
            Instance.TogglePatching(isPatched);
        }

        #region Button events
        private void btnDownload_Click(object sender, EventArgs e)
        {
            btnDownloadCancel.Enabled = true;
            btnDownload.Enabled = false;
            Downloader.Instance.DownloadFile();
        }

        private void btnPatch_Click(object sender, EventArgs e)
        {
            Patcher.PatchBinaries();
        }

        private void btnGameUpdate_Click(object sender, EventArgs e)
        {
            Updater.StartGameUpdate();
        }

        private void btnDownloadCancel_MouseClick(object sender, MouseEventArgs e)
        {
            btnDownloadCancel.Enabled = false;
            btnDownload.Enabled = true;
            Downloader.Instance.DownloadCancel();
        }

        private void btnDownload_MouseHover(object sender, EventArgs e)
        {
            btnDownload.BackgroundImage = Properties.Resources.button_on;
            btnDownload.Refresh();
        }

        private void btnDownload_MouseLeave(object sender, EventArgs e)
        {
            btnDownload.BackgroundImage = Properties.Resources.button_off;
            btnDownload.Refresh();
        }        

        private void btnDownloadCancel_MouseHover(object sender, EventArgs e)
        {
            btnDownloadCancel.BackgroundImage = Properties.Resources.button_on;
            btnDownloadCancel.Refresh();
        }

        private void btnDownloadCancel_MouseLeave(object sender, EventArgs e)
        {
            btnDownloadCancel.BackgroundImage= Properties.Resources.button_off;
            btnDownloadCancel.Refresh();
        }        

        private void btnGameUpdate_MouseHover(object sender, EventArgs e)
        {
            btnGameUpdate.BackgroundImage = Properties.Resources.button_on;
            btnGameUpdate.Refresh();
        }

        private void btnGameUpdate_MouseLeave(object sender, EventArgs e)
        {
            btnGameUpdate.BackgroundImage = Properties.Resources.button_off;
            btnGameUpdate.Refresh();
        }       

        private void btnPatch_MouseHover(object sender, EventArgs e)
        {
            btnPatch.BackgroundImage = Properties.Resources.button_on;
            btnPatch.Refresh();
        }

        private void btnPatch_MouseLeave(object sender, EventArgs e)
        {
            btnPatch.BackgroundImage = Properties.Resources.button_off;
            btnPatch.Refresh();
        }
        #endregion

        private void chkKeepFiles_CheckedChanged(object sender, EventArgs e)
        {

        }
    }
}
