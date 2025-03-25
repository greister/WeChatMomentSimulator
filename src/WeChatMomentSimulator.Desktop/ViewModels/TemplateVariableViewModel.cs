using System.ComponentModel;
using System.Runtime.CompilerServices;
using WeChatMomentSimulator.Core.Models.Template;
using WeChatMomentSimulator.Core.Models.Template.Enums;
using System.Collections.Generic;
using WeChatMomentSimulator.Core.Models.Template.Enums;

namespace WeChatMomentSimulator.Desktop.ViewModels;

/// <summary>
/// 模板变量视图模型
/// </summary>
public class TemplateVariableViewModel : INotifyPropertyChanged
{
    private readonly TemplateVariable _variable;
    private object _value;
        
        
    public TemplateVariableViewModel(TemplateVariable variable)
    {
        _variable = variable ?? throw new ArgumentNullException(nameof(variable));
        //_value = variable.Value ?? variable.DefaultValue;
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public string Name => _variable.Name;

    public string DisplayName => _variable.DisplayName;

    public string Description => _variable.Description;

    public VariableType Type => _variable.Type;


    public bool IsRequired => _variable.IsRequired;

    public System.Collections.Generic.List<string> Options => _variable.Options.ToList();
    public object Value
    {
        get => _value;
        set
        {
            if (_value != value)
            {
                _value = value;
                _variable.Value = value; // 关键：同步到模型
                OnPropertyChanged();
            }
        }
    }
}