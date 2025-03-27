using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using WeChatMomentSimulator.Core.DataBinding;
using WeChatMomentSimulator.Core.Models.Template;
using WeChatMomentSimulator.Desktop.ViewModels.Base;
using WeChatMomentSimulator.Services.DataBinding;

namespace WeChatMomentSimulator.Desktop.ViewModels
{
    /// <summary>
    /// 占位符编辑器视图模型
    /// </summary>
    public class PlaceholderEditorViewModel : ViewModelBase
    {
        private readonly ILogger<PlaceholderEditorViewModel> _logger;
        private readonly DataBindingContext _bindingContext;
        private readonly IDataProvider _dataProvider;
        private KeyValuePair<string, Dictionary<string, object>> _selectedDataSet;
        
        /// <summary>
        /// 数据集合
        /// </summary>
        public ObservableCollection<KeyValuePair<string, Dictionary<string, object>>> DataSets { get; } = 
            new ObservableCollection<KeyValuePair<string, Dictionary<string, object>>>();
            
        /// <summary>
        /// 按分类组织的占位符绑定
        /// </summary>
        public ObservableCollection<KeyValuePair<string, ObservableCollection<PlaceholderBindingViewModel>>> CategoryBindings { get; } =
            new ObservableCollection<KeyValuePair<string, ObservableCollection<PlaceholderBindingViewModel>>>();
            
        /// <summary>
        /// 选中的数据集
        /// </summary>
        public KeyValuePair<string, Dictionary<string, object>> SelectedDataSet
        {
            get => _selectedDataSet;
            set
            {
                if (SetProperty(ref _selectedDataSet, value))
                {
                    // 加载选中的数据集
                    if (_dataProvider is MockDataProvider mockProvider && !string.IsNullOrEmpty(value.Key))
                    {
                        mockProvider.LoadDataSet(value.Key);
                    }
                }
            }
        }
        
        /// <summary>
        /// 浏览图片命令
        /// </summary>
        public ICommand BrowseImageCommand { get; }
        
        /// <summary>
        /// 编辑列表命令
        /// </summary>
        public ICommand EditListCommand { get; }
        
        /// <summary>
        /// 生成随机数据命令
        /// </summary>
        public ICommand GenerateRandomDataCommand { get; }
        
