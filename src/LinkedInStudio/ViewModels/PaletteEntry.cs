using LinkedInStudio.Models;

namespace LinkedInStudio.ViewModels;

public sealed class PaletteEntry
{
    public required string LabelKey { get; init; }
    public required string Glyph { get; init; }
    public required Func<ItemBase> Factory { get; init; }
}
