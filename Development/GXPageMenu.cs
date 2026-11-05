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

using Microsoft.AspNetCore.Components.Forms;
using Gurux.UI.Components.Enums;

namespace Gurux.UI.Components;

/// <summary>
/// Collects a page's commands under one ownership registration and ignores updates after disposal.
/// </summary>
public sealed class GXPageMenu(IGXMenuService menu, MenuLocation location = MenuLocation.Top, MenuKind kind = MenuKind.MenuBar, Guid? menuId = null) : IDisposable
{
    private readonly List<GXMenuItem> _commands = [];
    private IDisposable? _registration;
    private EditContext? _editContext;
    private bool _disposed;

    /// <summary>
    /// Gets or sets the form associated with this page's commands.
    /// </summary>
    public EditContext? EditContext
    {
        get => _editContext;
        set
        {
            if (_disposed)
            {
                return;
            }
            _editContext = value;
            if (_commands.Count != 0)
            {
                Publish();
            }
        }
    }

    /// <summary>
    /// Adds commands to this page and publishes the complete owned menu.
    /// </summary>
    /// <param name="menus">Commands to add.</param>
    public void AddMenuItems(params IEnumerable<GXMenuItem> menus)
    {
        if (_disposed)
        {
            return;
        }
        _commands.AddRange(menus);
        Publish();
    }

    /// <summary>
    /// Clears this page's commands without changing another page's menu.
    /// </summary>
    public void Clear()
    {
        _commands.Clear();
        _editContext = null;
        _registration?.Dispose();
        _registration = null;
    }

    private void Publish()
    {
        var previous = _registration;
        _registration = menu.RegisterMenu(_commands, _editContext, location, kind, menuId);
        previous?.Dispose();
    }

    /// <summary>Releases this page's registration and prevents late callbacks from changing the menu.</summary>
    public void Dispose()
    {
        _disposed = true;
        Clear();
    }
}
