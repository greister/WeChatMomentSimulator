// 文件: Core/Interfaces/Rendering/ITemplateRenderer.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using WeChatMomentSimulator.Core.Models.Template;

namespace WeChatMomentSimulator.Core.Interfaces.Rendering
{
    /// <summary>
    /// SVG模板渲染器接口（平台无关）
    /// </summary>
    public interface ITemplateRenderer
    {
        /// <summary>
        /// 从SVG内容渲染到流
        /// </summary>
        /// <param name="svgContent">SVG内容字符串</param>
        /// <param name="parameters">渲染参数</param>
        /// <returns>包含渲染结果的流</returns>
        Task<Stream> RenderToStreamAsync(string svgContent, IDictionary<string, object> parameters = null);

        /// <summary>
        /// 处理SVG内容（应用参数替换等）
        /// </summary>
        /// <param name="svgContent">原始SVG内容</param>
        /// <param name="parameters">参数字典</param>
        /// <returns>处理后的SVG内容</returns>
        string ProcessSvgContent(string svgContent, IDictionary<string, object> parameters = null);

        /// <summary>
        /// 将SVG导出为图像
        /// </summary>
        /// <param name="svgContent">SVG内容</param>
        /// <param name="outputPath">输出路径</param>
        /// <param name="format">图像格式</param>
        /// <param name="width">输出宽度</param>
        /// <param name="height">输出高度</param>
        /// <returns>是否成功</returns>
        Task<bool> ExportToImageAsync(string svgContent, string outputPath, ImageFormat format, 
            double width = 0, double height = 0);

        /// <summary>
        /// 清除渲染缓存
        /// </summary>
        void ClearCache();

        /// <summary>
        /// 当渲染完成时触发
        /// </summary>
        event EventHandler<RenderCompletedEventArgs> RenderCompleted;

        /// <summary>
        /// 图像格式枚举
        /// </summary>
        public enum ImageFormat
        {
            Png,
            Jpeg,
            Bmp
        }
    }

    /// <summary>
    /// 渲染完成事件参数
    /// </summary>
    public class RenderCompletedEventArgs : EventArgs
    {
        /// <summary>
        /// 渲染是否成功
        /// </summary>
        public bool Success { get; set; }
        
        /// <summary>
        /// 渲染结果ID或路径
        /// </summary>
        public string ResultId { get; set; }
    }
}