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
    /// Provides asynchronous access to individual and grouped browser local storage entries.
    /// </summary>
    public interface IGXLocalStorage
    {
        /// <summary>
        /// Writes a value to browser local storage, removing the entry when the value is null.
        /// </summary>
        /// <param name="key">Key.</param>
        /// <param name="value">Value</param>
        Task SetValueAsync(
            string key,
            string? value);

        /// <summary>
        /// Writes a value to browser local storage, removing the entry when the value is null.
        /// </summary>
        /// <param name="group">Key group.</param>
        /// <param name="key">Key.</param>
        /// <param name="value">Value</param>
        Task SetValueAsync(
            string group,
            string key,
            string? value);

        /// <summary>
        /// Reads a value from browser local storage, returning null when no value is available.
        /// </summary>
        /// <param name="key">Key</param>
        /// <returns>Value from local storage.</returns>
        Task<string?> GetValueAsync(
            string key);

        /// <summary>
        /// Reads a value from browser local storage, returning null when no value is available.
        /// </summary>
        /// <param name="group">Key group.</param>
        /// <param name="key">Key</param>
        /// <returns>Value from local storage.</returns>
        Task<string?> GetValueAsync(
            string group,
            string key);

        /// <summary>
        /// Removes the browser local storage entry identified by the key and optional group.
        /// </summary>
        /// <param name="key">Key</param>
        Task RemoveAsync(string key);

        /// <summary>
        /// Removes the browser local storage entry identified by the key and optional group.
        /// </summary>
        /// <param name="group">Key group.</param>
        /// <param name="key">Key</param>
        Task RemoveAsync(string group, string key);

        /// <summary>
        /// Clears all browser local storage entries or only entries belonging to the specified group.
        /// </summary>
        /// <param name="group">Key group.</param>
        Task ClearAsync(string group);

        /// <summary>
        /// Clears all browser local storage entries or only entries belonging to the specified group.
        /// </summary>
        Task ClearAsync();

        /// <summary>
        /// Retrieves browser local storage keys beginning with the specified group prefix.
        /// </summary>
        /// <param name="group">The key prefix identifying the local storage group.</param>
        /// <returns>The full keys in the group, or null when grouped browser storage is unavailable.</returns>
        Task<IEnumerable<string>?> GetGroupValuesAsync(string group);
    }
}
