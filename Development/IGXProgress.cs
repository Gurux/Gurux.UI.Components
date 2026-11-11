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
        bool IsBusy
        {
            get;
        }

        /// <summary>
        /// Gets the nonempty messages of the currently active progress operations.
        /// </summary>
        IReadOnlyList<string> Tasks
        {
            get;
        }

        /// <summary>
        /// Requests cancellation of all currently active progress operations.
        /// </summary>
        Task CancelAllAsync();

        /// <summary>
        /// Gets snapshots of the active operations and their progress.
        /// </summary>
        IReadOnlyList<GXProgressItem> Items { get; }

        /// <summary>
        /// Updates an active operation and notifies listeners.
        /// A total of zero indicates that progress is unknown.
        /// </summary>
        void ProgressUpdate(Guid id, int completed, int total);

        /// <summary>
        /// Replaces progress with nonnegative completed counts grouped by color.
        /// A total of zero indicates unknown progress.
        /// </summary>
        void ProgressUpdate(Guid id, IEnumerable<(Color Color, int Completed)> completed, int total);

        /// <summary>
        /// Registers a progress operation and returns a scope whose disposal ends that operation.
        /// </summary>
        /// <param name="message">The message describing the operation.</param>
        /// <param name="completed">The number of completed work items.</param>
        /// <param name="total">The total number of work items, or zero when unknown.</param>
        GXProgressScope ProgressStart(string? message, int completed = 0, int total = 0);

        /// <summary>
        /// Registers a progress operation with completed counts grouped by color.
        /// </summary>
        /// <param name="message">The message describing the operation.</param>
        /// <param name="completed">The nonnegative completed counts by color, in display order.</param>
        /// <param name="total">The total number of work items, or zero when unknown.</param>
        /// <returns>A scope whose disposal ends the operation.</returns>
        GXProgressScope ProgressStart(string? message, IEnumerable<(Color Color, int Completed)> completed, int total = 0);

        /// <summary>
        /// Ends the progress operation with the specified id and notifies listeners when an operation was removed.
        /// </summary>
        void ProgressEnd(Guid id);
    }
}


