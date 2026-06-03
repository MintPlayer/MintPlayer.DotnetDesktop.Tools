namespace MintPlayer.ThreeDee.Commands;

/// <summary>Undo/redo stack (PRD §5.7). Executing a new command clears the redo stack.</summary>
public sealed class History
{
    readonly Stack<ICommand> _undo = new();
    readonly Stack<ICommand> _redo = new();

    public bool CanUndo => _undo.Count > 0;
    public bool CanRedo => _redo.Count > 0;
    public string? NextUndo => _undo.TryPeek(out var c) ? c.Name : null;
    public string? NextRedo => _redo.TryPeek(out var c) ? c.Name : null;

    /// <summary>Raised after any change to the stacks.</summary>
    public event Action? Changed;

    public void Clear() { _undo.Clear(); _redo.Clear(); Changed?.Invoke(); }

    public void Execute(ICommand command)
    {
        command.Do();
        _undo.Push(command);
        _redo.Clear();
        Changed?.Invoke();
    }

    public void Undo()
    {
        if (_undo.Count == 0) return;
        var c = _undo.Pop();
        c.Undo();
        _redo.Push(c);
        Changed?.Invoke();
    }

    public void Redo()
    {
        if (_redo.Count == 0) return;
        var c = _redo.Pop();
        c.Do();
        _undo.Push(c);
        Changed?.Invoke();
    }
}
