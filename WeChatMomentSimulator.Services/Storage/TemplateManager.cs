using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WeChatMomentSimulator.Core.Interfaces;
using WeChatMomentSimulator.Core.Models.Template;
using WeChatMomentSimulator.Services.Infrastructure;

namespace WeChatMomentSimulator.Services.Storage
{
    /// <summary>
    /// 模板管理服务，提供对模板的高级操作和缓存功能
    /// </summary>
    public class TemplateManager
    {
        private readonly ITemplateStorage _templateStorage;
        private readonly IPathService _pathService; // 添加PathService引用
        private readonly ILogger<TemplateManager> _logger;
        private Dictionary<Guid, TemplateDefinition> _templateCache;
        private bool _isCacheInitialized = false;

        /// <summary>
        /// 初始化模板管理器
        /// </summary>
        /// <param name="templateStorage">模板存储接口</param>
        /// <param name="logger">日志服务</param>
        public TemplateManager(ITemplateStorage templateStorage, IPathService pathService, ILogger<TemplateManager> logger = null)
        {
            _templateStorage = templateStorage ?? throw new ArgumentNullException(nameof(templateStorage));
            _pathService = pathService ?? throw new ArgumentNullException(nameof(pathService)); // 初始化
            _logger = logger;
            _templateCache = new Dictionary<Guid, TemplateDefinition>();
        }

        /// <summary>
        /// 初始化模板缓存
        /// </summary>
        /// <returns>异步任务</returns>
        public async Task InitializeCacheAsync()
        {
            try
            {
                _logger?.LogDebug("初始化模板缓存...");
                var templates = await _templateStorage.GetAllTemplatesAsync();
                
                _templateCache = templates.ToDictionary(t => t.Id);
                _isCacheInitialized = true;
                
                _logger?.LogInformation("模板缓存已初始化，加载了 {TemplateCount} 个模板", _templateCache.Count);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "初始化模板缓存时出错");
                _templateCache = new Dictionary<Guid, TemplateDefinition>();
                _isCacheInitialized = false;
            }
        }

