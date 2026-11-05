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

/// <summary>
/// Specifies optional notification fields to display; flags can be combined.
/// </summary>
[Flags]
public enum NotificationFields
{
    /// <summary>
    /// Displays no optional notification fields.
    /// </summary>
    None = 0,
    /// <summary>
    /// Displays the notification title.
    /// </summary>
    Title = 1,
    /// <summary>
    /// Displays the notification creation time.
    /// </summary>
    CreatedAt = 2,
    /// <summary>
    /// Displays the notification identifier.
    /// </summary>
    Id = 4,
    /// <summary>
    /// Displays every optional notification field.
    /// </summary>
    All = Title | CreatedAt | Id
}