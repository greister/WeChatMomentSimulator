using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using WeChatMomentSimulator.Core.Interfaces.Rendering;

namespace WeChatMomentSimulator.Core.Interfaces.Editors
{
    public interface ITemplateCombiner
    {
        /// <summary>
        /// 将内容模板与手机模板组合生成最终效果
        /// </summary>
        Task<string> CombineTemplatesAsync(string contentTemplateId, string phoneTemplateId);
    
        /// <summary>
        /// 使用提供的参数值预览组合后的模板
        /// </summary>
        Task<string> PreviewCombinedTemplateAsync(string contentTemplateId, string phoneTemplateId, 
            IDictionary<string, object> placeholderValues);
        
        /// <summary>
        /// 验证占位符值是否符合定义
        /// </summary>
        Task<(bool IsValid, IDictionary<string, string> Errors)> ValidatePlaceholderValuesAsync(
            string templateId, IDictionary<string, object> values);
    }
}