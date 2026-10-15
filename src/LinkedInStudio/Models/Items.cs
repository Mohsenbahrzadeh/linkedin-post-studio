using System.IO;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using LinkedInStudio.Infrastructure;

namespace LinkedInStudio.Models;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$kind")]
[JsonDerivedType(typeof(TitleItem), "title")]
[JsonDerivedType(typeof(BannerItem), "banner")]
[JsonDerivedType(typeof(SectionLabelItem), "section")]
[JsonDerivedType(typeof(TextItem), "text")]
[JsonDerivedType(typeof(ProblemBoxItem), "problem")]
[JsonDerivedType(typeof(SolutionBoxItem), "solution")]
[JsonDerivedType(typeof(CodeItem), "code")]
[JsonDerivedType(typeof(NotesItem), "notes")]
[JsonDerivedType(typeof(ReferenceItem), "reference")]
[JsonDerivedType(typeof(FlowItem), "flow")]
[JsonDerivedType(typeof(BenefitsItem), "benefits")]
[JsonDerivedType(typeof(ImageItem), "image")]
[JsonDerivedType(typeof(VsBadgeItem), "vs")]
[JsonDerivedType(typeof(CustomBoxItem), "custombox")]
[JsonDerivedType(typeof(StatItem), "stat")]
[JsonDerivedType(typeof(ChecklistItem), "checklist")]
[JsonDerivedType(typeof(QuoteItem), "quote")]
[JsonDerivedType(typeof(DividerItem), "divider")]
[JsonDerivedType(typeof(ArrowItem), "arrow")]
[JsonDerivedType(typeof(PillItem), "pill")]
[JsonDerivedType(typeof(ProgressItem), "progress")]
[JsonDerivedType(typeof(IconItem), "icon")]
[JsonDerivedType(typeof(IllustrationItem), "scene")]
[JsonDerivedType(typeof(TableItem), "table")]
[JsonDerivedType(typeof(StepsItem), "steps")]
[JsonDerivedType(typeof(TerminalItem), "terminal")]
public abstract class ItemBase : ObservableObject
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    private double _x = 60, _y = 60, _w = 400, _h = 160;
    private int _z;
    private double _rot;

    public double X { get => _x; set => Set(ref _x, Math.Round(value)); }
    public double Y { get => _y; set => Set(ref _y, Math.Round(value)); }
    public double Width { get => _w; set => Set(ref _w, Math.Max(40, Math.Round(value))); }
    public double Height { get => _h; set => Set(ref _h, Math.Max(30, Math.Round(value))); }
    public int Z { get => _z; set => Set(ref _z, value); }
    public double Rotation { get => _rot; set { if (Set(ref _rot, value)) OnChanged(nameof(RotateTransform)); } }

    private double _opacity = 1.0;
    public double Opacity { get => _opacity; set => Set(ref _opacity, Math.Clamp(value, 0, 1)); }

    private bool _isVisible = true;
    public bool IsVisible { get => _isVisible; set { if (Set(ref _isVisible, value)) OnChanged(nameof(CanvasVisibility)); } }

    private bool _isLocked;
    public bool IsLocked { get => _isLocked; set => Set(ref _isLocked, value); }

    private string? _name;
    /// <summary>Optional user-given layer name; falls back to the localized widget label.</summary>
    public string? Name { get => _name; set => Set(ref _name, value); }

    private bool _selected;
    [JsonIgnore] public bool IsSelected { get => _selected; set => Set(ref _selected, value); }

    [JsonIgnore] public System.Windows.Visibility CanvasVisibility => _isVisible ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
    [JsonIgnore] public System.Windows.Media.Transform RotateTransform => new System.Windows.Media.RotateTransform(_rot);

    /// <summary>True when this widget has adjustable text (enables the A- / A+ control).</summary>
    [JsonIgnore] public virtual bool HasFont => false;

    /// <summary>Grows (dir=+1) or shrinks (dir=-1) the widget's text.</summary>
    public virtual void AdjustFont(int dir) { }

    protected static double Clamp(double v, double lo, double hi) => Math.Clamp(v, lo, hi);

    /// <summary>Localization key used as the palette / layer label.</summary>
    [JsonIgnore] public abstract string LabelKey { get; }
}

// ---------------------------------------------------------------- Title
public sealed class TitleItem : ItemBase
{
    public TitleItem() { Width = 640; Height = 130; }

    private string _text = "How to Handle *DateTime* in *Web API*";
    private double _fontSize = 46;
    private string _font = Catalog.Vazirmatn;
    private TextDir _dir = TextDir.Auto;

    public string Text { get => _text; set { if (Set(ref _text, value)) { OnChanged(nameof(Inlines)); OnChanged(nameof(Flow)); } } }
    public double FontSize { get => _fontSize; set => Set(ref _fontSize, value); }
    public string Font { get => _font; set => Set(ref _font, value); }
    public TextDir Dir { get => _dir; set { if (Set(ref _dir, value)) OnChanged(nameof(Flow)); } }

    [JsonIgnore] public override string LabelKey => "Widget.Title";
    [JsonIgnore] public override bool HasFont => true;
    public override void AdjustFont(int dir) => FontSize = Clamp(_fontSize + dir * 2, 14, 120);
    [JsonIgnore] public IEnumerable<Inline> Inlines => Markup.Build(_text, Palette.Navy, Palette.Green, bold: true);
    [JsonIgnore] public FlowDirection Flow => TextDirectionUtil.Resolve(_text, _dir);
}

// ---------------------------------------------------------------- Banner
public sealed class BannerItem : ItemBase
{
    public BannerItem() { Width = 720; Height = 96; }

    private string _text = "*Best Practice:* Use a *TICKET (Unix Timestamp)* as long in requests and convert it to *DateTime* inside the Web API.";
    private double _fontSize = 19;
    private TextDir _dir = TextDir.Auto;

