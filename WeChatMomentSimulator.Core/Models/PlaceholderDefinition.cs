// Models/PlaceholderDefinition.cs

using WeChatMomentSimulator.Core.Models.Enums;

namespace WeChatMomentSimulator.Core.Models
{
    public class PlaceholderDefinition
    {
        public string Key { get; set; }
        public string Description { get; set; }
        public string DefaultValue { get; set; }
        public PlaceholderType Type { get; set; }
        public bool IsRequired { get; set; } = true;

        public PlaceholderDefinition()
        {
        }

        public PlaceholderDefinition(string key, string description, string defaultValue, PlaceholderType type, bool isRequired = true)
        {
            Key = key;
            Description = description;
            DefaultValue = defaultValue;
            Type = type;
            IsRequired = isRequired;
        }
    }
}