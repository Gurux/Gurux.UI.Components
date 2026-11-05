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

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Gurux.UI.Components;

/// <summary>Evaluates command policies and invalidates visibility when authorization changes.</summary>
internal sealed class GXCommandAuthorization : IDisposable
{
    private readonly IAuthorizationService? authorization;
    private readonly AuthenticationStateProvider? authentication;
    private readonly IGXAuthorizationRefreshSource? refreshSource;
    private readonly Action invalidated;
    private Snapshot snapshot = new(0, new Dictionary<string, bool>(StringComparer.Ordinal));
    private long version;
    private volatile bool disposed;

    internal GXCommandAuthorization(IServiceProvider services, Action invalidated)
    {
        this.invalidated = invalidated;
        authorization = services.GetService<IAuthorizationService>();
        authentication = services.GetService<AuthenticationStateProvider>();
        refreshSource = services.GetService<IGXAuthorizationRefreshSource>();
        if (authentication != null)
        {
            authentication.AuthenticationStateChanged += AuthenticationChanged;
        }
        if (refreshSource != null)
        {
            refreshSource.Changed += Invalidate;
        }
    }

    internal bool IsAllowed(string? policy)
    {
        if (disposed)
        {
            return false;
        }
        if (string.IsNullOrWhiteSpace(policy))
        {
            return true;
        }
        var current = Volatile.Read(ref snapshot);
        return current.Version == Volatile.Read(ref version) && current.Decisions.TryGetValue(policy, out var allowed) && allowed;
    }

    internal long Version => Volatile.Read(ref version);

    internal async Task RefreshAsync(IEnumerable<string?> policies, Task<AuthenticationState>? cascadingState)
    {
        if (disposed)
        {
            return;
        }
        var currentVersion = Interlocked.Increment(ref version);
        Volatile.Write(ref snapshot, new Snapshot(currentVersion, new Dictionary<string, bool>(StringComparer.Ordinal)));
        var decisions = new Dictionary<string, bool>(StringComparer.Ordinal);
        foreach (var policy in policies.Where(p => !string.IsNullOrWhiteSpace(p)).Distinct(StringComparer.Ordinal))
        {
            decisions[policy!] = await EvaluateAsync(policy!, cascadingState);
        }
        if (!disposed && currentVersion == Volatile.Read(ref version))
        {
            Volatile.Write(ref snapshot, new Snapshot(currentVersion, decisions));
        }
    }

    internal async Task<bool> CanExecuteAsync(string? policy, Task<AuthenticationState>? cascadingState)
    {
        if (disposed)
        {
            return false;
        }
        var currentVersion = Volatile.Read(ref version);
        var allowed = string.IsNullOrWhiteSpace(policy) || await EvaluateAsync(policy, cascadingState);
        return !disposed && currentVersion == Volatile.Read(ref version) && allowed;
    }

    private async Task<bool> EvaluateAsync(string policy, Task<AuthenticationState>? cascadingState)
    {
        if (disposed || authorization == null)
        {
            return false;
        }
        try
        {
            var state = authentication != null ? authentication.GetAuthenticationStateAsync() : cascadingState;
            return state != null && (await authorization.AuthorizeAsync((await state).User, null, policy)).Succeeded;
        }
        catch (Exception) { return false; }
    }

    private void AuthenticationChanged(Task<AuthenticationState> _) => Invalidate();

    private void Invalidate()
    {
        if (disposed)
        {
            return;
        }
        var currentVersion = Interlocked.Increment(ref version);
        Volatile.Write(ref snapshot, new Snapshot(currentVersion, new Dictionary<string, bool>(StringComparer.Ordinal)));
        invalidated();
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }
        disposed = true;
        Interlocked.Increment(ref version);
        if (authentication != null)
        {
            authentication.AuthenticationStateChanged -= AuthenticationChanged;
        }
        if (refreshSource != null)
        {
            refreshSource.Changed -= Invalidate;
        }
    }

    private sealed record Snapshot(long Version, IReadOnlyDictionary<string, bool> Decisions);
}