    public string Text { get => _text; set { if (Set(ref _text, value)) { OnChanged(nameof(Inlines)); OnChanged(nameof(Flow)); } } }
    public double FontSize { get => _fontSize; set => Set(ref _fontSize, value); }
    public TextDir Dir { get => _dir; set { if (Set(ref _dir, value)) OnChanged(nameof(Flow)); } }

    [JsonIgnore] public override string LabelKey => "Widget.Banner";
    [JsonIgnore] public override bool HasFont => true;
    public override void AdjustFont(int dir) => FontSize = Clamp(_fontSize + dir, 12, 48);
    [JsonIgnore] public IEnumerable<Inline> Inlines => Markup.Build(_text, Palette.White, Palette.Green, bold: false);
    [JsonIgnore] public FlowDirection Flow => TextDirectionUtil.Resolve(_text, _dir);
}

// ---------------------------------------------------------------- Section label pill
public sealed class SectionLabelItem : ItemBase
{
    public SectionLabelItem() { Width = 220; Height = 46; }

    private string _text = "How it works";
    private double _fontSize = 18;
    private TextDir _dir = TextDir.Auto;

    public string Text { get => _text; set { if (Set(ref _text, value)) { OnChanged(nameof(Flow)); OnChanged(nameof(Align)); } } }
    public double FontSize { get => _fontSize; set => Set(ref _fontSize, value); }
    public TextDir Dir { get => _dir; set { if (Set(ref _dir, value)) { OnChanged(nameof(Flow)); OnChanged(nameof(Align)); } } }

    [JsonIgnore] public override string LabelKey => "Widget.SectionLabel";
    [JsonIgnore] public override bool HasFont => true;
    public override void AdjustFont(int dir) => FontSize = Clamp(_fontSize + dir, 10, 48);
    [JsonIgnore] public FlowDirection Flow => TextDirectionUtil.Resolve(_text, _dir);
    /// <summary>Pill hugs the leading edge: right for Persian, left for English.</summary>
    [JsonIgnore] public HorizontalAlignment Align => Flow == FlowDirection.RightToLeft ? HorizontalAlignment.Right : HorizontalAlignment.Left;
}

// ---------------------------------------------------------------- Generic text
public sealed class TextItem : ItemBase
{
    public TextItem() { Width = 380; Height = 90; }

    private string _text = "Double-click me and edit the text in the Properties panel.";
    private double _fontSize = 16;
    private bool _bold;
    private string _color = "#17233F";
    private string _font = Catalog.Vazirmatn;
    private TextDir _dir = TextDir.Auto;
    private TextAlignment _align = TextAlignment.Left;

    public string Text { get => _text; set { if (Set(ref _text, value)) OnChanged(nameof(Flow)); } }
    public double FontSize { get => _fontSize; set => Set(ref _fontSize, value); }
    public bool Bold { get => _bold; set { if (Set(ref _bold, value)) OnChanged(nameof(Weight)); } }
    public string Color { get => _color; set { if (Set(ref _color, value)) OnChanged(nameof(Foreground)); } }
    public string Font { get => _font; set => Set(ref _font, value); }
    public TextDir Dir { get => _dir; set { if (Set(ref _dir, value)) OnChanged(nameof(Flow)); } }
    public TextAlignment Align { get => _align; set => Set(ref _align, value); }

    [JsonIgnore] public override string LabelKey => "Widget.Text";
    [JsonIgnore] public override bool HasFont => true;
    public override void AdjustFont(int dir) => FontSize = Clamp(_fontSize + dir, 8, 120);
    [JsonIgnore] public Brush Foreground => ColorUtil.Brush(_color);
    [JsonIgnore] public FontWeight Weight => _bold ? FontWeights.Bold : FontWeights.Normal;
    [JsonIgnore] public FlowDirection Flow => TextDirectionUtil.Resolve(_text, _dir);
}

// ---------------------------------------------------------------- Problem box (red)
public sealed class ProblemBoxItem : ItemBase
{
    public ProblemBoxItem() { Width = 470; Height = 320; }

    private string _header = "The Problem with DateTime Strings";
    private string _intro = "Clients can send dates in different formats, calendars and time zones.";
    private string _rowsText =
        "09/05/2026 14:30 | (MM/dd/yyyy)\n05-09-2026 14:30 | (dd-MM-yyyy)\n2026/09/05 14:30 | (yyyy/MM/dd)\n1405/06/14 14:30 | (Persian Calendar)";
    private string _bulletsText =
        "Culture & format dependent\nParsing errors\nTime zone confusion\nDifficult to maintain";
    private TextDir _dir = TextDir.Auto;

    public string Header { get => _header; set { if (Set(ref _header, value)) OnChanged(nameof(Flow)); } }
    public string Intro { get => _intro; set { if (Set(ref _intro, value)) OnChanged(nameof(Flow)); } }
    public string RowsText { get => _rowsText; set { if (Set(ref _rowsText, value)) OnChanged(nameof(Rows)); } }
    public string BulletsText { get => _bulletsText; set { if (Set(ref _bulletsText, value)) { OnChanged(nameof(Bullets)); OnChanged(nameof(Flow)); } } }
    public TextDir Dir { get => _dir; set { if (Set(ref _dir, value)) OnChanged(nameof(Flow)); } }

    private double _textScale = 1.0;
    public double TextScale { get => _textScale; set => Set(ref _textScale, value); }

