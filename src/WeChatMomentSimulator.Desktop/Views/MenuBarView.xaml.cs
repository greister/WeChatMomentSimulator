
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace WeChatMomentSimulator.Desktop.Views;

public partial class MenuBarView : UserControl
{
    // 使用Serilog对象工厂方法创建logger
    private readonly ILogger _logger = Log.ForContext<MenuBarView>();
    // 依赖注入服务提供者
    private readonly IServiceProvider _serviceProvider;
    
    
    // public MenuBarView(IServiceProvider serviceProvider)
    // {
    //     InitializeComponent();
    //     _serviceProvider = serviceProvider;
    //     _logger.Information("菜单栏视图已初始化");
    //     // 初始化命令
    //     
    //     
    // }
    
    public MenuBarView()
    {
        InitializeComponent();
        _logger.Information("菜单栏视图已使用默认构造函数初始化");
    
        // 尝试从应用程序获取服务提供者
        try 
        {
            _serviceProvider = ((App)Application.Current)._host?.Services;
        }
        catch (Exception ex)
        {
            _logger.Warning(ex, "无法从应用程序获取服务提供者");
        }
    }

    private void OpenSvgEditor_Click(object sender, RoutedEventArgs e)
    {
        _logger.Information("用户点击了SVG模板编辑器按钮");
        try
        {
            // //var editor = new SvgTemplateEditor();
            // _logger.Debug("已创建SVG模板编辑器实例");
            //
            // editor.Owner = Application.Current.MainWindow;
            // editor.ShowDialog();
            // _logger.Information("SVG模板编辑器已关闭");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "打开SVG模板编辑器失败");
            MessageBox.Show($"打开SVG模板编辑器失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
  
    

    private void OpenSvgEditor2_Click(object sender, RoutedEventArgs e)
    {
        _logger.Information("用户点击了SVG模板编辑器窗口按钮");
        try
        {
            // var editorWindow = _serviceProvider.GetService<SvgTemplateEditorWindow328>();
            var editorWindow = _serviceProvider.GetRequiredService<SvgTemplateEditorWindow>();;
            
            _logger.Debug("已创建SVG模板编辑器窗口实例");
        
            editorWindow.Owner = Application.Current.MainWindow;
            editorWindow.ShowDialog();
            _logger.Information("SVG模板编辑器窗口已关闭");
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "打开SVG模板编辑器窗口失败");
            MessageBox.Show($"打开SVG模板编辑器窗口失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

// ... existing code ...
}