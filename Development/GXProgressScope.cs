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

/// <summary>
/// Represents a cancellable progress operation that ends once when its scope is disposed.
/// </summary>
public sealed class GXProgressScope : IDisposable
{
    private Action? _end;
    private readonly CancellationTokenSource _cancellation = new();
    /// <summary>
    /// Gets the token used to request cancellation of the operation.
    /// </summary>
    public CancellationToken CancellationToken => _cancellation.Token;
    /// <summary>
    /// Requests cancellation of this progress operation unless it has already ended.
    /// </summary>
    public Task CancelAsync()
    {
        if (Volatile.Read(ref _end) is null)
        {
            return Task.CompletedTask;
        }
        try
        {
            return _cancellation.CancelAsync();
        }
        catch (ObjectDisposedException)
        {
            return Task.CompletedTask;
        }
    }
    /// <summary>
    /// Gets the Guid identifying this progress operation.
    /// </summary>
    public Guid Id { get; } = Guid.NewGuid();
    /// <summary>
    /// Creates a progress scope that invokes the supplied callback once when disposed.
    /// </summary>
    public GXProgressScope(Action<Guid> end)
    {
        ArgumentNullException.ThrowIfNull(end);
        _end = () => end(Id);
    }

    /// <summary>
    /// Creates a progress scope that invokes the supplied callback once when disposed.
    /// </summary>
    /// <param name="end">The action to invoke when the scope is disposed.</param>
    public GXProgressScope(Action end)
    {
        ArgumentNullException.ThrowIfNull(end);
        _end = end;
    }

    /// <summary>
    /// Ends the operation at most once and releases its cancellation token source.
    /// </summary>
    public void Dispose()
    {
        Action? end = Interlocked.Exchange(ref _end, null);
        if (end == null)
        {
            return;
        }
        try
        {
            end();
        }
        finally
        {
            _cancellation.Dispose();
        }
    }
}


