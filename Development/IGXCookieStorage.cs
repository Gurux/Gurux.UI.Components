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
    /// Provides asynchronous access to browser cookies with configurable expiration.
    /// </summary>
    public interface IGXCookieStorage
    {
        /// <summary>
        /// Writes a browser cookie with the specified value and expiration in days.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="value">Value</param>
        /// <param name="days">Expiration days.</param>
        Task SetValueAsync(
            string key,
            string? value,
            int days = 30);

        /// <summary>
        /// Reads a browser cookie, returning the supplied default when the key is not found.
        /// </summary>
        /// <param name="key">Key</param>
        /// <param name="def">Default value.</param>
        /// <returns>Cookie value.</returns>
        Task<string?> GetValueAsync(
            string key,
            string? def = null);
    }
}