using System;
using System.Collections.Generic;
using System.Linq;

namespace WeChatMomentSimulator.Core.Models.Template
{
    /// <summary>
    /// 占位符定义，描��模板中占位符的类型、行为和属性
    /// </summary>
    public class PlaceholderDefinition
    {
        /// <summary>
        /// id
        /// </summary>
        public string Id { get; set; } = Guid.NewGuid().ToString();
        
        /// <summary>
        /// 占位符名称
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// 占位符类型
        /// </summary>
        public PlaceholderType Type { get; set; }
        
        /// <summary>
        /// 占位符描述
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// 默认值
        /// </summary>
        public object DefaultValue { get; set; }
        
        /// <summary>
        /// 可选项（适用于列表、枚举等类型）
        /// </summary>
        public string[] Options { get; set; }
        
        /// <summary>
        /// 格式字符串（适用于日期、数字等）
        /// </summary>
        public string Format { get; set; }
        
        /// <summary>
        /// 是否必填
        /// </summary>
        public bool Required { get; set; } = false;
        
        /// <summary>
        /// 占位符分类（用于UI分组展示）
        /// </summary>
        public string Category { get; set; } = "通用";

        /// <summary>
        /// 通过名称和类型创建占位符定义
        /// </summary>
        /// <param name="name">占位符名称</param>
        /// <param name="type">占位符类型</param>
        public PlaceholderDefinition(string name, PlaceholderType type)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Type = type;
            
            // 根据类型设置默认值
            DefaultValue = GetDefaultValueForType(type);
        }
        
        /// <summary>
        /// 创建占位符定义
        /// </summary>
        public PlaceholderDefinition()
        {
            Name = string.Empty;
            Type = PlaceholderType.Text;
            Description = string.Empty;
        }
        
        /// <summary>
        /// 验证当前值是否符合占位符定义要求
        /// </summary>
        /// <param name="value">要验证的值</param>
        /// <param name="errorMessage">错误信息</param>
        /// <returns>是否有效</returns>
        public bool ValidateValue(object value, out string errorMessage)
        {
            // 检查必填项
            if (Required && value == null)
            {
                errorMessage = $"占位符 '{Name}' 是必填项";
                return false;
            }
            
            // 如果值为空但不是必填项，则通过验证
            if (value == null)
            {
                errorMessage = null;
                return true;
            }
            
            // 根据类型进行验证
            switch (Type)
            {
                case PlaceholderType.Text:
                    // 文本类型接受所有可转换为字符串的值
                    break;
                    
                case PlaceholderType.Number:
                    if (!(value is int || value is double || value is float || value is decimal))
                    {
                        errorMessage = $"占位符 '{Name}' 需要数字类型的值，但得到的是 {value.GetType().Name}";
                        return false;
                    }
                    break;
                    
                case PlaceholderType.DateTime:
                    if (!(value is DateTime))
                    {
                        errorMessage = $"占位符 '{Name}' 需要日期时间类型的值，但得到的是 {value.GetType().Name}";
                        return false;
                    }
                    break;
                    
                case PlaceholderType.Boolean:
                    if (!(value is bool))
                    {
                        errorMessage = $"占位符 '{Name}' 需要布尔类型的值，但得到的是 {value.GetType().Name}";
                        return false;
                    }
                    break;
                    
                case PlaceholderType.Image:
                    if (!(value is string))
                    {
                        errorMessage = $"占位符 '{Name}' 需要字符串类型的图片路径，但得到的是 {value.GetType().Name}";
                        return false;
                    }
                    break;
                    
                case PlaceholderType.List:
                    if (!(value is IEnumerable<object> || value is Array))
                    {
                        errorMessage = $"占位符 '{Name}' 需要列表类型的值，但得到的是 {value.GetType().Name}";
                        return false;
                    }
                    break;
            }
            
            // 验证选项限制（如果有）
            if (Options != null && Options.Length > 0 && value is string stringValue)
            {
                if (!Options.Contains(stringValue))
                {
                    errorMessage = $"占位符 '{Name}' 的值 '{stringValue}' 不在允许的选项范围内";
                    return false;
                }
            }
            
            errorMessage = null;
            return true;
        }
        
        /// <summary>
        /// 格式化值为显示字符串
        /// </summary>
        /// <param name="value">值</param>
        /// <returns>格式化后的字符串</returns>
        public string FormatValue(object value)
        {
            if (value == null)
            {
                return string.Empty;
            }
            
            switch (Type)
            {
                case PlaceholderType.DateTime:
                    if (value is DateTime dateTime)
                    {
                        return !string.IsNullOrEmpty(Format) 
                            ? dateTime.ToString(Format) 
                            : dateTime.ToString();
                    }
                    break;
                    
                case PlaceholderType.Number:
                    if (value is IFormattable formattable && !string.IsNullOrEmpty(Format))
                    {
                        return formattable.ToString(Format, null);
                    }
                    break;
            }
            
            return value.ToString();
        }
        
        /// <summary>
        /// 克隆占位符定义
        /// </summary>
        /// <returns>克隆的定义</returns>
        public PlaceholderDefinition Clone()
        {
            return new PlaceholderDefinition
            {
                Name = Name,
                Type = Type,
                Description = Description,
                DefaultValue = DefaultValue,
                Options = Options?.ToArray(),
                Format = Format,
                Required = Required,
                Category = Category
            };
        }
        
        /// <summary>
        /// 获取给定类型的默认值
        /// </summary>
        /// <param name="type">占位符类型</param>
        /// <returns>默认值</returns>
        private static object GetDefaultValueForType(PlaceholderType type)
        {
            switch (type)
            {
                case PlaceholderType.Text:
                    return string.Empty;
                case PlaceholderType.Number:
                    return 0;
                case PlaceholderType.DateTime:
                    return DateTime.Now;
                case PlaceholderType.Boolean:
                    return false;
                case PlaceholderType.Image:
                    return string.Empty;
                case PlaceholderType.List:
                    return Array.Empty<object>();
                default:
                    return null;
            }
        }
        
        /// <summary>
        /// 返回占位符名称
        /// </summary>
        public override string ToString()
        {
            return Name;
        }
        
        /// <summary>
        /// 用于占位符比较的哈希代码
        /// </summary>
        public override int GetHashCode()
        {
            return Name?.GetHashCode() ?? 0;
        }
        
        /// <summary>
        /// 占位符定义相等性比较
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj is PlaceholderDefinition other)
            {
                return Name == other.Name && Type == other.Type;
            }
            return false;
        }
    }
}