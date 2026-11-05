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

namespace Gurux.UI.Components.Enums
{
    /// <summary>
    /// Defines responsive breakpoint and print visibility options for table cells.
    /// </summary>
    public enum Visibility
    {
        /// <summary>
        /// Displays the cell at every screen size.
        /// </summary>
        All,
        /// <summary>
        /// Displays the cell only at the largest configured responsive breakpoint.
        /// </summary>
        ExtraLargeLarge,
        /// <summary>
        /// Displays the cell at extra-large screen sizes and above.
        /// </summary>
        ExtraLarge,
        /// <summary>
        /// Displays the cell at large screen sizes and above.
        /// </summary>
        Large,
        /// <summary>
        /// Displays the cell at medium screen sizes and above.
        /// </summary>
        Medium,
        /// <summary>
        /// Displays the cell at small screen sizes and above.
        /// </summary>
        Small,
        /// <summary>
        /// Displays the cell at extra-small screen sizes and above.
        /// </summary>
        ExtraSmall,
        /// <summary>
        /// Shows the cell in printed output.
        /// </summary>
        Print,
        /// <summary>
        /// Hides the cell in printed output.
        /// </summary>
        PrintHide
    }
}
