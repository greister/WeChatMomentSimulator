using System.IO;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using WeChatMomentSimulator.Core.Interfaces;
using WeChatMomentSimulator.Services.Infrastructure;
using WeChatMomentSimulator.UI.ViewModels;


namespace WeChatMomentSimulator.UI
{
    public partial class App : Application
    {
        private ServiceProvider _serviceProvider;
        private bool _isDevMode;
        
        // 引入 Windows API 函数
        [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool AllocConsole();

        [System.Runtime.InteropServices.DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FreeConsole();
 

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            try
            {
                // 重定向标准输出流到控制台
                var standardOutput = new System.IO.StreamWriter(Console.OpenStandardOutput())
                {
                    AutoFlush = true
                };
                Console.SetOut(standardOutput);
                
                Console.WriteLine("===================================");
                Console.WriteLine("应用程序正在启动...");
                Console.WriteLine("===================================");
                // 1. 简单的环境检测
             
                   // 1. 简单的环境检测
                _isDevMode = File.Exists("development.flag");
                Console.WriteLine($"环境模式: {(_isDevMode ? "开发" : "生产")}");
                
                // 2. 初始化Serilog (全局配置)
                SerilogLogger.Configure(_isDevMode);
                
                // 3. 配置依赖注入
                var services = new ServiceCollection();
                
                // 注册日志服务
                services.AddSingleton<IAppLogger, SerilogLogger>();
                
                // 注册ViewModel
                services.AddSingleton<MainViewModel>(provider => 
                    new MainViewModel(provider.GetRequiredService<IAppLogger>(), _isDevMode));
                
                // 注册主窗口
                services.AddSingleton<MainWindow>(provider =>
                    new MainWindow { DataContext = provider.GetRequiredService<MainViewModel>() });
                
                _serviceProvider = services.BuildServiceProvider();
                
                // 4. 获取日志服务实例
                var logger = _serviceProvider.GetRequiredService<IAppLogger>();
                logger.Info("应用启动中");
                
                // 5. 显示主窗口
                var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
                mainWindow.Show();
                
                logger.Info("应用已成功启动");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"应用启动错误: {ex.Message}");
                MessageBox.Show($"应用启动失败: {ex.Message}", "错误", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                var logger = _serviceProvider?.GetService<IAppLogger>();
                logger?.Info("应用正在关闭");
                
              
                
                _serviceProvider?.Dispose();
            }
            catch
            {
                // 忽略退出时的错误
            }
            
            base.OnExit(e);
        }
        
      
    }
    
}