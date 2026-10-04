using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;
using LinkedInStudio.Infrastructure;
using LinkedInStudio.Localization;
using LinkedInStudio.Models;
using LinkedInStudio.Services;

namespace LinkedInStudio.ViewModels;

public sealed class EditorViewModel : ObservableObject
{
    private static readonly JsonSerializerOptions CloneOptions = new()
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly History _history = new();
    private string? _clipboard;

    public Localizer Loc => Localizer.Instance;

    private DocumentModel _doc = new();
    public DocumentModel Doc { get => _doc; private set => Set(ref _doc, value); }

    public ObservableCollection<ItemBase> Items => _doc.Items;

    private ItemBase? _selected;
    public ItemBase? Selected
    {
        get => _selected;
        set
        {
            if (_selected == value) return;
            if (_selected is not null) _selected.IsSelected = false;
            _selected = value;
            if (_selected is not null) _selected.IsSelected = true;
            OnChanged();
            OnChanged(nameof(HasSelection));
        }
    }

    public bool HasSelection => _selected is not null;
    public bool CanUndo => _history.CanUndo;
    public bool CanRedo => _history.CanRedo;

    public List<PaletteEntry> Palette { get; }

    // File dialog / export hooks wired by the view.
    public Func<bool, string?>? PickFile;
    public Func<string?>? PickImage;
    public Func<string?>? PickExportPath;
    public Action<string>? ExportTo;
    public Action<string>? Toast;

    public RelayCommand AddCommand { get; }
    public RelayCommand DeleteCommand { get; }
    public RelayCommand DeleteItemCommand { get; }
    public RelayCommand DuplicateCommand { get; }
    public RelayCommand FrontCommand { get; }
    public RelayCommand BackCommand { get; }
    public RelayCommand MoveUpCommand { get; }
    public RelayCommand MoveDownCommand { get; }
    public RelayCommand NewCommand { get; }
    public RelayCommand SaveCommand { get; }
    public RelayCommand OpenCommand { get; }
    public RelayCommand ExportCommand { get; }
    public RelayCommand LangCommand { get; }
    public RelayCommand PickImageCommand { get; }
    public RelayCommand PickPhotoCommand { get; }
    public RelayCommand SetSizeCommand { get; }
    public RelayCommand UndoCommand { get; }
    public RelayCommand RedoCommand { get; }
    public RelayCommand CopyCommand { get; }
    public RelayCommand PasteCommand { get; }
    public RelayCommand SelectCommand { get; }
    public RelayCommand FontBiggerCommand { get; }
    public RelayCommand FontSmallerCommand { get; }
    public RelayCommand FitCommand { get; }
    public RelayCommand FitAllCommand { get; }
    public RelayCommand SaveProfileCommand { get; }

