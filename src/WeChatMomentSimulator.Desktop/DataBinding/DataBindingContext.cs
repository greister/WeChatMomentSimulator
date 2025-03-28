using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Extensions.Logging;
using WeChatMomentSimulator.Core.DataBinding;
using WeChatMomentSimulator.Core.Interfaces;
using WeChatMomentSimulator.Core.Interfaces.DataBingding;
using WeChatMomentSimulator.Core.Models.Template;

namespace WeChatMomentSimulator.Services.DataBinding
{
    /// <summary>
    /// 数据绑定上下文，管理占位符绑定关系
    /// </summary>
    public class DataBindingContext : IDataBindingContext
    {
        private readonly ILogger<IDataBindingContext> _logger;
        private readonly IDataProvider _dataProvider;
        private readonly IPlaceholderParser _placeholderParser;
        private string _currentTemplate;
        private readonly ObservableCollection<PlaceholderBinding> _bindings = 
            new ObservableCollection<PlaceholderBinding>();
        private readonly Dictionary<string, List<PlaceholderBinding>> _categoryBindings =
            new Dictionary<string, List<PlaceholderBinding>>();
        
        /// <summary>
        /// 当前活动的占位符绑定集合
        /// </summary>
        public IReadOnlyCollection<IPlaceholderBinding> Bindings => _bindings;
        
        /// <summary>
        /// 按分类组织的占位符绑定
        /// </summary>
        public IReadOnlyDictionary<string, IReadOnlyCollection<IPlaceholderBinding>> CategoryBindings =>
            _categoryBindings.ToDictionary(
                kvp => kvp.Key, 
                kvp => (IReadOnlyCollection<IPlaceholderBinding>)kvp.Value.AsReadOnly());
        
        /// <summary>
        /// 当数据发生变化时触发
        /// </summary>
        public event EventHandler<DataChangedEventArgs> DataChanged;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public DataBindingContext(
            IDataProvider dataProvider,
            IPlaceholderParser placeholderParser,
            ILogger<DataBindingContext> logger)
        {
            _dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
            _placeholderParser = placeholderParser ?? throw new ArgumentNullException(nameof(placeholderParser));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // 监听数据提供者的数据变化
            _dataProvider.DataChanged += OnDataProviderChanged;
        }
        
        /// <summary>
        /// 设置要绑定的模板内容
        /// </summary>
        public void SetTemplate(string templateContent)
        {
            if (string.IsNullOrEmpty(templateContent))
            {
                _logger.LogWarning("尝试绑定空模板内容");
                return;
            }
            
            _currentTemplate = templateContent;
            
            try
            {
                // 从模板中提取所有占位符名称
                var placeholderNames = _placeholderParser.ExtractPlaceholders(templateContent);
                
                // 获取所有标准占位符定义
                var allPlaceholders = StandardPlaceholders.GetAll();
                
                // 查找模板中用到的占位符定义
                var usedPlaceholders = allPlaceholders.Where(p => placeholderNames.Contains(p.Name)).ToList();
                
                // 清除现有绑定
                _bindings.Clear();
                _categoryBindings.Clear();
                
                // 创建新的绑定
                foreach (var placeholder in usedPlaceholders)
                {
                    // 获取当前值或默认值
                    _dataProvider.TryGetData(placeholder.Name, out object currentValue);
                    if (currentValue == null)
                    {
                        currentValue = placeholder.DefaultValue;
                        // 如果数据提供者中没有此值，则更新进去
                        _dataProvider.UpdateData(placeholder.Name, currentValue);
                    }
                    
                    // 创建绑定
                    var binding = new PlaceholderBinding(placeholder, currentValue);
                    _bindings.Add(binding);
                    
                    // 添加到分类
                    if (!_categoryBindings.ContainsKey(placeholder.Category))
                    {
                        _categoryBindings[placeholder.Category] = new List<PlaceholderBinding>();
                    }
                    _categoryBindings[placeholder.Category].Add(binding);
                }
                
                _logger.LogInformation("已为模板创建 {Count} 个绑定，分属于 {CategoryCount} 个分类", 
                    _bindings.Count, _categoryBindings.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "设置模板绑定失败");
            }
        }
        
        /// <summary>
        /// 获取用于预览的处理后的SVG内容
        /// </summary>
        public string GetProcessedTemplate()
        {
            if (string.IsNullOrEmpty(_currentTemplate))
            {
                _logger.LogWarning("尝试处理空模板");
                return string.Empty;
            }
            
            try
            {
                // 从数据提供者获取所有数据
                var data = _dataProvider.GetAllData();
                
                // 处理模板
                return _placeholderParser.ReplacePlaceholders(_currentTemplate, data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "处理模板失败");
                return $"<!-- 处理错误: {ex.Message} -->\n{_currentTemplate}";
            }
        }
        
