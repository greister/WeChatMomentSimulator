using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.Logging;
using Svg;
using WeChatMomentSimulator.Core.Interfaces.Services;
using WeChatMomentSimulator.Core.Logging;
using LoggerExtensions = WeChatMomentSimulator.Core.Logging.LoggerExtensions;

namespace WeChatMomentSimulator.Services.Rendering
{
    public class SvgRenderer : ISvgCustomRenderer
    {
        private readonly ILogger<SvgRenderer> _logger;

        public SvgRenderer()
        {
            _logger = LoggerExtensions.GetLogger<SvgRenderer>();
        }

        /// <summary>
        /// 处理SVG模板，替换占位符
        /// </summary>
        public string ProcessTemplate(string template, Dictionary<string, object> placeholders)
        {
            if (string.IsNullOrEmpty(template))
            {
                _logger.LogWarning("模板内容为空，无法处理");
                return template;
            }
        
            _logger.LogInformation("开始处理SVG模板，共 {PlaceholderCount} 个占位符", placeholders?.Count ?? 0);
        
            if (placeholders == null || placeholders.Count == 0)
            {
                return template;
            }

            try
            {
                // 使用StringBuilder替代多次字符串替换，提高性能
                StringBuilder result = new StringBuilder(template);
        
                // 设置替换超时保护
                using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));
                var token = cancellationTokenSource.Token;

                foreach (var placeholder in placeholders)
                {
                    // 检查是否请求取消
                    token.ThrowIfCancellationRequested();
            
                    string pattern = "{{" + placeholder.Key + "}}";
                    string value = placeholder.Value?.ToString() ?? string.Empty;
            
                    // 直接使用StringBuilder替换，避免创建多个字符串对象
                    result.Replace(pattern, value);
                }

