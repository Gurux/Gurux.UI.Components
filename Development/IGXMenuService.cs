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

/// <summary>Owns independent command menus addressed by layout region, menu kind and optional GXMenu ID.</summary>
public interface IGXMenuService
{
    event Action? Changed;
    /// <summary>Default Top/MenuBar commands.</summary>
    IReadOnlyList<GXMenuItem> Items { get; }
    /// <summary>Default Top/MenuBar form.</summary>
    EditContext? EditContext { get; set; }
    IReadOnlyList<GXMenuItem> GetItems(MenuLocation location = MenuLocation.Top, MenuKind kind = MenuKind.MenuBar, Guid? menuId = null);
    EditContext? GetEditContext(MenuLocation location = MenuLocation.Top, MenuKind kind = MenuKind.MenuBar, Guid? menuId = null);
    void SetEditContext(EditContext? context, MenuLocation location = MenuLocation.Top, MenuKind kind = MenuKind.MenuBar, Guid? menuId = null);
    void AddMenuItems(params IEnumerable<GXMenuItem> menus);
    void AddMenuItems(MenuLocation location, MenuKind kind, params IEnumerable<GXMenuItem> menus);
    void AddMenuItems(MenuLocation location, MenuKind kind, Guid? menuId, params IEnumerable<GXMenuItem> menus);
    /// <summary>Replaces one menu. Disposing an older owner never clears its replacement.</summary>
    IDisposable RegisterMenu(IEnumerable<GXMenuItem> menus, EditContext? editContext = null,
        MenuLocation location = MenuLocation.Top, MenuKind kind = MenuKind.MenuBar, Guid? menuId = null);
    void Clear(MenuLocation location = MenuLocation.Top, MenuKind kind = MenuKind.MenuBar, Guid? menuId = null);
    /// <summary>Clears every known menu and returns an idempotent restore action for canceled navigation.</summary>
    Action ClearForNavigation();
    /// <summary>Clears one menu and restores it only if no newer owner or change replaced it.</summary>
    Action ClearForNavigation(MenuLocation location, MenuKind kind, Guid? menuId = null);
}
