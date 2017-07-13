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
        public string Error { get; set; }
        public int UsedTime { get; set; } // ms
        public int ExitCode { get; set; }
        public int PeakMemory { get; set; } // bytes, rss
    }
}
