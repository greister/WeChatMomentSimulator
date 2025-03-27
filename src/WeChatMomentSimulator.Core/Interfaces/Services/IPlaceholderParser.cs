using System;
using System.Collections.Generic;
using WeChatMomentSimulator.Core.Models.Template;

namespace WeChatMomentSimulator.Core.Interfaces
{
    /// <summary>
    /// 占位符解析器接口
    /// </summary>
    public interface IPlaceholderParser
    {
        /// <summary>
        /// 从模板文本中提取所有占位符
        /// </summary>
        /// <param name="templateText">模板文本</param>
        /// <returns>占位符集合</returns>
        IEnumerable<string> ExtractPlaceholders(string templateText);
        
        /// <summary>
        /// 替换模板中的占位符
        /// </summary>
        /// <param name="templateText">模板文本</param>
        /// <param name="values">值字典</param>
        /// <returns>替换后的文本</returns>
        string ReplacePlaceholders(string templateText, IDictionary<string, object> values);
        
        /// <summary>
        /// 检查并获取模板中使用的所有占位符定义
        /// </summary>
        /// <param name="templateText">模板文本</param>
        /// <param name="availablePlaceholders">可用的占位符定义</param>
        /// <returns>模板中使用的占位符定义</returns>
        IEnumerable<PlaceholderDefinition> GetUsedPlaceholderDefinitions(
            string templateText, 
            IEnumerable<PlaceholderDefinition> availablePlaceholders);
    }
}