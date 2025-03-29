using System;

namespace WeChatMomentSimulator.Core.Models.Template
{
    /// <summary>
    /// 模板基类，定义所有模板共有的属性
    /// </summary>
    public abstract class TemplateBase
    {
        /// <summary>
        /// 模板唯一标识符
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 模板名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 模板分类
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// SVG内容
        /// </summary>
        public string SvgContent { get; set; }

        /// <summary>
        /// 模板文件路径
        /// </summary>
        public string FilePath { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 最后修改时间
        /// </summary>
        public DateTime ModifiedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// 模板描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 模板作者
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// 模板版本
        /// </summary>
        public string Version { get; set; } = "1.0";
    }
}