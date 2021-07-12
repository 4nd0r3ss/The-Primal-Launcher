using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PrimalLauncher
{
    public partial class Downloader : Form
    {
        private readonly string _downloadURL = "http://ffxivpatches.s3.amazonaws.com/";
        private int _downloadIndex = 0;
        private readonly List<KeyValuePair<string, uint>> _files = new List<KeyValuePair<string, uint>>
            {
                new KeyValuePair<string, uint>("2d2a390f/patch/D2010.09.18.0000.patch", 0x0055046),
                new KeyValuePair<string, uint>("48eca647/patch/D2010.09.19.0000.patch", 444398866),
                new KeyValuePair<string, uint>("48eca647/patch/D2010.09.23.0000.patch", 6907277 ),
                new KeyValuePair<string, uint>("48eca647/patch/D2010.09.28.0000.patch", 18803280),
                new KeyValuePair<string, uint>("48eca647/patch/D2010.10.07.0001.patch", 19226330),
                new KeyValuePair<string, uint>("48eca647/patch/D2010.10.14.0000.patch", 19464329),
                new KeyValuePair<string, uint>("48eca647/patch/D2010.10.22.0000.patch", 19778252),
                new KeyValuePair<string, uint>("48eca647/patch/D2010.10.26.0000.patch", 19778391),
                new KeyValuePair<string, uint>("48eca647/patch/D2010.11.25.0002.patch", 250718651),
                new KeyValuePair<string, uint>("48eca647/patch/D2010.11.30.0000.patch", 6921623 ),
                new KeyValuePair<string, uint>("48eca647/patch/D2010.12.06.0000.patch", 7158904 ),
                new KeyValuePair<string, uint>("48eca647/patch/D2010.12.13.0000.patch", 263311481),
                new KeyValuePair<string, uint>("48eca647/patch/D2010.12.21.0000.patch", 7521358 ),
                new KeyValuePair<string, uint>("48eca647/patch/D2011.01.18.0000.patch", 9954265 ),
                new KeyValuePair<string, uint>("48eca647/patch/D2011.02.01.0000.patch", 11632816),
                new KeyValuePair<string, uint>("48eca647/patch/D2011.02.10.0000.patch", 11714096),
                new KeyValuePair<string, uint>("48eca647/patch/D2011.03.01.0000.patch", 77464101),
                new KeyValuePair<string, uint>("48eca647/patch/D2011.03.24.0000.patch", 108923937),
                new KeyValuePair<string, uint>("48eca647/patch/D2011.03.30.0000.patch", 109010880),
                new KeyValuePair<string, uint>("48eca647/patch/D2011.04.13.0000.patch", 341603850),
                new KeyValuePair<string, uint>("48eca647/patch/D2011.04.21.0000.patch", 343579198),
                new KeyValuePair<string, uint>("48eca647/patch/D2011.05.19.0000.patch", 344239925),
                new KeyValuePair<string, uint>("48eca647/patch/D2011.06.10.0000.patch", 344334860),
                new KeyValuePair<string, uint>("48eca647/patch/D2011.07.20.0000.patch", 584926805),
                new KeyValuePair<string, uint>("48eca647/patch/D2011.07.26.0000.patch", 7649141 ),
                new KeyValuePair<string, uint>("48eca647/patch/D2011.08.05.0000.patch", 152064532),
                new KeyValuePair<string, uint>("48eca647/patch/D2011.08.09.0000.patch", 8573687 ),
                new KeyValuePair<string, uint>("48eca647/patch/D2011.08.16.0000.patch", 6118907 ),
                new KeyValuePair<string, uint>("48eca647/patch/D2011.10.04.0000.patch", 677633296),
                new KeyValuePair<string, uint>("48eca647/patch/D2011.10.12.0001.patch", 28941655),
                new KeyValuePair<string, uint>("48eca647/patch/D2011.10.27.0000.patch", 29179764),
                new KeyValuePair<string, uint>("48eca647/patch/D2011.12.14.0000.patch", 374617428),
                new KeyValuePair<string, uint>("48eca647/patch/D2011.12.23.0000.patch", 22363713),
                new KeyValuePair<string, uint>("48eca647/patch/D2012.01.18.0000.patch", 48998794),
                new KeyValuePair<string, uint>("48eca647/patch/D2012.01.24.0000.patch", 49126606),
                new KeyValuePair<string, uint>("48eca647/patch/D2012.01.31.0000.patch", 49536396),
                new KeyValuePair<string, uint>("48eca647/patch/D2012.03.07.0000.patch", 320630782),
                new KeyValuePair<string, uint>("48eca647/patch/D2012.03.09.0000.patch", 8312819 ),
                new KeyValuePair<string, uint>("48eca647/patch/D2012.03.22.0000.patch", 22027738),
                new KeyValuePair<string, uint>("48eca647/patch/D2012.03.29.0000.patch", 8322920 ),
                new KeyValuePair<string, uint>("48eca647/patch/D2012.04.04.0000.patch", 8678570 ),
                new KeyValuePair<string, uint>("48eca647/patch/D2012.04.23.0001.patch", 289511791),
                new KeyValuePair<string, uint>("48eca647/patch/D2012.05.08.0000.patch", 27266546),
                new KeyValuePair<string, uint>("48eca647/patch/D2012.05.15.0000.patch", 27416023),
                new KeyValuePair<string, uint>("48eca647/patch/D2012.05.22.0000.patch", 27742726),
                new KeyValuePair<string, uint>("48eca647/patch/D2012.06.06.0000.patch", 129984024),
                new KeyValuePair<string, uint>("48eca647/patch/D2012.06.19.0000.patch", 133434217),
                new KeyValuePair<string, uint>("48eca647/patch/D2012.06.26.0000.patch", 133581048),
                new KeyValuePair<string, uint>("48eca647/patch/D2012.07.21.0000.patch", 253224781),
                new KeyValuePair<string, uint>("48eca647/patch/D2012.08.10.0000.patch", 42851112),
                new KeyValuePair<string, uint>("48eca647/patch/D2012.09.06.0000.patch", 20566711),
                new KeyValuePair<string, uint>("48eca647/patch/D2012.09.19.0001.patch", 20874726)
            };


        public Downloader()
        {
            InitializeComponent();
        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void label3_Click(object sender, EventArgs e)
        {

        }

        private void btnDownload_Click(object sender, EventArgs e) => DownloadFile();

        private void DownloadFile()
        {
            string downloadPath = Preferences.Instance.Options.UserFilesPath + Preferences.AppFolder + _files[_downloadIndex].Key.Substring(0, _files[_downloadIndex].Key.LastIndexOf(@"/")).Replace(@"/", @"\") + @"\";

            if (!Directory.Exists(downloadPath))
                Directory.CreateDirectory(downloadPath);

            if (!File.Exists(Preferences.AppFolder + _files[_downloadIndex].Key.Replace(@"/", @"\")))
            {
                lblDownloadStat.Text = "Downloading file " + (_downloadIndex +1) + @"/" + _files.Count + "  - " + _files[_downloadIndex].Key;

                using(WebClient wc = new WebClient())
                {
                    wc.DownloadProgressChanged += ChangeProgressBarValue;
                    wc.DownloadFileCompleted += DownloadComplete;
                    wc.DownloadFileAsync(new Uri(_downloadURL + _files[_downloadIndex].Key), Preferences.Instance.Options.UserFilesPath + Preferences.AppFolder + _files[_downloadIndex].Key.Replace(@"/", @"\"));
                }              
            }
        }

        private void ChangeProgressBarValue(object sender, DownloadProgressChangedEventArgs e) => progressBar.Value = e.ProgressPercentage;

        private void DownloadComplete(object sender, AsyncCompletedEventArgs e)
        {
            progressBar.Value = 0;

            if (_downloadIndex < _files.Count)
            {
                _downloadIndex++;
                DownloadFile();
            }
            else
            {

            }          
        }

        private void btnDownloadCancel_Click(object sender, EventArgs e)
        {
            
        }

        private void lblDownloadStat_Click(object sender, EventArgs e)
        {

        }

        private void progressBar_Click(object sender, EventArgs e)
        {

        }
    }
}
