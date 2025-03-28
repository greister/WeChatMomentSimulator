using System;
using System.Windows;
using Microsoft.Xaml.Behaviors;
using ICSharpCode.AvalonEdit;

namespace WeChatMomentSimulator.Desktop.Behaviors
{
    public class AvalonEditBehavior : Behavior<TextEditor>
    {
        public static readonly DependencyProperty TextProperty = 
            DependencyProperty.Register("Text", typeof(string), typeof(AvalonEditBehavior), 
            new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, 
                                        PropertyChangedCallback));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        private static void PropertyChangedCallback(DependencyObject d, 
                                                  DependencyPropertyChangedEventArgs e)
        {
            var behavior = d as AvalonEditBehavior;
            if (behavior?.AssociatedObject != null)
            {
                var editor = behavior.AssociatedObject;
                
                // 防止光标位置重置
                int caretOffset = editor.CaretOffset;
                int line = editor.TextArea.Caret.Line;
                int column = editor.TextArea.Caret.Column;
                double verticalOffset = editor.VerticalOffset;
                
                editor.Text = (string)e.NewValue;
                
                // 尝试恢复光标位置
                try
                {
                    editor.TextArea.Caret.Line = line;
                    editor.TextArea.Caret.Column = column;
                    editor.ScrollToVerticalOffset(verticalOffset);
                }
                catch
                {
                    // 如果行/列不再存在，就不恢复位置
                }
            }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            if (AssociatedObject != null)
            {
                AssociatedObject.TextChanged += OnTextChanged;
                
                if (!string.IsNullOrEmpty(Text))
                {
                    AssociatedObject.Text = Text;
                }
            }
        }

        protected override void OnDetaching()
        {
            if (AssociatedObject != null)
            {
                AssociatedObject.TextChanged -= OnTextChanged;
            }
            base.OnDetaching();
        }

        private void OnTextChanged(object sender, EventArgs e)
        {
            if (sender is TextEditor editor)
            {
                Text = editor.Text;
            }
        }
    }
}