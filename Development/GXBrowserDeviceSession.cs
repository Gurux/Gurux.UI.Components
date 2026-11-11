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

// Owns references only; device state remains inside each independent JavaScript controller.
internal sealed class GXBrowserDeviceSession(IJSRuntime runtime, string path, params object?[] arguments) : IAsyncDisposable
{
    private Task<IJSObjectReference>? _initialization;
    private IJSObjectReference? _module;
    private IJSObjectReference? _controller;
    private bool _disposed;

    internal static bool IsInteropFailure(Exception error) =>
        error is JSException or JSDisconnectedException or OperationCanceledException;

    internal Task<IJSObjectReference> GetAsync()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_initialization is null || _initialization.IsFaulted || _initialization.IsCanceled)
        {
            _initialization = InitializeAsync();
        }
        return _initialization;
    }

    private async Task<IJSObjectReference> InitializeAsync()
    {
        IJSObjectReference? module = null;
        IJSObjectReference? controller = null;
        var transferred = false;
        try
        {
            module = await runtime.InvokeAsync<IJSObjectReference>("import", path);
            ObjectDisposedException.ThrowIf(_disposed, this);
            controller = await module.InvokeAsync<IJSObjectReference>("create", arguments);
            ObjectDisposedException.ThrowIf(_disposed, this);
            _module = module;
            _controller = controller;
            transferred = true;
            return controller;
        }
        finally
        {
            if (!transferred)
            {
                await ReleaseAsync(controller, module);
            }
        }
    }

    private static async ValueTask ReleaseAsync(IJSObjectReference? controller, IJSObjectReference? module)
    {
        if (controller is not null)
        {
            try
            {
                await controller.InvokeVoidAsync("dispose");
            }
            catch (Exception error) when (IsInteropFailure(error) || error is ObjectDisposedException) { }
            try
            {
                await controller.DisposeAsync();
            }
            catch (Exception error) when (IsInteropFailure(error) || error is ObjectDisposedException) { }
        }
        if (module is not null)
        {
            try
            {
                await module.DisposeAsync();
            }
            catch (Exception error) when (IsInteropFailure(error) || error is ObjectDisposedException) { }
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }
        _disposed = true;
        var controller = _controller;
        var module = _module;
        _controller = null;
        _module = null;
        // A pending initialization cleans up its own local references in finally.
        await ReleaseAsync(controller, module);
    }
}
