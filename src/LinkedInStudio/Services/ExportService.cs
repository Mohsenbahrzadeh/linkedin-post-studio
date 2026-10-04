using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace LinkedInStudio.Services;

public static class ExportService
{
    /// <summary>
    /// Renders a framework element at (width x height) device-independent units,
    /// upscaled by <paramref name="scale"/> for crisp output.
    /// </summary>
    public static RenderTargetBitmap Render(FrameworkElement element, double width, double height, double scale)
    {
        // Make sure the element is fully measured/arranged at its true size.
        element.Measure(new Size(width, height));
        element.Arrange(new Rect(new Size(width, height)));
        element.UpdateLayout();

        var dpi = 96.0 * scale;
        var rtb = new RenderTargetBitmap(
            (int)Math.Round(width * scale),
            (int)Math.Round(height * scale),
            dpi, dpi, PixelFormats.Pbgra32);

        // Render through a VisualBrush so any parent transforms are ignored.
        var visual = new DrawingVisual();
        using (var ctx = visual.RenderOpen())
        {
            var brush = new VisualBrush(element) { Stretch = Stretch.None };
            ctx.DrawRectangle(brush, null, new Rect(new Size(width, height)));
        }
        rtb.Render(visual);
        return rtb;
    }

    public static void SavePng(BitmapSource bitmap, string path)
    {
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var fs = File.Create(path);
        encoder.Save(fs);
    }

    public static byte[] ToJpeg(BitmapSource bitmap, int quality = 92)
    {
        var encoder = new JpegBitmapEncoder { QualityLevel = quality };
        encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var ms = new MemoryStream();
        encoder.Save(ms);
        return ms.ToArray();
    }

    public static void ExportPng(FrameworkElement element, double width, double height, double scale, string path) =>
        SavePng(Render(element, width, height, scale), path);

    /// <summary>Writes the post text (with hashtags) next to the image so it can be pasted into LinkedIn.</summary>
    public static void SaveCaption(string caption, string imagePath)
    {
        if (string.IsNullOrWhiteSpace(caption)) return;
        File.WriteAllText(Path.ChangeExtension(imagePath, ".txt"), caption.Replace("\r\n", "\n"), new System.Text.UTF8Encoding(true));
    }
}
