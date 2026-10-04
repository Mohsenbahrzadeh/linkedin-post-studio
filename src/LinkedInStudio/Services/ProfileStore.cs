using System.IO;
using System.Text.Json;
using LinkedInStudio.Models;

namespace LinkedInStudio.Services;

/// <summary>A reusable footer identity (name, role, contacts, photo).</summary>
public sealed class FooterProfile
{
    public string Name { get; set; } = "";
    public string Role { get; set; } = "";
    public string Tagline { get; set; } = "";
    public string Email { get; set; } = "";
    public string LinkedIn { get; set; } = "";
    public string PhotoPath { get; set; } = "";
}

/// <summary>
/// Persists the user's footer identity locally (NOT in the repo), so the public build ships
/// generic placeholders while each user keeps their own footer. Stored in
/// %AppData%/LinkedInStudio/profile.json and applied to every new document.
/// </summary>
public static class ProfileStore
{
    private static readonly JsonSerializerOptions Options = new() { WriteIndented = true };

    private static string Dir =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "LinkedInStudio");

    private static string FilePath => Path.Combine(Dir, "profile.json");

    public static bool Exists => File.Exists(FilePath);

    public static FooterProfile? Load()
    {
        try
        {
            return File.Exists(FilePath)
                ? JsonSerializer.Deserialize<FooterProfile>(File.ReadAllText(FilePath), Options)
                : null;
        }
        catch { return null; }
    }

    public static void Save(FooterProfile profile)
    {
        try
        {
            Directory.CreateDirectory(Dir);
            File.WriteAllText(FilePath, JsonSerializer.Serialize(profile, Options));
        }
        catch { /* saving a preference must never crash the app */ }
    }

    public static FooterProfile From(DocumentModel doc) => new()
    {
        Name = doc.Name,
        Role = doc.Role,
        Tagline = doc.Tagline,
        Email = doc.Email,
        LinkedIn = doc.LinkedIn,
        PhotoPath = doc.PhotoPath,
    };

    /// <summary>Fills a document's footer from the saved profile, if one exists.</summary>
    public static void Apply(DocumentModel doc)
    {
        if (Load() is not { } p) return;
        if (!string.IsNullOrWhiteSpace(p.Name)) doc.Name = p.Name;
        if (!string.IsNullOrWhiteSpace(p.Role)) doc.Role = p.Role;
        if (!string.IsNullOrWhiteSpace(p.Tagline)) doc.Tagline = p.Tagline;
        if (!string.IsNullOrWhiteSpace(p.Email)) doc.Email = p.Email;
        if (!string.IsNullOrWhiteSpace(p.LinkedIn)) doc.LinkedIn = p.LinkedIn;
        if (!string.IsNullOrWhiteSpace(p.PhotoPath)) doc.PhotoPath = p.PhotoPath;
    }
}
