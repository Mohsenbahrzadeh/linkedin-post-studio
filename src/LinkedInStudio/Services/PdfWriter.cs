using System.Globalization;
using System.IO;
using System.Text;

namespace LinkedInStudio.Services;

/// <summary>One page of a PDF: a full-page JPEG, sized in PDF points.</summary>
public sealed record PdfPage(byte[] Jpeg, int PixelWidth, int PixelHeight, double WidthPt, double HeightPt);

/// <summary>
/// Minimal PDF writer (one full-bleed JPEG per page, no dependencies). LinkedIn "document"
/// posts are swipeable carousels built from a PDF, so a series of slides can be exported this way.
/// </summary>
public static class PdfWriter
{
    public static void Write(string path, IReadOnlyList<PdfPage> pages)
    {
        if (pages.Count == 0) throw new ArgumentException("No pages to write.", nameof(pages));

        using var ms = new MemoryStream();
        var offsets = new List<long>();

        void Raw(string s) => ms.Write(Encoding.ASCII.GetBytes(s));
        void Begin(int id) { offsets.Add(ms.Position); Raw($"{id} 0 obj\n"); }
        static string N(double v) => v.ToString("0.##", CultureInfo.InvariantCulture);

        Raw("%PDF-1.4\n");
        ms.Write(new byte[] { (byte)'%', 0xE2, 0xE3, 0xCF, 0xD3, (byte)'\n' });

        Begin(1);
        Raw("<< /Type /Catalog /Pages 2 0 R >>\nendobj\n");

        Begin(2);
        var kids = string.Join(" ", Enumerable.Range(0, pages.Count).Select(i => $"{3 + 3 * i} 0 R"));
        Raw($"<< /Type /Pages /Kids [{kids}] /Count {pages.Count} >>\nendobj\n");

        for (int i = 0; i < pages.Count; i++)
        {
            var p = pages[i];
            int pageId = 3 + 3 * i, contentId = pageId + 1, imageId = pageId + 2;

            Begin(pageId);
            Raw($"<< /Type /Page /Parent 2 0 R /MediaBox [0 0 {N(p.WidthPt)} {N(p.HeightPt)}] " +
                $"/Resources << /XObject << /Im0 {imageId} 0 R >> >> /Contents {contentId} 0 R >>\nendobj\n");

            var content = Encoding.ASCII.GetBytes($"q {N(p.WidthPt)} 0 0 {N(p.HeightPt)} 0 0 cm /Im0 Do Q");
            Begin(contentId);
            Raw($"<< /Length {content.Length} >>\nstream\n");
            ms.Write(content);
            Raw("\nendstream\nendobj\n");

            Begin(imageId);
            Raw($"<< /Type /XObject /Subtype /Image /Width {p.PixelWidth} /Height {p.PixelHeight} " +
                $"/ColorSpace /DeviceRGB /BitsPerComponent 8 /Filter /DCTDecode /Length {p.Jpeg.Length} >>\nstream\n");
            ms.Write(p.Jpeg);
            Raw("\nendstream\nendobj\n");
        }

        long xref = ms.Position;
        Raw($"xref\n0 {offsets.Count + 1}\n");
        Raw("0000000000 65535 f \n");
        foreach (var o in offsets) Raw($"{o:D10} 00000 n \n");
        Raw($"trailer\n<< /Size {offsets.Count + 1} /Root 1 0 R >>\nstartxref\n{xref}\n%%EOF\n");

        File.WriteAllBytes(path, ms.ToArray());
    }
}
