using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using WeChatMomentSimulator.Core.Interfaces;
using WeChatMomentSimulator.Core.Interfaces.DataBingding;
using WeChatMomentSimulator.Core.Interfaces.Services;
using WeChatMomentSimulator.Core.Models.Template;
using WeChatMomentSimulator.Desktop.Utils;
using WeChatMomentSimulator.Desktop.ViewModels.Base;
using WeChatMomentSimulator.Services.DataBinding;
using LoggerExtensions = WeChatMomentSimulator.Core.Logging.LoggerExtensions;

namespace WeChatMomentSimulator.Desktop.ViewModels
{
    /// <summary>
    /// 预览尺寸选项
    /// </summary>
    public class PreviewSizeOption
    {
        public string Name { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }
    
    /// <summary>
    /// SVG模板编辑器视图模型
    /// </summary>
    public class SvgTemplateEditorViewModel : ViewModelBase
    {
        private readonly ITemplateManager _templateManager;
        private readonly ISvgCustomRenderer _svgRenderer;
        private readonly IDataBindingContext _dataBindingContext;
        private readonly PlaceholderEditorViewModel _placeholderEditorVM;
        private readonly ILogger<SvgTemplateEditorViewModel> _logger;
        private readonly Debouncer _refreshDebouncer = new Debouncer();
        //Change the collection type in the class properties  
        //public ObservableCollection<Template> AvailableTemplates { get; } = new ObservableCollection<Template>();
        
        private string _templateContent;
        private string _currentTemplateName;
        private BitmapImage _previewImage;
        private bool _isRefreshing;
        private string _statusText;
        private string _svgInfo;
        private PreviewSizeOption _selectedPreviewSize;
        
        /// <summary>
        /// 模板内容
        /// </summary>
        public string TemplateContent
        {
            get => _templateContent;
            set
            {
                if (SetProperty(ref _templateContent, value))
                {
                    _refreshDebouncer.Debounce(500, async () =>
                    {
                        _dataBindingContext.SetTemplate(value);
                        _placeholderEditorVM.UpdateTemplate(value);
                        await RefreshPreviewAsync();
                    });
                }
            }
        }
        
        /// <summary>
        /// 当前模板名称
        /// </summary>
        public string CurrentTemplateName
        {
            get => _currentTemplateName;
            set
            {
                if (SetProperty(ref _currentTemplateName, value))
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        _ = LoadTemplateAsync(value);
                    }
                }
            }
        }
        
        /// <summary>
        /// 预览图像
        /// </summary>
        public BitmapImage PreviewImage
        {
            get => _previewImage;
            set => SetProperty(ref _previewImage, value);
        }
        
        /// <summary>
        /// 是否正在刷新
        /// </summary>
        public bool IsRefreshing
        {
            get => _isRefreshing;
            set => SetProperty(ref _isRefreshing, value);
        }
        
        /// <summary>
        /// 状态文本
        /// </summary>
        public string StatusText
        {
            get => _statusText;
            set => SetProperty(ref _statusText, value);
        }
        
        /// <summary>
        /// SVG信息
        /// </summary>
        public string SvgInfo
        {
            get => _svgInfo;
            set => SetProperty(ref _svgInfo, value);
        }
        
        /// <summary>
        /// 可用的模板列表
        /// </summary>
        public ObservableCollection<string> AvailableTemplates { get; } = new ObservableCollection<string>();
        
        /// <summary>
        /// 预览尺寸选项
        /// </summary>
        public ObservableCollection<PreviewSizeOption> PreviewSizes { get; } = new ObservableCollection<PreviewSizeOption>();
        
        /// <summary>
        /// 选中的预览尺寸
        /// </summary>
        public PreviewSizeOption SelectedPreviewSize
        {
            get => _selectedPreviewSize;
            set
            {
                if (SetProperty(ref _selectedPreviewSize, value))
                {
                    _ = RefreshPreviewAsync();
                }
            }
        }
        
        /// <summary>
        /// 占位符编辑器视图模型
        /// </summary>
        public PlaceholderEditorViewModel PlaceholderEditorVM => _placeholderEditorVM;
        
