using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ValheimLauncher
{

    enum LauncherStatus
    {
        ready,
        failed,
        downloadingGame,
        downloadingUpdate
    }
    public partial class Form1 : Form
    {
        private string rootPath;
        private string versionFile;
        private string gameZip;
        private string gameExe;
        private string gameFolder;

        private LauncherStatus _status;

        LauncherStatus Status {
            get => _status;
            set {
                _status = value;

                switch (_status)
                {
                    case LauncherStatus.ready:
                        button1.Text = "Играть";
                        break;
                    case LauncherStatus.failed:
                        button1.Text = "Ошибка загрузки - повторить";
                        break;
                    case LauncherStatus.downloadingGame:
                        button1.Text = "Загрузка...";
                        break;
                    case LauncherStatus.downloadingUpdate:
                        button1.Text = "Обновление...";
                        break;
                }
            }
        }

        private void CheckForUpdate()
        {
            button1.Enabled = false;
            if (File.Exists(versionFile))
            {
                Version localVersion = new Version(File.ReadAllText(versionFile));
                VersionText.Text = localVersion.ToString();
                try
                {
                    WebClient webClient = new WebClient();
                    Version onlineVersion = new Version(webClient.DownloadString("https://drive.google.com/uc?export=download&id=1H_wikZPY6mVjqDPbECzozcfROlPOzjN5"));
                    if (onlineVersion.IsDifferentThan(localVersion))
                    {
                        
                        InstallGameFile(true, onlineVersion);
                    }
                    else
                    {
                        button1.Enabled = true;
                    }
                }
                catch (Exception ex)
                {
                    Status = LauncherStatus.failed;
                }
            }
            else
            {
              InstallGameFile(false, Version.zero);
            }
            
        }
        

        private void  InstallGameFile(bool isUpdate, Version _onlineVersion)
        {
            try
            {
                WebClient webClient = new WebClient();
                if (isUpdate)
                {
                    Directory.Delete(gameFolder, true);
                    Status = LauncherStatus.downloadingUpdate;
                }
                else
                {
                    Status = LauncherStatus.downloadingGame;
                    _onlineVersion = new Version(webClient.DownloadString("https://drive.google.com/uc?export=download&id=1H_wikZPY6mVjqDPbECzozcfROlPOzjN5"));
                }
                webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadGameProgressCallback);
                webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadGameComplitedCallback); 
                webClient.DownloadFileAsync(new Uri("https://s126vla.storage.yandex.net/rdisk/3803e70bcd419393e284cbc559f2b575c4a92c2b2f3c65c1bbe6e77d1fb6bca1/60522606/NlECKdbUsx_JA2-xdcXr0OVgKt52ZiUcYu3IWPLUsTHJGB2BKr2LGaBqJVF8uYdA89qcOz6otpqkh7ldbhQrgg==?uid=0&filename=Valheim.zip&disposition=attachment&hash=YZtTVut8LjctGIdiMWJYMozmIXLh0Ql8KoS/%2B2LlotYMMXJNZj3cjSAeDrrsMtviq/J6bpmRyOJonT3VoXnDag%3D%3D&limit=0&content_type=application%2Fzip&owner_uid=364401962&fsize=653097221&hid=aed649b49c7e4138f275277e1bbd42c8&media_type=compressed&tknv=v2&rtoken=XMnwsWgHASUI&force_default=no&ycrid=na-a5a1bf3af5f6dfa0d078b67471fd7776-downloader2f&ts=5bdbd7cb10d80&s=8aa47df6ede6c1c449f066dc1d05373aa511a2a45edeba83c243c23e3f320448&pb=U2FsdGVkX1_EgbJ3Bq0ltPaTntwyzCyKINQELBMC5BQtCG0StIxZS5wAmU1RvwzI3eHRbmOfBR-JgZ_1NrNZDFvz4qe3y3iX-ncR8KwXiCE"), gameZip, _onlineVersion);
            }
            catch(Exception ex)
            {
                Status = LauncherStatus.failed;
            }
        }
        private void DownloadGameComplitedCallback(object sender, AsyncCompletedEventArgs e)
        {
            try
            {
                string onlineVersion = ((Version)e.UserState).ToString();
                ZipFile.ExtractToDirectory(gameZip, rootPath);
                File.Delete(gameZip);
                
                File.WriteAllText(versionFile, onlineVersion);
                VersionText.Text = onlineVersion;
                Status = LauncherStatus.ready;
                button1.Enabled = true;
            }
            catch(Exception ex)
            {
                Status = LauncherStatus.failed; 
            }
        }

        private void DownloadGameProgressCallback(object sender, DownloadProgressChangedEventArgs e)
        {
            try
            {
                progressBar2.Value = e.ProgressPercentage;
            }
            catch (Exception ex)
            {
                Status = LauncherStatus.failed;
            }
        }
        public Form1()
        {
            InitializeComponent();
            rootPath = Directory.GetCurrentDirectory();
            versionFile = Path.Combine(rootPath, "Version.txt");
            gameZip = Path.Combine(rootPath, "Valheim.zip");
            gameExe = Path.Combine(rootPath, "Valheim", "valheim.exe");
            gameFolder = Path.Combine(rootPath, "Valheim");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            CheckForUpdate();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(File.Exists(gameExe) && Status == LauncherStatus.ready)
            {
                ProcessStartInfo startInfo = new ProcessStartInfo(gameExe);
                startInfo.WorkingDirectory = Path.Combine(rootPath, "Valheim");
                Process.Start(startInfo);
                Close();
            }
            else
            {
                CheckForUpdate();
            }
        }

      
    }

    internal struct Version
    {
        internal static Version zero = new Version(0, 0, 0);

        private short major;
        private short minor;
        private short subMinor;
        internal Version(short _major, short _minor, short _subMinor)
        {
            major = _major;
            minor = _minor;
            subMinor = _subMinor;
        }

        internal Version(string _version)
        {
            string[] _versionStrings = _version.Split('.');
            if(_versionStrings.Length != 3)
            {
                major = 0;
                minor = 0;
                subMinor = 0;
                return;
            }
            major = short.Parse(_versionStrings[0]);
            minor = short.Parse(_versionStrings[1]);
            subMinor = short.Parse(_versionStrings[2]);
        }

        internal bool IsDifferentThan(Version _otgerVersion)
        {
            if (major != _otgerVersion.major || minor != _otgerVersion.minor || subMinor != _otgerVersion.subMinor)
            {
                return true;
            }
            return false;
        }
        public override string ToString()
        {
            return $"{major}.{minor}.{subMinor}";
        }
    }
}
