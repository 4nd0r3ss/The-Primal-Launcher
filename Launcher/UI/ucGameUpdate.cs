using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Net;
using Ionic.Zlib;

namespace Launcher
{
    public partial class ucGameUpdate : UserControl
    {
        private static ucGameUpdate _instance;
        public static ucGameUpdate Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ucGameUpdate();
                return _instance;
            }
        }

        private readonly string _downloadURL = "http://ffxivpatches.s3.amazonaws.com/";
        private int _downloadIndex = -1;
        private bool _keepDownloading = true;
        private WebClient _webClient;
        private readonly List<KeyValuePair<string, uint>> _filesToDownload = new List<KeyValuePair<string, uint>>
        {
            new KeyValuePair<string, uint>("2d2a390f/patch/D2010.09.18.0000.patch", 5571687 ),
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
        private List<KeyValuePair<string, uint>> _files = new List<KeyValuePair<string, uint>>();

        private string _downloadMessage = "Click the download button to resume downloading the update files.";

        public ucGameUpdate()
        {
            InitializeComponent();
            CheckUpdateFiles();
        }

        private void CheckUpdateFiles()
        {
            lblDownloadStat.Text = "Checking downloaded files...";
            long totalBytesToDownload = 0;

            foreach (var file in _filesToDownload)
            {
                string filePath = Preferences.Instance.Options.UserFilesPath + Preferences.AppFolder + @"update\" + file.Key;

                if (File.Exists(filePath)) //if file was already dowloaded
                {
                    //check size
                    long fileLength = new FileInfo(filePath).Length;

                    if (fileLength == file.Value) //if file is there and the size is ok, leave it alone.
                        continue;
                    else //size different from expected, delete it and download again.
                    {
                        File.Delete(filePath);
                        _files.Add(file);
                        totalBytesToDownload += (int)file.Value;
                    }
                }
                else
                {
                    _files.Add(file); //if file was not downloaded, put it into dl list.
                    totalBytesToDownload += (int)file.Value;
                }                   
            }

            if (_files.Count == 0)
                UpdateReady();
            else
            {
                double downloadSize = (((totalBytesToDownload / 1024) / 1024));
                string downloadSizeUnit = "MB";

                if (downloadSize > 1024)
                {
                    downloadSize = Math.Round((downloadSize / 1024), 2);
                    downloadSizeUnit = "GB";
                }                
                
                lblDownloadStat.Text = "You need to download " + _files.Count + " files totalizing " + downloadSize.ToString() + downloadSizeUnit;
            }
        }

        private void ucGameUpdate_Load(object sender, EventArgs e){}

        private void DownloadFile()
        {
            _downloadIndex++;
            progressBar.Value = 0;


            if (!_keepDownloading) return; //cancel button was clicked

            string downloadPath = Preferences.Instance.Options.UserFilesPath + Preferences.AppFolder + @"update\" + _files[_downloadIndex].Key.Substring(0, _files[_downloadIndex].Key.LastIndexOf(@"/")).Replace(@"/", @"\") + @"\";

            if (!Directory.Exists(downloadPath))
                Directory.CreateDirectory(downloadPath);

            if (!File.Exists(Preferences.AppFolder + _files[_downloadIndex].Key.Replace(@"/", @"\")))
            {
                lblDownloadStat.Text = "Downloading file " + (_downloadIndex + 1) + @"/" + _files.Count + "  - " + _files[_downloadIndex].Key;

                using (_webClient = new WebClient())
                {
                    _webClient.DownloadProgressChanged += ChangeProgressBarValue;
                    _webClient.DownloadFileCompleted += DownloadComplete;
                    _webClient.DownloadFileAsync(new Uri(_downloadURL + _files[_downloadIndex].Key), Preferences.Instance.Options.UserFilesPath + Preferences.AppFolder + @"update\"  + _files[_downloadIndex].Key.Replace(@"/", @"\"));
                }
            }
        }

        private void ChangeProgressBarValue(object sender, DownloadProgressChangedEventArgs e) => progressBar.Value = e.ProgressPercentage;

        private void DownloadComplete(object sender, AsyncCompletedEventArgs e)
        {
            if (_downloadIndex == _files.Count - 1)
            {
                UpdateReady();
                return;
            }
            else
            {
                DownloadFile();
            }      
        }            
       
        private void btnDownload_Click(object sender, EventArgs e) => DownloadFile();

        private void btnDownloadCancel_Click(object sender, EventArgs e)
        {
            _webClient.CancelAsync();
            _keepDownloading = false;
            lblDownloadStat.Text = _downloadMessage;
        }

        private void UpdateReady()
        {
            lblDownloadStat.Text = "All update files successfully downloaded.";
            btnDownload.Enabled = false;
            btnDownloadCancel.Enabled = false;
            btnGameUpdate.Enabled = true;
        }

        private void GameUpdate()
        {
            CheckUpdateFiles(); //just to be sure...

            lblUpdate.Text = "Initiating game update...";

            string filesPath = Preferences.Instance.Options.UserFilesPath + Preferences.AppFolder + @"update\";

            foreach(var file in _filesToDownload)
            {                

                string currentFilePath = filesPath + file.Key.Replace('/','\\');
                lblUpdate.Text = "Applying file " + file.Key.Replace('/', '\\');
                //byte[] fileData = File.ReadAllBytes(currentFilePath);

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
                            while(br.BaseStream.Position != br.BaseStream.Length)
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
                        }                      
                    }
                    //break;
                //}catch(Exception e){ throw e; }       
            }

            //all update files were processed
            File.WriteAllText(Preferences.Instance.Options.GameInstallPath + "/game.ver", "2012.09.19.0001 [Patched by Primal Launcher]");
            File.WriteAllText(Preferences.Instance.Options.GameInstallPath + "/boot.ver", "2010.09.18.0000 [Patched by Primal Launcher]");

            lblUpdate.Text = "Game is updated to the latest version.";

            btnGameUpdate.Enabled = false;

            if (!chkKeepFiles.Checked)
            {
                DirectoryInfo dir = new DirectoryInfo(Preferences.Instance.Options.PatchDownloadPath);

                foreach(DirectoryInfo subDir in dir.GetDirectories())
                {
                    subDir.Delete(true);
                }
            }           
        }

        private uint SwapEndian(uint number)
        {
            byte[] data = BitConverter.GetBytes(number);
            return (uint)(data[0] << 24 | data[1] << 16 | data[2] << 8 | data[3]);
        }  
        
        private string ReadDirectory(ref BinaryReader br)
        {
            uint pathSize = SwapEndian(br.ReadUInt32());
            string pathToCreate = Encoding.ASCII.GetString(br.ReadBytes((int)pathSize));

            br.ReadUInt64(); //unknown

            if((br.BaseStream.Length - br.BaseStream.Position) >= 0x08) //bug fix
                br.ReadUInt64(); //unknown

            return Preferences.Instance.Options.GameInstallPath + pathToCreate;
        }

        private void ProcessETRY(ref BinaryReader br)
        {
            uint pathSize = SwapEndian(br.ReadUInt32());
            string filePath = Encoding.ASCII.GetString(br.ReadBytes((int)pathSize));
            int itemCount = (int)SwapEndian(br.ReadUInt32());            

            for(int i = 0; i < itemCount; i++)
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

                    //Save file.
                    File.WriteAllBytes(fileDestinationPath, fileData);
                }
                catch(Exception e) { throw e; }  
            }            
        }

        private void ProcessFHDR(ref BinaryReader br) => br.ReadInt32();

        private void ProcessHIST(ref BinaryReader br)
        {
            br.ReadInt32();  //unknown
            br.ReadUInt64(); //unknown
            br.ReadUInt32(); //unknown
            br.ReadUInt32(); //unknown
        }

        private void ProcessAPLY(ref BinaryReader br)
        {
            br.ReadInt32();  //unknown
            br.ReadUInt64(); //unknown
            br.ReadUInt32(); //unknown
            br.ReadUInt32(); //unknown
        }

        private void ProcessADIR(ref BinaryReader br)
        {
            string fileDestinationPath = ReadDirectory(ref br);

            if (!Directory.Exists(fileDestinationPath))
                Directory.CreateDirectory(fileDestinationPath);        
        }

        private void ProcessDIFF(ref BinaryReader br)
        {
            br.ReadInt32();  //unknown
            br.ReadUInt64(); //unknown
            br.ReadUInt32(); //unknown
            br.ReadUInt32(); //unknown
        }

        private void ProcessDELD(ref BinaryReader br)
        {
            string fileDestinationPath = ReadDirectory(ref br);

            if (Directory.Exists(fileDestinationPath))
                ClearFolder(fileDestinationPath);            
        }

        private void btnGameUpdate_Click(object sender, EventArgs e) => GameUpdate();

        private void ClearFolder(string folderName)
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

        private void btnPatch_Click(object sender, EventArgs e)
        {
            //if Primal Launcher can write to the game installation folder
            if (Patcher.HasWritePermission())            
                Patcher.PatchExecutableFiles();
            else           
                MessageBox.Show("Primal Launcher", "You need write permissions in the game installation folder. Please close this program, right-click the shortcut and select 'Run as administrator'. This operation is required only once. Aborting patching operation.");
        }
    }
}
