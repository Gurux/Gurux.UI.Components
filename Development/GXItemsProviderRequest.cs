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
    /// Describes the paging, filtering, sorting, and cancellation options for an item request.
    /// </summary>
    public readonly struct GXItemsProviderRequest
    {
        /// <summary>
        /// Gets the zero-based offset of the first requested item.
        /// </summary>
        public int StartIndex
        {
            get;
        }

        /// <summary>
        /// Gets the requested item count; a provider may return fewer items.
        /// </summary>
        public int Count
        {
            get;
        }

        /// <summary>
        /// Gets whether the request includes removed items.
        /// </summary>
        public bool Removed
        {
            get;
        }

        /// <summary>
        /// Gets the text filter supplied to the items provider.
        /// </summary>
        public string? Filter
        {
            get;
        }

        /// <summary>
        /// Gets the property name used for sorting, or null to use the provider's default order.
        /// </summary>
        /// <remarks> Default order by is used if this is not set. </remarks>
        /// <seealso cref="Descending" />
        public string? OrderBy
        {
            get;
        }

        /// <summary>
        /// Gets whether the requested sort order is descending.
        /// </summary>
        /// <seealso cref="OrderBy" />
        public bool Descending
        {
            get;
        }

        /// <summary>
        /// Gets whether the request includes data belonging to all users.
        /// </summary>
        public bool ShowAllUserData
        {
            get;
        }


        /// <summary>
        /// Gets the token used to cancel the item request.
        /// </summary>
        public CancellationToken CancellationToken
        {
            get;
        }

        /// <summary>
        /// Creates an item request with paging, filtering, sorting, and cancellation options.
        /// </summary>
        /// <param name="startIndex">The start index of the data segment requested.</param>
        /// <param name="count">The requested number of items to be provided.</param>
        /// <param name="showAllUserData">Data from the all users is shown for the admin.</param>
        /// <param name="removed">Are removed items searched.</param>
        /// <param name="orderBy">Default order by is used if this is not set.</param>
        /// <param name="descending">Are values shown as descending order.</param>
        /// <param name="filter">Used filter.</param>
        /// <param name="cancellationToken">The <see cref="System.Threading.CancellationToken" /> used to relay cancellation of the request.</param>
        public GXItemsProviderRequest(int startIndex,
            int count,
            bool showAllUserData = true,
            bool removed = false,
            string? orderBy = null,
            bool descending = false,
            string? filter = null,
            CancellationToken cancellationToken = default)
        {
            StartIndex = startIndex;
            ShowAllUserData = showAllUserData;
            Count = count;
            OrderBy = orderBy;
            Descending = descending;
            Filter = filter;
            CancellationToken = cancellationToken;
            Removed = removed;
        }
    }
}