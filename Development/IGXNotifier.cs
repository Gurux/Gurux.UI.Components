//
// --------------------------------------------------------------------------
//  Gurux Ltd
//
//
//
// Filename:        $HeadURL$
//
// Version:         $Revision$,
//                  $Date$
//                  $Author$
//
// Copyright (c) Gurux Ltd
//
//---------------------------------------------------------------------------
//
//  DESCRIPTION
//
// This file is a part of Gurux Device Framework.
//
// Gurux Device Framework is Open Source software; you can redistribute it
// and/or modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; version 2 of the License.
// Gurux Device Framework is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU General Public License for more details.
//
// This code is licensed under the GNU General Public License v2.
// Full text may be retrieved at http://www.gnu.org/licenses/gpl-2.0.txt
//---------------------------------------------------------------------------

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

