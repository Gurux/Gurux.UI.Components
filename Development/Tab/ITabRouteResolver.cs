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

using Microsoft.AspNetCore.Components;

namespace Gurux.UI.Components.Tab;

/// <summary>Maps optional application route groups to persistent tab selections.</summary>
public interface ITabRouteResolver
{
    /// <summary>Identifies navigation retaining the current page.</summary>
    bool IsTabNavigation(string source, string target) => TabUrlState.IsTabNavigation(source, target);
    /// <summary>Resolves the active tab from the URL and the supplied fallback.</summary>
    string? GetActive(NavigationManager navigation, string? routeGroup, int depth, string? fallback);
    /// <summary>Builds the URL for a selected tab while retaining unrelated page parameters.</summary>
    string GetSelectionUrl(NavigationManager navigation, string? routeGroup, int depth, string tab);
}
