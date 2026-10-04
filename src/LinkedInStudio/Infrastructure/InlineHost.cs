using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace LinkedInStudio.Infrastructure;

/// <summary>Attaches a computed inline collection to a TextBlock.</summary>
public static class InlineHost
{
    public static readonly DependencyProperty SourceProperty =
        DependencyProperty.RegisterAttached(
            "Source", typeof(IEnumerable), typeof(InlineHost),
            new PropertyMetadata(null, OnChanged));

    public static void SetSource(DependencyObject o, IEnumerable? v) => o.SetValue(SourceProperty, v);
    public static IEnumerable? GetSource(DependencyObject o) => (IEnumerable?)o.GetValue(SourceProperty);

    private static void OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not TextBlock tb) return;
        tb.Inlines.Clear();
        if (e.NewValue is IEnumerable items)
        {
            foreach (var item in items)
                if (item is Inline inline)
                    tb.Inlines.Add(inline);
        }
    }
}
