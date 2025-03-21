using System.Collections.ObjectModel;
using Microsoft.Extensions.DependencyInjection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Serilog; // 这里假设使用 Serilog 的 Logger 类型
using WeChatMomentSimulator.Core.Interfaces;
using WeChatMomentSimulator.Core.Models.Template;
using WeChatMomentSimulator.Services.Storage;
using WeChatMomentSimulator.UI.Testing;
using WeChatMomentSimulator.UI.ViewModels.Templates;
using WeChatMomentSimulator.UI.Views.Templates;

namespace WeChatMomentSimulator.UI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly ILogger _logger;
    private readonly IServiceProvider _serviceProvider;

    public MainWindow(IServiceProvider serviceProvider, ILogger logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
        InitializeComponent();
        // 在窗口加载时运行测试
        Loaded += async (s, e) => await CreateAndTestTemplate();
    }


    private async void LoadTemplates_Click(object sender, RoutedEventArgs e)
    {
        _logger.Information("开始加载模板"); // 使用日志记录器
        StatusText.Text = "正在加载模板...";
        try
        {
            var templateManager = _serviceProvider.GetService<ITemplateRepository>();
            if (templateManager == null)
            {
                _logger.Error("无法获取模板管理器服务");
                StatusText.Text = "无法获取模板管理器服务，请检查依赖注入配置。";
                return;
            }

            var templates = await templateManager.GetAllTemplatesAsync();
            TemplatesItemsControl.ItemsSource = templates;
            StatusText.Text = $"已加载 {templates.Count()} 个模板";
            _logger.Information("模板加载成功，共加载 {TemplateCount} 个模板", templates.Count());
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "加载模板时发生错误");
            StatusText.Text = $"加载失败: {ex.Message}";
        }

    }

    private async Task CreateAndTestTemplate()
    {
        try
        {
            // 获取模板管理器
            var templateManager = _serviceProvider.GetRequiredService<TemplateManager>();

            // 创建符合实际结构的测试模板
            var testTemplate = new TemplateDefinition
            {
                Id = Guid.NewGuid(),
                Metadata = new TemplateMetadata
                {
                    Name = "测试模板1",
                    Description = "这是一个测试模板",
                    Author = "测试作者",
                    Category = "测试类别",
                    Tags = new List<string> { "测试", "示例" }
                },
                SvgContent = "<svg>...</svg>", // 这里填入实际的SVG内容
                Placeholders = new List<PlaceholderInfo>
                {
                    new PlaceholderInfo
                    {
                        Id = "placeholder1",
                        Type = PlaceholderType.Text,
                        Name = "用户名",
                        ElementId = "username-text",
                        Description = "用户名称输入区域",
                        DefaultValue = "张三",
                        IsRequired = true,
                        MaxLength = 20,
                        ValidationRegex = "^[\\u4e00-\\u9fa5a-zA-Z0-9]+$",
                        X = 100.5f,
                        Y = 200.3f,
                        Width = 150.0f,
                        Height = 30.0f,
                        CustomProperties = new Dictionary<string, string>
                        {
                            { "fontSize", "14" },
                            { "color", "#333333" }
                        }
                    }
                },
                CreatedDate = DateTime.Now,
                ModifiedDate = DateTime.Now,
                Version = "1.0.0"
            };

            // 验证模板
            if (!testTemplate.IsValid())
            {
                _logger.Error("测试模板无效");
            }

            // 保存模板
            bool saveResult = await templateManager.SaveTemplateAsync(testTemplate);
            if (saveResult)
            {
                _logger.Information("测试模板保存成功");

                // 加载模板
                var loadedTemplate = await templateManager.GetTemplateByIdAsync(testTemplate.Id);
                if (loadedTemplate != null)
                {
                    _logger.Information("成功加载测试模板: {Name}", loadedTemplate.Metadata.Name);
                    _logger.Debug("模板详细信息: {@Template}", loadedTemplate);
                }
                else
                {
                    _logger.Warning("未能加载测试模板");
                }
            }
            else
            {
                _logger.Error("测试模板保存失败");
            }
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "创建和测试模板时发生错误");
        }
    }


protected override void OnKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.L &&
            (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control &&
            (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
        {
            ((App)Application.Current).ShowLogWindow();
        }

        base.OnKeyDown(e);
    }

    private void TestTemplateButton_code_Click(object sender, RoutedEventArgs e)
    {
        CreateAndTestTemplate2();
    }

    /// <summary>
    /// 创建并测试模板功能
    /// </summary>
    private async void CreateAndTestTemplate2()
    {
        try
        {
            // 使用类成员服务
            var templateManager = _serviceProvider.GetRequiredService<TemplateManager>();
        
            _logger.Information("开始测试模板功能");

            // 步骤 1: 创建测试模板
            _logger.Information("步骤 1: 创建测试模板");
            var template = await templateManager.CreateTemplateAsync("测试模板");
            if (template == null)
            {
                _logger.Error("创建模板失败");
                MessageBox.Show("创建测试模板失败", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
        
            _logger.Information($"成功创建模板，ID: {template.Id}");
        
            // 步骤 2: 更新模板元数据
            _logger.Information("步骤 2: 更新模板元数据");
            template.Metadata.Name = "更新后的测试模板";
            template.Metadata.Description = "这是一个测试模板，用于验证模板功能";
            template.Metadata.Author = "测试用户";
            template.Metadata.Category = "测试分类";
        
           await templateManager.UpdateTemplateAsync(template);
            
            _logger.Information("模板元数据更新成功");
        
            // 步骤 3: 获取所有模板
            _logger.Information("步骤 3: 获取所有模板");
            var allTemplates = await templateManager.GetAllTemplatesAsync();
            _logger.Information($"获取到 {allTemplates.Count()} 个模板");
        
            // 测试结果
            _logger.Information("模板功能测试完成");
            MessageBox.Show(
                $"模板功能测试完成!\n\n" +
                $"模板ID: {template.Id}\n" +
                $"模板名称: {template.Metadata.Name}\n" +
                $"总模板数: {allTemplates.Count()}",
                "测试成功",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "测试模板功能时发生错误");
            MessageBox.Show($"测试失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void TestTemplateButton_2_Click(object sender, RoutedEventArgs e)
    {
        
       
    }

    private void StoragePathTest_Click(object sender, RoutedEventArgs e)
    {
        var pathService = _serviceProvider.GetService(typeof(IPathService)) as IPathService;
        var testWindow = new StoragePathTestWindow(pathService, _serviceProvider);
        testWindow.Show();
         
    }

    private void OpenLogWindow_Click(object sender, RoutedEventArgs e)
    {
        throw new NotImplementedException();
    }
}