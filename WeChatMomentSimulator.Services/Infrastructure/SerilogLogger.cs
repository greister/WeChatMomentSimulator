using Serilog;
using WeChatMomentSimulator.Core.Interfaces;

namespace WeChatMomentSimulator.Services.Infrastructure
{
    public class SerilogLogger : IAppLogger
    {
        // 静态配置方法 - 在应用启动时调用一次
        public static void Configure(bool isDevelopment)
        {
            // 强制启用调试级别和控制台输出
            var config = new LoggerConfiguration()
                .MinimumLevel.Debug() // 确保记录所有级别
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
                    theme: Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme.Code); // 使用ANSI彩色输出

            // 应用配置
            Log.Logger = config.CreateLogger();
    
            // 测试日志记录
            Log.Debug("调试信息测试");
            Log.Information("信息测试");
            Log.Warning("警告测试");
            Log.Error("错误测试");
        }
        
        // 实例方法 - 供应用程序组件使用
        public void Debug(string messageTemplate, params object[] propertyValues)
        {
            Log.Debug(messageTemplate, propertyValues);
        }

        public void Info(string messageTemplate, params object[] propertyValues)
        {
            Log.Information(messageTemplate, propertyValues);
        }

        public void Warning(string messageTemplate, params object[] propertyValues)
        {
            Log.Warning(messageTemplate, propertyValues);
        }

        public void Error(string messageTemplate, params object[] propertyValues)
        {
            Log.Error(messageTemplate, propertyValues);
        }

        public void Error(System.Exception exception, string messageTemplate, 
            params object[] propertyValues)
        {
            Log.Error(exception, messageTemplate, propertyValues);
        }
        
        // 应用关闭时调用
        public static void CloseAndFlush()
        {
            Log.CloseAndFlush();
        }
    }
}