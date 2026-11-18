// --------------------------------------------------------------------------
// Gurux Ltd
//
// Copyright (c) Gurux Ltd.
//
// This file is part of Gurux Device Framework.
//
// Commit: $Format:%H$
// Date:   $Format:%cI$
//
// This software is available under either of the following licenses:
//
// 1. GNU General Public License, version 2 only (GPL-2.0-only).
//    See the LICENSE file included with this distribution.
//
// 2. A commercial license granted by Gurux Ltd under a separate written
//    agreement. Use under this option requires a valid commercial license
//    and is governed by the terms of that agreement.
//
// If you have not obtained a commercial license from Gurux Ltd,
// use of this software is governed by GPL-2.0-only.
//
// Commercial licensing: https://www.gurux.fi/
//
// SPDX-License-Identifier: GPL-2.0-only OR LicenseRef-Gurux-Commercial
// --------------------------------------------------------------------------

using Gurux.UI.Components.Enums;
using Microsoft.AspNetCore.Components.Forms;

namespace Gurux.UI.Components;

/// <summary>Maintains separate command ownership, form tracking and navigation state per menu address.</summary>
public sealed class GXMenuService : IGXMenuService, IDisposable
{
    /// <summary>Stores independently owned menus by layout region, menu kind, and optional identifier.</summary>
    private readonly Dictionary<(MenuLocation, MenuKind, Guid?), GXMenuState> _menus = new();
    /// <summary>Records whether this owner has released its resources and stopped accepting updates.</summary>
    private bool _disposed;
    /// <inheritdoc />
    public event Action? Changed;
    /// <inheritdoc />
    public IReadOnlyList<GXMenuItem> Items => GetItems();
    /// <inheritdoc />
    public EditContext? EditContext { get => GetEditContext(); set => SetEditContext(value); }
    /// <inheritdoc />
    public IReadOnlyList<GXMenuItem> GetItems(MenuLocation location = MenuLocation.Top, MenuKind kind = MenuKind.MenuBar, Guid? menuId = null)
        => Get(location, kind, menuId).Items;
    /// <inheritdoc />
    public EditContext? GetEditContext(MenuLocation location = MenuLocation.Top, MenuKind kind = MenuKind.MenuBar, Guid? menuId = null)
        => Get(location, kind, menuId).EditContext;
    /// <inheritdoc />
    public void SetEditContext(EditContext? context, MenuLocation location = MenuLocation.Top, MenuKind kind = MenuKind.MenuBar, Guid? menuId = null)
        => Get(location, kind, menuId).EditContext = context;
    /// <inheritdoc />
    public void AddMenuItems(params IEnumerable<GXMenuItem> items) => AddMenuItems(MenuLocation.Top, MenuKind.MenuBar, items);
    /// <inheritdoc />
    public void AddMenuItems(MenuLocation location, MenuKind kind, params IEnumerable<GXMenuItem> items)
        => AddMenuItems(location, kind, null, items);
    /// <inheritdoc />
    public void AddMenuItems(MenuLocation location, MenuKind kind, Guid? menuId, params IEnumerable<GXMenuItem> items)
        => Get(location, kind, menuId).AddMenuItems(items);
    /// <inheritdoc />
    public IDisposable RegisterMenu(IEnumerable<GXMenuItem> items, EditContext? editContext = null,
        MenuLocation location = MenuLocation.Top, MenuKind kind = MenuKind.MenuBar, Guid? menuId = null)
        => Get(location, kind, menuId).RegisterMenu(items, editContext);
    /// <inheritdoc />
    public void Clear(MenuLocation location = MenuLocation.Top, MenuKind kind = MenuKind.MenuBar, Guid? menuId = null) => Get(location, kind, menuId).Clear();
    /// <inheritdoc />
    public Action ClearForNavigation(MenuLocation location, MenuKind kind, Guid? menuId = null) => Get(location, kind, menuId).ClearForNavigation();
    /// <inheritdoc />
    public Action ClearForNavigation()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var restore = _menus.Values.ToArray().Select(menu => menu.ClearForNavigation()).ToArray();
        return () => { foreach (var action in restore)
        {
            action();
        } };
    }
    /// <summary>Finds or creates the state for a valid menu address, rejecting access after disposal.</summary>
    /// <param name="location">The application layout region containing the menu.</param>
    /// <param name="kind">The category of menu commands to address.</param>
    /// <param name="menuId">The optional identifier distinguishing menus at the same location and category.</param>
    /// <returns>The independent state owned by the specified menu address.</returns>
    private GXMenuState Get(MenuLocation location, MenuKind kind, Guid? menuId)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (!Enum.IsDefined(location))
        {
            throw new ArgumentOutOfRangeException(nameof(location));
        }
        if (!Enum.IsDefined(kind) || kind == MenuKind.All)
        {
            throw new ArgumentOutOfRangeException(nameof(kind));
        }
        var key = (location, kind, menuId);
        if (!_menus.TryGetValue(key, out var menu))
        {
            menu = new GXMenuState();
            menu.Changed += Notify;
            _menus.Add(key, menu);
        }
        return menu;
    }
    /// <summary>Notifies subscribers that menu state has changed.</summary>
    private void Notify() => Changed?.Invoke();
    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        _disposed = true;
        foreach (var menu in _menus.Values)
        {
            menu.Dispose();
        }
        _menus.Clear();
        Changed = null;
    }
}
