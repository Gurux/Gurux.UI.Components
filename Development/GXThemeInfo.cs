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
