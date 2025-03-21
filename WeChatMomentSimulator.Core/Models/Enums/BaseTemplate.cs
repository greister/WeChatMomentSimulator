// Models/BaseTemplate.cs
using System.Collections.Generic;
using WeChatMomentSimulator.Core.Models.Enums;

namespace WeChatMomentSimulator.Core.Models
{
    public abstract class BaseTemplate
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public TemplateType Type { get; set; }
        public string FilePath { get; set; }
        public string SvgContent { get; set; }
        
        public StatusBarSettings StatusBarSettings { get; set; }
        public Dictionary<string, string> Metadata { get; set; }
        public List<PlaceholderDefinition> Placeholders { get; set; }
        
        public BaseTemplate()
        {
            Metadata = new Dictionary<string, string>();
            Placeholders = new List<PlaceholderDefinition>();
            StatusBarSettings = new StatusBarSettings();
        }
    }
}