    public EditorViewModel()
    {
        Palette = new List<PaletteEntry>
        {
            new() { LabelKey = "Widget.Title",        Glyph = "T",   Factory = () => new TitleItem() },
            new() { LabelKey = "Widget.Banner",       Glyph = "▬",   Factory = () => new BannerItem() },
            new() { LabelKey = "Widget.SectionLabel", Glyph = "▭",   Factory = () => new SectionLabelItem() },
            new() { LabelKey = "Widget.Problem",      Glyph = "✕",   Factory = () => new ProblemBoxItem() },
            new() { LabelKey = "Widget.Solution",     Glyph = "✓",   Factory = () => new SolutionBoxItem() },
            new() { LabelKey = "Widget.Custom",       Glyph = "▢",   Factory = () => new CustomBoxItem() },
            new() { LabelKey = "Widget.Code",         Glyph = "{ }", Factory = () => new CodeItem() },
            new() { LabelKey = "Widget.Notes",        Glyph = "≣",   Factory = () => new NotesItem() },
            new() { LabelKey = "Widget.Reference",    Glyph = "≡",   Factory = () => new ReferenceItem() },
            new() { LabelKey = "Widget.Checklist",    Glyph = "☑",   Factory = () => new ChecklistItem() },
            new() { LabelKey = "Widget.Flow",         Glyph = "→",   Factory = () => new FlowItem() },
            new() { LabelKey = "Widget.Benefits",     Glyph = "★",   Factory = () => new BenefitsItem() },
            new() { LabelKey = "Widget.Stat",         Glyph = "42",  Factory = () => new StatItem() },
            new() { LabelKey = "Widget.Progress",     Glyph = "▰▱",  Factory = () => new ProgressItem() },
            new() { LabelKey = "Widget.Quote",        Glyph = "❝",   Factory = () => new QuoteItem() },
            new() { LabelKey = "Widget.Text",         Glyph = "Aa",  Factory = () => new TextItem() },
            new() { LabelKey = "Widget.Pill",         Glyph = "#",   Factory = () => new PillItem() },
            new() { LabelKey = "Widget.Icon",         Glyph = "◉",   Factory = () => new IconItem() },
            new() { LabelKey = "Widget.Image",        Glyph = "🖼",  Factory = () => new ImageItem() },
            new() { LabelKey = "Widget.Vs",           Glyph = "VS",  Factory = () => new VsBadgeItem() },
            new() { LabelKey = "Widget.Divider",      Glyph = "―",   Factory = () => new DividerItem() },
            new() { LabelKey = "Widget.Arrow",        Glyph = "➜",   Factory = () => new ArrowItem() },
            new() { LabelKey = "Widget.Scene",        Glyph = "⬚→⬚", Factory = () => new IllustrationItem() },
            new() { LabelKey = "Widget.Table",        Glyph = "▦",   Factory = () => new TableItem() },
            new() { LabelKey = "Widget.Steps",        Glyph = "①②",  Factory = () => new StepsItem() },
            new() { LabelKey = "Widget.Terminal",     Glyph = "▢_",  Factory = () => new TerminalItem() },
        };

        AddCommand = new RelayCommand(p => Add(p as PaletteEntry));
        DeleteCommand = new RelayCommand(_ => Delete(), _ => HasSelection);
        DeleteItemCommand = new RelayCommand(p => DeleteItem(p as ItemBase));
        DuplicateCommand = new RelayCommand(_ => Duplicate(), _ => HasSelection);
        FrontCommand = new RelayCommand(_ => BringFront(), _ => HasSelection);
        BackCommand = new RelayCommand(_ => SendBack(), _ => HasSelection);
        MoveUpCommand = new RelayCommand(p => MoveLayer(p as ItemBase ?? _selected, +1));
        MoveDownCommand = new RelayCommand(p => MoveLayer(p as ItemBase ?? _selected, -1));
        NewCommand = new RelayCommand(_ => NewDocument());
        SaveCommand = new RelayCommand(_ => Save());
        OpenCommand = new RelayCommand(_ => Open());
        ExportCommand = new RelayCommand(_ => Export());
        LangCommand = new RelayCommand(_ => Loc.Toggle());
        PickImageCommand = new RelayCommand(_ => ChooseImage(), _ => Selected is ImageItem);
        PickPhotoCommand = new RelayCommand(_ => ChoosePhoto());
        SetSizeCommand = new RelayCommand(p => ApplyPreset(p as string));
        UndoCommand = new RelayCommand(_ => Undo(), _ => CanUndo);
        RedoCommand = new RelayCommand(_ => Redo(), _ => CanRedo);
        CopyCommand = new RelayCommand(_ => Copy(), _ => HasSelection);
        PasteCommand = new RelayCommand(_ => Paste(), _ => _clipboard is not null);
        SelectCommand = new RelayCommand(p => Selected = p as ItemBase);
        FontBiggerCommand = new RelayCommand(_ => AdjustFont(+1), _ => Selected?.HasFont == true);
        FontSmallerCommand = new RelayCommand(_ => AdjustFont(-1), _ => Selected?.HasFont == true);
        FitCommand = new RelayCommand(_ => FitSelected(), _ => HasSelection);
        FitAllCommand = new RelayCommand(_ => FitEverything());
        SaveProfileCommand = new RelayCommand(_ => SaveProfile());

        BuildStarter();
        ProfileStore.Apply(_doc);
        _history.Reset(_doc);
    }

    // ------------------------------------------------------------ history
    public void Commit()
    {
        _history.Commit(_doc);
        OnChanged(nameof(CanUndo));
        OnChanged(nameof(CanRedo));
    }

    private void Undo()
    {
        var d = _history.Undo(_doc);
        if (d is not null) SwapDocument(d);
    }

    private void Redo()
    {
        var d = _history.Redo(_doc);
        if (d is not null) SwapDocument(d);
    }

    private void SwapDocument(DocumentModel doc)
    {
        Doc = doc;
        Selected = null;
        OnChanged(nameof(Items));
        OnChanged(nameof(CanUndo));
        OnChanged(nameof(CanRedo));
    }

    // ------------------------------------------------------------ items
    private void Add(PaletteEntry? entry)
    {
        if (entry is null) return;
        var item = entry.Factory();
        var offset = (Items.Count % 8) * 24;
        item.X = 80 + offset;
        item.Y = 80 + offset;
        Items.Add(item);
        Renumber();
        Selected = item;
        Commit();
    }

    private void Renumber()
    {
        for (int i = 0; i < Items.Count; i++) Items[i].Z = i;
    }

    private void Delete() => DeleteItem(_selected);

    public void DeleteItem(ItemBase? item)
    {
        if (item is null) return;
        var idx = Items.IndexOf(item);
        if (idx < 0) return;
        Items.Remove(item);
        Renumber();
        Selected = Items.Count == 0 ? null : Items[Math.Clamp(idx, 0, Items.Count - 1)];
        Commit();
    }

    private ItemBase? Clone(ItemBase src)
    {
        var json = JsonSerializer.Serialize(src, src.GetType(), CloneOptions);
        if (JsonSerializer.Deserialize<ItemBase>(json, CloneOptions) is { } clone)
        {
            clone.Id = Guid.NewGuid().ToString("N");
            return clone;
        }
        return null;
    }

    private void Duplicate()
    {
        if (_selected is null || Clone(_selected) is not { } clone) return;
        clone.X += 24;
        clone.Y += 24;
        Items.Add(clone);
        Renumber();
        Selected = clone;
        Commit();
    }

