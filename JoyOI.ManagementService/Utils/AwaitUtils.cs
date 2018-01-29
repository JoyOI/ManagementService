using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
        private static Dictionary<DateTime, List<string>> TimeoutErrorHistory = new Dictionary<DateTime, List<string>>();
        private static object TimeoutErrorHistoryThreadLock = new object();
        private static int TimeoutErrorHistoryKeepDays = 30; // 最多保留30天的记录

        /// <summary>
        /// 获取超时错误的历史记录
        /// </summary>
        /// <returns></returns>
        public static IList<string> GetTimeoutErrors()
        {
            List<string> result;
            lock (TimeoutErrorHistoryThreadLock)
            {
                result = TimeoutErrorHistory.OrderBy(x => x.Key).SelectMany(x => x.Value).ToList();
            }
            return result;
        }

        /// <summary>
        /// 添加超时错误的历史记录
        /// </summary>
        private static void AddTimeoutError(string error)
        {
            var now = DateTime.UtcNow.ToLocalTime();
            var today = now.Date;
            var timeString = now.ToString("R");
            var message = $"({timeString}): {error}";
            lock (TimeoutErrorHistoryThreadLock)
            {
                // 清理旧纪录(不考虑日期是否连续)
                if (TimeoutErrorHistory.Keys.Count > TimeoutErrorHistoryKeepDays)
                {
                    var allKeys = TimeoutErrorHistory.Keys.OrderBy(x => x).ToList();
                    foreach (var key in allKeys.Take(allKeys.Count - TimeoutErrorHistoryKeepDays))
                        TimeoutErrorHistory.Remove(key);
                }
                // 添加纪录到当天
                List<string> messages;
                if (!TimeoutErrorHistory.TryGetValue(today, out messages))
                {
                    messages = new List<string>();
                    TimeoutErrorHistory[today] = messages;
                }
                messages.Add(message);
            }
        }

        /// <summary>
        /// 等待任务结束，超过指定时间时强制抛出错误
        /// 用于防止操作不支持CancellationToken时永久等待
        /// </summary>
        /// <returns></returns>
        public static async Task WithTimeout(
            Func<CancellationToken, Task> func, TimeSpan timeout, string hint)
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
                    var error = $"wait task failed due to timeout, timeout: {timeout.TotalSeconds}s, hint: \"${hint}\"";
                    AddTimeoutError(error);
                    throw new TimeoutException(error);
                }
            }
        }

        /// <summary>
        /// 等待任务结束，超过指定时间时强制抛出错误
        /// 用于防止操作不支持CancellationToken时永久等待
        /// </summary>
        /// <returns></returns>
        public static async Task<T> WithTimeout<T>(
            Func<CancellationToken, Task<T>> func, TimeSpan timeout, string hint)
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
                    var error = $"wait task failed due to timeout, timeout: {timeout.TotalSeconds}s, hint: \"${hint}\"";
                    AddTimeoutError(error);
                    throw new TimeoutException(error);
                }
            }
        }
    }
}
