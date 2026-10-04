using LinkedInStudio.Models;

namespace LinkedInStudio.Services;

/// <summary>Snapshot-based undo/redo. Baseline holds the last committed serialized document.</summary>
public sealed class History
{
    private readonly Stack<string> _undo = new();
    private readonly Stack<string> _redo = new();
    private string _baseline = "";

    public bool CanUndo => _undo.Count > 0;
    public bool CanRedo => _redo.Count > 0;

    public void Reset(DocumentModel doc)
    {
        _undo.Clear();
        _redo.Clear();
        _baseline = DocumentStore.Serialize(doc);
    }

    /// <summary>Call after a change completes. Pushes the previous baseline if the state actually changed.</summary>
    public void Commit(DocumentModel doc)
    {
        var now = DocumentStore.Serialize(doc);
        if (now == _baseline) return;
        _undo.Push(_baseline);
        _redo.Clear();
        _baseline = now;
    }

    public DocumentModel? Undo(DocumentModel current)
    {
        if (_undo.Count == 0) return null;
        _redo.Push(DocumentStore.Serialize(current));
        _baseline = _undo.Pop();
        return DocumentStore.Deserialize(_baseline);
    }

    public DocumentModel? Redo(DocumentModel current)
    {
        if (_redo.Count == 0) return null;
        _undo.Push(DocumentStore.Serialize(current));
        _baseline = _redo.Pop();
        return DocumentStore.Deserialize(_baseline);
    }
}
