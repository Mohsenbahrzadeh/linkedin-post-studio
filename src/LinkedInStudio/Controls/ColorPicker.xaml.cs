using System.Windows;
using System.Windows.Controls;
using LinkedInStudio.Infrastructure;

namespace LinkedInStudio.Controls;

public partial class ColorPicker : UserControl
{
    public static readonly DependencyProperty ColorProperty =
        DependencyProperty.Register(nameof(Color), typeof(string), typeof(ColorPicker),
            new FrameworkPropertyMetadata("#000000", FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

    public string Color
    {
        get => (string)GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    public ColorPicker()
    {
        InitializeComponent();
        Swatches.ItemsSource = Catalog.Swatches;
    }

    private void Swatch_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.Tag is string hex)
        {
            Color = hex;
            Toggle.IsChecked = false;
        }
    }
}
