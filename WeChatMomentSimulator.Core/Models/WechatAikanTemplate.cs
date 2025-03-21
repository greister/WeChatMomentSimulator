// 文件路径: WeChatMomentSimulator.Core/Models/WechatAikanTemplate.cs
using WeChatMomentSimulator.Core.Models.Enums;

namespace WeChatMomentSimulator.Core.Models
{
    public class WechatAikanTemplate : BaseTemplate
    {
        public AikanLayoutSettings LayoutSettings { get; set; }
        public string LogoPath { get; set; }
        
        public WechatAikanTemplate()
        {
            Type = TemplateType.WechatAikan;
            LayoutSettings = new AikanLayoutSettings();
        }
    }
    
    public class AikanLayoutSettings
    {
        public int HeaderHeight { get; set; } = 45;
        public string HeaderBackgroundColor { get; set; } = "#FFFFFF";
        public int TitleFontSize { get; set; } = 16;
        public string TitleColor { get; set; } = "#333333";
        public int ImageHeight { get; set; } = 180;
        public int ImageWidth { get; set; } = 300;
        public int ImageCornerRadius { get; set; } = 5;
        public string FontFamily { get; set; } = "Microsoft YaHei";
        public string BackgroundColor { get; set; } = "#F8F8F8";
    }
}