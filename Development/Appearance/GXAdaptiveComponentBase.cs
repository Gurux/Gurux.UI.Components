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

using Gurux.UI.Components.Enums;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Gurux.UI.Components.Appearance;

/// <summary>Optional appearance subscription and scoped conventions for existing components.</summary>
public abstract class GXAdaptiveComponentBase : ComponentBase
{
    /// <summary>Gets or sets services used to subscribe to the scoped appearance state.</summary>
    [Inject] protected IServiceProvider AppearanceServices { get; set; } = default!;
    /// <summary>Gets or sets the optional layout override inherited from the parent component.</summary>
    [CascadingParameter(Name = "GXParentLayout")] protected Enums.LayoutStyle? ParentLayoutStyle { get; set; }
    /// <summary>Caches module initialization tasks by asset path for this component instance.</summary>
    private readonly Dictionary<string, Task<IJSObjectReference?>> _componentModules = [];
    /// <summary>Gets whether adaptive subscriptions and interactions have been stopped.</summary>
    protected bool AdaptiveDisposed => _appearanceDisposed;
    /// <summary>Provides the current scoped theme and adaptive behavior settings.</summary>
    private IGXThemeService? _appearance;
    /// <summary>Records whether appearance subscriptions and gestures have been stopped.</summary>
    private bool _appearanceDisposed;
    /// <summary>Cancels the currently pending long press gesture.</summary>
    private CancellationTokenSource? _press;
    /// <summary>Stores the initial pointer coordinates used to cancel a moving long press gesture.</summary>
    private double _pressX, _pressY;
    /// <summary>Records whether a completed long press must consume the following click.</summary>
    private bool _suppressLongPressClick;
    /// <summary>Gets conventions resolved from the scoped appearance state and parent layout override.</summary>
    protected GXAdaptiveBehavior AdaptiveBehavior => _appearance?.GetAdaptiveBehavior(ParentLayoutStyle) ?? GXAdaptiveBehavior.Default;
    /// <summary>Gets whether a layout other than the default is active.</summary>
    protected bool IsAdaptive => AdaptiveBehavior.Layout != LayoutStyle.Default;
    /// <summary>Adds the active layout and viewport classes to the supplied component classes.</summary>
    /// <param name="original">The original CSS classes to preserve.</param>
    /// <returns>The supplied classes together with active adaptive layout classes.</returns>
    protected string AdaptiveClass(string original) => !IsAdaptive ? original : $"{original} gx-layout-{AdaptiveBehavior.Layout.ToString().ToLowerInvariant()} gx-responsive-{AdaptiveBehavior.ResponsiveMode.ToString().ToLowerInvariant()}";

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        base.OnInitialized();
        _appearance = AppearanceServices.GetService(typeof(IGXThemeService)) as IGXThemeService;
        _appearance?.AppearanceChanged += AppearanceChanged;
    }

    /// <summary>Refreshes the component when appearance settings change, unless it has been disposed.</summary>
    /// <param name="sender">The service that raised the appearance notification.</param>
    /// <param name="args">The appearance change notification data.</param>
    private void AppearanceChanged(object? sender, EventArgs args)
    {
        if (!_appearanceDisposed)
        {
            _ = InvokeAsync(() =>
            {
                if (!_appearanceDisposed)
                {
                    StateHasChanged();
                }
            });
        }
    }

    /// <summary>Opens the supplied action after a stationary touch or pen press on supported adaptive layouts.</summary>
    /// <param name="args">The pointer coordinates and device type associated with the gesture.</param>
    /// <param name="open">The asynchronous action invoked when the long press completes.</param>
    /// <returns>The pending gesture operation, completing normally when the gesture is canceled.</returns>
    protected async Task StartLongPressAsync(PointerEventArgs args, Func<Task> open)
    {
        CancelLongPress();
        _suppressLongPressClick = false;
        if (args.PointerType is not ("touch" or "pen") || AdaptiveBehavior.Layout is not (LayoutStyle.Cupertino or LayoutStyle.Material))
        {
            return;
        }
        var press = new CancellationTokenSource();
        _press = press;
        _pressX = args.ClientX;
        _pressY = args.ClientY;
        try
        {
            await Task.Delay(500, press.Token);
            if (!_appearanceDisposed && !press.IsCancellationRequested)
            {
                _suppressLongPressClick = true;
                await open();
            }
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            if (ReferenceEquals(_press, press))
            {
                _press = null;
            }
            press.Dispose();
        }
    }

    /// <summary>Cancels the pending long press when the pointer moves beyond the gesture threshold.</summary>
    /// <param name="args">The pointer coordinates and device type associated with the gesture.</param>
    protected void MoveLongPress(PointerEventArgs args)
    {
        if (Math.Abs(args.ClientX - _pressX) > 8 || Math.Abs(args.ClientY - _pressY) > 8)
        {
            CancelLongPress();
        }
    }

    /// <summary>Consumes the next click and cancels any pending long press after a context gesture.</summary>
    protected void MarkContextGestureHandled()
    {
        _suppressLongPressClick = true;
        CancelLongPress();
    }
    /// <summary>Cancels the currently pending long press delay.</summary>
    protected void CancelLongPress() => _press?.Cancel();
    /// <summary>Consumes and clears the click suppression flag set by a completed context gesture.</summary>
    /// <returns>True when the next click belongs to an already handled context gesture.</returns>
    protected bool ConsumeLongPressClick()
    {
        var suppress = _suppressLongPressClick;
        _suppressLongPressClick = false;
        return suppress;
    }

    /// <summary>Returns a cached module initialization task, retrying failed or canceled imports.</summary>
    /// <param name="runtime">The JavaScript runtime connected to the browser.</param>
    /// <param name="path">The browser module asset path to import.</param>
    /// <returns>The module import task, or a completed null result after disposal.</returns>
    protected Task<IJSObjectReference?> GetComponentModuleAsync(IJSRuntime runtime, string path)
    {
        if (_appearanceDisposed)
        {
            return Task.FromResult<IJSObjectReference?>(null);
        }
        if (!_componentModules.TryGetValue(path, out var pending) || pending.IsFaulted || pending.IsCanceled)
        {
            pending = LoadComponentModuleAsync(runtime, path);
            _componentModules[path] = pending;
        }
        return pending;
    }

    /// <summary>Imports a module and releases it immediately if the component was disposed during import.</summary>
    /// <param name="runtime">The JavaScript runtime connected to the browser.</param>
    /// <param name="path">The browser module asset path to import.</param>
    /// <returns>The imported module, or null if the component was disposed during import.</returns>
    private async Task<IJSObjectReference?> LoadComponentModuleAsync(IJSRuntime runtime, string path)
    {
        var module = await GXJavaScriptInterop.InvokeAsync<IJSObjectReference>(runtime, "import", path);
        if (_appearanceDisposed)
        {
            await ReleaseModuleAsync(module);
            return null;
        }
        return module;
    }

    /// <summary>Releases completed module imports owned by this component and clears the cache.</summary>
    /// <returns>The asynchronous module cleanup operation.</returns>
    protected async Task ReleaseComponentModulesAsync()
    {
        var pending = _componentModules.Values.ToArray();
        _componentModules.Clear();
        foreach (var task in pending)
        {
            if (task.IsCompletedSuccessfully)
            {
                await ReleaseModuleAsync(task.Result);
            }
        }
    }

    /// <summary>Disposes an imported module while tolerating browser disconnects and cancellation during cleanup.</summary>
    /// <param name="module">The imported module to release, if created.</param>
    /// <returns>The asynchronous module cleanup operation.</returns>
    private static async Task ReleaseModuleAsync(IJSObjectReference? module)
    {
        try
        {
            if (module != null)
            {
                await module.DisposeAsync();
            }
        }
        catch (Exception ex) when (ex is JSException or JSDisconnectedException or OperationCanceledException)
        {
        }
    }

    /// <summary>Stops gestures and unsubscribes from appearance notifications.</summary>
    protected void DisposeAdaptiveBehavior()
    {
        _appearanceDisposed = true;
        CancelLongPress();
        _appearance?.AppearanceChanged -= AppearanceChanged;
    }
}
