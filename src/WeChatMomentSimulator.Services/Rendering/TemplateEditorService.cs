using Microsoft.Extensions.Logging;
using WeChatMomentSimulator.Core.Interfaces.Services;

public class TemplateEditorService : ITemplateEditorService
{
    private readonly ILogger<TemplateEditorService> _logger;
    private readonly ITemplateManager _templateManager;
    private readonly IDialogService _dialogService;

    public TemplateEditorService(
        ILogger<TemplateEditorService> logger,
        ITemplateManager templateManager,
        IDialogService dialogService)
    {
        _logger = logger;
        _templateManager = templateManager;
        _dialogService = dialogService;
    }

    public Task<string> CreateNewTemplateAsync()
    {
        var template = @"<svg xmlns='http://www.w3.org/2000/svg' width='375' height='667'>
    <rect width='100%' height='100%' fill='#F5F5F5'/>
    <rect x='0' y='0' width='375' height='20' fill='#2E2E2E'/>
</svg>";
        return Task.FromResult(template);
    }

    public async Task<(string content, string name)> OpenTemplateAsync()
    {
        try
        {
            string filePath = _dialogService.ShowOpenFileDialog(
                "打开SVG模板",
                "SVG文件(*.svg)|*.svg|所有文件(*.*)|*.*");

            if (string.IsNullOrEmpty(filePath))
                return (null, null);

            string fileName = Path.GetFileNameWithoutExtension(filePath);
            string content = await File.ReadAllTextAsync(filePath);

            // 检查模板是否已存在
            if (await _templateManager.TemplateExistsAsync(fileName))
            {
                fileName = $"{fileName}_{DateTime.Now:yyyyMMddHHmmss}";
            }

            // 保存为新模板
            await _templateManager.SaveTemplateAsync(fileName, content);
            _logger.LogInformation("已导入模板: {FileName} from {FilePath}", fileName, filePath);

            return (content, fileName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "打开模板失败");
            return (null, null);
        }
    }

    public async Task<string> SaveAsTemplateAsync(string content, string currentName)
    {
        try
        {
            string filePath = _dialogService.ShowSaveFileDialog(
                "保存SVG模板",
                "SVG文件(*.svg)|*.svg");

            if (string.IsNullOrEmpty(filePath))
                return null;

            string templateName = Path.GetFileNameWithoutExtension(filePath);
            await _templateManager.SaveTemplateAsync(templateName, content);
            _logger.LogInformation("模板已保存为: {TemplateName}", templateName);

            return templateName;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "另存模板失败");
            throw;
        }
    }

    public async Task SaveTemplateAsync(string templateName, string content)
    {
        try
        {
            await _templateManager.SaveTemplateAsync(templateName, content);
            _logger.LogInformation("模板已保存: {TemplateName}", templateName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "保存模板失败");
            throw;
        }
    }

   
}