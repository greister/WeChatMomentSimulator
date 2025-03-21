using Serilog.Events;
using WeChatMomentSimulator.Core.Logging;

public class LogEntry
{
    public LogEntry(LogEvent logEvent)
    {
        Timestamp = logEvent.Timestamp.LocalDateTime;
        Level = logEvent.Level.ToString();
        Source = logEvent.Properties.TryGetValue("SourceContext", out var source) 
            ? source.ToString().Trim('"') : "Unknown";
        InstanceId = GetInstanceId(logEvent);
        Message = logEvent.RenderMessage();
        Exception = logEvent.Exception;
        Color = LogColors.LevelColors[logEvent.Level];
    }

    private static string GetInstanceId(LogEvent logEvent)
    {
        if (logEvent.Properties.TryGetValue("InstanceId", out var instanceId))
            return instanceId.ToString();
        
        // 如果没有提供InstanceId，生成一个简短的随机ID
        return Guid.NewGuid().ToString("N")[..8];
    }

    public DateTime Timestamp { get; }
    public string Level { get; }
    public string Source { get; }
    public string InstanceId { get; }
    public string Message { get; }
    public Exception? Exception { get; }
    public string Color { get; }
}