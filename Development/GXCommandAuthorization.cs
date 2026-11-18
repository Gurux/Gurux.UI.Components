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
    /// <summary>Evaluates policies against the current authenticated user.</summary>
    private readonly IAuthorizationService? _authorization;
    /// <summary>Supplies authentication state and notifies when the current user changes.</summary>
    private readonly AuthenticationStateProvider? _authentication;
    /// <summary>Notifies when authorization policies must be evaluated again.</summary>
    private readonly IGXAuthorizationRefreshSource? _refreshSource;
    /// <summary>Notifies the owner that cached authorization decisions are no longer current.</summary>
    private readonly Action _invalidated;
    /// <summary>Stores the latest immutable set of policy decisions and its version.</summary>
    private Snapshot _snapshot = new(0, new Dictionary<string, bool>(StringComparer.Ordinal));
    /// <summary>Identifies the current authorization evaluation generation.</summary>
    private long _version;
    /// <summary>Records whether this owner has released its resources and stopped accepting updates.</summary>
    private volatile bool _disposed;

    /// <summary>Connects policy evaluation to authentication and authorization refresh notifications.</summary>
    /// <param name="services">The scoped services used to resolve authorization dependencies.</param>
    /// <param name="invalidated">The callback invoked when cached policy decisions must be refreshed.</param>
    internal GXCommandAuthorization(IServiceProvider services, Action invalidated)
    {
        this._invalidated = invalidated;
        _authorization = services.GetService<IAuthorizationService>();
        _authentication = services.GetService<AuthenticationStateProvider>();
        _refreshSource = services.GetService<IGXAuthorizationRefreshSource>();
        if (_authentication != null)
        {
            _authentication.AuthenticationStateChanged += AuthenticationChanged;
        }
        if (_refreshSource != null)
        {
            _refreshSource.Changed += Invalidate;
        }
    }

    /// <summary>Checks the latest cached decision for a command policy.</summary>
    /// <param name="policy">The authorization policy name, or an empty value for unrestricted access.</param>
    /// <returns>True when the cached decision permits the command.</returns>
    internal bool IsAllowed(string? policy)
    {
        if (_disposed)
        {
            return false;
        }
        if (string.IsNullOrWhiteSpace(policy))
        {
            return true;
        }
        var current = Volatile.Read(ref _snapshot);
        return current.Version == Volatile.Read(ref _version) && current.Decisions.TryGetValue(policy, out var allowed) && allowed;
    }

    /// <summary>Gets the generation used to reject outdated authorization results.</summary>
    internal long Version => Volatile.Read(ref _version);

    /// <summary>Evaluates the supplied policies and publishes results only if the authorization generation is still current.</summary>
    /// <param name="policies">The policy names whose current authorization decisions are required.</param>
    /// <param name="cascadingState">The authentication state inherited from the containing component, when available.</param>
    /// <returns>The asynchronous policy refresh operation.</returns>
    internal async Task RefreshAsync(IEnumerable<string?> policies, Task<AuthenticationState>? cascadingState)
    {
        if (_disposed)
        {
            return;
        }
        var currentVersion = Interlocked.Increment(ref _version);
        Volatile.Write(ref _snapshot, new Snapshot(currentVersion, new Dictionary<string, bool>(StringComparer.Ordinal)));
        var decisions = new Dictionary<string, bool>(StringComparer.Ordinal);
        foreach (var policy in policies.Where(p => !string.IsNullOrWhiteSpace(p)).Distinct(StringComparer.Ordinal))
        {
            decisions[policy!] = await EvaluateAsync(policy!, cascadingState);
        }
        if (!_disposed && currentVersion == Volatile.Read(ref _version))
        {
            Volatile.Write(ref _snapshot, new Snapshot(currentVersion, decisions));
        }
    }

    /// <summary>Evaluates whether the current user may execute a command under its policy.</summary>
    /// <param name="policy">The authorization policy name, or an empty value for unrestricted access.</param>
    /// <param name="cascadingState">The authentication state inherited from the containing component, when available.</param>
    /// <returns>True when the current user satisfies the policy.</returns>
    internal async Task<bool> CanExecuteAsync(string? policy, Task<AuthenticationState>? cascadingState)
    {
        if (_disposed)
        {
            return false;
        }
        var currentVersion = Volatile.Read(ref _version);
        var allowed = string.IsNullOrWhiteSpace(policy) || await EvaluateAsync(policy, cascadingState);
        return !_disposed && currentVersion == Volatile.Read(ref _version) && allowed;
    }

    /// <summary>Evaluates one policy, treating an absent policy as unrestricted.</summary>
    /// <param name="policy">The authorization policy name, or an empty value for unrestricted access.</param>
    /// <param name="cascadingState">The authentication state inherited from the containing component, when available.</param>
    /// <returns>True when the policy is absent or authorization succeeds.</returns>
    private async Task<bool> EvaluateAsync(string policy, Task<AuthenticationState>? cascadingState)
    {
        if (_disposed || _authorization == null)
        {
            return false;
        }
        try
        {
            var state = _authentication != null ? _authentication.GetAuthenticationStateAsync() : cascadingState;
            return state != null && (await _authorization.AuthorizeAsync((await state).User, null, policy)).Succeeded;
        }
        catch (Exception) { return false; }
    }

    /// <summary>Invalidates cached policies after an authentication state change.</summary>
    /// <param name="_">The authentication state notification; the next refresh reads the current user.</param>
    private void AuthenticationChanged(Task<AuthenticationState> _) => Invalidate();

    /// <summary>Advances the authorization generation and notifies the owner to refresh.</summary>
    private void Invalidate()
    {
        if (_disposed)
        {
            return;
        }
        var currentVersion = Interlocked.Increment(ref _version);
        Volatile.Write(ref _snapshot, new Snapshot(currentVersion, new Dictionary<string, bool>(StringComparer.Ordinal)));
        _invalidated();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }
        _disposed = true;
        Interlocked.Increment(ref _version);
        if (_authentication != null)
        {
            _authentication.AuthenticationStateChanged -= AuthenticationChanged;
        }
        if (_refreshSource != null)
        {
            _refreshSource.Changed -= Invalidate;
        }
    }

    /// <summary>Stores authorization decisions for one evaluation generation.</summary>
    /// <param name="Version">The evaluation generation associated with these decisions.</param>
    /// <param name="Decisions">The policy decisions captured by this snapshot.</param>
    private sealed record Snapshot(long Version, IReadOnlyDictionary<string, bool> Decisions);
}
