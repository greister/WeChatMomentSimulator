using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using WeChatMomentSimulator.Core.Interfaces;
using WeChatMomentSimulator.Core.Models.Template;
using WeChatMomentSimulator.Core.Logging;
using LoggerExtensions = WeChatMomentSimulator.Core.Logging.LoggerExtensions;

namespace WeChatMomentSimulator.Services.Services
{
    /// <summary>
    /// 占位符解析器实现
    /// </summary>
    public class PlaceholderParser : IPlaceholderParser
    {
        private readonly ILogger<PlaceholderParser> _logger;
        
        // 匹配简单占位符的正则表达式 {{name}}
        private static readonly Regex SimplePattern = new Regex(@"\{\{([^{}#\/]+?)\}\}", RegexOptions.Compiled);
        
        // 匹配条件块的正则表达式 {{#if condition}}...{{/if}}
        private static readonly Regex ConditionPattern = 
            new Regex(@"\{\{#if\s+([^{}]+?)\}\}(.*?)\{\{\/if\}\}", RegexOptions.Compiled | RegexOptions.Singleline);
        
        // 匹配循环块的正则表达式 {{#each items}}...{{/each}}
        private static readonly Regex LoopPattern = 
            new Regex(@"\{\{#each\s+([^{}]+?)\}\}(.*?)\{\{\/each\}\}", RegexOptions.Compiled | RegexOptions.Singleline);

        public PlaceholderParser()
        {
            _logger = LoggerExtensions.GetLogger<PlaceholderParser>();
        }

        /// <inheritdoc/>
        public IEnumerable<string> ExtractPlaceholders(string templateText)
        {
            if (string.IsNullOrEmpty(templateText))
            {
                _logger.LogWarning("提取占位符时收到空模板");
                return Enumerable.Empty<string>();
            }

            var placeholders = new HashSet<string>();
            
            // 提取简单占位符
            foreach (Match match in SimplePattern.Matches(templateText))
            {
                if (match.Groups.Count > 1)
                {
                    placeholders.Add(match.Groups[1].Value.Trim());
                }
            }
            
            // 提取条件占位符中的变量
            foreach (Match match in ConditionPattern.Matches(templateText))
            {
                if (match.Groups.Count > 1)
                {
                    placeholders.Add(match.Groups[1].Value.Trim());
                    
                    // 递归提取条件块内部的占位符
                    if (match.Groups.Count > 2)
                    {
                        foreach (var placeholder in ExtractPlaceholders(match.Groups[2].Value))
                        {
                            placeholders.Add(placeholder);
                        }
                    }
                }
            }
            
            // 提取循环块占位符
            foreach (Match match in LoopPattern.Matches(templateText))
            {
                if (match.Groups.Count > 1)
                {
                    placeholders.Add(match.Groups[1].Value.Trim());
                    
                    // 递归提取循环块内部的占位符
                    if (match.Groups.Count > 2)
                    {
                        foreach (var placeholder in ExtractPlaceholders(match.Groups[2].Value))
                        {
                            if (!placeholder.Equals("item") && !placeholder.Equals("index"))
                            {
                                placeholders.Add(placeholder);
                            }
                        }
                    }
                }
            }
            
            return placeholders;
        }

        /// <inheritdoc/>
        public string ReplacePlaceholders(string templateText, IDictionary<string, object> values)
        {
            if (string.IsNullOrEmpty(templateText))
            {
                _logger.LogWarning("替换占位符时收到空模板");
                return string.Empty;
            }

            if (values == null)
            {
                _logger.LogWarning("替换占位符时收到空值字典");
                return templateText;
            }

            // 处理条件块
            string processedTemplate = ProcessConditionBlocks(templateText, values);
            
            // 处理循环块
            processedTemplate = ProcessLoopBlocks(processedTemplate, values);
            
            // 处理简单占位符
            processedTemplate = ProcessSimplePlaceholders(processedTemplate, values);
            
            return processedTemplate;
        }

        /// <inheritdoc/>
        public IEnumerable<PlaceholderDefinition> GetUsedPlaceholderDefinitions(
            string templateText, 
            IEnumerable<PlaceholderDefinition> availablePlaceholders)
        {
            // 提取模板中使用的所有占位符名称
            var usedPlaceholderNames = ExtractPlaceholders(templateText);
            
            // 找出已定义的且在模板中使用的占位符定义
            return availablePlaceholders.Where(p => usedPlaceholderNames.Contains(p.Name));
        }
        
        /// <summary>
        /// 处理简单占位符
        /// </summary>
        private string ProcessSimplePlaceholders(string template, IDictionary<string, object> values)
        {
            return SimplePattern.Replace(template, match =>
            {
                string placeholder = match.Groups[1].Value.Trim();
                
                if (values.TryGetValue(placeholder, out object value))
                {
                    return value?.ToString() ?? string.Empty;
                }
                
                _logger.LogDebug("占位符 '{Placeholder}' 在数据中不存在", placeholder);
                return match.Value; // 保留未替换的占位符
            });
        }
        
        /// <summary>
        /// 处理条件块
        /// </summary>
        private string ProcessConditionBlocks(string template, IDictionary<string, object> values)
        {
            return ConditionPattern.Replace(template, match =>
            {
                string condition = match.Groups[1].Value.Trim();
                string content = match.Groups[2].Value;
                
                bool conditionMet = false;
                
                if (values.TryGetValue(condition, out object value))
                {
                    if (value is bool boolValue)
                    {
                        conditionMet = boolValue;
                    }
                    else
                    {
                        // 非布尔类型值，检查非空、非零等
                        conditionMet = value != null && 
                                      !string.Equals(value.ToString(), "0") && 
                                      !string.Equals(value.ToString(), string.Empty);
                    }
                }
                
                if (conditionMet)
                {
                    // 条件成立，递归处理内容
                    return ReplacePlaceholders(content, values);
                }
                
                return string.Empty; // 条件不成立，返回空
            });
        }
        
        /// <summary>
        /// 处理循环块
        /// </summary>
        private string ProcessLoopBlocks(string template, IDictionary<string, object> values)
        {
            return LoopPattern.Replace(template, match =>
            {
                string collectionName = match.Groups[1].Value.Trim();
                string itemTemplate = match.Groups[2].Value;
                
                if (!values.TryGetValue(collectionName, out object collectionObj) || 
                    !(collectionObj is IEnumerable<object> collection))
                {
                    _logger.LogWarning("循环占位符 '{CollectionName}' 不是有效的集合", collectionName);
                    return string.Empty;
                }
                
                var result = new StringBuilder();
                int index = 0;
                
                foreach (var item in collection)
                {
                    // 为每个项创建新的数据上下文
                    var itemData = new Dictionary<string, object>(values)
                    {
                        ["item"] = item,
                        ["index"] = index++
                    };
                    
                    // 递归处理项模板
                    result.Append(ReplacePlaceholders(itemTemplate, itemData));
                }
                
                return result.ToString();
            });
        }
    }
}