using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WeChatMomentSimulator.Core.Models.Template;
using WeChatMomentSimulator.Core.Models.Template.Enums;

namespace WeChatMomentSimulator.Core.Interfaces.Repositories
{
    /// <summary>
    /// 模板存储库接口
    /// </summary>
    public interface ITemplateRepository
    {
        /// <summary>
        /// 获取所有模板
        /// </summary>
        Task<IEnumerable<Template>> GetAllAsync();

        /// <summary>
        /// 根据ID获取模板
        /// </summary>
        Task<Template> GetByIdAsync(Guid id);

        /// <summary>
        /// 根据类型获取模板
        /// </summary>
        Task<IEnumerable<Template>> GetByTypeAsync(TemplateType templateType);

        /// <summary>
        /// 添加模板
        /// </summary>
        Task<Template> AddAsync(Template template);

        /// <summary>
        /// 更新模板
        /// </summary>
        Task<Template> UpdateAsync(Template template);

        /// <summary>
        /// 删除模板
        /// </summary>
        Task<bool> DeleteAsync(Guid id);
    }
}