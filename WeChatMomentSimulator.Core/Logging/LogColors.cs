using Serilog.Events;

namespace WeChatMomentSimulator.Core.Logging
{
    public static class LogColors
    {
        public static readonly Dictionary<LogEventLevel, string> LevelColors = new()
        {
            [LogEventLevel.Verbose] = "#808080",  // 灰色
            [LogEventLevel.Debug] = "#0000FF",    // 蓝色
            [LogEventLevel.Information] = "#008000",  // 绿色
            [LogEventLevel.Warning] = "#FFA500",  // 橙色
            [LogEventLevel.Error] = "#FF0000",    // 红色
            [LogEventLevel.Fatal] = "#800000"     // 深红色
        };
    }
}