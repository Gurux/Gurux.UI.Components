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
