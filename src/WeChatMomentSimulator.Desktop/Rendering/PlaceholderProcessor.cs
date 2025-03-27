using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace WeChatMomentSimulator.Desktop.Rendering
{
    /// <summary>
    /// 占位符处理器，负责解析和替换模板中的各种占位符
    /// </summary>
    public class PlaceholderProcessor
    {
        /// <summary>
        /// 处理模板中的所有占位符
        /// </summary>
        public string Process(string template, Dictionary<string, object> data)
        {
            if (string.IsNullOrEmpty(template) || data == null)
                return template;
                
            // 按顺序处理不同类型的占位符
            string result = template;
            
            // 1. 处理条件块 {{#if condition}}...{{/if}}
            result = ProcessConditionalBlocks(result, data);
            
            // 2. 处理循环块 {{#each items}}...{{/each}}
            result = ProcessLoopBlocks(result, data);
            
            // 3. 处理简单占位符 {{name}}
            result = ProcessSimplePlaceholders(result, data);
            
            return result;
        }
        
        /// <summary>
        /// 提取模板中使用的所有占位符
        /// </summary>
        public List<string> ExtractPlaceholders(string template)
        {
            var result = new List<string>();
            
            // 匹配简单占位符 {{name}}
            var regex = new Regex(@"\{\{([^{}#\/]+?)\}\}");
            var matches = regex.Matches(template);
            
            foreach (Match match in matches)
            {
                string placeholder = match.Groups[1].Value.Trim();
                if (!result.Contains(placeholder))
                {
                    result.Add(placeholder);
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// 处理条件块
        /// </summary>
        private string ProcessConditionalBlocks(string template, Dictionary<string, object> data)
        {
            // 匹配条件块 {{#if condition}}...{{/if}}
            return Regex.Replace(template, 
                @"\{\{#if\s+([^{}]+?)\}\}(.*?)\{\{\/if\}\}", 
                match =>
                {
                    string condition = match.Groups[1].Value.Trim();
                    string content = match.Groups[2].Value;
                    
                    // 评估条件
                    bool conditionMet = EvaluateCondition(condition, data);
                    
                    // 条件为真时返回内容，否则返回空字符串
                    return conditionMet ? content : string.Empty;
                },
                RegexOptions.Singleline);
        }
        
        /// <summary>
        /// 处理循环块
        /// </summary>
        private string ProcessLoopBlocks(string template, Dictionary<string, object> data)
        {
            // 匹配循环块 {{#each items}}...{{/each}}
            return Regex.Replace(template,
                @"\{\{#each\s+([^{}]+?)\}\}(.*?)\{\{\/each\}\}",
                match =>
                {
                    string collectionName = match.Groups[1].Value.Trim();
                    string itemTemplate = match.Groups[2].Value;
                    
                    // 获取集合数据
                    if (!data.TryGetValue(collectionName, out object collectionObj) || 
                        !(collectionObj is IEnumerable<object> collection))
                    {
                        return string.Empty;
                    }
                    
                    var result = new StringBuilder();
                    int index = 0;
                    
                    // 处理集合中的每一项
                    foreach (var item in collection)
                    {
                        // 为每个项创建新的数据上下文
                        var itemData = new Dictionary<string, object>(data)
                        {
                            ["item"] = item,
                            ["index"] = index++
                        };
                        
                        // 递归处理项模板
                        string processedItem = Process(itemTemplate, itemData);
                        result.Append(processedItem);
                    }
                    
                    return result.ToString();
                },
                RegexOptions.Singleline);
        }
        
        /// <summary>
        /// 处理简单占位符
        /// </summary>
        private string ProcessSimplePlaceholders(string template, Dictionary<string, object> data)
        {
            // 匹配简单占位符 {{name}}
            return Regex.Replace(template, 
                @"\{\{([^{}#\/]+?)\}\}", 
                match =>
                {
                    string key = match.Groups[1].Value.Trim();
                    
                    // 处理嵌套属性 item.name
                    if (key.Contains("."))
                    {
                        string[] parts = key.Split('.');
                        if (parts.Length == 2 && 
                            data.TryGetValue(parts[0], out object obj) && 
                            obj is Dictionary<string, object> dict)
                        {
                            if (dict.TryGetValue(parts[1], out object value))
                            {
                                return value?.ToString() ?? string.Empty;
                            }
                        }
                    }
                    
                    // 处理普通属性
                    if (data.TryGetValue(key, out object result))
                    {
                        return result?.ToString() ?? string.Empty;
                    }
                    
                    // 未找到值时保留原占位符
                    return match.Value;
                });
        }
        
        /// <summary>
        /// 评估条件表达式
        /// </summary>
        private bool EvaluateCondition(string condition, Dictionary<string, object> data)
        {
            // 处理否定条件
            if (condition.StartsWith("!") && condition.Length > 1)
            {
                string actualCondition = condition.Substring(1);
                return !EvaluateCondition(actualCondition, data);
            }
            
            // 处理等式条件 (key==value)
            if (condition.Contains("=="))
            {
                string[] parts = condition.Split(new[] { "==" }, StringSplitOptions.None);
                if (parts.Length == 2)
                {
                    string leftKey = parts[0].Trim();
                    string rightValue = parts[1].Trim();
                    
                    // 左侧是数据键
                    if (data.TryGetValue(leftKey, out object leftObj))
                    {
                        string leftValue = leftObj?.ToString() ?? string.Empty;
                        return leftValue == rightValue;
                    }
                    return false;
                }
            }
            
            // 处理简单条件 (键存在且为真值)
            if (data.TryGetValue(condition, out object value))
            {
                if (value is bool boolValue)
                    return boolValue;
                
                if (value is int intValue)
                    return intValue != 0;
                
                if (value is string strValue)
                    return !string.IsNullOrEmpty(strValue);
                
                return value != null;
            }
            
            return false;
        }
    }
}