using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.EntityFrameworkCore.Migrations
{
    /// <summary>
    /// Actor生成的runner.json的信息
    /// </summary>
    public class RunnerInfo
    {
        public string Command { get; set; }
        public int UserTime { get; set; } // ms
        public int TotalTime { get; set; } // ms
        public int PeakMemory { get; set; } // bytes, rss
        public int ExitCode { get; set; }
        public bool IsTimeout { get; set; }
        public string Error { get; set; }
    }
}
