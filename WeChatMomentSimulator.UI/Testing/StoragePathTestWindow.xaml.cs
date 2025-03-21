using Microsoft.Win32;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Media;
using WeChatMomentSimulator.Core.Interfaces;
using WeChatMomentSimulator.Services;
using WeChatMomentSimulator.UI.Views.Dialogs;
using MessageBox = System.Windows.MessageBox;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace WeChatMomentSimulator.UI.Testing
{
    /// <summary>
    /// 存储路径管理功能测试窗口
    /// </summary>
    public partial class StoragePathTestWindow : Window
    {
        private readonly IPathService _pathService;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger _logger;

        public StoragePathTestWindow(IPathService pathService, IServiceProvider serviceProvider)
        {
            _pathService = pathService ?? throw new ArgumentNullException(nameof(pathService));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _logger = Log.ForContext<StoragePathTestWindow>();
            
            InitializeComponent();
            
            // 初始化日志
            _logger.Information("测试窗口已初始化");
            _logger.Information("当前存储根目录: {RootDirectory}", _pathService.GetRootDirectory());
            
            LogMessage("测试窗口已初始化", LogLevel.Info);
            LogMessage($"当前存储根目录: {_pathService.GetRootDirectory()}", LogLevel.Info);
        }

        #region 测试操作方法

        /// <summary>
        /// 显示当前存储位置
        /// </summary>
        private void ShowCurrentPath_Click(object sender, RoutedEventArgs e)
        {
            _logger.Information("显示当前存储位置");
            try
            {
                var sb = new StringBuilder();
                string rootDir = _pathService.GetRootDirectory();
                string templatesDir = _pathService.GetTemplatesDirectory();
                string thumbnailsDir = _pathService.GetTemplateThumbnailsDirectory();
                string assetsDir = _pathService.GetAssetsDirectory();
                string settingsDir = _pathService.GetSettingsDirectory();
                string exportsDir = _pathService.GetExportsDirectory();

                sb.AppendLine($"存储根目录: {rootDir}");
                sb.AppendLine($"模板目录: {templatesDir}");
                sb.AppendLine($"缩略图目录: {thumbnailsDir}");
                sb.AppendLine($"资源目录: {assetsDir}");
                sb.AppendLine($"设置目录: {settingsDir}");
                sb.AppendLine($"导出目录: {exportsDir}");

                _logger.Information("获取到路径信息: 根目录={RootDir}, 模板目录={TemplatesDir}, 资源目录={AssetsDir}", 
                    rootDir, templatesDir, assetsDir);

                PathInfoTextBox.Text = sb.ToString();
                LogMessage("已显示当前存储位置信息", LogLevel.Info);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "显示当前路径失败");
                LogMessage($"显示当前路径失败: {ex.Message}", LogLevel.Error);
                MessageBox.Show($"操作失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 显示目录结构
        /// </summary>
        private void ShowDirectoryStructure_Click(object sender, RoutedEventArgs e)
        {
            _logger.Information("显示目录结构");
            try
            {
                string rootDir = _pathService.GetRootDirectory();
                _logger.Debug("分析目录: {RootDir}", rootDir);
                if (!Directory.Exists(rootDir))
                {
                    _logger.Warning("根目录不存在: {RootDir}", rootDir);
                    LogMessage($"根目录不存在: {rootDir}", LogLevel.Warning);
                    PathInfoTextBox.Text = "存储根目录尚未创建";
                    return;
                }

                var sb = new StringBuilder();
                sb.AppendLine($"目录结构 [{rootDir}]");
                sb.AppendLine();

                // 递归构建目录树
                BuildDirectoryTree(sb, rootDir, 0);
                int dirCount = CountDirectories(rootDir);
                _logger.Information("目录扫描完成，共 {DirectoryCount} 个目录", dirCount);

                PathInfoTextBox.Text = sb.ToString();
                LogMessage("已显示目录结构", LogLevel.Info);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "显示目录结构失败");
                LogMessage($"显示目录结构失败: {ex.Message}", LogLevel.Error);
                MessageBox.Show($"操作失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 打开默认目录
        /// </summary>
        private void OpenDefaultDirectory_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string rootDir = _pathService.GetRootDirectory();
                if (!Directory.Exists(rootDir))
                {
                    _pathService.EnsureAllDirectoriesExist();
                    LogMessage($"已创建根目录: {rootDir}", LogLevel.Info);
                }

                Process.Start("explorer.exe", rootDir);
                LogMessage($"已打开文件夹: {rootDir}", LogLevel.Info);
            }
            catch (Exception ex)
            {
                LogMessage($"打开目录失败: {ex.Message}", LogLevel.Error);
                MessageBox.Show($"操作失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 测试更改存储位置
        /// </summary>
        private void ChangeStorageLocation_Click(object sender, RoutedEventArgs e)
        {
            _logger.Information("尝试更改存储位置");
            try
            {
                using var dialog = new FolderBrowserDialog
                {
                    Description = "选择新的存储位置",
                    UseDescriptionForTitle = true,
                    SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                };

                _logger.Debug("显示文件夹选择对话框");
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string oldPath = _pathService.GetRootDirectory();
                    string newPath = Path.Combine(dialog.SelectedPath, "WeChatMomentSimulator");
                    
                    _logger.Information("用户选择了新路径: {NewPath}, 旧路径: {OldPath}", newPath, oldPath);

                    bool moveFiles = MessageBox.Show(
                        $"是否将现有文件从\n{oldPath}\n移动到新位置?\n{newPath}",
                        "移动文件?",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question) == MessageBoxResult.Yes;

                    _logger.Information("用户选择{MoveAction}文件", moveFiles ? "移动" : "不移动");
                    bool result = _pathService.SetRootDirectory(newPath, moveFiles);

                    if (result)
                    {
                        _logger.Information("成功更改存储位置到 {NewPath}, 移动文件: {MoveFiles}", newPath, moveFiles);
                        LogMessage($"已更改存储位置: {newPath}，移动文件: {moveFiles}", LogLevel.Success);
                        ShowCurrentPath_Click(sender, e); // 更新路径显示
                    }
                    else
                    {
                        _logger.Error("更改存储位置失败");
                        LogMessage("更改存储位置失败", LogLevel.Error);
                    }
                }
                else
                {
                    _logger.Information("用户取消了路径选择");
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "更改存储位置时发生错误");
                LogMessage($"更改存储位置失败: {ex.Message}", LogLevel.Error);
                MessageBox.Show($"操作失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 重置到默认位置
        /// </summary>
        private void ResetToDefault_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string currentPath = _pathService.GetRootDirectory();
                string defaultPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "WeChatMomentSimulator");

                if (currentPath.Equals(defaultPath, StringComparison.OrdinalIgnoreCase))
                {
                    LogMessage("当前已经是默认位置", LogLevel.Warning);
                    return;
                }

                bool moveFiles = MessageBox.Show(
                    $"是否将现有文件从\n{currentPath}\n移动到默认位置?\n{defaultPath}",
                    "移动文件?",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question) == MessageBoxResult.Yes;

                bool result = _pathService.SetRootDirectory(defaultPath, moveFiles);

                if (result)
                {
                    LogMessage($"已重置到默认位置: {defaultPath}，移动文件: {moveFiles}", LogLevel.Success);
                    ShowCurrentPath_Click(sender, e); // 更新路径显示
                }
                else
                {
                    LogMessage("重置到默认位置失败", LogLevel.Error);
                }
            }
            catch (Exception ex)
            {
                LogMessage($"重置到默认位置失败: {ex.Message}", LogLevel.Error);
                MessageBox.Show($"操作失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 模拟只读文件夹错误
        /// </summary>
        private void SimulateReadOnlyError_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 创建一个临时的只读文件夹
                string tempFolder = Path.Combine(Path.GetTempPath(), "ReadOnlyTestFolder");
                if (Directory.Exists(tempFolder))
                {
                    Directory.Delete(tempFolder, true);
                }

                Directory.CreateDirectory(tempFolder);

                // 获取目录信息
                DirectoryInfo dirInfo = new DirectoryInfo(tempFolder);
                
                // 尝试设置为只读
                try
                {
                    dirInfo.Attributes |= FileAttributes.ReadOnly;
                    LogMessage($"已创建只读文件夹: {tempFolder}", LogLevel.Info);
                }
                catch
                {
                    LogMessage("无法设置只读属性，将模拟只读文件夹", LogLevel.Warning);
                }

                // 尝试设置为存储路径
                try
                {
                    bool result = _pathService.SetRootDirectory(tempFolder, false);
                    if (result)
                    {
                        LogMessage("意外成功设置了只读文件夹，检查应用权限控制", LogLevel.Warning);
                    }
                    else
                    {
                        LogMessage("设置只读文件夹失败，符合预期", LogLevel.Success);
                    }
                }
                catch (Exception ex)
                {
                    LogMessage($"设置只读文件夹时发生异常（符合预期）: {ex.Message}", LogLevel.Success);
                }
                finally
                {
                    // 清理
                    try
                    {
                        dirInfo.Attributes &= ~FileAttributes.ReadOnly;
                        Directory.Delete(tempFolder, true);
                        LogMessage("测试完成，已清理临时文件夹", LogLevel.Info);
                    }
                    catch
                    {
                        LogMessage("无法清理临时文件夹，可能需要手动删除", LogLevel.Warning);
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage($"测试只读文件夹失败: {ex.Message}", LogLevel.Error);
                MessageBox.Show($"操作失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 模拟首次运行对话框
        /// </summary>
        private void SimulateFirstRunDialog_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 创建并显示首次运行对话框
                var dialog = new FirstRunStorageDialog(_pathService);
                bool? result = dialog.ShowDialog();

                if (result == true)
                {
                    LogMessage($"首次运行对话框确认，路径: {_pathService.GetRootDirectory()}", LogLevel.Success);
                    LogMessage($"不再显示选项: {dialog.DoNotShowAgain}", LogLevel.Info);
                    ShowCurrentPath_Click(sender, e);
                }
                else
                {
                    LogMessage("首次运行对话框取消", LogLevel.Info);
                }
            }
            catch (Exception ex)
            {
                LogMessage($"显示首次运行对话框失败: {ex.Message}", LogLevel.Error);
                MessageBox.Show($"操作失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 创建测试文件
        /// </summary>
        private void CreateTestFiles_Click(object sender, RoutedEventArgs e)
        {
            _logger.Information("开始创建测试文件");
            try
            {
                // 首先确保所有必要的目录都已创建
                _logger.Debug("确保所有必要的目录已创建");
                _pathService.EnsureAllDirectoriesExist();
                
                // 创建各种类型的测试文件
                _logger.Debug("创建测试模板");
                string template1Id = CreateTestTemplate("测试模板1");
                string template2Id = CreateTestTemplate("测试模板2");
                _logger.Debug("已创建测试模板，ID：{Template1Id}，{Template2Id}", template1Id, template2Id);

                // 创建一些测试资源
                string avatarsDir = _pathService.GetAvatarsDirectory();
                string bgDir = _pathService.GetBackgroundsDirectory();

                _logger.Information("创建测试头像到 {AvatarsDir}", avatarsDir);
                CreateTestImage(Path.Combine(avatarsDir, "test_avatar1.png"), 100, 100);
                CreateTestImage(Path.Combine(avatarsDir, "test_avatar2.png"), 100, 100);
                
                _logger.Information("创建测试背景到 {BackgroundsDir}", bgDir);
                CreateTestImage(Path.Combine(bgDir, "test_bg1.jpg"), 300, 200);

                // 创建测试设置
                string settingsFile = _pathService.GetSettingsFilePath();
                File.WriteAllText(settingsFile, "{ \"testSetting\": \"value\" }");
                _logger.Debug("已创建测试设置文件 {SettingsFile}", settingsFile);

                _logger.Information("所有测试文件创建完成");
                LogMessage("已创建测试文件", LogLevel.Success);
                ShowDirectoryStructure_Click(sender, e);
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "创建测试文件失败");
                LogMessage($"创建测试文件失败: {ex.Message}", LogLevel.Error);
                MessageBox.Show($"操作失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 移动位置并保留文件
        /// </summary>
        private void MoveLocationWithFiles_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using var dialog = new FolderBrowserDialog
                {
                    Description = "选择新的存储位置（将移动文件）",
                    UseDescriptionForTitle = true,
                    SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                };

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string oldPath = _pathService.GetRootDirectory();
                    string newPath = Path.Combine(dialog.SelectedPath, "WeChatMomentSimulator_Moved");

                    // 获取移动前的文件列表
                    var filesBefore = GetFileCount(oldPath);
                    LogMessage($"移动前文件数: {filesBefore}", LogLevel.Info);

                    // 移动文件
                    bool result = _pathService.SetRootDirectory(newPath, true);

                    if (result)
                    {
                        // 获取移动后的文件列表
                        var filesAfter = GetFileCount(newPath);
                        LogMessage($"移动后文件数: {filesAfter}", LogLevel.Info);

                        if (filesBefore == filesAfter)
                        {
                            LogMessage($"文件已成功移动到: {newPath}", LogLevel.Success);
                        }
                        else
                        {
                            LogMessage($"文件可能未完全移动。移动前: {filesBefore}, 移动后: {filesAfter}", LogLevel.Warning);
                        }

                        // 检查原位置是否仍有文件
                        if (Directory.Exists(oldPath))
                        {
                            int remainingFiles = GetFileCount(oldPath);
                            if (remainingFiles > 0)
                            {
                                LogMessage($"原位置仍有 {remainingFiles} 个文件", LogLevel.Warning);
                            }
                            else
                            {
                                LogMessage("原位置已无文件", LogLevel.Success);
                            }
                        }

                        ShowCurrentPath_Click(sender, e);
                    }
                    else
                    {
                        LogMessage("移动文件失败", LogLevel.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage($"移动文件失败: {ex.Message}", LogLevel.Error);
                MessageBox.Show($"操作失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 移动位置不保留文件
        /// </summary>
        private void MoveLocationWithoutFiles_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using var dialog = new FolderBrowserDialog
                {
                    Description = "选择新的存储位置（不移动文件）",
                    UseDescriptionForTitle = true,
                    SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                };

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string oldPath = _pathService.GetRootDirectory();
                    string newPath = Path.Combine(dialog.SelectedPath, "WeChatMomentSimulator_NoMove");

                    // 获取移动前的文件列表
                    int filesBefore = GetFileCount(oldPath);
                    LogMessage($"移动前原位置文件数: {filesBefore}", LogLevel.Info);

                    // 不移动文件
                    bool result = _pathService.SetRootDirectory(newPath, false);

                    if (result)
                    {
                        // 获取移动后的新位置文件列表
                        int filesAfterNew = GetFileCount(newPath);
                        LogMessage($"移动后新位置文件数: {filesAfterNew}", LogLevel.Info);

                        // 检查原位置是否仍有文件
                        if (Directory.Exists(oldPath))
                        {
                            int filesAfterOld = GetFileCount(oldPath);
                            LogMessage($"移动后原位置文件数: {filesAfterOld}", LogLevel.Info);

                            if (filesAfterOld == filesBefore)
                            {
                                LogMessage("原始文件保留在原位置，符合预期", LogLevel.Success);
                            }
                            else
                            {
                                LogMessage($"原位置文件数变化。移动前: {filesBefore}, 移动后: {filesAfterOld}", LogLevel.Warning);
                            }
                        }

                        ShowCurrentPath_Click(sender, e);
                    }
                    else
                    {
                        LogMessage("更改存储位置失败", LogLevel.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage($"更改存储位置失败: {ex.Message}", LogLevel.Error);
                MessageBox.Show($"操作失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 显示存储统计
        /// </summary>
        private void ShowStorageStats_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string rootDir = _pathService.GetRootDirectory();
                if (!Directory.Exists(rootDir))
                {
                    PathInfoTextBox.Text = "存储根目录尚未创建";
                    return;
                }

                var sb = new StringBuilder();
                sb.AppendLine("存储统计信息:");
                sb.AppendLine();

                // 统计总体大小
                long totalSize = GetDirectorySize(rootDir);
                sb.AppendLine($"总存储大小: {FormatFileSize(totalSize)}");
                sb.AppendLine();

                // 统计各子目录
                string[] subDirs = Directory.GetDirectories(rootDir);
                foreach (string dir in subDirs)
                {
                    DirectoryInfo dirInfo = new DirectoryInfo(dir);
                    long size = GetDirectorySize(dir);
                    sb.AppendLine($"{dirInfo.Name}: {FormatFileSize(size)}");

                    // 如果是模板目录，额外显示模板数量
                    if (dir == _pathService.GetTemplatesDirectory())
                    {
                        int templateCount = Directory.GetFiles(dir, "*.json").Length;
                        sb.AppendLine($" - 模板数量: {templateCount}");
                    }
                    // 如果是资源目录，额外显示子类型
                    else if (dir == _pathService.GetAssetsDirectory())
                    {
                        string[] assetSubDirs = Directory.GetDirectories(dir);
                        foreach (string assetDir in assetSubDirs)
                        {
                            DirectoryInfo assetDirInfo = new DirectoryInfo(assetDir);
                            int fileCount = Directory.GetFiles(assetDir).Length;
                            sb.AppendLine($" - {assetDirInfo.Name}: {fileCount} 文件");
                        }
                    }
                }

                PathInfoTextBox.Text = sb.ToString();
                LogMessage("已显示存储统计信息", LogLevel.Info);
            }
            catch (Exception ex)
            {
                LogMessage($"获取存储统计失败: {ex.Message}", LogLevel.Error);
                MessageBox.Show($"操作失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 清理所有测试数据
        /// </summary>
        private void CleanupAllData_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MessageBoxResult result = MessageBox.Show(
                    "确定要清理所有测试数据吗？此操作将删除所有模板和资源文件！",
                    "警告",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result == MessageBoxResult.Yes)
                {
                    string rootDir = _pathService.GetRootDirectory();
                    
                    if (Directory.Exists(rootDir))
                    {
                        // 清空所有子目录
                        string[] dirs = Directory.GetDirectories(rootDir);
                        foreach (string dir in dirs)
                        {
                            try
                            {
                                Directory.Delete(dir, true);
                            }
                            catch (Exception ex)
                            {
                                LogMessage($"无法删除目录 {dir}: {ex.Message}", LogLevel.Warning);
                            }
                        }

                        // 删除根目录下的文件
                        string[] files = Directory.GetFiles(rootDir);
                        foreach (string file in files)
                        {
                            try
                            {
                                File.Delete(file);
                            }
                            catch (Exception ex)
                            {
                                LogMessage($"无法删除文件 {file}: {ex.Message}", LogLevel.Warning);
                            }
                        }

                        // 重新创建必要的目录结构
                        _pathService.EnsureAllDirectoriesExist();

                        LogMessage("已清理所有测试数据", LogLevel.Success);
                        ShowDirectoryStructure_Click(sender, e);
                    }
                    else
                    {
                        LogMessage("存储目录不存在，无需清理", LogLevel.Info);
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage($"清理测试数据失败: {ex.Message}", LogLevel.Error);
                MessageBox.Show($"操作失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 递归构建目录树
        /// </summary>
        private void BuildDirectoryTree(StringBuilder sb, string path, int level)
        {
            string indent = new string(' ', level * 2);
            DirectoryInfo dir = new DirectoryInfo(path);
            
            sb.AppendLine($"{indent}+ {dir.Name}");

            // 列出子目录
            foreach (var subDir in dir.GetDirectories())
            {
                BuildDirectoryTree(sb, subDir.FullName, level + 1);
            }

            // 列出文件
            foreach (var file in dir.GetFiles())
            {
                string fileIndent = new string(' ', (level + 1) * 2);
                sb.AppendLine($"{fileIndent}- {file.Name} ({FormatFileSize(file.Length)})");
            }
        }

        /// <summary>
        /// 格式化文件大小
        /// </summary>
        private string FormatFileSize(long bytes)
        {
            string[] suffixes = { "B", "KB", "MB", "GB", "TB" };
            int counter = 0;
            decimal number = bytes;
            while (Math.Round(number / 1024) >= 1)
            {
                number = number / 1024;
                counter++;
            }
            return $"{Math.Round(number, 2)} {suffixes[counter]}";
        }

        /// <summary>
        /// 获取目录大小
        /// </summary>
        private long GetDirectorySize(string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            return GetDirectorySize(dir);
        }

        /// <summary>
        /// 获取目录大小
        /// </summary>
        private long GetDirectorySize(DirectoryInfo dir)
        {
            long size = 0;
            
            // 计算所有文件的大小
            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                size += file.Length;
            }
            
            // 递归计算子目录的大小
            DirectoryInfo[] subDirs = dir.GetDirectories();
            foreach (DirectoryInfo subDir in subDirs)
            {
                size += GetDirectorySize(subDir);
            }
            
            return size;
        }

        /// <summary>
        /// 获取目录中文件数量
        /// </summary>
        private int GetFileCount(string path)
        {
            if (!Directory.Exists(path))
                return 0;

            int count = Directory.GetFiles(path, "*", SearchOption.AllDirectories).Length;
            return count;
        }

        /// <summary>
        /// 统计目录中的子目录数量
        /// </summary>
        private int CountDirectories(string path)
        {
            if (!Directory.Exists(path))
                return 0;

            int count = 1; // 包括自身
            string[] subDirs = Directory.GetDirectories(path);
            
            foreach (string dir in subDirs)
            {
                count += CountDirectories(dir);
            }
            
            return count;
        }

        /// <summary>
        /// 创建测试模板文件
        /// </summary>
        private string CreateTestTemplate(string name)
        {
            string templateId = Guid.NewGuid().ToString();
            string templatePath = _pathService.GetTemplateFilePath(templateId);
            string thumbnailPath = _pathService.GetTemplateThumbnailPath(templateId);
            
            _logger.Debug("创建测试模板 '{Name}' (ID: {TemplateId})", name, templateId);
            _logger.Debug("模板路径: {TemplatePath}, 缩略图路径: {ThumbnailPath}", templatePath, thumbnailPath);
            
            try
            {
                // 确保目录存在
                string templateDir = Path.GetDirectoryName(templatePath);
                string thumbnailDir = Path.GetDirectoryName(thumbnailPath);
                
                if (string.IsNullOrEmpty(templateDir) || string.IsNullOrEmpty(thumbnailDir))
                {
                    _logger.Error("路径无效: 模板路径={TemplatePath}, 缩略图路径={ThumbnailPath}", 
                        templatePath, thumbnailPath);
                    throw new InvalidOperationException("模板或缩略图路径无效");
                }
                
                _logger.Debug("确保目录存在: {TemplateDir}, {ThumbnailDir}", templateDir, thumbnailDir);
                
                if (!Directory.Exists(templateDir))
                {
                    _logger.Debug("创建模板目录: {TemplateDir}", templateDir);
                    Directory.CreateDirectory(templateDir);
                }
                
                if (!Directory.Exists(thumbnailDir))
                {
                    _logger.Debug("创建缩略图目录: {ThumbnailDir}", thumbnailDir);
                    Directory.CreateDirectory(thumbnailDir);
                }
                
                // 创建测试JSON
                string testJson = $@"{{
  ""id"": ""{templateId}"",
  ""version"": ""1.0"",
  ""createdDate"": ""{DateTime.Now:yyyy-MM-ddTHH:mm:ss}"",
  ""modifiedDate"": ""{DateTime.Now:yyyy-MM-ddTHH:mm:ss}"",
  ""metadata"": {{
    ""name"": ""{name}"",
    ""description"": ""测试模板描述"",
    ""author"": ""测试用户"",
    ""category"": ""测试"",
    ""tags"": [""测试"", ""UI测试""]
  }},
  ""placeholders"": [],
  ""thumbnail"": ""{Path.GetFileName(thumbnailPath)}""
}}";

                // 写入文件
                File.WriteAllText(templatePath, testJson);
                _logger.Debug("已写入模板JSON文件: {TemplatePath}", templatePath);
                
                // 创建测试缩略图
                CreateTestImage(thumbnailPath, 200, 200);
                
                LogMessage($"已创建测试模板: {name} (ID: {templateId})", LogLevel.Info);
                return templateId;
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "创建测试模板失败: {Name}", name);
                LogMessage($"创建测试模板 {name} 失败: {ex.Message}", LogLevel.Error);
                throw;
            }
        }

        /// <summary>
        /// 创建测试图像文件
        /// </summary>
        private void CreateTestImage(string path, int width, int height)
        {
            try
            {
                _logger.Debug("创建测试图像: {FileName} ({Width}x{Height})", Path.GetFileName(path), width, height);
                
                string directory = Path.GetDirectoryName(path);
                if (string.IsNullOrEmpty(directory))
                {
                    _logger.Error("图像路径无效: {Path}", path);
                    throw new InvalidOperationException($"图像路径无效: {path}");
                }
                
                _logger.Debug("确保目录存在: {Directory}", directory);
                if (!Directory.Exists(directory))
                {
                    _logger.Debug("创建目录: {Directory}", directory);
                    Directory.CreateDirectory(directory);
                }
                
                // 检查目录是否成功创建
                if (!Directory.Exists(directory))
                {
                    _logger.Error("无法创建目录: {Directory}", directory);
                    throw new DirectoryNotFoundException($"无法创建目录: {directory}");
                }
                
                // 创建测试图像
                using (var bmp = new System.Drawing.Bitmap(width, height))
                {
                    using (var g = System.Drawing.Graphics.FromImage(bmp))
                    {
                        // 填充背景
                        g.FillRectangle(System.Drawing.Brushes.LightGray, 0, 0, width, height);
                        
                        // 添加文字
                        string text = Path.GetFileNameWithoutExtension(path);
                        var font = new System.Drawing.Font("Arial", 12);
                        g.DrawString(text, font, System.Drawing.Brushes.Black, 10, 10);
                        
                        // 添加边框
                        g.DrawRectangle(System.Drawing.Pens.Black, 0, 0, width - 1, height - 1);
                    }
                    
                    // 保存图像
                    bmp.Save(path);
                    _logger.Debug("已保存测试图像: {Path}", path);
                }
            }
            catch (Exception ex)
            {
                _logger.Error(ex, "创建测试图像失败: {Path}", path);
                LogMessage($"创建测试图像失败: {ex.Message}", LogLevel.Error);
                throw;
            }
        }

        /// <summary>
        /// 日志级别枚举
        /// </summary>
        private enum LogLevel
        {
            Info,
            Warning,
            Error,
            Success
        }

        /// <summary>
        /// 输出日志消息到UI并同时记录到Serilog
        /// </summary>
        private void LogMessage(string message, LogLevel level)
        {
            // 同时记录到Serilog
            switch (level)
            {
                case LogLevel.Info:
                    _logger.Information(message);
                    break;
                case LogLevel.Warning:
                    _logger.Warning(message);
                    break;
                case LogLevel.Error:
                    _logger.Error(message);
                    break;
                case LogLevel.Success:
                    _logger.Information("[成功] " + message);
                    break;
            }
            
            // 更新UI
            Dispatcher.Invoke(() =>
            {
                // 创建新段落
                var paragraph = new Paragraph();
                
                // 添加时间戳
                var timeRun = new Run($"[{DateTime.Now:HH:mm:ss}] ")
                {
                    Foreground = Brushes.Gray
                };
                paragraph.Inlines.Add(timeRun);
                
                // 添加级别标记
                var levelRun = new Run();
                switch (level)
                {
                    case LogLevel.Info:
                        levelRun.Text = "[信息] ";
                        levelRun.Foreground = Brushes.Blue;
                        break;
                    case LogLevel.Warning:
                        levelRun.Text = "[警告] ";
                        levelRun.Foreground = Brushes.Orange;
                        break;
                    case LogLevel.Error:
                        levelRun.Text = "[错误] ";
                        levelRun.Foreground = Brushes.Red;
                        break;
                    case LogLevel.Success:
                        levelRun.Text = "[成功] ";
                        levelRun.Foreground = Brushes.Green;
                        break;
                }
                paragraph.Inlines.Add(levelRun);
                
                // 添加消息内容
                paragraph.Inlines.Add(new Run(message));
                
                // 添加到日志框
                LogTextBox.Document.Blocks.Add(paragraph);
                
                // 滚动到最底部
                LogTextBox.ScrollToEnd();
                
                // 更新状态栏
                StatusTextBlock.Text = message;
            });
        }

        #endregion
    }
}