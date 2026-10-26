using Gurux.UI.Components.Enums;
using Microsoft.AspNetCore.Components.Forms;

namespace Gurux.UI.Components;

/// <summary>Maintains separate command ownership, form tracking and navigation state per menu address.</summary>
public sealed class GXMenuService : IGXMenuService, IDisposable
{
    private readonly Dictionary<(MenuLocation, MenuKind, Guid?), GXMenuState> menus = new();
    private bool disposed;
    public event Action? Changed;
    public IReadOnlyList<GXMenuItem> Items => GetItems();
    public EditContext? EditContext { get => GetEditContext(); set => SetEditContext(value); }
    public IReadOnlyList<GXMenuItem> GetItems(MenuLocation location = MenuLocation.Top, MenuKind kind = MenuKind.MenuBar, Guid? menuId = null)
        => Get(location, kind, menuId).Items;
    public EditContext? GetEditContext(MenuLocation location = MenuLocation.Top, MenuKind kind = MenuKind.MenuBar, Guid? menuId = null)
        => Get(location, kind, menuId).EditContext;
    public void SetEditContext(EditContext? context, MenuLocation location = MenuLocation.Top, MenuKind kind = MenuKind.MenuBar, Guid? menuId = null)
        => Get(location, kind, menuId).EditContext = context;
    public void AddMenuItems(params IEnumerable<GXMenuItem> items) => AddMenuItems(MenuLocation.Top, MenuKind.MenuBar, items);
    public void AddMenuItems(MenuLocation location, MenuKind kind, params IEnumerable<GXMenuItem> items)
        => AddMenuItems(location, kind, null, items);
    public void AddMenuItems(MenuLocation location, MenuKind kind, Guid? menuId, params IEnumerable<GXMenuItem> items)
        => Get(location, kind, menuId).AddMenuItems(items);
    public IDisposable RegisterMenu(IEnumerable<GXMenuItem> items, EditContext? editContext = null,
        MenuLocation location = MenuLocation.Top, MenuKind kind = MenuKind.MenuBar, Guid? menuId = null)
        => Get(location, kind, menuId).RegisterMenu(items, editContext);
    public void Clear(MenuLocation location = MenuLocation.Top, MenuKind kind = MenuKind.MenuBar, Guid? menuId = null) => Get(location, kind, menuId).Clear();
    public Action ClearForNavigation(MenuLocation location, MenuKind kind, Guid? menuId = null) => Get(location, kind, menuId).ClearForNavigation();
    public Action ClearForNavigation()
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        var restore = menus.Values.ToArray().Select(menu => menu.ClearForNavigation()).ToArray();
        return () => { foreach (var action in restore) action(); };
    }
    private GXMenuState Get(MenuLocation location, MenuKind kind, Guid? menuId)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        if (!Enum.IsDefined(location)) throw new ArgumentOutOfRangeException(nameof(location));
        if (!Enum.IsDefined(kind) || kind == MenuKind.All) throw new ArgumentOutOfRangeException(nameof(kind));
        var key = (location, kind, menuId);
        if (!menus.TryGetValue(key, out var menu))
        {
            menu = new GXMenuState();
            menu.Changed += Notify;
            menus.Add(key, menu);
        }
        return menu;
    }
    private void Notify() => Changed?.Invoke();
    public void Dispose()
    {
        if (disposed) return;
        disposed = true;
        foreach (var menu in menus.Values) menu.Dispose();
        menus.Clear();
        Changed = null;
    }
}
