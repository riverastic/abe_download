using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABE_Download.Models
{
    public class DownloadLogModel
    {
        public DateTime LogTime { get; set; }
        public LogTitle Title { get; set; }
        public string Message { get; set; }
        public int Progress { get; set; } = 0;
    }
}
