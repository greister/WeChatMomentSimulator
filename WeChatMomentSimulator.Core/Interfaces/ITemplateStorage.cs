using System.Collections.Generic;
using System.Threading.Tasks;
using WeChatMomentSimulator.Core.Models.Template;

namespace WeChatMomentSimulator.Core.Interfaces
{
    /// <summary>
    /// 模板存储接口
    /// </summary>
    public interface ITemplateStorage
    {
        /// <summary>
        /// 获取所有模板
        /// </summary>
        /// <returns>模板集合</returns>
        Task<IEnumerable<TemplateDefinition>> GetAllTemplatesAsync();
        
        /// <summary>
        /// 获取特定ID的模板
        /// </summary>
        /// <param name="id">模板ID</param>
        /// <returns>模板定义，如果不存在则返回null</returns>
        Task<TemplateDefinition> GetTemplateAsync(string id);
        
        /// <summary>
        /// 保存模板
        /// </summary>
        /// <param name="template">要保存的模板</param>
        Task SaveTemplateAsync(TemplateDefinition template);
        
        /// <summary>
        /// 更新现有模板
        /// </summary>
        /// <param name="template">更新后的模板</param>
        /// <returns>是否成功更新</returns>
        Task<bool> UpdateTemplateAsync(TemplateDefinition template);
        
        /// <summary>
        /// 删除模板
        /// </summary>
        /// <param name="id">要删除的模板ID</param>
        Task DeleteTemplateAsync(string id);
        
        /// <summary>
        /// 导出模板到指定文件
        /// </summary>
        /// <param name="id">模板ID</param>
        /// <param name="filePath">导出文件路径</param>
        Task ExportTemplateAsync(string id, string filePath);
        
        /// <summary>
        /// 导入模板
        /// </summary>
        /// <param name="filePath">模板文件路径</param>
        /// <returns>导入的模板定义</returns>
        Task<TemplateDefinition> ImportTemplateAsync(string filePath);
        
        /// <summary>
        /// 搜索模板
        /// </summary>
        /// <param name="keyword">搜索关键词</param>
        /// <returns>匹配的模板集合</returns>
        Task<IEnumerable<TemplateDefinition>> SearchTemplatesAsync(string keyword);
        
        /// <summary>
        /// 获取指定标签的模板
        /// </summary>
        /// <param name="tag">标签名</param>
        /// <returns>匹配的模板集合</returns>
        Task<IEnumerable<TemplateDefinition>> GetTemplatesByTagAsync(string tag);
        
        /// <summary>
        /// 获取指定分类的模板
        /// </summary>
        /// <param name="category">分类名</param>
        /// <returns>匹配的模板集合</returns>
        Task<IEnumerable<TemplateDefinition>> GetTemplatesByCategoryAsync(string category);
        
        /// <summary>
        /// 获取模板文件中引用的所有资源文件路径
        /// </summary>
        /// <param name="id">模板ID</param>
        /// <returns>资源文件路径集合</returns>
        Task<IEnumerable<string>> GetTemplateResourcesAsync(string id);

        Task UpdateTemplateThumbnailAsync(Guid newTemplateId, object sourceThumbnailPath);
        Task<string?> GetTemplateThumbnailPathAsync(Guid id);
    }
}