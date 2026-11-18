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
/// <summary>Application defaults and trusted additional theme registrations.</summary>
public sealed class GXThemeOptions
{
    /// <summary>Gets or sets the initial appearance settings used before saved preferences are loaded.</summary>
    public GXAppearanceSettings Defaults { get; set; } = new();
    /// <summary>Gets additional trusted theme registrations supplied by the application.</summary>
    public List<GXThemeDefinition> Themes { get; } = new();
    /// <summary>Gets or sets the local storage key used to persist appearance preferences.</summary>
    public string StorageKey { get; set; } = "Gurux.UI.Components:Appearance";
}
