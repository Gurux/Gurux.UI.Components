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

namespace Gurux.UI.Components.Table
{
    /// <summary>
    /// Exposes table sorting, editing, selection, and column visibility to child components.
    /// </summary>
    public interface IGXTable
    {
        /// <summary>
        /// Gets the identifier of the table.
        /// </summary>
        string? Id
        {
            get;
        }

        /// <summary>
        /// Gets or sets the property name used to sort items.
        /// </summary>
        string? OrderBy { get; set; }
        /// <summary>
        /// Gets or sets the direction used to sort the table.
        /// </summary>
        SortMode SortMode { get; set; }

        /// <summary>
        /// Refreshes the table after its sort column or direction changes.
        /// </summary>
        Task NotificationShortChange();

        /// <summary>
        /// Sets the active table row and invokes the row change callback.
        /// </summary>
        /// <param name="selected">Selected row.</param>
        Task SelectRow(object selected);

        /// <summary>
        /// Invokes the callback for the selected table cell.
        /// </summary>
        /// <param name="selected">Selected cell.</param>
        Task SelectCell(object selected);

        /// <summary>
        /// Gets whether table cells can render editable content.
        /// </summary>
        bool CanEdit
        {
            get;
        }

        /// <summary>
        /// Determines whether the named table column is excluded from the configured visible columns.
        /// </summary>
        /// <param name="name">Column name.</param>
        /// <returns>True, if column is hidden.</returns>
        bool IsHidden(string? name);

        /// <summary>Returns the persisted CSS width for the column, if available.</summary>
        string? GetColumnWidth(string? name) => null;

        /// <summary>Registers a named header for automatic column customization.</summary>
        void RegisterColumn(string name, Microsoft.AspNetCore.Components.RenderFragment? content, Visibility visibility) { }

        /// <summary>Removes a header from automatic column customization.</summary>
        void UnregisterColumn(string name) { }

        /// <summary>Returns the customized visibility or the component's default.</summary>
        Visibility GetColumnVisibility(string? name, Visibility defaultValue) => defaultValue;
    }
}