        /// <summary>
        /// 预览宽度
        /// </summary>
        public int PreviewWidth => SelectedPreviewSize?.Width ?? 400;
        
        /// <summary>
        /// 预览高度
        /// </summary>
        public int PreviewHeight => SelectedPreviewSize?.Height ?? 600;
        
        /// <summary>
        /// 新建模板命令
        /// </summary>
        public ICommand NewTemplateCommand { get; }
        
        /// <summary>
        /// 保存模板命令
        /// </summary>
        public ICommand SaveTemplateCommand { get; }
        
        /// <summary>
        /// 删除模板命令
        /// </summary>
        public ICommand DeleteTemplateCommand { get; }
        
        /// <summary>
        /// 导出图片命令
        /// </summary>
        public ICommand ExportImageCommand { get; }
        
        /// <summary>
        /// 刷新预览命令
        /// </summary>
        public ICommand RefreshPreviewCommand { get; }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public SvgTemplateEditorViewModel(
            ITemplateManager templateManager,
            ISvgCustomRenderer svgRenderer,
            IDataBindingContext dataBindingContext,
            PlaceholderEditorViewModel placeholderEditorViewModel)
        {
            _templateManager = templateManager ?? throw new ArgumentNullException(nameof(templateManager));
            _svgRenderer = svgRenderer ?? throw new ArgumentNullException(nameof(svgRenderer));
            _dataBindingContext = dataBindingContext ?? throw new ArgumentNullException(nameof(dataBindingContext));
            _placeholderEditorVM = placeholderEditorViewModel ?? throw new ArgumentNullException(nameof(placeholderEditorViewModel));
            _logger = LoggerExtensions.GetLogger<SvgTemplateEditorViewModel>();
            
            // 初始化命令
            NewTemplateCommand = new AsyncRelayCommand(ExecuteNewTemplateAsync);
            SaveTemplateCommand = new AsyncRelayCommand(ExecuteSaveTemplateAsync);
            DeleteTemplateCommand = new AsyncRelayCommand(ExecuteDeleteTemplateAsync);
            ExportImageCommand = new AsyncRelayCommand(ExecuteExportImageAsync);
            RefreshPreviewCommand = new AsyncRelayCommand(RefreshPreviewAsync);
            
            // 初始化预览尺寸选项
            InitializePreviewSizes();
            
            // 监听数据变更
            _dataBindingContext.DataChanged += (s, e) => 
            {
                // 使用防抖动刷新预览
                _refreshDebouncer.Debounce(300, () => 
                {
                    _logger.LogDebug("数据已变更，刷新预览");
                    _ = RefreshPreviewAsync();
                });
            };
        }
        
