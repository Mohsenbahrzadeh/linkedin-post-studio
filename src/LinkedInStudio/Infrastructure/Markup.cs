using System.Windows.Documents;
using System.Windows.Media;

namespace LinkedInStudio.Infrastructure;

/// <summary>
/// Tiny inline markup: text wrapped in *asterisks* is rendered with the highlight brush.
/// Used by Title and Banner widgets, e.g. "How to Handle DateTime in *Web API*".
/// </summary>
public static class Markup
{
    public static IEnumerable<Inline> Build(string? text, Brush normal, Brush highlight, bool bold)
    {
        var list = new List<Inline>();
        if (string.IsNullOrEmpty(text)) return list;

        bool hi = false;
        foreach (var part in text.Split('*'))
        {
            if (part.Length > 0)
            {
                var run = new Run(part) { Foreground = hi ? highlight : normal };
                if (bold) run.FontWeight = System.Windows.FontWeights.Bold;
                list.Add(run);
            }
            hi = !hi;
        }
        return list;
    }
}
