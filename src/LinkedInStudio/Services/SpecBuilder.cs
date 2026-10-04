using System.IO;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using LinkedInStudio.Models;

namespace LinkedInStudio.Services;

public sealed class SpecResult
{
    public DocumentModel Doc { get; init; } = new();
    public string Id { get; set; } = "post";
    public List<string> Warnings { get; } = new();
}

/// <summary>
/// Builds a finished post from a short JSON "spec": rows of widgets with no coordinates.
/// The builder lays the rows out top to bottom, splits each row into columns, measures every
/// widget's real height (so Persian and English text never overflows), equalises sibling boxes
/// and sizes the canvas to fit. This is what makes it practical to author many posts.
///
///   { "id": "n1-en", "caption": "…", "rows": [
///       { "kind": "title", "text": "Fix *N+1* in EF Core" },
///       { "vs": true, "items": [ { "kind": "problem", … }, { "kind": "solution", … } ] },
///       { "items": [ { "kind": "stat", "span": 1 }, { "kind": "stat", "span": 1 } ] } ] }
/// </summary>
public static class SpecBuilder
{
    private static readonly JsonDocumentOptions DocOptions = new()
    {
        CommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    private static readonly JsonSerializerOptions ItemOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowOutOfOrderMetadataProperties = true,
        Converters = { new JsonStringEnumConverter() },
    };

    private static readonly Dictionary<string, string> KindAliases = new(StringComparer.OrdinalIgnoreCase)
    {
        ["custom"] = "custombox", ["box"] = "custombox", ["list"] = "checklist", ["kpi"] = "stat",
        ["tag"] = "pill", ["label"] = "section", ["sectionlabel"] = "section", ["hook"] = "banner",
    };

    public static bool IsSpec(string json)
    {
        try
        {
            using var d = JsonDocument.Parse(json, DocOptions);
            return d.RootElement.ValueKind == JsonValueKind.Object &&
                   d.RootElement.EnumerateObject().Any(p => p.Name.Equals("rows", StringComparison.OrdinalIgnoreCase));
        }
        catch { return false; }
    }

    public static SpecResult Build(string json, string fallbackId)
    {
        var root = JsonNode.Parse(json, null, DocOptions) as JsonObject
                   ?? throw new InvalidDataException("The spec must be a JSON object.");

        var doc = new DocumentModel();
        var res = new SpecResult { Doc = doc, Id = Str(root, "id") ?? fallbackId };

        double width = Num(root, "width", 1080);
        double margin = Num(root, "margin", 60);
        double rowGap = Num(root, "gap", 22);
        double colGap = Num(root, "colGap", 24);
        double minHeight = Num(root, "minHeight", 1080);
        double fixedHeight = Num(root, "height", 0);
        double y = Num(root, "top", 52);

        doc.CanvasWidth = width;
        if (Str(root, "background") is { } bg) doc.Background = bg;
        if (Str(root, "caption") is { } caption) doc.Caption = caption;
        if (Get(root, "footer") is JsonObject f)
        {
            if (Str(f, "name") is { } v1) doc.Name = v1;
            if (Str(f, "role") is { } v2) doc.Role = v2;
            if (Str(f, "tagline") is { } v3) doc.Tagline = v3;
            if (Str(f, "email") is { } v4) doc.Email = v4;
            if (Str(f, "linkedIn") is { } v5) doc.LinkedIn = v5;
            if (Str(f, "photoPath") is { } v6) doc.PhotoPath = v6;
        }

        double usable = width - 2 * margin;
        var rows = Get(root, "rows") as JsonArray ?? throw new InvalidDataException("The spec needs a \"rows\" array.");

        int rowNo = 0;
        foreach (var rowNode in rows)
        {
            rowNo++;
            if (rowNode is not JsonObject row) continue;

            JsonArray cells;
            bool vs = false;
            double thisColGap = colGap;
            if (Get(row, "items") is JsonArray ia)
            {
                cells = ia;
                vs = Bool(row, "vs");
                thisColGap = Num(row, "colGap", vs ? 96 : colGap);
                y += Num(row, "before", 0);
            }
            else
            {
                cells = new JsonArray(row.DeepClone());   // a bare widget is a one-column row
            }

            var items = new List<Cell>();
            foreach (var n in cells)
            {
                if (n is not JsonObject o) continue;
                var cell = MakeCell(o, res.Warnings, rowNo);
                if (cell is not null) items.Add(cell);
            }
            if (items.Count == 0) continue;

            double totalSpan = items.Sum(c => c.Span);
            double avail = usable - thisColGap * (items.Count - 1);
            double x = margin;
            foreach (var c in items)
            {
                c.Item.X = x;
                c.Item.Width = Math.Round(avail * c.Span / totalSpan);
                x += c.Item.Width + thisColGap;
                if (c.Item is CodeItem code && !LayoutService.FitCodeFont(code))
                    res.Warnings.Add($"Row {rowNo}: code is wider than its block even at {code.FontSize}px; shorten the longest line.");
            }

            double rowH = 0;
            foreach (var c in items)
            {
                if (!c.ExplicitHeight && !LayoutService.IsFixedSize(c.Item))
                    c.Item.Height = Math.Max(40, LayoutService.MeasureHeight(c.Item));
                rowH = Math.Max(rowH, c.Item.Height);
            }
            if (items.Count > 1)
                foreach (var c in items)
                    if (!c.ExplicitHeight && LayoutService.IsStretchy(c.Item)) c.Item.Height = rowH;

            foreach (var c in items)
            {
                bool center = LayoutService.IsFixedSize(c.Item) || c.Item.Height < rowH && !LayoutService.IsStretchy(c.Item);
                c.Item.Y = y + (center ? Math.Round((rowH - c.Item.Height) / 2) : 0);
                doc.Items.Add(c.Item);
            }

            if (vs && items.Count >= 2)
            {
                var a = items[0].Item; var b = items[1].Item;
                var badge = new VsBadgeItem { Width = 66, Height = 66 };
                badge.X = Math.Round((a.X + a.Width + b.X) / 2 - 33);
                badge.Y = Math.Round(y + rowH / 2 - 33);
                doc.Items.Add(badge);
            }

            y += rowH + rowGap;
        }

        for (int i = 0; i < doc.Items.Count; i++) doc.Items[i].Z = i;

        double bottom = y - rowGap + 30;
        if (fixedHeight > 0)
        {
            doc.CanvasHeight = fixedHeight;
            if (bottom + doc.FooterHeight > fixedHeight)
                res.Warnings.Add($"Content needs {bottom + doc.FooterHeight:0}px but the canvas height is fixed at {fixedHeight:0}px.");
        }
        else
        {
            doc.CanvasHeight = Math.Max(minHeight, bottom + doc.FooterHeight);
        }
        if (doc.CanvasHeight > 1500)
            res.Warnings.Add($"Canvas is {doc.CanvasWidth:0}x{doc.CanvasHeight:0}; LinkedIn crops tall images, consider splitting into slides.");

        return res;
    }

