using System.Windows;
using System.Windows.Controls;

namespace LinkedInStudio.Infrastructure;

/// <summary>Selects a DataTemplate by resource key "&lt;Prefix&gt;&lt;TypeName&gt;".</summary>
public sealed class KeyedTemplateSelector : DataTemplateSelector
{
    public string Prefix { get; set; } = "V.";

    public override DataTemplate? SelectTemplate(object? item, DependencyObject container)
    {
        if (item is null || container is not FrameworkElement fe) return null;
        var key = Prefix + item.GetType().Name;
        return fe.TryFindResource(key) as DataTemplate;
    }
}
