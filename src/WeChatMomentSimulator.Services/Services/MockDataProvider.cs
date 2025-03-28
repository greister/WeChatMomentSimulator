using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Serilog;
using WeChatMomentSimulator.Core.DataBinding;
using WeChatMomentSimulator.Core.Models.Template;
using LoggerExtensions = WeChatMomentSimulator.Core.Logging.LoggerExtensions;

namespace WeChatMomentSimulator.Services.DataBinding
{
    /// <summary>
    /// 虚拟数据提供者，用于开发和测试
    /// </summary>
    public class MockDataProvider : IDataProvider
    {
        private readonly ILogger<MockDataProvider> _logger;
        private readonly Dictionary<string, object> _data = new Dictionary<string, object>();
        private readonly Random _random = new Random();
        private bool _batchUpdateMode;
        private readonly HashSet<string> _batchUpdatedKeys = new HashSet<string>();
        
        // 预设数据集合
        private readonly Dictionary<string, Dictionary<string, object>> _dataSets;
        
        public event EventHandler<DataChangedEventArgs> DataChanged;
        
        /// <summary>
        /// 当前活动的数据集名称
        /// </summary>
        public string ActiveDataSet { get; private set; }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public MockDataProvider()
        {
            _logger = LoggerExtensions.GetLogger<MockDataProvider>();
            
            // 初始化预设数据集
            _dataSets = InitializeDataSets();
            
            // 默认加载标准数据集
            LoadDataSet("Standard");
        }
        
        /// <summary>
        /// 获取所有数据
        /// </summary>
        public Dictionary<string, object> GetAllData()
        {
            return new Dictionary<string, object>(_data);
        }
        
        /// <summary>
        /// 获取类型化数据
        /// </summary>
        public T GetTypedData<T>(string key)
        {
            if (TryGetData(key, out object value) && value is T typedValue)
            {
                return typedValue;
            }
            
            _logger.LogWarning("无法获取 '{Key}' 的类型化值 {Type}", key, typeof(T).Name);
            return default;
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
            _logger.LogDebug("更新占位符 '{Key}' 的值为 '{Value}'", key, value);
            _data[key] = value;
            // 如果不是批量更新模式，则触发事件
            if (!BatchUpdateMode)
            {
                DataChanged?.Invoke(this, new DataChangedEventArgs(key, value));
            }
            else
            {
                // 记录已更新的键，以便退出批量模式时使用
                _batchUpdatedKeys.Add(key);
            }
            //DataChanged?.Invoke(this, new DataChangedEventArgs(key, value));
        }
        
        /// <summary>
        /// 批量更新数据
        /// </summary>
        public void UpdateBulkData(Dictionary<string, object> data)
        {
            if (data == null)
                return;
                
            foreach (var pair in data)
            {
                _data[pair.Key] = pair.Value;
            }
            
            _logger.LogDebug("批量更新 {Count} 个占位符值", data.Count);
            DataChanged?.Invoke(this, new DataChangedEventArgs());
        }
        
        /// <summary>
        /// 加载指定的数据集
        /// </summary>
        public void LoadDataSet(string dataSetName)
        {
            if (_dataSets.TryGetValue(dataSetName, out var dataSet))
            {
                _data.Clear();
                foreach (var pair in dataSet)
                {
                    _data[pair.Key] = pair.Value;
                }
                
                ActiveDataSet = dataSetName;
                _logger.LogInformation("已加载数据集 '{DataSetName}' 共 {Count} 项", dataSetName, dataSet.Count);
                
                DataChanged?.Invoke(this, new DataChangedEventArgs());
            }
            else
            {
                _logger.LogWarning("找不到数据集 '{DataSetName}'", dataSetName);
            }
        }
        
        /// <summary>
        /// 生成随机数据
        /// </summary>
        public void GenerateRandomData()
        {
            var standardPlaceholders = StandardPlaceholders.GetAll();
            
            foreach (var placeholder in standardPlaceholders)
            {
                _data[placeholder.Name] = GenerateRandomValue(placeholder.Type);
            }
            
            _logger.LogInformation("已生成随机数据 共 {Count} 项", standardPlaceholders.Count());
            DataChanged?.Invoke(this, new DataChangedEventArgs());
        }
        
        /// <summary>
        /// 为指定类型生成随机值
        /// </summary>
        public object GenerateRandomValue(PlaceholderType type)
        {
            switch (type)
            {
                case PlaceholderType.Text:
                    return GetRandomText();
                    
                case PlaceholderType.Number:
                    return _random.Next(0, 100);
                    
                case PlaceholderType.Boolean:
                    return _random.Next(2) == 1;
                    
                case PlaceholderType.DateTime:
                    // 生成最近7天内的随机时间
                    return DateTime.Now.AddDays(-_random.Next(7)).AddHours(-_random.Next(24));
                    
                case PlaceholderType.Image:
                    // 返回随机样本图片路径
                    return $"sample_{_random.Next(1, 10)}.jpg";
                    
                case PlaceholderType.List:
                    // 生成1-5个随机文本项的列表
                    int count = _random.Next(1, 6);
                    var list = new List<string>();
                    for (int i = 0; i < count; i++)
                    {
                        list.Add(GetRandomText());
                    }
                    return list;
                    
                default:
                    return null;
            }
        }
        
