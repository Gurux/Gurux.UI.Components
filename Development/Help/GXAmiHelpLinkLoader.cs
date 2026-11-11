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

using System.Net;
using System.Net.Http.Json;

namespace Gurux.UI.Components.Help;

/// <summary>Loads official HTML through the authenticated same-origin proxy.</summary>
public sealed class GXAmiHelpLinkLoader(HttpClient http) : IGXHelpLinkLoader
{
    /// <inheritdoc />
    public async Task<GXHelpContent?> LoadAsync(string address, CancellationToken cancellationToken = default)
    {
        using var response = await http.GetAsync("api/Help/Article?address=" + Uri.EscapeDataString(address), cancellationToken);
        if (response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.BadRequest)
        {
            return null;
        }
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<GXHelpContent>(cancellationToken: cancellationToken);
    }
}
