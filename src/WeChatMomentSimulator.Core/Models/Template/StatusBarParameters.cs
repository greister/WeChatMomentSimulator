namespace WeChatMomentSimulator.Core.Models.Template
{
    /// <summary>
    /// 状态栏参数模型，包含可配置的状态栏元素
    /// </summary>
    public class StatusBarParameters
    {
        // 现有的基本属性...
        
        /// <summary>
        /// 活动通知列表
        /// </summary>
        public List<NotificationItem> Notifications { get; set; } = new List<NotificationItem>();
        
        /// <summary>
        /// 活动应用状态列表
        /// </summary>
        public List<AppStatusItem> ActiveAppStatuses { get; set; } = new List<AppStatusItem>();
        
        /// <summary>
        /// 是否显示定位图标
        /// </summary>
        public bool ShowLocationIcon { get; set; } = false;
        
        /// <summary>
        /// 是否显示蓝牙图标
        /// </summary>
        public bool ShowBluetoothIcon { get; set; } = false;
        
        /// <summary>
        /// 是否显示闹钟图标
        /// </summary>
        public bool ShowAlarmIcon { get; set; } = false;
        
        /// <summary>
        /// 是否显示静音图标
        /// </summary>
        public bool ShowMuteIcon { get; set; } = false;
        
        /// <summary>
        /// 是否显示勿扰模式图标
        /// </summary>
        public bool ShowDoNotDisturbIcon { get; set; } = false;
        
        /// <summary>
        /// 是否显示耳机已连接图标
        /// </summary>
        public bool ShowHeadphonesConnectedIcon { get; set; } = false;
        
        /// <summary>
        /// 是否显示VPN连接图标
        /// </summary>
        public bool ShowVpnIcon { get; set; } = false;
        
        /// <summary>
        /// 运营商名称
        /// </summary>
        public string CarrierName { get; set; } = "";
        
        // 保留原有的GetRandom静态方法，但改为调用新的随机生成器
        public static StatusBarParameters GetRandom()
        {
            return StatusBarRandomizer.GenerateRandomStatusBar();
        }
    }
    
    /// <summary>
    /// 通知项目
    /// </summary>
    public class NotificationItem
    {
        /// <summary>
        /// 通知来源应用
        /// </summary>
        public string AppName { get; set; }
        
        /// <summary>
        /// 图标类型
        /// </summary>
        public NotificationIconType IconType { get; set; }
    }
    
    /// <summary>
    /// 应用状态项目
    /// </summary>
    public class AppStatusItem
    {
        /// <summary>
        /// 应用名称
        /// </summary>
        public string AppName { get; set; }
        
        /// <summary>
        /// 状态类型
        /// </summary>
        public AppStatusType StatusType { get; set; }
        
        /// <summary>
        /// 状态图标
        /// </summary>
        public string IconId { get; set; }
    }
    
    /// <summary>
    /// 通知图标类型枚举
    /// </summary>
    public enum NotificationIconType
    {
        Message,
        Email,
        Call,
        Update,
        Calendar,
        Social,
        Custom
    }
    
    /// <summary>
    /// 应用状态类型枚举
    /// </summary>
    public enum AppStatusType
    {
        Recording,
        Playing,
        Navigating,
        Calling,
        Downloading,
        Uploading,
        Sharing,
        Hotspot
    }
}