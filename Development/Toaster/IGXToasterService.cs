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

namespace Gurux.UI.Component.Toaster
{
    /// <summary>
    /// Manages transient toast messages and notifies consumers when their display state changes.
    /// </summary>
    public interface IGXToasterService
    {
        /// <summary>
        /// Gets or sets the stored-message limit used when adding toast messages.
        /// </summary>
        int MaxCount { get; set; }

        /// <summary>
        /// Adds a toast message to the service.
        /// </summary>
        void Add(GXToast toast);

        /// <summary>
        /// Gets whether any toast messages are currently stored.
        /// </summary>
        bool Any
        {
            get;
        }

        /// <summary>
        /// Removes expired messages and returns the stored toast list.
        /// </summary>
        List<GXToast> GetToasts();

        /// <summary>
        /// Removes the specified toast message.
        /// </summary>
        /// <param name="toast">Toast to remove.</param>
        void Remove(GXToast toast);

        /// <summary>
        /// Replaces the title filter used to reject newly added toast messages.
        /// </summary>
        void Filter(IEnumerable<string> filters);

        /// <summary>
        /// Occurs when stored toast messages change.
        /// </summary>
        event EventHandler? ToasterChanged;
        /// <summary>
        /// Occurs when the expiration timer changes the toast list.
        /// </summary>
        event EventHandler? ToasterTimerElapsed;


    }
}
