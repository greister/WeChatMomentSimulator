using System;
using System.Collections.Generic;

namespace WeChatMomentSimulator.Core.DataBinding
{
    /// <summary>
    /// 数据提供者接口，用于为模板提供占位符数据
    /// </summary>
    public interface IDataProvider
    {
        /// <summary>
        /// 获取所有占位符数据
        /// </summary>
        Dictionary<string, object> GetAllData();
        
        /// <summary>
        /// 获取指定类型的数据
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="key">占位符键名</param>
        /// <returns>类型化的数据值</returns>
        T GetTypedData<T>(string key);
        
        /// <summary>
        /// 尝试获取数据
        /// </summary>
        /// <param name="key">占位符键名</param>
        /// <param name="value">获取到的值</param>
        /// <returns>是否成功获取数据</returns>
        bool TryGetData(string key, out object value);
        
        /// <summary>
        /// 更新数据
        /// </summary>
        /// <param name="key">占位符键名</param>
        /// <param name="value">新值</param>
        void UpdateData(string key, object value);
        
        /// <summary>
        /// 批量更新数据
        /// </summary>
        /// <param name="data">要更新的数据字典</param>
        void UpdateBulkData(Dictionary<string, object> data);
        
        /// <summary>
        /// 数据变更事件
        /// </summary>
        event EventHandler<DataChangedEventArgs> DataChanged;
        
        /// <summary>
        /// 获取或设置批量更新模式
        /// 当为true时，数据变更不会立即触发DataChanged事件
        /// </summary>
        bool BatchUpdateMode { get; set; }
    }
    
    /// <summary>
    /// 数据变更事件参数
    /// </summary>
    public class DataChangedEventArgs : EventArgs
    {
        /// <summary>
        /// 变更的键名，为null表示批量变更
        /// </summary>
        public string Key { get; }
        
        /// <summary>
        /// 变更后的值
        /// </summary>
        public object Value { get; }
        
        /// <summary>
        /// 是否为批量更新
        /// </summary>
        public bool IsBulkUpdate { get; }
        
        /// <summary>
        /// 构造单个更新的事件参数
        /// </summary>
        public DataChangedEventArgs(string key, object value)
        {
            Key = key;
            Value = value;
            IsBulkUpdate = false;
        }
        
        /// <summary>
        /// 构造批量更新的事件参数
        /// </summary>
        public DataChangedEventArgs()
        {
            IsBulkUpdate = true;
        }
    }
}