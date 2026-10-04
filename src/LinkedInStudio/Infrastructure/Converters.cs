using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace LinkedInStudio.Infrastructure;

public sealed class NullToVisibilityConverter : IValueConverter
{
    /// <summary>When true (default), null → Visible. When "invert" parameter, null → Collapsed.</summary>
    public object Convert(object? value, Type t, object? parameter, CultureInfo c)
    {
        bool isNull = value is null || (value is string s && s.Length == 0);
        bool invert = parameter as string == "invert";
        bool visible = invert ? !isNull : isNull;
        return visible ? Visibility.Visible : Visibility.Collapsed;
    }

    public object ConvertBack(object? value, Type t, object? p, CultureInfo c) => throw new NotSupportedException();
}

/// <summary>MultiBinding [key, languageTrigger] → localized string (re-evaluates on language change).</summary>
public sealed class LocMultiConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type t, object? parameter, CultureInfo c)
    {
        var key = values.Length > 0 ? values[0]?.ToString() : null;
        return string.IsNullOrEmpty(key) ? "" : Localization.Localizer.Instance[key];
    }

    public object[] ConvertBack(object? value, Type[] t, object? p, CultureInfo c) => throw new NotSupportedException();
}

public sealed class InverseBoolConverter : IValueConverter
{
    public object Convert(object? value, Type t, object? p, CultureInfo c) => !(value is bool b && b);
    public object ConvertBack(object? value, Type t, object? p, CultureInfo c) => !(value is bool b && b);
}

public sealed class BrushFromHexConverter : IValueConverter
{
    public object Convert(object? value, Type t, object? parameter, CultureInfo c) => ColorUtil.Brush(value as string);
    public object ConvertBack(object? value, Type t, object? p, CultureInfo c) => throw new NotSupportedException();
}

/// <summary>MultiBinding [Name, LabelKey, languageTrigger] → custom name or localized widget label.</summary>
public sealed class LayerLabelConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type t, object? parameter, CultureInfo c)
    {
        var name = values.Length > 0 ? values[0] as string : null;
        if (!string.IsNullOrWhiteSpace(name)) return name!;
        var key = values.Length > 1 ? values[1]?.ToString() : null;
        return string.IsNullOrEmpty(key) ? "" : Localization.Localizer.Instance[key];
    }

    public object[] ConvertBack(object? value, Type[] t, object? p, CultureInfo c) => throw new NotSupportedException();
}

public sealed class EnumMatchConverter : IValueConverter
{
    public object Convert(object? value, Type t, object? parameter, CultureInfo c)
        => value?.ToString() == parameter?.ToString();

    public object? ConvertBack(object? value, Type t, object? parameter, CultureInfo c)
        => (value is bool b && b) ? Enum.Parse(t, parameter!.ToString()!) : Binding.DoNothing;
}
