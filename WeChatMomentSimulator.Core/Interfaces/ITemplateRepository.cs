using System.Collections.Generic;
using System.Threading.Tasks;
using WeChatMomentSimulator.Core.Models;
using WeChatMomentSimulator.Core.Models.Enums;

namespace WeChatMomentSimulator.Core.Interfaces
{
    public interface ITemplateRepository
    {
        Task<IEnumerable<BaseTemplate>> GetAllTemplatesAsync();
        Task<BaseTemplate> GetTemplateByIdAsync(string id);
        Task<IEnumerable<BaseTemplate>> GetTemplatesByTypeAsync(TemplateType type);
        Task SaveTemplateAsync(BaseTemplate template);
        Task DeleteTemplateAsync(string id);
    }
}