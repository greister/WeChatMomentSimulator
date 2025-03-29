using System.Collections.Generic;
using System.Threading.Tasks;
using WeChatMomentSimulator.Core.Enums;
using WeChatMomentSimulator.Core.Models.Template;

namespace WeChatMomentSimulator.Core.Interfaces.Editors
{
    public interface IPhoneTemplateEditor : ITemplateEditor
    {
        /// <summary>
        /// 获取当前设备模板
        /// </summary>
        Task<PhoneTemplate> GetPhoneTemplateAsync();
    
        /// <summary>
        /// 应用设备模板
        /// </summary>
        Task<bool> ApplyPhoneTemplateAsync(PhoneTemplate phoneTemplate);
    
        /// <summary>
        /// 获取可用设备类型列表
        /// </summary>
        Task<IList<TemplateTypes.DeviceType>> GetAvailableDeviceTypesAsync();
    
        /// <summary>
        /// 应用设备类型
        /// </summary>
        Task<bool> ApplyDeviceTypeAsync(TemplateTypes.DeviceType deviceType);
    
        /// <summary>
        /// 更新状态栏参数
        /// </summary>
        Task<bool> UpdateStatusBarParametersAsync(StatusBarParameters parameters);
    }
    
}