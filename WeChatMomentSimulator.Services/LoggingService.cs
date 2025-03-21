// WeChatMomentSimulator.Services/Infrastructure/SimpleLoggingService.cs
using System;
using System.IO;
using Serilog;
using WeChatMomentSimulator.Core.Interfaces;

namespace WeChatMomentSimulator.Services.Infrastructure
{
    public class SimpleLoggingService : ILoggingService
    {
        public SimpleLoggingService(bool isDevMode = false)
        {
            SetDevMode(isDevMode);
        }

        public void SetDevMode(bool isDevMode)
        {
            // 创建日志目录
            var logDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "WeChatMomentSimulator", "logs");
                
            Directory.CreateDirectory(logDir);
            
            // 初始化Serilog
            var logConfig = new LoggerConfiguration()
                .WriteTo.File(Path.Combine(logDir, "app-.log"), 
                    rollingInterval: RollingInterval.Day);

            // 开发模式特定配置
            if (isDevMode)
            {
                logConfig = logConfig
                    .MinimumLevel.Debug()
                    .WriteTo.Console();
            }
            else
            {
                logConfig = logConfig.MinimumLevel.Information();
            }
            
            Log.Logger = logConfig.CreateLogger();
        }

        public void Debug(string message)
        {
            Log.Debug(message);
        }

        public void Info(string message)
        {
            Log.Information(message);
        }

        public void Warning(string message)
        {
            Log.Warning(message);
        }

        public void Error(string message, Exception exception = null)
        {
            if (exception != null)
                Log.Error(exception, message);
            else
                Log.Error(message);
        }
    }
}