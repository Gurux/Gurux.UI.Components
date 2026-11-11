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

namespace Gurux.UI.Components;

/// <summary>
/// Tracks active progress operations and their messages within one application scope.
/// </summary>
public sealed class GXProgressService : IGXProgress
{
    private readonly object _sync = new();
    private readonly Dictionary<Guid, (GXProgressScope Scope, GXProgressItem Item)> _operations = new();

    /// <summary>
    /// Occurs when progress operations are added, updated, or ended.
    /// </summary>
    public event Action? Changed;

    /// <summary>
    /// Gets whether at least one progress operation is active, including operations without a message.
    /// </summary>
    public bool IsBusy
    {
        get
        {
            lock (_sync)
            {
                return _operations.Count != 0;
            }
        }
    }

    /// <summary>
    /// Gets the nonempty messages of the currently active progress operations.
    /// </summary>
    public IReadOnlyList<string> Tasks
    {
        get
        {
            lock (_sync)
            {
                return _operations.Values.Select(operation => operation.Item.Message)
                .Where(message => !string.IsNullOrWhiteSpace(message)).Select(message => message!).ToArray();
            }
        }
    }

    /// <inheritdoc />
    public IReadOnlyList<GXProgressItem> Items
    {
        get
        {
            lock (_sync)
            {
                return _operations.Values.Select(operation => operation.Item).ToArray();
            }
        }
    }

    /// <inheritdoc />
    public void ProgressUpdate(Guid id, int completed, int total)
    {
        lock (_sync)
        {
            if (!_operations.TryGetValue(id, out var operation))
            {
                return;
            }
            _operations[id] = (operation.Scope, operation.Item with { Completed = completed, Total = total, Segments = Array.Empty<(Color Color, int Completed)>() });
        }
        Changed?.Invoke();
    }

    /// <inheritdoc />
    public void ProgressUpdate(Guid id, IEnumerable<(Color Color, int Completed)> completed, int total)
    {
        lock (_sync)
        {
            if (!_operations.TryGetValue(id, out var operation))
            {
                return;
            }
            ArgumentNullException.ThrowIfNull(completed);
            var segments = completed.ToArray();
            int count = 0;
            foreach (var segment in segments)
            {
                ArgumentOutOfRangeException.ThrowIfNegative(segment.Completed);
                count = checked(count + segment.Completed);
            }
            _operations[id] = (operation.Scope, operation.Item with
            {
                Completed = count,
                Total = total,
                Segments = Array.AsReadOnly(segments)
            });
        }
        Changed?.Invoke();
    }

    /// <inheritdoc />
    public GXProgressScope ProgressStart(string? message, int completed = 0, int total = 0)
    {
        var scope = new GXProgressScope(ProgressEnd);
        lock (_sync)
        {
            _operations.Add(scope.Id, (scope, new GXProgressItem(scope.Id, message, completed, total)));
        }

        Changed?.Invoke();
        return scope;
    }

    /// <inheritdoc />
    public GXProgressScope ProgressStart(string? message, IEnumerable<(Color Color, int Completed)> completed, int total = 0)
    {
        ArgumentNullException.ThrowIfNull(completed);
        var segments = completed.ToArray();
        int count = 0;
        foreach (var segment in segments)
        {
            ArgumentOutOfRangeException.ThrowIfNegative(segment.Completed);
            count = checked(count + segment.Completed);
        }
        var scope = new GXProgressScope(ProgressEnd);
        lock (_sync)
        {
            _operations.Add(scope.Id, (scope, new GXProgressItem(scope.Id, message, count, total)
            {
                Segments = Array.AsReadOnly(segments)
            }));
        }

        Changed?.Invoke();
        return scope;
    }

    /// <summary>
    /// Ends the progress operation with the specified id and notifies listeners when an operation was removed.
    /// </summary>
    public void ProgressEnd(Guid id)
    {
        GXProgressScope scope;
        lock (_sync)
        {
            if (!_operations.Remove(id, out var operation))
            {
                return;
            }
            scope = operation.Scope;
        }
        scope.Dispose();
        Changed?.Invoke();
    }

    /// <summary>
    /// Requests cancellation of all currently active progress operations.
    /// </summary>
    public Task CancelAllAsync()
    {
        GXProgressScope[] scopes;
        lock (_sync)
        {
            scopes = _operations.Values.Select(operation => operation.Scope).ToArray();
        }

        return Task.WhenAll(scopes.Select(scope => scope.CancelAsync()));
    }
}
