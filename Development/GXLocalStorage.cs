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

using Gurux.UI.Components;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using System.Text;

/// <summary>
/// Reads, writes, and removes browser local storage entries, optionally grouped by a key prefix.
/// </summary>
public class GXLocalStorage : IGXLocalStorage
{
    private readonly IJSRuntime? _jsRuntime;
    private readonly ILogger<GXLocalStorage>? _logger;

    /// <summary>
    /// Creates a local storage service using the available browser runtime and logger.
    /// </summary>
    public GXLocalStorage(IJSRuntime? jsRuntime,
        ILogger<GXLocalStorage>? logger)
    {
        _logger = logger;
        _jsRuntime = jsRuntime;
        if (_jsRuntime?.GetType().Name == "UnsupportedJavaScriptRuntime")
        {
            _jsRuntime = null;
        }
    }

    /// <summary>
    /// Writes a value to browser local storage, removing the entry when the value is null.
    /// </summary>
    public Task SetValueAsync(string key, string? value) => SetValueAsync("", key, value);

    /// <summary>
    /// Writes a value to browser local storage, removing the entry when the value is null.
    /// </summary>
    public async Task SetValueAsync(string group, string key, string? value)
    {
        _logger?.LogDebug("Set local storage value, key: {Key}, value: {Value}", key, value);
        //_jsRuntime is null on the server side.
        if (_jsRuntime != null)
        {
            if (!string.IsNullOrEmpty(group))
            {
                key = group + ":" + key;
            }
            if (value == null)
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
            }
            else
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, Convert.ToBase64String(Encoding.Unicode.GetBytes(value)));
            }
        }
    }

    /// <summary>
    /// Reads a value from browser local storage, returning null when no value is available.
    /// </summary>
    public Task<string?> GetValueAsync(string key) => GetValueAsync("", key);

    /// <summary>
    /// Reads a value from browser local storage, returning null when no value is available.
    /// </summary>
    public async Task<string?> GetValueAsync(string group, string key)
    {
        try
        {
            string? value = null;
            if (_jsRuntime != null)
            {
                if (!string.IsNullOrEmpty(group))
                {
                    key = group + ":" + key;
                }
                value = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", key);
                if (string.IsNullOrEmpty(value) || value == "null")
                {
                    value = null;
                }
                if (value != null)
                {
                    try
                    {
                        value = ASCIIEncoding.Unicode.GetString(Convert.FromBase64String(value));
                    }
                    catch (Exception)
                    {
                        value = null;
                    }
                }
                else
                {
                    value = null;
                }
            }
            _logger?.LogDebug("Get local storage value, key: {Key}, value: {Value}", key, value);
            return value;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error getting local storage value, key: {Key}", key);
        }
        return null;
    }

    /// <summary>
    /// Removes the browser local storage entry identified by the key and optional group.
    /// </summary>
    public Task RemoveAsync(string key) => RemoveAsync("", key);

    /// <summary>
    /// Removes the browser local storage entry identified by the key and optional group.
    /// </summary>
    public async Task RemoveAsync(string group, string key)
    {
        if (_jsRuntime != null)
        {
            if (!string.IsNullOrEmpty(group))
            {
                key = group + ":" + key;
            }
            await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
        }
    }

    /// <summary>
    /// Clears all browser local storage entries or only entries belonging to the specified group.
    /// </summary>
    public Task ClearAsync() => ClearAsync("");

    /// <summary>
    /// Clears all browser local storage entries or only entries belonging to the specified group.
    /// </summary>
    public async Task ClearAsync(string? group)
    {
        if (_jsRuntime != null)
        {
            if (string.IsNullOrEmpty(group))
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.clear");
            }
            else
            {
                string tmp = "Object.keys(localStorage).filter(k => k.startsWith(" + System.Text.Json.JsonSerializer.Serialize(group + ":") + "))";
                string[]? values = await _jsRuntime.InvokeAsync<string[]>("eval", tmp);
                foreach (var it in values)
                {
                    await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", it);
                }
            }
        }
    }

    /// <summary>
    /// Retrieves browser local storage keys beginning with the specified group prefix.
    /// </summary>
    public async Task<IEnumerable<string>?> GetGroupValuesAsync(string group)
    {
        if (_jsRuntime != null)
        {
            if (!string.IsNullOrEmpty(group))
            {
                string tmp = "Object.keys(localStorage).filter(k => k.startsWith(" + System.Text.Json.JsonSerializer.Serialize(group + ":") + "))";
                string[]? values = await _jsRuntime.InvokeAsync<string[]>("eval", tmp);
                return values;
            }
        }
        return null;
    }
}
