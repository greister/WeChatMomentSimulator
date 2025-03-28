using System.Threading.Tasks;

namespace WeChatMomentSimulator.Core.Interfaces.Services
{
    /// <summary>
    /// SVG渲染器接口
    /// </summary>
    public interface ISvgCustomRenderer
    {
        /// <summary>
        /// 将SVG内容渲染为图像数据
        /// </summary>
        /// <param name="svgContent">SVG内容</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <returns>图像字节数据</returns>
        Task<byte[]> RenderToImageAsync(string svgContent, int width, int height);
        
        /// <summary>
        /// 将SVG内容保存为图像文件
        /// </summary>
        /// <param name="svgContent">SVG内容</param>
        /// <param name="filePath">文件路径</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <returns>是否成功</returns>
        Task<bool> SaveToImageFileAsync(string svgContent, string filePath, int width, int height);
        
        /// <summary>
        /// 将SVG文件保存为图像文件
        /// </summary>
        /// <param name="svgFilePath">SVG文件路径</param>
        /// <param name="outputFilePath">输出文件路径</param>
        /// <param name="width">宽度</param>
        /// <param name="height">高度</param>
        /// <returns>是否成功</returns>
        Task<bool> ConvertSvgFileToImageAsync(string svgFilePath, string outputFilePath, int width, int height);
    }
}