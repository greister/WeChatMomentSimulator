using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using WeChatMomentSimulator.Core.Logging;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using ICSharpCode.AvalonEdit.Highlighting;
using Microsoft.Extensions.Logging;
using Serilog;
using WeChatMomentSimulator.Core.Interfaces.Services;
using WeChatMomentSimulator.Core.Models.Template;
using WeChatMomentSimulator.Desktop.Rendering;
using WeChatMomentSimulator.Services.Services;
using ILogger = Serilog.ILogger;
using LoggerExtensions = WeChatMomentSimulator.Core.Logging.LoggerExtensions;

namespace WeChatMomentSimulator.Desktop.Views
{
    /// <summary>
    /// SVG模板编辑器界面交互逻辑
    /// </summary>
    public partial class SvgTemplateEditor : Window
    {
        private readonly ITemplateManager _templateManager;
        private readonly SvgRenderer _svgRenderer;
        private Dictionary<string, object> _placeholderData;
        private System.Windows.Threading.DispatcherTimer _refreshTimer;
        private bool _autoRefresh = false;
        //添加日志记录
        private readonly ILogger<SvgTemplateEditor> _logger;
        private readonly ILogger _logger2 = Log.ForContext<SvgTemplateEditor>();
        
        public SvgTemplateEditor()
        {
            InitializeComponent();
            _logger = LoggerExtensions.GetLogger<SvgTemplateEditor>();
            // 初始化渲染器和模板管理器
            _templateManager = new TemplateManager();
            _svgRenderer = new SvgRenderer();
            
            // 创建防抖动计时器
            _refreshTimer = new System.Windows.Threading.DispatcherTimer();
            _refreshTimer.Interval = TimeSpan.FromMilliseconds(500); // 500ms防抖动
            _refreshTimer.Tick += (s, e) => 
            {
                _refreshTimer.Stop();
                RefreshPreview();
            };
            
            // 初始化占位符数据
            InitPlaceholderData();
            
            // 加载模板列表
            LoadTemplateList();
            
            // 创建默认模板（如果需要）
            _templateManager.CreateDefaultTemplatesAsync();
        }
        
        /// <summary>
        /// 初始化占位符数据
        /// </summary>
        private void InitPlaceholderData()
        {
            _placeholderData = new Dictionary<string, object>
            {
                ["userName"] = "张三",
                ["time"] = "15:30",
                ["battery"] = "87%",
                ["content"] = "这是一条朋友圈测试内容...",
                ["hasImages"] = true,
                ["likes"] = 12,
                ["timeText"] = "10分钟前"
            };
        }
        
        /// <summary>
        /// 从UI控件更新占位符数据
        /// </summary>
        private void UpdatePlaceholderDataFromUI()
        {
            _placeholderData["userName"] = UserNameTextBox.Text;
            _placeholderData["time"] = TimeTextBox.Text;
            _placeholderData["battery"] = BatteryTextBox.Text;
            _placeholderData["content"] = ContentTextBox.Text;
            _placeholderData["hasImages"] = HasImagesCheckBox.IsChecked ?? false;
            
            if (int.TryParse(LikesTextBox.Text, out int likes))
            {
                _placeholderData["likes"] = likes;
            }
        }
        
