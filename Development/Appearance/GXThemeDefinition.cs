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

namespace Gurux.UI.Components;

/// <summary>A trusted application-registered Bootstrap-compatible stylesheet.</summary>
public class GXThemeDefinition
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string CssUrl { get; set; } = "";
    public bool IsDark { get; set; }
    /// <summary>False for themes whose fixed palette cannot fully follow color modes.</summary>
    public bool SupportsColorModes { get; set; } = true;
    public string? Description { get; set; }
    /// <summary>Optional trusted application preview color, not a full stylesheet.</summary>
    public string? PreviewColor { get; set; }

    /// <summary>Allow trusted HTTP(S) stylesheet URLs, including CDN assets. Disabled by default.</summary>
    public bool AllowExternalStylesheet { get; set; }
    /// <summary>False for additive application CSS that must retain the base Bootstrap stylesheet.</summary>
    public bool ReplaceBootstrap { get; set; } = true;
    /// <summary>CSS custom properties applied while this theme is active.</summary>
    public Dictionary<string, string> CssVariables { get; set; } = new(StringComparer.Ordinal);

    internal GXThemeDefinition Copy()
    {
        var copy = (GXThemeDefinition)MemberwiseClone();
        copy.CssVariables = new Dictionary<string, string>(CssVariables, StringComparer.Ordinal);
        return copy;
    }
}

/// <summary>Application defaults and trusted additional theme registrations.</summary>
public sealed class GXThemeOptions
{
    public GXAppearanceSettings Defaults { get; set; } = new();
    public List<GXThemeDefinition> Themes { get; } = new();
    public string StorageKey { get; set; } = "Gurux.UI.Components:Appearance";
}
