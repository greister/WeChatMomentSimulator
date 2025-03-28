using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Extensions.Logging;
using WeChatMomentSimulator.Core.DataBinding;
using WeChatMomentSimulator.Core.Interfaces;
using WeChatMomentSimulator.Core.Models.Template;
using LoggerExtensions = WeChatMomentSimulator.Core.Logging.LoggerExtensions;

namespace WeChatMomentSimulator.Services.DataBinding
{
    /// <summary>
    /// 数据绑定上下文，管理占位符绑定关系
    /// </summary>
    public class DataBindingContext
    {
        private readonly ILogger<DataBindingContext> _logger;
        private readonly IDataProvider _dataProvider;
        private readonly IPlaceholderParser _placeholderParser;
        private string _currentTemplate;
        
        /// <summary>
        /// 当前活动的占位符绑定
        /// </summary>
        public ObservableCollection<PlaceholderBinding> Bindings { get; } 
            = new ObservableCollection<PlaceholderBinding>();
        
        /// <summary>
        /// 按分类组织的占位符绑定
        /// </summary>
        public Dictionary<string, List<PlaceholderBinding>> CategoryBindings { get; }
            = new Dictionary<string, List<PlaceholderBinding>>();
        
        /// <summary>
        /// 当数据发生变化时触发
        /// </summary>
        public event EventHandler<DataChangedEventArgs> DataChanged;
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public DataBindingContext(
            IDataProvider dataProvider,
            IPlaceholderParser placeholderParser)
        {
            _dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
            _placeholderParser = placeholderParser ?? throw new ArgumentNullException(nameof(placeholderParser));
            _logger = LoggerExtensions.GetLogger<DataBindingContext>();
            
            // 监听数据提供者的数据变化
            _dataProvider.DataChanged += OnDataProviderChanged;
        }
        
        /// <summary>
        /// 设置要绑定的模板内容
        /// </summary>
        /// <param name="templateContent">SVG模板内容</param>
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
                Bindings.Clear();
                CategoryBindings.Clear();
                
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
                    Bindings.Add(binding);
                    
                    // 添加到分类
                    if (!CategoryBindings.ContainsKey(placeholder.Category))
                    {
                        CategoryBindings[placeholder.Category] = new List<PlaceholderBinding>();
                    }
                    CategoryBindings[placeholder.Category].Add(binding);
                }
                
                _logger.LogInformation("已为模板创建 {Count} 个绑定，分属于 {CategoryCount} 个分类", 
                    Bindings.Count, CategoryBindings.Count);
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
            _dataProvider.UpdateData(placeholderName, value);
        }
        
        /// <summary>
        /// 数据提供者数据变更处理
        /// </summary>
        private void OnDataProviderChanged(object sender, DataChangedEventArgs e)
        {
            // 更新绑定值
            if (!e.IsBulkUpdate && e.Key != null)
            {
                var binding = Bindings.FirstOrDefault(b => b.Definition.Name == e.Key);
                if (binding != null)
                {
                    binding.Value = e.Value;
                }
            }
            else
            {
                // 批量更新所有绑定
                var data = _dataProvider.GetAllData();
                foreach (var binding in Bindings)
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
    }
}