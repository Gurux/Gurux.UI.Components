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
    /// Tracks active operations, exposes their messages, and supports cancelling them together.
    /// </summary>
    public interface IGXProgress
    {
        /// <summary>
        /// Occurs when the active progress operations change.
        /// </summary>
        event Action? Changed;

        /// <summary>
        /// Gets whether at least one progress operation is active, including operations without a message.
        /// </summary>
        bool IsBusy { get; }

        /// <summary>
        /// Gets the nonempty messages of the currently active progress operations.
        /// </summary>
        IReadOnlyList<string> Tasks { get; }

        /// <summary>
        /// Requests cancellation of all currently active progress operations.
        /// </summary>
        Task CancelAllAsync();

        /// <summary>
        /// Registers a progress operation and returns a scope whose disposal ends that operation.
        /// </summary>
        GXProgressScope ProgressStart(string? message);

        /// <summary>
        /// Ends the progress operation with the specified id and notifies listeners when an operation was removed.
        /// </summary>
        void ProgressEnd(Guid id);
    }
}