    [JsonIgnore] public override string LabelKey => "Widget.Problem";
    [JsonIgnore] public override bool HasFont => true;
    public override void AdjustFont(int dir) => TextScale = Clamp(_textScale + dir * 0.06, 0.6, 2.2);
    [JsonIgnore] public FlowDirection Flow => TextDirectionUtil.Resolve(TextDirectionUtil.Join(_header, _intro, _bulletsText), _dir);
    [JsonIgnore] public IEnumerable<ProblemRow> Rows =>
        Parse.Lines(_rowsText).Select(l => { var (a, b) = Parse.Pair(l); return new ProblemRow { Code = a, Note = b }; });
    [JsonIgnore] public IEnumerable<string> Bullets => Parse.Lines(_bulletsText);
}

// ---------------------------------------------------------------- Solution box (green)
public sealed class SolutionBoxItem : ItemBase
{
    public SolutionBoxItem() { Width = 470; Height = 320; }

    private string _header = "The Better Approach: Use TICKET";
    private string _intro = "Send DateTime as a Unix Timestamp (ticket) as long. It's format-agnostic and consistent.";
    private string _codeLinesText = "1788611400 | // seconds\nor\n1788611400000 | // milliseconds";
    private string _bulletsText =
        "No matter which client or platform\nNo culture or format issues\nTimezone handled on the server (UTC)\nSimple, consistent, reliable";
    private TextDir _dir = TextDir.Auto;

    public string Header { get => _header; set { if (Set(ref _header, value)) OnChanged(nameof(Flow)); } }
    public string Intro { get => _intro; set { if (Set(ref _intro, value)) OnChanged(nameof(Flow)); } }
    public string CodeLinesText { get => _codeLinesText; set { if (Set(ref _codeLinesText, value)) OnChanged(nameof(CodeLines)); } }
    public string BulletsText { get => _bulletsText; set { if (Set(ref _bulletsText, value)) { OnChanged(nameof(Bullets)); OnChanged(nameof(Flow)); } } }
    public TextDir Dir { get => _dir; set { if (Set(ref _dir, value)) OnChanged(nameof(Flow)); } }

    private double _textScale = 1.0;
    public double TextScale { get => _textScale; set => Set(ref _textScale, value); }

    [JsonIgnore] public override string LabelKey => "Widget.Solution";
    [JsonIgnore] public override bool HasFont => true;
    public override void AdjustFont(int dir) => TextScale = Clamp(_textScale + dir * 0.06, 0.6, 2.2);
    [JsonIgnore] public FlowDirection Flow => TextDirectionUtil.Resolve(TextDirectionUtil.Join(_header, _intro, _bulletsText), _dir);
    [JsonIgnore] public IEnumerable<ProblemRow> CodeLines =>
        Parse.Lines(_codeLinesText).Select(l => { var (a, b) = Parse.Pair(l); return new ProblemRow { Code = a, Note = b }; });
    [JsonIgnore] public IEnumerable<string> Bullets => Parse.Lines(_bulletsText);
}

// ---------------------------------------------------------------- Code block (dark)
public sealed class CodeItem : ItemBase
{
    public CodeItem() { Width = 520; Height = 250; }

    private string _caption = "Web API (C#)";
    private string _code =
        "public class MyRequest\n{\n    public long StartDate { get; set; }\n    public long EndDate { get; set; }\n}\n\nvar start = DateTimeOffset.FromUnixTimeSeconds(request.StartDate);\n// Now you have DateTimeOffset in UTC";
    private string _language = "csharp";
    private double _fontSize = 14;
    private bool _showBadge = true;

    public string Caption { get => _caption; set => Set(ref _caption, value); }
    public string Code { get => _code; set { if (Set(ref _code, value)) OnChanged(nameof(Inlines)); } }
    public string Language { get => _language; set { if (Set(ref _language, value)) OnChanged(nameof(Inlines)); } }
    public double FontSize { get => _fontSize; set => Set(ref _fontSize, value); }
    public bool ShowBadge { get => _showBadge; set { if (Set(ref _showBadge, value)) OnChanged(nameof(BadgeVisibility)); } }

    [JsonIgnore] public System.Windows.Visibility BadgeVisibility => _showBadge ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
    [JsonIgnore] public override string LabelKey => "Widget.Code";
    [JsonIgnore] public override bool HasFont => true;
    public override void AdjustFont(int dir) => FontSize = Clamp(_fontSize + dir, 8, 40);
    [JsonIgnore] public IEnumerable<Inline> Inlines => CodeHighlighter.Build(_code, _language);
}

// ---------------------------------------------------------------- Notes box (cream)
public sealed class NotesItem : ItemBase
{
    public NotesItem() { Width = 430; Height = 220; }

    private string _header = "Important Notes";
    private string _bulletsText =
        "Use UTC-based timestamps.\nDecide and document: Seconds or Milliseconds.\nPrefer DateTimeOffset to keep timezone info.\nConvert at the boundary, not in the domain.";
    private TextDir _dir = TextDir.Auto;

    public string Header { get => _header; set { if (Set(ref _header, value)) OnChanged(nameof(Flow)); } }
    public string BulletsText { get => _bulletsText; set { if (Set(ref _bulletsText, value)) { OnChanged(nameof(Bullets)); OnChanged(nameof(Flow)); } } }
    public TextDir Dir { get => _dir; set { if (Set(ref _dir, value)) OnChanged(nameof(Flow)); } }

    private double _textScale = 1.0;
    public double TextScale { get => _textScale; set => Set(ref _textScale, value); }

    [JsonIgnore] public override string LabelKey => "Widget.Notes";
    [JsonIgnore] public override bool HasFont => true;
    public override void AdjustFont(int dir) => TextScale = Clamp(_textScale + dir * 0.06, 0.6, 2.2);
    [JsonIgnore] public FlowDirection Flow => TextDirectionUtil.Resolve(TextDirectionUtil.Join(_header, _bulletsText), _dir);
    [JsonIgnore] public IEnumerable<string> Bullets => Parse.Lines(_bulletsText);
}

