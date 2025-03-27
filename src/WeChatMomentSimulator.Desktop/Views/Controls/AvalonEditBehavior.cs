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
            if (AssociatedObject != null && !_isUpdatingText)
            {
                Text = AssociatedObject.Text;
            }
        }
        
        private bool _isUpdatingText;

        private static void PropertyChangedCallback(DependencyObject dependencyObject,
                    DependencyPropertyChangedEventArgs dependencyPropertyChangedEventArgs)
        {
            var behavior = dependencyObject as AvalonEditBehavior;
            if (behavior != null)
            {
                behavior.UpdateText();
            }
        }

        private void UpdateText()
        {
            if (AssociatedObject != null && !_isUpdatingText)
            {
                _isUpdatingText = true;
                try
                {
                    var caretOffset = AssociatedObject.CaretOffset;
                    AssociatedObject.Text = Text ?? string.Empty;
                    if (caretOffset <= AssociatedObject.Text.Length)
                    {
                        AssociatedObject.CaretOffset = caretOffset;
                    }
                }
                finally
                {
                    _isUpdatingText = false;
                }
            }
        }
    }
}