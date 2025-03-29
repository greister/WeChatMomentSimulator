using System.Collections.Generic;

namespace WeChatMomentSimulator.Core.Models.Template
{
    /// <summary>
    /// 内容模板，包含朋友圈、爱看等内容区域的模板
    /// </summary>
    public class ContentTemplate : TemplateBase
    {
        /// <summary>
        /// 内容类型(朋友圈/爱看等)
        /// </summary>
        public string ContentType { get; set; } = "WeChatMoment";

        /// <summary>
        /// 兼容的设备类型列表
        /// </summary>
        public List<string> CompatibleDevices { get; set; } = new List<string> { "iPhone", "Android" };

        /// <summary>
        /// 占位符列表
        /// </summary>
        public List<PlaceholderDefinition> Placeholders { get; set; } = new List<PlaceholderDefinition>();

        /// <summary>
        /// 原始设计宽度，用于缩放计算
        /// </summary>
        public double NativeWidth { get; set; } = 390;

        /// <summary>
        /// 原始设计高度，用于缩放计算
        /// </summary>
        public double NativeHeight { get; set; } = 600;

        /// <summary>
        /// 是否允许自动缩放以适应内容区域
        /// </summary>
        public bool AllowAutoScaling { get; set; } = true;

        /// <summary>
        /// 添加占位符
        /// </summary>
        /// <param name="placeholder">占位符定义</param>
        public void AddPlaceholder(PlaceholderDefinition placeholder)
        {
            // 确保ID不重复
            if (Placeholders.Any(p => p.Id == placeholder.Id))
            {
                throw new InvalidOperationException($"占位符ID '{placeholder.Id}' 已存在");
            }

            Placeholders.Add(placeholder);
        }

        /// <summary>
        /// 获取特定类型的所有占位符
        /// </summary>
        /// <param name="type">占位符类型</param>
        /// <returns>符合条件的占位符列表</returns>
        public List<PlaceholderDefinition> GetPlaceholdersOfType(PlaceholderType type)
        {
            return Placeholders.Where(p => p.Type == type).ToList();
        }
    }
}