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

/// <summary>Converts a lost browser connection into cancellation while preserving other JavaScript failures.</summary>
internal static class GXJavaScriptInterop
{
    /// <summary>Invokes browser JavaScript and cancels the operation if its connection has ended.</summary>
    /// <param name="target">The browser runtime or imported JavaScript object.</param>
    /// <param name="identifier">The JavaScript function to invoke.</param>
    /// <param name="arguments">Arguments forwarded unchanged to the function.</param>
    /// <returns>The asynchronous invocation.</returns>
    /// <exception cref="TaskCanceledException">The browser connection ended before the call completed.</exception>
    /// <exception cref="JSException">JavaScript failed while the connection was available.</exception>
    internal static async ValueTask InvokeVoidAsync(IJSRuntime target, string identifier, params object?[]? arguments)
    {
        try
        {
            await target.InvokeVoidAsync(identifier, arguments);
        }
        catch (JSDisconnectedException error)
        {
            throw new TaskCanceledException(error.Message, error);
        }
    }

    /// <summary>Invokes browser JavaScript and cancels the operation if its connection has ended.</summary>
    /// <typeparam name="TValue">The JavaScript result type.</typeparam>
    /// <param name="target">The browser runtime or imported JavaScript object.</param>
    /// <param name="identifier">The JavaScript function to invoke.</param>
    /// <param name="arguments">Arguments forwarded unchanged to the function.</param>
    /// <returns>The JavaScript result.</returns>
    /// <exception cref="TaskCanceledException">The browser connection ended before the call completed.</exception>
    /// <exception cref="JSException">JavaScript failed while the connection was available.</exception>
    internal static async ValueTask<TValue> InvokeAsync<TValue>(IJSRuntime target, string identifier, params object?[]? arguments)
    {
        try
        {
            return await target.InvokeAsync<TValue>(identifier, arguments);
        }
        catch (JSDisconnectedException error)
        {
            throw new TaskCanceledException(error.Message, error);
        }
    }

    /// <summary>Invokes browser JavaScript and cancels the operation if its connection has ended.</summary>
    /// <param name="target">The browser runtime or imported JavaScript object.</param>
    /// <param name="identifier">The JavaScript function to invoke.</param>
    /// <param name="arguments">Arguments forwarded unchanged to the function.</param>
    /// <returns>The asynchronous invocation.</returns>
    /// <exception cref="TaskCanceledException">The browser connection ended before the call completed.</exception>
    /// <exception cref="JSException">JavaScript failed while the connection was available.</exception>
    internal static async ValueTask InvokeVoidAsync(IJSObjectReference target, string identifier, params object?[]? arguments)
    {
        try
        {
            await target.InvokeVoidAsync(identifier, arguments);
        }
        catch (JSDisconnectedException error)
        {
            throw new TaskCanceledException(error.Message, error);
        }
    }

    /// <summary>Invokes browser JavaScript and cancels the operation if its connection has ended.</summary>
    /// <typeparam name="TValue">The JavaScript result type.</typeparam>
    /// <param name="target">The browser runtime or imported JavaScript object.</param>
    /// <param name="identifier">The JavaScript function to invoke.</param>
    /// <param name="arguments">Arguments forwarded unchanged to the function.</param>
    /// <returns>The JavaScript result.</returns>
    /// <exception cref="TaskCanceledException">The browser connection ended before the call completed.</exception>
    /// <exception cref="JSException">JavaScript failed while the connection was available.</exception>
    internal static async ValueTask<TValue> InvokeAsync<TValue>(IJSObjectReference target, string identifier, params object?[]? arguments)
    {
        try
        {
            return await target.InvokeAsync<TValue>(identifier, arguments);
        }
        catch (JSDisconnectedException error)
        {
            throw new TaskCanceledException(error.Message, error);
        }
    }

}
