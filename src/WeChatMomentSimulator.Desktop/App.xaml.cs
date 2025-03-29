﻿using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using WeChatMomentSimulator.Core.DataBinding;
using WeChatMomentSimulator.Core.Interfaces;
using WeChatMomentSimulator.Core.Interfaces.DataBingding;
using WeChatMomentSimulator.Core.Interfaces.Repositories;
using WeChatMomentSimulator.Core.Interfaces.Services;
using WeChatMomentSimulator.Core.Logging;
using WeChatMomentSimulator.Desktop.Services;
using WeChatMomentSimulator.Services.Repositories;
using WeChatMomentSimulator.Services.Services;
using WeChatMomentSimulator.Desktop.ViewModels;
using WeChatMomentSimulator.Desktop.Views;
using WeChatMomentSimulator.Services.DataBinding;
using WeChatMomentSimulator.Services.Rendering;
using SvgTemplateEditorWindow = WeChatMomentSimulator.Desktop.Views.SvgTemplateEditorWindow;

namespace WeChatMomentSimulator.Desktop
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public readonly IHost _host;

        public App()
        {
            // 创建主机
            _host = CreateHostBuilder().Build();
        }

        /// <summary>
        /// 创建通用主机构建器
        /// </summary>
        private static IHostBuilder CreateHostBuilder() =>
            Host.CreateDefaultBuilder()
                .ConfigureAppConfiguration((hostContext, configBuilder) =>
                {
                    // 配置应用程序设置
                    configBuilder
                        .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production"}.json", optional: true)
                        .AddEnvironmentVariables();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    // 配置 Serilog
                    var logConfig = new LoggerConfiguration()
                        .ReadFrom.Configuration(hostContext.Configuration)
                        .Enrich.FromLogContext()
                        .Enrich.WithMachineName()
                        .Enrich.WithThreadId()
                        .CreateLogger();

                    // 设置全局 Serilog 记录器
                    Log.Logger = logConfig;

                    // 配置路径
                    string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    string templateDirectory = Path.Combine(baseDirectory, "Templates");

                    // 注册核心服务
                    services.AddSingleton<IPlaceholderParser, PlaceholderParser>();
                    services.AddSingleton<IFileService, FileService>();
                    services.AddSingleton<ITemplateRepository>(sp =>
                        new TemplateRepository(
                            sp.GetRequiredService<IFileService>(),
                            templateDirectory)
                    );
                    // 在应用程序依赖项注册处添加
                    services.AddSingleton<ISvgCustomRenderer, SvgRenderer>();
                    services.AddSingleton<ITemplateManager, TemplateManager>();
                    
                    
                    // 注册应用服务
                    //services.AddSingleton<ITemplateService, TemplateService>();
                    // In ConfigureServices method
                    services.AddSingleton<IDialogService, DialogService>();
                    services.AddSingleton<ITemplateEditorService, TemplateEditorService>();
                    // In ConfigureServices method
                    services.AddSingleton<ISettingsService, SettingsService>();
                    
                    services.AddSingleton<IFileFinder, FileFinder>();

                    // 注册数据绑定相关服务
                    services.AddSingleton<IDataProvider, MemoryDataProvider>();
                    services.AddSingleton<IDataBindingContext,  DataBindingContext>();
                    
                    // 注册视图模型
                    services.AddSingleton<MainViewModel>();
                    // services.AddSingleton<PlaceholderEditorViewModel>();
                    // services.AddSingleton<SvgTemplateEditorViewModel>();
                    //services.AddSingleton<SvgTemplateEditorViewModel328>();
                    
                    // 注册编辑器相关服务
                    services.AddSingleton<ITemplateRenderer, SharpVectorRenderer>();
                    services.AddSingleton<ITemplateCombiner, TemplateCombiner>();
            
                    // 注册编辑器
                    services.AddTransient<IPhoneTemplateEditor, PhoneTemplateEditor>();
                    services.AddTransient<IContentTemplateEditor, ContentTemplateEditor>();
            
                    // 注册参数处理器
                    services.AddTransient<IStatusBarParameterProcessor, StatusBarParameterProcessor>();
                    services.AddTransient<IParameterPresetManager, ParameterPresetManager>();
            
                    // 注册视图模型
                    services.AddTransient<PhoneTemplateEditorViewModel>();
                    services.AddTransient<ContentTemplateEditorViewModel>();
                    
                    // 注册主窗口
                    services.AddSingleton<MainWindow>();
                    services.AddTransient<SvgTemplateEditorWindow>();
                    //services.AddTransient<SvgTemplateEditor328Window>();
                })
                .UseSerilog() // 配置使用 Serilog
                .ConfigureLogging((hostContext, logging) =>
                {
                    // 如需额外日志配置，可在此处添加
                });

        /// <summary>
        /// 应用程序启动
        /// </summary>
        protected override async void OnStartup(StartupEventArgs e)
        {
            await _host.StartAsync();

            try
            {
                // 获取或创建 logger
                var logger = _host.Services.GetRequiredService<ILogger<App>>();
                logger.LogInformation("应用程序启动");
                
                
                
                // 获取主窗口并显示
                // var mainWindow = _host.Services.GetRequiredService<MainWindow>(_host.Services);
                var mainWindow = new MainWindow(_host.Services);
                mainWindow.Show();
            }
            catch (Exception ex)
            {
                var logger = _host.Services.GetRequiredService<ILogger<App>>();
                logger.LogCritical(ex, "应用程序启动失败");
                Shutdown(1);
            }

            base.OnStartup(e);
        }

        /// <summary>
        /// 应用程序退出
        /// </summary>
        protected override async void OnExit(ExitEventArgs e)
        {
            try
            {   
                // 获取或创建 logger
                var logger = _host.Services.GetRequiredService<ILogger<App>>();
                logger.LogInformation($"应用程序正常退出，退出码: {e.ApplicationExitCode}");

                // 停止主机
                using (_host)
                {
                    await _host.StopAsync(TimeSpan.FromSeconds(5));
                }
                
                // 关闭日志系统
                Log.CloseAndFlush();
            }
            catch
            {
                // 在退出时不抛出异常
            }
            finally
            {
                base.OnExit(e);
            }
        }
    }
}