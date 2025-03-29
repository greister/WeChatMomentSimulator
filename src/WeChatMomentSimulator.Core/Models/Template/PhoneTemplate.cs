using WeChatMomentSimulator.Core.Enums;

namespace WeChatMomentSimulator.Core.Models.Template;

public class PhoneTemplate
{
    public TemplateTypes.DeviceType DeviceType { get; set; }
    public ContentAreaDefinition ContentArea { get; set; } = new ContentAreaDefinition();
    public StatusBarParameters DefaultStatusBarParameters { get; set; } = new StatusBarParameters();

}