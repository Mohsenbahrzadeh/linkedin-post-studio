using System.Text.RegularExpressions;
using System.Windows.Documents;
using System.Windows.Media;

namespace LinkedInStudio.Infrastructure;

public static class CodeHighlighter
{
    private static readonly HashSet<string> CsKeywords = new()
    {
        "public","private","protected","internal","static","class","interface","struct","enum",
        "void","var","new","return","using","namespace","get","set","if","else","for","foreach",
        "while","switch","case","break","continue","this","base","null","true","false","async",
        "await","try","catch","finally","throw","readonly","const","override","virtual","abstract",
        "int","long","double","float","decimal","bool","string","char","byte","object","record",
        "in","out","ref","params","default","is","as","typeof","nameof"
    };

    // token order matters: comment, string, number, word, punctuation, whitespace
    private static readonly Regex CsToken = new(
        @"(?<comment>//[^\n]*)|(?<str>""(?:\\.|[^""\\])*"")|(?<num>\b\d+(?:\.\d+)?\b)|(?<word>[A-Za-z_][A-Za-z0-9_]*)|(?<nl>\n)|(?<ws>[ \t\r]+)|(?<punct>[^\sA-Za-z0-9_])",
        RegexOptions.Compiled);

    private static readonly Regex JsonToken = new(
        @"(?<key>""(?:\\.|[^""\\])*"")(?=\s*:)|(?<str>""(?:\\.|[^""\\])*"")|(?<num>-?\b\d+(?:\.\d+)?\b)|(?<bool>\btrue\b|\bfalse\b|\bnull\b)|(?<nl>\n)|(?<ws>[ \t\r]+)|(?<punct>[^\sA-Za-z0-9_""]+)|(?<other>.)",
        RegexOptions.Compiled);

    public static IEnumerable<Inline> Build(string? code, string language)
    {
        var inlines = new List<Inline>();
        if (string.IsNullOrEmpty(code)) return inlines;

        code = code.Replace("\r\n", "\n").Replace("\r", "\n");

        if (string.Equals(language, "json", StringComparison.OrdinalIgnoreCase))
            BuildJson(code, inlines);
        else if (string.Equals(language, "none", StringComparison.OrdinalIgnoreCase))
            AddPlain(code, inlines);
        else
            BuildCs(code, inlines);

        return inlines;
    }

    private static void AddPlain(string code, List<Inline> inlines)
    {
        var lines = code.Split('\n');
        for (int i = 0; i < lines.Length; i++)
        {
            inlines.Add(new Run(lines[i]) { Foreground = Palette.CodeText });
            if (i < lines.Length - 1) inlines.Add(new LineBreak());
        }
    }

    private static void BuildCs(string code, List<Inline> inlines)
    {
        foreach (Match m in CsToken.Matches(code))
        {
            if (m.Groups["nl"].Success) { inlines.Add(new LineBreak()); continue; }
            if (m.Groups["ws"].Success) { inlines.Add(new Run(m.Value) { Foreground = Palette.CodeText }); continue; }
            if (m.Groups["comment"].Success) { inlines.Add(new Run(m.Value) { Foreground = Palette.CodeComment }); continue; }
            if (m.Groups["str"].Success) { inlines.Add(new Run(m.Value) { Foreground = Palette.CodeString }); continue; }
            if (m.Groups["num"].Success) { inlines.Add(new Run(m.Value) { Foreground = Palette.CodeNumber }); continue; }
            if (m.Groups["word"].Success)
            {
                var w = m.Value;
                Brush brush = CsKeywords.Contains(w) ? Palette.CodeKeyword
                            : (char.IsUpper(w[0]) ? Palette.CodeType : Palette.CodeText);
                inlines.Add(new Run(w) { Foreground = brush });
                continue;
            }
            inlines.Add(new Run(m.Value) { Foreground = Palette.CodePunct });
        }
    }

    private static void BuildJson(string code, List<Inline> inlines)
    {
        foreach (Match m in JsonToken.Matches(code))
        {
            if (m.Groups["nl"].Success) { inlines.Add(new LineBreak()); continue; }
            if (m.Groups["ws"].Success) { inlines.Add(new Run(m.Value) { Foreground = Palette.CodeText }); continue; }
            if (m.Groups["key"].Success) { inlines.Add(new Run(m.Value) { Foreground = Palette.CodeKeyword }); continue; }
            if (m.Groups["str"].Success) { inlines.Add(new Run(m.Value) { Foreground = Palette.CodeString }); continue; }
            if (m.Groups["num"].Success) { inlines.Add(new Run(m.Value) { Foreground = Palette.CodeNumber }); continue; }
            if (m.Groups["bool"].Success) { inlines.Add(new Run(m.Value) { Foreground = Palette.CodeType }); continue; }
            inlines.Add(new Run(m.Value) { Foreground = Palette.CodePunct });
        }
    }
}
