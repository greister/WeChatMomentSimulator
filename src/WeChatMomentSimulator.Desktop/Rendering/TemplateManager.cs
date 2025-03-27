using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using WeChatMomentSimulator.Core.Interfaces.Services;

namespace WeChatMomentSimulator.Desktop.Rendering
{
    /// <summary>
    /// 模板管理器，负责加载、保存和管理SVG模板
    /// </summary>
    public class TemplateManager: ITemplateManager
    {
        private readonly string _templateDirectory;
        private Dictionary<string, string> _templateCache = new Dictionary<string, string>();
        
        /// <summary>
        /// 初始化模板管理器
        /// </summary>
        public TemplateManager(string templateDirectory = null)
        {
            // 如果没有指定目录，使用应用程序目录下的Templates文件夹
            _templateDirectory = templateDirectory ?? 
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Templates");
            
            // 确保模板目录存在
            EnsureTemplateDirectoryExists();
        }
        
        /// <summary>
        /// 获取所有可用的模板名称
        /// </summary>
        public IEnumerable<string> GetAvailableTemplates()
        {
            EnsureTemplateDirectoryExists();
            
            // 获取所有.svg文件并返回不带扩展名的文件名
            return Directory.GetFiles(_templateDirectory, "*.svg")
                .Select(Path.GetFileNameWithoutExtension);
        }
        
        /// <summary>
        /// 获取指定名称的模板内容
        /// </summary>
        public string GetTemplate(string templateName)
        {
            // 先从缓存中查找
            if (_templateCache.TryGetValue(templateName, out string content))
            {
                return content;
            }
            
            // 构建模板文件路径
            string templatePath = Path.Combine(_templateDirectory, $"{templateName}.svg");
            
            // 检查文件是否存在
            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException($"模板文件不存在: {templateName}.svg");
            }
            
            // 读取模板内容
            content = File.ReadAllText(templatePath);
            
            // 添加到缓存
            _templateCache[templateName] = content;
            
            return content;
        }
        
        /// <summary>
        /// 保存模板
        /// </summary>
        public void SaveTemplate(string templateName, string content)
        {
            EnsureTemplateDirectoryExists();
            
            // 构建模板文件路径
            string templatePath = Path.Combine(_templateDirectory, $"{templateName}.svg");
            
            // 保存模板内容
            File.WriteAllText(templatePath, content);
            
            // 更新缓存
            _templateCache[templateName] = content;
        }
        
        /// <summary>
        /// 删除模板
        /// </summary>
        public bool DeleteTemplate(string templateName)
        {
            string templatePath = Path.Combine(_templateDirectory, $"{templateName}.svg");
            
            if (File.Exists(templatePath))
            {
                File.Delete(templatePath);
                _templateCache.Remove(templateName);
                return true;
            }
            
            return false;
        }
        
        /// <summary>
        /// 创建默认模板（如果没有模板）
        /// </summary>
        public void CreateDefaultTemplatesIfNeeded()
        {
            if (!GetAvailableTemplates().Any())
            {
                CreateDefaultTemplates();
            }
        }
        
        /// <summary>
        /// 确保模板目录存在
        /// </summary>
        private void EnsureTemplateDirectoryExists()
        {
            if (!Directory.Exists(_templateDirectory))
            {
                Directory.CreateDirectory(_templateDirectory);
            }
        }
        
