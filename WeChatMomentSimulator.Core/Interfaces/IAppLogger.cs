namespace WeChatMomentSimulator.Core.Interfaces
{
    public interface IAppLogger
    {
        void Debug(string messageTemplate, params object[] propertyValues);
        void Info(string messageTemplate, params object[] propertyValues);
        void Warning(string messageTemplate, params object[] propertyValues);
        void Error(string messageTemplate, params object[] propertyValues);
        void Error(System.Exception exception, string messageTemplate, params object[] propertyValues);
    }
}