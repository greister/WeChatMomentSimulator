// WeChatMomentSimulator.UI/ViewModels/MainViewModel.cs
using System.ComponentModel;
using System.Runtime.CompilerServices;
using WeChatMomentSimulator.Core.Interfaces;

namespace WeChatMomentSimulator.UI.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly IAppLogger _logger;
        private string _statusMessage = "就绪";
        
        public MainViewModel(IAppLogger logger, bool isDevEnvironment)
        {
            _logger = logger;
            IsDevEnvironment = isDevEnvironment;
            
            _logger.Debug("MainViewModel 已初始化");
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
                    _logger.Debug("状态已更新: {Status}", value);
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