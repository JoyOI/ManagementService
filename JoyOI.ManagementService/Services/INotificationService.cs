using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JoyOI.ManagementService.Services
{
    /// <summary>
    /// 通知服务, 用于实时通知管理员
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// 发送通知
        /// </summary>
        Task Send(string title, string message);
    }
}
