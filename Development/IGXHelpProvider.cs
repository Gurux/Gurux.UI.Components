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

using Gurux.UI.Components.Help;

namespace Gurux.UI.Components;

/// <summary>Provides help content for the topics owned by a component library or module.</summary>
public interface IGXHelpProvider
{
    /// <summary>Loads help for a topic, object version and culture.</summary>
    /// <param name="request">Identifies the requested topic and optional version and culture.</param>
    /// <param name="cancellationToken">Token used to cancel loading.</param>
    /// <returns>Help content, or null if this provider does not handle the request.</returns>
    /// <remarks>
    /// Only null permits resolution to continue with another provider. Loading errors and cancellation
    /// propagate to the caller. Implementations may read embedded resources, files or remote services.
    /// </remarks>
    Task<GXHelpContent?> GetHelpAsync(GXHelpRequest request, CancellationToken cancellationToken = default);
}