    private sealed record Cell(ItemBase Item, double Span, bool ExplicitHeight);

    private static Cell? MakeCell(JsonObject src, List<string> warnings, int rowNo)
    {
        string? kind = null;
        double span = 1;
        bool explicitHeight = false;
        var props = new List<KeyValuePair<string, JsonNode?>>();

        foreach (var kv in src)
        {
            if (kv.Key.Equals("kind", StringComparison.OrdinalIgnoreCase) || kv.Key == "$kind")
            { kind = kv.Value?.GetValue<string>(); continue; }
            if (kv.Key.Equals("span", StringComparison.OrdinalIgnoreCase))
            { span = Math.Max(0.1, kv.Value?.GetValue<double>() ?? 1); continue; }
            if (kv.Key.Equals("height", StringComparison.OrdinalIgnoreCase)) explicitHeight = true;

            JsonNode? v = kv.Value?.DeepClone();
            // Convenience: a JSON array of strings is joined into the widget's one-item-per-line text.
            if (v is JsonArray arr && arr.All(e => e is JsonValue))
                v = string.Join("\n", arr.Select(e => e?.ToString() ?? ""));
            props.Add(new(kv.Key, v));
        }

        if (string.IsNullOrWhiteSpace(kind))
        {
            warnings.Add($"Row {rowNo}: a widget has no \"kind\" and was skipped.");
            return null;
        }
        if (KindAliases.TryGetValue(kind, out var real)) kind = real;

        var final = new JsonObject { ["$kind"] = kind.ToLowerInvariant() };
        foreach (var kv in props) final[kv.Key] = kv.Value;

        try
        {
            var item = JsonSerializer.Deserialize<ItemBase>(final.ToJsonString(), ItemOptions);
            return item is null ? null : new Cell(item, span, explicitHeight);
        }
        catch (Exception ex)
        {
            warnings.Add($"Row {rowNo}: could not read a \"{kind}\" widget ({ex.Message}).");
            return null;
        }
    }

    private static JsonNode? Get(JsonObject o, string name)
    {
        foreach (var kv in o)
            if (kv.Key.Equals(name, StringComparison.OrdinalIgnoreCase)) return kv.Value;
        return null;
    }

    private static string? Str(JsonObject o, string name) => Get(o, name) is JsonValue v && v.TryGetValue<string>(out var s) ? s : null;
    private static double Num(JsonObject o, string name, double fallback) => Get(o, name) is JsonValue v && v.TryGetValue<double>(out var d) ? d : fallback;
    private static bool Bool(JsonObject o, string name) => Get(o, name) is JsonValue v && v.TryGetValue<bool>(out var b) && b;
}
