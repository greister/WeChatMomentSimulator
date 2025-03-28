using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using WeChatMomentSimulator.Core.DataBinding;
using WeChatMomentSimulator.Core.Logging;
using LoggerExtensions = WeChatMomentSimulator.Core.Logging.LoggerExtensions;

namespace WeChatMomentSimulator.Services.DataBinding
{
    /// <summary>
    /// IDataProvider 的默认内存实现
    /// </summary>
    public class MemoryDataProvider : IDataProvider
    {
        private readonly ILogger<MemoryDataProvider> _logger;
        private readonly ConcurrentDictionary<string, object> _data = new ConcurrentDictionary<string, object>();
        // 批量更新模式标志
        private bool _batchUpdateMode;
        // 存储所有数据的字典
        private readonly Dictionary<string, object> _dataStore = new Dictionary<string, object>();

        
            // 在批量更新模式下已更新的键集合
        private readonly HashSet<string> _batchUpdatedKeys = new HashSet<string>();
        /// <summary>
        /// 数据变更事件
        /// </summary>
        public event EventHandler<DataChangedEventArgs> DataChanged;

    
        /// <summary>
        /// 批量更新模式 - 启用时不会立即触发事件
        /// </summary>
        public bool BatchUpdateMode
        {
            get => _batchUpdateMode;
            set
            {
                if (_batchUpdateMode == value)
                    return;
                
                _batchUpdateMode = value;
            
                // 如果关闭批量更新模式，且有数据更新，触发一次批量事件
                if (!_batchUpdateMode && _batchUpdatedKeys.Count > 0)
                {
                    // 触发批量更新事件
                    DataChanged?.Invoke(this, new DataChangedEventArgs());
                    _batchUpdatedKeys.Clear();
                }
            }
        }

        
        /// <summary>
        /// 更新数据值
        /// </summary>
        public void UpdateData(string key, object value)
        {
            if (string.IsNullOrEmpty(key))
                throw new ArgumentException("键不能为空", nameof(key));
            
            _dataStore[key] = value;
            _logger.LogDebug("已更新键 {Key} 的值为 {Value}", key, value);
            if (!BatchUpdateMode)
            {
                // 非批量模式，立即触发单个数据更新事件
                DataChanged?.Invoke(this, new DataChangedEventArgs(key, value));
            }
            else
            {
                // 批量模式，仅记录已更新的键
                _batchUpdatedKeys.Add(key);
            }
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        public MemoryDataProvider()
        {
            _logger = LoggerExtensions.GetLogger<MemoryDataProvider>();
        }

        /// <summary>
        /// 获取所有占位符数据
        /// </summary>
        public Dictionary<string, object> GetAllData()
        {
            return new Dictionary<string, object>(_data);
        }

        /// <summary>
        /// 获取指定类型的数据
        /// </summary>
        public T GetTypedData<T>(string key)
        {
            if (!_data.TryGetValue(key, out var value))
            {
                _logger.LogWarning("请求的键 {Key} 不存在", key);
                return default;
            }

            try
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "将键 {Key} 的值 {Value} 转换为类型 {Type} 失败", key, value, typeof(T).Name);
                return default;
            }
        }

        /// <summary>
        /// 尝试获取数据
        /// </summary>
        public bool TryGetData(string key, out object value)
        {
            return _data.TryGetValue(key, out value);
        }



        /// <summary>
        /// 批量更新数据
        /// </summary>
        public void UpdateBulkData(Dictionary<string, object> data)
        {
            if (data == null)
                return;

            foreach (var item in data)
            {
                _data[item.Key] = item.Value;
            }

            _logger.LogInformation("已批量更新 {Count} 个数据项", data.Count);
            DataChanged?.Invoke(this, new DataChangedEventArgs());
        }
    }
}