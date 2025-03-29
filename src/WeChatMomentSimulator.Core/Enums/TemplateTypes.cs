namespace WeChatMomentSimulator.Core.Enums;

public class TemplateTypes
{
    public enum TemplateType
    {
        Phone,
        Content
    }
    
    public enum DeviceType
    {
        iPhone,
        Android
    }
    
    public enum ContentType
    {
        WeChatMoment,
        Aikan,
        Other
    }
    
    public enum PlaceholderType
    {
        Text,
        Image,
        Avatar,
        DateTime
    }
}