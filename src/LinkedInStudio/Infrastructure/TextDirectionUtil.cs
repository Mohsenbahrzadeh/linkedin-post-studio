using System.Windows;

namespace LinkedInStudio.Infrastructure;

public enum TextDir { Auto, Rtl, Ltr }

public static class TextDirectionUtil
{
    /// <summary>True when the character belongs to Arabic/Persian script blocks.</summary>
    public static bool IsRtlChar(char c) =>
        (c >= '؀' && c <= 'ۿ') || // Arabic (includes Persian letters and digits)
        (c >= 'ݐ' && c <= 'ݿ') || // Arabic Supplement
        (c >= 'ࢠ' && c <= 'ࣿ') || // Arabic Extended-A
        (c >= 'ﭐ' && c <= '﷿') || // Arabic Presentation Forms-A
        (c >= 'ﹰ' && c <= '﻿');   // Arabic Presentation Forms-B

    private static bool IsLtrChar(char c) =>
        (c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z');

    /// <summary>
    /// Resolves the effective flow direction for a piece of text. In Auto mode the decision is
    /// made by the script that dominates the whole text (not by the first letter), so a Persian
    /// sentence that starts with an English term such as "Lazy Loading ..." still flows RTL.
    /// </summary>
    public static FlowDirection Resolve(string? text, TextDir mode)
    {
        switch (mode)
        {
            case TextDir.Rtl: return FlowDirection.RightToLeft;
            case TextDir.Ltr: return FlowDirection.LeftToRight;
            default:
                if (string.IsNullOrEmpty(text)) return FlowDirection.LeftToRight;
                int rtl = 0, ltr = 0;
                foreach (var c in text)
                {
                    if (IsRtlChar(c)) rtl++;
                    else if (IsLtrChar(c)) ltr++;
                }
                if (rtl == 0) return FlowDirection.LeftToRight;
                // Persian text routinely embeds English terms, so RTL wins unless Latin clearly dominates.
                return rtl * 2 >= ltr ? FlowDirection.RightToLeft : FlowDirection.LeftToRight;
        }
    }

    /// <summary>Joins several fields so a widget decides its direction from all of its text.</summary>
    public static string Join(params string?[] parts) => string.Join("\n", parts.Where(p => !string.IsNullOrEmpty(p)));
}
