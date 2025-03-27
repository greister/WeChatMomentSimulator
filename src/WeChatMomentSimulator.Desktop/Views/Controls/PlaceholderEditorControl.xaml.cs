using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.Logging;
using Serilog;
using ILogger = Serilog.ILogger;

namespace WeChatMomentSimulator.Desktop.Views.Controls;

public partial class PlaceholderEditorControl : UserControl
{
    //日志器
    private readonly ILogger _logger = Log.ForContext<PlaceholderEditorControl>();
    public PlaceholderEditorControl()
    {
        InitializeComponent();
        _logger.Information("PlaceholderEditorControl 启动");
    }
}