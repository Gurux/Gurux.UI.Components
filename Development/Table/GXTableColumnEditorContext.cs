using Gurux.UI.Components.Enums;

namespace Gurux.UI.Components.Table;

/// <summary>A read-only snapshot of an open column editor draft and commands for editing it.</summary>
public sealed class GXTableColumnEditorContext
{
    internal GXTableColumnEditorContext() { }

    /// <summary>Available columns in the draft's current display order.</summary>
    public IReadOnlyList<GXTableColumn> Columns { get; internal init; } = [];
    /// <summary>Stable column identifiers in the draft's display order.</summary>
    public IReadOnlyList<string> ColumnOrder { get; internal init; } = [];
    /// <summary>Identifiers of the selected columns, including an empty selection.</summary>
    public IReadOnlyList<string> VisibleColumns { get; internal init; } = [];
    /// <summary>Effective responsive and print visibility for every available column.</summary>
    public IReadOnlyDictionary<string, Visibility> Visibility { get; internal init; } = new Dictionary<string, Visibility>();
    /// <summary>Saved pixel widths, or an empty dictionary when Reset is staged.</summary>
    public IReadOnlyDictionary<string, double> Widths { get; internal init; } = new Dictionary<string, double>();
    /// <summary>Whether the table is saving the draft.</summary>
    public bool IsSaving { get; internal init; }
    /// <summary>The latest save error displayed by the editor, if any.</summary>
    public string? Error { get; internal init; }

    internal Func<string, bool, Task> SetVisible { get; init; } = default!;
    internal Func<string, Visibility, Task> SetVisibility { get; init; } = default!;
    internal Func<string, int, Task> Move { get; init; } = default!;
    internal Func<Task> Reset { get; init; } = default!;
    internal Func<Task> Save { get; init; } = default!;
    internal Func<Task> Cancel { get; init; } = default!;

    /// <summary>Selects or hides a column in this draft without committing it.</summary>
    public Task SetVisibleAsync(string name, bool visible) => SetVisible(name, visible);
    /// <summary>Changes a column's responsive or print visibility in this draft.</summary>
    public Task SetVisibilityAsync(string name, Visibility visibility) => SetVisibility(name, visibility);
    /// <summary>Moves a column to a zero-based position in this draft.</summary>
    public Task MoveAsync(string name, int index) => Move(name, index);
    /// <summary>Stages a reset to the configured defaults.</summary>
    public Task ResetAsync() => Reset();
    /// <summary>Commits this draft using the table's standard save behavior.</summary>
    public Task SaveAsync() => Save();
    /// <summary>Discards this draft and closes the editor.</summary>
    public Task CancelAsync() => Cancel();
}
