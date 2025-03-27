using System;
using System.IO;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using WeChatMomentSimulator.Core.Logging;

namespace WeChatMomentSimulator.Core.Interfaces.Services
{
    public interface ISettingsService
    {
        ApplicationSettings LoadSettings();
        void SaveSettings(ApplicationSettings settings);
    }

    public class ApplicationSettings
    {
        // Basic application settings
        public string LastOpenedFile { get; set; }
        public bool AutoSave { get; set; } = true;
        public int AutoSaveInterval { get; set; } = 5; // Minutes
        public string DefaultExportPath { get; set; }
        
        // UI preferences
        public bool UseDarkMode { get; set; }
        public double WindowWidth { get; set; } = 1200;
        public double WindowHeight { get; set; } = 800;
        public bool RememberWindowSize { get; set; } = true;
        /// <summary>
        /// 上次打开的项目路径
        /// </summary>
        public string LastProjectPath { get; set; }

        /// <summary>
        /// 当前项目路径
        /// </summary>
        public string CurrentProjectPath { get; set; }
    }
}