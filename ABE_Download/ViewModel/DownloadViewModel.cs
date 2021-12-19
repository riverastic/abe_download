using ABE_Download.Models;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
        public ICommand ScanCommand { get; set; }
        public ICommand CheckConnectCommand { get; set; }
        public DownloadViewModel()
        {
            CheckConnectCommand = new DelegateCommand(async () => await CheckConnection());
            ScanCommand = new DelegateCommand(async () => await ScanAsync(Model.Session));
        }

        private async Task<bool> CheckConnection()
        {
            await Task.Run(() =>
            {
                try
                {
                    Model.Session = new Session();
                    Model.Session.Open(Model.SessionOptions);
                    Model.IsConnected = Model.Session.Opened;
                    Model.Session.Close();
                }
                catch (Exception)
                {
                    Model.IsConnected = false;
                }
            });

            return Model.IsConnected;
        }

        private async Task ScanAsync(Session session)
        {
            if (!session.Opened) session.Open(Model.SessionOptions);
            RemoteDirectoryInfo remoteDirectoryInfo = session.ListDirectory(Model.RemoteDir);
            var remotes = new ObservableCollection<RemoteFileInfo>(remoteDirectoryInfo.Files.Where(f => f.IsDirectory && !f.Name.Contains("..")).Cast<RemoteFileInfo>());
            await Task.Run(() => remotes.ToList().ForEach(f => Scan(session, f.FullName)));
            session.Close();
        }

        private void Scan(Session session, string remoteDirectory)
        {
            RemoteDirectoryInfo remoteDirectoryInfo = session.ListDirectory(remoteDirectory);
            var remoteFolders = remoteDirectoryInfo.Files.Where(rm => rm.IsDirectory && DateTime.TryParse(rm.Name, out DateTime date)).OrderByDescending(rm => rm.LastWriteTime);
            foreach (var remoteFolder in remoteFolders)
            {
                App.Current.Dispatcher.Invoke((Action)delegate
                {
                    Debug.WriteLine(remoteFolder.FullName);
                    Model.RemoteDownloadFolders.Add(remoteFolder.FullName);
                });
                return;
            }
            foreach (RemoteFileInfo subRemoteFolder in remoteDirectoryInfo.Files.Where(rm => rm.IsDirectory && !rm.Name.Contains("..")).OrderByDescending(rm => rm.LastWriteTime))
            {
                try
                {
                    Scan(session, subRemoteFolder.FullName);
                }
                catch (UnauthorizedAccessException ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }
            }
        }
    }
}
