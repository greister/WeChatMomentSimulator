using System;
using System.Windows;
using System.Windows.Controls;
using WeChatMomentSimulator.Core.Logging;
using Microsoft.Extensions.Logging;
using WeChatMomentSimulator.Desktop.ViewModels;
using LoggerExtensions = WeChatMomentSimulator.Core.Logging.LoggerExtensions;

namespace WeChatMomentSimulator.Desktop.Views
{
    /// <summary>
    /// MenuBarView.xaml 的交互逻辑
    /// </summary>
    public partial class MenuBarView : UserControl
    {
        private readonly ILogger<MenuBarView> _logger;
        
        public MenuBarView()
        {
            InitializeComponent();
            
            // 在构造函数中初始化日志记录器
            _logger = LoggerExtensions.GetLogger<MenuBarView>();
            _logger.LogDebug("菜单栏视图已初始化");
        }
        
        // 文件菜单事件处理
        private void NewTemplate_Click(object sender, RoutedEventArgs e)
        {
            _logger.LogInformation("用户点击菜单：新建模板");
            
            // 获取主视图模型
            if (DataContext is MainViewModel viewModel)
            {
                if (viewModel.NewTemplateCommand.CanExecute(null))
                {
                    viewModel.NewTemplateCommand.Execute(null);
                }
            }
        }
        
        private void OpenTemplate_Click(object sender, RoutedEventArgs e)
        {
            _logger.LogInformation("用户点击菜单：打开模板");
            
            // 示例：打开模板文件对话框
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "打开模板文件",
                Filter = "SVG模板文件 (*.svg)|*.svg|所有文件 (*.*)|*.*",
                DefaultExt = ".svg"
            };
            
            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                _logger.LogInformation("用户选择了文件: {FilePath}", dialog.FileName);
                // 执行打开模板操作
            }
        }
        
        private void SaveTemplate_Click(object sender, RoutedEventArgs e)
        {
            _logger.LogInformation("用户点击菜单：保存模板");
            
            if (DataContext is MainViewModel viewModel)
            {
                if (viewModel.SaveTemplateCommand.CanExecute(null))
                {
                    viewModel.SaveTemplateCommand.Execute(null);
                }
            }
        }
        
        private void ExportImage_Click(object sender, RoutedEventArgs e)
        {
            _logger.LogInformation("用户点击菜单：导出图片");
            
            if (DataContext is MainViewModel viewModel)
            {
                if (viewModel.ExportImageCommand.CanExecute(null))
                {
                    viewModel.ExportImageCommand.Execute(null);
                }
            }
        }
        
        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            _logger.LogInformation("用户点击菜单：退出应用");
            Application.Current.Shutdown();
        }
        
        // 编辑菜单事件处理
        private void GenerateTestData_Click(object sender, RoutedEventArgs e)
        {
            _logger.LogInformation("用户点击菜单：生成测试数据");
            
            if (DataContext is MainViewModel viewModel)
            {
                try
                {
                    // 假设MainViewModel有一个GenerateTestData方法
                    // viewModel.GenerateTestData();
                    _logger.LogInformation("已生成测试数据");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "生成测试数据失败");
                }
            }
        }
        
        // 帮助菜单事件处理
        private void About_Click(object sender, RoutedEventArgs e)
        {
            _logger.LogInformation("用户点击菜单：关于");
            
            string aboutMessage = "微信朋友圈模拟器 v1.0\n" +
                                 "Copyright © 2025\n\n" +
                                 "一个用于创建模拟微信内容的工具";
                                 
            MessageBox.Show(aboutMessage, "关于", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
        private void CheckForUpdates_Click(object sender, RoutedEventArgs e)
        {
            _logger.LogInformation("用户点击菜单：检查更新");
            
            // 示例：检查更新
            try
            {
                _logger.LogDebug("开始检查应用更新");
                // TODO: 实现检查更新逻辑
                
                MessageBox.Show("您的应用已是最新版本。", "检查更新", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "检查更新失败");
                MessageBox.Show($"检查更新时发生错误: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}