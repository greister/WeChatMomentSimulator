using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WeChatMomentSimulator.Core.Interfaces;
using WeChatMomentSimulator.Core.Models.Template;

namespace WeChatMomentSimulator.Services.Storage
{
    /// <summary>
    /// 基于文件系统的模板存储实现
    /// </summary>
    public class FileTemplateStorage : ITemplateStorage
    {
        private readonly IPathService _pathService;
        private readonly ILogger<FileTemplateStorage> _logger;
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly string _baseDirectory;
        private readonly string _templatesDirectory;
        private readonly string _thumbnailsDirectory;

        /// <summary>
        /// 初始化文件模板存储
        /// </summary>
        /// <param name="pathService">路径服务</param>
        /// <param name="logger">日志记录器</param>
        public FileTemplateStorage(IPathService pathService, ILogger<FileTemplateStorage> logger = null)
        {
            _pathService = pathService ?? throw new ArgumentNullException(nameof(pathService));
            _logger = logger;
            
            // JSON序列化选项
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            // 确保目录结构存在
            EnsureDirectoriesExist();
        }

        /// <summary>
        /// 确保所需目录结构存在
        /// </summary>
        private void EnsureDirectoriesExist()
        {
            try
            {
                Directory.CreateDirectory(_pathService.GetTemplatesDirectory());
                Directory.CreateDirectory(_pathService.GetTemplateThumbnailsDirectory());
                _logger?.LogInformation("已创建模板目录结构");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "创建模板目录结构失败");
                throw;
            }
        }

        /// <summary>
        /// 获取所有模板
        /// </summary>
        /// <returns>模板集合</returns>
        public async Task<IEnumerable<TemplateDefinition>> GetAllTemplatesAsync()
        {
            var templates = new List<TemplateDefinition>();
            string templatesDirectory = _pathService.GetTemplatesDirectory();
            
            if (!Directory.Exists(templatesDirectory))
            {
                _logger?.LogWarning("模板目录不存在: {Directory}", templatesDirectory);
                return templates;
            }

            foreach (var file in Directory.GetFiles(templatesDirectory, "*.json"))
            {
                try
                {
                    var template = await LoadTemplateFromFileAsync(file);
                    if (template != null)
                    {
                        templates.Add(template);
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "加载模板文件失败: {File}", file);
                }
            }

            return templates;
        }

        /// <summary>
        /// 获取存储路径
        /// </summary>
        public string GetStoragePath()
        {
            return _baseDirectory;
        }

