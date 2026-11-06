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

namespace Gurux.UI.Components;

/// <summary>Dispatches named notifications and coalesces deferred UI refreshes per listener.</summary>
/// <remarks>Use a scoped instance in Blazor applications. Notifications are not marshalled to a
/// renderer; component callbacks should use InvokeAsync. Defer only refresh notifications,
/// never commands, authentication changes or permission revocation.</remarks>
public class GXNotifierService : IGXNotifier, IDisposable
{
    private readonly object _sync = new();
    private readonly Dictionary<object, Listener> _listeners = new(ReferenceEqualityComparer.Instance);
    private bool _disposed;

    /// <inheritdoc />
    public void On(object listener, string methodName, Action handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        OnAsync(listener, methodName, () => { handler(); return Task.CompletedTask; });
    }

    /// <inheritdoc />
    public void On<T1>(object listener, string methodName, Action<T1> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        OnAsync<T1>(listener, methodName, value => { handler(value); return Task.CompletedTask; });
    }

    /// <summary>Registers an awaitable refresh handler.</summary>
    public void OnAsync(object listener, string methodName, Func<Task> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        Register(listener, methodName, false, _ => handler());
    }

    /// <summary>Registers an awaitable handler for a typed notification.</summary>
    public void OnAsync<T>(object listener, string methodName, Func<T, Task> handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        Register(listener, methodName, true, value => handler((T)value!));
    }

