using System.Windows.Media;

namespace LinkedInStudio.Infrastructure;

/// <summary>Code-side mirror of the design tokens, so model/highlighter code can build brushes.</summary>
public static class Palette
{
    public static SolidColorBrush Frozen(string hex)
    {
        var b = new SolidColorBrush((Color)ColorConverter.ConvertFromString(hex)!);
        b.Freeze();
        return b;
    }

    public static readonly SolidColorBrush Navy = Frozen("#17233F");
    public static readonly SolidColorBrush Green = Frozen("#2E9E52");
    public static readonly SolidColorBrush GreenDark = Frozen("#1E8E4E");
    public static readonly SolidColorBrush Red = Frozen("#D93A2E");
    public static readonly SolidColorBrush Amber = Frozen("#E8912A");
    public static readonly SolidColorBrush Blue = Frozen("#2F80D8");
    public static readonly SolidColorBrush Purple = Frozen("#6C4BB6");
    public static readonly SolidColorBrush Muted = Frozen("#5B6472");
    public static readonly SolidColorBrush White = Frozen("#FFFFFF");

    // Code token brushes
    public static readonly SolidColorBrush CodeText = Frozen("#D5DCE8");
    public static readonly SolidColorBrush CodeKeyword = Frozen("#4FA6E0");
    public static readonly SolidColorBrush CodeType = Frozen("#38BDB0");
    public static readonly SolidColorBrush CodeString = Frozen("#E0A15E");
    public static readonly SolidColorBrush CodeNumber = Frozen("#C69AF0");
    public static readonly SolidColorBrush CodeComment = Frozen("#6FCF97");
    public static readonly SolidColorBrush CodePunct = Frozen("#9AA6BC");
}
