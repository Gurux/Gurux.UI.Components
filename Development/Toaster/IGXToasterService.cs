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
        int MaxCount
        {
            get;
            set;
        }

        /// <summary>
        /// Adds a toast message to the service.
        /// </summary>
        void Add(GXToast toast);

        /// <summary>
        /// Gets whether any toast messages are currently stored.
        /// </summary>
        bool Any { get; }

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
