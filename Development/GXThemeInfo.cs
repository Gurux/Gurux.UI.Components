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

namespace Gurux.UI.Components
{

    /// <summary>
    /// Describes a configurable theme style, its value, and its value type.
    /// </summary>
    public class GXThemeStyle
    {
        /// <summary>
        /// Gets or sets the name identifying the theme or its configurable style.
        /// </summary>
        public string Name { get; set; } = default!;
        /// <summary>
        /// Gets or sets the explanatory text for the configurable theme style.
        /// </summary>
        public string? Description { get; set; }
        /// <summary>
        /// Gets or sets the configured value of the theme style.
        /// </summary>
        public string? Value { get; set; }

        /// <summary>
        /// Gets or sets the data type of the configurable theme style.
        /// </summary>
        public byte Type { get; set; }

        /// <summary>
        /// Creates an unconfigured theme style.
        /// </summary>
        public GXThemeStyle()
        {

        }

        /// <summary>
        /// Creates a theme style with its type, name, description, and configured value.
        /// </summary>
        /// <param name="type">Theme type.</param>
        /// <param name="name">Name</param>
        /// <param name="description">Description</param>
        /// <param name="value">Value</param>
        public GXThemeStyle(byte type, string name, string? description, string? value)
        {
            Type = type;
            Name = name;
            Description = description;
            Value = value;
        }
    }

    /// <summary>
    /// Describes a theme's name, stylesheet, and configurable styles.
    /// </summary>
    public class GXThemeInfo
    {
        /// <summary>
        /// Gets or sets the name identifying the theme or its configurable style.
        /// </summary>
        public string Name { get; set; } = default!;
        /// <summary>
        /// Gets or sets the path to the theme's stylesheet.
        /// </summary>
        public string CssFile { get; set; } = default!;
        /// <summary>
        /// Gets or sets the theme's configurable style values.
        /// </summary>
        public List<GXThemeStyle> Styles { get; set; } = new List<GXThemeStyle>();
    }
}
