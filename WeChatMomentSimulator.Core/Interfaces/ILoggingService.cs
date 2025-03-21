// WeChatMomentSimulator.Core/Interfaces/ILoggingService.cs
namespace WeChatMomentSimulator.Core.Interfaces
{
    public interface ILoggingService
    {
        void Debug(string message);
        void Info(string message);
        void Warning(string message);
        void Error(string message, System.Exception exception = null);
        void SetDevMode(bool isDevMode);
    }
}

