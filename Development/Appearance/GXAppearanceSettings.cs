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

namespace Gurux.UI.Components.Appearance;
/// <summary>Independent user appearance preferences.</summary>
public class GXAppearanceSettings
{
    /// <summary>Gets or sets the requested layout, resolving Auto from the client platform.</summary>
    public LayoutStyle Layout { get; set; } = LayoutStyle.Default;
    /// <summary>Gets or sets the registered Bootstrap theme identifier.</summary>
    public string BootstrapTheme { get; set; } = "default";
    /// <summary>Gets or sets the requested color mode, resolving Auto from the browser preference.</summary>
    public ColorMode ColorMode { get; set; } = ColorMode.Auto;

    /// <summary>Creates an independent copy so callers cannot mutate the stored settings or theme variables.</summary>
    /// <returns>An independent copy of these appearance preferences.</returns>
    internal GXAppearanceSettings Copy() => new() { Layout = Layout, BootstrapTheme = BootstrapTheme, ColorMode = ColorMode };
}
