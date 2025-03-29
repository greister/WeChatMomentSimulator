using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace WeChatMomentSimulator.Desktop.Converters
{
    public class BooleanToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool bValue = false;
            if (value is bool)
            {
                bValue = (bool)value;
            }
            else if (value is bool?)
            {
                bool? tmp = (bool?)value;
                bValue = tmp.GetValueOrDefault();
            }
            
            if (parameter != null && parameter.ToString() == "Inverse")
            {
                bValue = !bValue;
            }
            
            return bValue ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Visibility)
            {
                if (parameter != null && parameter.ToString() == "Inverse")
                {
                    return (Visibility)value != Visibility.Visible;
                }
                
                return (Visibility)value == Visibility.Visible;
            }
            
            return false;
        }
    }
}