        /// <summary>
        /// 更新指定占位符的值
        /// </summary>
        public void UpdateBinding(string placeholderName, object value)
        {
            var binding = Bindings.FirstOrDefault(b => b.Definition.Name == placeholderName);
            if ( binding == null )
            {
                try
                {   
                    binding.SetValue(value);
                    _dataProvider.UpdateData(placeholderName, value);       
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e, "类型验证失败:{placeholder}", placeholderName);
                    throw;
                }
                
            }
            
           
        }
        
        /// <summary>
        /// 获取指定占位符的值
        /// </summary>
        /// <param name="placeholderName">占位符名称</param>
        /// <returns>占位符的值，如果不存在则返回null</returns>
        public object GetBindingValue(string placeholderName)
        {
            if (string.IsNullOrEmpty(placeholderName))
            {
                _logger.LogWarning("尝试获取无效的占位符值");
                return null;
            }
            
            if (_dataProvider.TryGetData(placeholderName, out object value))
            {
                return value;
            }
            
            // 如果数据提供者中没有，尝试从绑定中查找
            var binding = _bindings.FirstOrDefault(b => b.Definition.Name == placeholderName);
            if (binding != null)
            {
                return binding.Value;
            }
            
            _logger.LogDebug("占位符 {PlaceholderName} 未找到绑定值", placeholderName);
            return null;
        }
        
        /// <summary>
        /// 获取所有绑定的数据
        /// </summary>
        /// <returns>键值对形式的数据字典</returns>
        public IDictionary<string, object> GetAllBindingValues()
        {
            // 首先从数据提供者获取所有数据
            var result = _dataProvider.GetAllData();
            
            // 如果某些绑定在数据提供者中不存在，则添加进去
            foreach (var binding in _bindings)
            {
                if (!result.ContainsKey(binding.Definition.Name))
                {
                    result[binding.Definition.Name] = binding.Value;
                }
            }
            
            return result;
        }
        
        /// <summary>
        /// 重置所有占位符为默认值
        /// </summary>
        public void ResetToDefaults()
        {
            _logger.LogInformation("重置所有占位符为默认值");
            
            // 创建批量更新标记，避免多次触发更新事件
            bool originalBatchMode = _dataProvider.BatchUpdateMode;
            _dataProvider.BatchUpdateMode = true;
            
            try
            {
                // 重置每个绑定到其默认值
                foreach (var binding in _bindings)
                {
                    _dataProvider.UpdateData(binding.Definition.Name, binding.Definition.DefaultValue);
                    // 同时更新绑定对象的值，以便UI立即反映变化
                    binding.Value = binding.Definition.DefaultValue;
                }
            }
            finally
            {
                // 恢复原始批量模式
                _dataProvider.BatchUpdateMode = originalBatchMode;
                
                // 手动触发一次数据变更事件，通知所有监听器
                DataChanged?.Invoke(this, new DataChangedEventArgs());
            }
        }

        /// <summary>
        /// 获取带默认值的绑定值
        /// </summary>
        public T GetValue<T>(string placeholderName, T defaultValue = default)
        {
            if (_dataProvider.TryGetData(placeholderName, out object value))
            {
                return value != null ? (T)value : defaultValue;
            }
            return defaultValue;
        }

        /// <summary>
        /// 数据提供者数据变更处理
        /// </summary>
        private void OnDataProviderChanged(object sender, DataChangedEventArgs e)
        {
            // 更新绑定值
            if (!e.IsBulkUpdate && e.Key != null)
            {
                var binding = _bindings.FirstOrDefault(b => b.Definition.Name == e.Key);
                if (binding != null)
                {
                    binding.Value = e.Value;
                }
            }
            else
            {
                // 批量更新所有绑定
                var data = _dataProvider.GetAllData();
                foreach (var binding in _bindings)
                {
                    if (data.TryGetValue(binding.Definition.Name, out object value))
                    {
                        binding.Value = value;
                    }
                }
            }
            
            // 转发事件
            DataChanged?.Invoke(this, e);
        }
        
        /// <summary>
        /// 清理资源
        /// </summary>
        public void Dispose()
        {
            // 取消事件订阅
            if (_dataProvider != null)
            {
                _dataProvider.DataChanged -= OnDataProviderChanged;
            }
        }
    }
}