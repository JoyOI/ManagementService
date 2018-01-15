using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace JoyOI.ManagementService.Utils
{
    /// <summary>
    /// 异步等待的工具类
    /// </summary>
    public static class AwaitUtils
    {
        /// <summary>
        /// 等待任务结束，超过指定时间时强制抛出错误
        /// 用于防止操作不支持CancellationToken时永久等待
        /// </summary>
        /// <returns></returns>
        public static async Task WithTimeout(Func<CancellationToken, Task> func, TimeSpan timeout)
        {
            using (var waitCancelToken = new CancellationTokenSource())
            {
                waitCancelToken.CancelAfter(timeout);
                var waitCancelTask = Task.Delay(timeout);
                var task = func(waitCancelToken.Token);
                var waitResult = await Task.WhenAny(waitCancelTask, task);
                if (waitResult == task)
                {
                    return;
                }
                else
                {
                    waitCancelToken.Cancel();
                    throw new TimeoutException("wait task failed due to timeout");
                }
            }
        }

        /// <summary>
        /// 等待任务结束，超过指定时间时强制抛出错误
        /// 用于防止操作不支持CancellationToken时永久等待
        /// </summary>
        /// <returns></returns>
        public static async Task<T> WithTimeout<T>(Func<CancellationToken, Task<T>> func, TimeSpan timeout)
        {
            using (var waitCancelToken = new CancellationTokenSource())
            {
                waitCancelToken.CancelAfter(timeout);
                var waitCancelTask = Task.Delay(timeout);
                var task = func(waitCancelToken.Token);
                var waitResult = await Task.WhenAny(waitCancelTask, task);
                if (waitResult == task)
                {
                    return task.Result;
                }
                else
                {
                    waitCancelToken.Cancel();
                    throw new TimeoutException("wait task failed due to timeout");
                }
            }
        }
    }
}
