using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Concurrent;
using System.IO;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace WeChatMomentSimulator.Core.Logging
{
    /// <summary>
    /// 日志提供程序 - 配置 Serilog 并提供 Microsoft ILogger
    /// </summary>
    public static class LogProvider
    {
        // 缓存不同类型的 ILogger 实例
        private static readonly ConcurrentDictionary<Type, object> LoggerCache = new();
        
        // 日志工厂
        private static ILoggerFactory _loggerFactory;
        
        // Serilog 日志级别映射
        private static readonly IReadOnlyDictionary<string, LogEventLevel> LevelMap = 
            new Dictionary<string, LogEventLevel>(StringComparer.OrdinalIgnoreCase)
            {
                ["Verbose"] = LogEventLevel.Verbose,
                ["Debug"] = LogEventLevel.Debug,
                ["Information"] = LogEventLevel.Information,
                ["Warning"] = LogEventLevel.Warning,
                ["Error"] = LogEventLevel.Error,
                ["Fatal"] = LogEventLevel.Fatal
            };

        /// <summary>
        /// 初始化日志系统并应用默认配置
        /// </summary>
        public static void Initialize(string logFilePath = null)
        {
            // 默认日志文件路径
            logFilePath ??= Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "app-.log");
            
            // 确保日志目录存在
            string logDirectory = Path.GetDirectoryName(logFilePath);
            if (!string.IsNullOrEmpty(logDirectory) && !Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }

            // 配置 Serilog
            Logger serilogLogger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .Enrich.WithProcessId()
                .Enrich.WithEnvironmentUserName()
                .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext} {Message:lj}{NewLine}{Exception}")
                .WriteTo.File(
                    logFilePath,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 31,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] ({ThreadId}) {SourceContext} {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            // 设置全局 Serilog 记录器
            Log.Logger = serilogLogger;

            // 创建 LoggerFactory 并添加 Serilog 提供程序
            _loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddSerilog(serilogLogger, dispose: true);
            });

            // 记录初始化日志
            Log.Information("日志系统已初始化 | 日志文件: {LogFilePath}", logFilePath);
        }

        /// <summary>
        /// 使用配置对象初始化日志系统
        /// </summary>
        /// <param name="configuration">配置对象</param>
        public static void Initialize(IConfiguration configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException(nameof(configuration));

            // 从配置中获取日志配置
            var logConfig = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .Enrich.WithProcessId();

            // 设置全局 Serilog 记录器
            Log.Logger = logConfig.CreateLogger();

            // 创建 LoggerFactory 并添加 Serilog 提供程序
            _loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddSerilog(Log.Logger, dispose: true);
            });

            Log.Information("日志系统已通过配置初始化");
        }

        /// <summary>
        /// 使用高级选项初始化日志系统
        /// </summary>
        public static void Initialize(Action<LoggerConfiguration> configureLogger)
        {
            if (configureLogger == null)
                throw new ArgumentNullException(nameof(configureLogger));

            // 创建基础配置
            var loggerConfig = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId();

            // 应用自定义配置
            configureLogger(loggerConfig);

            // 设置全局 Serilog 记录器
            Log.Logger = loggerConfig.CreateLogger();

            // 创建 LoggerFactory 并添加 Serilog 提供程序
            _loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddSerilog(Log.Logger, dispose: true);
            });

            Log.Information("日志系统已使用自定义配置初始化");
        }

        /// <summary>
        /// 获取特定类型的 Microsoft ILogger
        /// </summary>
        public static ILogger<T> GetLogger<T>()
        {
            if (_loggerFactory == null)
                Initialize(); // 如果未初始化，使用默认设置初始化

            // 从缓存获取或创建新的 logger
            return (ILogger<T>)LoggerCache.GetOrAdd(typeof(T), t => 
                _loggerFactory.CreateLogger<T>());
        }

        /// <summary>
        /// 获取指定类型名称的 Microsoft ILogger
        /// </summary>
        public static ILogger GetLogger(string categoryName)
        {
            if (_loggerFactory == null)
                Initialize(); // 如果未初始化，使用默认设置初始化

            return _loggerFactory.CreateLogger(categoryName);
        }

        /// <summary>
        /// 设置全局最小日志级别
        /// </summary>
        public static void SetMinimumLevel(string levelName)
        {
            if (LevelMap.TryGetValue(levelName, out LogEventLevel level))
            {
                Serilog.Core.Logger logger = (Serilog.Core.Logger)Log.Logger;
                
                // 通过反射修改最小日志级别（注意：这是一个高级技巧，不是标准做法）
                // 在理想情况下，应该重新配置日志系统
                var levelSwitch = new LoggingLevelSwitch(level);
                typeof(Logger).GetField("_levelSwitch", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.SetValue(logger, levelSwitch);
                
                Log.Information("全局日志级别已设置为: {LogLevel}", levelName);
            }
            else
            {
                Log.Warning("未知的日志级别名称: {LogLevelName}", levelName);
            }
        }

        /// <summary>
        /// 关闭并释放日志系统资源
        /// </summary>
        public static void Shutdown()
        {
            Log.Information("正在关闭日志系统...");
            Log.CloseAndFlush();
        }
    }
}