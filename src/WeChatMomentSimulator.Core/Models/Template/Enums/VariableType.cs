namespace WeChatMomentSimulator.Core.Models.Template.Enums
{
    /// <summary>
    /// 模板变量类型
    /// </summary>
    public enum VariableType
    {
        /// <summary>
        /// 文本类型
        /// </summary>
        Text = 0,

        /// <summary>
        /// 数字类型
        /// </summary>
        Number = 1,

        /// <summary>
        /// 布尔类型
        /// </summary>
        Boolean = 2,

        /// <summary>
        /// 日期时间类型
        /// </summary>
        DateTime = 3,

        /// <summary>
        /// 颜色类型
        /// </summary>
        Color = 4,

        /// <summary>
        /// 图片路径
        /// </summary>
        ImagePath = 5,

        /// <summary>
        /// 富文本类型
        /// </summary>
        RichText = 6,

        /// <summary>
        /// JSON 对象类型
        /// </summary>
        JsonObject = 7,

        /// <summary>
        /// 枚举类型
        /// </summary>
        Enum = 8,

        /// <summary>
        /// 列表类型
        /// </summary>
        List = 9
    }
}