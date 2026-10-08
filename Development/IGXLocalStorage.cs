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
