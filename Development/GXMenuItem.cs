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
using Microsoft.AspNetCore.Components;

namespace Gurux.UI.Components
{
    /// <summary>
    /// Describes a top menu command, its icon, and the edit state in which it is enabled.
    /// </summary>
    public class GXMenuItem
    {
        /// <summary>
        /// Gets or sets the label displayed for the top menu command.
        /// </summary>
        public string Text { get; set; } = "";

        /// <summary>
        /// Gets or sets the icon displayed for the top menu command.
        /// </summary>
        public string? Icon { get; set; }

        /// <summary>
        /// Stores the callback invoked when the menu command is clicked.
        /// </summary>
        public EventCallback OnClick;

        /// <summary>
        /// Gets or sets the form edit state required for the menu command to be enabled.
        /// </summary>
        public EnableStyle? Enabled
        {
            get;
            set;
        } = EnableStyle.Always;

        /// <summary>
        /// Creates an empty menu command that is enabled regardless of form edit state.
        /// </summary>
        public GXMenuItem()
        {
        }


        /// <summary>
        /// Creates a menu command with its label, icon, click callback, and required edit state.
        /// </summary>
        /// <param name="text">Menu text.</param>
        /// <param name="icon">Menu icon.</param>
        /// <param name="onClick">Menu action.</param>
        /// <param name="enabled">The form edit state in which the command is enabled.</param>
        public GXMenuItem(string text, string? icon, EventCallback onClick, EnableStyle enabled = EnableStyle.Always)
        {
            Text = text;
            Icon = icon;
            OnClick = onClick;
            Enabled = enabled;
        }
    }
}