    private void Copy() { if (_selected is not null) _clipboard = JsonSerializer.Serialize(_selected, _selected.GetType(), CloneOptions); }

    private void Paste()
    {
        if (_clipboard is null) return;
        if (JsonSerializer.Deserialize<ItemBase>(_clipboard, CloneOptions) is { } clone)
        {
            clone.Id = Guid.NewGuid().ToString("N");
            clone.X += 24;
            clone.Y += 24;
            Items.Add(clone);
            Renumber();
            Selected = clone;
            Commit();
        }
    }

    private void BringFront() { if (_selected is not null) { Items.Move(Items.IndexOf(_selected), Items.Count - 1); Renumber(); Commit(); } }
    private void SendBack() { if (_selected is not null) { Items.Move(Items.IndexOf(_selected), 0); Renumber(); Commit(); } }

    private void MoveLayer(ItemBase? item, int dir)
    {
        if (item is null) return;
        int i = Items.IndexOf(item);
        int j = i + dir;
        if (i < 0 || j < 0 || j >= Items.Count) return;
        Items.Move(i, j);
        Renumber();
        Commit();
    }

    public void Nudge(double dx, double dy)
    {
        if (_selected is null || _selected.IsLocked) return;
        _selected.X += dx;
        _selected.Y += dy;
    }

    public void AdjustFont(int dir)
    {
        if (_selected is { HasFont: true })
        {
            _selected.AdjustFont(dir);
            Commit();
        }
    }

    /// <summary>Sets the selected widget's height to exactly what its content needs.</summary>
    private void FitSelected()
    {
        if (_selected is null) return;
        LayoutService.FitHeight(_selected);
        Commit();
    }

    /// <summary>Fits every widget to its content and trims the canvas to end just above the footer.</summary>
    private void FitEverything()
    {
        LayoutService.FitAll(_doc);
        LayoutService.FitCanvas(_doc);
        Commit();
    }

    // ------------------------------------------------------------ document / files
    private void NewDocument()
    {
        Doc = new DocumentModel();
        ProfileStore.Apply(_doc);
        Selected = null;
        OnChanged(nameof(Items));
        _history.Reset(_doc);
        OnChanged(nameof(CanUndo));
        OnChanged(nameof(CanRedo));
    }

    private void SaveProfile()
    {
        ProfileStore.Save(ProfileStore.From(_doc));
        Toast?.Invoke(Loc["Footer.ProfileSaved"]);
    }

    private void Save()
    {
        var path = PickFile?.Invoke(true);
        if (string.IsNullOrEmpty(path)) return;
        DocumentStore.Save(_doc, path);
        Toast?.Invoke(path);
    }

    private void Open()
    {
        var path = PickFile?.Invoke(false);
        if (string.IsNullOrEmpty(path)) return;
        LoadDocument(DocumentStore.Load(path));
    }

    public void LoadDocument(DocumentModel doc)
    {
        Doc = doc;
        Selected = null;
        OnChanged(nameof(Items));
        _history.Reset(_doc);
        OnChanged(nameof(CanUndo));
        OnChanged(nameof(CanRedo));
    }

    private void Export()
    {
        var path = PickExportPath?.Invoke();
        if (string.IsNullOrEmpty(path)) return;
        Selected = null;
        ExportTo?.Invoke(path);
        Toast?.Invoke(Loc["Msg.Exported"]);
    }

    private void ChooseImage()
    {
        if (_selected is not ImageItem img) return;
        var path = PickImage?.Invoke();
        if (!string.IsNullOrEmpty(path)) { img.Path = path; Commit(); }
    }

    private void ChoosePhoto()
    {
        var path = PickImage?.Invoke();
        if (!string.IsNullOrEmpty(path)) { _doc.PhotoPath = path; Commit(); }
    }

    private void ApplyPreset(string? preset)
    {
        switch (preset)
        {
            case "portrait": _doc.CanvasWidth = 1080; _doc.CanvasHeight = 1350; break;
            case "square": _doc.CanvasWidth = 1080; _doc.CanvasHeight = 1080; break;
        }
        Commit();
    }

    private void BuildStarter()
    {
        void A(ItemBase it, double x, double y, double w, double h)
        {
            it.X = x; it.Y = y; it.Width = w; it.Height = h; it.Z = Items.Count;
            Items.Add(it);
        }

        A(new TitleItem(), 60, 46, 900, 130);
        A(new BannerItem(), 60, 188, 960, 96);
        A(new ProblemBoxItem(), 60, 300, 440, 348);
        A(new VsBadgeItem(), 508, 448, 66, 66);
        A(new SolutionBoxItem(), 590, 300, 430, 348);
        A(new SectionLabelItem(), 60, 672, 240, 44);
        A(new FlowItem(), 60, 724, 960, 150);
        A(new CodeItem(), 60, 892, 560, 248);
        A(new NotesItem(), 640, 892, 380, 188);
        A(new ReferenceItem(), 640, 1096, 380, 118);
        A(new BenefitsItem(), 60, 1236, 960, 96);
    }
}
