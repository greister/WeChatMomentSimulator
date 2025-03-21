using System;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using WeChatMomentSimulator.Core.Interfaces;

namespace WeChatMomentSimulator.Services.Infrastructure
{
    /// <summary>
    /// 提供应用程序中所有文件路径的统一管理
    /// </summary>
    public class PathService : IPathService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<PathService> _logger;
        private string _rootDirectory;

        // 子目录常量
        private const string USER_TEMPLATES_DIR = "UserTemplates";
        private const string TEMPLATE_THUMBNAILS_DIR = "Thumbnails";
        private const string ASSETS_DIR = "Assets";
        private const string AVATARS_DIR = "Avatars";
        private const string BACKGROUNDS_DIR = "Backgrounds";
        private const string STATUSBAR_DIR = "StatusBar";
        private const string EXPORTS_DIR = "Exports";
        private const string SETTINGS_DIR = "Settings";

        public PathService(IConfiguration configuration, ILogger<PathService> logger = null)
        {
            _configuration = configuration;
            _logger = logger;
            InitializeRootDirectory();
        }

        /// <summary>
        /// 初始化根目录，从配置读取或创建默认路径
        /// </summary>
        private void InitializeRootDirectory()
        {
            // 从配置读取根目录路径，如果不存在则使用默认路径
            _rootDirectory = _configuration.GetValue<string>("DataStorage:RootDirectory");
            
            if (string.IsNullOrEmpty(_rootDirectory))
            {
                _rootDirectory = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "WeChatMomentSimulator"
                );
                _logger?.LogInformation("未配置根目录，使用默认路径: {Path}", _rootDirectory);
            }

            // 确保根目录存在
            EnsureDirectoryExists(_rootDirectory);
            
            // 确保所有必要子目录存在
            EnsureAllDirectoriesExist();
        }

        /// <summary>
        /// 确保所有必要的子目录结构存在
        /// </summary>
        public void EnsureAllDirectoriesExist()
        {
            EnsureDirectoryExists(GetTemplatesDirectory());
            EnsureDirectoryExists(GetTemplateThumbnailsDirectory());
            EnsureDirectoryExists(GetAssetsDirectory());
            EnsureDirectoryExists(GetAvatarsDirectory());
            EnsureDirectoryExists(GetBackgroundsDirectory());
            EnsureDirectoryExists(GetStatusBarDirectory());
            EnsureDirectoryExists(GetExportsDirectory());
            EnsureDirectoryExists(GetSettingsDirectory());
        }

        /// <summary>
        /// 确保指定目录存在
        /// </summary>
        private void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                try
                {
                    Directory.CreateDirectory(path);
                    _logger?.LogInformation("创建目录: {Path}", path);
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "创建目录失败: {Path}", path);
                    throw;
                }
            }
        }

        /// <summary>
        /// 获取根目录路径
        /// </summary>
        public string GetRootDirectory() => _rootDirectory;

        /// <summary>
        /// 获取模板目录路径
        /// </summary>
        public string GetTemplatesDirectory() => Path.Combine(_rootDirectory, USER_TEMPLATES_DIR);

        /// <summary>
        /// 获取模板缩略图目录路径
        /// </summary>
        public string GetTemplateThumbnailsDirectory() => Path.Combine(GetTemplatesDirectory(), TEMPLATE_THUMBNAILS_DIR);

        /// <summary>
        /// 获取指定ID模板文件的完整路径
        /// </summary>
        public string GetTemplateFilePath(string templateId) => Path.Combine(GetTemplatesDirectory(), $"{templateId}.json");

        /// <summary>
        /// 获取指定ID模板缩略图的完整路径
        /// </summary>
        public string GetTemplateThumbnailPath(string templateId) => Path.Combine(GetTemplateThumbnailsDirectory(), $"{templateId}.png");

        /// <summary>
        /// 获取资源根目录
        /// </summary>
        public string GetAssetsDirectory() => Path.Combine(_rootDirectory, ASSETS_DIR);

        /// <summary>
        /// 获取头像目录
        /// </summary>
        public string GetAvatarsDirectory() => Path.Combine(GetAssetsDirectory(), AVATARS_DIR);

        /// <summary>
        /// 获取背景图片目录
        /// </summary>
        public string GetBackgroundsDirectory() => Path.Combine(GetAssetsDirectory(), BACKGROUNDS_DIR);

        /// <summary>
        /// 获取状态栏资源目录
        /// </summary>
        public string GetStatusBarDirectory() => Path.Combine(GetAssetsDirectory(), STATUSBAR_DIR);

        /// <summary>
        /// 获取导出文件目录
        /// </summary>
        public string GetExportsDirectory() => Path.Combine(_rootDirectory, EXPORTS_DIR);

        /// <summary>
        /// 获取设置文件目录
        /// </summary>
        public string GetSettingsDirectory() => Path.Combine(_rootDirectory, SETTINGS_DIR);

        /// <summary>
        /// 获取设置文件路径
        /// </summary>
        public string GetSettingsFilePath() => Path.Combine(GetSettingsDirectory(), "preferences.json");

        /// <summary>
        /// 设置新的根目录
        /// </summary>
        /// <param name="newPath">新根目录路径</param>
        /// <param name="moveExistingFiles">是否移动现有文件</param>
        /// <returns>操作是否成功</returns>
        public bool SetRootDirectory(string newPath, bool moveExistingFiles = true)
        {
            if (string.IsNullOrEmpty(newPath))
                throw new ArgumentNullException(nameof(newPath));

            try
            {
                string oldRoot = _rootDirectory;
                
                // 如果路径相同，无需更改
                if (Path.GetFullPath(oldRoot).Equals(Path.GetFullPath(newPath), StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                // 创建新目录
                EnsureDirectoryExists(newPath);
                
                // 如果需要移动现有文件
                if (moveExistingFiles && Directory.Exists(oldRoot))
                {
                    MoveDirectoryContents(oldRoot, newPath);
                }

                // 更新根目录路径
                _rootDirectory = newPath;
                
                // 确保所有子目录结构存在
                EnsureAllDirectoriesExist();
                
                _logger?.LogInformation("根目录已更改: {OldPath} -> {NewPath}", oldRoot, newPath);
                return true;
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "更改根目录失败: {Path}", newPath);
                return false;
            }
        }

        /// <summary>
        /// 移动目录内容到新位置
        /// </summary>
        private void MoveDirectoryContents(string sourceDir, string targetDir)
        {
            // 创建目标目录
            Directory.CreateDirectory(targetDir);

            // 移动所有文件
            foreach (var file in Directory.GetFiles(sourceDir))
            {
                string fileName = Path.GetFileName(file);
                string destFile = Path.Combine(targetDir, fileName);
                
                // 如果目标文件已存在，先删除
                if (File.Exists(destFile))
                {
                    File.Delete(destFile);
                }
                
                File.Move(file, destFile);
            }

            // 递归移动所有子目录
            foreach (var directory in Directory.GetDirectories(sourceDir))
            {
                string dirName = Path.GetFileName(directory);
                string destDir = Path.Combine(targetDir, dirName);
                MoveDirectoryContents(directory, destDir);
            }
        }
    }
}