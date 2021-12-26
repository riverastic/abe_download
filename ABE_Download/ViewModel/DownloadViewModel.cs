using ABE_Download.Models;
using Prism.Commands;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WinSCP;

namespace ABE_Download.ViewModel
{
    [AddINotifyPropertyChangedInterface]
    public class DownloadViewModel
    {
        public DownloadModel Model { get; } = new();
        public DownloadLogModel ModelLog { get; } = new();
        public ObservableCollection<RemoteFileInfo> RemoteFileInfos = new();
        public bool Expander1 { get; set; } = false;
        public bool Expander2 { get; set; } = false;
        public ICommand ScanCommand { get; set; }
        public ICommand CheckConnectCommand { get; set; }
        public ICommand DownloadCommand { get; set; }
        public ICommand ScanAndDownloadCommand { get; set; }

        public DownloadViewModel()
        {
            CheckConnectCommand = new DelegateCommand( async () => await CheckConnectionAsync() );
            ScanCommand = new DelegateCommand( async () => await ScanAsync() );
            DownloadCommand = new DelegateCommand( async () => await DownloadAsync() );
            ScanAndDownloadCommand = new DelegateCommand( async () => await AlwaysFromKeyboard() );
        }

        private async Task AlwaysFromKeyboard()
        {
            await CheckConnectionAsync();
            Expander1 = true;
            await ScanAsync();
            Expander1 = false;
            Expander2 = true;
            await DownloadAsync();
            Expander2 = false;
            Model.Status = "All process have been completed.";
            Environment.Exit( 0 );
        }

        private async Task<bool> CheckConnectionAsync()
        {
            await Task.Run( () =>
             {
                 try
                 {
                     Model.Session = new Session();
                     Model.Session.Open( Model.SessionOptions );
                     Model.IsConnected = Model.Session.Opened;
                     Model.Session.Close();
                 }
                 catch ( Exception )
                 {
                     Model.IsConnected = false;
                 }
             } );

            return Model.IsConnected;
        }

        private async Task ScanAsync()
        {
            try
            {
                if ( !Model.Session.Opened ) Model.Session.Open( Model.SessionOptions );
                Model.Status = "ABE scanning ...";
                RemoteDirectoryInfo remoteDirectoryInfo = Model.Session.ListDirectory( Model.RemoteDir );
                var remotes = new ObservableCollection<RemoteFileInfo>( remoteDirectoryInfo.Files.Where( f => f.IsDirectory && !f.Name.Contains( ".." ) ).Cast<RemoteFileInfo>() );
                await Task.Run( () => remotes.ToList().ForEach( f => Scan( f.FullName ) ) );
                Model.Session.Close();
                Model.Status = "ABE scan completed.";
                Model.IsScanned = true;
            }
            catch ( Exception )
            {
                Model.IsScanned = false;
            }
        }

        private void Scan( string remoteDirectory )
        {
            RemoteDirectoryInfo remoteDirectoryInfo = Model.Session.ListDirectory( remoteDirectory );
            var remoteFolders = remoteDirectoryInfo.Files
                .Where( rm => rm.IsDirectory && DateTime.TryParse( rm.Name, out DateTime date ) )
                .OrderByDescending( rm => rm.Name );
                
            if ( remoteFolders is not null && remoteFolders.Count() > 0 )
            {
                var selectFolder = remoteFolders.FirstOrDefault( x => x.Name.Contains( "12-24-2021" ) && !x.FullName.Contains( "Related" ) );
                if ( selectFolder is not null )
                {
                    App.Current.Dispatcher.Invoke( () =>
                    {
                        Debug.WriteLine( selectFolder.FullName );
                        var tranToLocalPath = RemotePath.TranslateRemotePathToLocal( selectFolder.FullName, Model.RemoteDir, Model.LocalDir );
                        Debug.WriteLine( $"Local path {tranToLocalPath}" );
                        if ( !Directory.Exists( tranToLocalPath ) ) Directory.CreateDirectory( tranToLocalPath );
                        Model.DownloadInfos.Add( new DownloadInfoModel()
                        {
                            RemoteDirectory = selectFolder.FullName,
                            LocalDirectory = tranToLocalPath,
                        } );
                    } );
                }
                return;
            }

            foreach ( RemoteFileInfo subRemoteFolder in remoteDirectoryInfo.Files.Where( rm => rm.IsDirectory && !rm.Name.Contains( ".." ) ).OrderByDescending( rm => rm.LastWriteTime ) )
            {
                try
                {
                    Scan( subRemoteFolder.FullName );
                }
                catch ( UnauthorizedAccessException ex )
                {
                    MessageBox.Show( ex.Message );
                    return;
                }
            }
        }

