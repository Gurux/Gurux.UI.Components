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

using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Gurux.UI.Components;

/// <summary>Reads device positions through the browser Geolocation API without storing coordinates.</summary>
public partial class GXGeoLocation
{
    /// <summary>Gets or sets whether the browser should prefer high accuracy, which may increase power use.</summary>
    [Parameter] public bool EnableHighAccuracy { get; set; } = true;
    /// <summary>Gets or sets the nonnegative location timeout in milliseconds.</summary>
    [Parameter] public int Timeout { get; set; } = 15000;
    /// <summary>Gets or sets the nonnegative maximum cached-position age in milliseconds.</summary>
    [Parameter] public int MaximumAge { get; set; }
    /// <summary>Gets or sets whether tracking starts after interactive rendering and follows parameter changes.</summary>
    [Parameter] public bool WatchPosition { get; set; }
    /// <summary>Gets or sets the callback receiving one-shot positions and active tracking updates.</summary>
    [Parameter] public EventCallback<GXGeoLocationResult> OnLocationChanged { get; set; }
    /// <summary>Gets or sets the callback receiving permission, timeout, browser and interop errors.</summary>
    [Parameter] public EventCallback<string> OnError { get; set; }

    [Inject] private IJSRuntime JS { get; set; } = default!;
    private ElementReference _element;
    private GXBrowserDeviceSession? _session;
    private DotNetObjectReference<GXGeoLocation>? _callback;
    private readonly CancellationTokenSource _lifetime = new();
    private bool _disposed;
    private bool _rendered;
    private bool _watching;
    private int _generation;
    private (bool Watch, bool HighAccuracy, int Timeout, int Age)? _appliedParameters;

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        if (Timeout < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(Timeout));
        }
        if (MaximumAge < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(MaximumAge));
        }
    }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        _rendered = true;
        var parameters = (WatchPosition, EnableHighAccuracy, Timeout, MaximumAge);
        if (_disposed || _appliedParameters == parameters)
        {
            return;
        }
        var previous = _appliedParameters;
        _appliedParameters = parameters;
        if (WatchPosition)
        {
            await StartWatchingAsync();
        }
        else if (previous?.Watch == true)
        {
            await StopWatchingAsync();
        }
    }

    private object Options => new { enableHighAccuracy = EnableHighAccuracy, timeout = Timeout, maximumAge = MaximumAge };

    private Task<IJSObjectReference> ControllerAsync()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        if (!_rendered)
        {
            throw new InvalidOperationException("Location methods require an interactive render.");
        }
        _callback ??= DotNetObjectReference.Create(this);
        _session ??= new GXBrowserDeviceSession(JS, "./_content/Gurux.UI.Components/js/gx-geolocation.js", _element, _callback);
        return _session.GetAsync();
    }

    /// <summary>Gets one position and invokes OnLocationChanged. Reports browser failures through OnError and rethrows them.</summary>
    public async Task<GXGeoLocationResult> GetLocationAsync()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        OnParametersSet();
        GXGeoLocationResult result;
        try
        {
            var controller = await ControllerAsync();
            ObjectDisposedException.ThrowIf(_disposed, this);
            result = await controller.InvokeAsync<GXGeoLocationResult>("getLocation", _lifetime.Token, Options);
            ObjectDisposedException.ThrowIf(_disposed, this);
        }
        catch (Exception error) when (GXBrowserDeviceSession.IsInteropFailure(error))
        {
            if (!_disposed)
            {
                await OnError.InvokeAsync(error.Message);
            }
            throw;
        }
        await OnLocationChanged.InvokeAsync(result);
        return result;
    }

    /// <summary>Starts or replaces this component's watcher. Browser failures are reported through OnError.</summary>
    public async Task StartWatchingAsync()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        OnParametersSet();
        var generation = ++_generation;
        try
        {
            var controller = await ControllerAsync();
            if (_disposed || generation != _generation)
            {
                return;
            }
            _watching = true;
            await controller.InvokeVoidAsync("startWatching", Options, generation);
        }
        catch (Exception error) when (GXBrowserDeviceSession.IsInteropFailure(error) || (_disposed && error is ObjectDisposedException))
        {
            if (!_disposed && generation == _generation)
            {
                _watching = false;
                await OnError.InvokeAsync(error.Message);
            }
        }
    }

    /// <summary>Stops only this component's watcher and invalidates its already queued notifications.</summary>
    public async Task StopWatchingAsync()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        ++_generation;
        _watching = false;
        try
        {
            if (_session is not null)
            {
                var controller = await _session.GetAsync();
                if (!_disposed)
                {
                    await controller.InvokeVoidAsync("stopWatching");
                }
            }
        }
        catch (Exception error) when (GXBrowserDeviceSession.IsInteropFailure(error) || (_disposed && error is ObjectDisposedException))
        {
            if (!_disposed)
            {
                await OnError.InvokeAsync(error.Message);
            }
        }
    }

    /// <summary>Receives an active watcher position. Intended only for the isolated JavaScript controller.</summary>
    [JSInvokable]
    public Task ReceiveLocation(GXGeoLocationResult location, int generation) => _disposed ? Task.CompletedTask : InvokeAsync(async () =>
    {
        if (!_disposed && _watching && generation == _generation)
        {
            await OnLocationChanged.InvokeAsync(location);
        }
    });

    /// <summary>Receives an active watcher error. Intended only for the isolated JavaScript controller.</summary>
    [JSInvokable]
    public Task ReceiveError(string message, int generation) => _disposed ? Task.CompletedTask : InvokeAsync(async () =>
    {
        if (!_disposed && _watching && generation == _generation)
        {
            await OnError.InvokeAsync(message);
        }
    });

    /// <summary>Clears this component's watcher and releases its callback and module references.</summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }
        _disposed = true;
        _lifetime.Cancel();
        _watching = false;
        ++_generation;
        try
        {
            if (_session is not null)
            {
                await _session.DisposeAsync();
            }
        }
        finally
        {
            _callback?.Dispose();
            _callback = null;
            _lifetime.Dispose();
        }
    }
}