        /// <summary>
        /// 加载模板列表
        /// </summary>
        private void LoadTemplateList()
        {
            try
            {
                TemplateSelector.Items.Clear();
                //away  the tasy to get actual IEnumerable<Template>
               var templates = _templateManager.GetTemplatesAsync().Result;
                foreach (var templateName in templates)
                {
                    TemplateSelector.Items.Add(templateName);
                }
                
                if (TemplateSelector.Items.Count > 0)
                {
                    TemplateSelector.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"加载模板列表失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        /// <summary>
        /// 刷新预览
        /// </summary>
  private async void RefreshPreview()
{
    try
    {
        // 更新占位符数据
        UpdatePlaceholderDataFromUI();
        
        // 获取编辑器内容
        string svgContent = CodeEditor.Text;
        if (string.IsNullOrWhiteSpace(svgContent))
            return;
            
        // 处理SVG内容（替换占位符）
        string processedSvg = _svgRenderer.ProcessTemplate(svgContent, _placeholderData);
        
        // 临时实现：显示占位图像
        _logger.LogInformation("log1显示占位图像");
        _logger2.Information("log2显示占位图像");
        
        // 确定目标目录路径
        string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
        string targetDirectory = System.IO.Path.Combine(baseDirectory, "Resources", "Templates");
        
        // 确保目录存在
        if (!Directory.Exists(targetDirectory))
        {
            Directory.CreateDirectory(targetDirectory);
            _logger.LogInformation("已创建目录: {DirectoryPath}", targetDirectory);
        }
        
        // 文件完整路径
        string imagePath = System.IO.Path.Combine(targetDirectory, "flag.png");
        
        // 检查文件是否已存在，如果不存在则创建
        if (!File.Exists(imagePath))
        {
            // 创建一个简单的1x1像素的PNG图片
            using (var bitmap = new System.Drawing.Bitmap(100, 100))
            {
                using (var g = System.Drawing.Graphics.FromImage(bitmap))
                {
                    g.Clear(System.Drawing.Color.Red);
                    // 绘制一个简单的旗帜图案
                    g.FillRectangle(System.Drawing.Brushes.Blue, 0, 0, 50, 50);
                }
                bitmap.Save(imagePath, System.Drawing.Imaging.ImageFormat.Png);
            }
            _logger.LogInformation("已创建图像文件: {FilePath}", imagePath);
        }
        else
        {
            _logger.LogInformation("文件已存在，无需创建: {FilePath}", imagePath);
        }
        
        // 将文件路径转换为BitmapImage
        var bitmapImage = new BitmapImage();
        bitmapImage.BeginInit();
        bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
        bitmapImage.UriSource = new Uri(imagePath);
        bitmapImage.EndInit();
        
        _logger.LogInformation("显示占位图像： {imagePath}", imagePath);
        PreviewImage.Source = bitmapImage;
    }
    catch (Exception ex)
    {
        MessageBox.Show($"更新预览失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
        
        #region 事件处理
        
        /// <summary>
        /// 模板选择发生变化
        /// </summary>
        private void TemplateSelector_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (TemplateSelector.SelectedItem != null)
            {
                try
                {
                    string selectedTemplate = TemplateSelector.SelectedItem.ToString();
                    Task<Template> templateContent = _templateManager.GetTemplateByNameAsync(selectedTemplate);
                    CodeEditor.Text = templateContent.ToString();
                    RefreshPreview();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"加载模板失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        
        /// <summary>
        /// 刷新预览按钮点击
        /// </summary>
        private void RefreshPreview_Click(object sender, RoutedEventArgs e)
        {
            RefreshPreview();
        }
        
        /// <summary>
        /// 新建模板按钮点击
        /// </summary>
        private void NewTemplate_Click(object sender, RoutedEventArgs e)
        {
            // 创建输入对话框
            var dialog = new InputDialog("新建模板", "请输入模板名称:");
            
            if (dialog.ShowDialog() == true && !string.IsNullOrWhiteSpace(dialog.InputValue))
            {
                string templateName = dialog.InputValue;
                
                try
                {
                    // 创建基础模板
                    string basicTemplate = @"<svg width=""1080"" height=""1920"" viewBox=""0 0 1080 1920"">
    <!-- 基本模板 -->
    <rect width=""1080"" height=""1920"" fill=""#f6f6f6""/>
    
    <!-- 状态栏 -->
    <g transform=""translate(0,0)"">
        <rect width=""1080"" height=""80"" fill=""#333333""/>
        <text x=""540"" y=""50"" font-family=""Arial"" font-size=""36"" fill=""white"" text-anchor=""middle"">{{time}}</text>
        <text x=""980"" y=""50"" font-family=""Arial"" font-size=""32"" fill""white"" text-anchor=""end"">{{battery}}</text>
    </g>
    
    <!-- 朋友圈内容区 -->
    <g transform""translate(0,200)"">
        <text x""100"" y""80"" font-family""Arial"" font-size""42"" fill""#333333"" font-weight""bold"">{{userName}}</text>
        <text x""100"" y""150"" font-family""Arial"" font-size""36"" fill""#333333"" width""880"">{{content}}</text>
    </g>
</svg>";
                    
                    _templateManager.SaveTemplateAsync(templateName, basicTemplate);
                    
                    // 刷新模板列表并选择新模板
                    LoadTemplateList();
                    TemplateSelector.SelectedItem = templateName;
                    
                    MessageBox.Show($"模板'{templateName}'已创建!", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"创建模板失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
        
        /// <summary>
        /// 保存模板按钮点击
        /// </summary>
        private void SaveTemplate_Click(object sender, RoutedEventArgs e)
        {
            if (TemplateSelector.SelectedItem == null)
            {
                MessageBox.Show("请先选择模板或创建新模板", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            
            try
            {
                string templateName = TemplateSelector.SelectedItem.ToString();
                string templateContent = CodeEditor.Text;
                
                _templateManager.SaveTemplateAsync(templateName, templateContent);
                MessageBox.Show($"模板{templateName}'已保存!", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存模板失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        /// <summary>
        /// 导出图片按钮点击
        /// </summary>
        private async void ExportImage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // 更新占位符数据
                UpdatePlaceholderDataFromUI();
                
                var saveDialog = new SaveFileDialog
                {
                    Filter = "PNG图像|*.png|JPEG图像|*.jpg",
                    DefaultExt = ".png",
                    Title = "导出朋友圈图像"
                };
                
                if (saveDialog.ShowDialog() == true)
                {
                    string svgContent = CodeEditor.Text;
                    string processedSvg = _svgRenderer.ProcessTemplate(svgContent, _placeholderData);
                    
                    ImageFormat format = Path.GetExtension(saveDialog.FileName).ToLower() == ".jpg" 
                        ? ImageFormat.JPEG 
                        : ImageFormat.PNG;
                        
                    byte[] imageData = await _svgRenderer.RenderToImage(processedSvg, 1080, 1920, format);
                    
                    // 保存图像（实际实现会保存真实的渲染结果）
                    File.WriteAllBytes(saveDialog.FileName, imageData);
                    
                    MessageBox.Show("图像导出成功!", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"导出失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        /// <summary>
        /// 自动刷新选项更改
        /// </summary>
        private void AutoRefreshCheckBox_CheckedChanged(object sender, RoutedEventArgs e)
        {
            _autoRefresh = AutoRefreshCheckBox.IsChecked ?? false;
        }
        
        /// <summary>
        /// 输入改变事件
        /// </summary>
        private void InputChanged(object sender, EventArgs e)
        {
            if (_autoRefresh)
            {
                _refreshTimer.Stop();
                _refreshTimer.Start();
            }
        }
        
        /// <summary>
        /// 缩放滑块值改变
        /// </summary>
        private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (PhoneFrame == null) return;
            
            double scale = e.NewValue / 100.0;
            PhoneFrame.RenderTransform = new ScaleTransform(scale, scale);
        }
        
        /// <summary>
        /// 生成随机内容
        /// </summary>
        private void GenerateRandomContent_Click(object sender, RoutedEventArgs e)
        {
            string[] randomContents = {
                "今天天气真好，出去走走放松一下~",
                "新买的相机终于到了，迫不及待想试试效果！",
                "周末和朋友去爬山，风景太美了，分享给大家。",
                "刚做的蛋糕，第一次尝试，还不错吧？",
                "读完这本书，感触良多，推荐给大家！"
            };
            
            Random random = new Random();
            ContentTextBox.Text = randomContents[random.Next(randomContents.Length)];
        }
        
        #endregion
    }
    
    /// <summary>
    /// 输入对话框
    /// </summary>
    public class InputDialog : Window
    {
        public string InputValue { get; private set; }
        
        public InputDialog(string title, string prompt)
        {
            Title = title;
            Width = 350;
            Height = 150;
            WindowStartupLocation = WindowStartupLocation.CenterOwner;
            ResizeMode = ResizeMode.NoResize;
            
            var grid = new Grid { Margin = new Thickness(10) };
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
            
            var promptText = new TextBlock { Text = prompt, Margin = new Thickness(0, 0, 0, 5) };
            Grid.SetRow(promptText, 0);
            
            var inputBox = new TextBox { Margin = new Thickness(0, 0, 0, 10) };
            Grid.SetRow(inputBox, 1);
            
            var buttonPanel = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
            
            var okButton = new Button { Content = "确定", Width = 75, Margin = new Thickness(0, 0, 10, 0) };
            okButton.Click += (s, e) => 
            {
                InputValue = inputBox.Text;
                DialogResult = true;
            };
            
            var cancelButton = new Button { Content = "取消", Width = 75 };
            cancelButton.Click += (s, e) => DialogResult = false;
            
            buttonPanel.Children.Add(okButton);
            buttonPanel.Children.Add(cancelButton);
            Grid.SetRow(buttonPanel, 2);
            
            grid.Children.Add(promptText);
            grid.Children.Add(inputBox);
            grid.Children.Add(buttonPanel);
            
            Content = grid;
        }
    }
}