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
        string? Id { get; }

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
        bool CanEdit { get; }

        /// <summary>
        /// Determines whether the named table column is excluded from the configured visible columns.
        /// </summary>
        /// <param name="name">Column name.</param>
        /// <returns>True, if column is hidden.</returns>
        bool IsHidden(string? name);
    }
}