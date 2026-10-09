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
    /// Provides shared top menu commands and the edit context used to determine their enabled state.
    /// </summary>
    public interface IGXTopMenu
    {
        /// <summary>
        /// Occurs when menu commands or their enabled state need to be refreshed.
        /// </summary>
        event Action? Changed;

        /// <summary>
        /// Gets the commands registered with the shared menu service.
        /// </summary>
        IReadOnlyList<GXMenuItem> Items
        {
            get;
        }

        /// <summary>
        /// Adds the supplied commands to the shared top menu and notifies listeners.
        /// </summary>
        /// <param name="menus">Menu items to add</param>
        void AddMenuItems(params IEnumerable<GXMenuItem> menus);

        /// <summary>
        /// Clears menu commands and their associated edit context.
        /// </summary>
        void Clear();

        /// <summary>
        /// Gets or sets the edit context used to track form changes and validation.
        /// </summary>
        EditContext? EditContext { get; set; }
    }
}
