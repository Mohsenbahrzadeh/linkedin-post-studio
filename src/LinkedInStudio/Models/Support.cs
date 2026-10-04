using System.Windows.Media;

namespace LinkedInStudio.Models;

/// <summary>Parsing helpers shared by widgets that edit list content as multiline text.</summary>
public static class Parse
{
    public static List<string> Lines(string? text)
    {
        var list = new List<string>();
        if (string.IsNullOrEmpty(text)) return list;
        foreach (var raw in text.Replace("\r\n", "\n").Split('\n'))
        {
            var line = raw.TrimEnd();
            if (line.Length > 0) list.Add(line);
        }
        return list;
    }

    public static (string a, string b) Pair(string line, char sep = '|')
    {
        var idx = line.IndexOf(sep);
        if (idx < 0) return (line.Trim(), string.Empty);
        return (line[..idx].Trim(), line[(idx + 1)..].Trim());
    }

    public static (string a, string b, string c) Triple(string line, char sep = '|')
    {
        var parts = line.Split(sep);
        string a = parts.Length > 0 ? parts[0].Trim() : "";
        string b = parts.Length > 1 ? parts[1].Trim() : "";
        string c = parts.Length > 2 ? parts[2].Trim() : "";
        return (a, b, c);
    }
}

// ---- lightweight view rows used by widget templates ----

public sealed class ProblemRow
{
    public string Code { get; init; } = "";
    public string Note { get; init; } = "";
}

public sealed class FlowNode
{
    public string Title { get; init; } = "";
    public string Subtitle { get; init; } = "";
    public string Glyph { get; init; } = "●";
    public Brush Accent { get; init; } = Brushes.Gray;
    public Brush Bg { get; init; } = Brushes.White;
    public Brush Border { get; init; } = Brushes.Gray;
    public bool ShowArrow { get; init; }
}

public sealed class BenefitCard
{
    public string Title { get; init; } = "";
    public string Desc { get; init; } = "";
    public string Glyph { get; init; } = "⭐";
    public Brush Accent { get; init; } = Brushes.Gray;
}

public sealed class ChecklistRow
{
    public string Marker { get; init; } = "•";
    public string Text { get; init; } = "";
    public Brush Accent { get; init; } = Brushes.Gray;
}

public sealed class BoxRow
{
    public string Glyph { get; init; } = "●";
    public Brush MarkerBrush { get; init; } = Brushes.Gray;
    public Brush GlyphBrush { get; init; } = Brushes.Gray;
    public bool ShowCircle { get; init; }
    public string Text { get; init; } = "";
}

public sealed class SceneNode
{
    public string Glyph { get; init; } = "📦";
    public Brush Tile { get; init; } = Brushes.White;
    public Brush Border { get; init; } = Brushes.Gray;
    public Brush ArrowBrush { get; init; } = Brushes.Gray;
    public bool ShowArrow { get; init; }
}

public sealed class TableRow
{
    public IReadOnlyList<string> Cells { get; init; } = Array.Empty<string>();
    public int Columns { get; init; }
    public bool IsHeader { get; init; }
    public Brush Bg { get; init; } = Brushes.Transparent;
    public Brush Fg { get; init; } = Brushes.Black;
    public System.Windows.FontWeight Weight { get; init; } = System.Windows.FontWeights.Normal;
}

public sealed class StepRow
{
    public string Number { get; init; } = "";
    public string Title { get; init; } = "";
    public string Desc { get; init; } = "";
    public Brush Accent { get; init; } = Brushes.Gray;
}
