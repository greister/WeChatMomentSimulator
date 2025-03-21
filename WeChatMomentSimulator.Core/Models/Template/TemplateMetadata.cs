using System.Collections.Generic;

namespace WeChatMomentSimulator.Core.Models.Template
{
    /// <summary>
    /// 表示模板的元数据信息
    /// </summary>
    public class TemplateMetadata
    {
        /// <summary>
        /// 模板名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 模板描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 模板作者
        /// </summary>
        public string Author { get; set; }

        /// <summary>
        /// 模板分类
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// 模板标签
        /// </summary>
        public List<string> Tags { get; set; } = new List<string>();

        /// <summary>
        /// 缩略图路径
        /// </summary>
        public string ThumbnailPath { get; set; }

        /// <summary>
        /// 微信版本兼容性
        /// </summary>
        public string WeChatVersion { get; set; }

        /// <summary>
        /// 默认宽度
        /// </summary>
        public int DefaultWidth { get; set; } = 1080;

        /// <summary>
        /// 默认高度
        /// </summary>
        public int DefaultHeight { get; set; } = 1920;

        /// <summary>
        /// 是否为官方模板
        /// </summary>
        public bool IsOfficial { get; set; }

        /// <summary>
        /// 自定义属性
        /// </summary>
        public Dictionary<string, string> CustomProperties { get; set; } = new Dictionary<string, string>();
    }
}