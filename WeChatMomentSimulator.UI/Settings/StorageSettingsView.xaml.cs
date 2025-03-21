using System.Windows.Controls;
using WeChatMomentSimulator.UI.ViewModels;

namespace WeChatMomentSimulator.UI.Settings
{
    /// <summary>
    /// StorageSettingsView.xaml 的交互逻辑
    /// </summary>
    public partial class StorageSettingsView : UserControl
    {
        public StorageSettingsView(StorageSettingsViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
        }
    }
}