using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinSCP;

namespace ABE_Download.Model
{
    [AddINotifyPropertyChangedInterfaceAttribute]
    public class DownloadModel
    {
        public SessionOptions sessionOptions { get; }
        public Session session { get; set; }
        public const string HOST_NAME = "autotransfer.spi-global.com";
        public const string USER_NAME = "vietnamln.sftp";
        private const string PASSWORD = "ntiae0)8Zl";
        private const string SSH_HOST_KEY = "ssh-rsa 2048 Yc9gTF4+Go210UWFUxDg+wzdljn6Rp4Oooag5TBziO0=";
        public string RemoteDir = "/FROM SPI/LEXIS-NEXIS";
        public bool IsConnected { get; set; } = false;
        public ObservableCollection<RemoteFileInfo> RemoteFileInfos { get; set; }
        public DownloadModel()
        {
            sessionOptions = new SessionOptions
            {
                Protocol = Protocol.Sftp,
                HostName = HOST_NAME,
                UserName = USER_NAME,
                Password = PASSWORD,
                SshHostKeyFingerprint = SSH_HOST_KEY,
            };
            try
            {
                session = new Session();
                session.Open(sessionOptions);
                IsConnected = session.Opened;
                session.Close();
            }
            catch (Exception)
            {
                IsConnected = false;
            }
        }
    }
}
