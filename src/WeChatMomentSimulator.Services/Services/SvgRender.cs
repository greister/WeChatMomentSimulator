using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.Logging;
using SharpVectors.Converters;
using SharpVectors.Dom.Svg;
using SharpVectors.Renderers.Wpf;
using WeChatMomentSimulator.Core.Interfaces;
using WeChatMomentSimulator.Core.Logging;
using ISvgRenderer = WeChatMomentSimulator.Core.Interfaces.ISvgRenderer;
using LoggerExtensions = WeChatMomentSimulator.Core.Logging.LoggerExtensions;

namespace WeChatMomentSimulator.Services.Rendering
{
    /// <summary>
    /// SVG渲染器实现
    /// </summary>
    public class SvgRenderer : ISvgRenderer
    {
        private readonly ILogger<SvgRenderer> _logger;

        /// <summary>
        /// 构造函数
        /// </summary>
        public SvgRenderer()
        {
            _logger = LoggerExtensions.GetLogger<SvgRenderer>();
        }

        /// <summary>
        /// 将SVG内容渲染为图像数据
        /// </summary>
        public async Task<byte[]> RenderToImageAsync(string svgContent, int width, int height)
        {
            return await Task.Run(() =>
            {
                try
                {
                    // 创建临时文件
                    string tempFile = Path.GetTempFileName() + ".svg";
                    File.WriteAllText(tempFile, svgContent, Encoding.UTF8);

                    // 配置SVG转换器
                    var settings = new WpfDrawingSettings
                    {
                        IncludeRuntime = true,
                        TextAsGeometry = false
                    };

                    // 设置字体嵌入
                    //settings.FontEmbeddingManager.EmbedFonts = true;
                    var converter = new FileSvgConverter(settings);
                    if (!converter.Convert(tempFile))
                    {
                        _logger.LogError("SVG转换失败");
                        return CreateErrorImage(width, height);
                    }

                    DrawingGroup drawing = converter.Drawing;

                    // 删除临时文件
                    try { File.Delete(tempFile); } catch { }

                    // 使用模块化方案渲染
                    var bitmap = RenderDrawingToBitmap(drawing, width, height);
                    return EncodeToBytes(bitmap);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "SVG渲染失败");
                    return CreateErrorImage(width, height);
                }
            });
        }

        /// <summary>
        /// 将Drawing渲染为BitmapSource
        /// </summary>
        private BitmapSource RenderDrawingToBitmap(DrawingGroup drawing, int width, int height)
        {
            var drawingVisual = new DrawingVisual();
            using (var context = drawingVisual.RenderOpen())
            {
                context.DrawDrawing(drawing);
            }

            var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(drawingVisual);
            return bitmap;
        }

        /// <summary>
        /// 将BitmapSource编码为PNG字节数组
        /// </summary>
        private byte[] EncodeToBytes(BitmapSource bitmapSource)
        {
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

            using var stream = new MemoryStream();
            encoder.Save(stream);
            return stream.ToArray();
        }

        private byte[] CreateErrorImage(int width, int height)
        {
            var drawingVisual = new DrawingVisual();
            using (var dc = drawingVisual.RenderOpen())
            {
                dc.DrawRectangle(
                    new SolidColorBrush(Colors.LightGray),
                    null,
                    new Rect(0, 0, width, height));
            }

            var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(drawingVisual);

            // 使用封装的编码方法
            return EncodeToBytes(bitmap);
        }

        /// <summary>
        /// 将SVG内容保存为图像文件
        /// </summary>
        public async Task<bool> SaveToImageFileAsync(string svgContent, string filePath, int width, int height)
        {
            if (string.IsNullOrEmpty(svgContent) || string.IsNullOrEmpty(filePath))
            {
                _logger.LogWarning("保存SVG到图像时参数无效");
                return false;
            }

            try
            {
                byte[] imageData = await RenderToImageAsync(svgContent, width, height);
                if (imageData == null)
                {
                    _logger.LogError("SVG渲染失败，无法保存图像");
                    return false;
                }

                string directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                await File.WriteAllBytesAsync(filePath, imageData);
                _logger.LogInformation("已保存SVG图像到 {FilePath}", filePath);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "保存SVG到图像失败: {FilePath}", filePath);
                return false;
            }
        }

        /// <summary>
        /// 将SVG文件保存为图像文件
        /// </summary>
        public async Task<bool> ConvertSvgFileToImageAsync(string svgFilePath, string outputFilePath, int width, int height)
        {
            if (string.IsNullOrEmpty(svgFilePath) || string.IsNullOrEmpty(outputFilePath))
            {
                _logger.LogWarning("转换SVG文件到图像时参数��效");
                return false;
            }

            if (!File.Exists(svgFilePath))
            {
                _logger.LogError("SVG文件不存在: {SvgFilePath}", svgFilePath);
                return false;
            }

            try
            {
                // 配置SVG转换器
                var settings = new WpfDrawingSettings
                {
                    IncludeRuntime = true,
                    TextAsGeometry = false
                };

                var converter = new FileSvgConverter(settings);
                if (!converter.Convert(svgFilePath))
                {
                    _logger.LogError("SVG转换失败");
                    return false;
                }

                // 使用模块化方案渲染
                var bitmap = RenderDrawingToBitmap(converter.Drawing, width, height);
                
                string directory = Path.GetDirectoryName(outputFilePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // 保存到文件
                SaveBitmapSourceToFile(bitmap, outputFilePath);
                _logger.LogInformation("已保存SVG图像到 {FilePath}", outputFilePath);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "转换SVG文件到图像失败: {SvgFilePath} -> {OutputFilePath}", svgFilePath, outputFilePath);
                return false;
            }
        }

        /// <summary>
        /// 将BitmapSource保存到文件
        /// </summary>
        private void SaveBitmapSourceToFile(BitmapSource bitmapSource, string filePath)
        {
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmapSource));

            using var fileStream = new FileStream(filePath, FileMode.Create);
            encoder.Save(fileStream);
        }
    }
}