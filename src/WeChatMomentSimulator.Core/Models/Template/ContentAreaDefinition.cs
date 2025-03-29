namespace WeChatMomentSimulator.Core.Models.Template
{
    /// <summary>
    /// 内容区域定义，描述手机模板中内容应该放置的位置和大小
    /// </summary>
    public class ContentAreaDefinition
    {
        /// <summary>
        /// X坐标位置
        /// </summary>
        public double X { get; set; }

        /// <summary>
        /// Y坐标位置
        /// </summary>
        public double Y { get; set; }

        /// <summary>
        /// 区域宽度
        /// </summary>
        public double Width { get; set; }

        /// <summary>
        /// 区域高度
        /// </summary>
        public double Height { get; set; }

        /// <summary>
        /// 区域ID，用于在SVG中标识内容区域
        /// </summary>
        public string AreaId { get; set; } = "content-area";

        /// <summary>
        /// 创建新的内容区域定义
        /// </summary>
        public ContentAreaDefinition() { }

        /// <summary>
        /// 使用指定的位置和大小创建内容区域
        /// </summary>
        /// <param name="x">X坐标</param>
        /// <param name="y">Y坐标</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        public ContentAreaDefinition(double x, double y, double width, double height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        /// <summary>
        /// 检查区域定义是否有效
        /// </summary>
        /// <returns>如果宽高都大于0则有效</returns>
        public bool IsValid()
        {
            return Width > 0 && Height > 0;
        }
    }
}