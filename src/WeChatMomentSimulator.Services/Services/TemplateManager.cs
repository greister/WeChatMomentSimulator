using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Newtonsoft.Json;
using WeChatMomentSimulator.Core.Interfaces;
using WeChatMomentSimulator.Core.Interfaces.Services;
using Microsoft.Extensions.Logging;
using WeChatMomentSimulator.Core.Models.Template;
using WeChatMomentSimulator.Core.Models.Template.Enums;
using Formatting = Newtonsoft.Json.Formatting;
using LoggerExtensions = WeChatMomentSimulator.Core.Logging.LoggerExtensions;

namespace WeChatMomentSimulator.Services.Services
{
    /// <summary>
    /// 模板服务实现
    /// </summary>
    public class TemplateManager : ITemplateManager
    {
        private readonly string _templatesDirectory;
        private readonly ILogger<TemplateManager> _logger;
        private readonly ISvgRenderer _svgRenderer;
        private const string INDEX_FILE = "templates.json";
        private static readonly Regex _placeholderRegex = new Regex(@"{{([^{}]+)}}", RegexOptions.Compiled);

        /// <summary>
        /// 构造函数
        /// </summary>
        public TemplateManager(ISvgRenderer svgRenderer = null)
        {
            _logger = LoggerExtensions.GetLogger<TemplateManager>();
            _svgRenderer = svgRenderer;

            // 获取用户AppData目录下的模板存储路径
            string appDataFolder = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            _templatesDirectory = Path.Combine(appDataFolder, "WeChatMomentSimulator", "Templates");

            // 确保目录存在
            if (!Directory.Exists(_templatesDirectory))
            {
                Directory.CreateDirectory(_templatesDirectory);
                _logger.LogInformation("创建模板目录: {Directory}", _templatesDirectory);
            }
        }

        /// <summary>
        /// 获取所有可用模板
        /// </summary>
        public async Task<IEnumerable<string>> GetAllTemplatesAsync()
        {
            try
            {
                var index = await LoadTemplateIndexAsync();
                return index.Values
                    .OrderBy(t => t.Name)
                    .Select(t => t.Name)  // 将Template对象转换为名称字符串
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取模板列表失败");
                return new List<string>();
            }
        }

