using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WeChatMomentSimulator.Core.Logging;
using WeChatMomentSimulator.Desktop.ViewModels;
using LoggerExtensions = WeChatMomentSimulator.Core.Logging.LoggerExtensions;

namespace WeChatMomentSimulator.Desktop
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly MainViewModel _viewModel;
        private readonly ILogger<MainWindow> _logger;

        public MainWindow(IServiceProvider serviceProvider)
        {
            // 获取日志记录器
            _logger = LoggerExtensions.GetLogger<MainWindow>();
            _logger.LogDebug("正在初始化主窗口");
            
           
            
            InitializeComponent();
            // 确保视图模型不为空
            // 从服务提供程序获取视图模型
            _viewModel = serviceProvider.GetRequiredService<MainViewModel>();
            
            // 设置数据上下文
            DataContext = _viewModel;
            
            _logger.LogInformation("主窗口已初始化");
            
            // 注册窗口事件
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _logger.LogInformation("主窗口已加载");
            
            try
            {
                // 在此处添加窗口加载后的初始化逻辑
                _logger.LogDebug("正在执行窗口加载后的初始化操作");
                
                // 可以在此触发ViewModel中的初始化命令
                // 例如: _viewModel.InitializeCommand.Execute(null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "窗口加载过程中发生错误");
                MessageBox.Show($"初始化过程中发生错误：{ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            _logger.LogInformation("正在关闭主窗口");
            
            try
            {
                // 在此处添加窗口关闭前的清理逻辑
                _logger.LogDebug("执行窗口关闭前的清理操作");
                
                // 可以在此触发ViewModel中的清理命令
                // 例如: _viewModel.CleanupCommand.Execute(null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "窗口关闭过程中发生错误");
                // 通常不应阻止窗口关闭，所以这里只记录错误不显示消息框
            }
        }

        /// <summary>
        /// 处理未捕获的异常
        /// </summary>
        private void HandleUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var exception = e.ExceptionObject as Exception;
            _logger.LogCritical(exception, "发生未处理的异常: {Message}", exception?.Message ?? "未知错误");
            
            if (!e.IsTerminating)
            {
                MessageBox.Show($"发生错误：{exception?.Message}\n\n应用程序将继续运行，但可能不稳定。", 
                    "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// UI线程未处理异常
        /// </summary>
        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            _logger.LogCritical(e.Exception, "UI线程发生未处理的异常: {Message}", e.Exception.Message);
            
            MessageBox.Show($"发生错误：{e.Exception.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            
            // 标记为已处理，防止应用崩溃
            e.Handled = true;
        }

        private void RefreshPreview_Click(object sender, RoutedEventArgs e)
        {
            throw new NotImplementedException();
        }
    }
}