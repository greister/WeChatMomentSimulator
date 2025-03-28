using System;
using System.Windows;
using Microsoft.Xaml.Behaviors;
using ICSharpCode.AvalonEdit;

namespace WeChatMomentSimulator.Desktop.Views.Controls
{
    /// <summary>
    /// AvalonEdit文本编辑器的绑定行为
    /// </summary>
    public sealed class AvalonEditBehavior : Behavior<TextEditor>
    {
        private bool _isUpdating;
        private bool _isUpdatingText;
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(AvalonEditBehavior),
                new FrameworkPropertyMetadata(default(string), FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, PropertyChangedCallback));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            if (AssociatedObject != null)
            {
                AssociatedObject.TextChanged += AssociatedObjectOnTextChanged;
                UpdateText();
            }
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            if (AssociatedObject != null)
            {
                AssociatedObject.TextChanged -= AssociatedObjectOnTextChanged;
            }
        }

        private void AssociatedObjectOnTextChanged(object sender, EventArgs eventArgs)
        {
            if (_isUpdating)
                return;

            _isUpdating = true;
            try
            {
                Text = AssociatedObject.Text;
            }
            finally
            {
                _isUpdating = false;
            }
        }

        private void UpdateText()
        {
            if (_isUpdating)
                return;

            _isUpdating = true;
            try
            {
                // 确保在设置文本前关闭所有撤销组
                if (AssociatedObject.Document != null)
                {
                    // 将Document转换为IDocument接口
                    var document = AssociatedObject.Document as ICSharpCode.AvalonEdit.Document.IDocument;
                    if (document != null)
                    {
                        using (document.OpenUndoGroup())
                        {
                            // 故意在这里不做任何操作，只是确保撤销组正确关闭
                        }
                    }
                }

                // 清理当前的撤销栈
                AssociatedObject.Document?.UndoStack?.ClearAll();

                // 设置文本
                AssociatedObject.Text = Text ?? string.Empty;
            }
            finally
            {
                _isUpdating = false;
            }
        }
        
        

        private static void PropertyChangedCallback(DependencyObject dependencyObject,
                    DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var behavior = dependencyObject as AvalonEditBehavior;
            if (behavior != null)
            {
                behavior.UpdateText();
            }
        }

    
    }
}