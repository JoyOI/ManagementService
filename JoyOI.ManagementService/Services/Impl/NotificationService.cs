using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace JoyOI.ManagementService.Services.Impl
{
    public class NotificationService : INotificationService
    {
        public async Task Send(string title, string message)
        {
            // TODO: 实现这里的内容
            Console.Error.WriteLine($"Notify {DateTime.Now}: {title}\r\n{message}\r\n\r\n");
			await Console.Error.FlushAsync();
        }
    }
}
