using System;
using System.Collections.Generic;
using WeChatMomentSimulator.Core.Models.Template.Enums;

namespace WeChatMomentSimulator.Core.Models.Template
{
    /// <summary>
    /// 表示一个模板
    /// </summary>
    public class Template
    {
        /// <summary>
        /// 获取或设置模板 ID
        /// </summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>
        /// 获取或设置模板名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 获取或设置模板显示名称
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// 获取或设置模板描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 获取或设置模板类型
        /// </summary>
        public TemplateType Type { get; set; } = TemplateType.Moment;

        /// <summary>
        /// 获取或设置模板路径
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// 获取或设置模板内容 (SVG)
        /// </summary>
        public string Content { get; set; }

        /// <summary>
        /// 获取或设置模板创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        
        /// <summary>
        /// 更新时间
        /// </summary>  
        public DateTime UpdatedAt { get; set; }
        /// <summary>
        /// 获取或设置模板修改时间
        /// </summary>
        public DateTime ModifiedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 获取或设置是否为默认模板
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// 获取或设置模板变量集合
        /// </summary>
        public List<TemplateVariable> Variables { get; set; } = new List<TemplateVariable>();

        /// <summary>
        /// 获取或设置模板标签
        /// </summary>
        public List<string> Tags { get; set; } = new List<string>();

        /// <summary>
        /// 获取或设置模板预览图路径
        /// </summary>
        public string PreviewImagePath { get; set; }
    }
}