        /// <summary>
        /// 生成随机值命令
        /// </summary>
        public ICommand GenerateRandomValueCommand { get; }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public PlaceholderEditorViewModel(
            DataBindingContext bindingContext,
            IDataProvider dataProvider,
            ILogger<PlaceholderEditorViewModel> logger)
        {
            _bindingContext = bindingContext ?? throw new ArgumentNullException(nameof(bindingContext));
            _dataProvider = dataProvider ?? throw new ArgumentNullException(nameof(dataProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            // 命令初始化
            BrowseImageCommand = new RelayCommand<PlaceholderBindingViewModel>(ExecuteBrowseImage);
            EditListCommand = new RelayCommand<PlaceholderBindingViewModel>(ExecuteEditList);
            GenerateRandomDataCommand = new RelayCommand(ExecuteGenerateRandomData);
            GenerateRandomValueCommand = new RelayCommand<PlaceholderBindingViewModel>(ExecuteGenerateRandomValue);
            
            // 初始化预设数据集
            if (_dataProvider is MockDataProvider mockProvider)
            {
                // 获取预设数据集
                LoadDataSets(mockProvider);
            }
            
            // 监听绑定上下文的变化
            _bindingContext.DataChanged += OnDataBindingChanged;
            
            // 初始化分类绑定集合
            UpdateCategoryBindings();
        }
        
        /// <summary>
        /// 更新模板
        /// </summary>
        public void UpdateTemplate(string templateContent)
        {
            _bindingContext.SetTemplate(templateContent);
            UpdateCategoryBindings();
        }
        
        /// <summary>
        /// 更新分类绑定集合
        /// </summary>
        private void UpdateCategoryBindings()
        {
            CategoryBindings.Clear();
            
            foreach (var category in _bindingContext.CategoryBindings)
            {
                var bindingVMs = new ObservableCollection<PlaceholderBindingViewModel>();
                
                foreach (var binding in category.Value)
                {
                    var bindingVM = new PlaceholderBindingViewModel(binding, this);
                    bindingVMs.Add(bindingVM);
                }
                
                CategoryBindings.Add(new KeyValuePair<string, ObservableCollection<PlaceholderBindingViewModel>>(
                    category.Key, bindingVMs));
            }
        }
        
        /// <summary>
        /// 加载数据集
        /// </summary>
        private void LoadDataSets(MockDataProvider mockProvider)
        {
            // 假设MockDataProvider有一个方法可以获取所有预设数据集
            // 这里需要根据实际情况调整
            var dataSets = new Dictionary<string, Dictionary<string, object>>
            {
                { "标准数据", mockProvider.GetAllData() },
                // 可以添加更多预设数据集
            };
            
            DataSets.Clear();
            foreach (var dataSet in dataSets)
            {
                DataSets.Add(new KeyValuePair<string, Dictionary<string, object>>(dataSet.Key, dataSet.Value));
            }
            
            // 默认选中第一个数据集
            if (DataSets.Count > 0)
            {
                SelectedDataSet = DataSets[0];
            }
        }
        
        /// <summary>
        /// 数据绑定变化处理
        /// </summary>
        private void OnDataBindingChanged(object sender, DataChangedEventArgs e)
        {
            // 如果是批量更新，则重新创建所有绑定
            if (e.IsBulkUpdate)
            {
                UpdateCategoryBindings();
            }
        }
        
        /// <summary>
        /// 执行浏览图片命令
        /// </summary>
        private void ExecuteBrowseImage(PlaceholderBindingViewModel binding)
        {
            try
            {
                var dialog = new OpenFileDialog
                {
                    Title = "选择图片",
                    Filter = "图片文件 (*.png;*.jpg;*.jpeg;*.bmp)|*.png;*.jpg;*.jpeg;*.bmp|所有文件 (*.*)|*.*",
                    CheckFileExists = true
                };
                
                if (dialog.ShowDialog() == true)
                {
                    binding.Value = dialog.FileName;
                    _bindingContext.UpdateBinding(binding.Definition.Name, dialog.FileName);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "浏览图片失败");
            }
        }
        
        /// <summary>
        /// 执行编辑列表命令
        /// </summary>
        private void ExecuteEditList(PlaceholderBindingViewModel binding)
        {
            // 此处应打开一个列表编辑对话框
            _logger.LogInformation("请实现列表编辑功能");
        }
        
        /// <summary>
        /// 执行生成随机数据命令
        /// </summary>
        private void ExecuteGenerateRandomData()
        {
            try
            {
                if (_dataProvider is MockDataProvider mockProvider)
                {
                    mockProvider.GenerateRandomData();
                    _logger.LogInformation("已生成随机数据");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成随机数据失败");
            }
        }
        
        /// <summary>
        /// 执行生成随机值命令
        /// </summary>
        private void ExecuteGenerateRandomValue(PlaceholderBindingViewModel binding)
        {
            try
            {
                if (_dataProvider is MockDataProvider mockProvider)
                {
                    object randomValue = mockProvider.GenerateRandomValue(binding.Definition.Type);
                    binding.Value = randomValue;
                    _bindingContext.UpdateBinding(binding.Definition.Name, randomValue);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "生成随机值失败");
            }
        }
    }
    
    /// <summary>
    /// 占位符绑定视图模型
    /// </summary>
    public class PlaceholderBindingViewModel : ViewModelBase
    {
        private readonly PlaceholderBinding _binding;
        private readonly PlaceholderEditorViewModel _parent;
        
        /// <summary>
        /// 占位符定义
        /// </summary>
        public PlaceholderDefinition Definition => _binding.Definition;
        
        /// <summary>
        /// 当前值
        /// </summary>
        public object Value
        {
            get => _binding.Value;
            set
            {
                if (_binding.Value != value)
                {
                    _binding.Value = value;
                    OnPropertyChanged();
                }
            }
        }
        
        /// <summary>
        /// 是否支持随机值生成
        /// </summary>
        public bool SupportsRandom
        {
            get
            {
                // 只有某些类型支持随机生成
                return Definition.Type == PlaceholderType.Text ||
                       Definition.Type == PlaceholderType.Number ||
                       Definition.Type == PlaceholderType.Boolean;
            }
        }
        
        /// <summary>
        /// 构造函数
        /// </summary>
        public PlaceholderBindingViewModel(PlaceholderBinding binding, PlaceholderEditorViewModel parent)
        {
            _binding = binding ?? throw new ArgumentNullException(nameof(binding));
            _parent = parent ?? throw new ArgumentNullException(nameof(parent));
        }
    }
}