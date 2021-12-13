using ABE_Download.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WinSCP;

namespace ABE_Download.ViewModel
{
    public class DownloadViewModel
    {
        public DownloadModel Model { get; } = new();
        public ObservableCollection<RemoteFileInfo> RemoteFileInfos = new();
        public ICommand DownloadCommand { get; set; }
        public DownloadViewModel()
        {
            DownloadCommand = new RelayCommand<Session>(s => { return Model.IsConnected; }, s => GetRemoteDirectoryInfos(Model.session));
        }

        private void GetRemoteDirectoryInfos(Session s)
        {
            if (!s.Opened) s.Open(Model.sessionOptions);
            RemoteDirectoryInfo remoteDirectoryInfo = s.ListDirectory(Model.RemoteDir);
            Model.RemoteFileInfos = new ObservableCollection<RemoteFileInfo>(remoteDirectoryInfo.Files.Where(f => f.IsDirectory && !f.Name.Contains("..")).Cast<RemoteFileInfo>());
            foreach (RemoteFileInfo fileInfo in Model.RemoteFileInfos)
            {
                //var opt = EnumerationOptions.EnumerateDirectories | EnumerationOptions.AllDirectories;
                //var folders = s.EnumerateRemoteFiles(fileInfo.FullName, null, opt).Where(f => f.Name.Equals("12-11-2021")).FirstOrDefault();
                //if (folders != null)
                //{
                //    MessageBox.Show(folders.FullName);
                //}
                


            }
            s.Close();
        }
    }
}
