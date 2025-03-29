using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WeChatMomentSimulator.Core.Models.Template;

namespace WeChatMomentSimulator.Core.Interfaces.Editors
{
    /// <summary>
    /// 模板编辑器基础接口
    /// </summary>
    public interface ITemplateEditor
    {
        /// <summary>
        /// 初始化编辑器
        /// </summary>
        void Initialize();

        /// <summary>
        /// 加载模板
        /// </summary>
        /// <param name="templateId">模板ID</param>
        Task<bool> LoadTemplateAsync(string templateId);

        /// <summary>
        /// 保存模板
        /// </summary>
        /// <param name="templateId">模板ID，为null时使用当前ID</param>
        /// <returns>保存后的模板ID</returns>
        Task<string> SaveTemplateAsync(string templateId = null);

        /// <summary>
        /// 获取当前SVG内容
        /// </summary>
        string GetSvgContent();

        /// <summary>
        /// 应用新的SVG内容
        /// </summary>
        /// <param name="svgContent">SVG字符串</param>
        void ApplySvgContent(string svgContent);

        /// <summary>
        /// 检测模板中的参数化元素
        /// </summary>
        /// <returns>参数化元素映射</returns>
        Task<IDictionary<PlaceholderType, IList<string>>> DetectParameterizedElementsAsync();

        /// <summary>
        /// 验证模板内容有效性
        /// </summary>
        Task<(bool IsValid, IList<string> Errors)> ValidateTemplateAsync();

        /// <summary>
        /// 当前模板是否已修改
        /// </summary>
        bool IsModified { get; }

        /// <summary>
        /// 当前模板ID
        /// </summary>
        string CurrentTemplateId { get; }

        /// <summary>
        /// 当前模板名称
        /// </summary>
        string CurrentTemplateName { get; set; }

        /// <summary>
        /// 当SVG内容变更时触发
        /// </summary>
        event EventHandler<TemplateContentEventArgs> ContentChanged;
    }

    /// <summary>
    /// 模板内容事件参数
    /// </summary>
    public class TemplateContentEventArgs : EventArgs
    {
        /// <summary>
        /// 变更类型
        /// </summary>
        public ContentChangeType ChangeType { get; set; }
    }

    /// <summary>
    /// 内容变更类型
    /// </summary>
    public enum ContentChangeType
    {
        Structure,
        Style,
        Attribute,
        Parameter
    }
}