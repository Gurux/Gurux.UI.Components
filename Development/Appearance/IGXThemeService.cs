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
/// <summary>Scoped appearance state. Register once per browser application or Server circuit.</summary>
public interface IGXThemeService
{
    /// <summary>Gets an independent copy of the current appearance preferences.</summary>
    GXAppearanceSettings Current { get; }
    /// <summary>Gets the requested layout with Auto resolved from the client platform.</summary>
    LayoutStyle EffectiveLayout { get; }
    /// <summary>Resolves Auto from the client platform and preserves explicit layout choices.</summary>
    /// <param name="requested">The explicitly requested layout or Auto for platform resolution.</param>
    /// <returns>An explicit layout, resolved from the platform when Auto was requested.</returns>
    LayoutStyle ResolveLayout(LayoutStyle requested);
    /// <summary>Gets the requested color mode with Auto resolved from the browser preference.</summary>
    ColorMode EffectiveColorMode { get; }
    /// <summary>Gets the layout density resolved from the current viewport width.</summary>
    ResponsiveLayoutMode ResponsiveMode => ResponsiveLayoutMode.Expanded;
    /// <summary>Gets whether the browser reports a coarse primary pointer.</summary>
    bool CoarsePointer => false;
    /// <summary>Resolves adaptive conventions using the optional parent layout override.</summary>
    /// <param name="layout">The requested adaptive layout.</param>
    /// <returns>Conventions for the effective layout, viewport density, and pointer precision.</returns>
    GXAdaptiveBehavior GetAdaptiveBehavior(LayoutStyle? layout = null) => GXAdaptiveBehavior.Create(ResolveLayout(layout ?? Current.Layout), ResponsiveMode, CoarsePointer);
    /// <summary>Updates viewport density and pointer precision, notifying subscribers when either changes.</summary>
    /// <param name="width">The viewport width in CSS pixels.</param>
    /// <param name="coarsePointer">Whether the browser primary pointer has coarse precision.</param>
    void UpdateViewport(int width, bool coarsePointer) { }
    /// <summary>Gets independent copies of the registered built-in and application themes.</summary>
    /// <returns>Independent copies of all registered themes.</returns>
    IReadOnlyList<GXThemeDefinition> GetThemes();
    /// <summary>Loads saved preferences once, retaining defaults and recording persistence failures when loading fails.</summary>
    /// <returns>The asynchronous preference initialization operation.</returns>
    Task InitializeAsync();
    /// <summary>Register or replace an application theme at runtime. Built-in themes cannot be replaced.</summary>
    void RegisterTheme(GXThemeDefinition theme);
    /// <summary>Selects a registered theme and persists the resulting appearance preferences.</summary>
    /// <param name="themeId">The identifier of a registered theme to select.</param>
    /// <returns>The asynchronous preference update operation.</returns>
    Task SetThemeAsync(string themeId);
    /// <summary>Selects a defined layout and persists the resulting appearance preferences.</summary>
    /// <param name="layout">The requested adaptive layout.</param>
    /// <returns>The asynchronous preference update operation.</returns>
    Task SetLayoutAsync(LayoutStyle layout);
    /// <summary>Selects a defined color mode and persists the resulting appearance preferences.</summary>
    /// <param name="mode">The requested light, dark, or automatic color scheme.</param>
    /// <returns>The asynchronous preference update operation.</returns>
    Task SetColorModeAsync(ColorMode mode);
    /// <summary>Restores the application default appearance preferences and persists them.</summary>
    /// <returns>The asynchronous preference reset operation.</returns>
    Task ResetAsync();
    /// <summary>Updates the client platform and dark mode preference and notifies subscribers of changes.</summary>
    /// <param name="platform">The browser platform name used to resolve automatic layouts.</param>
    /// <param name="prefersDark">Whether the browser prefers the dark color scheme.</param>
    void UpdateEnvironment(string? platform, bool prefersDark);
    /// <summary>Occurs when preferences, viewport information, or client appearance change.</summary>
    event EventHandler? AppearanceChanged;
    /// <summary>Gets the most recent preference loading or saving error, if any.</summary>
    Exception? PersistenceError { get; }
}
