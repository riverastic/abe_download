using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ABE_Download.Models
{
    [AddINotifyPropertyChangedInterface]
    public class DownloadLogModel
    {
        string _fileSize;
        public DateTime? LogTime { get; set; }
        public LogTitle? Title { get; set; }
        public string FileName { get; set; }
        public string FileSize
        {
            get => _fileSize; set
            {
                _fileSize = value;
                _fileSize += "bytes";
            }
        }
        public double Progress { get; set; }
        public Visibility Visible { get; set; }
    }
}
