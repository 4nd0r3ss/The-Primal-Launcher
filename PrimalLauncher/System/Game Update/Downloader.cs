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
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Net;

namespace PrimalLauncher
{
    public class Downloader
    {
        private static Downloader _instance;
        public static Downloader Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new Downloader();
                return _instance;
            }
        }

        private readonly string _downloadURL = "http://ffxivpatches.s3.amazonaws.com/";
        private int _downloadIndex = 0;
        private bool _keepDownloading = true;
        private WebClient _webClient;
        private string _downloadMessage = "Click the download button to resume downloading the update files.";
        private List<KeyValuePair<string, uint>> _filesToDownload = new List<KeyValuePair<string, uint>>();
        private static readonly List<KeyValuePair<string, uint>> _updateFiles = new List<KeyValuePair<string, uint>>
        {
            new KeyValuePair<string, uint>("2d2a390f/patch/D2010.09.18.0000.patch", 5571687),
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

        public static List<KeyValuePair<string, uint>> UpdateFiles
        {
            get
            {
                return _updateFiles;
            }
        }

        public void DownloadFile()
        {
            if (!_keepDownloading) return; //cancel button was clicked

            string downloadPath = Preferences.Instance.Options.UserFilesPath + Preferences.AppFolder + _filesToDownload[_downloadIndex].Key.Substring(0, _filesToDownload[_downloadIndex].Key.LastIndexOf(@"/")).Replace(@"/", @"\") + @"\";

            if (!Directory.Exists(downloadPath))
                Directory.CreateDirectory(downloadPath);

            if (!File.Exists(Preferences.AppFolder + _filesToDownload[_downloadIndex].Key.Replace(@"/", @"\")))
            {
                ucUpdate.Instance.LblDownloadStat.Text = "Downloading file " + (_downloadIndex + 1) + @"/" + _filesToDownload.Count + "  - " + _filesToDownload[_downloadIndex].Key;

                using (_webClient = new WebClient())
                {
                    _webClient.DownloadProgressChanged += ChangeProgressBarValue;
                    _webClient.DownloadFileCompleted += DownloadComplete;
                    _webClient.DownloadFileAsync(new Uri(_downloadURL + _filesToDownload[_downloadIndex].Key), Preferences.Instance.Options.UserFilesPath + Preferences.AppFolder + _filesToDownload[_downloadIndex].Key.Replace(@"/", @"\"));
                }
            }
        }

        private static void ChangeProgressBarValue(object sender, DownloadProgressChangedEventArgs e) => ucUpdate.Instance.DownloadProgressBar.Value = e.ProgressPercentage;

        private void DownloadComplete(object sender, AsyncCompletedEventArgs e)
        {
            ucUpdate.Instance.DownloadProgressBar.Value = 0;

            if (_downloadIndex < (_filesToDownload.Count - 1))
            {
                _downloadIndex++;
                DownloadFile();
            }
            else
            {
                //if all files are downloaded and healthy
                if (CheckUpdateFiles())
                {
                    ucUpdate.Instance.ToggleDownload(false);
                    ucUpdate.Instance.ToggleUpdate(true);
                }                               
            }
        }

        public bool CheckUpdateFiles()
        {
            ucUpdate.Instance.LblDownloadStat.Text = "Checking downloaded files...";
            long totalBytesToDownload = 0;
            _filesToDownload.Clear();

            foreach (var file in _updateFiles)
            {
                string filePath = Preferences.Instance.Options.UserFilesPath + Preferences.AppFolder + @"\" + file.Key;

                if (File.Exists(filePath)) //if file was already dowloaded
                {
                    //check size
                    long fileLength = new FileInfo(filePath).Length;

                    if (fileLength == file.Value) //if file is there and the size is ok, leave it alone.
                        continue;
                    else //size different from expected, delete it and download again.
                    {
                        File.Delete(filePath);
                        _filesToDownload.Add(file);
                        totalBytesToDownload += (int)file.Value;
                    }
                }
                else
                {
                    _filesToDownload.Add(file); //if file was not downloaded, put it into dl list.
                    totalBytesToDownload += (int)file.Value;
                }
            }

            if (_filesToDownload.Count == 0)
            {                
                return true;
            }
            else
            {
                double downloadSize = (((totalBytesToDownload / 1024) / 1024));
                string downloadSizeUnit = "MB";

                if (downloadSize > 1024)
                {
                    downloadSize = Math.Round((downloadSize / 1024), 2);
                    downloadSizeUnit = "GB";
                }

                ucUpdate.Instance.LblDownloadStat.Text = "You need to download " + _filesToDownload.Count + " files totalizing " + downloadSize.ToString() + downloadSizeUnit + ".";
                return false;
            }           
        }

        public void DownloadCancel()
        {
            _webClient.CancelAsync();
            _keepDownloading = false;
            ucUpdate.Instance.LblDownloadStat.Text = _downloadMessage;
        }
    }
}
