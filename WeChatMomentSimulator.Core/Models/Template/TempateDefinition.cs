using System;
using System.Collections.Generic;

namespace WeChatMomentSimulator.Core.Models.Template
{
    /// <summary>
    /// 表示微信朋友圈模板定义
    /// </summary>
    public class TemplateDefinition
    {
        /// <summary>
        /// 模板唯一标识符
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// 模板元数据
        /// </summary>
        public TemplateMetadata Metadata { get; set; }

        /// <summary>
        /// 模板SVG内容
        /// </summary>
        public string SvgContent { get; set; }

        /// <summary>
        /// 模板中的占位符列表
        /// </summary>
        public List<PlaceholderInfo> Placeholders { get; set; } = new List<PlaceholderInfo>();

        /// <summary>
        /// 模板创建日期
        /// </summary>
        public DateTime CreatedDate { get; set; }

        /// <summary>
        /// 模板修改日期
        /// </summary>
        public DateTime ModifiedDate { get; set; }

        /// <summary>
        /// 版本号
        /// </summary>
        public string Version { get; set; }

        /// <summary>
        /// 检验模板是否有效
        /// </summary>
        public bool IsValid()
        {
            // 基本验证：确保必要字段不为空
            if (string.IsNullOrEmpty(SvgContent))
                return false;

            if (Metadata == null || string.IsNullOrEmpty(Metadata.Name))
                return false;

            // 进一步验证可以添加在这里

            return true;
        }
    }
}