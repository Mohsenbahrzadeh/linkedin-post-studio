using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using LinkedInStudio.Models;

namespace LinkedInStudio.Services;

public static class DocumentStore
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };

    public static void Save(DocumentModel doc, string path)
    {
        var json = JsonSerializer.Serialize(doc, Options);
        File.WriteAllText(path, json);
    }

    public static DocumentModel Load(string path)
    {
        var json = File.ReadAllText(path);
        // A "spec" (rows of widgets, no coordinates) is laid out automatically; a project is loaded as-is.
        if (SpecBuilder.IsSpec(json))
            return SpecBuilder.Build(json, Path.GetFileNameWithoutExtension(path)).Doc;
        return JsonSerializer.Deserialize<DocumentModel>(json, Options) ?? new DocumentModel();
    }

    public static string Serialize(DocumentModel doc) => JsonSerializer.Serialize(doc, Options);

    public static DocumentModel Deserialize(string json) =>
        JsonSerializer.Deserialize<DocumentModel>(json, Options) ?? new DocumentModel();
}
