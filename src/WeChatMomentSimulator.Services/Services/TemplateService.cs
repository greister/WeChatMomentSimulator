using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WeChatMomentSimulator.Core.Logging;
using Microsoft.Extensions.Logging;
using WeChatMomentSimulator.Core.Interfaces.Repositories;
using WeChatMomentSimulator.Core.Interfaces.Services;
using WeChatMomentSimulator.Core.Models.Template;
using WeChatMomentSimulator.Core.Models.Template.Enums;
using LoggerExtensions = WeChatMomentSimulator.Core.Logging.LoggerExtensions;

namespace WeChatMomentSimulator.Services.Services
{
    /// <summary>
    /// 模板服务实现
    /// </summary>
    public class TemplateService : ITemplateService
    {
        private readonly ITemplateRepository _templateRepository;
        private readonly IFileService _fileService;
        private readonly ILogger<TemplateService> _logger;
        
        // 变量解析的正则表达式
        private static readonly Regex VariableRegex = new Regex(@"{{([^{}]+)}}", RegexOptions.Compiled);
        
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="templateRepository">模板仓库</param>
        /// <param name="fileService">文件服务</param>
        public TemplateService(
            ITemplateRepository templateRepository,
            IFileService fileService)
        {
            _templateRepository = templateRepository ?? throw new ArgumentNullException(nameof(templateRepository));
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _logger = LoggerExtensions.GetLogger<TemplateService>();
            
            _logger.LogDebug("模板服务已初始化");
        }
        
        /// <summary>
        /// 获取所有模板
        /// </summary>
        public async Task<IEnumerable<Template>> GetAllTemplatesAsync()
        {
            _logger.LogDebug("获取所有模板");
            return await _templateRepository.GetAllAsync();
        }
        
        /// <summary>
        /// 根据ID获取模板
        /// </summary>
        public async Task<Template> GetTemplateByIdAsync(Guid id)
        {
            _logger.LogDebug("根据ID获取模板: {TemplateId}", id);
            var template = await _templateRepository.GetByIdAsync(id);
            
            if (template != null && string.IsNullOrEmpty(template.Content))
            {
                _logger.LogDebug("模板内容为空，正在加载内容");
                template = await LoadTemplateContentAsync(template);
            }
            
            if (template == null)
            {
                _logger.LogWarning("未找到ID为 {TemplateId} 的模板", id);
            }
            
            return template;
        }
        
        /// <summary>
        /// 根据类型获取模板
        /// </summary>
        public async Task<IEnumerable<Template>> GetTemplatesByTypeAsync(TemplateType templateType)
        {
            _logger.LogDebug("根据类型获取模板: {TemplateType}", templateType);
            return await _templateRepository.GetByTypeAsync(templateType);
        }
        
        /// <summary>
        /// 保存模板
        /// </summary>
        public async Task<Template> SaveTemplateAsync(Template template)
        {
            if (template == null)
                throw new ArgumentNullException(nameof(template));
            
            _logger.LogInformation("保存模板: {TemplateName}", template.Name ?? "未命名");
            
            // 检查模板是否有ID，如果没有则是新建模板
            if (template.Id == Guid.Empty)
            {
                template.Id = Guid.NewGuid();
                template.CreatedAt = DateTime.Now;
                _logger.LogDebug("创建新模板，ID: {TemplateId}", template.Id);
                template = await _templateRepository.AddAsync(template);
            }
            else
            {
                // 更新现有模板
                template.ModifiedAt = DateTime.Now;
                _logger.LogDebug("更新现有模板，ID: {TemplateId}", template.Id);
                template = await _templateRepository.UpdateAsync(template);
            }
            
            // 解析模板变量
            if (!string.IsNullOrEmpty(template.Content))
            {
                _logger.LogDebug("解析模板变量");
                template.Variables = ExtractVariables(template.Content).ToList();
                _logger.LogDebug("找到 {Count} 个变量", template.Variables.Count);
            }
            
            return template;
        }
        
        /// <summary>
        /// 删除模板
        /// </summary>
        public async Task<bool> DeleteTemplateAsync(Guid id)
        {
            _logger.LogInformation("删除模板，ID: {TemplateId}", id);
            return await _templateRepository.DeleteAsync(id);
        }
        
        /// <summary>
        /// 获取默认模板
        /// </summary>
        public async Task<Template> GetDefaultTemplateAsync(TemplateType templateType)
        {
            _logger.LogDebug("获取类型 {TemplateType} 的默认模板", templateType);
            var templates = await _templateRepository.GetByTypeAsync(templateType);
            var defaultTemplate = templates.FirstOrDefault(t => t.IsDefault);
            
            if (defaultTemplate == null)
            {
                _logger.LogDebug("未找到默认模板，使用该类型的第一个模板");
                // 如果没有找到默认模板，返回该类型的第一个模板
                defaultTemplate = templates.FirstOrDefault();
            }
            
            if (defaultTemplate != null && string.IsNullOrEmpty(defaultTemplate.Content))
            {
                _logger.LogDebug("加载默认模板内容");
                defaultTemplate = await LoadTemplateContentAsync(defaultTemplate);
            }
            
            if (defaultTemplate == null)
            {
                _logger.LogWarning("未找到类型 {TemplateType} 的默认模板", templateType);
            }
            else
            {
                _logger.LogInformation("已加载默认模板: {TemplateName}", defaultTemplate.Name);
            }
            
            return defaultTemplate;
        }
        
