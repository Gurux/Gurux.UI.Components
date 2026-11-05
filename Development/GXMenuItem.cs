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
        /// Commands displayed in a dropdown instead of invoking this item's click callback.
        /// </summary>
        public List<GXMenuItem> Children { get; set; } = new();

        /// <summary>Displays a separator instead of an actionable command.</summary>
        public bool IsSeparator { get; set; }

        /// <summary>Gets or sets the authorization policy required to display and execute this command.</summary>
        public string? Policy { get; set; }

        /// <summary>
        /// Stores the callback invoked when the menu command is clicked.
        /// </summary>
        public EventCallback OnClick;

        /// <summary>
        /// Gets or sets the form edit state required for the menu command to be enabled.
        /// </summary>
        public EnableStyle? Enabled { get; set; } = EnableStyle.Always;

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
