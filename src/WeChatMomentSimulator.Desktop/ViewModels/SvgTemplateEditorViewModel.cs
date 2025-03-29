using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using Serilog.Core;
using LoggerExtensions = WeChatMomentSimulator.Core.Logging.LoggerExtensions;

namespace WeChatMomentSimulator.Desktop.ViewModels
{
    public class SvgTemplateEditorViewModel : INotifyPropertyChanged
    {
        private readonly ILogger<SvgTemplateEditorViewModel> _logger;
        
        
        #region 字段

        private double _zoomLevel = 1.0;
        private bool _isEditingMode;
        private string _svgSource;
        private string _svgSourceText;
        private string _currentFilePath;

        #endregion

        #region 属性

        public double ZoomLevel
        {
            get => _zoomLevel;
            set
            {
                if (Math.Abs(_zoomLevel - value) > 0.001)
                {
                    _zoomLevel = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsEditingMode
        {
            get => _isEditingMode;
            set
            {
                if (_isEditingMode != value)
                {
                    //log info  value 
                    _logger.LogInformation("Value is {Value}", value ? "true" : "false");
                    _logger.LogInformation($"执行StartEditingCommand，当前IsEditingMode={IsEditingMode}");
                    _isEditingMode = value;
                    _logger.LogInformation($"设置后IsEditingMode={IsEditingMode}");
                    OnPropertyChanged();
                    
                    if (value && !string.IsNullOrEmpty(_svgSource))
                    {
                        _logger?.LogDebug($"加载SVG源文本，文件路径: {_svgSource}");
                        LoadSvgSourceText();
                    }
                }
            }
        }

        public string SvgSource
        {
            get => _svgSource;
            set
            {
                if (_svgSource != value)
                {
                    _svgSource = value;
                    _logger.LogDebug($"SvgSource已更改，新内容长度: {value?.Length ?? 0}");
                    OnPropertyChanged();
                }
            }
        }

        // 增强SvgSourceText属性的日志
        public string SvgSourceText
        {
            get => _svgSourceText;
            set
            {
                if (_svgSourceText != value)
                {
                    _logger.LogDebug($"SVG源文本已更改，新内容长度: {value?.Length ?? 0}");
                    _svgSourceText = value;
                    OnPropertyChanged();

                    // 当源代码更改时，保存到临时文件并更新预览
                    if (IsEditingMode && !string.IsNullOrEmpty(value))
                    {
                        _logger.LogInformation("更新SVG源文本，准备保存临时文件并更新预览");
                        SaveToTempFileAndUpdatePreview();
                    }
                }
            }
        }


        #endregion

        #region 命令

        public ICommand ZoomInCommand { get; }
        public ICommand ZoomOutCommand { get; }
        public ICommand StartEditingCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand OpenCommand { get; }

        #endregion

        #region 构造函数

        public SvgTemplateEditorViewModel()
        {
            _logger = LoggerExtensions.GetLogger<SvgTemplateEditorViewModel>();
            // 初始化命令
            ZoomInCommand = new RelayCommand(_ => ZoomIn());
            ZoomOutCommand = new RelayCommand(_ => ZoomOut());
            //StartEditingCommand = new RelayCommand(_ => IsEditingMode = !IsEditingMode);
            SaveCommand = new RelayCommand(_ => SaveSvgFile());
            OpenCommand = new RelayCommand(_ => LoadSvgFile());
            
            // 在StartEditingCommand创建时添加日志
            StartEditingCommand = new RelayCommand(_ => {
                _logger.LogInformation($"执行StartEditingCommand，当前IsEditingMode={IsEditingMode}");
                IsEditingMode = !IsEditingMode;
                _logger.LogInformation($"切换后IsEditingMode={IsEditingMode}");
            });
        }

        #endregion

        #region 公共方法

        /// <summary>
        /// ��载SVG文件
        /// </summary>
        public void LoadSvgFile(string filePath = null)
        {
            if (filePath == null)
            {
                var openFileDialog = new OpenFileDialog
                {
                    Title = "选择SVG文件",
                    Filter = "SVG文件 (*.svg)|*.svg|所有文件 (*.*)|*.*"
                };

                if (openFileDialog.ShowDialog() != true)
                {
                    return;
                }

                filePath = openFileDialog.FileName;
            }

            if (File.Exists(filePath))
            {
                _currentFilePath = filePath;
                SvgSource = filePath;

                if (IsEditingMode)
                {
                    LoadSvgSourceText();
                }
            }
        }

        #endregion

        #region 私有方法

        private void ZoomIn()
        {
            ZoomLevel = Math.Min(10.0, ZoomLevel + 0.1);
        }

        private void ZoomOut()
        {
            ZoomLevel = Math.Max(0.1, ZoomLevel - 0.1);
        }

        // 增强LoadSvgSourceText方法的日志
        private void LoadSvgSourceText()
        {
            _logger.LogInformation($"开始加载SVG源文本，当前文件路径: {_currentFilePath}");
            if (!string.IsNullOrEmpty(_currentFilePath) && File.Exists(_currentFilePath))
            {
                try 
                {
                    string content = File.ReadAllText(_currentFilePath);
                    _logger.LogDebug($"读取到的文件内容长度: {content?.Length ?? 0}");
                    SvgSourceText = content;
                    _logger.LogInformation("SVG源文本加载成功");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "加载SVG源文本失败");
                }
            }
            else
            {
                _logger.LogWarning($"无法加载SVG文件，路径无效或文件不存在: {_currentFilePath}");
            }
        }

        
// 增强SaveToTempFileAndUpdatePreview方法的日志
        private void SaveToTempFileAndUpdatePreview()
        {
            string tempPath = Path.Combine(Path.GetTempPath(), "temp_preview.svg");
            _logger.LogInformation($"保存临时预览文件到: {tempPath}");

            try
            {
                File.WriteAllText(tempPath, SvgSourceText);
                _logger.LogInformation($"临时文件保存成功，内容长度: {SvgSourceText?.Length ?? 0}");
        
                string oldSource = SvgSource;
                SvgSource = tempPath;
                _logger.LogInformation($"SVG源路径从 {oldSource} 更新为 {tempPath}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "保存临时文件失败");
            }
        }

        private void SaveSvgFile()
        {
            string fileToSave = _currentFilePath;

            if (string.IsNullOrEmpty(fileToSave))
            {
                var saveFileDialog = new SaveFileDialog
                {
                    Title = "保存SVG文件",
                    Filter = "SVG文件 (*.svg)|*.svg|所有文件 (*.*)|*.*",
                    DefaultExt = ".svg"
                };

                if (saveFileDialog.ShowDialog() != true)
                {
                    return;
                }

                fileToSave = saveFileDialog.FileName;
                _currentFilePath = fileToSave;
            }

            try
            {
                if (IsEditingMode && !string.IsNullOrEmpty(SvgSourceText))
                {
                    File.WriteAllText(fileToSave, SvgSourceText);
                }
                else if (!string.IsNullOrEmpty(SvgSource) && SvgSource != fileToSave)
                {
                    File.Copy(SvgSource, fileToSave, true);
                }

                _currentFilePath = fileToSave;
                SvgSource = fileToSave;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"保存文件失败: {ex.Message}");
            }
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    /// <summary>
    /// 实现ICommand的命令类
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        public bool CanExecute(object parameter) => _canExecute?.Invoke(parameter) ?? true;

        public void Execute(object parameter) => _execute(parameter);

        public event EventHandler CanExecuteChanged
        {
            add => CommandManager.RequerySuggested += value;
            remove => CommandManager.RequerySuggested -= value;
        }
    }
}