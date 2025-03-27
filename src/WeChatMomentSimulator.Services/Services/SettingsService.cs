using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text.Json;
using WeChatMomentSimulator.Core.Interfaces.Services;
using WeChatMomentSimulator.Core.Logging;
using LoggerExtensions = WeChatMomentSimulator.Core.Logging.LoggerExtensions;

namespace WeChatMomentSimulator.Desktop.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly ILogger<SettingsService> _logger;
        private readonly string _settingsFilePath;
        private string _lastProjectPath;

        public SettingsService()
        {
            _logger = LoggerExtensions.GetLogger<SettingsService>();
            string appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "WeChatMomentSimulator");

            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }

            _settingsFilePath = Path.Combine(appDataPath, "settings.json");
            _logger.LogInformation("设置文件路径: {Path}", _settingsFilePath);
        }

        public string LastProjectPath
        {
            get => _lastProjectPath;
            set => _lastProjectPath = value;
        }

        public ApplicationSettings LoadSettings()
        {
            using ("Method".LogContext("LoadSettings"))
            {
                try
                {
                    if (File.Exists(_settingsFilePath))
                    {
                        string json = File.ReadAllText(_settingsFilePath);
                        var settings = JsonSerializer.Deserialize<ApplicationSettings>(json);
                        _logger.LogInformation("已成功加载设置");
                        return settings;
                    }

                    _logger.LogInformation("设置文件不存在，将返回默认设置");
                    return new ApplicationSettings();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "加载设置失败");
                    return new ApplicationSettings();
                }
            }
        }

        public void SaveSettings(ApplicationSettings settings)
        {
            using ("Method".LogContext("SaveSettings"))
            {
                try
                {
                    var options = new JsonSerializerOptions { WriteIndented = true };
                    string json = JsonSerializer.Serialize(settings, options);
                    File.WriteAllText(_settingsFilePath, json);
                    _logger.LogInformation("已成功保存设置");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "保存设置失败");
                    throw;
                }
            }
        }
    }
}