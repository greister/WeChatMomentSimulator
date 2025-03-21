using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using WeChatMomentSimulator.Core.Models.Template;

namespace WeChatMomentSimulator.UI.Controls
{
    /// <summary>
    /// TemplateThumbnail.xaml 的交互逻辑
    /// </summary>
    public partial class TemplateThumbnail : UserControl, INotifyPropertyChanged
    {
        #region 依赖属性

        /// <summary>
        /// 模板依赖属性
        /// </summary>
        public static readonly DependencyProperty TemplateProperty =
            DependencyProperty.Register("Template", typeof(TemplateDefinition), typeof(TemplateThumbnail),
                new PropertyMetadata(null, OnTemplateChanged));

        /// <summary>
        /// 是否选中依赖属性
        /// </summary>
        public static readonly DependencyProperty IsSelectedProperty =
            DependencyProperty.Register("IsSelected", typeof(bool), typeof(TemplateThumbnail),
                new PropertyMetadata(false));

        /// <summary>
        /// 命令依赖属性
        /// </summary>
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register("Command", typeof(ICommand), typeof(TemplateThumbnail));

        /// <summary>
        /// 命令参数依赖属性
        /// </summary>
        public static readonly DependencyProperty CommandParameterProperty =
            DependencyProperty.Register("CommandParameter", typeof(object), typeof(TemplateThumbnail));

        #endregion

        #region 属性

        /// <summary>
        /// 模板对象
        /// </summary>
        public TemplateDefinition Template
        {
            get => (TemplateDefinition)GetValue(TemplateProperty);
            set => SetValue(TemplateProperty, value);
        }

        /// <summary>
        /// 是否选中
        /// </summary>
        public bool IsSelected
        {
            get => (bool)GetValue(IsSelectedProperty);
            set => SetValue(IsSelectedProperty, value);
        }

        /// <summary>
        /// 点击命令
        /// </summary>
        public ICommand Command
        {
            get => (ICommand)GetValue(CommandProperty);
            set => SetValue(CommandProperty, value);
        }

        /// <summary>
        /// 命令参数
        /// </summary>
        public object CommandParameter
        {
            get => GetValue(CommandParameterProperty);
            set => SetValue(CommandParameterProperty, value);
        }

        private BitmapImage _thumbnailSource;
        /// <summary>
        /// 缩略图源
        /// </summary>
        public BitmapImage ThumbnailSource
        {
            get => _thumbnailSource;
            set
            {
                _thumbnailSource = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasThumbnail));
            }
        }

        /// <summary>
        /// 是否有缩略图
        /// </summary>
        public bool HasThumbnail => ThumbnailSource != null;

        /// <summary>
        /// 修改日期文本
        /// </summary>
        public string ModifiedDateText
        {
            get
            {
                if (Template?.ModifiedDate == null || Template.ModifiedDate == default)
                    return string.Empty;

                var date = Template.ModifiedDate;
                var now = DateTime.Now;

                if (date.Date == now.Date)
                    return $"今天 {date:HH:mm}";
                if (date.Date == now.Date.AddDays(-1))
                    return $"昨天 {date:HH:mm}";
                if (now.Year == date.Year)
                    return date.ToString("MM-dd HH:mm");

                return date.ToString("yyyy-MM-dd");
            }
        }

        #endregion

        public TemplateThumbnail()
        {
            InitializeComponent();
            DataContext = this;
        }

        private static void OnTemplateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TemplateThumbnail thumbnail && e.NewValue is TemplateDefinition template)
            {
                thumbnail.LoadThumbnail(template);
                thumbnail.OnPropertyChanged(nameof(ModifiedDateText));
            }
        }

        private void LoadThumbnail(TemplateDefinition template)
        {
            try
            {
                // 如果模板中没有缩略图路径，不加载
                if (string.IsNullOrEmpty(template.Metadata?.ThumbnailPath) ||
                    !File.Exists(template.Metadata.ThumbnailPath))
                {
                    ThumbnailSource = null;
                    return;
                }

                // 创建位图图像
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(template.Metadata.ThumbnailPath);
                bitmap.EndInit();
                bitmap.Freeze(); // 提高性能

                ThumbnailSource = bitmap;
            }
            catch (Exception ex)
            {
                // 加载失败，清除缩略图
                ThumbnailSource = null;
                Console.WriteLine($"加载缩略图失败: {ex.Message}");
            }
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}