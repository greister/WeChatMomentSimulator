using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using WeChatMomentSimulator.Core.Interfaces;
using WeChatMomentSimulator.Core.Models.Template;
using WeChatMomentSimulator.Services.Storage;

namespace WeChatMomentSimulator.UI.ViewModels.Templates
{
    /// <summary>
    /// 模板列表视图模型
    /// </summary>
    public class TemplateListViewModel : INotifyPropertyChanged
    {
        private readonly ILogger<TemplateListViewModel> _logger;
        private readonly TemplateManager _templateManager;
        private ObservableCollection<TemplateDefinition> _templates;
        private bool _isLoading;
        private bool _isBusy;
        private string _busyMessage;
        private string _searchText;
        private string _selectedCategory;
        private TemplateDefinition _selectedTemplate;
        
        /// <summary>
        /// 模板选择事件
        /// </summary>
        public event EventHandler<TemplateDefinition> TemplateSelected;

        /// <summary>
        /// 所有模板
        /// </summary>
        public ObservableCollection<TemplateDefinition> Templates
        {
            get => _templates;
            set
            {
                _templates = value;
                OnPropertyChanged();
                // 更新筛选后的模板
                ApplyFilters();
            }
        }

        private ObservableCollection<TemplateDefinition> _filteredTemplates;
        /// <summary>
        /// 筛选后的模板
        /// </summary>
        public ObservableCollection<TemplateDefinition> FilteredTemplates
        {
            get => _filteredTemplates;
            set
            {
                _filteredTemplates = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasTemplates));
                OnPropertyChanged(nameof(TemplateCountText));
            }
        }

        /// <summary>
        /// 是否正在加载
        /// </summary>
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 是否正忙
        /// </summary>
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 忙时显示的消息
        /// </summary>
        public string BusyMessage
        {
            get => _busyMessage;
            set
            {
                _busyMessage = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 搜索文本
        /// </summary>
        public string SearchText
        {
            get => _searchText;
            set
            {
                _searchText = value;
                OnPropertyChanged();
                ApplyFilters();
            }
        }

        /// <summary>
        /// 所有分类
        /// </summary>
        public ObservableCollection<string> Categories { get; private set; }

        /// <summary>
        /// 选中的分类
        /// </summary>
        public string SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                _selectedCategory = value;
                OnPropertyChanged();
                ApplyFilters();
            }
        }

        /// <summary>
        /// 选中的模板
        /// </summary>
        public TemplateDefinition SelectedTemplate
        {
            get => _selectedTemplate;
            set
            {
                _selectedTemplate = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// 是否有模板
        /// </summary>
        public bool HasTemplates => FilteredTemplates != null && FilteredTemplates.Count > 0;

        /// <summary>
        /// 模板计数文本
        /// </summary>
        public string TemplateCountText => FilteredTemplates == null ? "无模板" :
            (FilteredTemplates.Count == 0 ? "无匹配模板" :
            (FilteredTemplates.Count == Templates.Count ?
                $"共 {FilteredTemplates.Count} 个模板" :
                $"显示 {FilteredTemplates.Count} / {Templates.Count} 个模板"));

        // 命令
        public ICommand RefreshCommand { get; }
        public ICommand CreateTemplateCommand { get; }
        public ICommand ImportTemplateCommand { get; }
        public ICommand SelectTemplateCommand { get; }

        /// <summary>
        /// 初始化模板列表视图模型
        /// </summary>
        /// <param name="templateManager">模板管理器</param>
        /// <param name="logger">日志服务</param>
        public TemplateListViewModel(TemplateManager templateManager, ILogger<TemplateListViewModel> logger)
        {
            _templateManager = templateManager ?? throw new ArgumentNullException(nameof(templateManager));
            _logger = logger;

            _templates = new ObservableCollection<TemplateDefinition>();
            _filteredTemplates = new ObservableCollection<TemplateDefinition>();
            Categories = new ObservableCollection<string>();

            RefreshCommand = new RelayCommand(async _ => await LoadTemplatesAsync());
            CreateTemplateCommand = new RelayCommand(async _ => await CreateTemplateAsync());
            ImportTemplateCommand = new RelayCommand(async _ => await ImportTemplateAsync());
            SelectTemplateCommand = new RelayCommand(OnTemplateSelected);

            _logger?.LogDebug("TemplateListViewModel 已初始化");
        }

        private async Task<object> LoadTemplatesAsync()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 初始化加载
        /// </summary>
        public async Task InitializeAsync()
        {
            await LoadTemplatesAsync();
            await LoadCategoriesAsync();
            try
            {
                var allCategories = await _templateManager.GetAllCategoriesAsync();
                Categories.Clear();
                foreach (var category in allCategories)
                {
                    Categories.Add(category);
                }
                
                // 默认选择"全部"
                SelectedCategory = Categories.FirstOrDefault();
                
                _logger?.LogDebug("成功加载 {CategoryCount} 个分类", Categories.Count - 1); // 减去"全部"选项
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "加载分类列表失败");
            }
        }

        private async Task LoadCategoriesAsync()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// 创建新模板
        /// </summary>
        private async Task CreateTemplateAsync()
        {
            try
            {
                IsBusy = true;
                BusyMessage = "正在创建新模板...";
                _logger?.LogDebug("开始创建新模板");

                var template = await _templateManager.CreateTemplateAsync("新建模板");
                if (template != null)
                {
                    // 添加到列表并选中
                    Templates.Add(template);
                    SelectedTemplate = template;
                    ApplyFilters();
                    
                    // 触发选中事件
                    TemplateSelected?.Invoke(this, template);
                    
                    _logger?.LogInformation("成功创建新模板: {TemplateId}", template.Id);
                }
                else
                {
                    _logger?.LogWarning("创建新模板失败");
                    MessageBox.Show("创建新模板失败", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "创建新模板时出错");
                MessageBox.Show($"创建新模板失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        /// <summary>
        /// 导入模板
        /// </summary>
        private async Task ImportTemplateAsync()
        {
            // 打开文件对话框
            var openFileDialog = new OpenFileDialog
            {
                Title = "选择要导入的模板文件",
                Filter = "模板文件 (*.json)|*.json|所有文件 (*.*)|*.*",
                Multiselect = false
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    IsBusy = true;
                    BusyMessage = "正在导入模板...";
                    _logger?.LogDebug("开始导入模板: {FileName}", openFileDialog.FileName);

                    var template = await _templateManager.ImportTemplateAsync(openFileDialog.FileName);
                    if (template != null)
                    {
                        // 添加到列表并选中
                        Templates.Add(template);
                        SelectedTemplate = template;
                        ApplyFilters();
                        
                        // 刷新分类
                        await LoadCategoriesAsync();
                        
                        // 触发选中事件
                        TemplateSelected?.Invoke(this, template);
                        
                        _logger?.LogInformation("成功导入模板: {TemplateId}", template.Id);
                        MessageBox.Show("模板导入成功", "操作成功", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        _logger?.LogWarning("导入模板失败");
                        MessageBox.Show("导入模板失败，请确保文件格式正确", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    _logger?.LogError(ex, "导入模板时出错");
                    MessageBox.Show($"导入模板失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    IsBusy = false;
                }
            }
        }

        /// <summary>
        /// 模板选中处理
        /// </summary>
        private void OnTemplateSelected(object parameter)
        {
            if (parameter is TemplateDefinition template)
            {
                SelectedTemplate = template;
                _logger?.LogDebug("选中模板: {TemplateName} ({TemplateId})", template.Metadata?.Name, template.Id);
                TemplateSelected?.Invoke(this, template);
            }
        }

        /// <summary>
        /// 应用筛选
        /// </summary>
        private void ApplyFilters()
        {
            if (Templates == null)
            {
                FilteredTemplates = new ObservableCollection<TemplateDefinition>();
                return;
            }

            IEnumerable<TemplateDefinition> results = Templates;

            // 应用分类筛选
            if (!string.IsNullOrEmpty(SelectedCategory) && SelectedCategory != "全部")
            {
                results = results.Where(t => t.Metadata?.Category == SelectedCategory);
            }

            // 应用搜索文本筛选
            if (!string.IsNullOrEmpty(SearchText))
            {
                var searchTerm = SearchText.ToLowerInvariant();
                results = results.Where(t =>
                    (t.Metadata?.Name?.ToLowerInvariant().Contains(searchTerm) ?? false) ||
                    (t.Metadata?.Description?.ToLowerInvariant().Contains(searchTerm) ?? false) ||
                    (t.Metadata?.Author?.ToLowerInvariant().Contains(searchTerm) ?? false)
                );
            }

            FilteredTemplates = new ObservableCollection<TemplateDefinition>(results);
            _logger?.LogDebug("应用筛选: {FilteredCount}/{TotalCount} 个模板匹配", FilteredTemplates.Count, Templates.Count);
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }

    /// <summary>
    /// 用于绑定ICommand的简单实现
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

        public bool CanExecute(object parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            _execute(parameter);
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}