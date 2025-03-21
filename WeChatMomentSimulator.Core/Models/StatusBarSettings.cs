// Models/StatusBarSettings.cs
namespace WeChatMomentSimulator.Core.Models
{
    public class StatusBarSettings
    {
        public string BatteryStyle { get; set; } = "ios";
        public DynamicParameter<int> Battery { get; set; } = new DynamicParameter<int> { MinValue = 20, MaxValue = 100, CurrentValue = 80 };
        
        public string SignalStyle { get; set; } = "5bar";
        public DynamicParameter<int> SignalStrength { get; set; } = new DynamicParameter<int> { MinValue = 3, MaxValue = 5, CurrentValue = 4 };
        
        public string CarrierName { get; set; } = "中国移动";
        public string TimeFormat { get; set; } = "HH:mm";
    }
}