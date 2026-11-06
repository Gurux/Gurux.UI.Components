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

namespace Gurux.UI.Components
{
    /// <summary>
    /// Registers and removes handlers for named application notifications.
    /// </summary>
    public interface IGXNotifier
    {
        /// <summary>
        /// Registers a handler for the named notification and associates it with the supplied listener.
        /// </summary>
        /// <param name="listener">Listener.</param>
        /// <param name="methodName">Method name.</param>
        /// <param name="handler">Handler.</param>
        void On(object listener, string methodName, Action handler);

        /// <summary>
        /// Registers a handler for the named notification and associates it with the supplied listener.
        /// </summary>
        /// <typeparam name="T1">Notification parameter type.</typeparam>
        /// <param name="listener">Listener.</param>
        /// <param name="methodName">Method name.</param>
        /// <param name="handler">Handler.</param>
        /// <remarks> Event listening is stopped by calling RemoveListener. </remarks>
        /// <seealso cref="RemoveListener" />
        void On<T1>(object listener, string methodName, Action<T1> handler);

        /// <summary>
        /// Removes every notification handler registered by the supplied listener.
        /// </summary>
        void RemoveListener(object listener);
        /// <summary>Defers selected parameterless refresh notifications for one listener.</summary>
        /// <param name="listener">The same instance supplied when registering handlers.</param>
        /// <param name="types">Exact registered notification names, not payload-specific object types.</param>
        /// <returns>An async scope. Dispose it to deliver one pending invalidation per name after
        /// its last overlapping scope closes. An empty interval delivers nothing.</returns>
        /// <remarks>Other listeners and other names remain live. Typed messages require the refresh
        /// overload. Removing the listener drops pending work and invalidates outstanding scopes.</remarks>
        /// <exception cref="NotSupportedException">The application's notifier has not implemented deferral.</exception>
        IAsyncDisposable DeferUpdates(object listener, IEnumerable<string> types)
            => throw new NotSupportedException("This notifier does not implement update deferral. Use or derive from GXNotifierService.");

        /// <summary>Coalesces selected notifications into a canonical refresh instead of replaying payloads.</summary>
        /// <param name="listener">The same instance supplied when registering handlers.</param>
        /// <param name="types">Exact registered notification names to defer.</param>
        /// <param name="refresh">Reloads canonical state, typically through the component's InvokeAsync.
        /// It is awaited once when all scopes for the listener close, if matching notifications arrived.</param>
        /// <returns>An async scope supporting nested and overlapping deferrals.</returns>
        /// <remarks>Use for typed invalidations or incremental change batches: no payload is cached or lost
        /// by last-message replacement. Refresh callbacks must re-read the affected state. Critical events
        /// must not be included. Callback failures propagate from DisposeAsync; remaining callbacks still run.
        /// Existing notifier implementations remain source compatible but must opt in to this behavior.</remarks>
        IAsyncDisposable DeferUpdates(object listener, IEnumerable<string> types, Func<Task> refresh)
            => throw new NotSupportedException("This notifier does not implement update deferral. Use or derive from GXNotifierService.");
    }
}

