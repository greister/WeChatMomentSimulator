using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WeChatMomentSimulator.Core.Interfaces.Repositories;
using WeChatMomentSimulator.Core.Interfaces.Services;
using WeChatMomentSimulator.Core.Models.Template;
using WeChatMomentSimulator.Core.Models.Template.Enums;
using LoggerExtensions = WeChatMomentSimulator.Core.Logging.LoggerExtensions;

namespace WeChatMomentSimulator.Services.Repositories
{
    /// <summary>
    /// 模板存储库实现
    /// </summary>
    public class TemplateRepository : ITemplateRepository
    {
        private readonly IFileService _fileService;
        private readonly ILogger<TemplateRepository> _logger;
        private readonly string _templateDirectory;
        private readonly string _indexFilePath;
        private readonly JsonSerializerOptions _jsonOptions;
        private Dictionary<Guid, Template> _templates;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="fileService">文件服务</param>
        /// <param name="logger">日志服务</param>
        /// <param name="templateDirectory">模板目录</param>
        public TemplateRepository(
            IFileService fileService,
            string templateDirectory = null)
        {
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _logger = LoggerExtensions.GetLogger<TemplateRepository>();
            
            // 设置模板目录
            _templateDirectory = templateDirectory ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates");
            _indexFilePath = Path.Combine(_templateDirectory, "template_index.json");
            
            // 配置JSON序列化选项
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            _templates = new Dictionary<Guid, Template>();
            
            _logger.LogDebug("模板存储库已初始化，模板目录: {TemplateDirectory}", _templateDirectory);
            
            // 确保目录存在
            _fileService.EnsureDirectoryExists(_templateDirectory);
            
            // 异步初始化模板数据
            _ = InitializeAsync();
        }
        
        /// <summary>
        /// 初始化模板数据
        /// </summary>
        private async Task InitializeAsync()
        {
            _logger.LogDebug("开始初始化模板数据");
            
            try
            {
                if (_fileService.FileExists(_indexFilePath))
                {
                    _logger.LogDebug("开始加载模板索引: {IndexFilePath}", _indexFilePath);
                    string json = await _fileService.ReadTextAsync(_indexFilePath);
                    var templates = JsonSerializer.Deserialize<List<Template>>(json, _jsonOptions);
                    
                    if (templates != null)
                    {
                        _templates = templates.ToDictionary(t => t.Id);
                        _logger.LogInformation("已加载 {Count} 个模板", _templates.Count);
                    }
                }
                else
                {
                    _logger.LogDebug("模板索引文件不存在，将创建默认模板");
                    await CreateDefaultTemplatesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "初始化模板数据时发生错误");
                
                // 如果加载失败，我们尝试创建默认模板
                _templates = new Dictionary<Guid, Template>();
                try
                {
                    await CreateDefaultTemplatesAsync();
                }
                catch (Exception innerEx)
                {
                    _logger.LogError(innerEx, "创建默认模板失败");
                }
            }
        }
        
        /// <summary>
        /// 创建默认模板
        /// </summary>
        private async Task CreateDefaultTemplatesAsync()
        {
            _logger.LogInformation("创建默认模板");
            
            try
            {
                // 创建朋友圈默认模板
                var momentTemplate = new Template
                {
                    Id = Guid.NewGuid(),
                    Name = "default_moment",
                    DisplayName = "默认朋友圈模板",
                    Description = "默认的朋友圈模板",
                    Type = TemplateType.Moment,
                    Path = Path.Combine(_templateDirectory, "default_moment.svg"),
                    IsBuiltIn = true,
                    CreatedAt = DateTime.Now,
                    ModifiedAt = DateTime.Now
                };
                
                // 创建聊天默认模板
                var chatTemplate = new Template
                {
                    Id = Guid.NewGuid(),
                    Name = "default_chat",
                    DisplayName = "默认聊天模板",
                    Description = "默认的聊天模板",
                    Type = TemplateType.Chat,
                    Path = Path.Combine(_templateDirectory, "default_chat.svg"),
                    IsBuiltIn = true,
                    CreatedAt = DateTime.Now,
                    ModifiedAt = DateTime.Now
                };
                
                // 创建个人资料默认模板
                var profileTemplate = new Template
                {
                    Id = Guid.NewGuid(),
                    Name = "default_profile",
                    DisplayName = "默认个人资料模板",
                    Description = "默认的个人资料模板",
                    Type = TemplateType.Profile,
                    Path = Path.Combine(_templateDirectory, "default_profile.svg"),
                    IsBuiltIn = true,
                    CreatedAt = DateTime.Now,
                    ModifiedAt = DateTime.Now
                };
                
                // 添加默认模板并保存
                _templates.Add(momentTemplate.Id, momentTemplate);
                _templates.Add(chatTemplate.Id, chatTemplate);
                _templates.Add(profileTemplate.Id, profileTemplate);
                
                _logger.LogDebug("已创建 3 个默认模板，正在保存模板索引");
                
                // 复制默认资源文件到模板目录
                await CopyDefaultTemplateFilesAsync();
                
                // 保存模板索引
                await SaveTemplateIndexAsync();
                
                _logger.LogInformation("默认模板创建完成");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建默认模板时发生错误");
                throw;
            }
        }
        
