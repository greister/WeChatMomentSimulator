using System;
using CommunityToolkit.Mvvm.ComponentModel;
using WeChatMomentSimulator.Core.Models.Template;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Media;



namespace WeChatMomentSimulator.Core.DataBinding
{
    /// <summary>
    /// 表示占位符与数据之间的绑定关系
    /// </summary>
    public class PlaceholderBinding : IPlaceholderBinding,INotifyPropertyChanged
    {
        /// <summary>
        /// 占位符定义
        /// </summary>
        public PlaceholderDefinition Definition { get; }

        /// <summary>
        /// 当前绑定值
        /// </summary>
        public object Value { get; set; }

        /// <summary>
        /// 是否是必需的
        /// </summary>
        public bool IsRequired => Definition.Required;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="definition">占位符定义</param>
        /// <param name="initialValue">初始值</param>
        public PlaceholderBinding(PlaceholderDefinition definition, object initialValue)
        {
            Definition = definition ?? throw new ArgumentNullException(nameof(definition));
            Value = initialValue ?? definition.DefaultValue;
        }

        /// <summary>
        /// 验证当前值是否有效
        /// </summary>
        /// <param name="errorMessage">错误信息</param>
        /// <returns>是否有效</returns>
        public bool Validate(out string errorMessage)
        {
            // 如果是必需的，则不能为null
            if (IsRequired && Value == null)
            {
                errorMessage = $"占位符 '{Definition.Name}' 是必需的，但值为空";
                return false;
            }

            // 根据类型进行特定验证
            switch (Definition.Type)
            {
                case PlaceholderType.Number:
                    if (Value != null && !(Value is int || Value is double || Value is float || Value is decimal))
                    {
                        errorMessage = $"占位符 '{Definition.Name}' 需要数字类型的值，但得到的是 {Value.GetType().Name}";
                        return false;
                    }

                    break;

                case PlaceholderType.Boolean:
                    if (Value != null && !(Value is bool))
                    {
                        errorMessage = $"占位符 '{Definition.Name}' 需要布尔类型的值，但得到的是 {Value.GetType().Name}";
                        return false;
                    }

                    break;

                case PlaceholderType.DateTime:
                    if (Value != null && !(Value is DateTime))
                    {
                        errorMessage = $"占位符 '{Definition.Name}' 需要日期时间类型的值，但得到的是 {Value.GetType().Name}";
                        return false;
                    }

                    break;

                // 其他类型的验证逻辑...
            }

            errorMessage = null;
            return true;
        }

        public string DisplayValue { get; }

        /// <summary>
        /// 格式化值为字符串
        /// </summary>
        /// <returns>格式化后的字符串</returns>
        /// <summary>
        /// 设置绑定值
        /// </summary>
        public void SetValue(object value)
        {
            if (value != null)
            {
                // 根据占位符类型检查值的类型兼容性
                if (!IsCompatibleType(value, Definition.Type))
                {
                    throw new ArgumentException($"值类型不匹配，预期 {Definition.Type.ToString()}");
                }
            }

            Value = value;
        }



        private bool IsCompatibleType(object value, PlaceholderType placeholderType)
        {
            switch (placeholderType)
            {
                case PlaceholderType.Text:
                    // 几乎所有对象都可以转换为文本
                    return true;

                case PlaceholderType.Number:
                    // 检查是否可以解析为数字
                    return value is int || value is double || value is decimal || value is float ||
                           value is long || value is short || value is byte ||
                           (value is string str && double.TryParse(str, out _));

                case PlaceholderType.Boolean:
                    // 检查是否可以解析为布尔值
                    return value is bool ||
                           (value is string s && (s == "0" || s == "1" ||
                                                  s.Equals("true", StringComparison.OrdinalIgnoreCase) ||
                                                  s.Equals("false", StringComparison.OrdinalIgnoreCase)));

                case PlaceholderType.DateTime:
                    // 检查是否是日期时间类型或可以解析为日期时间
                    return value is DateTime ||
                           (value is string dateStr && DateTime.TryParse(dateStr, out _));

                case PlaceholderType.Image:
                    // 图片一般是路径字符串或特定图片对象
                    return value is string || value is System.Drawing.Image ||
                           value is System.Windows.Media.ImageSource;

                case PlaceholderType.List:
                    // 检查是否是集合类型
                    return value is System.Collections.IEnumerable && !(value is string);

                default:
                    return true;
            }
        }


        public event PropertyChangedEventHandler? PropertyChanged;
    }
}