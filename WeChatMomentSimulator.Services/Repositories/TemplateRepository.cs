// 文件路径: WeChatMomentSimulator.Services/Repositories/TemplateRepository.cs
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using WeChatMomentSimulator.Core.Interfaces;
using WeChatMomentSimulator.Core.Models;
using WeChatMomentSimulator.Core.Models.Enums;
using WeChatMomentSimulator.Services.Storage;

namespace WeChatMomentSimulator.Services.Repositories
{
    public class TemplateRepository : ITemplateRepository
    {
        private readonly ILogger<TemplateRepository> _logger;
        private readonly FileStorageService _fileStorage;
        private readonly string _templatesPath;
        private readonly Dictionary<string, BaseTemplate> _cache = new Dictionary<string, BaseTemplate>();

        public TemplateRepository(ILogger<TemplateRepository> logger, FileStorageService fileStorage)
        {
            _logger = logger;
            _fileStorage = fileStorage;
            _templatesPath = Path.Combine("templates");
            
            // 确保目录结构存在
            EnsureDirectoryStructure();
        }

        private void EnsureDirectoryStructure()
        {
            try
            {
                // 这里简化处理，实际实现应该使用FileStorageService来创建目录
                var baseDir = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "WeChatMomentSimulator");
                    
                Directory.CreateDirectory(Path.Combine(baseDir, "templates", "wechat"));
                Directory.CreateDirectory(Path.Combine(baseDir, "templates", "aikan"));
                Directory.CreateDirectory(Path.Combine(baseDir, "templates", "assets"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建模板目录结构失败");
            }
        }

        public async Task<IEnumerable<BaseTemplate>> GetAllTemplatesAsync()
        {
            try
            {
                var templates = new List<BaseTemplate>();
                
                // 先获取朋友圈模板
                var momentTemplates = await GetTemplatesByTypeAsync(TemplateType.WechatMoment);
                templates.AddRange(momentTemplates);
                
                // 再获取爱看模板
                var aikanTemplates = await GetTemplatesByTypeAsync(TemplateType.WechatAikan);
                templates.AddRange(aikanTemplates);
                
                return templates;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取所有模板失败");
                return Enumerable.Empty<BaseTemplate>();
            }
        }

        public async Task<BaseTemplate> GetTemplateByIdAsync(string id)
        {
            // 先从缓存中查找
            if (_cache.TryGetValue(id, out var cachedTemplate))
                return cachedTemplate;
                
            try
            {
                // 读取模板索引查找ID
                var allTemplates = await GetAllTemplatesAsync();
                var template = allTemplates.FirstOrDefault(t => t.Id == id);
                
                if (template != null)
                {
                    // 加载SVG内容
                    await LoadSvgContentForTemplate(template);
                    
                    // 添加到缓存
                    _cache[id] = template;
                }
                
                return template;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"获取模板失败: {id}");
                return null;
            }
        }

        public async Task<IEnumerable<BaseTemplate>> GetTemplatesByTypeAsync(TemplateType type)
        {
            try
            {
                var templates = new List<BaseTemplate>();
                
                // 根据类型确定要读取的目录
                string typePath = type == TemplateType.WechatMoment ? "wechat" : "aikan";
                string indexPath = Path.Combine(_templatesPath, typePath, "metadata.json");
                
                // 读取元数据索引
                var metadata = await _fileStorage.ReadJsonAsync<List<TemplateMetadata>>(indexPath);
                
                if (metadata == null)
                {
                    // 如果元数据不存在，返回空列表
                    _logger.LogWarning($"模板元数据不存在: {indexPath}");
                    return templates;
                }
                
                // 转换元数据为模板对象
                foreach (var item in metadata)
                {
                    BaseTemplate template = type == TemplateType.WechatMoment 
                        ? new WechatMomentTemplate { Id = item.Id, Name = item.Name, FilePath = item.FilePath, Type = type }
                        : new WechatAikanTemplate { Id = item.Id, Name = item.Name, FilePath = item.FilePath, Type = type };
                        
                    // 添加到集合
                    templates.Add(template);
                }
                
                return templates;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"获取模板类型失败: {type}");
                return Enumerable.Empty<BaseTemplate>();
            }
        }
        
