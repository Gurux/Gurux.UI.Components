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

using System.Text.Json.Serialization;

namespace Gurux.UI.Components;

/// <summary>Requested color scheme; Auto follows the browser preference.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<GXColorMode>))]
public enum GXColorMode
{
    Auto,
    Light,
    Dark
}

/// <summary>Independent user appearance preferences.</summary>
public class GXAppearanceSettings
{
    public GXLayoutStyle Layout { get; set; } = GXLayoutStyle.Default;
    public string BootstrapTheme { get; set; } = "default";
    public GXColorMode ColorMode { get; set; } = GXColorMode.Auto;

    internal GXAppearanceSettings Copy() => new() { Layout = Layout, BootstrapTheme = BootstrapTheme, ColorMode = ColorMode };
}
