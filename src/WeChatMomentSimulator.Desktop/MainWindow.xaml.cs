using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using WeChatMomentSimulator.Core.Logging;
using WeChatMomentSimulator.Desktop.Commands;
using WeChatMomentSimulator.Desktop.ViewModels;
using WeChatMomentSimulator.Desktop.Services;
using WeChatMomentSimulator.Core.Interfaces.Services;

namespace WeChatMomentSimulator.Desktop
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ILogger<MainWindow> _logger;
        private readonly MainViewModel _viewModel;
        private readonly IDialogService _dialogService;
        private readonly ISettingsService _settingsService;
        private readonly ITemplateService _templateService;
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// 使用依赖注入的构造函数
        /// </summary>
        /// <param name="serviceProvider">服务提供者</param>
        public MainWindow(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

            // 从服务容器获取服务
            _logger = serviceProvider.GetRequiredService<ILogger<MainWindow>>();
            _viewModel = serviceProvider.GetRequiredService<MainViewModel>();
            _dialogService = serviceProvider.GetRequiredService<IDialogService>();
            _settingsService = serviceProvider.GetRequiredService<ISettingsService>();
            _templateService = serviceProvider.GetRequiredService<ITemplateService>();

            _logger.LogInformation("正在初始化主窗口");

            try
            {
                InitializeComponent();

                // 设置数据上下文
                DataContext = _viewModel;

                // 注册窗口事件
                Loaded += MainWindow_Loaded;
                Closing += MainWindow_Closing;

                // 初始化命令绑定
                InitializeCommands();

                _logger.LogInformation("主窗口初始化完成");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "主��口初始化失败");
                MessageBox.Show($"应用程序初始化失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 初始化命令绑定
        /// </summary>
        private void InitializeCommands()
        {
            using ("Method".LogContext("InitializeCommands"))
            {
                _logger.LogDebug("正在初始化命令绑定");

                try
                {
                    // 文件操作命令
                    CommandBindings.Add(new CommandBinding(
                        ApplicationCommands.New,
                        ExecuteNewProject,
                        CanExecuteStandardCommand));

                    CommandBindings.Add(new CommandBinding(
                        ApplicationCommands.Open,
                        ExecuteOpenProject,
                        CanExecuteStandardCommand));

                    CommandBindings.Add(new CommandBinding(
                        ApplicationCommands.Save,
                        ExecuteSaveProject,
                        CanExecuteSaveProject));

                    CommandBindings.Add(new CommandBinding(
                        ApplicationCommands.SaveAs,
                        ExecuteSaveProjectAs,
                        CanExecuteStandardCommand));

                    CommandBindings.Add(new CommandBinding(
                        ApplicationCommands.Close,
                        ExecuteExit,
                        CanExecuteStandardCommand));

                    // 自定义命令
                    _viewModel.SaveTemplateCommand = new RelayCommand(ExecuteSaveTemplate, CanExecuteSaveTemplate);
                    _viewModel.ExportImageCommand = new RelayCommand(ExecuteExportImage, CanExecuteExportImage);
                    _viewModel.EditTemplateCommand = new RelayCommand(ExecuteEditTemplate, CanExecuteStandardCheck);
                    _viewModel.AdvancedSettingsCommand = new RelayCommand(ExecuteAdvancedSettings, CanExecuteStandardCheck);
                    _viewModel.ShowStatisticsCommand = new RelayCommand(ExecuteShowStatistics, CanExecuteStandardCheck);
                    _viewModel.SaveUserSettingsCommand = new RelayCommand(ExecuteSaveUserSettings, CanExecuteSaveUserSettings);

                    _logger.LogInformation("命令绑定初始化完成");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "命令绑定初始化失败");
                    _viewModel.ErrorMessage = "命令初始化失败，某些功能可能不可用";
                }
            }
        }

        #region 窗口事件处理

        /// <summary>
        /// 窗口加载完成事件处理
        /// </summary>
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            using ("Event".LogContext("MainWindow_Loaded"))
            {
                _logger.LogInformation("窗口加载完成");

                try
                {
                    // 初始化视图模型
                    _viewModel.Initialize();

                    // 加载上次会话设置
                    LoadLastSessionSettings();

                    // 检查命令行参数
                    CheckCommandLineArgs();

                    // 设置状态消息
                    _viewModel.StatusMessage = "就绪";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "窗口加载事件处理失败");
                    _viewModel.ErrorMessage = $"初始化失败: {ex.Message}";
                }
            }
        }

        /// <summary>
        /// 窗��关闭前事件处理
        /// </summary>
        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            using ("Event".LogContext("MainWindow_Closing"))
            {
                _logger.LogInformation("窗口正在关闭");

                try
                {
                    // 检查未保存的更改
                    if (_viewModel.HasUnsavedChanges)
                    {
                        var result = _dialogService.ShowConfirmation(
                            "是否保存更改?",
                            "保存确认",
                            CustomDialogButton.YesNoCancel);

                        switch (result)
                        {
                            case CustomDialogResult.Yes:
                                _logger.LogInformation("用户选择保存更改");
                                SaveChanges();
                                break;

                            case CustomDialogResult.Cancel:
                                _logger.LogInformation("用户取消了关闭操作");
                                e.Cancel = true;
                                return;

                            case CustomDialogResult.No:
                                _logger.LogInformation("用户选择不保存继续关闭");
                                break;
                        }
                    }

                    // 保存会话设置
                    SaveSessionSettings();

                    // 清理资源
                    _viewModel.Cleanup();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "窗口关闭事件处理失败");
                    _dialogService.ShowError("关闭时发生错误", ex.Message);
                }
            }
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 保存当前更改
        /// </summary>
        private void SaveChanges()
        {
            try
            {
                if (string.IsNullOrEmpty(_viewModel.CurrentProjectPath))
                {
                    // 如果是新项目，调用另存为
                    ExecuteSaveProjectAs(this, null);
                }
                else
                {
                    // 保存到已有路径
                    _viewModel.SaveProject();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "保存更改失败");
                _viewModel.ErrorMessage = $"保存失败: {ex.Message}";
            }
        }

        /// <summary>
        /// 加载上次会话设置
        /// </summary>
        private void LoadLastSessionSettings()
        {
            using ("Method".LogContext("LoadLastSessionSettings"))
            {
                try
                {
                    _logger.LogInformation("加载会话设置");

                    // 从设置服务加载数据
                    var settings = _settingsService.LoadSettings();

                    // 窗口位置和大小
                    if (settings.WindowWidth > 0)
                    {
                        Width = settings.WindowWidth;
                        Height = settings.WindowHeight;

                        // 确保窗口在屏幕内
                        EnsureWindowVisibility();
                    }

                    // 最近项目
                    if (!string.IsNullOrEmpty(settings.LastProjectPath) &&
                        File.Exists(settings.LastProjectPath))
                    {
                        _viewModel.LastProjectPath = settings.LastProjectPath;
                    }

                    _logger.LogInformation("会话设置加载完成");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "加载会话设置失败，将使用默认设置");
                }
            }
        }

        /// <summary>
        /// 保存会话设置
        /// </summary>
        private void SaveSessionSettings()
        {
            using ("Method".LogContext("SaveSessionSettings"))
            {
                try
                {
                    _logger.LogInformation("保存会话设置");

                    // 创建设置对象
                    var settings = new ApplicationSettings
                    {
                        WindowWidth = Width,
                        WindowHeight = Height,
                        LastProjectPath = _viewModel.CurrentProjectPath
                    };

                    // 保存设置
                    _settingsService.SaveSettings(settings);

                    _logger.LogInformation("会话设置已保存");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "保存会话设置失败");
                }
            }
        }

        /// <summary>
        /// 确保窗口在屏幕可见区域内
        /// </summary>
        private void EnsureWindowVisibility()
        {
            var screenWidth = SystemParameters.PrimaryScreenWidth;
            var screenHeight = SystemParameters.PrimaryScreenHeight;

            if (Left < 0) Left = 0;
            if (Top < 0) Top = 0;

            if (Left + Width > screenWidth) Left = screenWidth - Width;
            if (Top + Height > screenHeight) Top = screenHeight - Height;
        }

        /// <summary>
        /// 检查命令行参数
        /// </summary>
        private void CheckCommandLineArgs()
        {
            using ("Method".LogContext("CheckCommandLineArgs"))
            {
                try
                {
                    var args = Environment.GetCommandLineArgs();
                    if (args.Length > 1 && File.Exists(args[1]))
                    {
                        string filePath = args[1];
                        _logger.LogInformation("从命令行参数加载文件: {Path}", filePath);
                        _viewModel.OpenProject(filePath);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "处理命令行参数失败");
                }
            }
        }

        #endregion

        #region 命令执行方法

        // 新建项目
        private void ExecuteNewProject(object sender, ExecutedRoutedEventArgs e)
        {
            using ("Command".LogContext("NewProject"))
            {
                _logger.LogInformation("执行新建项目命令");
                try
                {
                    // 检查未保存的更改
                    if (_viewModel.HasUnsavedChanges)
                    {
                        var result = _dialogService.ShowConfirmation(
                            "当前有未保存的更改，是否保存?",
                            "保存确认",
                            CustomDialogButton.YesNoCancel);

                        if (result == CustomDialogResult.Cancel)
                        {
                            return;
                        }

                        if (result == CustomDialogResult.Yes)
                        {
                            SaveChanges();
                        }
                    }

                    // 创建新项目
                    _viewModel.CreateNewProject();
                    _viewModel.StatusMessage = "已创建新项目";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "新建项目失败");
                    _viewModel.ErrorMessage = $"新建项目失败: {ex.Message}";
                }
            }
        }

        // 打开项目
        private void ExecuteOpenProject(object sender, ExecutedRoutedEventArgs e)
        {
            using ("Command".LogContext("OpenProject"))
            {
                _logger.LogInformation("执行打开项目命令");
                try
                {
                    // 处理未保存的更改
                    if (_viewModel.HasUnsavedChanges)
                    {
                        var result = _dialogService.ShowConfirmation(
                            "当前有未保存的更改，是否保存?",
                            "保存确认",
                            CustomDialogButton.YesNoCancel);

                        if (result == CustomDialogResult.Cancel)
                        {
                            return;
                        }

                        if (result == CustomDialogResult.Yes)
                        {
                            SaveChanges();
                        }
                    }

                    // 显示打开文件对话框
                    var filePath = _dialogService.ShowOpenFileDialog(
                        "打开项目",
                        "微信朋友圈模拟器项目 (*.wxsim)|*.wxsim|所有文件 (*.*)|*.*");

                    if (!string.IsNullOrEmpty(filePath))
                    {
                        _viewModel.OpenProject(filePath);
                        _viewModel.StatusMessage = $"已打开: {Path.GetFileName(filePath)}";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "打开项目失败");
                    _viewModel.ErrorMessage = $"打开项目失败: {ex.Message}";
                }
            }
        }

        // 保存项目
        private void ExecuteSaveProject(object sender, ExecutedRoutedEventArgs e)
        {
            using ("Command".LogContext("SaveProject"))
            {
                _logger.LogInformation("执行保存项目命令");
                try
                {
                    if (string.IsNullOrEmpty(_viewModel.CurrentProjectPath))
                    {
                        ExecuteSaveProjectAs(sender, e);
                    }
                    else
                    {
                        _viewModel.SaveProject();
                        _viewModel.StatusMessage = "项目已保存";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "保存项目失败");
                    _viewModel.ErrorMessage = $"保存失败: {ex.Message}";
                }
            }
        }

        // 另存为
        private void ExecuteSaveProjectAs(object sender, ExecutedRoutedEventArgs e)
        {
            using ("Command".LogContext("SaveProjectAs"))
            {
                _logger.LogInformation("执行另存为命令");
                try
                {
                    var filePath = _dialogService.ShowSaveFileDialog(
                        "保存项目",
                        "微信朋友圈模拟器项目 (*.wxsim)|*.wxsim|所有文件 (*.*)|*.*",
                        "wxsim");

                    if (!string.IsNullOrEmpty(filePath))
                    {
                        _viewModel.SaveProjectAs(filePath);
                        _viewModel.StatusMessage = $"已保存: {Path.GetFileName(filePath)}";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "项目另存为失败");
                    _viewModel.ErrorMessage = $"保存失败: {ex.Message}";
                }
            }
        }

        // 退出
        private void ExecuteExit(object sender, ExecutedRoutedEventArgs e)
        {
            using ("Command".LogContext("Exit"))
            {
                _logger.LogInformation("执行退出命令");
                Close();
            }
        }

        // 保存用户设置
        private void ExecuteSaveUserSettings(object parameter)
        {
            using ("Command".LogContext("SaveUserSettings"))
            {
                _logger.LogInformation("执行保存用户设置命令");
                try
                {
                    _viewModel.SaveUserSettings();
                    _viewModel.StatusMessage = "用户设置已保存";
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "保存用户设置失败");
                    _viewModel.ErrorMessage = $"保存用户设置失败: {ex.Message}";
                }
            }
        }

        // 其他命令实现...
        private void ExecuteSaveTemplate(object parameter) { /* 实现保存模板逻辑 */ }
        private void ExecuteExportImage(object parameter) { /* 实现导出图片逻辑 */ }
        private void ExecuteEditTemplate(object parameter) { /* 实现编辑模板逻辑 */ }
        private void ExecuteAdvancedSettings(object parameter) { /* 实现高级设置逻辑 */ }
        private void ExecuteShowStatistics(object parameter) { /* 实现显示统���逻辑 */ }

        #endregion

        #region 命令可执行状态判断

        // 保存项目可执行条件
        private void CanExecuteSaveProject(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !_viewModel.IsLoading && _viewModel.HasUnsavedChanges;
        }

        // 标准命令可执行条件
        private void CanExecuteStandardCommand(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = !_viewModel.IsLoading;
        }

        // 标准检查（用于RelayCommand）
        private bool CanExecuteStandardCheck(object parameter)
        {
            return !_viewModel.IsLoading;
        }

        // 保存模板可执行条件
        private bool CanExecuteSaveTemplate(object parameter)
        {
            return !_viewModel.IsLoading && _viewModel.HasContent;
        }

        // 导出图片可执行条件
        private bool CanExecuteExportImage(object parameter)
        {
            return !_viewModel.IsLoading && _viewModel.HasPreviewContent;
        }

        // 保存用户设置可执行条件
        private bool CanExecuteSaveUserSettings(object parameter)
        {
            return !_viewModel.IsLoading && _viewModel.HasUserSettingsChanged;
        }

        #endregion
    }
}