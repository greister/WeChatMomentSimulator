using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using LoggerExtensions = WeChatMomentSimulator.Core.Logging.LoggerExtensions;

namespace WeChatMomentSimulator.Services.Services
{
    /// <summary>
    /// 文件查找服务实现
    /// </summary>
    public class FileFinder : IFileFinder
    {
        private readonly ILogger<FileFinder> _logger;
        

        public FileFinder()
        {
            _logger = LoggerExtensions.GetLogger<FileFinder>();
        }

        /// <inheritdoc/>
        public async Task<string> FindFileAsync(string fileName, params string[] searchDirectories)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                _logger.LogWarning("搜索的文件名为空");
                return null;
            }

            _logger.LogInformation("开始搜索文件: {FileName}", fileName);

            foreach (var directory in searchDirectories)
            {
                if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
                {
                    _logger.LogWarning("跳过不存在的目录: {Directory}", directory);
                    continue;
                }

                _logger.LogDebug("在目录中查找: {Directory}", directory);
                string filePath = Path.Combine(directory, fileName);
                
                if (File.Exists(filePath))
                {
                    _logger.LogInformation("找到文件: {FilePath}", filePath);
                    return filePath;
                }
            }

            _logger.LogWarning("未能找到文件: {FileName}", fileName);
            return null;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<string>> FindFilesAsync(string searchPattern, params string[] searchDirectories)
        {
            var results = new List<string>();
            
            if (string.IsNullOrEmpty(searchPattern))
            {
                _logger.LogWarning("搜索模式为空");
                return results;
            }

            _logger.LogInformation("开始搜索文件模式: {Pattern}", searchPattern);

            foreach (var directory in searchDirectories)
            {
                if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
                {
                    _logger.LogWarning("跳过不存在的目录: {Directory}", directory);
                    continue;
                }

                try
                {
                    _logger.LogDebug("在目录中搜索: {Directory}", directory);
                    var files = Directory.GetFiles(directory, searchPattern);
                    
                    _logger.LogInformation("在 {Directory} 中找到 {Count} 个匹配文件", 
                        directory, files.Length);
                    
                    results.AddRange(files);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "搜索目录时出错: {Directory}", directory);
                }
            }

            return results;
        }
    }
}