using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using WeChatMomentSimulator.Core.Logging;

namespace WeChatMomentSimulator.UI.Views
{
    public partial class LogWindow : Window
    {
        public LogWindow()
        {
            InitializeComponent();
            MaxLines = 1000;
        }

        public int MaxLines { get; set; }

        public void AppendLog(LogEntry entry)
        {
            Dispatcher.Invoke(() =>
            {
                var paragraph = new Paragraph
                {
                    Margin = new Thickness(0)
                };

                // 时间戳
                paragraph.Inlines.Add(new Run($"[{entry.Timestamp:HH:mm:ss.fff}] ")
                {
                    Foreground = Brushes.Gray
                });

                // 日志级别
                paragraph.Inlines.Add(new Run($"[{entry.Level}] ")
                {
                    Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(entry.Color))
                });

                // 来源和实例
                paragraph.Inlines.Add(new Run($"[{entry.Source}:{entry.InstanceId}] ")
                {
                    Foreground = Brushes.DarkCyan
                });

                // 消息
                paragraph.Inlines.Add(new Run(entry.Message)
                {
                    Foreground = Brushes.Black
                });

                // 异常
                if (entry.Exception != null)
                {
                    paragraph.Inlines.Add(new LineBreak());
                    paragraph.Inlines.Add(new Run(entry.Exception.ToString())
                    {
                        Foreground = Brushes.DarkRed
                    });
                }

                LogTextBox.Document.Blocks.Add(paragraph);

                // 自动滚动
                LogTextBox.ScrollToEnd();

                // 限制最大行数
                if (LogTextBox.Document.Blocks.Count > MaxLines)
                {
                    LogTextBox.Document.Blocks.Remove(LogTextBox.Document.Blocks.FirstBlock);
                }
            });
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            LogTextBox.Document.Blocks.Clear();
        }

        private void LogLevelFilter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // 实现日志级别过滤
            // 可以根据选择的级别过滤显示的日志
        }
    }
}