        /// <summary>
        /// 复制默认模板文件到模板目录
        /// </summary>
        private async Task CopyDefaultTemplateFilesAsync()
        {
            _logger.LogDebug("开始复制默认模板文件");
            
            try
            {
                // 获取嵌入资源中的默认模板文件
                // 这里假设默认模板文件已包含在应用程序资源中
                // 实际实现可能需要根据项目结构调整
                string resourceDirPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources", "Templates");
                
                if (!Directory.Exists(resourceDirPath))
                {
                    _logger.LogWarning("默认模板资源目录不存在: {ResourceDir}", resourceDirPath);
                    return;
                }
                
                string[] defaultTemplateFiles = Directory.GetFiles(resourceDirPath, "*.svg");
                
                foreach (string file in defaultTemplateFiles)
                {
                    string fileName = Path.GetFileName(file);
                    string targetPath = Path.Combine(_templateDirectory, fileName);
                    
                    _logger.LogDebug("复制模板文件: {Source} -> {Target}", file, targetPath);
                    await _fileService.CopyFileAsync(file, targetPath, true);
                }
                
                _logger.LogInformation("复制了 {Count} 个默认模板文件", defaultTemplateFiles.Length);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "复制默认模板文件时发生错误");
            }
        }
        
        /// <summary>
        /// 保存模板索引
        /// </summary>
        private async Task SaveTemplateIndexAsync()
        {
            _logger.LogDebug("保存模板索引到: {IndexFilePath}", _indexFilePath);
            
            try
            {
                string json = JsonSerializer.Serialize(_templates.Values.ToList(), _jsonOptions);
                await _fileService.WriteTextAsync(_indexFilePath, json);
                _logger.LogDebug("模板索引保存成功");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "保存模板索引失败");
                throw;
            }
        }

        /// <summary>
        /// 获取所有模板
        /// </summary>
        public async Task<IEnumerable<Template>> GetAllAsync()
        {
            _logger.LogDebug("获取所有模板");
            
            // 如果模板字典为空，尝试重新初始化
            if (_templates.Count == 0)
            {
                _logger.LogDebug("模板集合为空，尝试重新初始化");
                await InitializeAsync();
            }
            
            return _templates.Values.ToList();
        }

        /// <summary>
        /// 根据ID获取模板
        /// </summary>
        public async Task<Template> GetByIdAsync(Guid id)
        {
            _logger.LogDebug("根据ID获取模板: {TemplateId}", id);
            
            // 如果模板字典为空，尝试重新初始化
            if (_templates.Count == 0)
            {
                _logger.LogDebug("模板集合为空，尝试重新初始化");
                await InitializeAsync();
            }
            
            if (_templates.TryGetValue(id, out Template template))
            {
                _logger.LogDebug("找到模板: {TemplateName}", template.Name);
                return template;
            }
            
            _logger.LogWarning("未找到ID为 {TemplateId} 的模板", id);
            return null;
        }

        /// <summary>
        /// 根据类型获取模板
        /// </summary>
        public async Task<IEnumerable<Template>> GetByTypeAsync(TemplateType templateType)
        {
            _logger.LogDebug("根据类型获取模板: {TemplateType}", templateType);
            
            // 如果模板字典为空，尝试重新初始化
            if (_templates.Count == 0)
            {
                _logger.LogDebug("模板集合为空，尝试重新初始化");
                await InitializeAsync();
            }
            
            var templates = _templates.Values
                .Where(t => t.Type == templateType)
                .ToList();
                
            _logger.LogDebug("找到 {Count} 个类型为 {TemplateType} 的模板", templates.Count, templateType);
            return templates;
        }

        /// <summary>
        /// 添加模板
        /// </summary>
        public async Task<Template> AddAsync(Template template)
        {
            if (template == null)
                throw new ArgumentNullException(nameof(template));
                
            _logger.LogInformation("添加新模板: {TemplateName}", template.Name ?? "未命名模板");
            
            // 确保模板有有效的ID
            if (template.Id == Guid.Empty)
            {
                template.Id = Guid.NewGuid();
                _logger.LogDebug("为新模板生成ID: {TemplateId}", template.Id);
            }
            
            // 确保创建和修改时间正确设置
            if (template.CreatedAt == default)
                template.CreatedAt = DateTime.Now;
                
            template.ModifiedAt = DateTime.Now;
            
            // 如果模板路径未指定，生成默认路径
            if (string.IsNullOrEmpty(template.Path))
            {
                string fileName = $"{template.Id}.svg";
                template.Path = Path.Combine(_templateDirectory, fileName);
                _logger.LogDebug("为新模板生成默认路径: {TemplatePath}", template.Path);
            }
            
            try
            {
                // 保存模板内容到文件（如果有内容）
                if (!string.IsNullOrEmpty(template.Content))
                {
                    _logger.LogDebug("保存模板内容到文件: {TemplatePath}", template.Path);
                    await _fileService.WriteTextAsync(template.Path, template.Content);
                }
                
                // 添加模板到字典
                _templates[template.Id] = template;
                _logger.LogDebug("添加模板到内存集合，当前模板数量: {Count}", _templates.Count);
                
                // 保存模板索引
                await SaveTemplateIndexAsync();
                
                _logger.LogInformation("模板添加成功: {TemplateId}", template.Id);
                return template;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "添加模板失败: {TemplateName}", template.Name);
                throw new Exception($"添加模板失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 更新模板
        /// </summary>
        public async Task<Template> UpdateAsync(Template template)
        {
            if (template == null)
                throw new ArgumentNullException(nameof(template));
                
            _logger.LogInformation("更新模板: {TemplateId} ({TemplateName})", template.Id, template.Name);
            
            if (!_templates.ContainsKey(template.Id))
            {
                _logger.LogWarning("未找到ID为 {TemplateId} 的模板，无法更新", template.Id);
                throw new KeyNotFoundException($"未找到ID为 {template.Id} 的模板");
            }
            
            try
            {
                // 更新修改时间
                template.ModifiedAt = DateTime.Now;
                
                // 保存模板内容到文件（如果有内容）
                if (!string.IsNullOrEmpty(template.Content))
                {
                    _logger.LogDebug("保存更新后的模板内容到文件: {TemplatePath}", template.Path);
                    await _fileService.WriteTextAsync(template.Path, template.Content);
                }
                
                // 更新模板到字典
                _templates[template.Id] = template;
                
                // 保存模板索引
                await SaveTemplateIndexAsync();
                
                _logger.LogInformation("模板更新成功: {TemplateId}", template.Id);
                return template;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新模板失败: {TemplateId}", template.Id);
                throw new Exception($"更新模板失败: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// 删除模板
        /// </summary>
        public async Task<bool> DeleteAsync(Guid id)
        {
            _logger.LogInformation("删除模板: {TemplateId}", id);
            
            if (!_templates.ContainsKey(id))
            {
                _logger.LogWarning("未找到ID为 {TemplateId} 的模板，无法删除", id);
                return false;
            }
            
            try
            {
                var template = _templates[id];
                
                // 删除模板文件
                if (!string.IsNullOrEmpty(template.Path) && _fileService.FileExists(template.Path))
                {
                    _logger.LogDebug("删除模板文件: {TemplatePath}", template.Path);
                    await _fileService.DeleteFileAsync(template.Path);
                }
                
                // 删除预览图
                if (!string.IsNullOrEmpty(template.PreviewImagePath) && _fileService.FileExists(template.PreviewImagePath))
                {
                    _logger.LogDebug("删除模板预览图: {PreviewPath}", template.PreviewImagePath);
                    await _fileService.DeleteFileAsync(template.PreviewImagePath);
                }
                
                // 从字典中移除
                _templates.Remove(id);
                _logger.LogDebug("从内存集合中移除模板，剩余模板数量: {Count}", _templates.Count);
                
                // 保存模板索引
                await SaveTemplateIndexAsync();
                
                _logger.LogInformation("模板删除成功: {TemplateId}", id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除模板失败: {TemplateId}", id);
                return false;
            }
        }
    }
}