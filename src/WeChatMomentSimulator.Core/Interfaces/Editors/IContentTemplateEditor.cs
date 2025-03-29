using System.Collections.Generic;
using System.Threading.Tasks;
using WeChatMomentSimulator.Core.Models.Template;

namespace WeChatMomentSimulator.Core.Interfaces.Editors
{
    public interface IContentTemplateEditor : ITemplateEditor
    {
        /// <summary>
        /// 获取模板中的占位符定义
        /// </summary>
        Task<IList<PlaceholderDefinition>> GetPlaceholderDefinitionsAsync();
    
        /// <summary>
        /// 更新占位符定义
        /// </summary>
        Task<bool> UpdatePlaceholderDefinitionsAsync(IList<PlaceholderDefinition> definitions);
    
        /// <summary>
        /// 获取模板内容区域
        /// </summary>
        Task<ContentAreaDefinition> GetContentAreaAsync();
    
        /// <summary>
        /// 更新模板内容区域
        /// </summary>
        Task<bool> UpdateContentAreaAsync(ContentAreaDefinition contentArea);
    }
}