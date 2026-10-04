using System.Windows.Media;

namespace LinkedInStudio.Infrastructure;

public static class ColorUtil
{
    public static SolidColorBrush Brush(string? hex)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(hex)) return Palette.Navy;
            var c = (Color)ColorConverter.ConvertFromString(hex)!;
            return new SolidColorBrush(c);
        }
        catch
        {
            return Palette.Navy;
        }
    }

    public static string ToHex(Color c) => $"#{c.R:X2}{c.G:X2}{c.B:X2}";

    /// <summary>A translucent version of the colour (for soft tinted tiles).</summary>
    public static SolidColorBrush Alpha(string? hex, byte a)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(hex)) return new SolidColorBrush(Color.FromArgb(a, 0, 0, 0));
            var c = (Color)ColorConverter.ConvertFromString(hex)!;
            return new SolidColorBrush(Color.FromArgb(a, c.R, c.G, c.B));
        }
        catch
        {
            return new SolidColorBrush(Color.FromArgb(a, 0, 0, 0));
        }
    }
}
