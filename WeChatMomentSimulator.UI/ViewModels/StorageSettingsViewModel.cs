using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using WeChatMomentSimulator.Core.Interfaces;
using WeChatMomentSimulator.Services.Storage;
using Microsoft.Extensions.Logging;
using WeChatMomentSimulator.UI.ViewModels.Templates;

namespace WeChatMomentSimulator.UI.ViewModels
{
    public class StorageSettingsViewModel : ViewModelBase
    {
        private readonly ILogger _logger;
        private readonly FileTemplateStorage _fileTemplateStorage;

        public StorageSettingsViewModel(ILogger<StorageSettingsViewModel> logger, FileTemplateStorage fileTemplateStorage)
        {
            _logger = logger;
            _fileTemplateStorage = fileTemplateStorage;

            // 初始化命令
            ChangeStorageLocationCommand = new RelayCommand(_=>ChangeStorageLocation());
            RefreshStorageInfoCommand = new RelayCommand(_=>RefreshStorageInfo());

            // 初始化数据
            RefreshStorageInfo();
        }

        // 存储位置属性
        private string _storageLocation;
        public string StorageLocation
        {
            get => _storageLocation;
            set => SetProperty(ref _storageLocation, value);
        }

        // 存储使用情况
        private string _storageUsage;
        public string StorageUsage
        {
            get => _storageUsage;
            set => SetProperty(ref _storageUsage, value);
        }

        // 模板数量
        private int _templateCount;
        public int TemplateCount
        {
            get => _templateCount;
            set => SetProperty(ref _templateCount, value);
        }

        // 命令
        public ICommand ChangeStorageLocationCommand { get; }
        public ICommand RefreshStorageInfoCommand { get; }

        // 刷新存储信息
        private void RefreshStorageInfo()
        {
            try
            {
                // 获取存储位置
                StorageLocation = _fileTemplateStorage.GetStoragePath();

                // 计算存储使用情况
               // var storageInfo = _fileTemplateStorage.GetStorageUsage();
                //StorageUsage = $"{storageInfo.UsedSpaceMB} MB / {storageInfo.TotalSpaceMB} MB";

                // 获取模板数量
                TemplateCount = _fileTemplateStorage.GetTemplateCount();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "刷新存储信息时出错");
            }
        }

        // 更改存储位置
        private void ChangeStorageLocation()
        {
            try
            {
                // 实现更改存储位置的逻辑
                // 这里可以打开文件夹选择对话框
                // 然后调用_fileTemplateStorage.ChangeStorageLocation(newPath)
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更改存储位置时出错");
            }
        }
    }
}