// ---------------------------------------------------------------- Reference box (light blue, mono)
public sealed class ReferenceItem : ItemBase
{
    public ReferenceItem() { Width = 430; Height = 160; }

    private string _header = "Timestamp Quick Reference";
    private string _linesText =
        "1970-01-01 00:00:00 UTC = 0\n1788611400       = 2026-09-05 11:30:00 UTC\n1788611400000    = 2026-09-05 11:30:00.000 UTC";

    public string Header { get => _header; set => Set(ref _header, value); }
    public string LinesText { get => _linesText; set { if (Set(ref _linesText, value)) OnChanged(nameof(Lines)); } }

    private double _textScale = 1.0;
    public double TextScale { get => _textScale; set => Set(ref _textScale, value); }

    [JsonIgnore] public override string LabelKey => "Widget.Reference";
    [JsonIgnore] public override bool HasFont => true;
    public override void AdjustFont(int dir) => TextScale = Clamp(_textScale + dir * 0.06, 0.6, 2.2);
    [JsonIgnore] public IEnumerable<string> Lines => Parse.Lines(_linesText);
}

// ---------------------------------------------------------------- Flow (3 steps)
public sealed class FlowItem : ItemBase
{
    public FlowItem() { Width = 740; Height = 180; }

    private string _nodesText = "Client / Consumer | Any platform, any language | 🌐\nWeb API | Convert TICKET to DateTime | ⚙️\nDateTime in .NET | Use it in your application | ✅";

    public string NodesText { get => _nodesText; set { if (Set(ref _nodesText, value)) OnChanged(nameof(Nodes)); } }

    [JsonIgnore] public override string LabelKey => "Widget.Flow";
    [JsonIgnore]
    public IEnumerable<FlowNode> Nodes
    {
        get
        {
            var accents = new[] { Palette.Blue, Palette.Green, Palette.Purple };
            var bgs = new[] { "#EAF2FC", "#EAF6EE", "#F1ECFA" };
            var borders = new[] { "#D2E4F8", "#CBE7D3", "#E0D4F3" };
            var glyphs = new[] { "🌐", "⚙️", "✅" };
            var lines = Parse.Lines(_nodesText);
            for (int i = 0; i < lines.Count; i++)
            {
                var (a, b, c) = Parse.Triple(lines[i]);
                int k = i % 3;
                yield return new FlowNode
                {
                    Title = a,
                    Subtitle = b,
                    Glyph = string.IsNullOrEmpty(c) ? glyphs[k] : c,
                    Accent = accents[k],
                    Bg = ColorUtil.Brush(bgs[k]),
                    Border = ColorUtil.Brush(borders[k]),
                    ShowArrow = i < lines.Count - 1
                };
            }
        }
    }
}

// ---------------------------------------------------------------- Benefits row
public sealed class BenefitsItem : ItemBase
{
    public BenefitsItem() { Width = 740; Height = 130; }

    private string _cardsText =
        "Works for all clients | Any platform, same contract | 🌐\nSimpler API contract | Just a long value | 🧩\nFewer bugs | No date parsing issues | 🛡️\nBetter maintainability | Clean and scalable | 🚀";

    public string CardsText { get => _cardsText; set { if (Set(ref _cardsText, value)) OnChanged(nameof(Cards)); } }

    [JsonIgnore] public override string LabelKey => "Widget.Benefits";
    [JsonIgnore]
    public IEnumerable<BenefitCard> Cards
    {
        get
        {
            var accents = new[] { Palette.Blue, Palette.Green, Palette.Purple, Palette.Amber, Palette.Red };
            var glyphs = new[] { "🌐", "🧩", "🛡️", "🚀", "⭐" };
            var lines = Parse.Lines(_cardsText);
            for (int i = 0; i < lines.Count; i++)
            {
                var (a, b, c) = Parse.Triple(lines[i]);
                yield return new BenefitCard
                {
                    Title = a, Desc = b,
                    Glyph = string.IsNullOrEmpty(c) ? glyphs[i % glyphs.Length] : c,
                    Accent = accents[i % accents.Length]
                };
            }
        }
    }
}

// ---------------------------------------------------------------- Image
public sealed class ImageItem : ItemBase
{
    public ImageItem() { Width = 300; Height = 220; }

    private string _path = "";
    private string _stretch = "Uniform";
    private double _corner = 8;

    public string Path { get => _path; set { if (Set(ref _path, value)) OnChanged(nameof(Source)); } }
    public string Stretch { get => _stretch; set { if (Set(ref _stretch, value)) OnChanged(nameof(StretchMode)); } }
    public double Corner { get => _corner; set { if (Set(ref _corner, value)) OnChanged(nameof(CornerRadius)); } }

    [JsonIgnore] public override string LabelKey => "Widget.Image";
    [JsonIgnore] public Stretch StretchMode => Enum.TryParse<Stretch>(_stretch, out var s) ? s : System.Windows.Media.Stretch.Uniform;
    [JsonIgnore] public CornerRadius CornerRadius => new(_corner);
    [JsonIgnore]
    public ImageSource? Source
    {
        get
        {
            try
            {
                if (string.IsNullOrWhiteSpace(_path)) return null;
                // Resolve relative paths (e.g. in portable spec files) against the working directory.
                var full = System.IO.Path.GetFullPath(_path);
                if (!File.Exists(full)) return null;
                var bmp = new BitmapImage();
                bmp.BeginInit();
                bmp.CacheOption = BitmapCacheOption.OnLoad;
                bmp.UriSource = new Uri(full);
                bmp.EndInit();
                bmp.Freeze();
                return bmp;
            }
            catch { return null; }
        }
    }
}

// ---------------------------------------------------------------- VS badge
public sealed class VsBadgeItem : ItemBase
{
    public VsBadgeItem() { Width = 76; Height = 76; }

