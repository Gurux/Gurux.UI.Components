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

namespace Gurux.UI.Components
{
    /// <summary>
    /// This interface is used to listen and notification events.
    /// </summary>
    public interface IGXTopMenu
    {
        /// <summary>Raised when menu items or the edit state change.</summary>
        event Action? Changed;

        /// <summary>Current menu commands.</summary>
        IReadOnlyList<GXMenuItem> Items { get; }

        /// <summary>
        /// Add new menu items.
        /// </summary>
        /// <param name="menus">Menu items to add</param>
        void AddMenuItems(params IEnumerable<GXMenuItem> menus);

        /// <summary>
        /// Clear menu items.
        /// </summary>
        void Clear();

        /// <summary>
        /// EditContext is used to show when user edit the page content.
        /// </summary>
        EditContext? EditContext
        {
            get;
            set;
        }
    }
}
