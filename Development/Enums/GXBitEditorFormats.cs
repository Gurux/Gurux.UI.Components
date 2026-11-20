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


namespace Gurux.UI.Components.Enums;

/// <summary>Selects the value representations available below a bit editor's grid.</summary>
[Flags]
public enum GXBitEditorFormats
{
    /// <summary>Hides the bit grid, toolbar and value representation fields.</summary>
    None = 0,
    /// <summary>Displays the bit grid and its editing toolbar.</summary>
    Editor = 1,
    /// <summary>Displays the binary representation.</summary>
    Binary = 2,
    /// <summary>Displays the hexadecimal representation.</summary>
    Hexadecimal = 4,
    /// <summary>Displays the decimal representation for numeric values.</summary>
    Decimal = 8,
    /// <summary>Displays the decoded text representation in text mode.</summary>
    Text = 16,
    /// <summary>Makes every representation and the bit editor available.</summary>
    All = Binary | Hexadecimal | Decimal | Text | Editor
}