    private string _text = "VS";
    public string Text { get => _text; set => Set(ref _text, value); }

    [JsonIgnore] public override string LabelKey => "Widget.Vs";
}

// ---------------------------------------------------------------- Custom box (free colors)
public sealed class CustomBoxItem : ItemBase
{
    public CustomBoxItem() { Width = 430; Height = 220; }

    private string _header = "Custom Box";
    private string _body = "";
    private string _bulletsText = "First point\nSecond point\nThird point";
    private string _marker = "Dot";   // Dot | CheckCircle | CrossCircle | Info | Number | None
    private string _glyph = "📌";
    private string _bg = "#EEF3FB";
    private string _border = "#D2E0F2";
    private string _accent = "#2F80D8";
    private string _text = "#17233F";
    private double _headerSize = 18;
    private TextDir _dir = TextDir.Auto;

    public string Header { get => _header; set { if (Set(ref _header, value)) OnChanged(nameof(Flow)); } }
    public string Body { get => _body; set { if (Set(ref _body, value)) { OnChanged(nameof(HasBody)); OnChanged(nameof(Flow)); } } }
    public string BulletsText { get => _bulletsText; set { if (Set(ref _bulletsText, value)) { OnChanged(nameof(Rows)); OnChanged(nameof(Flow)); } } }
    public string Marker { get => _marker; set { if (Set(ref _marker, value)) OnChanged(nameof(Rows)); } }
    public string Glyph { get => _glyph; set => Set(ref _glyph, value); }
    public string Bg { get => _bg; set { if (Set(ref _bg, value)) OnChanged(nameof(BgBrush)); } }
    public string Border { get => _border; set { if (Set(ref _border, value)) OnChanged(nameof(BorderBrush)); } }
    public string Accent { get => _accent; set { if (Set(ref _accent, value)) { OnChanged(nameof(AccentBrush)); OnChanged(nameof(Rows)); } } }
    public string Text { get => _text; set { if (Set(ref _text, value)) OnChanged(nameof(TextBrush)); } }
    public double HeaderSize { get => _headerSize; set => Set(ref _headerSize, value); }
    public TextDir Dir { get => _dir; set { if (Set(ref _dir, value)) OnChanged(nameof(Flow)); } }

    private double _textScale = 1.0;
    public double TextScale { get => _textScale; set => Set(ref _textScale, value); }

    [JsonIgnore] public override string LabelKey => "Widget.Custom";
    [JsonIgnore] public override bool HasFont => true;
    public override void AdjustFont(int dir) => TextScale = Clamp(_textScale + dir * 0.06, 0.6, 2.2);
    [JsonIgnore] public Brush BgBrush => ColorUtil.Brush(_bg);
    [JsonIgnore] public Brush BorderBrush => ColorUtil.Brush(_border);
    [JsonIgnore] public Brush AccentBrush => ColorUtil.Brush(_accent);
    [JsonIgnore] public Brush TextBrush => ColorUtil.Brush(_text);
    [JsonIgnore] public FlowDirection Flow => TextDirectionUtil.Resolve(TextDirectionUtil.Join(_header, _body, _bulletsText), _dir);
    [JsonIgnore] public System.Windows.Visibility HasBody => string.IsNullOrWhiteSpace(_body) ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
    [JsonIgnore]
    public IEnumerable<BoxRow> Rows
    {
        get
        {
            var accent = ColorUtil.Brush(_accent);
            var green = Palette.Green; var red = Palette.Red; var blue = Palette.Blue;
            var lines = Parse.Lines(_bulletsText);
            for (int i = 0; i < lines.Count; i++)
            {
                yield return _marker switch
                {
                    "CheckCircle" => new BoxRow { Glyph = "✓", MarkerBrush = green, GlyphBrush = Palette.White, ShowCircle = true, Text = lines[i] },
                    "CrossCircle" => new BoxRow { Glyph = "✕", MarkerBrush = red, GlyphBrush = Palette.White, ShowCircle = true, Text = lines[i] },
                    "Info" => new BoxRow { Glyph = "i", MarkerBrush = blue, GlyphBrush = Palette.White, ShowCircle = true, Text = lines[i] },
                    "Number" => new BoxRow { Glyph = $"{i + 1}", MarkerBrush = accent, GlyphBrush = accent, ShowCircle = false, Text = lines[i] },
                    "None" => new BoxRow { Glyph = "", MarkerBrush = accent, GlyphBrush = accent, ShowCircle = false, Text = lines[i] },
                    _ => new BoxRow { Glyph = "●", MarkerBrush = accent, GlyphBrush = accent, ShowCircle = false, Text = lines[i] },
                };
            }
        }
    }
}

// ---------------------------------------------------------------- Stat / KPI
public sealed class StatItem : ItemBase
{
    public StatItem() { Width = 240; Height = 150; }

    private string _value = "10×";
    private string _label = "Faster delivery";
    private string _accent = "#2E9E52";
    private double _valueSize = 52;
    private TextDir _dir = TextDir.Auto;

    public string Value { get => _value; set => Set(ref _value, value); }
    public string Label { get => _label; set { if (Set(ref _label, value)) OnChanged(nameof(Flow)); } }
    public string Accent { get => _accent; set { if (Set(ref _accent, value)) OnChanged(nameof(AccentBrush)); } }
    public double ValueSize { get => _valueSize; set => Set(ref _valueSize, value); }
    public TextDir Dir { get => _dir; set { if (Set(ref _dir, value)) OnChanged(nameof(Flow)); } }

    [JsonIgnore] public override string LabelKey => "Widget.Stat";
    [JsonIgnore] public override bool HasFont => true;
    public override void AdjustFont(int dir) => ValueSize = Clamp(_valueSize + dir * 2, 16, 140);
    [JsonIgnore] public Brush AccentBrush => ColorUtil.Brush(_accent);
    [JsonIgnore] public FlowDirection Flow => TextDirectionUtil.Resolve(_label, _dir);
}

