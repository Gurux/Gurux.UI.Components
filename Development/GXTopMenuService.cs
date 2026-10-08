using Microsoft.AspNetCore.Components.Forms;

namespace Gurux.UI.Components;

/// <summary>
/// Maintains shared top menu commands and tracks changes to the active form's edit state.
/// </summary>
public sealed class GXTopMenuService : IGXTopMenu, IDisposable
{
    private readonly List<GXMenuItem> _items = new();
    private EditContext? _editContext;
    /// <summary>
    /// Occurs when commands or the tracked form edit state change.
    /// </summary>
    public event Action? Changed;
    /// <summary>
    /// Gets the commands registered with the shared menu service.
    /// </summary>
    public IReadOnlyList<GXMenuItem> Items => _items.AsReadOnly();

    /// <summary>
    /// Gets or sets the tracked edit context and notifies menu consumers when its fields change.
    /// </summary>
    public EditContext? EditContext
    {
        get => _editContext;
        set
        {
            if (ReferenceEquals(_editContext, value))
            {
                return;
            }
            if (_editContext is not null)
            {
                _editContext.OnFieldChanged -= OnFieldChanged;
            }
            _editContext = value;
            if (_editContext is not null)
            {
                _editContext.OnFieldChanged += OnFieldChanged;
            }
            Changed?.Invoke();
        }
    }

    /// <summary>
    /// Adds the supplied commands to the shared top menu and notifies listeners.
    /// </summary>
    public void AddMenuItems(params IEnumerable<GXMenuItem> menus)
    {
        _items.AddRange(menus);
        Changed?.Invoke();
    }

    /// <summary>
    /// Clears menu commands, detaches the active edit context, and notifies listeners.
    /// </summary>
    public void Clear()
    {
        if (_editContext is not null)
        {
            _editContext.OnFieldChanged -= OnFieldChanged;
        }
        _editContext = null;
        _items.Clear();
        Changed?.Invoke();
    }

    /// <summary>
    /// Notifies menu consumers that the edit context's modified state may have changed.
    /// </summary>
    private void OnFieldChanged(object? sender, FieldChangedEventArgs args) => Changed?.Invoke();

    /// <summary>
    /// Detaches the edit context handler and clears stored commands and subscribers.
    /// </summary>
    public void Dispose()
    {
        if (_editContext is not null)
        {
            _editContext.OnFieldChanged -= OnFieldChanged;
        }
        _editContext = null;
        _items.Clear();
        Changed = null;
    }
}
