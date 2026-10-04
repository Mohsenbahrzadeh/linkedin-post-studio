namespace LinkedInStudio.Infrastructure;

/// <summary>Curated emoji icon set (renders reliably in WPF) and font families for pickers.</summary>
public static class Catalog
{
    public static readonly string[] Icons =
    {
        "🌐","💻","📱","☁️","🖥️","🗄️","⚙️","🔧","🔒","🔑","🕒","📅",
        "✅","❌","⚠️","ℹ️","🚀","⭐","💡","📋","📌","🎯","🔗","📦",
        "🧩","🛡️","📈","📊","⚡","💬","✉️","👤","🏷️","➡️","🔥","👍",
    };

    /// <summary>The bundled Vazirmatn family (OFL), resolved from app resources — works without install.</summary>
    public const string Vazirmatn = "pack://application:,,,/Fonts/#Vazirmatn";

    public sealed record FontOption(string Label, string Value);

    public static readonly FontOption[] FontOptions =
    {
        new("Vazirmatn (Persian)", Vazirmatn),
        new("Segoe UI", "Segoe UI"),
        new("Tahoma", "Tahoma"),
        new("Calibri", "Calibri"),
        new("Arial", "Arial"),
        new("Georgia", "Georgia"),
        new("Verdana", "Verdana"),
        new("Consolas (mono)", "Consolas"),
        new("Cascadia Code (mono)", "Cascadia Code, Consolas"),
    };

    public static readonly string[] Swatches =
    {
        "#17233F","#0C2445","#2E9E52","#1E8E4E","#D93A2E","#C62828",
        "#E8912A","#2F80D8","#6C4BB6","#0A66C2","#5B6472","#94A3B8",
        "#FFFFFF","#000000","#EAF2FC","#E9F5EC","#FDECEA","#FDF6EA",
    };
}
