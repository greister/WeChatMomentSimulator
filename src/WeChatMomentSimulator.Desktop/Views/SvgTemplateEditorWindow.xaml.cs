using System.Windows;
using ICSharpCode.AvalonEdit;
using WeChatMomentSimulator.Desktop.ViewModels;

namespace WeChatMomentSimulator.Desktop.Views
{
    public partial class SvgTemplateEditorWindow: Window
    {
        private readonly SvgTemplateEditorViewModel _viewModel;
        
        
        public SvgTemplateEditorWindow(SvgTemplateEditorViewModel viewModel)
        {
            InitializeComponent();

            _viewModel = viewModel;
            DataContext = _viewModel;
            
            Loaded += SvgTemplateEditorWindow_Loaded;
        }
        
        private async void SvgTemplateEditorWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // 传递编辑器实例给视图模型
            _viewModel.SetEditor(svgEditor);
            
            // 初始化
            await _viewModel.InitializeAsync();
            
            // 设置编辑器的事件处理
            svgEditor.TextArea.Caret.PositionChanged += (s, args) => {
                _viewModel.EditorPosition = $"{svgEditor.TextArea.Caret.Line}:{svgEditor.TextArea.Caret.Column}";
            };
        }
    }
}