        /// <summary>
        /// 初始化视图模型
        /// </summary>
        public async Task InitializeAsync()
        {
            try
            {
                _logger.LogInformation("正在初始化SVG模板编辑器");
                
                // 创建默认模板
                await _templateManager.CreateDefaultTemplatesAsync();
                
                // 加载模板列表
                await LoadTemplateListAsync();
                
                // 加载第一个模板
                if (AvailableTemplates.Count > 0)
                {
                    await LoadTemplateAsync(AvailableTemplates[0]);
                }
                else
                {
                    // 如果没有模板，创建一个新的
                    await ExecuteNewTemplateAsync();
                }
                
                StatusText = "准备就绪";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "初始化SVG模板编辑器失败");
                StatusText = "初始化失败";
            }
        }
        
        /// <summary>
        /// 加载模板列表
        /// </summary>
        private async Task LoadTemplateListAsync()
        {
            try
            {
                // 获取所有模板
                var templates = await _templateManager.GetAllTemplatesAsync();
                
                // 更新列表
                AvailableTemplates.Clear();
                foreach (string template in templates)
                {
                    AvailableTemplates.Add(template);
                }
                
                _logger.LogInformation("已加载 {Count} 个模板", AvailableTemplates.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载模板列表失败");
                throw;
            }
        }
        
        /// <summary>
        /// 加载模板
        /// </summary>
        public async Task LoadTemplateAsync(string templateName)
        {
            try
            {
                _logger.LogInformation("加载模板: {TemplateName}", templateName);

                // 加载模板内容
                Template _template = await _templateManager.GetTemplateByNameAsync(templateName);
        
                // 检查模板是否存在
                if (_template == null)
                {
                    _logger.LogWarning("模板不存在: {TemplateName}", templateName);
                    StatusText = $"找不到模板: {templateName}";
            
                    // 创建一个空模板对象
                    _template = new Template 
                    { 
                        Name = templateName,
                        Content = null  // 这将触发使用默认模板
                    };
                }
        
                TemplateContent = await _templateManager.ConvertTemplateToStringAsync(_template);
                CurrentTemplateName = templateName;

                // 更新绑定上下文
                _dataBindingContext.SetTemplate(TemplateContent);
                _placeholderEditorVM.UpdateTemplate(TemplateContent);

                // 刷新预览
                await RefreshPreviewAsync();

                StatusText = $"已加载模板: {templateName}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "加载模板失败: {TemplateName}", templateName);
                StatusText = $"加载模板失败: {ex.Message}";
            }
        }
        
        /// <summary>
        /// 刷新预览
        /// </summary>
        private async Task RefreshPreviewAsync()
        {
            try
            {
                _logger.LogDebug("刷新预览");
                IsRefreshing = true;
                
                // 使用绑定上下文处理模板
                string processedSvg = _dataBindingContext.GetProcessedTemplate();
                
                // 非空检查
                if (string.IsNullOrEmpty(processedSvg))
                {
                    _logger.LogWarning("处理后的SVG为空");
                    IsRefreshing = false;
                    return;
                }
                
                // 渲染SVG
                var imageBytes = await _svgRenderer.RenderToImageAsync(
                    processedSvg, PreviewWidth, PreviewHeight);
                    
                if (imageBytes == null)
                {
                    _logger.LogWarning("渲染SVG返回空数据");
                    IsRefreshing = false;
                    return;
                }
                
                // 更新预览图像
                PreviewImage = LoadImage(imageBytes);
                
                // 更新SVG信息
                SvgInfo = $"SVG大小: {processedSvg.Length}字节 | 预览尺寸: {PreviewWidth}x{PreviewHeight}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刷新预览失败");
                StatusText = $"刷新预览失败: {ex.Message}";
            }
            finally
            {
                IsRefreshing = false;
            }
        }
        
        /// <summary>
        /// 初始化预览尺寸选项
        /// </summary>
        private void InitializePreviewSizes()
        {
            PreviewSizes.Add(new PreviewSizeOption { Name = "小 (320x480)", Width = 320, Height = 480 });
            PreviewSizes.Add(new PreviewSizeOption { Name = "中 (375x667)", Width = 375, Height = 667 });
            PreviewSizes.Add(new PreviewSizeOption { Name = "大 (414x736)", Width = 414, Height = 736 });
            PreviewSizes.Add(new PreviewSizeOption { Name = "超大 (428x926)", Width = 428, Height = 926 });
            
            // 默认选中中等尺寸
            SelectedPreviewSize = PreviewSizes[1];
        }
        
        /// <summary>
        /// 从字节数组加载图像
        /// </summary>
        private BitmapImage LoadImage(byte[] imageData)
        {
            if (imageData == null) return null;
            
            var image = new BitmapImage();
            using (var stream = new MemoryStream(imageData))
            {
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.StreamSource = stream;
                image.EndInit();
                image.Freeze();
            }
            return image;
        }
        
        /// <summary>
        /// 执行新建模板命令
        /// </summary>
        private async Task ExecuteNewTemplateAsync()
        {
            try
            {
                // 创建新模板名称
                string baseTemplateName = "新模板";
                string templateName = baseTemplateName;
                int counter = 1;
                
                // 确保名称唯一
                while (await _templateManager.TemplateExistsAsync(templateName))
                {
                    templateName = $"{baseTemplateName}_{counter++}";
                }
                
                // 创建空模板
                const string emptyTemplate = @"<svg width=""400"" height=""600"" xmlns=""http://www.w3.org/2000/svg"">
  <!-- 状态栏 -->
  <rect x=""0"" y=""0"" width=""400"" height=""40"" fill=""#f5f5f5"" />
  <text x=""20"" y=""25"" font-family=""Arial"" font-size=""14"">{{time}}</text>
  <text x=""380"" y=""25"" font-family=""Arial"" font-size=""14"" text-anchor=""end"">{{battery}}</text>
  
  <!-- 内容区域 -->
  <text x=""200"" y=""300"" font-family=""Arial"" font-size=""16"" text-anchor=""middle"">
    开始编辑您的模板
  </text>
</svg>";
                
                // 保存模板
                await _templateManager.SaveTemplateAsync(templateName, emptyTemplate);
                
                // 更新模板列表
                AvailableTemplates.Add(templateName);
                
                // 加载新模板
                await LoadTemplateAsync(templateName);
                
                StatusText = $"已创建新模板: {templateName}";
                _logger.LogInformation("已创建新模板: {TemplateName}", templateName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "创建新模板失败");
                StatusText = $"创建新模板失败: {ex.Message}";
            }
        }
        
        /// <summary>
        /// 执行保存模板命令
        /// </summary>
        private async Task ExecuteSaveTemplateAsync()
        {
            try
            {
                // 如果没有选择模板名称，提示输入
                if (string.IsNullOrEmpty(CurrentTemplateName))
                {
                    StatusText = "请输入模板名称";
                    return;
                }
                
                // 保存模板
                await _templateManager.SaveTemplateAsync(TemplateContent, CurrentTemplateName );
                
                // 如果是新模板，添加到列表
                if (!AvailableTemplates.Contains(CurrentTemplateName))
                {
                    AvailableTemplates.Add(CurrentTemplateName);
                }
                
                StatusText = $"已保存模板: {CurrentTemplateName}";
                _logger.LogInformation("已保存模板: {TemplateName}", CurrentTemplateName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "保存模板失败: {TemplateName}", CurrentTemplateName);
                StatusText = $"保存模板失败: {ex.Message}";
            }
        }
        
        /// <summary>
        /// 执行删除模板命令
        /// </summary>
        private async Task ExecuteDeleteTemplateAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(CurrentTemplateName))
                {
                    StatusText = "未选择模板";
                    return;
                }
                
                // TODO: 添加确认对话框
                
                // 删除模板
                await _templateManager.DeleteTemplateByNameAsync(CurrentTemplateName);
                
                // 从列表中移除
                AvailableTemplates.Remove(CurrentTemplateName);
                
                // 如果列表不为空，选择第一个模板
                if (AvailableTemplates.Count > 0)
                {
                    await LoadTemplateAsync(AvailableTemplates[0]);
                }
                else
                {
                    // 创建一个新模板
                    await ExecuteNewTemplateAsync();
                }
                
                StatusText = $"已删除模板: {CurrentTemplateName}";
                _logger.LogInformation("已删除模板: {TemplateName}", CurrentTemplateName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "删除模板失败: {TemplateName}", CurrentTemplateName);
                StatusText = $"删除模板失败: {ex.Message}";
            }
        }
        
        /// <summary>
        /// 执行导出图片命令
        /// </summary>
        private async Task ExecuteExportImageAsync()
        {
            try
            {
                // 获取处理后的SVG内容
                string processedSvg = _dataBindingContext.GetProcessedTemplate();
                if (string.IsNullOrEmpty(processedSvg))
                {
                    StatusText = "没有可导出的内容";
                    return;
                }
                
                // 显示保存对话框
                var dialog = new SaveFileDialog
                {
                    Title = "保存图片",
                    Filter = "PNG图片(*.png)|*.png|JPEG图片(*.jpg)|*.jpg|所有文件(*.*)|*.*",
                    DefaultExt = "png",
                    FileName = $"{CurrentTemplateName ?? "template"}.png"
                };
                
                if (dialog.ShowDialog() == true)
                {
                    // 渲染并保存
                    bool success = await _svgRenderer.SaveToImageFileAsync(
                        processedSvg, dialog.FileName, PreviewWidth, PreviewHeight);
                        
                    if (success)
                    {
                        StatusText = $"已导出图片: {dialog.FileName}";
                        _logger.LogInformation("已导出图片: {FilePath}", dialog.FileName);
                    }
                    else
                    {
                        StatusText = "导出图片失败";
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "导出图片失败");
                StatusText = $"导出图片失败: {ex.Message}";
            }
        }
    }
}