// ---------------------------------------------------------------- Checklist
public sealed class ChecklistItem : ItemBase
{
    public ChecklistItem() { Width = 420; Height = 190; }

    private string _itemsText = "Validate input at the boundary\nUse UTC everywhere\nCover it with tests\nDocument the contract";
    private string _style = "Check";   // Check | Dot | Number | Arrow | Cross
    private string _accent = "#2E9E52";
    private double _fontSize = 15;
    private TextDir _dir = TextDir.Auto;

    public string ItemsText { get => _itemsText; set { if (Set(ref _itemsText, value)) OnChanged(nameof(Rows)); } }
    public string Style { get => _style; set { if (Set(ref _style, value)) OnChanged(nameof(Rows)); } }
    public string Accent { get => _accent; set { if (Set(ref _accent, value)) { OnChanged(nameof(Rows)); OnChanged(nameof(AccentBrush)); } } }
    public double FontSize { get => _fontSize; set => Set(ref _fontSize, value); }
    public TextDir Dir { get => _dir; set { if (Set(ref _dir, value)) OnChanged(nameof(Flow)); } }

    [JsonIgnore] public override string LabelKey => "Widget.Checklist";
    [JsonIgnore] public override bool HasFont => true;
    public override void AdjustFont(int dir) => FontSize = Clamp(_fontSize + dir, 8, 48);
    [JsonIgnore] public Brush AccentBrush => ColorUtil.Brush(_accent);
    [JsonIgnore] public FlowDirection Flow => TextDirectionUtil.Resolve(_itemsText, _dir);
    [JsonIgnore]
    public IEnumerable<ChecklistRow> Rows
    {
        get
        {
            var accent = ColorUtil.Brush(_accent);
            var lines = Parse.Lines(_itemsText);
            for (int i = 0; i < lines.Count; i++)
            {
                string marker = _style switch
                {
                    "Dot" => "●",
                    "Number" => $"{i + 1}.",
                    "Arrow" => "➜",
                    "Cross" => "✕",
                    _ => "✓",
                };
                yield return new ChecklistRow { Marker = marker, Text = lines[i], Accent = accent };
            }
        }
    }
}

// ---------------------------------------------------------------- Quote / Callout
public sealed class QuoteItem : ItemBase
{
    public QuoteItem() { Width = 480; Height = 160; }

    private string _text = "Timestamps remove a whole class of date bugs.";
    private string _author = "— Best practice";
    private string _accent = "#2F80D8";
    private double _fontSize = 20;
    private TextDir _dir = TextDir.Auto;

    public string Text { get => _text; set { if (Set(ref _text, value)) OnChanged(nameof(Flow)); } }
    public string Author { get => _author; set => Set(ref _author, value); }
    public string Accent { get => _accent; set { if (Set(ref _accent, value)) OnChanged(nameof(AccentBrush)); } }
    public double FontSize { get => _fontSize; set => Set(ref _fontSize, value); }
    public TextDir Dir { get => _dir; set { if (Set(ref _dir, value)) OnChanged(nameof(Flow)); } }

    [JsonIgnore] public override string LabelKey => "Widget.Quote";
    [JsonIgnore] public override bool HasFont => true;
    public override void AdjustFont(int dir) => FontSize = Clamp(_fontSize + dir, 10, 60);
    [JsonIgnore] public Brush AccentBrush => ColorUtil.Brush(_accent);
    [JsonIgnore] public FlowDirection Flow => TextDirectionUtil.Resolve(_text, _dir);
}

// ---------------------------------------------------------------- Divider line
public sealed class DividerItem : ItemBase
{
    public DividerItem() { Width = 500; Height = 20; }

    private string _color = "#D2DAE6";
    private double _thickness = 3;
    private bool _dashed;

    public string Color { get => _color; set { if (Set(ref _color, value)) OnChanged(nameof(LineBrush)); } }
    public double Thickness { get => _thickness; set { if (Set(ref _thickness, value)) OnChanged(nameof(DashArray)); } }
    public bool Dashed { get => _dashed; set { if (Set(ref _dashed, value)) OnChanged(nameof(DashArray)); } }

    [JsonIgnore] public override string LabelKey => "Widget.Divider";
    [JsonIgnore] public Brush LineBrush => ColorUtil.Brush(_color);
    [JsonIgnore] public DoubleCollection? DashArray => _dashed ? new DoubleCollection(new double[] { 3, 2 }) : null;
}

// ---------------------------------------------------------------- Arrow
public sealed class ArrowItem : ItemBase
{
    public ArrowItem() { Width = 160; Height = 40; }

    private string _color = "#5B6472";
    private double _thickness = 4;

    public string Color { get => _color; set { if (Set(ref _color, value)) OnChanged(nameof(LineBrush)); } }
    public double Thickness { get => _thickness; set => Set(ref _thickness, value); }

    [JsonIgnore] public override string LabelKey => "Widget.Arrow";
    [JsonIgnore] public Brush LineBrush => ColorUtil.Brush(_color);
}

// ---------------------------------------------------------------- Pill / Tag
public sealed class PillItem : ItemBase
{
    public PillItem() { Width = 150; Height = 44; }

    private string _text = "#DotNet";
    private string _bg = "#EAF2FC";
    private string _fg = "#2F80D8";
    private double _fontSize = 15;
    private TextDir _dir = TextDir.Auto;

    public string Text { get => _text; set { if (Set(ref _text, value)) OnChanged(nameof(Flow)); } }
    public string Bg { get => _bg; set { if (Set(ref _bg, value)) OnChanged(nameof(BgBrush)); } }
    public string Fg { get => _fg; set { if (Set(ref _fg, value)) OnChanged(nameof(FgBrush)); } }
    public double FontSize { get => _fontSize; set => Set(ref _fontSize, value); }
    public TextDir Dir { get => _dir; set { if (Set(ref _dir, value)) OnChanged(nameof(Flow)); } }

