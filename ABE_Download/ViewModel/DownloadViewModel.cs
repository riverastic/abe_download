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
        public DownloadModel Model { get; }
        public ICommand DownloadCommand { get; set; }
        public DownloadViewModel()
        {
            Model = new DownloadModel();
            DownloadCommand = new RelayCommand<Session>(s => { return Model.IsConnected; }, s => GetRemoteDirectoryInfos(Model.session));
        }

        private void GetRemoteDirectoryInfos(Session s)
        {
            if (!s.Opened) s.Open(Model.sessionOptions);
            RemoteDirectoryInfo remoteDirectoryInfo = s.ListDirectory(Model.RemoteDir);
            foreach (RemoteFileInfo fileInfo in remoteDirectoryInfo.Files.Where(f => f.IsDirectory && !f.Name.Contains("..")))
            {
                //var opt = EnumerationOptions.EnumerateDirectories | EnumerationOptions.AllDirectories;
                //var folders = s.EnumerateRemoteFiles(fileInfo.FullName, null, opt).Where(f => f.Name.Equals("12-11-2021")).FirstOrDefault();
                //if (folders!=null)
                //{
                //    MessageBox.Show(folders.FullName);
                //}
                

            }
            s.Close();
        }

    }
}
