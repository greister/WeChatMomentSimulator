using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using WeChatMomentSimulator.Core.Interfaces.Services;
using WeChatMomentSimulator.Core.Models.Template;
using WeChatMomentSimulator.Core.Models.Template.Enums;
using WeChatMomentSimulator.Desktop.Commands;
using WeChatMomentSimulator.Core.Logging;
using LoggerExtensions = WeChatMomentSimulator.Core.Logging.LoggerExtensions;

namespace WeChatMomentSimulator.Desktop.ViewModels
{
    /// <summary>
    /// 主窗口视图模型
    /// </summary>
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly ITemplateService _templateService;
        private readonly ILogger<MainViewModel> _logger;

        private bool _isLoading;
        private bool _isSaving;
        private string _statusMessage;
        private string _errorMessage;
        private Template _selectedTemplate;
        private string _previewContent;
        private TemplateType _currentTemplateType = TemplateType.Moment;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="templateService">模板服务</param>
        public MainViewModel(ITemplateService templateService)
        {
            _templateService = templateService ?? throw new ArgumentNullException(nameof(templateService));
            _logger = LoggerExtensions.GetLogger<MainViewModel>();

            Templates = new ObservableCollection<Template>();
            TemplateVariables = new ObservableCollection<TemplateVariableViewModel>();

            // 初始化命令
            LoadTemplatesCommand = new RelayCommand(async _ => await LoadTemplatesAsync(), _ => !IsLoading);
            NewTemplateCommand = new RelayCommand(_ => CreateNewTemplate(), _ => !IsSaving);
            SaveTemplateCommand = new RelayCommand(async _ => await SaveTemplateAsync(), _ => !IsSaving && SelectedTemplate != null);
            DeleteTemplateCommand = new RelayCommand(async _ => await DeleteTemplateAsync(), _ => !IsSaving && SelectedTemplate != null && !SelectedTemplate.IsDefault);
            ExportImageCommand = new RelayCommand(async _ => await ExportImageAsync(), _ => !IsLoading && !string.IsNullOrEmpty(PreviewContent));
            
            _logger.LogInformation("MainViewModel 已初始化");
            
            // 自动加载模板
            _ = LoadTemplatesAsync();
        }

