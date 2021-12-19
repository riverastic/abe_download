using ABE_Download.Models;
using Prism.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
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
        public DownloadLogModel ModelLog { get; } = new();
        public ObservableCollection<RemoteFileInfo> RemoteFileInfos = new();
        public ICommand ScanCommand { get; set; }
        public ICommand CheckConnectCommand { get; set; }
        public ICommand DownloadCommand { get; set; }
        public DownloadViewModel()
        {
            CheckConnectCommand = new DelegateCommand(async () => await CheckConnection());
            ScanCommand = new DelegateCommand(async () => await ScanAsync());
            DownloadCommand = new DelegateCommand(async () => await DownloadAsync());
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

        private async Task ScanAsync()
        {
            try
            {
                if (!Model.Session.Opened) Model.Session.Open(Model.SessionOptions);
                RemoteDirectoryInfo remoteDirectoryInfo = Model.Session.ListDirectory(Model.RemoteDir);
                var remotes = new ObservableCollection<RemoteFileInfo>(remoteDirectoryInfo.Files.Where(f => f.IsDirectory && !f.Name.Contains("..")).Cast<RemoteFileInfo>());
                await Task.Run(() => remotes.ToList().ForEach(f => Scan(f.FullName)));
                Model.Session.Close();
                Model.IsScanned = true;
            }
            catch (Exception)
            {
                Model.IsScanned = false;
            }
        }

        private void Scan(string remoteDirectory)
        {
            RemoteDirectoryInfo remoteDirectoryInfo = Model.Session.ListDirectory(remoteDirectory);
            var remoteFolders = remoteDirectoryInfo.Files.Where(rm => rm.IsDirectory && DateTime.TryParse(rm.Name, out DateTime date)).OrderByDescending(rm => rm.LastWriteTime);
            foreach (var remoteFolder in remoteFolders)
            {
                if (remoteFolder.FullName.EndsWith(/*DateTime.Now.ToString("MM-dd-yyyy")*/"12-18-2021"))
                {
                    App.Current.Dispatcher.Invoke((Action)delegate
                    {
                        Debug.WriteLine(remoteFolder.FullName);
                        var tranToLocalPath = RemotePath.TranslateRemotePathToLocal(remoteFolder.FullName, Model.RemoteDir, Model.LocalDir);
                        Debug.WriteLine($"Local path {tranToLocalPath}");
                        if (!Directory.Exists(tranToLocalPath)) Directory.CreateDirectory(tranToLocalPath);
                        Model.DownloadInfos.Add(new DownloadInfoModel()
                        {
                            RemoteDirectory = remoteFolder.FullName,
                            LocalDirectory = tranToLocalPath,
                        });
                    });
                }
                return;
            }
            foreach (RemoteFileInfo subRemoteFolder in remoteDirectoryInfo.Files.Where(rm => rm.IsDirectory && !rm.Name.Contains("..")).OrderByDescending(rm => rm.LastWriteTime))
            {
                try
                {
                    Scan(subRemoteFolder.FullName);
                }
                catch (UnauthorizedAccessException ex)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }
            }
        }

        private async Task DownloadAsync()
        {
            Model.Session.FileTransferProgress += Session_FileTransferProgress;
            if (!Model.Session.Opened) Model.Session.Open(Model.SessionOptions);
            Model.TransferOptions = new TransferOptions();
            Model.TransferOptions.TransferMode = TransferMode.Binary;
            Model.TransferOptions.ResumeSupport.State = TransferResumeSupportState.On;
            await Task.Run(() => Model.DownloadInfos.ToList().ForEach(di =>
            {
                Report($"On checking {di.RemoteDirectory}", LogTitle.INFO);
                if (di.RemoteDirectory.EndsWith("12-18-2021"))
                {
                    Download(di.RemoteDirectory);
                }
            }));
            Model.Session.Close();
        }

        private void Download(string remoteDirectory)
        {
            var opts = EnumerationOptions.EnumerateDirectories | EnumerationOptions.AllDirectories;
            IEnumerable<RemoteFileInfo> fileInfos = Model.Session.EnumerateRemoteFiles(remoteDirectory, null, opts).Where(p => !p.Name.Equals(".."));
            foreach (RemoteFileInfo fileInfo in fileInfos)
            {
                Report($"Downloading {fileInfo.FullName}", LogTitle.INFO);
                string DownloadPath = RemotePath.TranslateRemotePathToLocal(fileInfo.FullName, Model.RemoteDir, Model.LocalDir);
                if (fileInfo.IsDirectory)
                {
                    if (!Directory.Exists(DownloadPath)) Directory.CreateDirectory(DownloadPath);
                }
                else
                {
                    Model.TransferResult = Model.Session.GetFiles(fileInfo.FullName, DownloadPath);
                    if (!Model.TransferResult.IsSuccess) Debug.WriteLine($"Error downloading file {fileInfo.FullName}: {Model.TransferResult.Failures[0].Message})");
                }
            }
        }

        private void Session_FileTransferProgress(object sender, FileTransferProgressEventArgs e)
        {
            Debug.WriteLine(e.FileProgress);
        }

        private void Report(string message, LogTitle logTitle)
        {
            App.Current.Dispatcher.Invoke((Action)delegate
            {
                Model.DownloadLogInfos.Add(new DownloadLogModel()
                {
                    LogTime = DateTime.Now,
                    Title = logTitle,
                    Message = message,
                });
            });
        }
    }
}