        /// <summary>
        /// 获取特定ID的模板
        /// </summary>
        /// <param name="id">模板ID</param>
        /// <returns>模板定义，如果不存在则返回null</returns>
        public async Task<TemplateDefinition> GetTemplateAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                _logger?.LogWarning("尝试获取模板时提供了空ID");
                return null;
            }

            string filePath = _pathService.GetTemplateFilePath(id);
            
            if (!File.Exists(filePath))
            {
                _logger?.LogWarning("模板文件不存在: {FilePath}", filePath);
                return null;
            }

            return await LoadTemplateFromFileAsync(filePath);
        }

        /// <summary>
        /// 保存模板
        /// </summary>
        /// <param name="template">要保存的模板</param>
        public async Task SaveTemplateAsync(TemplateDefinition template)
        {
            if (template == null)
                throw new ArgumentNullException(nameof(template));
            string templateId = template.Id.ToString();
            if (string.IsNullOrEmpty(templateId))
                throw new ArgumentException("Template ID cannot be null or empty", nameof(template));

            
            string filePath = _pathService.GetTemplateFilePath(templateId);
            
            await SaveTemplateToFileAsync(template, filePath);
            
            _logger?.LogInformation("模板已保存: {TemplateId} 到 {FilePath}", templateId, filePath);
        }

        /// <summary>
        /// 更新现有模板
        /// </summary>
        /// <param name="template">更新后的模板</param>
        /// <returns>是否成功更新</returns>
        public async Task<bool> UpdateTemplateAsync(TemplateDefinition template)
        {
            if (template == null)
                throw new ArgumentNullException(nameof(template));
                
            string templateId = template.Id.ToString();
            
            if (string.IsNullOrEmpty(templateId))
                throw new ArgumentException("Template ID cannot be null or empty", nameof(template));

            string filePath = _pathService.GetTemplateFilePath(templateId);
            
            if (!File.Exists(filePath))
            {
                _logger?.LogWarning("无法更新 - 模板文件不存在: {TemplateId}", templateId);
                return false;
            }

            try
            {
                // 更新修改时间
                template.ModifiedDate = DateTime.Now;
                
                // 保存到文件
                await SaveTemplateToFileAsync(template, filePath);
                
                _logger?.LogInformation("模板已更新: {TemplateId}", templateId);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "更新模板失败: {TemplateId}", templateId);
                return false;
            }
        }

        /// <summary>
        /// 删除模板
        /// </summary>
        /// <param name="id">要删除的模板ID</param>
        public Task DeleteTemplateAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("Template ID cannot be null or empty", nameof(id));

            string filePath = _pathService.GetTemplateFilePath(id);
            string thumbnailPath = _pathService.GetTemplateThumbnailPath(id);

            try
            {
                // 删除模板文件
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    _logger?.LogInformation("已删除模板文件: {TemplateId}", id);
                }
                else
                {
                    _logger?.LogWarning("模板文件不存在，无法删除: {TemplateId}", id);
                }

                // 删除缩略图文件
                if (File.Exists(thumbnailPath))
                {
                    File.Delete(thumbnailPath);
                    _logger?.LogInformation("已删除模板缩略图: {TemplateId}", id);
                }

                return Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "删除模板文件失败: {TemplateId}", id);
                throw;
            }
        }

        /// <summary>
        /// 导出模板到指定文件
        /// </summary>
        /// <param name="id">模板ID</param>
        /// <param name="filePath">导出文件路径</param>
        public async Task ExportTemplateAsync(string id, string filePath)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("Template ID cannot be null or empty", nameof(id));
                
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("Export file path cannot be null or empty", nameof(filePath));

            var template = await GetTemplateAsync(id);
            if (template == null)
                throw new FileNotFoundException($"Template with ID {id} not found");

            try
            {
                // 确保目标目录存在
                string directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                // 保存模板到文件
                await SaveTemplateToFileAsync(template, filePath);
                
                // 导出缩略图（如果存在）
                string thumbnailSource = _pathService.GetTemplateThumbnailPath(id);
                string thumbnailTarget = Path.Combine(
                    Path.GetDirectoryName(filePath),
                    Path.GetFileNameWithoutExtension(filePath) + ".png");
                    
                if (File.Exists(thumbnailSource))
                {
                    File.Copy(thumbnailSource, thumbnailTarget, true);
                    _logger?.LogInformation("已导出模板缩略图: {TemplateId} 到 {Target}", id, thumbnailTarget);
                }
                
                _logger?.LogInformation("已导出模板: {TemplateId} 到 {FilePath}", id, filePath);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "导出模板失败: {TemplateId} 到 {FilePath}", id, filePath);
                throw;
            }
        }

        /// <summary>
        /// 导入模板
        /// </summary>
        /// <param name="filePath">模板文件路径</param>
        /// <returns>导入的模板定义</returns>
        public async Task<TemplateDefinition> ImportTemplateAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new ArgumentException("Import file path cannot be null or empty", nameof(filePath));
                
            if (!File.Exists(filePath))
                throw new FileNotFoundException($"Template file not found: {filePath}");

            try
            {
                // 加载模板
                var template = await LoadTemplateFromFileAsync(filePath);
                
                if (template == null)
                    throw new InvalidDataException("导入的文件不是有效的模板");
                    
                if (template.Id == null || string.IsNullOrEmpty(template.Id.ToString()))
                {
                    // 如果导入的模板没有ID，生成一个新ID
                    template.Id = Guid.NewGuid();
                    _logger?.LogInformation("为导入的模板生成新ID: {TemplateId}", template.Id);
                }
                
                string templateId = template.Id.ToString();
                
                // 保存到标准位置
                string targetPath = _pathService.GetTemplateFilePath(templateId);
                await SaveTemplateToFileAsync(template, targetPath);
                
                // 导入缩略图（如果存在）
                string thumbnailSource = Path.Combine(
                    Path.GetDirectoryName(filePath),
                    Path.GetFileNameWithoutExtension(filePath) + ".png");
                    
                string thumbnailTarget = _pathService.GetTemplateThumbnailPath(templateId);
                
                if (File.Exists(thumbnailSource))
                {
                    // 确保缩略图目录存在
                    string thumbnailDir = Path.GetDirectoryName(thumbnailTarget);
                    if (!string.IsNullOrEmpty(thumbnailDir) && !Directory.Exists(thumbnailDir))
                    {
                        Directory.CreateDirectory(thumbnailDir);
                    }
                    
                    File.Copy(thumbnailSource, thumbnailTarget, true);
                    _logger?.LogInformation("已导入模板缩略图: {TemplateId}", templateId);
                }
                
                _logger?.LogInformation("已导入模板: {TemplateId}", templateId);
                return template;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "导入模板失败: {FilePath}", filePath);
                throw;
            }
        }

        /// <summary>
        /// 搜索模板
        /// </summary>
        /// <param name="keyword">搜索关键词</param>
        /// <returns>匹配的模板集合</returns>
        public async Task<IEnumerable<TemplateDefinition>> SearchTemplatesAsync(string keyword)
        {
            if (string.IsNullOrWhiteSpace(keyword))
            {
                return await GetAllTemplatesAsync();
            }

            var allTemplates = await GetAllTemplatesAsync();
            keyword = keyword.ToLower();
            
            return allTemplates.Where(t => 
                (t.Metadata?.Name?.ToLower()?.Contains(keyword) == true) ||
                (t.Metadata?.Description?.ToLower()?.Contains(keyword) == true) ||
                (t.Metadata?.Author?.ToLower()?.Contains(keyword) == true) ||
                (t.Metadata?.Category?.ToLower()?.Contains(keyword) == true) ||
                (t.Metadata?.Tags != null && t.Metadata.Tags.Any(tag => tag.ToLower().Contains(keyword)))
            );
        }

        /// <summary>
        /// 获取指定标签的模板
        /// </summary>
        /// <param name="tag">标签名</param>
        /// <returns>匹配的模板集合</returns>
        public async Task<IEnumerable<TemplateDefinition>> GetTemplatesByTagAsync(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
            {
                return new List<TemplateDefinition>();
            }

            var allTemplates = await GetAllTemplatesAsync();
            tag = tag.ToLower();
            
            return allTemplates.Where(t => 
                t.Metadata?.Tags != null && 
                t.Metadata.Tags.Any(t => t.ToLower() == tag)
            );
        }

        /// <summary>
        /// 获取指定分类的模板
        /// </summary>
        /// <param name="category">分类名</param>
        /// <returns>匹配的模板集合</returns>
        public async Task<IEnumerable<TemplateDefinition>> GetTemplatesByCategoryAsync(string category)
        {
            if (string.IsNullOrWhiteSpace(category))
            {
                return new List<TemplateDefinition>();
            }

            var allTemplates = await GetAllTemplatesAsync();
            category = category.ToLower();
            
            return allTemplates.Where(t => 
                t.Metadata?.Category?.ToLower() == category
            );
        }

        /// <summary>
        /// 获取模板文件中引用的所有资源文件路径
        /// </summary>
        /// <param name="id">模板ID</param>
        /// <returns>资源文件路径集合</returns>
        public async Task<IEnumerable<string>> GetTemplateResourcesAsync(string id)
        {
            var template = await GetTemplateAsync(id);
            if (template == null)
                return Enumerable.Empty<string>();

            var resources = new List<string>();
            
            // 添加缩略图
            string thumbnailPath = _pathService.GetTemplateThumbnailPath(id);
            if (File.Exists(thumbnailPath))
            {
                resources.Add(thumbnailPath);
            }
            
            // 分析SVG内容中的资源引用
            // 这里可以添加解析SVG中的图像引用、字体等资源的代码
            
            return resources;
        }

        public Task UpdateTemplateThumbnailAsync(Guid newTemplateId, object sourceThumbnailPath)
        {
            throw new NotImplementedException();
        }

        public Task<string?> GetTemplateThumbnailPathAsync(Guid id)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 从文件加载模板
        /// </summary>
        /// <param name="filePath">文件路径</param>
        /// <returns>模板定义</returns>
        private async Task<TemplateDefinition> LoadTemplateFromFileAsync(string filePath)
        {
            try
            {
                using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                var template = await JsonSerializer.DeserializeAsync<TemplateDefinition>(fileStream, _jsonOptions);
                
                // 兼容性处理：如果模板缺少某些必要字段，在这里补充
                if (template != null)
                {
                    if (template.Metadata == null)
                    {
                        template.Metadata = new TemplateMetadata
                        {
                            Name = Path.GetFileNameWithoutExtension(filePath),
                            Author = "未知作者",
                            Category = "未分类",
                            Description = "已导入模板"
                        };
                    }
                    
                    if (template.Placeholders == null)
                    {
                        template.Placeholders = new List<PlaceholderInfo>();
                    }
                    
                    // 确保ModifiedDate有值
                    if (template.ModifiedDate == default)
                    {
                        template.ModifiedDate = File.GetLastWriteTime(filePath);
                    }
                }
                
                return template;
            }
            catch (JsonException ex)
            {
                _logger?.LogError(ex, "解析模板JSON文件失败: {FilePath}", filePath);
                throw new InvalidDataException($"文件不是有效的模板JSON: {filePath}", ex);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "从文件加载模板失败: {FilePath}", filePath);
                throw;
            }
        }

        /// <summary>
        /// 保存模板到文件
        /// </summary>
        /// <param name="template">模板定义</param>
        /// <param name="filePath">文件路径</param>
        private async Task SaveTemplateToFileAsync(TemplateDefinition template, string filePath)
        {
            try
            {
                // 确保目录存在
                string directory = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                {
                    Directory.CreateDirectory(directory);
                }
                
                using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                await JsonSerializer.SerializeAsync(fileStream, template, _jsonOptions);
                
                // 刷新流确保完全写入
                await fileStream.FlushAsync();
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "保存模板到文件失败: {FilePath}", filePath);
                throw;
            }
        }

        /// <summary>
        /// 获取模板数量
        /// </summary>
        public int GetTemplateCount()
        {
            try
            {
                // 确保模板目录存在
                if (!Directory.Exists(_templatesDirectory))
                {
                    Directory.CreateDirectory(_templatesDirectory);
                    return 0;
                }

                // 获取所有JSON模板文件
                return Directory.GetFiles(_templatesDirectory, "*.json").Length;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "获取模板数量时出错");
                return 0;
            }
        }

    }
}