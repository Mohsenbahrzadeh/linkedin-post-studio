using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Media.Imaging;
using LinkedInStudio.Controls;
using LinkedInStudio.Models;
using LinkedInStudio.ViewModels;

namespace LinkedInStudio.Services;

/// <summary>
/// Command-line mode (no window). Examples:
///   LinkedInStudio.exe --render out.png --in post.lipost
///   LinkedInStudio.exe --spec posts\*.json --out dist [--pdf carousel.pdf] [--scale 2]
/// For every input it writes &lt;id&gt;.png, &lt;id&gt;.lipost and (if the post has a caption) &lt;id&gt;.txt,
/// and a build.log with sizes and layout warnings. --pdf also bundles all pages into one document.
/// </summary>
public static class HeadlessRunner
{
    public static bool Handles(string[] args) =>
        Array.IndexOf(args, "--render") >= 0 || Array.IndexOf(args, "--spec") >= 0;

    public static void Run(string[] args)
    {
        var log = new StringBuilder();
        string outDir = Option(args, "--out") ?? Directory.GetCurrentDirectory();
        try
        {
            if (Option(args, "--render") is { } png)
            {
                var project = Option(args, "--in");
                var doc = project is not null ? DocumentStore.Load(project) : null;
                var bmp = RenderDocument(doc, Scale(args));
                ExportService.SavePng(bmp, png);
                if (doc is not null) ExportService.SaveCaption(doc.Caption, png);
                return;
            }

            Directory.CreateDirectory(outDir);
            double scale = Scale(args);
            var pages = new List<PdfPage>();

            foreach (var file in SpecFiles(args))
            {
                try
                {
                    var json = File.ReadAllText(file);
                    string id = Path.GetFileNameWithoutExtension(file);
                    DocumentModel doc;
                    List<string> warnings = new();
                    if (SpecBuilder.IsSpec(json))
                    {
                        var res = SpecBuilder.Build(json, id);
                        doc = res.Doc; id = res.Id; warnings = res.Warnings;
                    }
                    else doc = DocumentStore.Load(file);

                    var basePath = Path.Combine(outDir, id);
                    var bmp = RenderDocument(doc, scale);
                    ExportService.SavePng(bmp, basePath + ".png");
                    DocumentStore.Save(doc, basePath + ".lipost");
                    ExportService.SaveCaption(doc.Caption, basePath + ".png");

                    pages.Add(new PdfPage(ExportService.ToJpeg(bmp), bmp.PixelWidth, bmp.PixelHeight, doc.CanvasWidth, doc.CanvasHeight));
                    log.AppendLine($"OK   {id}  {doc.CanvasWidth:0}x{doc.CanvasHeight:0}  {doc.Items.Count} widgets");
                    foreach (var w in warnings) log.AppendLine($"     warning: {w}");
                }
                catch (Exception ex)
                {
                    log.AppendLine($"FAIL {file}: {ex.Message}");
                }
            }

            if (Option(args, "--pdf") is { } pdf && pages.Count > 0)
            {
                var pdfPath = Path.IsPathRooted(pdf) ? pdf : Path.Combine(outDir, pdf);
                PdfWriter.Write(pdfPath, pages);
                log.AppendLine($"PDF  {pdfPath}  {pages.Count} pages");
            }
        }
        catch (Exception ex)
        {
            log.AppendLine($"ERROR {ex}");
        }
        finally
        {
            try { File.WriteAllText(Path.Combine(Directory.Exists(outDir) ? outDir : Directory.GetCurrentDirectory(), "build.log"), log.ToString()); }
            catch { /* logging must never break the render */ }
        }
    }

    private static BitmapSource RenderDocument(DocumentModel? doc, double scale)
    {
        var vm = new EditorViewModel();
        if (doc is not null) vm.LoadDocument(doc);

        var surface = new DesignerSurface { DataContext = vm };
        surface.Width = vm.Doc.CanvasWidth + 200;
        surface.Height = vm.Doc.CanvasHeight + 200;
        surface.Measure(new Size(surface.Width, surface.Height));
        surface.Arrange(new Rect(0, 0, surface.Width, surface.Height));
        surface.UpdateLayout();

        return ExportService.Render(surface.ExportElement, vm.Doc.CanvasWidth, vm.Doc.CanvasHeight, scale);
    }

    private static double Scale(string[] args) =>
        double.TryParse(Option(args, "--scale"), System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out var s) && s > 0 ? s : 2.0;

    private static string? Option(string[] args, string name)
    {
        int i = Array.IndexOf(args, name);
        return i >= 0 && i + 1 < args.Length && !args[i + 1].StartsWith("--") ? args[i + 1] : null;
    }

    /// <summary>All values after --spec up to the next option; folders and wildcards are expanded.</summary>
    private static IEnumerable<string> SpecFiles(string[] args)
    {
        int i = Array.IndexOf(args, "--spec");
        var files = new List<string>();
        for (int k = i + 1; k < args.Length && !args[k].StartsWith("--"); k++)
        {
            var a = args[k];
            if (Directory.Exists(a))
                files.AddRange(Directory.GetFiles(a, "*.json").Concat(Directory.GetFiles(a, "*.lipost")).OrderBy(f => f, StringComparer.OrdinalIgnoreCase));
            else if (a.Contains('*') || a.Contains('?'))
            {
                var dir = Path.GetDirectoryName(a);
                if (string.IsNullOrEmpty(dir)) dir = ".";
                files.AddRange(Directory.GetFiles(dir, Path.GetFileName(a)).OrderBy(f => f, StringComparer.OrdinalIgnoreCase));
            }
            else files.Add(a);
        }
        return files;
    }
}
