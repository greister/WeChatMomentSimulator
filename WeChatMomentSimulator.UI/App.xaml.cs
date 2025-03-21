using System;
using System.IO;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using WeChatMomentSimulator.Core.Interfaces;
using WeChatMomentSimulator.Services.Infrastructure;
using WeChatMomentSimulator.Services.Repositories;
using WeChatMomentSimulator.Services.Storage;
using WeChatMomentSimulator.UI.Settings;
using WeChatMomentSimulator.UI.ViewModels;
using WeChatMomentSimulator.UI.Views;

namespace WeChatMomentSimulator.UI
{
    public partial class App : Application
    {
        private IHost _host;
        private LogWindow? _logWindow;
        private static readonly object _logLock = new();

        public App()
        {
            CheckEnvironment();
            _host = CreateHostBuilder().Build();
            // 在构造函数中初始化日志窗口
            _logWindow = new LogWindow();
            _logWindow.Closed += (s, e) => _logWindow = null; // 窗口关闭时置空
            InitializeServices();
        }

        private void CheckEnvironment()
        {
            // 这里可以添加环境检测的逻辑
        }

        private IHostBuilder CreateHostBuilder()
        {
            return Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration(ConfigureApp)
                .UseSerilog(ConfigureSerilog)
                .ConfigureServices(ConfigureServices);
        }

        private void ConfigureApp(HostBuilderContext hostContext, IConfigurationBuilder config)
        {
            config.SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables();
        }

        private void ConfigureSerilog(HostBuilderContext hostContext, LoggerConfiguration loggerConfiguration)
        {
            loggerConfiguration
                .ReadFrom.Configuration(hostContext.Configuration)
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.WithProperty("InstanceId", Guid.NewGuid().ToString("N")[..8])  // 默认添加实例ID
                .WriteTo.Sink(new LogWindowSink(this))
                .WriteTo.Console()
                .WriteTo.File(Path.Combine("logs", "log-.txt"), rollingInterval: RollingInterval.Day);
        }

        private void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            
            // 添加路径服务 (新增)
            services.AddSingleton<IPathService, PathService>();
            // 注册配置服务

            services.AddTransient<StorageSettingsViewModel>();
            services.AddTransient<StorageSettingsView>(provider =>

                new StorageSettingsView(provider.GetRequiredService<StorageSettingsViewModel>()));


            // 注册核心服务
            services.AddSingleton<FileStorageService>();
            services.AddSingleton<FileTemplateStorage>();
            services.AddSingleton<ITemplateRepository, TemplateRepository>();
            services.AddSingleton<ITemplateStorage, FileTemplateStorage>();
            services.AddSingleton<TemplateManager>();

            // 注册视图模型
            services.AddSingleton<MainViewModel>();

            // 注册主窗口 - 修正以下代码
            services.AddSingleton<MainWindow>(provider =>
                new MainWindow(
                    provider, // 提供 IServiceProvider
                    provider.GetRequiredService<Serilog.ILogger>() // 提供 ILogger
                )
                {
                    // 设置 DataContext 为 MainViewModel
                    DataContext = provider.GetRequiredService<MainViewModel>()
                }
            );

            // 添加日志支持
            services.AddLogging(configure => { configure.AddSerilog(); });
        }

        private void InitializeServices()
        {
            // 这里可以添加服务初始化的逻辑
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            // 初始化日志窗口
            // 确保日志窗口存在
            if (_logWindow == null)
            {
                _logWindow = new LogWindow();
            }

            ShowMainWindow();
            var logger = _host.Services.GetRequiredService<ILogger<App>>();
            logger.LogInformation("应用界面已启动");

            // 全局异常处理
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                Log.Fatal(e.ExceptionObject as Exception, "未处理的异常");
                ShowLogWindow();
            };

            // 添加快捷键支持
            this.Dispatcher.UnhandledException += (s, e) =>
            {
                Log.Error(e.Exception, "UI线程异常");
                e.Handled = true;
            };

        }

        public void ShowLogWindow()
        {
            _logWindow?.Show();
            _logWindow?.Activate();
        }

        private void ShowMainWindow()
        {
            var mainWindow = _host.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
        }

        protected override void OnExit(ExitEventArgs e)
        {
            try
            {
                var logger = _host.Services.GetRequiredService<ILogger<App>>();
                logger.LogInformation("应用正在关闭");
                Log.CloseAndFlush();
                _host.Dispose();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"应用退出时发生错误: {ex.Message}");
            }
            finally
            {
                base.OnExit(e);
            }
        }

        // 自定义Serilog Sink
        // 修改LogWindowSink
        public class LogWindowSink : Serilog.Core.ILogEventSink
        {
            private readonly App _app;

            public LogWindowSink(App app)
            {
                _app = app;
            }

            public void Emit(LogEvent logEvent)
            {
                var entry = new LogEntry(logEvent);
                _app.Dispatcher.Invoke(() => _app._logWindow?.AppendLog(entry));
            }
        }
    }
}