        public Task<IEnumerable<Template>> GetTemplatesAsync()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 根据ID获取模板
        /// </summary>
        public async Task<Template> GetTemplateByIdAsync(Guid id)
        {
            try
            {
                var index = await LoadTemplateIndexAsync();
                if (index.TryGetValue(id, out Template template))
                {
                    // 加载模板内容
                    template.Content = await LoadTemplateContentAsync(template.Id);
                    return template;
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取模板失败: ID {Id}", id);
                return null;
            }
        }

        /// <summary>
        /// 根据名称获取模板
        /// </summary>
        /// <summary>
        /// 根据名称获取模板
        /// </summary>
        public async Task<Template> GetTemplateByNameAsync(string templateName)
        {
            if (string.IsNullOrEmpty(templateName))
            {
                _logger.LogWarning("尝试获取无效的模板名称");
                return null;
            }

            try
            {
                string filePath = GetTemplateFilePath(templateName);

                if (File.Exists(filePath))
                {
                    var content = await File.ReadAllTextAsync(filePath);
                    return new Template
                    {
                        Name = templateName,
                        Content = content
                    };
                }
                else
                {
                    _logger.LogWarning("模板不存在: {TemplateName}", templateName);
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取模板失败: {TemplateName}", templateName);
                throw;
            }
        }

        /// <summary>
        /// 根据类型获取模板列表
        /// </summary>
        public async Task<IEnumerable<Template>> GetTemplatesByTypeAsync(TemplateType templateType)
        {
            try
            {
                var index = await LoadTemplateIndexAsync();
                return index.Values
                    .Where(t => t.Type == templateType)
                    .OrderBy(t => t.Name)
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取指定类型的模板失败: {TemplateType}", templateType);
                return new List<Template>();
            }
        }

        /// <summary>
        /// 获取默认模板
        /// </summary>
        public async Task<Template> GetDefaultTemplateAsync(TemplateType templateType)
        {
            try
            {
                var templates = await GetTemplatesByTypeAsync(templateType);
                var defaultTemplate = templates.FirstOrDefault(t => t.IsDefault)
                                      ?? templates.FirstOrDefault();

                if (defaultTemplate != null)
                {
                    // 加载模板内容
                    defaultTemplate.Content = await LoadTemplateContentAsync(defaultTemplate.Id);
                }

                return defaultTemplate;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取默认模板失败: {TemplateType}", templateType);
                return null;
            }
        }

        public Task SaveTemplateAsync(string templateName, string content)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 保存模板
        /// </summary>
        public async Task<Template> SaveTemplateAsync(Template template)
        {
            if (template == null)
            {
                throw new ArgumentNullException(nameof(template));
            }

            try
            {
                // 如果是新模板，生成ID
                if (template.Id == Guid.Empty)
                {
                    template.Id = Guid.NewGuid();
                }

                // 设置更新时间
                template.UpdatedAt = DateTime.Now;
                if (template.CreatedAt == default)
                {
                    template.CreatedAt = template.UpdatedAt;
                }

                // 加载模板��引
                var index = await LoadTemplateIndexAsync();

                // 更新索引
                index[template.Id] = template;

                // 保存模板内容
                await SaveTemplateContentAsync(template.Id, template.Content);

                // 保存索引
                await SaveTemplateIndexAsync(index);

                _logger.LogInformation("已保存模板: {TemplateName} (ID: {Id})", template.Name, template.Id);
                return template;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "保存模板失败: {TemplateName}", template.Name);
                throw;
            }
        }

        public string GetTemplateFilePath(Template template)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 删除模板
        /// </summary>
        public async Task<bool> DeleteTemplateByIdAsync(Guid id)
        {
            try
            {
                // 加载模板索引
                var index = await LoadTemplateIndexAsync();

                // 检查模板是否存在
                if (!index.TryGetValue(id, out Template template))
                {
                    _logger.LogWarning("尝试删除不存在的模板: ID {Id}", id);
                    return false;
                }

                // 从索引中删除
                index.Remove(id);

                // 保存索引
                await SaveTemplateIndexAsync(index);

                // 删除模板内容文件
                string contentFile = GetTemplateContentPath(id);
                if (File.Exists(contentFile))
                {
                    File.Delete(contentFile);
                }

                _logger.LogInformation("已删除模板: {TemplateName} (ID: {Id})", template.Name, id);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除模板失败: ID {Id}", id);
                return false;
            }
        }

        public Task<bool> DeleteTemplateByNameAsync(string templateName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 检查模板是否存在
        /// </summary>
        public async Task<bool> TemplateExistsAsync(string templateName)
        {
            if (string.IsNullOrEmpty(templateName))
            {
                return false;
            }

            try
            {
                var index = await LoadTemplateIndexAsync();
                return index.Values.Any(t =>
                    t.Name.Equals(templateName, StringComparison.OrdinalIgnoreCase));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "检查模板是否存在时出错: {TemplateName}", templateName);
                return false;
            }
        }

        /// <summary>
        /// 创建默认模板（如果不存在）
        /// </summary>
        public async Task CreateDefaultTemplatesAsync()
        {
            try
            {
                // 检查是否已有模板
                var templates = await GetAllTemplatesAsync();
                if (templates.Any())
                {
                    _logger.LogInformation("已存在模板，跳过创建默认模板");
                    return;
                }

                // 创建基本模板
                var basicTemplate = new Template
                {
                    Id = Guid.NewGuid(),
                    Name = "基本模板",
                    Description = "微信朋友圈基本显示模板",
                    Type = TemplateType.Moment,
                    IsDefault = true,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Content = GetBasicTemplateContent()
                };

                // 创建带图片的模板
                var imageTemplate = new Template
                {
                    Id = Guid.NewGuid(),
                    Name = "带图片模板",
                    Description = "含有图片的微信朋友圈模板",
                    Type = TemplateType.Moment,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now,
                    Content = GetImageTemplateContent()
                };

                // 保存模板
                await SaveTemplateAsync(basicTemplate);
                await SaveTemplateAsync(imageTemplate);

                _logger.LogInformation("已创建默认模板");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建默认模板失败");
            }
        }

        /// <summary>
        /// 获取模板中使用的占位符定义
        /// </summary>
        public async Task<IEnumerable<PlaceholderDefinition>> GetTemplatePlaceholdersAsync(Guid templateId)
        {
            try
            {
                // 获取模板
                var template = await GetTemplateByIdAsync(templateId);
                if (template == null || string.IsNullOrEmpty(template.Content))
                {
                    return new List<PlaceholderDefinition>();
                }

                // 解析占位符
                var matches = _placeholderRegex.Matches(template.Content);
                var placeholders = new Dictionary<string, PlaceholderDefinition>();

                foreach (Match match in matches)
                {
                    string name = match.Groups[1].Value.Trim();

                    // 处理条件语句
                    if (name.StartsWith("#if "))
                    {
                        name = name.Substring(4).Trim();
                        if (!placeholders.ContainsKey(name))
                        {
                            placeholders[name] = new PlaceholderDefinition
                            {
                                Name = name,
                                Type = PlaceholderType.Boolean,
                                DefaultValue = "false",
                                Description = $"条件: {name}"
                            };
                        }

                        continue;
                    }

                    // 普通占位符
                    if (!placeholders.ContainsKey(name))
                    {
                        // 根据常见占位符名称确定类型
                        var type = DetermineTypeFromName(name);

                        placeholders[name] = new PlaceholderDefinition
                        {
                            Name = name,
                            Type = type,
                            DefaultValue = GetDefaultValueForType(type, name),
                            Description = $"占位符: {name}"
                        };
                    }
                }

                return placeholders.Values.ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取模板占位符失败: {TemplateId}", templateId);
                return new List<PlaceholderDefinition>();
            }
        }

        /// <summary>
        /// 应用变量值到模板内容
        /// </summary>
        public async Task<string> ApplyVariablesAsync(Template template, Dictionary<string, object> variableValues)
        {
            if (template == null || string.IsNullOrEmpty(template.Content))
            {
                return string.Empty;
            }

            try
            {
                string content = template.Content;

                // 处理条件表达式
                content = await ProcessConditionalsAsync(content, variableValues);

                // 替换占位符
                foreach (var variable in variableValues)
                {
                    string placeholder = $"{{{{{variable.Key}}}}}";
                    string value = variable.Value?.ToString() ?? string.Empty;

                    content = content.Replace(placeholder, value);
                }

                return content;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "应用变量值到模板失败: {TemplateName}", template.Name);
                return template.Content;
            }
        }

