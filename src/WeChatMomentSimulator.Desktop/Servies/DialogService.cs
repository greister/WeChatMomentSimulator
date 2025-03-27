using System;
using Microsoft.Extensions.Logging;
using WeChatMomentSimulator.Core.Interfaces.Services;
using System.Windows; // Requires PresentationFramework reference

namespace WeChatMomentSimulator.Desktop.Services
{
    /// <summary>
    /// 对话框服务的实现类
    /// </summary>
    public class DialogService : IDialogService
    {
        private readonly ILogger<DialogService> _logger;

        /// <summary>
        /// 构造函数
        /// </summary>
        public DialogService(ILogger<DialogService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// 显示确认对话框
        /// </summary>
        public CustomDialogResult ShowConfirmation(string message, string title, CustomDialogButton buttons = CustomDialogButton.YesNo)
        {
            _logger.LogInformation("显示确认对话框: {Title}", title);
            var wpfButtons = ConvertToMessageBoxButton(buttons);
            var result = System.Windows.MessageBox.Show(message, title, wpfButtons, System.Windows.MessageBoxImage.Question);
            return ConvertToDialogResult(result);
        }

        /// <summary>
        /// 显示信息对话框
        /// </summary>
        public void ShowInformation(string title, string message)
        {
            _logger.LogInformation("显示信息对话框: {Title}", title);
            System.Windows.MessageBox.Show(message, title, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }

        /// <summary>
        /// 显示警告对话框
        /// </summary>
        public void ShowWarning(string title, string message)
        {
            _logger.LogWarning("显示警告对话框: {Title}", title);
            System.Windows.MessageBox.Show(message, title, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
        }

        /// <summary>
        /// 显示错误对话框
        /// </summary>
        public void ShowError(string title, string message)
        {
            _logger.LogError("显示错误对话框: {Title}", title);
            System.Windows.MessageBox.Show(message, title, System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }

        /// <summary>
        /// 显示打开文件对话框
        /// </summary>
        public string ShowOpenFileDialog(string title, string filter)
        {
            _logger.LogInformation("显示打开文件对话框: {Title}", title);

            try
            {
                var dialog = new Microsoft.Win32.OpenFileDialog
                {
                    Title = title,
                    Filter = filter,
                    CheckFileExists = true
                };

                if (dialog.ShowDialog() == true)
                {
                    _logger.LogInformation("用户选择了文件: {FilePath}", dialog.FileName);
                    return dialog.FileName;
                }

                _logger.LogInformation("用户取消了打开文件操作");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "打开文件对话框出错");
                throw;
            }
        }

        /// <summary>
        /// 显示保存文件对话框
        /// </summary>
        public string ShowSaveFileDialog(string title, string filter, string defaultExt = null)
        {
            _logger.LogInformation("显示保存文件对话框: {Title}", title);

            try
            {
                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    Title = title,
                    Filter = filter,
                    DefaultExt = defaultExt
                };

                if (dialog.ShowDialog() == true)
                {
                    _logger.LogInformation("用户选择了保存路径: {FilePath}", dialog.FileName);
                    return dialog.FileName;
                }

                _logger.LogInformation("用户取消��保存文件操作");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "保存文件对话框出错");
                throw;
            }
        }

        /// <summary>
        /// 将平台无关的按钮类型转换为WPF按钮类型
        /// </summary>
        private System.Windows.MessageBoxButton ConvertToMessageBoxButton(CustomDialogButton button)
        {
            return button switch
            {
                CustomDialogButton.OK => System.Windows.MessageBoxButton.OK,
                CustomDialogButton.OKCancel => System.Windows.MessageBoxButton.OKCancel,
                CustomDialogButton.YesNo => System.Windows.MessageBoxButton.YesNo,
                CustomDialogButton.YesNoCancel => System.Windows.MessageBoxButton.YesNoCancel,
                _ => System.Windows.MessageBoxButton.OK
            };
        }

        /// <summary>
        /// 将WPF对话框结果转换为平台无关的对话框结果
        /// </summary>
        private CustomDialogResult ConvertToDialogResult(System.Windows.MessageBoxResult result)
        {
            return result switch
            {
                System.Windows.MessageBoxResult.None => CustomDialogResult.None,
                System.Windows.MessageBoxResult.OK => CustomDialogResult.OK,
                System.Windows.MessageBoxResult.Cancel => CustomDialogResult.Cancel,
                System.Windows.MessageBoxResult.Yes => CustomDialogResult.Yes,
                System.Windows.MessageBoxResult.No => CustomDialogResult.No,
                _ => CustomDialogResult.None
            };
        }
    }
}