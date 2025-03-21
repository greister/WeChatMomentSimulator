// WeChatMomentSimulator.UI/ViewModels/MainViewModel.cs
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Serilog;
using WeChatMomentSimulator.Core.Interfaces;

namespace WeChatMomentSimulator.UI.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly ILogger<MainWindow> _logger;
        private string _statusMessage = "就绪";
        
        public MainViewModel(ILogger<MainWindow> logger)
        {
            _logger = logger;
            
            _logger.LogDebug("MainViewModel 已初始化");
        }
        
        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (_statusMessage != value)
                {
                    _statusMessage = value;
                    OnPropertyChanged();
                    _logger.LogDebug("状态已更新: {Status}", value);
                }
            }
        }
        
        public bool IsDevEnvironment { get; }
        
        public string EnvironmentName => IsDevEnvironment ? "开发环境" : "生产环境";
        
        public event PropertyChangedEventHandler PropertyChanged;
        
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
       
        
        
    }
}