        /// <summary>
        /// 加载模板内容
        /// </summary>
        public async Task<Template> LoadTemplateContentAsync(Template template)
        {
            if (template == null)
                throw new ArgumentNullException(nameof(template));
            
            try
            {
                string templatePath = string.IsNullOrEmpty(template.Path)
                    ? Path.Combine("Templates", $"{template.Id}.svg")
                    : template.Path;
                
                _logger.LogDebug("尝试从路径加载模板内容: {TemplatePath}", templatePath);
                    
                if (_fileService.FileExists(templatePath))
                {
                    template.Content = await _fileService.ReadTextAsync(templatePath);
                    _logger.LogDebug("已加载模板内容，长度: {ContentLength} 字符", template.Content?.Length ?? 0);
                    
                    // 解析模板变量
                    if (!string.IsNullOrEmpty(template.Content))
                    {
                        template.Variables = ExtractVariables(template.Content).ToList();
                        _logger.LogDebug("从内容中解析到 {Count} 个变量", template.Variables.Count);
                    }
                }
                else
                {
                    _logger.LogWarning("模板文件不存在: {TemplatePath}", templatePath);
                }
                
                return template;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载模板内容失败, 模板ID: {TemplateId}", template.Id);
                throw new Exception($"加载模板内容失败: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// 从模板内容中提取变量
        /// </summary>
        private IEnumerable<TemplateVariable> ExtractVariables(string content)
        {
            if (string.IsNullOrEmpty(content))
                yield break;
                
            var matches = VariableRegex.Matches(content);
            var processedVars = new HashSet<string>();
            
            _logger.LogDebug("开始从内容中提取变量，匹配数: {MatchCount}", matches.Count);
            
            foreach (Match match in matches)
            {
                string varName = match.Groups[1].Value.Trim();
                
                // 跳过已处理的变量
                if (processedVars.Contains(varName))
                    continue;
                    
                processedVars.Add(varName);
                
                // 创建变量对象
                var variable = new TemplateVariable
                {
                    Name = varName,
                    DisplayName = varName,
                    Type = InferVariableType(varName),
                    Description = $"变量 {varName}",
                    IsRequired = true
                };
                
                _logger.LogDebug("提取到变量: {VariableName}, 类型: {VariableType}", varName, variable.Type);
                
                yield return variable;
            }
        }
        
        /// <summary>
        /// 根据变量名称推断变量类型
        /// </summary>
        private VariableType InferVariableType(string variableName)
        {
            variableName = variableName.ToLowerInvariant();
            
            if (variableName.Contains("color") || variableName.Contains("colour"))
                return VariableType.Color;
                
            if (variableName.Contains("image") || variableName.Contains("pic") || variableName.Contains("photo") || variableName.Contains("avatar"))
                return VariableType.ImagePath;
                
            if (variableName.Contains("date") || variableName.Contains("time"))
                return VariableType.DateTime;
                
            if (variableName.Contains("enable") || variableName.Contains("disable") || variableName.Contains("is") || variableName.Contains("has"))
                return VariableType.Boolean;
                
            if (variableName.Contains("count") || variableName.Contains("number") || variableName.Contains("amount") || variableName.Contains("level"))
                return VariableType.Number;
                
            if (variableName.Contains("json") || variableName.Contains("object"))
                return VariableType.JsonObject;
                
            if (variableName.Contains("rich") || variableName.Contains("html"))
                return VariableType.RichText;
                
            return VariableType.Text;
        }
        
        /// <summary>
        /// 应用变量值到模板内容
        /// </summary>
        public string ApplyVariables(Template template, Dictionary<string, object> variableValues)
        {
            if (template == null)
                throw new ArgumentNullException(nameof(template));
                
            if (string.IsNullOrEmpty(template.Content))
                throw new InvalidOperationException("模板内容为空");
                
            if (variableValues == null || variableValues.Count == 0)
                return template.Content;
                
            _logger.LogDebug("应用 {Count} 个变量到模板: {TemplateId}", variableValues.Count, template.Id);
            
            string content = template.Content;
            
            foreach (var kvp in variableValues)
            {
                string pattern = $"{{{{{kvp.Key}}}}}";
                string replacement = kvp.Value?.ToString() ?? string.Empty;
                
                _logger.LogDebug("替换变量 {VariableName}: {Pattern} => {Value}", kvp.Key, pattern, replacement);
                
                content = content.Replace(pattern, replacement);
            }
            
            return content;
        }
    }
}