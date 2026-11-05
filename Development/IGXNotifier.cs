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
    }
}

