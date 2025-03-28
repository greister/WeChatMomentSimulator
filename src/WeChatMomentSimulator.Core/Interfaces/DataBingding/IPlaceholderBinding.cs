using WeChatMomentSimulator.Core.Models.Template;

/// <summary>
/// 占位符绑定接口，定义占位符与值之间的关联
/// </summary>
public interface IPlaceholderBinding
{
    /// <summary>
    /// 占位符定义
    /// </summary>
    PlaceholderDefinition Definition { get; }
        
    /// <summary>
    /// 绑定值
    /// </summary>
    object Value { get; set; }
        
    /// <summary>
    /// 用于显示的值
    /// </summary>
    string DisplayValue { get; }
        
    /// <summary>
    /// 设置绑定值并验证类型兼容性
    /// </summary>
    void SetValue(object value);
}