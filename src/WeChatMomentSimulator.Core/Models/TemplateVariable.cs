using System;
using WeChatMomentSimulator.Core.Models.Template.Enums;

namespace WeChatMomentSimulator.Core.Models.Template
{
    /// <summary>
    /// 模板变量类
    /// </summary>
    public class TemplateVariable
    {
        /// <summary>
        /// 变量唯一标识符
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();
        
        /// <summary>
        /// 变量名称
        /// </summary>
        public string Name { get; set; }
        
        /// <summary>
        /// 变量显示名称
        /// </summary>
        public string DisplayName { get; set; }
        
        /// <summary>
        /// 变量类型
        /// </summary>
        public VariableType Type { get; set; }
        
        /// <summary>
        /// 变量描述
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// 默认值
        /// </summary>
        public string DefaultValue { get; set; }
        
        /// <summary>
        /// 获取或设置变量当前值
        /// </summary>
        public object Value { get; set; }  // 添加此属性
        
        /// <summary>
        /// 是否必填
        /// </summary>
        public bool IsRequired { get; set; }
        
        /// <summary>
        /// 是否是系统变量
        /// </summary>
        public bool IsSystem { get; set; }
        
        /// <summary>
        /// 可选值列表（针对枚举类型变量）
        /// </summary>
        public string[] Options { get; set; }
        
        /// <summary>
        /// 最小值（针对数值类型变量）
        /// </summary>
        public double? MinValue { get; set; }
        
        /// <summary>
        /// 最大值（针对数值类型变量）
        /// </summary>
        public double? MaxValue { get; set; }
        
        /// <summary>
        /// 正则表达式约束（针对文本类型变量）
        /// </summary>
        public string RegexPattern { get; set; }
    }
}