//
// --------------------------------------------------------------------------
//  Gurux Ltd
//
//
//
// Filename:        $HeadURL$
//
// Version:         $Revision$,
//                  $Date$
//                  $Author$
//
// Copyright (c) Gurux Ltd
//
//---------------------------------------------------------------------------
//
//  DESCRIPTION
//
// This file is a part of Gurux Device Framework.
//
// Gurux Device Framework is Open Source software; you can redistribute it
// and/or modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; version 2 of the License.
// Gurux Device Framework is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU General Public License for more details.
//
// This code is licensed under the GNU General Public License v2.
// Full text may be retrieved at http://www.gnu.org/licenses/gpl-2.0.txt
//---------------------------------------------------------------------------

using Microsoft.AspNetCore.Components.Forms;

namespace Gurux.UI.Components;

/// <summary>
/// Collects a page's commands under one ownership registration and ignores updates after disposal.
/// </summary>
public sealed class GXPageMenu(IGXTopMenu menu) : IDisposable
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
        _registration = menu.RegisterMenu(_commands, _editContext);
        previous?.Dispose();
    }

    /// <summary>Releases this page's registration and prevents late callbacks from changing the menu.</summary>
    public void Dispose()
    {
        _disposed = true;
        Clear();
    }
}