        private async Task DownloadAsync()
        {
            Model.Session.FileTransferProgress += Session_FileTransferProgress;
            if ( !Model.Session.Opened ) Model.Session.Open( Model.SessionOptions );
            Model.Status = "ABE start downloading ...";
            Model.TransferOptions = new TransferOptions();
            Model.TransferOptions.TransferMode = TransferMode.Binary;
            Model.TransferOptions.ResumeSupport.State = TransferResumeSupportState.On;
            await Task.Run( () => Model.DownloadInfos.ToList().ForEach( di =>
              {
                  Report( new DownloadLogModel()
                  {
                      LogTime = DateTime.Now,
                      Title = LogTitle.CHECK,
                      FileName = di.RemoteDirectory,
                      Visible = Visibility.Collapsed
                  } );

                  var listNew = CheckNewFolders( di );
                  foreach ( var dir in listNew )
                  {
                      string targetRemoteDir = $"{di.RemoteDirectory}/{dir}";
                      Download( targetRemoteDir );
                  }
              } ) );
            Model.Session.Close();
            Model.Status = "ABE download completed.";
        }

        private IEnumerable<string> CheckNewFolders( DownloadInfoModel downloadInfoModel )
        {
            var listDownloaded = new List<string>();
            string done = Path.Combine( downloadInfoModel.LocalDirectory, "done" );
            if ( Directory.Exists( done ) )
            {
                foreach ( var dir in new DirectoryInfo( done ).GetDirectories( "Part*" ) )
                {
                    listDownloaded.Add( dir.Name );
                }
            }

            var localDir = new DirectoryInfo( downloadInfoModel.LocalDirectory ).GetDirectories( "Part*" );
            if ( localDir.Count() > 0 )
            {
                foreach ( var dir in localDir )
                {
                    listDownloaded.Add( dir.Name );
                }
            }
            listDownloaded.Sort();

            var remoteDir = Model.Session.ListDirectory( downloadInfoModel.RemoteDirectory ).Files
                .Where( x => x.IsDirectory && !x.Name.EndsWith( ".." ) )
                .Select( x => x.Name );

            var res = remoteDir.Except( listDownloaded );
            return localDir.Select( x => x.Name ).Concat( res ).Distinct();

        }

        private void Download( string remoteDirectory )
        {
            string localPath = RemotePath.TranslateRemotePathToLocal( remoteDirectory, Model.RemoteDir, Model.LocalDir );
            int countLocal;
            if ( Directory.Exists( localPath ) )
                countLocal = new DirectoryInfo( localPath ).GetFiles( "*.*" ).Count();
            else
            {
                Directory.CreateDirectory( localPath );
                countLocal = 0;
            }
            var opts = EnumerationOptions.EnumerateDirectories | EnumerationOptions.AllDirectories;
            IEnumerable<RemoteFileInfo> fileInfos = Model.Session.EnumerateRemoteFiles( remoteDirectory, null, opts ).Where( p => !p.Name.Equals( ".." ) );
            if ( countLocal != fileInfos.Count() )
            {
                foreach ( RemoteFileInfo fileInfo in fileInfos )
                {
                    bool isDirectory = fileInfo.IsDirectory;
                    string DownloadPath = RemotePath.TranslateRemotePathToLocal( fileInfo.FullName, Model.RemoteDir, Model.LocalDir );
                    if ( isDirectory )
                    {
                        if ( !Directory.Exists( DownloadPath ) )
                        {
                            Report( new DownloadLogModel()
                            {
                                LogTime = DateTime.Now,
                                Title = LogTitle.CREATE,
                                FileName = fileInfo.FullName,
                                Visible = Visibility.Collapsed
                            } );
                            Directory.CreateDirectory( DownloadPath );
                        }
                    }
                    else
                    {
                        if ( !File.Exists( DownloadPath ) )
                        {
                            Report( new DownloadLogModel()
                            {
                                LogTime = DateTime.Now,
                                Title = LogTitle.DOWNLOAD,
                                FileName = fileInfo.FullName,
                                Visible = Visibility.Visible
                            } );
                            Model.TransferResult = Model.Session.GetFiles( fileInfo.FullName, DownloadPath );
                            if ( !Model.TransferResult.IsSuccess ) Debug.WriteLine( $"Error downloading file {fileInfo.FullName}: {Model.TransferResult.Failures[ 0 ].Message})" );
                        }
                    }
                }
            }
        }

        private void Session_FileTransferProgress( object sender, FileTransferProgressEventArgs e )
        {
            var logDownload = Model.DownloadLogInfos.FirstOrDefault( x => x.FileName.Equals( e.FileName ) );
            if ( logDownload is not null )
                logDownload.Progress = e.FileProgress;
        }

        private void Report( DownloadLogModel downloadLog )
        {
            App.Current.Dispatcher.Invoke( () => Model.DownloadLogInfos.Add( downloadLog ) );
        }
    }
}
