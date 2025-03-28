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

        /// <summary>
        /// 数据变更事件
        /// </summary>
        public event EventHandler<DataChangedEventArgs> DataChanged;

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
        /// 更新数据
        /// </summary>
        public void UpdateData(string key, object value)
        {
            _data[key] = value;
            _logger.LogDebug("已更新键 {Key} 的值为 {Value}", key, value);
            DataChanged?.Invoke(this, new DataChangedEventArgs(key, value));
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