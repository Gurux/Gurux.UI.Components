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

namespace Gurux.UI.Components.Appearance;
/// <summary>Replaceable persistence for user appearance, independent of application databases.</summary>
public interface IGXAppearancePersistence
{
    /// <summary>Loads saved appearance settings, returning null when no settings have been stored.</summary>
    /// <returns>The saved preferences, or null when none are stored.</returns>
    Task<GXAppearanceSettings?> LoadAsync();
    /// <summary>Persists the supplied appearance settings for a later session.</summary>
    /// <param name="settings">The appearance preferences to validate or persist.</param>
    /// <returns>The asynchronous preference saving operation.</returns>
    Task SaveAsync(GXAppearanceSettings settings);
}
