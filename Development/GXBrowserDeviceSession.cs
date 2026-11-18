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
/// <summary>Owns a browser module and its independent device controller until disposal.</summary>
/// <param name="runtime">The JavaScript runtime connected to the browser.</param>
/// <param name="path">The browser module asset path to import.</param>
/// <param name="arguments">Arguments passed to the module controller factory.</param>
internal sealed class GXBrowserDeviceSession(IJSRuntime runtime, string path, params object?[] arguments) : IAsyncDisposable
{
    /// <summary>Caches the pending controller initialization to avoid duplicate browser devices.</summary>
    private Task<IJSObjectReference>? _initialization;
    /// <summary>Holds the imported JavaScript module until its owner is disposed.</summary>
    private IJSObjectReference? _module;
    /// <summary>Holds the independent browser device controller.</summary>
    private IJSObjectReference? _controller;
    /// <summary>Records whether this owner has released its resources and stopped accepting updates.</summary>
    private bool _disposed;

    /// <summary>Determines whether an error represents an unavailable or canceled browser operation.</summary>
    /// <param name="error">The failure to classify.</param>
    /// <returns>True when the failure represents JavaScript, a disconnected browser, or cancellation.</returns>
    internal static bool IsInteropFailure(Exception error) =>
        error is JSException or JSDisconnectedException or OperationCanceledException;

    /// <summary>Returns the existing controller or starts a retryable initialization attempt.</summary>
    /// <returns>The initialized controller reference.</returns>
    internal Task<IJSObjectReference> GetAsync()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (_initialization is null || _initialization.IsFaulted || _initialization.IsCanceled)
        {
            _initialization = InitializeAsync();
        }
        return _initialization;
    }

    /// <summary>Imports the browser module and creates the controller, releasing references if initialization fails.</summary>
    /// <returns>The initialized controller reference.</returns>
    private async Task<IJSObjectReference> InitializeAsync()
    {
        IJSObjectReference? module = null;
        IJSObjectReference? controller = null;
        var transferred = false;
        try
        {
            module = await GXJavaScriptInterop.InvokeAsync<IJSObjectReference>(runtime, "import", path);
            ObjectDisposedException.ThrowIf(_disposed, this);
            controller = await GXJavaScriptInterop.InvokeAsync<IJSObjectReference>(module, "create", arguments);
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

    /// <summary>Stops the browser controller and releases both JavaScript references.</summary>
    /// <param name="controller">The device controller to stop and release, if created.</param>
    /// <param name="module">The imported module to release, if created.</param>
    /// <returns>The asynchronous reference cleanup operation.</returns>
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

    /// <inheritdoc />
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
