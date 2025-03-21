using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace WeChatMomentSimulator.Services.Storage
{
    public class FileStorageService
    {
        private readonly ILogger<FileStorageService> _logger;
        private readonly string _basePath;

        public FileStorageService(ILogger<FileStorageService> logger)
        {
            _logger = logger;
            _basePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "WeChatMomentSimulator");
                
            if (!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
                _logger.LogInformation($"Created application data directory: {_basePath}");
            }
        }

        public async Task<T> ReadJsonAsync<T>(string relativePath)
        {
            var fullPath = Path.Combine(_basePath, relativePath);
            if (!File.Exists(fullPath))
            {
                _logger.LogWarning($"File not found: {fullPath}");
                return default;
            }

            try
            {
                var json = await File.ReadAllTextAsync(fullPath);
                return JsonSerializer.Deserialize<T>(json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error reading file: {fullPath}");
                throw;
            }
        }

        public async Task WriteJsonAsync<T>(string relativePath, T content)
        {
            var fullPath = Path.Combine(_basePath, relativePath);
            var directory = Path.GetDirectoryName(fullPath);
            
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            try
            {
                var json = JsonSerializer.Serialize(content, new JsonSerializerOptions 
                { 
                    WriteIndented = true 
                });
                
                await File.WriteAllTextAsync(fullPath, json);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error writing to file: {fullPath}");
                throw;
            }
        }
    }
}