    [JsonIgnore] public override string LabelKey => "Widget.Pill";
    [JsonIgnore] public override bool HasFont => true;
    public override void AdjustFont(int dir) => FontSize = Clamp(_fontSize + dir, 8, 48);
    [JsonIgnore] public Brush BgBrush => ColorUtil.Brush(_bg);
    [JsonIgnore] public Brush FgBrush => ColorUtil.Brush(_fg);
    [JsonIgnore] public FlowDirection Flow => TextDirectionUtil.Resolve(_text, _dir);
}

// ---------------------------------------------------------------- Progress bar
public sealed class ProgressItem : ItemBase
{
    public ProgressItem() { Width = 420; Height = 70; }

    private string _label = "Adoption";
    private double _percent = 72;
    private string _accent = "#2E9E52";
    private string _track = "#E4E9F0";
    private bool _showPercent = true;
    private TextDir _dir = TextDir.Auto;

    public string Label { get => _label; set { if (Set(ref _label, value)) OnChanged(nameof(Flow)); } }
    public double Percent
    {
        get => _percent;
        set { if (Set(ref _percent, Math.Clamp(value, 0, 100))) { OnChanged(nameof(FillWeight)); OnChanged(nameof(RestWeight)); OnChanged(nameof(PercentText)); } }
    }
    public string Accent { get => _accent; set { if (Set(ref _accent, value)) OnChanged(nameof(AccentBrush)); } }
    public string Track { get => _track; set { if (Set(ref _track, value)) OnChanged(nameof(TrackBrush)); } }
    public bool ShowPercent { get => _showPercent; set => Set(ref _showPercent, value); }
    public TextDir Dir { get => _dir; set { if (Set(ref _dir, value)) OnChanged(nameof(Flow)); } }

    [JsonIgnore] public override string LabelKey => "Widget.Progress";
    [JsonIgnore] public Brush AccentBrush => ColorUtil.Brush(_accent);
    [JsonIgnore] public Brush TrackBrush => ColorUtil.Brush(_track);
    [JsonIgnore] public FlowDirection Flow => TextDirectionUtil.Resolve(_label, _dir);
    [JsonIgnore] public GridLength FillWeight => new(_percent, GridUnitType.Star);
    [JsonIgnore] public GridLength RestWeight => new(100 - _percent, GridUnitType.Star);
    [JsonIgnore] public string PercentText => $"{_percent:0}%";
}

// ---------------------------------------------------------------- Icon
public sealed class IconItem : ItemBase
{
    public IconItem() { Width = 90; Height = 90; }

    private string _glyph = "🚀";
    private string _color = "#2F80D8";
    private double _iconSize = 40;
    private bool _showCircle = true;
    private string _circleColor = "#EAF2FC";

    public string Glyph { get => _glyph; set => Set(ref _glyph, value); }
    public string Color { get => _color; set { if (Set(ref _color, value)) OnChanged(nameof(ColorBrush)); } }
    public double IconSize { get => _iconSize; set => Set(ref _iconSize, value); }
    public bool ShowCircle { get => _showCircle; set { if (Set(ref _showCircle, value)) OnChanged(nameof(CircleVisibility)); } }
    public string CircleColor { get => _circleColor; set { if (Set(ref _circleColor, value)) OnChanged(nameof(CircleBrush)); } }

    [JsonIgnore] public override string LabelKey => "Widget.Icon";
    [JsonIgnore] public override bool HasFont => true;
    public override void AdjustFont(int dir) => IconSize = Clamp(_iconSize + dir * 3, 12, 240);
    [JsonIgnore] public Brush ColorBrush => ColorUtil.Brush(_color);
    [JsonIgnore] public Brush CircleBrush => ColorUtil.Brush(_circleColor);
    [JsonIgnore] public System.Windows.Visibility CircleVisibility => _showCircle ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
}

// ---------------------------------------------------------------- Illustration / header scene
public sealed class IllustrationItem : ItemBase
{
    public IllustrationItem() { Width = 420; Height = 130; }

    private string _nodesText = "📧 | #2F80D8\n🗄️ | #6C4BB6\n🛡️ | #2E9E52";
    private string _arrow = "#5B6472";
    private double _iconSize = 42;

    public string NodesText { get => _nodesText; set { if (Set(ref _nodesText, value)) OnChanged(nameof(Nodes)); } }
    public string Arrow { get => _arrow; set { if (Set(ref _arrow, value)) OnChanged(nameof(Nodes)); } }
    public double IconSize { get => _iconSize; set => Set(ref _iconSize, value); }

    [JsonIgnore] public override string LabelKey => "Widget.Scene";
    [JsonIgnore] public override bool HasFont => true;
    public override void AdjustFont(int dir) => IconSize = Clamp(_iconSize + dir * 3, 16, 160);
    [JsonIgnore]
    public IEnumerable<SceneNode> Nodes
    {
        get
        {
            var arrow = ColorUtil.Brush(_arrow);
            var lines = Parse.Lines(_nodesText);
            for (int i = 0; i < lines.Count; i++)
            {
                var (glyph, color) = Parse.Pair(lines[i]);
                if (string.IsNullOrEmpty(color)) color = "#2F80D8";
                yield return new SceneNode
                {
                    Glyph = glyph,
                    Tile = ColorUtil.Alpha(color, 36),
                    Border = ColorUtil.Alpha(color, 110),
                    ArrowBrush = arrow,
                    ShowArrow = i < lines.Count - 1
                };
            }
        }
    }
}

