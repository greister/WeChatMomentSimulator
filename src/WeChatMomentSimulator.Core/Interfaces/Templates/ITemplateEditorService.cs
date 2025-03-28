namespace WeChatMomentSimulator.Core.Interfaces.Services;

public interface ITemplateEditorService
{
    Task<string> CreateNewTemplateAsync();
    Task<(string content, string name)> OpenTemplateAsync();
    Task SaveTemplateAsync(string templateName, string content);
    Task<string> SaveAsTemplateAsync(string content, string currentName);
}