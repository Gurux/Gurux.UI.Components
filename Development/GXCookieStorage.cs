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

using Microsoft.JSInterop;

namespace Gurux.UI.Components;
/// <summary>
/// Reads and writes browser cookies through JavaScript interop.
/// </summary>
public class GXCookieStorage : IGXCookieStorage
{
    private Lazy<IJSObjectReference> _accessorJsRef = new();
    private readonly IJSRuntime _jsRuntime;

    /// <summary>
    /// Creates a cookie storage service using the supplied JavaScript runtime.
    /// </summary>
    public GXCookieStorage(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    /// <summary>
    /// Assigns the supplied cookie string through the browser's document.cookie API.
    /// </summary>
    private async Task SetCookie(string value)
    {
        await _jsRuntime.InvokeVoidAsync("eval", $"document.cookie = \"{value}\"");
    }

    /// <summary>
    /// Reads the cookie string visible to the current document.
    /// </summary>
    private async Task<string> GetCookie()
    {
        return await _jsRuntime.InvokeAsync<string>("eval", $"document.cookie");
    }

    /// <summary>
    /// Writes a browser cookie with the specified value, root path, and expiration in days.
    /// </summary>
    public async Task SetValueAsync(
        string key,
        string? value,
        int days)
    {
        var curExp = days > 0 ? DateToUTC(days) : "";
        await SetCookie($"{key}={value}; expires={curExp}; path=/");
    }

    /// <summary>
    /// Reads the named browser cookie, ignoring key case and returning the default when no matching cookie exists.
    /// </summary>
    public async Task<string?> GetValueAsync(
        string key,
        string? def)
    {
        var value = await GetCookie();
        if (string.IsNullOrEmpty(value))
        {
            return def;
        }
        var vals = value.Split(';', StringSplitOptions.RemoveEmptyEntries);
        foreach (var val in vals)
        {
            int pos = val.IndexOf('=');
            if (pos != -1)
            {
                if (string.Compare(val.Substring(0, pos).Trim(), key, true) == 0)
                {
                    return val.Substring(1 + pos);
                }
            }
        }
        return def;
    }

    /// <summary>
    /// Formats the cookie expiration date as an RFC 1123 UTC timestamp after the specified number of days.
    /// </summary>
    /// <param name="days">Days before expiration.</param>
    private static string DateToUTC(int days) =>
        DateTime.Now.AddDays(days).ToUniversalTime().ToString("R");

    /// <summary>
    /// Disposes the JavaScript accessor when it has been created.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_accessorJsRef.IsValueCreated)
        {
            await _accessorJsRef.Value.DisposeAsync();
        }
    }
}
