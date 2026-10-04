using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using LinkedInStudio.Models;
using LinkedInStudio.ViewModels;

namespace LinkedInStudio.Controls;

public partial class DesignerSurface : UserControl
{
    private const double MinW = 40;
    private const double MinH = 30;
    private const double SnapThreshold = 8;

    public DesignerSurface() => InitializeComponent();

    /// <summary>The true-size element rendered on export.</summary>
    public FrameworkElement ExportElement => ExportRoot;

    private EditorViewModel? Vm => DataContext as EditorViewModel;

    private void Thumb_DragStarted(object sender, DragStartedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.DataContext is ItemBase item && Vm is not null)
            Vm.Selected = item;
    }

    private void Thumb_DragDelta(object sender, DragDeltaEventArgs e)
    {
        if (sender is not FrameworkElement fe || fe.DataContext is not ItemBase item || item.IsLocked) return;
        var tag = (fe.Tag as string) ?? "MOVE";
        double dx = e.HorizontalChange, dy = e.VerticalChange;

        if (tag == "MOVE")
        {
            item.X += dx;
            item.Y += dy;
            ApplySnapping(item);
            return;
        }

        double x = item.X, y = item.Y, w = item.Width, h = item.Height;
        if (tag.Contains('L')) { var nw = w - dx; if (nw >= MinW) { x += dx; w = nw; } }
        if (tag.Contains('R')) { w = Math.Max(MinW, w + dx); }
        if (tag.Contains('T')) { var nh = h - dy; if (nh >= MinH) { y += dy; h = nh; } }
        if (tag.Contains('B')) { h = Math.Max(MinH, h + dy); }
        item.X = x; item.Y = y; item.Width = w; item.Height = h;
    }

    private void Thumb_DragCompleted(object sender, DragCompletedEventArgs e)
    {
        GuideV.Visibility = Visibility.Collapsed;
        GuideH.Visibility = Visibility.Collapsed;
        Vm?.Commit();
    }

    private void ApplySnapping(ItemBase item)
    {
        if (Vm is null) return;
        double cw = Vm.Doc.CanvasWidth, ch = Vm.Doc.CanvasHeight;

        // ---- X axis ----
        var targetsX = new List<double> { 0, cw, cw / 2, 60, cw - 60 };
        foreach (var o in Vm.Items)
            if (!ReferenceEquals(o, item) && o.IsVisible)
            { targetsX.Add(o.X); targetsX.Add(o.X + o.Width / 2); targetsX.Add(o.X + o.Width); }

        if (BestSnap(new[] { item.X, item.X + item.Width / 2, item.X + item.Width }, targetsX, out double adjX, out double gx))
        {
            item.X += adjX;
            GuideV.X1 = GuideV.X2 = gx;
            GuideV.Visibility = Visibility.Visible;
        }
        else GuideV.Visibility = Visibility.Collapsed;

        // ---- Y axis ----
        var targetsY = new List<double> { 0, ch, ch / 2, 46, ch - Vm.Doc.FooterHeight };
        foreach (var o in Vm.Items)
            if (!ReferenceEquals(o, item) && o.IsVisible)
            { targetsY.Add(o.Y); targetsY.Add(o.Y + o.Height / 2); targetsY.Add(o.Y + o.Height); }

        if (BestSnap(new[] { item.Y, item.Y + item.Height / 2, item.Y + item.Height }, targetsY, out double adjY, out double gy))
        {
            item.Y += adjY;
            GuideH.Y1 = GuideH.Y2 = gy;
            GuideH.Visibility = Visibility.Visible;
        }
        else GuideH.Visibility = Visibility.Collapsed;
    }

    private static bool BestSnap(double[] edges, List<double> targets, out double adjust, out double guide)
    {
        adjust = 0; guide = 0;
        double best = SnapThreshold;
        bool found = false;
        foreach (var edge in edges)
            foreach (var t in targets)
            {
                double d = Math.Abs(edge - t);
                if (d <= best)
                {
                    best = d; adjust = t - edge; guide = t; found = true;
                }
            }
        return found;
    }

    private void Background_MouseDown(object sender, RoutedEventArgs e)
    {
        if (Vm is not null) Vm.Selected = null;
    }
}