// ---------------------------------------------------------------- Table
public sealed class TableItem : ItemBase
{
    public TableItem() { Width = 580; Height = 220; }

    private string _rowsText = "Feature | Before | After\nN+1 queries | Many | One\nLatency | High | Low\nCode | Complex | Simple";
    private string _accent = "#2F80D8";
    private string _text = "#17233F";
    private double _fontSize = 15;
    private bool _firstRowHeader = true;
    private TextDir _dir = TextDir.Auto;

    public string RowsText { get => _rowsText; set { if (Set(ref _rowsText, value)) { OnChanged(nameof(Rows)); OnChanged(nameof(Flow)); } } }
    public string Accent { get => _accent; set { if (Set(ref _accent, value)) OnChanged(nameof(Rows)); } }
    public string Text { get => _text; set { if (Set(ref _text, value)) OnChanged(nameof(Rows)); } }
    public double FontSize { get => _fontSize; set => Set(ref _fontSize, value); }
    public bool FirstRowHeader { get => _firstRowHeader; set { if (Set(ref _firstRowHeader, value)) OnChanged(nameof(Rows)); } }
    public TextDir Dir { get => _dir; set { if (Set(ref _dir, value)) OnChanged(nameof(Flow)); } }

    [JsonIgnore] public override string LabelKey => "Widget.Table";
    [JsonIgnore] public override bool HasFont => true;
    public override void AdjustFont(int dir) => FontSize = Clamp(_fontSize + dir, 9, 36);
    [JsonIgnore] public FlowDirection Flow => TextDirectionUtil.Resolve(_rowsText, _dir);
    [JsonIgnore]
    public IEnumerable<TableRow> Rows
    {
        get
        {
            var lines = Parse.Lines(_rowsText);
            var cells = lines.Select(l => l.Split('|').Select(c => c.Trim()).ToArray()).ToList();
            int cols = cells.Count == 0 ? 1 : cells.Max(c => c.Length);
            var headerBg = ColorUtil.Alpha(_accent, 42);
            var accent = ColorUtil.Brush(_accent);
            var text = ColorUtil.Brush(_text);
            for (int i = 0; i < cells.Count; i++)
            {
                bool header = _firstRowHeader && i == 0;
                yield return new TableRow
                {
                    Cells = cells[i],
                    Columns = cols,
                    IsHeader = header,
                    Bg = header ? headerBg : (i % 2 == 0 ? Palette.White : ColorUtil.Brush("#F6F8FB")),
                    Fg = header ? accent : text,
                    Weight = header ? FontWeights.Bold : FontWeights.Normal,
                };
            }
        }
    }
}

// ---------------------------------------------------------------- Steps (vertical)
public sealed class StepsItem : ItemBase
{
    public StepsItem() { Width = 540; Height = 260; }

    private string _stepsText = "Define the contract | Agree on the API shape first\nValidate at the boundary | Parse and check input once\nStore in UTC | Never persist local time\nConvert on the way out | Format only for display";
    private string _accent = "#2F80D8";
    private double _fontSize = 15;
    private TextDir _dir = TextDir.Auto;

    public string StepsText { get => _stepsText; set { if (Set(ref _stepsText, value)) { OnChanged(nameof(Rows)); OnChanged(nameof(Flow)); } } }
    public string Accent { get => _accent; set { if (Set(ref _accent, value)) OnChanged(nameof(Rows)); } }
    public double FontSize { get => _fontSize; set => Set(ref _fontSize, value); }
    public TextDir Dir { get => _dir; set { if (Set(ref _dir, value)) OnChanged(nameof(Flow)); } }

    [JsonIgnore] public override string LabelKey => "Widget.Steps";
    [JsonIgnore] public override bool HasFont => true;
    public override void AdjustFont(int dir) => FontSize = Clamp(_fontSize + dir, 9, 36);
    [JsonIgnore] public FlowDirection Flow => TextDirectionUtil.Resolve(_stepsText, _dir);
    [JsonIgnore]
    public IEnumerable<StepRow> Rows
    {
        get
        {
            var accent = ColorUtil.Brush(_accent);
            var lines = Parse.Lines(_stepsText);
            for (int i = 0; i < lines.Count; i++)
            {
                var (a, b) = Parse.Pair(lines[i]);
                yield return new StepRow { Number = $"{i + 1}", Title = a, Desc = b, Accent = accent };
            }
        }
    }
}

// ---------------------------------------------------------------- Terminal
public sealed class TerminalItem : ItemBase
{
    public TerminalItem() { Width = 560; Height = 180; }

    private string _title = "bash";
    private string _code = "$ dotnet build\nBuild succeeded.\n$ dotnet run\nListening on http://localhost:5000";
    private double _fontSize = 14;

    public string Title { get => _title; set => Set(ref _title, value); }
    public string Code { get => _code; set { if (Set(ref _code, value)) OnChanged(nameof(Inlines)); } }
    public double FontSize { get => _fontSize; set => Set(ref _fontSize, value); }

    [JsonIgnore] public override string LabelKey => "Widget.Terminal";
    [JsonIgnore] public override bool HasFont => true;
    public override void AdjustFont(int dir) => FontSize = Clamp(_fontSize + dir, 9, 30);
    [JsonIgnore]
    public IEnumerable<Inline> Inlines
    {
        get
        {
            var list = new List<Inline>();
            var lines = (_code ?? "").Replace("\r\n", "\n").Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                if (line.StartsWith("$ "))
                {
                    list.Add(new Run("$ ") { Foreground = Palette.CodeComment, FontWeight = FontWeights.Bold });
                    list.Add(new Run(line[2..]) { Foreground = Palette.White });
                }
                else
                {
                    list.Add(new Run(line) { Foreground = Palette.CodePunct });
                }
                if (i < lines.Length - 1) list.Add(new LineBreak());
            }
            return list;
        }
    }
}
