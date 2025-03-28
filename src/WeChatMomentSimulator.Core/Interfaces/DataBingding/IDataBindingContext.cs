using WeChatMomentSimulator.Core.DataBinding;

namespace WeChatMomentSimulator.Core.Interfaces.DataBingding;

/// <summary>
/// 数据绑定上下文接口，定义数据与模板的绑定操作
/// </summary>
public interface IDataBindingContext
{
    /// <summary>
    /// 当前活动的占位符绑定集合
    /// </summary>
    IReadOnlyCollection<IPlaceholderBinding> Bindings { get; }
    
    /// <summary>
    /// 按分类组织的占位符绑定
    /// </summary>
    IReadOnlyDictionary<string, IReadOnlyCollection<IPlaceholderBinding>> CategoryBindings { get; }
    
    /// <summary>
    /// 当数据发生变化时触发的事件
    /// </summary>
    event EventHandler<DataChangedEventArgs> DataChanged;
    
    /// <summary>
    /// 设置要绑定的模板内容
    /// </summary>
    /// <param name="templateContent">SVG模板内容</param>
    void SetTemplate(string templateContent);
    
    /// <summary>
    /// 获取用于预览的处理后的SVG内容
    /// </summary>
    /// <returns>处理后的SVG内容</returns>
    string GetProcessedTemplate();
    
    /// <summary>
    /// 更新指定占位符的值
    /// </summary>
    /// <param name="placeholderName">占位符名称</param>
    /// <param name="value">新值</param>
    void UpdateBinding(string placeholderName, object value);
    
    /// <summary>
    /// 获取指定占位符的值
    /// </summary>
    /// <param name="placeholderName">占位符名称</param>
    /// <returns>占位符的值，如果不存在则返回null</returns>
    object GetBindingValue(string placeholderName);
    
    /// <summary>
    /// 获取所有绑定的数据
    /// </summary>
    /// <returns>键值对形式的数据字典</returns>
    IDictionary<string, object> GetAllBindingValues();
    
    /// <summary>
    /// 重置所有占位符为默认值
    /// </summary>
    void ResetToDefaults();


    /// <summary>
    /// 获取带默认值的绑定值
    /// </summary>
    public T GetValue<T>(string placeholderName, T defaultValue = default);



}