        /// <summary>
        /// 导出模板
        /// </summary>
        public async Task<bool> ExportTemplateAsync(Guid templateId, string exportPath)
        {
            try
            {
                // 获取模板
                var template = await GetTemplateByIdAsync(templateId);
                if (template == null)
                {
                    _logger.LogWarning("导出不存在的模板: ID {Id}", templateId);
                    return false;
                }

                // 创建导出对象
                var exportObj = new
                {
                    template.Id,
                    template.Name,
                    template.Description,
                    template.Type,
                    template.IsDefault,
                    template.CreatedAt,
                    template.UpdatedAt,
                    Content = template.Content ?? string.Empty
                };

                // 序列化并保存
                string json = JsonConvert.SerializeObject(exportObj, Formatting.Indented);
                await File.WriteAllTextAsync(exportPath, json);

                _logger.LogInformation("已导出模板: {TemplateName} 到 {Path}", template.Name, exportPath);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "导出模板失败: ID {Id} 到 {Path}", templateId, exportPath);
                return false;
            }
        }

        Task<Template> ITemplateManager.UpdateTemplateAsync(Template template)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 导入模板
        /// </summary>
        public async Task<Template> ImportTemplateAsync(string importPath)
        {
            try
            {
                // 读取文件
                string json = await File.ReadAllTextAsync(importPath);
                var importedTemplate = JsonConvert.DeserializeObject<Template>(json);

                // 验证模板
                if (importedTemplate == null || string.IsNullOrEmpty(importedTemplate.Content))
                {
                    _logger.LogWarning("导入的模板无效: {Path}", importPath);
                    return null;
                }

                // 检查SVG有效性
                if (!IsValidSvg(importedTemplate.Content))
                {
                    _logger.LogWarning("导入的模板SVG内容无效: {Path}", importPath);
                    return null;
                }

                // 确保模板有ID
                if (importedTemplate.Id == Guid.Empty)
                {
                    importedTemplate.Id = Guid.NewGuid();
                }

                // 检查名称是否已存在
                var existing = await GetTemplateByNameAsync(importedTemplate.Name);
                if (existing != null && existing.Id != importedTemplate.Id)
                {
                    // 重命名导入的模板
                    importedTemplate.Name = $"{importedTemplate.Name} (导入)";
                }

                // 设置导入时间
                importedTemplate.UpdatedAt = DateTime.Now;

                // 保存模板
                return await SaveTemplateAsync(importedTemplate);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "导入模板失败: {Path}", importPath);
                return null;
            }
        }

        public Task<string> ConvertTemplateToStringAsync(Template template)
        {
            throw new NotImplementedException();
        }

        #region 私有辅助方法

        /// <summary>
        /// 加载模板索引
        /// </summary>
        private async Task<Dictionary<Guid, Template>> LoadTemplateIndexAsync()
        {
            string indexPath = Path.Combine(_templatesDirectory, INDEX_FILE);

            if (!File.Exists(indexPath))
            {
                return new Dictionary<Guid, Template>();
            }

            try
            {
                string json = await File.ReadAllTextAsync(indexPath);
                var templates = JsonConvert.DeserializeObject<List<Template>>(json) ?? new List<Template>();

                // 转换为字典
                return templates.ToDictionary(t => t.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载模板索引失败");
                return new Dictionary<Guid, Template>();
            }
        }

        /// <summary>
        /// 保存模板索引
        /// </summary>
        private async Task SaveTemplateIndexAsync(Dictionary<Guid, Template> index)
        {
            string indexPath = Path.Combine(_templatesDirectory, INDEX_FILE);

            try
            {
                // 创建一个不含Content的模板列表进行保存
                var templates = index.Values.Select(t => new Template
                {
                    Id = t.Id,
                    Name = t.Name,
                    Description = t.Description,
                    Type = t.Type,
                    IsDefault = t.IsDefault,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt
                }).ToList();

                string json = JsonConvert.SerializeObject(templates, Formatting.Indented);
                await File.WriteAllTextAsync(indexPath, json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "保存模板索引失败");
                throw;
            }
        }

        /// <summary>
        /// 获取模板内容文件路径
        /// </summary>
        private string GetTemplateContentPath(Guid id)
        {
            return Path.Combine(_templatesDirectory, $"{id}.svg");
        }

        /// <summary>
        /// 加载模板内容
        /// </summary>
        private async Task<string> LoadTemplateContentAsync(Guid id)
        {
            string path = GetTemplateContentPath(id);

            if (!File.Exists(path))
            {
                return null;
            }

            try
            {
                return await File.ReadAllTextAsync(path);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载模板内容失败: ID {Id}", id);
                return null;
            }
        }

        /// <summary>
        /// 保存模板内容
        /// </summary>
        private async Task SaveTemplateContentAsync(Guid id, string content)
        {
            string path = GetTemplateContentPath(id);

            try
            {
                await File.WriteAllTextAsync(path, content ?? string.Empty);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "保存模板内容失败: ID {Id}", id);
                throw;
            }
        }

        /// <summary>
        /// 处理模板条件表达式
        /// </summary>
        private Task<string> ProcessConditionalsAsync(string template, Dictionary<string, object> variables)
        {
            // 处理条件表达式: {{#if variable}}...{{/if}}
            var ifRegex = new Regex(@"{{#if\s+([^}]+)}}(.*?){{/if}}", RegexOptions.Singleline);

            return Task.FromResult(ifRegex.Replace(template, match =>
            {
                string varName = match.Groups[1].Value.Trim();
                string content = match.Groups[2].Value;

                // 检查变量值
                if (variables.TryGetValue(varName, out object value))
                {
                    bool condition = false;

                    // 处理不同类型的值
                    if (value is bool boolValue)
                    {
                        condition = boolValue;
                    }
                    else if (value is string strValue)
                    {
                        condition = !string.IsNullOrEmpty(strValue) &&
                                    !strValue.Equals("false", StringComparison.OrdinalIgnoreCase);
                    }
                    else
                    {
                        condition = value != null;
                    }

                    return condition ? content : string.Empty;
                }

                return string.Empty; // 变量不存在时不显示内容
            }));
        }

        /// <summary>
        /// 根据名称确定占位符类型
        /// </summary>
        private PlaceholderType DetermineTypeFromName(string name)
        {
            name = name.ToLowerInvariant();

            if (name.Contains("time") || name.Contains("date"))
            {
                return PlaceholderType.DateTime;
            }

            if (name.StartsWith("is") || name.StartsWith("has") || name.EndsWith("able"))
            {
                return PlaceholderType.Boolean;
            }

            if (name.Contains("count") || name.Contains("number") || name.EndsWith("s"))
            {
                return PlaceholderType.Number;
            }



            if (name.Contains("image") || name.Contains("photo") || name.Contains("picture"))
            {
                return PlaceholderType.Image;
            }

            return PlaceholderType.Text;
        }

        /// <summary>
        /// 获取占位符类型的默认值
        /// </summary>
        private string GetDefaultValueForType(PlaceholderType type, string name)
        {
            switch (type)
            {
                case PlaceholderType.DateTime:
                    return DateTime.Now.ToString("yyyy-MM-dd HH:mm");

                case PlaceholderType.Boolean:
                    return "false";

                case PlaceholderType.Number:
                    return "0";


                case PlaceholderType.Image:
                    return "data:image/png;base64,iVBORw0KGgoAAAANSUh...";

                case PlaceholderType.Text:
                default:
                    if (name.Equals("username", StringComparison.OrdinalIgnoreCase))
                    {
                        return "用户名";
                    }

                    if (name.Equals("content", StringComparison.OrdinalIgnoreCase))
                    {
                        return "朋友圈内容";
                    }

                    return $"[{name}]";
            }
        }

        /// <summary>
        /// 验证SVG内容是否有效
        /// </summary>
        private bool IsValidSvg(string content)
        {
            try
            {
                if (string.IsNullOrEmpty(content))
                {
                    return false;
                }

                // 检查是否包含SVG标签
                if (!content.Contains("<svg") || !content.Contains("</svg>"))
                {
                    return false;
                }

                // 尝试解析XML
                var xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(content);

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 获取基本模板内容
        /// </summary>
        private string GetBasicTemplateContent()
        {
            return @"<svg width=""400"" height=""600"" xmlns=""http://www.w3.org/2000/svg"">
  <!-- 状态栏 -->
  <rect x=""0"" y=""0"" width=""400"" height=""40"" fill=""#f5f5f5"" />
  <text x=""20"" y=""25"" font-family=""Arial"" font-size=""14"">{{time}}</text>
  <text x=""380"" y=""25"" font-family=""Arial"" font-size=""14"" text-anchor=""end"">{{battery}}</text>

  <!-- 标题栏 -->
  <rect x=""0"" y=""40"" width=""400"" height=""50"" fill=""#ffffff"" />
  <text x=""200"" y=""70"" font-family=""Arial"" font-size=""18"" text-anchor=""middle"" font-weight=""bold"">朋友圈</text>

  <!-- 用户信息 -->
  <circle cx=""50"" cy=""120"" r=""30"" fill=""#dddddd"" />
  <text x=""100"" y=""115"" font-family=""Arial"" font-size=""16"" font-weight=""bold"">{{userName}}</text>
  <text x=""100"" y=""135"" font-family=""Arial"" font-size=""14"" fill=""#888888"">{{timeText}}</text>

  <!-- 内容 -->
  <text x=""50"" y=""180"" font-family=""Arial"" font-size=""14"" width=""300"">{{content}}</text>

  <!-- 互动区域 -->
  {{#if hasComments}}
  <rect x=""20"" y=""250"" width=""360"" height=""100"" fill=""#f9f9f9"" rx=""5"" />
  {{/if}}

  <!-- 点赞信息 -->
  <text x=""30"" y=""270"" font-family=""Arial"" font-size=""13"" fill=""#666666"">
    ❤️ {{likes}} 人点赞
  </text>
</svg>";
        }

        /// <summary>
        /// 获取带图片的模板内容
        /// </summary>
        private string GetImageTemplateContent()
        {
            return @"<svg width=""400"" height=""600"" xmlns=""http://www.w3.org/2000/svg"">
  <!-- 状态栏 -->
  <rect x=""0"" y=""0"" width=""400"" height=""40"" fill=""#f5f5f5"" />
  <text x=""20"" y=""25"" font-family=""Arial"" font-size=""14"">{{time}}</text>
  <text x=""380"" y=""25"" font-family=""Arial"" font-size=""14"" text-anchor=""end"">{{battery}}</text>

  <!-- 标题栏 -->
  <rect x=""0"" y=""40"" width=""400"" height=""50"" fill=""#ffffff"" />
  <text x=""200"" y=""70"" font-family=""Arial"" font-size=""18"" text-anchor=""middle"" font-weight=""bold"">朋友圈</text>

  <!-- 用户信息 -->
  <circle cx=""50"" cy=""120"" r=""30"" fill=""#dddddd"" />
  <text x=""100"" y=""115"" font-family=""Arial"" font-size=""16"" font-weight=""bold"">{{userName}}</text>
  <text x=""100"" y""135"" font-family=""Arial"" font-size=""14"" fill=""#888888"">{{timeText}}</text>

  <!-- 内容 -->
  <text x=""50"" y=""180"" font-family=""Arial"" font-size=""14"" width=""300"">{{content}}</text>

  <!-- 图片 -->
  {{#if hasImages}}
  <rect x=""50"" y=""200"" width=""150"" height=""150"" fill=""#eeeeee"" rx=""5"" />
  <text x=""125"" y=""275"" font-family=""Arial"" font-size=""14"" text-anchor=""middle"">[图片]</text>
  {{/if}}

  <!-- 互动区域 -->
  {{#if hasComments}}
  <rect x=""20"" y=""370"" width=""360"" height=""100"" fill=""#f9f9f9"" rx=""5"" />
  {{/if}}

  <!-- 点赞信息 -->
  <text x=""30"" y=""390"" font-family=""Arial"" font-size=""13"" fill=""#666666"">
    ❤️ {{likes}} 人点赞
  </text>

  <!-- 评论区 -->
  {{#if hasComments}}
  <text x=""30"" y=""420"" font-family=""Arial"" font-size=""13"" fill=""#666666"">{{commenter1}}: {{comment1}}</text>
  {{#if hasMultipleComments}}
  <text x=""30"" y""440"" font-family=""Arial"" font-size=""13"" fill=""#666666"">{{commenter2}}: {{comment2}}</text>
  {{/if}}
  {{/if}}
</svg>";
        }




        /// <summary>
        /// 删除模板
        /// </summary>
        public async Task DeleteTemplateAsync(string templateName)
        {
            if (string.IsNullOrEmpty(templateName))
            {
                _logger.LogWarning("尝试删除无效的模板名称");
                return;
            }

            try
            {
                string filePath = GetTemplateFilePath(templateName);

                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    _logger.LogInformation("已删除模板: {TemplateName}", templateName);
                }

                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除模板失败: {TemplateName}", templateName);
                throw;
            }
        }



        /// <summary>
        /// 获取模板文件路径
        /// </summary>
        public string GetTemplateFilePath(string templateName)
        {
            // 确保有效的文件名
            string safeFileName = string.Join("_", templateName.Split(Path.GetInvalidFileNameChars()));
            return Path.Combine(_templatesDirectory, $"{safeFileName}.svg");
        }


        /// <summary>
        /// 更新模板
        /// </summary>
        public async Task UpdateTemplateAsync(Template template)
        {
            if (template == null || string.IsNullOrEmpty(template.Name))
            {
                _logger.LogWarning("尝试更新无效的模板");
                return;
            }

            try
            {
                string filePath = GetTemplateFilePath(template.Name);

                // 将模板内容写入文件
                await File.WriteAllTextAsync(filePath, template.Content);
                _logger.LogInformation("已更新模板: {TemplateName}", template.Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新模板失败: {TemplateName}", template.Name);
                throw;
            }
        }


        /// <summary>
        /// 获取模板列表
        /// </summary>
        public async Task<IEnumerable<Template>> GetTemplateListAsync()
        {
            try
            {
                var templates = new List<Template>();

                // 获取模板目录中的所有文件
                var files = Directory.GetFiles(_templatesDirectory, "*.svg");

                foreach (var file in files)
                {
                    var content = await File.ReadAllTextAsync(file);
                    var templateName = Path.GetFileNameWithoutExtension(file);

                    templates.Add(new Template
                    {
                        Name = templateName,
                        Content = content
                    });
                }

                return templates;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "获取模板列表失败");
                throw;
            }
        }

        #endregion


        /// <summary>
        /// 将模板对象转换为字符串表示
        /// </summary>
       
    }
}