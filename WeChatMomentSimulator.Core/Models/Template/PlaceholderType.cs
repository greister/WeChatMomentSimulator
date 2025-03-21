using System;

namespace WeChatMomentSimulator.Core.Models.Template
{
    /// <summary>
    /// 占位符类型枚举
    /// </summary>
    public enum PlaceholderType
    {
        /// <summary>
        /// 文本占位符
        /// </summary>
        Text,

        /// <summary>
        /// 图像占位符
        /// </summary>
        Image,

        /// <summary>
        /// 时间占位符
        /// </summary>
        DateTime,

        /// <summary>
        /// 用户名称占位符
        /// </summary>
        UserName,

        /// <summary>
        /// 头像占位符
        /// </summary>
        Avatar,

        /// <summary>
        /// 状态栏占位符
        /// </summary>
        StatusBar,

        /// <summary>
        /// 互动数据占位符（如点赞、评论）
        /// </summary>
        Interaction,

        /// <summary>
        /// 自定义占位符
        /// </summary>
        Custom
    }

    /// <summary>
    /// 表示模板中的占位符信息
    /// </summary>
    public class PlaceholderInfo
    {
        /// <summary>
        /// 占位符唯一标识
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 占位符类型
        /// </summary>
        public PlaceholderType Type { get; set; }

        /// <summary>
        /// 占位符名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 占位符在SVG中的元素ID
        /// </summary>
        public string ElementId { get; set; }

        /// <summary>
        /// 占位符描述
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// 默认值
        /// </summary>
        public string DefaultValue { get; set; }

        /// <summary>
        /// 是否必须
        /// </summary>
        public bool IsRequired { get; set; }

        /// <summary>
        /// 占位符的最大长度（适用于文本）
        /// </summary>
        public int? MaxLength { get; set; }

        /// <summary>
        /// 值验证正则表达式
        /// </summary>
        public string ValidationRegex { get; set; }

        /// <summary>
        /// 占位符X坐标
        /// </summary>
        public float? X { get; set; }

        /// <summary>
        /// 占位符Y坐标
        /// </summary>
        public float? Y { get; set; }

        /// <summary>
        /// 占位符宽度
        /// </summary>
        public float? Width { get; set; }

        /// <summary>
        /// 占位符高度
        /// </summary>
        public float? Height { get; set; }

        /// <summary>
        /// 自定义属性
        /// </summary>
        public System.Collections.Generic.Dictionary<string, string> CustomProperties { get; set; } 
            = new System.Collections.Generic.Dictionary<string, string>();
    }
}