    private void Register(object listener, string name, bool typed, Func<object?, Task> callback)
    {
        ArgumentNullException.ThrowIfNull(listener);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        lock (_sync)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            var state = GetListener(listener);
            if (typed && state.Scopes.Any(scope => scope.Types.Contains(name) && scope.Refresh is null))
            {
                throw new InvalidOperationException("Typed notifications require a deferred refresh callback.");
            }
            state.Handlers.Add(new(name, typed, callback));
        }
    }

    /// <summary>Delivers a named refresh notification to its current subscribers.</summary>
    public Task ChangedAsync(string methodName) => DispatchAsync(methodName, null);

    /// <summary>Delivers a typed notification to its current subscribers.</summary>
    public Task ChangedAsync<T>(string methodName, T value) => DispatchAsync(methodName, value);

    private Task DispatchAsync(string name, object? value)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        List<Func<Task>> work = [];
        lock (_sync)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            foreach (var state in _listeners.Values)
            {
                foreach (var handler in state.Handlers.Where(handler => handler.Name == name))
                {
                    work.Add(() => DeliverAsync(state, handler, value));
                }
            }
        }
        return RunAsync(work);
    }

    private Task DeliverAsync(Listener state, Handler handler, object? value)
    {
        lock (_sync)
        {
            if (!IsCurrent(state) || !state.Handlers.Contains(handler))
            {
                return Task.CompletedTask;
            }
            var blockers = state.Scopes.Where(scope => scope.Types.Contains(handler.Name)).ToArray();
            if (blockers.Length != 0)
            {
                var refreshers = blockers.Where(scope => scope.Refresh != null).ToArray();
                if (refreshers.Length == 0)
                {
                    state.Pending.Add(handler.Name);
                }
                else
                {
                    // A canonical re-read covers every payload, including changes to different objects.
                    state.Pending.Remove(handler.Name);
                    foreach (var scope in refreshers)
                    {
                        state.Refreshes.Add(scope.Refresh!);
                    }
                }
                return Task.CompletedTask;
            }
        }
        return handler.Callback(value);
    }

    /// <inheritdoc />
    public IAsyncDisposable DeferUpdates(object listener, IEnumerable<string> types)
        => Defer(listener, types, null);

    /// <inheritdoc />
    public IAsyncDisposable DeferUpdates(object listener, IEnumerable<string> types, Func<Task> refresh)
    {
        ArgumentNullException.ThrowIfNull(refresh);
        return Defer(listener, types, refresh);
    }

    private IAsyncDisposable Defer(object listener, IEnumerable<string> types, Func<Task>? refresh)
    {
        ArgumentNullException.ThrowIfNull(listener);
        ArgumentNullException.ThrowIfNull(types);
        var names = types.ToArray();
        if (names.Length == 0 || names.Any(string.IsNullOrWhiteSpace))
        {
            throw new ArgumentException("Specify at least one nonempty notification name.", nameof(types));
        }
        var selected = new HashSet<string>(names, StringComparer.Ordinal);
        lock (_sync)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            var state = GetListener(listener);
            if (refresh is null && state.Handlers.Any(handler => handler.Typed && selected.Contains(handler.Name)))
            {
                throw new InvalidOperationException("Supply a refresh callback for typed notifications so incremental changes are not lost.");
            }
            var scope = new Deferral(this, state, selected, refresh);
            state.Scopes.Add(scope);
            return scope;
        }
    }

    private Task ResumeAsync(Deferral scope)
    {
        List<Func<Task>> work = [];
        lock (_sync)
        {
            var state = scope.Listener;
            if (!IsCurrent(state) || !state.Scopes.Remove(scope))
            {
                return Task.CompletedTask;
            }
            foreach (var name in state.Pending.ToArray())
            {
                if (!state.Scopes.Any(active => active.Types.Contains(name)))
                {
                    state.Pending.Remove(name);
                    foreach (var handler in state.Handlers.Where(handler => handler.Name == name))
                    {
                        work.Add(() => DeliverAsync(state, handler, null));
                    }
                }
            }
            // A canonical refresh can update several deferred types. Wait for all dialog scopes
            // belonging to this listener before invoking it, then call each distinct refresh once.
            if (state.Scopes.Count == 0)
            {
                foreach (var refresh in state.Refreshes)
                {
                    work.Add(() => RefreshAsync(state, refresh));
                }
                state.Refreshes.Clear();
            }
        }
        return RunAsync(work);
    }

    private Task RefreshAsync(Listener state, Func<Task> refresh)
    {
        lock (_sync)
        {
            if (!IsCurrent(state))
            {
                return Task.CompletedTask;
            }
            if (state.Scopes.Count != 0)
            {
                state.Refreshes.Add(refresh);
                return Task.CompletedTask;
            }
        }
        return refresh();
    }

    /// <inheritdoc />
    public void RemoveListener(object listener)
    {
        ArgumentNullException.ThrowIfNull(listener);
        lock (_sync)
        {
            if (_listeners.Remove(listener, out var state))
            {
                state.Clear();
            }
        }
    }

    private Listener GetListener(object listener)
    {
        if (!_listeners.TryGetValue(listener, out var state))
        {
            state = new Listener(listener);
            _listeners.Add(listener, state);
        }
        return state;
    }

    private bool IsCurrent(Listener state) => !_disposed
        && _listeners.TryGetValue(state.Owner, out var current) && ReferenceEquals(state, current);

    private static async Task RunAsync(IEnumerable<Func<Task>> callbacks)
    {
        List<Exception>? failures = null;
        foreach (var callback in callbacks)
        {
            try
            {
                await callback();
            }
            catch (Exception error)
            {
                (failures ??= []).Add(error);
            }
        }
        if (failures?.Count == 1)
        {
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(failures[0]).Throw();
        }
        if (failures?.Count > 1)
        {
            throw new AggregateException(failures);
        }
    }

    /// <summary>Discards subscriptions and pending refreshes; outstanding scopes become inert.</summary>
    public void Dispose()
    {
        lock (_sync)
        {
            _disposed = true;
            foreach (var state in _listeners.Values)
            {
                state.Clear();
            }
            _listeners.Clear();
        }
        GC.SuppressFinalize(this);
    }

    private sealed class Listener(object owner)
    {
        internal object Owner { get; } = owner;
        internal List<Handler> Handlers { get; } = [];
        internal HashSet<Deferral> Scopes { get; } = [];
        internal HashSet<string> Pending { get; } = new(StringComparer.Ordinal);
        internal HashSet<Func<Task>> Refreshes { get; } = [];
        internal void Clear()
        {
            Handlers.Clear();
            Scopes.Clear();
            Pending.Clear();
            Refreshes.Clear();
        }
    }

    private sealed record Handler(string Name, bool Typed, Func<object?, Task> Callback);

    private sealed class Deferral(GXNotifierService owner, Listener listener, HashSet<string> types, Func<Task>? refresh) : IAsyncDisposable
    {
        private GXNotifierService? _owner = owner;
        internal Listener Listener { get; } = listener;
        internal HashSet<string> Types { get; } = types;
        internal Func<Task>? Refresh { get; } = refresh;
        public ValueTask DisposeAsync()
        {
            var current = Interlocked.Exchange(ref _owner, null);
            return current is null ? ValueTask.CompletedTask : new ValueTask(current.ResumeAsync(this));
        }
    }
}
