namespace WeChatMomentSimulator.Core.Interfaces.Services
{
    /// <summary>
    /// Dialog service interface for displaying dialogs and messages.
    /// </summary>
    public interface IDialogService
    {
        /// <summary>
        /// Displays a confirmation dialog.
        /// </summary>
        /// <param name="message">The message content.</param>
        /// <param name="title">The title.</param>
        /// <param name="buttons">The button options.</param>
        /// <returns>The dialog result.</returns>
        CustomDialogResult ShowConfirmation(string message, string title, CustomDialogButton buttons = CustomDialogButton.YesNo);

        /// <summary>
        /// Displays an information dialog.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="message">The message content.</param>
        void ShowInformation(string title, string message);

        /// <summary>
        /// Displays a warning dialog.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="message">The warning content.</param>
        void ShowWarning(string title, string message);

        /// <summary>
        /// Displays an error dialog.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="message">The error content.</param>
        void ShowError(string title, string message);

        /// <summary>
        /// Displays an open file dialog.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="filter">The file filter.</param>
        /// <returns>The selected file path, or null if canceled.</returns>
        string ShowOpenFileDialog(string title, string filter);

        /// <summary>
        /// Displays a save file dialog.
        /// </summary>
        /// <param name="title">The title.</param>
        /// <param name="filter">The file filter.</param>
        /// <param name="defaultExt">The default file extension.</param>
        /// <returns>The saved file path, or null if canceled.</returns>
        string ShowSaveFileDialog(string title, string filter, string defaultExt = null);
    }

    /// <summary>
    /// Button options for dialogs.
    /// </summary>
    public enum CustomDialogButton
    {
        /// <summary>
        /// 确定按钮
        /// </summary>
        OK = 0,

        /// <summary>
        /// 确定和取消按钮
        /// </summary>
        OKCancel = 1,

        /// <summary>
        /// 是和否按钮
        /// </summary>
        YesNo = 2,

        /// <summary>
        /// 是、否和取消按钮
        /// </summary>
        YesNoCancel = 3
    }

    /// <summary>
    /// Result options for dialogs.
    /// </summary>
    public enum CustomDialogResult
    {
        /// <summary>
        /// 无结果
        /// </summary>
        None = 0,

        /// <summary>
        /// 确定
        /// </summary>
        OK = 1,

        /// <summary>
        /// 取消
        /// </summary>
        Cancel = 2,

        /// <summary>
        /// 是
        /// </summary>
        Yes = 6,

        /// <summary>
        /// 否
        /// </summary>
        No = 7
    }
}