        /// <summary>
        /// 属性变更事件
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// 触发属性变更事件
        /// </summary>
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 设置属性值并触发属性变更事件
        /// </summary>
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value)) return false;
            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// 模板集合
        /// </summary>
        public ObservableCollection<Template> Templates { get; }

        /// <summary>
        /// 模板变量视图模型集合
        /// </summary>
        public ObservableCollection<TemplateVariableViewModel> TemplateVariables { get; }

        /// <summary>
        /// 是否正在加载
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            private set
            {
                if (SetProperty(ref _isLoading, value))
                {
                    // 刷新命令可用状态
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        /// <summary>
        /// 是否正在保存
        /// </summary>
        public bool IsSaving
        {
            get => _isSaving;
            private set
            {
                if (SetProperty(ref _isSaving, value))
                {
                    // 刷新命令可用状态
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        /// <summary>
        /// 状态消息
        /// </summary>
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        /// <summary>
        /// 错误消息
        /// </summary>
        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        /// <summary>
        /// 当前模板类型
        /// </summary>
        public TemplateType CurrentTemplateType
        {
            get => _currentTemplateType;
            set
            {
                if (SetProperty(ref _currentTemplateType, value))
                {
                    _logger.LogDebug("模板类型已更改为: {TemplateType}", value);
                    _ = LoadTemplatesAsync();
                }
            }
        }

        /// <summary>
        /// 选中的模板
        /// </summary>
        public Template SelectedTemplate
        {
            get => _selectedTemplate;
            set
            {
                if (SetProperty(ref _selectedTemplate, value))
                {
                    _logger.LogDebug("已选择模板: {TemplateName}", value?.Name ?? "无");
                    _ = LoadSelectedTemplateAsync();
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        /// <summary>
        /// 预览内容
        /// </summary>
        public string PreviewContent
        {
            get => _previewContent;
            set => SetProperty(ref _previewContent, value);
        }

        /// <summary>
        /// 加载模板命令
        /// </summary>
        public ICommand LoadTemplatesCommand { get; }

        /// <summary>
        /// 新建模板命令
        /// </summary>
        public ICommand NewTemplateCommand { get; }

        /// <summary>
        /// 保存模板命令
        /// </summary>
        public ICommand SaveTemplateCommand { get; }

        /// <summary>
        /// 删除模板命令
        /// </summary>
        public ICommand DeleteTemplateCommand { get; }

        /// <summary>
        /// 导出图片命令
        /// </summary>
        public ICommand ExportImageCommand { get; }

        /// <summary>
        /// 加载指定类型的所有模板
        /// </summary>
        private async Task LoadTemplatesAsync()
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;
                StatusMessage = $"正在加载{CurrentTemplateType}模板...";
                ErrorMessage = string.Empty;
                
                _logger.LogInformation("开始加载{TemplateType}类型的模板", CurrentTemplateType);

                Templates.Clear();
                var templates = await _templateService.GetTemplatesByTypeAsync(CurrentTemplateType);
                
                foreach (var template in templates)
                {
                    Templates.Add(template);
                }

                StatusMessage = $"已加载 {Templates.Count} 个{CurrentTemplateType}模板";
                _logger.LogInformation("成功加载 {Count} 个{TemplateType}模板", Templates.Count, CurrentTemplateType);

                // 如果没有选中模板但有可用模板，则选中第一个
                if (SelectedTemplate == null && Templates.Count > 0)
                {
                    // 首先尝试选择默认模板
                    SelectedTemplate = Templates.FirstOrDefault(t => t.IsDefault) ?? Templates.First();
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"加载模板失败: {ex.Message}";
                _logger.LogError(ex, "加载{TemplateType}类型的模板失败", CurrentTemplateType);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// 加载选中的模板
        /// </summary>
        private async Task LoadSelectedTemplateAsync()
        {
            if (SelectedTemplate == null)
            {
                PreviewContent = string.Empty;
                TemplateVariables.Clear();
                return;
            }

            try
            {
                IsLoading = true;
                StatusMessage = $"正在加载模板: {SelectedTemplate.Name}...";
                ErrorMessage = string.Empty;
                
                _logger.LogDebug("开始加载模板内容: {TemplateName}", SelectedTemplate.Name);

                // 加载模板内容
                if (string.IsNullOrEmpty(SelectedTemplate.Content))
                {
                    var template = await _templateService.LoadTemplateContentAsync(SelectedTemplate);
                    SelectedTemplate.Content = template.Content;
                    SelectedTemplate.Variables = template.Variables;
                    
                    _logger.LogDebug("已加载模板内容, 内容长度: {Length} 字符", SelectedTemplate.Content?.Length ?? 0);
                }

                // 更新预览
                PreviewContent = SelectedTemplate.Content;

                // 更新变量列表
                UpdateTemplateVariables();

                StatusMessage = $"已加载模板: {SelectedTemplate.Name}";
                _logger.LogInformation("成功加载模板: {TemplateName}", SelectedTemplate.Name);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"加载模板内容失败: {ex.Message}";
                _logger.LogError(ex, "加载模板内容失败: {TemplateName}", SelectedTemplate?.Name);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// 更新模板变量
        /// </summary>
        private void UpdateTemplateVariables()
        {
            TemplateVariables.Clear();

            if (SelectedTemplate?.Variables == null) return;

            foreach (var variable in SelectedTemplate.Variables)
            {
                TemplateVariables.Add(new TemplateVariableViewModel(variable));
            }
            
            _logger.LogDebug("已更新 {Count} 个模板变量", TemplateVariables.Count);
        }

        /// <summary>
        /// 创建新模板
        /// </summary>
        private void CreateNewTemplate()
        {
            var newTemplate = new Template
            {
                Id = Guid.NewGuid(),
                Name = "新建模板",
                DisplayName = "新建模板",
                Description = "这是一个新建的模板",
                Type = CurrentTemplateType,
                CreatedAt = DateTime.Now,
                ModifiedAt = DateTime.Now,
                Content = "<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"750\" height=\"1334\"><rect width=\"100%\" height=\"100%\" fill=\"#f0f0f0\"/><text x=\"50%\" y=\"50%\" font-size=\"24\" text-anchor=\"middle\">{{text}}</text></svg>",
                Variables = new System.Collections.Generic.List<TemplateVariable>
                {
                    new TemplateVariable
                    {
                        Name = "text",
                        DisplayName = "文本内容",
                        Description = "显示的文本内容",
                        Type = VariableType.Text,
                        DefaultValue = "Hello World"
                    }
                }
            };

            Templates.Add(newTemplate);
            SelectedTemplate = newTemplate;
            
            _logger.LogInformation("已创建新模板: {TemplateId}", newTemplate.Id);
        }

        /// <summary>
        /// 保存模板
        /// </summary>
        private async Task SaveTemplateAsync()
        {
            if (SelectedTemplate == null || IsSaving) return;

            try
            {
                IsSaving = true;
                StatusMessage = $"正在保存模板: {SelectedTemplate.Name}...";
                ErrorMessage = string.Empty;
                
                _logger.LogInformation("开始保存模板: {TemplateName}", SelectedTemplate.Name);

                // 更新修改时间
                SelectedTemplate.ModifiedAt = DateTime.Now;

                // 保存模板
                var savedTemplate = await _templateService.SaveTemplateAsync(SelectedTemplate);
                
                // 更新模板引用
                var index = Templates.IndexOf(SelectedTemplate);
                if (index >= 0)
                {
                    Templates[index] = savedTemplate;
                    SelectedTemplate = savedTemplate;
                }

                StatusMessage = $"已保存模板: {SelectedTemplate.Name}";
                _logger.LogInformation("成功保存模板: {TemplateName}", SelectedTemplate.Name);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"保存模板失败: {ex.Message}";
                _logger.LogError(ex, "保存模板失败: {TemplateName}", SelectedTemplate?.Name);
            }
            finally
            {
                IsSaving = false;
            }
        }

        /// <summary>
        /// 删除模板
        /// </summary>
        private async Task DeleteTemplateAsync()
        {
            if (SelectedTemplate == null || IsSaving || SelectedTemplate.IsDefault) return;

            try
            {
                IsSaving = true;
                StatusMessage = $"正在删除模板: {SelectedTemplate.Name}...";
                ErrorMessage = string.Empty;
                
                _logger.LogWarning("开始删除模板: {TemplateName} ({TemplateId})", 
                    SelectedTemplate.Name, SelectedTemplate.Id);

                // 确认框
                var result = System.Windows.MessageBox.Show(
                    $"确定要删除模板 \"{SelectedTemplate.Name}\" 吗？此操作不可恢复。",
                    "确认删除",
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Warning);

                if (result != System.Windows.MessageBoxResult.Yes)
                {
                    _logger.LogInformation("用户取消了删除模板操作");
                    StatusMessage = "已取消删除操作";
                    return;
                }

                // 删除模板
                var templateToDelete = SelectedTemplate;
                SelectedTemplate = null; // 先清空选择，避免引用已删除对象
                
                bool deleted = await _templateService.DeleteTemplateAsync(templateToDelete.Id);

                if (deleted)
                {
                    Templates.Remove(templateToDelete);
                    StatusMessage = $"已删除模板: {templateToDelete.Name}";
                    _logger.LogInformation("成功删除模板: {TemplateName} ({TemplateId})", 
                        templateToDelete.Name, templateToDelete.Id);
                    
                    // 如果还有其他模板，选择第一个
                    if (Templates.Count > 0)
                    {
                        SelectedTemplate = Templates[0];
                    }
                }
                else
                {
                    ErrorMessage = "删除模板失败";
                    _logger.LogError("删除模板失败: {TemplateName} ({TemplateId})", 
                        templateToDelete.Name, templateToDelete.Id);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"删除模板失败: {ex.Message}";
                _logger.LogError(ex, "删除模板时发生异常: {TemplateName}", SelectedTemplate?.Name);
            }
            finally
            {
                IsSaving = false;
            }
        }

        /// <summary>
        /// 导出图片
        /// </summary>
        private async Task ExportImageAsync()
        {
            if (string.IsNullOrEmpty(PreviewContent) || IsLoading) return;

            try
            {
                IsLoading = true;
                StatusMessage = "正在导出图片...";
                ErrorMessage = string.Empty;
                
                _logger.LogInformation("开始导出模板为图片: {TemplateName}", SelectedTemplate?.Name);

                // 这里实现导出图片的逻辑
                // 实际应用中可能需要调用外部库将SVG转换为图片

                await Task.Delay(500); // 模拟处理时间

                StatusMessage = "图片导出成功";
                _logger.LogInformation("成功导出模板为图片");
            }
            catch (Exception ex)
            {
                ErrorMessage = $"导出图片失败: {ex.Message}";
                _logger.LogError(ex, "导出图片失败");
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// 预览更新后的模板内容
        /// </summary>
        public void UpdatePreview()
        {
            if (SelectedTemplate == null) return;
            
            try
            {
                _logger.LogDebug("更新模板预览");
                
                // 收集变量值
                var variableValues = new System.Collections.Generic.Dictionary<string, object>();
                
                foreach (var variableVm in TemplateVariables)
                {
                    variableValues[variableVm.Name] = variableVm.Value;
                }
                
                // 应用变量到模板内容
                var updatedContent = _templateService.ApplyVariables(SelectedTemplate, variableValues);
                PreviewContent = updatedContent;
                
                _logger.LogDebug("模板预览已更新，内容长度: {Length} 字符", PreviewContent?.Length ?? 0);
            }
            catch (Exception ex)
            {
                ErrorMessage = $"更新预览失败: {ex.Message}";
                _logger.LogError(ex, "更新模板预览失败");
            }
        }
    }

    
}