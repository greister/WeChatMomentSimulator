using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WeChatMomentSimulator.Core.Models.Template;
using WeChatMomentSimulator.Core.Models.Template.Enums;

namespace WeChatMomentSimulator.Core.Interfaces.Services
{
    /// <summary>
    /// 模板管理器接口
    /// </summary>
    public interface ITemplateManager
    {
        /// <summary>
        /// 获取所有可用模板
        /// </summary>
        Task<IEnumerable<String>> GetAllTemplatesAsync();
        
        /// <summary>
        /// 获取模板列表
        /// </summary>
        Task<IEnumerable<Template>> GetTemplatesAsync();
        
        /// <summary>
        /// 根据ID获取模板
        /// </summary>
        Task<Template> GetTemplateByIdAsync(Guid id);
        
        /// <summary>
        /// 根据名称获取模板
        /// </summary>
        Task<Template> GetTemplateByNameAsync(string templateName);
        
        /// <summary>
        /// 根据类型获取模板列表
        /// </summary>
        Task<IEnumerable<Template>> GetTemplatesByTypeAsync(TemplateType templateType);
        
        /// <summary>
        /// 获取默认模板
        /// </summary>
        Task<Template> GetDefaultTemplateAsync(TemplateType templateType);
        
        /// <summary>
        /// 保存模板
        /// </summary>
        /// <param name="templateName">模板名称</param>
        /// <param name="content">模板内容</param>
        Task SaveTemplateAsync(string templateName, string content);
        
        
        // <summary>
        /// 获取模板文件路径
        /// </summary>
        string GetTemplateFilePath(Template template);
        
        /// <summary>
        /// 删除模板
        /// </summary>
        Task<bool> DeleteTemplateByIdAsync(Guid id);
        
        /// <summary>
        /// 按照名称删除模板
        /// </summary>
        Task<bool> DeleteTemplateByNameAsync(string templateName);
        
        /// <summary>
        /// 检查模板是否存在
        /// </summary>
        Task<bool> TemplateExistsAsync(string templateName);
        
        /// <summary>
        /// 创建默认模板（如果不存在）
        /// </summary>
        Task CreateDefaultTemplatesAsync();
        
        /// <summary>
        /// 获取模板中使用的占位符定义
        /// </summary>
        Task<IEnumerable<PlaceholderDefinition>> GetTemplatePlaceholdersAsync(Guid templateId);
        
        /// <summary>
        /// 应用变量值到模板内容
        /// </summary>
        Task<string> ApplyVariablesAsync(Template template, Dictionary<string, object> variableValues);
        
        /// <summary>
        /// 导出模板
        /// </summary>
        Task<bool> ExportTemplateAsync(Guid templateId, string exportPath);
        
        /// <summary>
        /// 更新模板
        /// </summary>
        Task<Template> UpdateTemplateAsync(Template template);
        
        /// <summary>
        /// 导入模板
        /// </summary>
        Task<Template> ImportTemplateAsync(string importPath);
        
        /// <summary>
        /// 模板转换字符串
        /// </summary>
        Task<string> ConvertTemplateToStringAsync(Template template);
    }
}