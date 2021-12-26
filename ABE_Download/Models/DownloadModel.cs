using ABE_Download.Model;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinSCP;

namespace ABE_Download.Models
{
    [AddINotifyPropertyChangedInterface]
    public class DownloadModel
    {
        public ConnectionModel Model { get; } = new();
        public SessionOptions SessionOptions { get; }
        public Session Session { get; set; }
        public TransferOptions TransferOptions { get; set; }
        public TransferOperationResult TransferResult { get; set; }
        public string RemoteDir = "/FROM SPI/LEXIS-NEXIS";
        public string LocalDir { get; set; } = @"D:\test";
        public bool IsConnected { get; set; } = false;
        public bool IsScanned { get; set; } = false;
        public ObservableCollection<DownloadInfoModel> DownloadInfos { get; set; } = new();
        public ObservableCollection<DownloadLogModel> DownloadLogInfos { get; set; } = new();
        public string Status { get; set; } = "AB Extension";
        public DownloadModel()
        {
            SessionOptions = new SessionOptions
            {
                Protocol = Protocol.Sftp,
                HostName = Model.HostName,
                UserName = Model.UserName,
                Password = Model.Password,
                SshHostKeyFingerprint = Model.SshHostKey,
            };
            SessionOptions.AddRawSettings("ProxyMethod", "");
            SessionOptions.AddRawSettings("ProxyHost", "");
            SessionOptions.AddRawSettings("ProxyPort", "");
        }
    }
}
