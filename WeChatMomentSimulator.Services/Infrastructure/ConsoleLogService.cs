using System;
using WeChatMomentSimulator.Core.Interfaces;

namespace WeChatMomentSimulator.Services.Infrastructure
{
    public class ConsoleLogService : ILoggingService
    {
        private ILoggingService _loggingServiceImplementation;

        public void Debug(string message)
        {
            WriteWithColor(message, ConsoleColor.Gray, "DEBUG");
        }

        public void Info(string message)
        {
            WriteWithColor(message, ConsoleColor.White, "INFO");
        }

        public void Warning(string message)
        {
            WriteWithColor(message, ConsoleColor.Yellow, "WARN");
        }

        public void Error(string message, Exception exception = null)
        {
            WriteWithColor(message, ConsoleColor.Red, "ERROR");
            if (exception != null)
            {
                WriteWithColor(exception.ToString(), ConsoleColor.DarkRed, "ERROR");
            }
        }

        public void SetDevMode(bool isDevMode)
        {
            _loggingServiceImplementation.SetDevMode(isDevMode);
        }

        private void WriteWithColor(string message, ConsoleColor color, string level)
        {
            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] [{level}] {message}");
            Console.ForegroundColor = originalColor;
        }
    }
}