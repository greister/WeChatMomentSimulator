using Microsoft.Extensions.Logging;
using Serilog;
using System.Collections.Concurrent;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace WeChatMomentSimulator.Core.Logging
{
    /// <summary>
    /// 提供日志记录扩展方法
    /// </summary>
    public static class LoggerExtensions
    {
        
        private static readonly ConcurrentDictionary<Type, ILogger> _loggerCache = new();

        public static ILogger<T> AsILogger<T>(this Serilog.ILogger serilogLogger)
        {
            if (serilogLogger == null)
                throw new ArgumentNullException(nameof(serilogLogger));

            var type = typeof(T);
            return (ILogger<T>)_loggerCache.GetOrAdd(type, t =>
            {
                var loggerFactory = LoggerFactory.Create(builder => 
                    builder.AddSerilog(serilogLogger));
                return loggerFactory.CreateLogger<T>();
            });
        }
        /// <summary>
        /// 获取当前类型的日志记录器
        /// </summary>
        public static ILogger<T> GetLogger<T>()
        {
            return Log.ForContext<T>().AsILogger<T>();
        }

        

        
    }
}