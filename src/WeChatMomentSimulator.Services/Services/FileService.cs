using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WeChatMomentSimulator.Core.Interfaces.Services;
using WeChatMomentSimulator.Core.Logging;
using LoggerExtensions = WeChatMomentSimulator.Core.Logging.LoggerExtensions;

namespace WeChatMomentSimulator.Services.Services
{
    /// <summary>
    /// 提供文件操作相关服务
    /// </summary>
    public class FileService : IFileService
    {
        private readonly ILogger<FileService>  _logger; // 假设 ILog 是日志接口

        public FileService()
        {
            _logger = LoggerExtensions.GetLogger<FileService>();
        }

        /// <summary>
        /// 异步读取文本文件内容
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>文件内容</returns>
        public async Task<string> ReadTextAsync(string filePath)
        {
            if (!FileExists(filePath))
            {
                _logger.LogWarning($"找不到文件: {filePath}");
                throw new FileNotFoundException($"找不到文件: {filePath}");
            }

            return await File.ReadAllTextAsync(filePath);
        }

        /// <summary>
        /// 异步写入文本内容到文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <param name="content">要写入的内容</param>
        /// <returns>异步任务</returns>
        public async Task WriteTextAsync(string filePath, string content)
        {
            string directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory))
            {
                EnsureDirectoryExists(directory);
            }

            await File.WriteAllTextAsync(filePath, content);
        }

        /// <summary>
        /// 确保目录存在，如不存在则创建
        /// </summary>
        /// <param name="directory">目录路径</param>
        public void EnsureDirectoryExists(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        /// <summary>
        /// 检查文件是否存在
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>是否存在</returns>
        public bool FileExists(string filePath)
        {
            return File.Exists(filePath);
        }

        /// <summary>
        /// 拷贝文件
        /// </summary>
        /// <param name="sourcePath">源文件路径</param>
        /// <param name="targetPath">目标文件路径</param>
        /// <param name="overwrite">是否覆盖已存在文件</param>
        /// <returns>异步任务</returns>
        public async Task CopyFileAsync(string sourcePath, string targetPath, bool overwrite = true)
        {
            if (!FileExists(sourcePath))
            {
                _logger.LogWarning($"找不到文件: {sourcePath}");
                throw new FileNotFoundException($"找不到文件: {sourcePath}");
            }

            string directory = Path.GetDirectoryName(targetPath);
            if (!string.IsNullOrEmpty(directory))
            {
                EnsureDirectoryExists(directory);
            }

            // 由于 File.CopyAsync 不存在，我们使用任务包装同步方法
            await Task.Run(() => File.Copy(sourcePath, targetPath, overwrite));
        }

        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>是否删除成功</returns>
        public async Task<bool> DeleteFileAsync(string filePath)
        {
            if (!FileExists(filePath))
            {
                return false;
            }

            // 由于 File.DeleteAsync 不存在，我们使用任务包装同步方法
            await Task.Run(() =>
            {
                File.Delete(filePath);
            });

            return !FileExists(filePath);
        }

        /// <summary>
        /// 获取目录中的所有文件
        /// </summary>
        /// <param name="directory">目录路径</param>
        /// <param name="searchPattern">搜索模式</param>
        /// <param name="recursive">是否递归搜索</param>
        /// <returns>文件路径列表</returns>
        public async Task<string[]> GetFilesAsync(string directory, string searchPattern = "*.*", bool recursive = false)
        {
            if (!Directory.Exists(directory))
            {
                throw new DirectoryNotFoundException($"找不到目录: {directory}");
            }

            return await Task.Run(() =>
            {
                return Directory.GetFiles(
                    directory,
                    searchPattern,
                    recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly);
            });
        }
    }
}