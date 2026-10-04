using System.Windows;
using System.Windows.Controls;
using LinkedInStudio.Models;

namespace LinkedInStudio.Services;

/// <summary>
/// Content-aware sizing. Widgets are absolutely positioned on the canvas, so their height has to be
/// chosen by hand; this measures the real visual and returns the height the content needs
/// (including wrapped Persian / English text), so nothing is clipped and no empty space is left.
/// </summary>
public static class LayoutService
{
    /// <summary>Widgets whose size is a design choice rather than a function of their content.</summary>
    public static bool IsFixedSize(ItemBase item) =>
        item is ImageItem or DividerItem or ArrowItem or VsBadgeItem or IconItem or IllustrationItem;

    /// <summary>Boxes that look best when equal-height siblings in one row share the tallest height.</summary>
    public static bool IsStretchy(ItemBase item) =>
        item is ProblemBoxItem or SolutionBoxItem or CustomBoxItem or NotesItem or ReferenceItem or StatItem or QuoteItem;

    private static FrameworkElement? Host(ItemBase item, double? width)
    {
        var tpl = Application.Current?.TryFindResource("V." + item.GetType().Name) as DataTemplate;
        if (tpl is null) return null;
        var host = new ContentControl { Content = item, ContentTemplate = tpl };
        if (width is { } w) host.Width = w;
        return host;
    }

    /// <summary>Height needed by the widget's content when laid out at <paramref name="width"/>.</summary>
    public static double MeasureHeight(ItemBase item, double? width = null)
    {
        var w = width ?? item.Width;
        var host = Host(item, w);
        if (host is null) return item.Height;
        return Math.Ceiling(Settle(host, w).Height);
    }

    /// <summary>
    /// Lays the host out until its size stops changing. Bindings that look up an ancestor
    /// (e.g. a list row reading the list's font size) only resolve after the first pass, so a
    /// single Measure under-reports the height of wrapped lists.
    /// </summary>
    private static Size Settle(FrameworkElement host, double width)
    {
        var avail = new Size(width, double.PositiveInfinity);
        host.Measure(avail);
        var size = host.DesiredSize;
        for (int i = 0; i < 3; i++)
        {
            host.Arrange(new Rect(0, 0, width, size.Height));
            host.UpdateLayout();
            host.InvalidateMeasure();
            host.Measure(avail);
            if (Math.Abs(host.DesiredSize.Height - size.Height) < 0.5) { size = host.DesiredSize; break; }
            size = host.DesiredSize;
        }
        return size;
    }

    /// <summary>Unwrapped width of the widget's content (used to shrink code that is too wide).</summary>
    public static double MeasureNaturalWidth(ItemBase item)
    {
        var host = Host(item, null);
        if (host is null) return item.Width;
        host.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
        return Math.Ceiling(host.DesiredSize.Width);
    }

    public static void FitHeight(ItemBase item, double minHeight = 40)
    {
        if (IsFixedSize(item)) return;
        item.Height = Math.Max(minHeight, MeasureHeight(item));
    }

    /// <summary>Reduces a code block's font until its longest line fits the block width. Returns false if it still overflows.</summary>
    public static bool FitCodeFont(CodeItem code, double minSize = 10)
    {
        while (MeasureNaturalWidth(code) > code.Width + 0.5)
        {
            if (code.FontSize <= minSize) return false;
            code.FontSize -= 1;
        }
        return true;
    }

    public static void FitAll(DocumentModel doc)
    {
        foreach (var item in doc.Items)
            if (!item.IsLocked) FitHeight(item);
    }

    /// <summary>Resizes the canvas so it ends just below the lowest item, above the fixed footer.</summary>
    public static void FitCanvas(DocumentModel doc, double bottomPadding = 30, double minHeight = 1080)
    {
        double bottom = doc.Items.Where(i => i.IsVisible).Select(i => i.Y + i.Height).DefaultIfEmpty(0).Max();
        doc.CanvasHeight = Math.Max(minHeight, bottom + bottomPadding + doc.FooterHeight);
    }
}
