// 文件路径: WeChatMomentSimulator.Core/Models/WechatMomentTemplate.cs
using WeChatMomentSimulator.Core.Models.Enums;

namespace WeChatMomentSimulator.Core.Models
{
    public class WechatMomentTemplate : BaseTemplate
    {
        public bool SupportMultipleContents { get; set; } = true;
        public MomentLayoutSettings LayoutSettings { get; set; }
        
        public WechatMomentTemplate()
        {
            Type = TemplateType.WechatMoment;
            LayoutSettings = new MomentLayoutSettings();
        }
    }
    
    public class MomentLayoutSettings
    {
        public int AvatarSize { get; set; } = 40;
        public int ContentSpacing { get; set; } = 10;
        public string FontFamily { get; set; } = "Microsoft YaHei";
        public int TitleFontSize { get; set; } = 16;
        public int ContentFontSize { get; set; } = 14;
        public int TimestampFontSize { get; set; } = 12;
        public string TimestampColor { get; set; } = "#999999";
    }
}