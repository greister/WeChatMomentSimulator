using System.Windows;
using WeChatMomentSimulator.Desktop.ViewModels;

namespace WeChatMomentSimulator.Desktop.Views
{
    /// <summary>
    /// SvgTemplateEditorWindow.xaml 的交互逻辑
    /// </summary>
    public partial class SvgTemplateEditorWindow : Window
    {
        public SvgTemplateEditorWindow(SvgTemplateEditorViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            
            Loaded += SvgTemplateEditorWindow_Loaded;
        }
        
        private async void SvgTemplateEditorWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // 确保视图模型初始化
            if (DataContext is SvgTemplateEditorViewModel viewModel)
            {
                await viewModel.InitializeAsync();
            }
        }
    }
}