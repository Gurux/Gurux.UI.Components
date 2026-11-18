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

using System.Globalization;
using System.Resources;

namespace Gurux.UI.Components.Appearance;

/// <summary>Looks up built-in appearance labels using the current UI culture.</summary>
internal static class GXAppearanceText
{
    /// <summary>Provides culture-aware access to the embedded appearance labels.</summary>
    private static readonly ResourceManager _resources = new("Gurux.UI.Components.Appearance.GXAppearanceResources", typeof(GXAppearanceText).Assembly);
    /// <summary>Looks up a localized appearance label, falling back to its resource key when absent.</summary>
    /// <param name="key">The resource key to resolve in the current UI culture.</param>
    /// <returns>The localized label, or the unchanged key when no resource exists.</returns>
    internal static string Get(string key) => _resources.GetString(key, CultureInfo.CurrentUICulture) ?? key;
}
