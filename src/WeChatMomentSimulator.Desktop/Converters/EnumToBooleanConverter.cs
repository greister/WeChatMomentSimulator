using System;
using System.Globalization;
using System.Windows.Data;
using WeChatMomentSimulator.Core.Models.Enums;

namespace WeChatMomentSimulator.Desktop.Converters
{
    public class EnumToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null || value == null)
                return false;

            string parameterString = parameter.ToString();
            
            // 处理ImageType枚举
            if (value is ImageType imageType)
            {
                return imageType.ToString() == parameterString;
            }
            
            if (Enum.IsDefined(value.GetType(), value))
            {
                return value.ToString().Equals(parameterString, StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter == null || value == null || !(bool)value)
                return null;

            string parameterString = parameter.ToString();
            // 针对不同枚举类型返回相应的值
            if (targetType == typeof(ImageType) && Enum.TryParse<ImageType>(parameterString, out var imageTypeResult))
            {
                return imageTypeResult;
            }
            
            return null;
        }
    }
}