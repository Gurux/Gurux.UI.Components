using Microsoft.AspNetCore.Components.Forms;

namespace Gurux.UI.Components;

/// <summary>
/// Maintains one addressed menu's commands and tracks changes to the active form's edit state.
/// </summary>
internal sealed class GXMenuState : IDisposable
{
    private readonly List<GXMenuItem> _items = new();
    private EditContext? _editContext;
    private Registration? _registration;
    private long _version;
    private bool _disposed;
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
            SetEditContext(value);
            ++_version;
            Changed?.Invoke();
        }
    }

    /// <summary>
    /// Adds the supplied commands to the addressed menu and notifies listeners.
    /// </summary>
    public void AddMenuItems(params IEnumerable<GXMenuItem> menus)
    {
        _registration = null;
        _items.AddRange(menus);
        ++_version;
        Changed?.Invoke();
    }

    /// <inheritdoc />
    public IDisposable RegisterMenu(IEnumerable<GXMenuItem> menus, EditContext? editContext = null)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ArgumentNullException.ThrowIfNull(menus);
        var commands = menus.ToArray();
        var registration = new Registration(this);
        _registration = registration;
        _items.Clear();
        _items.AddRange(commands);
        SetEditContext(editContext);
        ++_version;
        Changed?.Invoke();
        return registration;
    }

    /// <inheritdoc />
    public Action ClearForNavigation()
    {
        // A second renderer for the same address must not invalidate the first
        // renderer's pending navigation snapshot by clearing an empty menu again.
        if (_items.Count == 0 && _editContext == null && _registration == null)
        {
            return static () => { };
        }
        var commands = _items.ToArray();
        var editContext = _editContext;
        var registration = _registration;
        Clear();
        var version = _version;
        return () =>
        {
            if (_disposed || _version != version || registration?.IsDisposed == true)
            {
                return;
            }
            _registration = registration;
            _items.AddRange(commands);
            SetEditContext(editContext);
            ++_version;
            Changed?.Invoke();
        };
    }

    /// <summary>
    /// Clears menu commands, detaches the active edit context, and notifies listeners.
    /// </summary>
    public void Clear()
    {
        _registration = null;
        SetEditContext(null);
        _items.Clear();
        ++_version;
        Changed?.Invoke();
    }

    private void SetEditContext(EditContext? context)
    {
        if (_editContext is not null)
        {
            _editContext.OnFieldChanged -= OnFieldChanged;
        }
        _editContext = context;
        if (_editContext is not null)
        {
            _editContext.OnFieldChanged += OnFieldChanged;
        }
    }

    private void Release(Registration registration)
    {
        if (!_disposed && ReferenceEquals(_registration, registration))
        {
            Clear();
        }
    }

    private sealed class Registration(GXMenuState service) : IDisposable
    {
        private GXMenuState? _service = service;
        public bool IsDisposed => _service == null;

        public void Dispose() => Interlocked.Exchange(ref _service, null)?.Release(this);
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
        _disposed = true;
        _registration = null;
        SetEditContext(null);
        _items.Clear();
        Changed = null;
    }
}
