using System;

namespace WeChatMomentSimulator.Core.Models.Template
{
    /// <summary>
    /// 定义所有支持的占位符类型
    /// </summary>
    public enum PlaceholderType
    {
        /// <summary>
        /// 文本类型占位符
        /// </summary>
        Text,

        /// <summary>
        /// 数值类型占位符
        /// </summary>
        Number,

        /// <summary>
        /// 日期时间类型占位符
        /// </summary>
        DateTime,

        /// <summary>
        /// 布尔类型占位符
        /// </summary>
        Boolean,

        /// <summary>
        /// 图片类型占位符
        /// </summary>
        Image,

        /// <summary>
        /// 列表类型占位符（用于循环渲染）
        /// </summary>
        List
    }
}