                _logger.LogInformation("模板处理完成，共替换 {ReplacedCount} 个占位符", placeholders.Count);
                return result.ToString();
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("处理模板占位符超时，返回原始模板");
                return template;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理模板占位符失败");
                return template;
            }
        }

        /// <summary>
        /// 将SVG渲染为图像
        /// </summary>
        public async Task<byte[]> RenderToImage(string svgContent, int width, int height, ImageFormat format)
        {
            if (string.IsNullOrEmpty(svgContent))
            {
                _logger.LogWarning("SVG内容为空，无法渲染为图像");
                return null;
            }

            _logger.LogInformation("开始渲染SVG为图像，尺寸: {Width}x{Height}, 格式: {Format}", width, height, format);
            
            return await Task.Run(() =>
            {
                try
                {
                    var svgDocument = SvgDocument.FromSvg<SvgDocument>(svgContent);
                    _logger.LogDebug("SVG文档解析成功");
                    svgDocument.Width = width;
                    svgDocument.Height = height;

                    using (var bitmap = svgDocument.Draw())
                    using (var memoryStream = new MemoryStream())
                    {
                        bitmap.Save(memoryStream, format);
                        _logger.LogInformation("SVG渲染完成，生成图像数据大小: {Size} bytes", memoryStream.Length);
                        return memoryStream.ToArray();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "渲染SVG到图像失败");
                    return null;
                }
            });
        }

        /// <summary>
        /// 将SVG渲染为WPF BitmapSource
        /// </summary>
        public async Task<BitmapSource> RenderToImageAsync(string svgContent, int width, int height)
        {
            if (string.IsNullOrEmpty(svgContent))
            {
                _logger.LogWarning("SVG内容为空，无法渲染");
                return null;
            }

            try
            {
                // 添加30秒超时保护
                using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));
                return await Task.Run(() =>
                {
                    try
                    {
                        var svgDocument = SvgDocument.FromSvg<SvgDocument>(svgContent);
                        svgDocument.Width = width;
                        svgDocument.Height = height;

                        using (var bitmap = svgDocument.Draw())
                        {
                            return ConvertToBitmapSource(bitmap);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "渲染SVG内容失败");
                        return null;
                    }
                }, cancellationTokenSource.Token);
            }
            catch (TaskCanceledException)
            {
                _logger.LogWarning("SVG渲染操作超时");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SVG渲染任务失败");
                return null;
            }
        }

        /// <summary>
        /// 保存SVG到图像文件
        /// </summary>
        public async Task<bool> SaveToImageFileAsync(string svgContent, string filePath, int width, int height)
        {
            if (string.IsNullOrEmpty(svgContent))
            {
                _logger.LogWarning("SVG内容为空，无法保存为图像文件");
                return false;
            }

            if (string.IsNullOrEmpty(filePath))
            {
                _logger.LogWarning("保存文件路径为空");
                return false;
            }

            try
            {
                string directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                ImageFormat format = GetImageFormat(filePath);
                byte[] imageData = await RenderToImage(svgContent, width, height, format);
                if (imageData == null || imageData.Length == 0)
                {
                    _logger.LogWarning("渲染图像失败，无有效图像数据");
                    return false;
                }

                await File.WriteAllBytesAsync(filePath, imageData);
                _logger.LogInformation($"已成功保存SVG图像到文件: {filePath}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"保存SVG到图像文件失败: {filePath}");
                return false;
            }
        }

        /// <summary>
        /// 转换SVG文件为图像
        /// </summary>
        public async Task<bool> ConvertSvgFileToImageAsync(string svgFilePath, string outputFilePath, int width, int height)
        {
            if (string.IsNullOrEmpty(svgFilePath))
            {
                _logger.LogWarning("SVG文件路径为空");
                return false;
            }

            if (string.IsNullOrEmpty(outputFilePath))
            {
                _logger.LogWarning("输出文件路径为空");
                return false;
            }

            try
            {
                if (!File.Exists(svgFilePath))
                {
                    _logger.LogWarning($"SVG文件不存在: {svgFilePath}");
                    return false;
                }

                string svgContent = await File.ReadAllTextAsync(svgFilePath);
                return await SaveToImageFileAsync(svgContent, outputFilePath, width, height);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"转换SVG文件到图像文件失败: {svgFilePath} -> {outputFilePath}");
                return false;
            }
        }

        // 将System.Drawing.Bitmap转换为WPF的BitmapSource
        private BitmapSource ConvertToBitmapSource(Bitmap bitmap)
        {
            _logger.LogDebug("开始转换Bitmap到BitmapSource");
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();
                bitmapImage.Freeze(); // 确保跨线程访问

                return bitmapImage;
            }
        }

        // 根据文件扩展名获取图像格式
        private ImageFormat GetImageFormat(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLowerInvariant();
            _logger.LogDebug("根据文件扩展名 {Extension} 确定图像格式", extension);
        
            return extension switch
            {
                ".png" => ImageFormat.Png,
                ".jpg" or ".jpeg" => ImageFormat.Jpeg,
                ".bmp" => ImageFormat.Bmp,
                ".gif" => ImageFormat.Gif,
                ".tiff" or ".tif" => ImageFormat.Tiff,
                _ => ImageFormat.Png // 默认使用PNG格式
            };
        }

        /// <summary>
        /// 接口实现：将SVG内容渲染为字节数组
        /// </summary>
        Task<byte[]> ISvgCustomRenderer.RenderToImageAsync(string svgContent, int width, int height)
        {
            if (string.IsNullOrEmpty(svgContent))
            {
                _logger.LogWarning("SVG内容为空，无法渲染");
                return Task.FromResult<byte[]>(null);
            }

            return Task.Run(() =>
            {
                try
                {
                    var svgDocument = SvgDocument.FromSvg<SvgDocument>(svgContent);
                    svgDocument.Width = width;
                    svgDocument.Height = height;

                    using (var bitmap = svgDocument.Draw())
                    using (var memoryStream = new MemoryStream())
                    {
                        bitmap.Save(memoryStream, ImageFormat.Png);
                        return memoryStream.ToArray();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "渲染SVG内容为字节数组失败");
                    return null;
                }
            });
        }
    }
}