using System.Collections.ObjectModel;
using System.IO;
using System.Text.Json.Serialization;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using LinkedInStudio.Infrastructure;

namespace LinkedInStudio.Models;

public sealed class DocumentModel : ObservableObject
{
    private double _canvasWidth = 1080;
    private double _canvasHeight = 1500;
    private string _background = "#FBFDFF";
    private double _footerHeight = 150;

    public double CanvasWidth { get => _canvasWidth; set { if (Set(ref _canvasWidth, Math.Clamp(value, 400, 4000))) OnChanged(nameof(ContentHeight)); } }
    public double CanvasHeight { get => _canvasHeight; set { if (Set(ref _canvasHeight, Math.Clamp(value, 400, 6000))) OnChanged(nameof(ContentHeight)); } }
    public string Background { get => _background; set { if (Set(ref _background, value)) OnChanged(nameof(BackgroundBrush)); } }
    public double FooterHeight { get => _footerHeight; set { if (Set(ref _footerHeight, value)) OnChanged(nameof(ContentHeight)); } }

    // Footer (fixed band) content
    private string _name = "Your Name";
    private string _role = "Your Title | Your Role";
    private string _tagline = "A short tagline about what you do";
    private string _email = "you@example.com";
    private string _linkedIn = "linkedin.com/in/your-handle";
    private string _photoPath = "";

    public string Name { get => _name; set => Set(ref _name, value); }
    public string Role { get => _role; set => Set(ref _role, value); }
    public string Tagline { get => _tagline; set => Set(ref _tagline, value); }
    public string Email { get => _email; set => Set(ref _email, value); }
    public string LinkedIn { get => _linkedIn; set => Set(ref _linkedIn, value); }
    public string PhotoPath { get => _photoPath; set { if (Set(ref _photoPath, value)) OnChanged(nameof(PhotoSource)); } }

    private string _caption = "";
    /// <summary>The LinkedIn post text (with hashtags). Exported next to the PNG as a .txt file.</summary>
    public string Caption { get => _caption; set => Set(ref _caption, value ?? ""); }

    public ObservableCollection<ItemBase> Items { get; set; } = new();

    [JsonIgnore] public Brush BackgroundBrush => ColorUtil.Brush(_background);
    [JsonIgnore] public double ContentHeight => Math.Max(0, _canvasHeight - _footerHeight);

    [JsonIgnore]
    public ImageSource PhotoSource
    {
        get
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(_photoPath) && File.Exists(_photoPath))
                {
                    var b = new BitmapImage();
                    b.BeginInit();
                    b.CacheOption = BitmapCacheOption.OnLoad;
                    b.UriSource = new Uri(_photoPath);
                    b.EndInit();
                    b.Freeze();
                    return b;
                }
            }
            catch { /* fall through to packaged asset */ }
            return new BitmapImage(new Uri("pack://application:,,,/Assets/profile.png"));
        }
    }
}
