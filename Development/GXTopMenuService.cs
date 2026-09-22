using Microsoft.AspNetCore.Components.Forms;

namespace Gurux.UI.Components;

/// <summary>Shared menu commands and edit state for one application scope.</summary>
public sealed class GXTopMenuService : IGXTopMenu, IDisposable
{
    private readonly List<GXMenuItem> _items = new();
    private EditContext? _editContext;
    public event Action? Changed;
    public IReadOnlyList<GXMenuItem> Items => _items.AsReadOnly();

    public EditContext? EditContext
    {
        get => _editContext;
        set
        {
            if (ReferenceEquals(_editContext, value)) return;
            if (_editContext is not null) _editContext.OnFieldChanged -= OnFieldChanged;
            _editContext = value;
            if (_editContext is not null) _editContext.OnFieldChanged += OnFieldChanged;
            Changed?.Invoke();
        }
    }

    public void AddMenuItems(params IEnumerable<GXMenuItem> menus)
    {
        _items.AddRange(menus);
        Changed?.Invoke();
    }

    public void Clear()
    {
        if (_editContext is not null) _editContext.OnFieldChanged -= OnFieldChanged;
        _editContext = null;
        _items.Clear();
        Changed?.Invoke();
    }

    private void OnFieldChanged(object? sender, FieldChangedEventArgs args) => Changed?.Invoke();

    public void Dispose()
    {
        if (_editContext is not null) _editContext.OnFieldChanged -= OnFieldChanged;
        _editContext = null;
        _items.Clear();
        Changed = null;
    }
}
