using System;
using WeChatMomentSimulator.Core.Models.Template;

namespace WeChatMomentSimulator.Core.DataBinding
{
    /// <summary>
    /// 表示占位符与数据之间的绑定关系
    /// </summary>
    public class PlaceholderBinding
    {
        /// <summary>
        /// 占位符定义
        /// </summary>
        public PlaceholderDefinition Definition { get; }
        
        /// <summary>
        /// 当前绑定值
        /// </summary>
        public object Value { get; set; }
        
        /// <summary>
        /// 是否是必需的
        /// </summary>
        public bool IsRequired => Definition.Required;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="definition">占位符定义</param>
        /// <param name="initialValue">初始值</param>
        public PlaceholderBinding(PlaceholderDefinition definition, object initialValue)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            Value = initialValue ?? definition.DefaultValue;
        }
        
        /// <summary>
        /// 验证当前值是否有效
        /// </summary>
        /// <param name="errorMessage">错误信息</param>
        /// <returns>是否有效</returns>
        public bool Validate(out string errorMessage)
        {
            // 如果是必需的，则不能为null
            if (IsRequired && Value == null)
            {
                errorMessage = $"占位符 '{Definition.Name}' 是必需的，但值为空";
                return false;
            }
            
            // 根据类型进行特定验证
            switch (Definition.Type)
            {
                case PlaceholderType.Number:
                    if (Value != null && !(Value is int || Value is double || Value is float || Value is decimal))
                    {
                        errorMessage = $"占位符 '{Definition.Name}' 需要数字类型的值，但得到的是 {Value.GetType().Name}";
                        return false;
                    }
                    break;
                    
                case PlaceholderType.Boolean:
                    if (Value != null && !(Value is bool))
                    {
                        errorMessage = $"占位符 '{Definition.Name}' 需要布尔类型的值，但得到的是 {Value.GetType().Name}";
                        return false;
                    }
                    break;
                    
                case PlaceholderType.DateTime:
                    if (Value != null && !(Value is DateTime))
                    {
                        errorMessage = $"占位符 '{Definition.Name}' 需要日期时间类型的值，但得到的是 {Value.GetType().Name}";
                        return false;
                    }
                    break;
                    
                // 其他类型的验证逻辑...
            }
            
            errorMessage = null;
            return true;
        }
    }
}