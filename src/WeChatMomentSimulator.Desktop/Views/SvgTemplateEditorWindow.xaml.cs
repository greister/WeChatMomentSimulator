using System.Windows;
using System.Windows.Media;
using WeChatMomentSimulator.Desktop.ViewModels;
using WeChatMomentSimulator.Desktop.ViewModels;

namespace WeChatMomentSimulator.Desktop.Views
{
    public partial class SvgTemplateEditorWindow : Window
    {
        public SvgTemplateEditorViewModel ViewModel { get; }

        public SvgTemplateEditorWindow()
        {
            InitializeComponent();
            ViewModel = new SvgTemplateEditorViewModel();
            DataContext = ViewModel;
            // 确保变换绑定正确
            ZoomTransform.ScaleX = ViewModel.ZoomLevel;
            ZoomTransform.ScaleY = ViewModel.ZoomLevel;
            Loaded += OnWindowLoaded;
        }



        private void OnWindowLoaded(object sender, RoutedEventArgs e)
        {
            // 初始化DPI自适应
            UpdateDpiScaling();
            
            // 示例：加载默认模板
            //ViewModel.LoadSvgFile("Resources/DefaultTemplate.svg");
        }

        private void UpdateDpiScaling()
        {
            var dpi = VisualTreeHelper.GetDpi(this);
            ViewModel.ZoomLevel *= dpi.DpiScaleX;
        }
    }
}