        /// <summary>
        /// 创建默认模板
        /// </summary>
        private void CreateDefaultTemplates()
        {
            // 标准朋友圈模板
            string standardTemplate = @"<svg width=""1080"" height=""1920"" viewBox=""0 0 1080 1920"">
    <!-- 朋友圈背景 -->
    <rect width=""1080"" height=""1920"" fill=""#f8f8f8""/>
    
    <!-- 状态栏 -->
    <g transform=""translate(0,0)"">
        <rect width=""1080"" height=""80"" fill=""#333333""/>
        <text x=""540"" y=""50"" font-family=""Arial"" font-size=""36"" fill=""white"" text-anchor=""middle"">{{time}}</text>
        <text x=""980"" y=""50"" font-family=""Arial"" font-size=""32"" fill""white"" text-anchor=""end"">{{battery}}</text>
    </g>
    
    <!-- 导航栏 -->
    <g transform=""translate(0,80)"">
        <rect width=""1080"" height=""100"" fill=""white""/>
        <text x=""540"" y=""60"" font-family=""Arial"" font-size=""46"" fill""#333333"" text-anchor=""middle"" font-weight=""bold"">朋友圈</text>
        <text x""100"" y=""60"" font-family=""Arial"" font-size=""36"" fill""#333333"">返回</text>
    </g>
    
    <!-- 内容区 -->
    <g transform""translate(0,300)"">
        <!-- 用户信息 -->
        <circle cx""100"" cy""100"" r""60"" fill""#dddddd""/>
        <text x""180"" y""80"" font-family""Arial"" font-size""42"" fill""#576B95"" font-weight""bold"">{{userName}}</text>
        
        <!-- 文本内容 -->
        <text x""180"" y""150"" font-family""Arial"" font-size""36"" fill""#333333"" width""860"">{{content}}</text>
        
        <!-- 图片区域 -->
        {{#if hasImages}}
        <g transform""translate(180, 180)"">
            <rect width""200"" height""200"" fill""#eeeeee"" rx""8"" ry""8""/>
            <rect x""220"" width""200"" height""200"" fill""#eeeeee"" rx""8"" ry""8""/>
            <rect x""440"" width""200"" height""200"" fill""#eeeeee"" rx""8"" ry""8""/>
        </g>
        {{/if}}
        
        <!-- 时间和互动 -->
        <text x""180"" y""430"" font-family""Arial"" font-size""32"" fill""#999999"">{{timeText}}</text>
        <text x""900"" y""430"" font-family""Arial"" font-size""32"" fill""#576B95"" text-anchor""end"">赞 {{likes}}</text>
    </g>
</svg>";
            
            SaveTemplate("Standard", standardTemplate);
            
            // 简约风格模板
            string minimalisticTemplate = @"<svg width=""1080"" height=""1920"" viewBox=""0 0 1080 1920"">
    <!-- 简约风格背景 -->
    <rect width=""1080"" height""1920"" fill""white""/>
    
    <!-- 状态栏 - 简约风格 -->
    <g transform""translate(0,0)"">
        <rect width""1080"" height""60"" fill""#fafafa""/>
        <text x""540"" y""40"" font-family""Arial"" font-size""32"" fill""#333333"" text-anchor""middle"">{{time}}</text>
    </g>
    
    <!-- 导航栏 - 简约风格 -->
    <g transform""translate(0,60)"">
        <rect width""1080"" height""80"" fill""white"" stroke""#eeeeee"" stroke-width""1""/>
        <text x""540"" y""50"" font-family""Arial"" font-size""36"" fill""#333333"" text-anchor""middle"">朋友圈</text>
    </g>
    
    <!-- 内容区 - 简约风格 -->
    <g transform""translate(0,250)"">
        <!-- 用户信息 -->
        <text x""100"" y""50"" font-family""Arial"" font-size""38"" fill""#333333"" font-weight""bold"">{{userName}}</text>
        
        <!-- 文本内容 -->
        <text x""100"" y""120"" font-family""Arial"" font-size""32"" fill""#333333"" width""880"">{{content}}</text>
        
        <!-- 图片区域 - 简约风格 -->
        {{#if hasImages}}
        <g transform""translate(100, 160)"">
            <rect width""300"" height""300"" fill""#f5f5f5"" rx""4"" ry""4""/>
        </g>
        {{/if}}
        
        <!-- 时间和互动 - 简约风格 -->
        <text x""100"" y""500"" font-family""Arial"" font-size""28"" fill""#999999"">{{timeText}}</text>
        <text x""980"" y""500"" font-family""Arial"" font-size""28"" fill""#999999"" text-anchor""end"">♥ {{likes}}</text>
    </g>
</svg>";
            
            SaveTemplate("Minimalistic", minimalisticTemplate);
            
            // 黑暗模式模板
            string darkModeTemplate = @"<svg width=""1080"" height=""1920"" viewBox=""0 0 1080 1920"">
    <!-- 黑暗模式背景 -->
    <rect width=""1080"" height""1920"" fill""#121212""/>
    
    <!-- 状态栏 - 黑暗模式 -->
    <g transform""translate(0,0)"">
        <rect width""1080"" height""80"" fill""#000000""/>
        <text x""540"" y""50"" font-family""Arial"" font-size""36"" fill""white"" text-anchor""middle"">{{time}}</text>
        <text x""980"" y""50"" font-family""Arial"" font-size""32"" fill""white"" text-anchor""end"">{{battery}}</text>
    </g>
    
    <!-- 导航栏 - 黑暗模式 -->
    <g transform""translate(0,80)"">
        <rect width""1080"" height""100"" fill""#1e1e1e""/>
        <text x""540"" y""60"" font-family""Arial"" font-size""46"" fill""white"" text-anchor""middle"" font-weight""bold"">朋友圈</text>
        <text x""100"" y""60"" font-family""Arial"" font-size""36"" fill""white"">返回</text>
    </g>
    
    <!-- 内容区 - 黑暗模式 -->
    <g transform""translate(0,300)"">
        <!-- 用户信息 -->
        <circle cx""100"" cy""100"" r""60"" fill""#333333""/>
        <text x""180"" y""80"" font-family""Arial"" font-size""42"" fill""#8ab4f8"" font-weight""bold"">{{userName}}</text>
        
        <!-- 文本内容 -->
        <text x""180"" y""150"" font-family""Arial"" font-size""36"" fill""#e8eaed"" width""860"">{{content}}</text>
        
        <!-- 图片区域 - 黑暗模式 -->
        {{#if hasImages}}
        <g transform""translate(180, 180)"">
            <rect width""200"" height""200"" fill""#333333"" rx""8"" ry""8""/>
            <rect x""220"" width""200"" height""200"" fill""#333333"" rx""8"" ry""8""/>
            <rect x""440"" width""200"" height""200"" fill""#333333"" rx""8"" ry""8""/>
        </g>
        {{/if}}
        
        <!-- 时间和互动 - 黑暗模式 -->
        <text x""180"" y""430"" font-family""Arial"" font-size""32"" fill""#9aa0a6"">{{timeText}}</text>
        <text x""900"" y""430"" font-family""Arial"" font-size""32"" fill""#8ab4f8"" text-anchor""end"">赞 {{likes}}</text>
    </g>
</svg>";
            
            SaveTemplate("DarkMode", darkModeTemplate);
        }
    }
}