        /// <summary>
        /// 获取所有模板
        /// </summary>
        /// <param name="forceRefresh">是否强制从存储刷新</param>
        /// <returns>模板集合</returns>
        public async Task<IEnumerable<TemplateDefinition>> GetAllTemplatesAsync(bool forceRefresh = false)
        {
            // 如果缓存未初始化或需要强制刷新
            if (!_isCacheInitialized || forceRefresh)
            {
                await InitializeCacheAsync();
            }

            return _templateCache.Values;
        }

        
        /// <summary>
        /// 保存模板
        /// </summary>
        /// <param name="template">要保存的模板</param>
        /// <returns>保存操作是否成功</returns>
        public async Task<bool> SaveTemplateAsync(TemplateDefinition template)
        {
            if (template == null)
            {
                _logger?.LogWarning("尝试保存空模板");
                return false;
            }

            try
            {
                await _templateStorage.SaveTemplateAsync(template);

                _templateCache[template.Id] = template;
                return true;


            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "保存模板时出错: {TemplateId}", template.Id);
                throw;
            }
        }

        /// <summary>
        /// 删除模板
        /// </summary>
        /// <param name="id">要删除的模板ID</param>
        /// <returns>删除操作是否成功</returns>
        /// <summary>
        /// 删除模板
        /// </summary>
        /// <param name="id">要删除的模板ID</param>
        /// <returns>删除操作是否成功</returns>
        public async Task<bool> DeleteTemplateAsync(Guid id)
        {
            try
            {
                // 修改位置 1: DeleteTemplateAsync 返回 void (Task)，不能赋值给变量
                // 修改位置 2: 参数类型不匹配 - 将 Guid 转换为 string
                string templateId = id.ToString();
        
                // 直接调用，不获取返回值
                await _templateStorage.DeleteTemplateAsync(templateId);
        
                // 修改位置 3: 从缓存中删除需要使用字符串键
                if (_templateCache.ContainsKey(id))
                {
                    // 从缓存中移除
                    _templateCache.Remove(id);
                    _logger?.LogDebug("模板已从缓存中删除: {TemplateId}", id);
                }
        
                return true; // 如果没有抛出异常，则认为操作成功
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "删除模板时出错: {TemplateId}", id);
                return false;
            }
        }

        /// <summary>
        /// 创建新模板
        /// </summary>
        /// <param name="name">模板名称</param>
        /// <param name="author">模板作者</param>
        /// <param name="category">模板分类</param>
        /// <returns>新创建的模板</returns>
        public async Task<TemplateDefinition> CreateTemplateAsync(string name, string author = null, string category = null)
        {
            try
            {
                var template = new TemplateDefinition
                {
                    Id = Guid.NewGuid(),
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now,
                    Version = "1.0",
                    Metadata = new TemplateMetadata
                    {
                        Name = name,
                        Author = author ?? "未知作者",
                        Category = category ?? "未分类",
                        Description = "新建模板"
                    },
                    SvgContent = "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"1080\" height=\"1920\" viewBox=\"0 0 1080 1920\"></svg>",
                    Placeholders = new List<PlaceholderInfo>()
                };

                await SaveTemplateAsync(template);
                _logger?.LogInformation("创建了新模板: {TemplateName} ({TemplateId})", name, template.Id);
                
                return template;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "创建模板时出错: {TemplateName}", name);
                return null;
            }
        }

        /// <summary>
        /// 导入模板
        /// </summary>
        /// <param name="filePath">模板文件路径</param>
        /// <returns>导入的模板</returns>
        public async Task<TemplateDefinition> ImportTemplateAsync(string filePath)
        {
            try
            {
                var template = await _templateStorage.ImportTemplateAsync(filePath);
                
                if (template != null)
                {
                    // 更新缓存
                    _templateCache[template.Id] = template;
                    _logger?.LogInformation("已导入模板并添加到缓存: {TemplateId}", template.Id);
                }
                
                return template;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "导入模板时出错: {FilePath}", filePath);
                return null;
            }
        }

        /// <summary>
        /// 导出模板
        /// </summary>
        /// <param name="id">模板ID</param>
        /// <param name="destinationPath">导出目标路径</param>
        /// <returns>导出操作是否成功</returns>
        public async Task<bool> ExportTemplateAsync(Guid id, string destinationPath)
        {
            // 修改位置：原代码直接返回void方法的Task，无法隐式转换为Task<bool>
            // return _templateStorage.ExportTemplateAsync(id.ToString(), destinationPath);

            try
            {
                // 调用存储导出方法并等待完成
                await _templateStorage.ExportTemplateAsync(id.ToString(), destinationPath);

                _logger?.LogInformation("已导出模板: {TemplateId} 到 {DestinationPath}", id, destinationPath);
                return true; // 返回成功状态
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "导出模板失败: {TemplateId} 到 {DestinationPath}", id, destinationPath);
                return false; // 返回失败状态
            }
        }

        /// <summary>
        /// 复制模板
        /// </summary>
        /// <param name="id">要复制的模板ID</param>
        /// <param name="newName">新模板名称</param>
        /// <returns>复制后的模板</returns>
        public async Task<TemplateDefinition> DuplicateTemplateAsync(Guid id, string newName = null)
        {
            try
            {
                var sourceTemplate = await GetTemplateByIdAsync(id);
                
                if (sourceTemplate == null)
                {
                    _logger?.LogWarning("尝试复制不存在的模板: {TemplateId}", id);
                    return null;
                }

                // 创建新模板对象
                var newTemplate = new TemplateDefinition
                {
                    Id = Guid.NewGuid(),
                    CreatedDate = DateTime.Now,
                    ModifiedDate = DateTime.Now,
                    Version = sourceTemplate.Version,
                    SvgContent = sourceTemplate.SvgContent,
                    Placeholders = new List<PlaceholderInfo>(sourceTemplate.Placeholders)
                };

                // 复制元数据
                if (sourceTemplate.Metadata != null)
                {
                    newTemplate.Metadata = new TemplateMetadata
                    {
                        Name = newName ?? $"{sourceTemplate.Metadata.Name} 副本",
                        Description = sourceTemplate.Metadata.Description,
                        Author = sourceTemplate.Metadata.Author,
                        Category = sourceTemplate.Metadata.Category,
                        Tags = new List<string>(sourceTemplate.Metadata.Tags),
                        WeChatVersion = sourceTemplate.Metadata.WeChatVersion,
                        DefaultWidth = sourceTemplate.Metadata.DefaultWidth,
                        DefaultHeight = sourceTemplate.Metadata.DefaultHeight,
                        IsOfficial = false, // 复制品不是官方模板
                        CustomProperties = new Dictionary<string, string>(sourceTemplate.Metadata.CustomProperties)
                    };
                }

                // 复制缩略图
                var sourceThumbnailPath = await _templateStorage.GetTemplateThumbnailPathAsync(id);
                if (sourceThumbnailPath != null && File.Exists(sourceThumbnailPath))
                {
                    // 先保存新模板以获得ID
                    await SaveTemplateAsync(newTemplate);
                    
                    // 复制并更新缩略图
                    await _templateStorage.UpdateTemplateThumbnailAsync(newTemplate.Id, sourceThumbnailPath);
                    
                    // 重新加载模板以获取更新后的缩略图路径
                    newTemplate = await GetTemplateByIdAsync(newTemplate.Id);
                }
                else
                {
                    // 保存新模板
                    await SaveTemplateAsync(newTemplate);
                }

                _logger?.LogInformation("已复制模板: {SourceTemplateId} -> {NewTemplateId}", id, newTemplate.Id);
                return newTemplate;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "复制模板时出错: {TemplateId}", id);
                return null;
            }
        }

        /// <summary>
        /// 按分类获取模板
        /// </summary>
        /// <param name="category">类别名称</param>
        /// <returns>该分类下的模板集合</returns>
        public async Task<IEnumerable<TemplateDefinition>> GetTemplatesByCategoryAsync(string category)
        {
            if (!_isCacheInitialized)
            {
                await InitializeCacheAsync();
            }

            return _templateCache.Values
                .Where(t => t.Metadata?.Category?.Equals(category, StringComparison.OrdinalIgnoreCase) ?? false);
        }

        /// <summary>
        /// 获取所有可用的模板分类
        /// </summary>
        /// <returns>分类列表</returns>
        public async Task<IEnumerable<string>> GetAllCategoriesAsync()
        {
            if (!_isCacheInitialized)
            {
                await InitializeCacheAsync();
            }

            return _templateCache.Values
                .Select(t => t.Metadata?.Category)
                .Where(c => !string.IsNullOrEmpty(c))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(c => c);
        }

        /// <summary>
        /// 搜索模板
        /// </summary>
        /// <param name="searchTerm">搜索关键词</param>
        /// <returns>符合搜索条件的模板</returns>
        public Task<IEnumerable<TemplateDefinition>> SearchTemplatesAsync(string searchTerm)
        {
            return _templateStorage.SearchTemplatesAsync(searchTerm);
        }

        /// <summary>
        /// 刷新模板缓存
        /// </summary>
        /// <returns>异步任务</returns>
        public Task RefreshCacheAsync()
        {
            return InitializeCacheAsync();
        }

        /// <summary>
        /// 清除模板缓存
        /// </summary>
        public void ClearCache()
        {
            _templateCache.Clear();
            _isCacheInitialized = false;
            _logger?.LogDebug("模板缓存已清除");
        }
        
        /// <summary>
        /// 获取模板数量
        /// </summary>
        /// <returns>模板数量</returns>
        public async Task<int> GetTemplateCountAsync()
        {
            if (!_isCacheInitialized)
            {
                await InitializeCacheAsync();
            }

            return _templateCache.Count;
        }
        
        /// <summary>
        /// 更新现有模板
        /// </summary>
        /// <param name="template">要更新的模板</param>
        /// <returns>操作是否成功</returns>
        public async Task<bool> UpdateTemplateAsync(TemplateDefinition template)
        {
            if (template == null || string.IsNullOrEmpty(template.Id.ToString()))
            {
                return false;
            }

            try
            {
                // 检查模板是否存在
                if (!_templateCache.ContainsKey(template.Id))
                {
                    await InitializeCacheAsync(); // 尝试从存储中刷新
                    if (!_templateCache.ContainsKey(template.Id))
                    {
                        return false;
                    }
                }

                // 更新缓存
                _templateCache[template.Id] = template;
        
                // 保存到存储
                await _templateStorage.SaveTemplateAsync(template);
        
                return true;
            }
            catch (Exception ex)
            {
                // 记录错误
                _logger?.LogError(ex, "更新模板失败: {TemplateId}", template.Id);
                return false;
            }
        }


        /// <summary>
        /// 获取模板缩略图路径
        /// </summary>
        /// <param name="id">模板ID</param>
        /// <returns>缩略图路径</returns>
        public async Task<string> GetTemplateThumbnailPathAsync(string id)
        {
            if (string.IsNullOrEmpty(id))
                return null;

            try
            {
                // 修改位置：原代码调用了不存在的ITemplateStorage.GetTemplateThumbnailPathAsync方法
                // 替换为使用IPathService获取缩略图路径

                // 首先检查模板是否存在
                var template = await _templateStorage.GetTemplateAsync(id);
                if (template == null)
                {
                    _logger?.LogWarning("找不到模板，无法获取缩略图: {TemplateId}", id);
                    return null;
                }

                // 使用PathService获取缩略图路径
                string thumbnailPath = _pathService.GetTemplateThumbnailPath(id);

                // 检查缩略图文件是否实际存在
                if (!File.Exists(thumbnailPath))
                {
                    _logger?.LogWarning("模板缩略图不存在: {ThumbnailPath}", thumbnailPath);
                    return null;
                }

                return thumbnailPath;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "获取模板缩略图路径失败: {TemplateId}", id);
                return null;
            }
        }


        /// <summary>
        /// 获取指定ID的模板
        /// </summary>
        /// <param name="id">模板ID</param>
        /// <returns>模板定义</returns>
        public async Task<TemplateDefinition> GetTemplateByIdAsync(Guid id)
        {
            if (string.IsNullOrEmpty(id.ToString()))
                return null;

            try
            {
                // 先检查缓存
                if (_templateCache.TryGetValue(id, out var template))
                {
                    return template;
                }

                // 缓存未初始化，初始化缓存
                if (!_isCacheInitialized)
                {
                    await InitializeCacheAsync();
                    if (_templateCache.TryGetValue(id, out template))
                    {
                        return template;
                    }
                }

                // 修改位置：原代码调用了不存在的 GetTemplateByIdAsync 方法
                // 修改为调用正确的 GetTemplateAsync 方法
                template = await _templateStorage.GetTemplateAsync(id.ToString());
                
                // 如果找到模板，添加到缓存
                if (template != null)
                {
                    _templateCache[id] = template;
                }

                return template;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "获取模板失败: {TemplateId}", id);
                return null;
            }
        }

        /// <summary>
        /// 更新模板缩略图
        /// </summary>
        /// <param name="id">模板ID</param>
        /// <param name="thumbnailPath">缩略图源文件路径</param>
        /// <returns>操作是否成功</returns>
        public async Task<bool> UpdateTemplateThumbnailAsync(string id, string thumbnailPath)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(thumbnailPath))
                return false;

            try
            {
                // 修改位置：原代码调用了不存在的ITemplateStorage.UpdateTemplateThumbnailAsync方法
                // 使用文件操作替代，自己实现缩略图更新功能

                // 首先检查模板是否存在
                var template = await _templateStorage.GetTemplateAsync(id);
                if (template == null)
                {
                    _logger?.LogWarning("找不到模板，无法更新缩略图: {TemplateId}", id);
                    return false;
                }

                // 确保源缩略图文件存在
                if (!File.Exists(thumbnailPath))
                {
                    _logger?.LogWarning("源缩略图文件不存在: {ThumbnailPath}", thumbnailPath);
                    return false;
                }

                // 获取目标缩略图路径
                string destThumbnailPath = _pathService.GetTemplateThumbnailPath(id);

                // 确保目标目录存在
                string destDirectory = Path.GetDirectoryName(destThumbnailPath);
                if (!Directory.Exists(destDirectory))
                {
                    Directory.CreateDirectory(destDirectory);
                }

                // 复制缩略图文件
                File.Copy(thumbnailPath, destThumbnailPath, true);

                _logger?.LogInformation("已更新模板缩略图: {TemplateId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "更新模板缩略图失败: {TemplateId}", id);
                return false;
            }
        }

    }

    
}