        private async Task LoadSvgContentForTemplate(BaseTemplate template)
        {
            // 组合文件路径
            string typePath = template.Type == TemplateType.WechatMoment ? "wechat" : "aikan";
            string svgPath = Path.Combine(_templatesPath, typePath, template.FilePath);
            
            try
            {
                // 这里简化为直接从文件系统读取，实际应该使用FileStorageService
                var appDataPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "WeChatMomentSimulator");
                string fullPath = Path.Combine(appDataPath, svgPath);
                
                if (File.Exists(fullPath))
                {
                    template.SvgContent = await File.ReadAllTextAsync(fullPath);
                }
                else
                {
                    _logger.LogWarning($"模板SVG文件不存在: {fullPath}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"读取SVG内容失败: {svgPath}");
            }
        }

        public async Task SaveTemplateAsync(BaseTemplate template)
        {
            try
            {
                // 根据类型确定保存路径
                string typePath = template.Type == TemplateType.WechatMoment ? "wechat" : "aikan";
                string indexPath = Path.Combine(_templatesPath, typePath, "metadata.json");
                
                // 读取当前元数据
                var metadata = await _fileStorage.ReadJsonAsync<List<TemplateMetadata>>(indexPath) 
                    ?? new List<TemplateMetadata>();
                    
                // 检查是否已存在此模板
                var existingItem = metadata.FirstOrDefault(m => m.Id == template.Id);
                
                if (existingItem != null)
                {
                    // 更新现有条目
                    existingItem.Name = template.Name;
                    existingItem.FilePath = template.FilePath;
                }
                else
                {
                    // 添加新条目
                    metadata.Add(new TemplateMetadata
                    {
                        Id = template.Id ?? Guid.NewGuid().ToString(),
                        Name = template.Name,
                        FilePath = template.FilePath ?? $"{template.Name.Replace(" ", "_")}.svg"
                    });
                    
                    // 确保模板有ID
                    if (string.IsNullOrEmpty(template.Id))
                        template.Id = metadata.Last().Id;
                }
                
                // 保存元数据
                await _fileStorage.WriteJsonAsync(indexPath, metadata);
                
                // 如果有SVG内容，保存SVG文件
                if (!string.IsNullOrEmpty(template.SvgContent))
                {
                    string svgPath = Path.Combine(_templatesPath, typePath, template.FilePath);
                    // 这里应该使用FileStorageService的文件写入方法
                    var appDataPath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "WeChatMomentSimulator");
                    string fullPath = Path.Combine(appDataPath, svgPath);
                    
                    // 确保目录存在
                    Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
                    
                    // 写入文件
                    await File.WriteAllTextAsync(fullPath, template.SvgContent);
                }
                
                // 更新缓存
                _cache[template.Id] = template;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"保存模板失败: {template.Name}");
                throw;
            }
        }

        public async Task DeleteTemplateAsync(string id)
        {
            try
            {
                // 先获取模板以确定类型
                var template = await GetTemplateByIdAsync(id);
                if (template == null)
                {
                    _logger.LogWarning($"要删除的模板不存在: {id}");
                    return;
                }
                
                // 根据类型确定元数据路径
                string typePath = template.Type == TemplateType.WechatMoment ? "wechat" : "aikan";
                string indexPath = Path.Combine(_templatesPath, typePath, "metadata.json");
                
                // 读取当前元数据
                var metadata = await _fileStorage.ReadJsonAsync<List<TemplateMetadata>>(indexPath);
                if (metadata == null)
                {
                    _logger.LogWarning($"模板元数据不存在: {indexPath}");
                    return;
                }
                
                // 删除元数据条目
                var item = metadata.FirstOrDefault(m => m.Id == id);
                if (item != null)
                {
                    metadata.Remove(item);
                    await _fileStorage.WriteJsonAsync(indexPath, metadata);
                    
                    // 删除SVG文件
                    string svgPath = Path.Combine(_templatesPath, typePath, item.FilePath);
                    var appDataPath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                        "WeChatMomentSimulator");
                    string fullPath = Path.Combine(appDataPath, svgPath);
                    
                    if (File.Exists(fullPath))
                        File.Delete(fullPath);
                }
                
                // 清除缓存
                if (_cache.ContainsKey(id))
                    _cache.Remove(id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"删除模板失败: {id}");
                throw;
            }
        }
    }
    
    // 辅助类，用于模板元数据序列化
    internal class TemplateMetadata
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string FilePath { get; set; }
    }
}