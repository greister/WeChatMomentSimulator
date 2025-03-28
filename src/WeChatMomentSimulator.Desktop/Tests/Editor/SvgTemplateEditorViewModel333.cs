using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Extensions.Logging;
using WeChatMomentSimulator.Desktop.Commands;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WeChatMomentSimulator.Desktop.ViewModels
{
    public enum PanelType
    {
        Preview,
        Placeholders,
        MockData
    }
    
    public class SvgTemplateEditorViewModel328 : INotifyPropertyChanged
    {
        private readonly ILogger<SvgTemplateEditorViewModel> _logger;
        
        // 编辑器状态
        private string _editorText = "";
        public string EditorText
        {
            get => _editorText;
            set 
            { 
                if (SetProperty(ref _editorText, value))
                {
                    // 当文本更新时，自动刷新预览
                    RefreshPreviewAsync().ConfigureAwait(false);
                }
            }
        }
        
        private string _editorPosition = "1:1";
        public string EditorPosition
        {
            get => _editorPosition;
            set => SetProperty(ref _editorPosition, value);
        }
        
        private string _documentStatus = "就绪";
        public string DocumentStatus
        {
            get => _documentStatus;
            set => SetProperty(ref _documentStatus, value);
        }
        
        private int _placeholderCount = 0;
        public int PlaceholderCount
        {
            get => _placeholderCount;
            set => SetProperty(ref _placeholderCount, value);
        }
        
        private int _imageCount = 0;
        public int ImageCount
        {
            get => _imageCount;
            set => SetProperty(ref _imageCount, value);
        }
        
        // 预览相关属性
        private BitmapSource _previewImage;
        public BitmapSource PreviewImage
        {
            get => _previewImage;
            set => SetProperty(ref _previewImage, value);
        }
        
        private double _previewZoom = 1.0;
        public double PreviewZoom
        {
            get => _previewZoom;
            set => SetProperty(ref _previewZoom, value);
        }
        
        private double _previewRotation = 0;
        public double PreviewRotation
        {
            get => _previewRotation;
            set => SetProperty(ref _previewRotation, value);
        }
        
        private int _zoomLevel = 100;
        public int ZoomLevel
        {
            get => _zoomLevel;
            set => SetProperty(ref _zoomLevel, value);
        }
        
        // 面板相关属性
        private bool _isPanelVisible = true;
        public bool IsPanelVisible
        {
            get => _isPanelVisible;
            set => SetProperty(ref _isPanelVisible, value);
        }
        
        private PanelType _activePanel = PanelType.Preview;
        public PanelType ActivePanel
        {
            get => _activePanel;
            set => SetProperty(ref _activePanel, value);
        }
        
        public bool IsPreviewPanelActive => ActivePanel == PanelType.Preview;
        public bool IsPlaceholdersPanelActive => ActivePanel == PanelType.Placeholders;
        public bool IsMockDataPanelActive => ActivePanel == PanelType.MockData;
        
        private GridLength _panelWidth = new GridLength(400);
        public GridLength PanelWidth
        {
            get => _panelWidth;
            set => SetProperty(ref _panelWidth, value);
        }
        
        // 模板相关属性
        private string _selectedTemplate;
        public string SelectedTemplate
        {
            get => _selectedTemplate;
            set => SetProperty(ref _selectedTemplate, value);
        }
        
        public ObservableCollection<string> AvailableTemplates { get; } = new ObservableCollection<string>();
        
        // 子视图模型
        public object PlaceholderEditorVM { get; set; }
        public object MockDataEditorVM { get; set; }
        
        // 命令
        public ICommand NewTemplateCommand { get; }
        public ICommand OpenTemplateCommand { get; }
        public ICommand SaveTemplateCommand { get; }
        public ICommand SaveAsTemplateCommand { get; }
        public ICommand ExitCommand { get; }
        public ICommand UndoCommand { get; }
        public ICommand RedoCommand { get; }
        public ICommand FindReplaceCommand { get; }
        public ICommand RefreshPreviewCommand { get; }
        public ICommand ZoomInCommand { get; }
        public ICommand ZoomOutCommand { get; }
        public ICommand ResetZoomCommand { get; }
        public ICommand RotateLeftCommand { get; }
        public ICommand RotateRightCommand { get; }
        public ICommand SelectPanelCommand { get; }
        public ICommand ClosePanelCommand { get; }
        public ICommand TogglePanelCommand { get; }
        public ICommand InsertTextPlaceholderCommand { get; }
        public ICommand InsertImagePlaceholderCommand { get; }
        public ICommand RefreshPlaceholdersCommand { get; }
        public ICommand GenerateMockDataCommand { get; }
        public ICommand ImportDataCommand { get; }
        public ICommand ExportDataCommand { get; }
        public ICommand AboutCommand { get; }
        public ICommand HelpCommand { get; }
        
        // 编辑器实例
        private ICSharpCode.AvalonEdit.TextEditor _editor;
        
        // 构造函数
        public SvgTemplateEditorViewModel328(ILogger<SvgTemplateEditorViewModel> logger = null)
        {
            _logger = logger;
            
            // 初始化命令
            NewTemplateCommand = new RelayCommand(_ => CreateNewTemplate());
            OpenTemplateCommand = new RelayCommand(_ => OpenTemplate());
            SaveTemplateCommand = new RelayCommand(_ => SaveTemplate());
            SaveAsTemplateCommand = new RelayCommand(_ => SaveAsTemplate());
            ExitCommand = new RelayCommand(_ => Exit());
            UndoCommand = new RelayCommand(_ => Undo(), _ => CanUndo());
            RedoCommand = new RelayCommand(_ => Redo(), _ => CanRedo());
            FindReplaceCommand = new RelayCommand(_ => ShowFindReplace());
            RefreshPreviewCommand = new RelayCommand(async _ => await RefreshPreviewAsync());
            ZoomInCommand = new RelayCommand(_ => ZoomIn());
            ZoomOutCommand = new RelayCommand(_ => ZoomOut());
            ResetZoomCommand = new RelayCommand(_ => ResetZoom());
            RotateLeftCommand = new RelayCommand(_ => RotateLeft());
            RotateRightCommand = new RelayCommand(_ => RotateRight());
            SelectPanelCommand = new RelayCommand(param => SelectPanel(param as string));
            ClosePanelCommand = new RelayCommand(_ => ClosePanel());
            TogglePanelCommand = new RelayCommand(param => TogglePanel(param as string));
            InsertTextPlaceholderCommand = new RelayCommand(_ => InsertTextPlaceholder());
            InsertImagePlaceholderCommand = new RelayCommand(_ => InsertImagePlaceholder());
            RefreshPlaceholdersCommand = new RelayCommand(_ => RefreshPlaceholders());
            GenerateMockDataCommand = new RelayCommand(_ => GenerateMockData());
            ImportDataCommand = new RelayCommand(_ => ImportData());
            ExportDataCommand = new RelayCommand(_ => ExportData());
            AboutCommand = new RelayCommand(_ => ShowAbout());
            HelpCommand = new RelayCommand(_ => ShowHelp());
            
            // 属性变更通知
            PropertyChanged += (s, e) => {
                if (e.PropertyName == nameof(ActivePanel))
                {
                    OnPropertyChanged(nameof(IsPreviewPanelActive));
                    OnPropertyChanged(nameof(IsPlaceholdersPanelActive));
                    OnPropertyChanged(nameof(IsMockDataPanelActive));
                }
            };
            
            // 初始化示例模板
            _editorText = GetDefaultSvgTemplate();
        }
        
        // 设置编辑器实例
        public void SetEditor(ICSharpCode.AvalonEdit.TextEditor editor)
        {
            _editor = editor;
        }
        
        // 初始化
        public async Task InitializeAsync()
        {
            // 这里可以添加初始化逻辑，如加载模板列表等
            AvailableTemplates.Add("基本模板");
            AvailableTemplates.Add("朋友圈动态");
            AvailableTemplates.Add("评论回复");
            
            SelectedTemplate = AvailableTemplates.Count > 0 ? AvailableTemplates[0] : null;
            
            await RefreshPreviewAsync();
        }
        
        // 获取默认SVG模板
        private string GetDefaultSvgTemplate()
        {
            return @"<svg width=""500"" height=""400"">
  <image x=""20"" y=""20"" width=""60"" height=""60""
         href=""{{img:avatar}}""
         clip-path=""url(#avatarClip)"" />
         
  <text x=""100"" y=""40"">{{userName}}</text>
  
  <text x=""100"" y=""70"" font-size=""14"">{{publishTime}}</text>
  
  <text x=""20"" y=""120"">{{content}}</text>
  
  <image x=""20"" y=""150"" width=""460"" height=""200""
         href=""{{img:photo}}"" />
         
  <text x=""30"" y=""380"">♥ {{likeCount}}</text>
  
  <text x=""150"" y=""380"">💬 {{commentCount}}</text>
</svg>";
        }
        
        // 面板相关方法
        private void SelectPanel(string parameter)
        {
            if (Enum.TryParse<PanelType>(parameter, out var panelType))
            {
                ActivePanel = panelType;
            }
        }
        
        private void ClosePanel()
        {
            IsPanelVisible = false;
        }
        
        private void TogglePanel(string parameter)
        {
            if (Enum.TryParse<PanelType>(parameter, out var panelType))
            {
                if (IsPanelVisible && ActivePanel == panelType)
                {
                    IsPanelVisible = false;
                }
                else
                {
                    IsPanelVisible = true;
                    ActivePanel = panelType;
                }
            }
        }
        
        // 预览相关方法
        private void ZoomIn()
        {
            PreviewZoom = Math.Min(PreviewZoom * 1.25, 5.0);
            ZoomLevel = (int)(PreviewZoom * 100);
        }
        
        private void ZoomOut()
        {
            PreviewZoom = Math.Max(PreviewZoom * 0.8, 0.1);
            ZoomLevel = (int)(PreviewZoom * 100);
        }
        
        private void ResetZoom()
        {
            PreviewZoom = 1.0;
            ZoomLevel = 100;
        }
        
        private void RotateLeft()
        {
            PreviewRotation = (PreviewRotation - 90) % 360;
        }
        
        private void RotateRight()
        {
            PreviewRotation = (PreviewRotation + 90) % 360;
        }
        
        // 预览更新
        // 更新预览刷新方法
        private async Task RefreshPreviewAsync()
        {
            try
            {
                DocumentStatus = "渲染中...";
        
                // 使用现有的 SVG 渲染器
                object _svgRenderer;
                PreviewImage = await _svgRenderer.RenderSvgAsync(EditorText);
        
                // 解析并统计占位符
                CountPlaceholders();
        
                DocumentStatus = "就绪";
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "SVG 预览渲染失败");
                DocumentStatus = "渲染错误";
            }
        }
        
        // 占位符和图片计数
        private void CountPlaceholders()
        {
            string text = EditorText ?? "";
            
            // 简单的占位符统计 {{name}}
            int textPlaceholders = 0;
            int imagePlaceholders = 0;
            
            int startIndex = 0;
            while ((startIndex = text.IndexOf("{{", startIndex)) >= 0)
            {
                int endIndex = text.IndexOf("}}", startIndex + 2);
                if (endIndex >= 0)
                {
                    string placeholder = text.Substring(startIndex + 2, endIndex - startIndex - 2).Trim();
                    if (placeholder.StartsWith("img:") || placeholder.StartsWith("avatar:") || 
                        placeholder.StartsWith("bg:") || placeholder.StartsWith("pattern:"))
                    {
                        imagePlaceholders++;
                    }
                    else
                    {
                        textPlaceholders++;
                    }
                    
                    startIndex = endIndex + 2;
                }
                else
                {
                    break;
                }
            }
            
            PlaceholderCount = textPlaceholders + imagePlaceholders;
            ImageCount = imagePlaceholders;
        }
        
        // 编辑器相关方法
        private bool CanUndo() => _editor?.CanUndo ?? false;
        private bool CanRedo() => _editor?.CanRedo ?? false;
        
        private void Undo()
        {
            _editor?.Undo();
        }
        
        private void Redo()
        {
            _editor?.Redo();
        }
        
        // 在光标位置插入文本
        public void InsertTextAtCursor(string text)
        {
            if (_editor != null && !string.IsNullOrEmpty(text))
            {
                _editor.Document.Insert(_editor.CaretOffset, text);
            }
        }
        
        // 占位符相关
        private void InsertTextPlaceholder()
        {
            // 这里应该弹出对话框让用户选择要插入的文本占位符
            // 现在暂时插入一个示例
            InsertTextAtCursor("{{placeholderName}}");
        }
        
        private void InsertImagePlaceholder()
        {
            // 这里应该弹出对话框让用户选择要插入的图片占位符
            // 现在暂时插入一个示例
            InsertTextAtCursor("{{img:imageName}}");
        }
        
        private void RefreshPlaceholders()
        {
            // TODO: 实现刷新占位符逻辑
        }
        
        // 数据相关
        private void GenerateMockData()
        {
            // TODO: 实现生成模拟数据逻辑
        }
        
        private void ImportData()
        {
            // TODO: 实现导入数据逻辑
        }
        
        private void ExportData()
        {
            // TODO: 实现导出数据逻辑
        }
        
        // 文件操作
        private void CreateNewTemplate()
        {
            EditorText = GetDefaultSvgTemplate();
            DocumentStatus = "新建文档";
        }
        
        private void OpenTemplate()
        {
            // TODO: 实现打开模板逻辑
        }
        
        private void SaveTemplate()
        {
            // TODO: 实现保存模板逻辑
            DocumentStatus = "已保存";
        }
        
        private void SaveAsTemplate()
        {
            // TODO: 实现另存为逻辑
        }
        
        // 帮助和其他
        private void ShowFindReplace()
        {
            // TODO: 实现查找替换对话框
        }
        
        private void ShowAbout()
        {
            MessageBox.Show("SVG模板编辑器 v1.0\n© 2025 微信朋友圈模拟器", "关于", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        
        private void ShowHelp()
        {
            // TODO: 实现帮助文档显示
        }
        
        private void Exit()
        {
            // 关闭窗口
            Application.Current.Windows[0]?.Close();
        }
        
        // INotifyPropertyChanged 实现
        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
                return false;
            
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }
    }
}