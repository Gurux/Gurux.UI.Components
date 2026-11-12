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
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Gurux.UI.Components;

/// <summary>Optional appearance subscription and scoped conventions for existing components.</summary>
public abstract class GXAdaptiveComponentBase : ComponentBase
{
    [Inject] protected IServiceProvider AppearanceServices { get; set; } = default!;
    [CascadingParameter(Name = "GXParentLayout")] protected GXLayoutStyle? ParentLayoutStyle { get; set; }
    private readonly Dictionary<string, Task<IJSObjectReference?>> _componentModules = new();
    protected bool AdaptiveDisposed => _appearanceDisposed;
    private IGXThemeService? _appearance;
    private bool _appearanceDisposed;
    private CancellationTokenSource? _press;
    private double _pressX, _pressY;
    private bool _suppressLongPressClick;
    protected GXAdaptiveBehavior AdaptiveBehavior => _appearance?.GetAdaptiveBehavior(ParentLayoutStyle) ?? GXAdaptiveBehavior.Default;
    protected bool IsAdaptive => AdaptiveBehavior.Layout != GXLayoutStyle.Default;
    protected string AdaptiveClass(string original) => !IsAdaptive ? original : $"{original} gx-layout-{AdaptiveBehavior.Layout.ToString().ToLowerInvariant()} gx-responsive-{AdaptiveBehavior.ResponsiveMode.ToString().ToLowerInvariant()}";

    protected override void OnInitialized()
    {
        base.OnInitialized();
        _appearance = AppearanceServices.GetService(typeof(IGXThemeService)) as IGXThemeService;
        if (_appearance != null)
        {
            _appearance.AppearanceChanged += AppearanceChanged;
        }
    }

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

    protected async Task StartLongPressAsync(PointerEventArgs args, Func<Task> open)
    {
        CancelLongPress();
        _suppressLongPressClick = false;
        if (args.PointerType is not ("touch" or "pen") || AdaptiveBehavior.Layout is not (GXLayoutStyle.Cupertino or GXLayoutStyle.Material))
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

    protected void MoveLongPress(PointerEventArgs args)
    {
        if (Math.Abs(args.ClientX - _pressX) > 8 || Math.Abs(args.ClientY - _pressY) > 8)
        {
            CancelLongPress();
        }
    }

    protected void MarkContextGestureHandled()
    {
        _suppressLongPressClick = true;
        CancelLongPress();
    }
    protected void CancelLongPress() => _press?.Cancel();
    protected bool ConsumeLongPressClick()
    {
        var suppress = _suppressLongPressClick;
        _suppressLongPressClick = false;
        return suppress;
    }

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

    private async Task<IJSObjectReference?> LoadComponentModuleAsync(IJSRuntime runtime, string path)
    {
        var module = await runtime.InvokeAsync<IJSObjectReference>("import", path);
        if (_appearanceDisposed)
        {
            await ReleaseModuleAsync(module);
            return null;
        }
        return module;
    }

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

    protected void DisposeAdaptiveBehavior()
    {
        _appearanceDisposed = true;
        CancelLongPress();
        if (_appearance != null)
        {
            _appearance.AppearanceChanged -= AppearanceChanged;
        }
    }
}
