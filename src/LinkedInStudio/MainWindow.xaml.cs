using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Microsoft.Win32;
using LinkedInStudio.Models;
using LinkedInStudio.Services;
using LinkedInStudio.ViewModels;

namespace LinkedInStudio;

public partial class MainWindow : Window
{
    private readonly EditorViewModel _vm = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _vm;

        _vm.PickFile = save =>
        {
            if (save)
            {
                var dlg = new SaveFileDialog
                {
                    Title = _vm.Loc["Dlg.SaveTitle"],
                    Filter = "LinkedIn Studio project (*.lipost)|*.lipost|JSON (*.json)|*.json",
                    FileName = "post.lipost"
                };
                return dlg.ShowDialog(this) == true ? dlg.FileName : null;
            }
            var open = new OpenFileDialog
            {
                Title = _vm.Loc["Dlg.OpenTitle"],
                Filter = "LinkedIn Studio project (*.lipost;*.json)|*.lipost;*.json|All files (*.*)|*.*"
            };
            return open.ShowDialog(this) == true ? open.FileName : null;
        };

        _vm.PickImage = () =>
        {
            var dlg = new OpenFileDialog
            {
                Title = _vm.Loc["Props.ChooseImage"],
                Filter = "Images (*.png;*.jpg;*.jpeg;*.bmp;*.gif)|*.png;*.jpg;*.jpeg;*.bmp;*.gif"
            };
            return dlg.ShowDialog(this) == true ? dlg.FileName : null;
        };

        _vm.PickExportPath = () =>
        {
            var dlg = new SaveFileDialog
            {
                Title = _vm.Loc["Dlg.ExportTitle"],
                Filter = "PNG image (*.png)|*.png",
                FileName = "linkedin-post.png"
            };
            return dlg.ShowDialog(this) == true ? dlg.FileName : null;
        };

        _vm.ExportTo = path =>
        {
            Surface.UpdateLayout();
            ExportService.ExportPng(Surface.ExportElement, _vm.Doc.CanvasWidth, _vm.Doc.CanvasHeight, 2.0, path);
            ExportService.SaveCaption(_vm.Doc.Caption, path);
        };

        _vm.Toast = msg => MessageBox.Show(this, msg, "LinkedIn Studio",
            MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void Layer_Click(object sender, MouseButtonEventArgs e)
    {
        if (sender is FrameworkElement fe && fe.DataContext is ItemBase item)
            _vm.Selected = item;
    }

    private void Props_LostFocus(object sender, RoutedEventArgs e) => _vm.Commit();

    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);

        // Never hijack shortcuts while typing in a text field.
        if (Keyboard.FocusedElement is TextBoxBase) return;

        bool ctrl = (Keyboard.Modifiers & ModifierKeys.Control) != 0;
        bool shift = (Keyboard.Modifiers & ModifierKeys.Shift) != 0;

        if (ctrl)
        {
            switch (e.Key)
            {
                case Key.Z: Exec(_vm.UndoCommand); e.Handled = true; return;
                case Key.Y: Exec(_vm.RedoCommand); e.Handled = true; return;
                case Key.D: Exec(_vm.DuplicateCommand); e.Handled = true; return;
                case Key.C: Exec(_vm.CopyCommand); e.Handled = true; return;
                case Key.V: Exec(_vm.PasteCommand); e.Handled = true; return;
                case Key.OemPlus or Key.Add: _vm.AdjustFont(+1); e.Handled = true; return;
                case Key.OemMinus or Key.Subtract: _vm.AdjustFont(-1); e.Handled = true; return;
            }
        }

        if (e.Key == Key.Delete) { Exec(_vm.DeleteCommand); e.Handled = true; return; }

        double step = shift ? 10 : 1;
        switch (e.Key)
        {
            case Key.Left: _vm.Nudge(-step, 0); e.Handled = true; break;
            case Key.Right: _vm.Nudge(step, 0); e.Handled = true; break;
            case Key.Up: _vm.Nudge(0, -step); e.Handled = true; break;
            case Key.Down: _vm.Nudge(0, step); e.Handled = true; break;
        }
    }

    protected override void OnPreviewKeyUp(KeyEventArgs e)
    {
        base.OnPreviewKeyUp(e);
        if (Keyboard.FocusedElement is TextBoxBase) return;
        if (e.Key is Key.Left or Key.Right or Key.Up or Key.Down)
            _vm.Commit();
    }

    private static void Exec(ICommand cmd) { if (cmd.CanExecute(null)) cmd.Execute(null); }
}
