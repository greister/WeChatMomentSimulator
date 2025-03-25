using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WeChatMomentSimulator.Core.Models.Template;
using WeChatMomentSimulator.Core.Models.Template.Enums;

namespace WeChatMomentSimulator.Core.Interfaces.Services
{
    /// <summary>
    /// 模板服务接口
    /// </summary>
    public interface ITemplateService
    {
        /// <summary>
        /// 获取所有模板
        /// </summary>
        Task<IEnumerable<Template>> GetAllTemplatesAsync();

        /// <summary>
        /// 根据ID获取模板
        /// </summary>
        Task<Template> GetTemplateByIdAsync(Guid id);

        /// <summary>
        /// 根据类型获取模板
        /// </summary>
        Task<IEnumerable<Template>> GetTemplatesByTypeAsync(TemplateType templateType);

        /// <summary>
        /// 获取默认模板
        /// </summary>
        Task<Template> GetDefaultTemplateAsync(TemplateType templateType);

        /// <summary>
        /// 加载模板内容
        /// </summary>
        Task<Template> LoadTemplateContentAsync(Template template);

        /// <summary>
        /// 保存模板
        /// </summary>
        Task<Template> SaveTemplateAsync(Template template);

        /// <summary>
        /// 删除模板
        /// </summary>
        Task<bool> DeleteTemplateAsync(Guid id);

        /// <summary>
        /// 应用变量值到模板内容
        /// </summary>
        string ApplyVariables(Template template, Dictionary<string, object> variableValues);
    }
}