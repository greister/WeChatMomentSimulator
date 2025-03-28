using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.Input;
using ICSharpCode.AvalonEdit;
using Microsoft.Extensions.Logging;
using WeChatMomentSimulator.Core.Interfaces.Services;
using WeChatMomentSimulator.Desktop.ViewModels.Base;
using AsyncRelayCommand = CommunityToolkit.Mvvm.Input.AsyncRelayCommand;
using MessageBox = System.Windows.MessageBox;

namespace WeChatMomentSimulator.Desktop.ViewModels
{
    public class SvgTemplateEditorViewModel : ViewModelBase
    {
        private readonly ILogger<SvgTemplateEditorViewModel> _logger;
        private readonly ITemplateManager _templateManager;
        private readonly ISvgCustomRenderer _svgRenderer;
        private readonly ITemplateEditorService  _templateEditorService;
        private TextEditor _editor;
        private string _editorPosition;
        
        
        
        /// <summary>
        /// 设置编辑器实例
        /// </summary>
        public void SetEditor(TextEditor editor)
        {
            _editor = editor;
        }
        
        // Editor State
        private string _editorText = string.Empty;
        public string EditorText
        {
            get => _editorText;
            set => SetProperty(ref _editorText, value, onChanged: () => RefreshPreviewAsync().ConfigureAwait(false));
        }

        
        /// <summary>
        /// 编辑器光标位置
        /// </summary>
        public string EditorPosition
        {
            get => _editorPosition;
            set => SetProperty(ref _editorPosition, value);
        }
        // Preview State
        private BitmapImage _previewImage;
        public BitmapImage PreviewImage
        {
            get => _previewImage;
            private set => SetProperty(ref _previewImage, value);
        }
    
        private string _currentTemplateName;
        public string CurrentTemplateName
        {
            get => _currentTemplateName;
            set => SetProperty(ref _currentTemplateName, value);
        }

        private double _previewZoom = 1.0;


        public double PreviewZoom
        {
            get => _previewZoom;
            set => SetProperty(ref _previewZoom, value);
        }

        // Commands
        public IAsyncRelayCommand NewTemplateCommand { get; }
        public IAsyncRelayCommand OpenTemplateCommand { get; }
        public IAsyncRelayCommand SaveTemplateCommand { get; }
        public IAsyncRelayCommand SaveAsTemplateCommand { get; }
        public IAsyncRelayCommand RefreshPreviewCommand { get; }
        public IRelayCommand ZoomInCommand { get; }
        public IRelayCommand ZoomOutCommand { get; }
        public IRelayCommand ResetZoomCommand { get; }

        public SvgTemplateEditorViewModel(
            ILogger<SvgTemplateEditorViewModel> logger,
            ITemplateManager templateManager,
            ITemplateEditorService templateEditorService,
            ISvgCustomRenderer svgRenderer)
        {
            _logger = logger;
            _templateManager = templateManager;
            _svgRenderer = svgRenderer;
            
            _templateEditorService = templateEditorService;
            // Initialize commands
            NewTemplateCommand = new AsyncRelayCommand(CreateNewTemplateAsync);
            OpenTemplateCommand = new AsyncRelayCommand(OpenTemplateAsync);
            SaveTemplateCommand = new AsyncRelayCommand(SaveTemplateAsync);
            SaveAsTemplateCommand = new AsyncRelayCommand(SaveAsTemplateAsync);
            RefreshPreviewCommand = new AsyncRelayCommand(RefreshPreviewAsync);
            
            ZoomInCommand = new RelayCommand(() => PreviewZoom *= 1.1);
            ZoomOutCommand = new RelayCommand(() => PreviewZoom *= 0.9);
            ResetZoomCommand = new RelayCommand(() => PreviewZoom = 1.0);
        }

       
        
        
        /// <summary>
        /// 初始化
        /// </summary>
        public async Task InitializeAsync()
        {
            // 加载默认模板或执行其他初始化操作
            EditorText = await _templateEditorService.CreateNewTemplateAsync();
            await RefreshPreviewAsync();
        }
        
        
        /// <summary>
        /// 从文件打开模板
        /// </summary>
       private async Task CreateNewTemplateAsync()
        {
            EditorText = await _templateEditorService.CreateNewTemplateAsync();
            await RefreshPreviewAsync();
        }

        private async Task OpenTemplateAsync()
        {
            var (content, name) = await _templateEditorService.OpenTemplateAsync();
            if (!string.IsNullOrEmpty(content))
            {
                EditorText = content;
                CurrentTemplateName = name;
                await RefreshPreviewAsync();
            }
        }

        private async Task SaveTemplateAsync()
        {
            try
            {
                if (string.IsNullOrEmpty(CurrentTemplateName))
                {
                    await SaveAsTemplateAsync();
                    return;
                }

                await _templateEditorService.SaveTemplateAsync(CurrentTemplateName, EditorText);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存模板失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task SaveAsTemplateAsync()
        {
            try
            {
                string name = await _templateEditorService.SaveAsTemplateAsync(EditorText, CurrentTemplateName);
                if (!string.IsNullOrEmpty(name))
                {
                    CurrentTemplateName = name;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"另存模板失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task RefreshPreviewAsync()
        {
            if (string.IsNullOrWhiteSpace(EditorText)) return;

            try
            {
                // 从SVG内容中解析宽高，或使用默认值
                int width = 375;  // 默认宽度
                int height = 667; // 默认高度

                // 解析尝试从SVG中提取宽高
                // 可以通过正则表达式从EditorText中提取

                var imageBytes = await _svgRenderer.RenderToImageAsync(EditorText, width, height);
                using var stream = new MemoryStream(imageBytes);
        
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.StreamSource = stream;
                bitmap.EndInit();
                bitmap.Freeze();

                PreviewImage = bitmap;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刷新预览失败");
            }
        }
    }
}