        /// <summary>
        /// 初始化预设数据集
        /// </summary>
        private Dictionary<string, Dictionary<string, object>> InitializeDataSets()
        {
            var result = new Dictionary<string, Dictionary<string, object>>();
            
            // 标准数据集
            result["Standard"] = new Dictionary<string, object>
            {
                ["userName"] = "张三",
                ["time"] = "12:30",
                ["battery"] = "80%",
                ["networkType"] = "4G",
                ["signalStrength"] = 4,
                ["content"] = "今天天气真好，出去走走放松一下~",
                ["timeText"] = "10分钟前",
                ["hasImages"] = true,
                ["likes"] = 12,
                ["hasComments"] = false,
                ["comments"] = new List<object>()
            };
            
            // 多图片数据集
            result["MultiImage"] = new Dictionary<string, object>
            {
                ["userName"] = "摄影爱好者",
                ["time"] = "15:45",
                ["battery"] = "65%",
                ["networkType"] = "5G",
                ["signalStrength"] = 5,
                ["content"] = "周末去了郊外拍照，分享一些美景给大家！",
                ["timeText"] = "2小时前",
                ["hasImages"] = true,
                ["likes"] = 45,
                ["hasComments"] = true,
                ["comments"] = new List<object> {
                    new Dictionary<string, object> {
                        ["commenterName"] = "李四",
                        ["commentText"] = "太美了，下次带我一起去！"
                    },
                    new Dictionary<string, object> {
                        ["commenterName"] = "王五",
                        ["commentText"] = "第三张构图很棒"
                    }
                }
            };
            
            // 长文本数据集
            result["LongText"] = new Dictionary<string, object>
            {
                ["userName"] = "读书笔记",
                ["time"] = "09:15",
                ["battery"] = "95%",
                ["networkType"] = "WIFI",
                ["signalStrength"] = 4,
                ["content"] = "最近读完了《三体》这本书，感触颇多。刘慈欣构建的宇宙观让人震撼，对人性的思考也很深刻。特别是书中对技术与伦理关系的探讨，在当今AI迅速发展的背景下尤为重要。强烈推荐给所有科幻爱好者，这不仅是一部科幻小说，更是对人类文明走向的深刻思考。",
                ["timeText"] = "昨天",
                ["hasImages"] = false,
                ["likes"] = 78,
                ["hasComments"] = true,
                ["comments"] = new List<object> {
                    new Dictionary<string, object> {
                        ["commenterName"] = "科幻迷",
                        ["commentText"] = "三体三部曲都很精彩，值得一读再读！"
                    }
                }
            };
            
            // 系统信息极限测试数据集
            result["SystemEdgeCase"] = new Dictionary<string, object>
            {
                ["userName"] = "系统测试员",
                ["time"] = "00:01",
                ["battery"] = "1%",
                ["networkType"] = "无网络",
                ["signalStrength"] = 0,
                ["content"] = "测试各种系统状态的边界情况",
                ["timeText"] = "刚刚",
                ["hasImages"] = false,
                ["likes"] = 0,
                ["hasComments"] = false,
                ["comments"] = new List<object>()
            };
            
            return result;
        }
        
        /// <summary>
        /// 获取随机文本
        /// </summary>
        private string GetRandomText()
        {
            string[] texts = {
                "今天天气真好！",
                "分享一个有趣的发现",
                "新买的手机不错",
                "周末有什么计划？",
                "刚健完身，感觉棒极了",
                "新电影很值得一看",
                "美食分享",
                "生活不止眼前的苟且，还有诗和远方",
                "加油，一起努力",
                "新的一天，新的开始"
            };
            
            return texts[_random.Next(texts.Length)];
        }
        /// <summary>
        /// 获取或设置批量更新模式
        /// </summary>
        public bool BatchUpdateMode
        {
            get => _batchUpdateMode;
            set
            {
                if (_batchUpdateMode != value)
                {
                    _batchUpdateMode = value;
                
                    // 如果从批量模式切换到非批量模式，触发批量更新事件
                    if (!_batchUpdateMode && _batchUpdatedKeys.Count > 0)
                    {
                        DataChanged?.Invoke(this, new DataChangedEventArgs());
                        _batchUpdatedKeys.Clear();
                    }
                }
            }
        }
        
    }
}