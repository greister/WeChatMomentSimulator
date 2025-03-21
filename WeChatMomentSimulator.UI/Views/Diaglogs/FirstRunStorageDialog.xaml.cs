using System;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using WeChatMomentSimulator.Core.Interfaces;
using MessageBox = System.Windows.MessageBox;

namespace WeChatMomentSimulator.UI.Views.Dialogs
{
    /// <summary>
    /// FirstRunStorageDialog.xaml 的交互逻辑
    /// </summary>
    public partial class FirstRunStorageDialog : Window
    {
        private readonly IPathService _pathService;

        public bool DoNotShowAgain { get; set; }
        public string DefaultStoragePath { get; private set; }

        public FirstRunStorageDialog(IPathService pathService)
        {
            _pathService = pathService ?? throw new ArgumentNullException(nameof(pathService));
            DefaultStoragePath = _pathService.GetRootDirectory();

            InitializeComponent();
            DataContext = this;
        }

        /// <summary>
        /// 当用户点击"更改位置"按钮时执行
        /// </summary>
        private void ChangeLocation_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using var dialog = new FolderBrowserDialog
                {
                    Description = "选择数据存储位置",
                    UseDescriptionForTitle = true,
                    SelectedPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
                };

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string newPath = Path.Combine(dialog.SelectedPath, "WeChatMomentSimulator");

                    // 更新路径
                    bool success = _pathService.SetRootDirectory(newPath, true);

                    if (success)
                    {
                        DefaultStoragePath = newPath;
                        // 通知UI更新
                        OnPropertyChanged(nameof(DefaultStoragePath));
                        MessageBox.Show($"存储位置已更改为:\n{newPath}", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("无法更改存储位置，请检查权限或磁盘空间。", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"更改位置时出错: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// 当用户点击"确定"按钮时执行
        /// </summary>
        private void OK_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        // 实现INotifyPropertyChanged接口的简化方法
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }

        public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;
    }
}