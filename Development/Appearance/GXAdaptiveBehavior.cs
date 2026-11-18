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
/// <summary>Immutable shared UI conventions resolved by the existing scoped theme service.</summary>
/// <param name="Layout">The effective layout used to choose platform conventions.</param>
/// <param name="ResponsiveMode">The viewport size class used to choose navigation density.</param>
/// <param name="CoarsePointer">Whether the primary pointer has coarse precision.</param>
/// <param name="DialogButtonOrder">The order of primary and cancel actions.</param>
/// <param name="Navigation">The navigation container appropriate for the layout and viewport.</param>
/// <param name="Menu">The presentation used for menu commands.</param>
/// <param name="Notification">The presentation used for notifications.</param>
public sealed record GXAdaptiveBehavior(
    LayoutStyle Layout,
    ResponsiveLayoutMode ResponsiveMode,
    bool CoarsePointer,
    DialogButtonOrder DialogButtonOrder,
    NavigationPresentation Navigation,
    MenuPresentation Menu,
    NotificationPresentation Notification)
{
    /// <summary>Gets the conventions for the default layout on an expanded viewport with a fine pointer.</summary>
    public static GXAdaptiveBehavior Default { get; } = Create(LayoutStyle.Default, ResponsiveLayoutMode.Expanded, false);

    /// <summary>Resolves dialog, navigation, menu, and notification conventions for the selected layout and viewport.</summary>
    /// <param name="layout">The requested adaptive layout.</param>
    /// <param name="mode">The viewport size class used to resolve responsive conventions.</param>
    /// <param name="coarsePointer">Whether the browser primary pointer has coarse precision.</param>
    /// <returns>The immutable conventions resolved for the supplied environment.</returns>
    public static GXAdaptiveBehavior Create(LayoutStyle layout, ResponsiveLayoutMode mode, bool coarsePointer)
    {
        var compact = mode == ResponsiveLayoutMode.Compact;
        return new(layout, mode, coarsePointer,
            layout is LayoutStyle.Cupertino or LayoutStyle.Material ? DialogButtonOrder.PrimaryLast : DialogButtonOrder.PrimaryFirst,
            layout switch
            {
                LayoutStyle.Fluent => compact ? NavigationPresentation.Drawer : NavigationPresentation.Sidebar,
                LayoutStyle.Cupertino => compact ? NavigationPresentation.BottomTabs : NavigationPresentation.Sidebar,
                LayoutStyle.Material => compact ? NavigationPresentation.BottomTabs : mode == ResponsiveLayoutMode.Medium ? NavigationPresentation.Rail : NavigationPresentation.Sidebar,
                _ => NavigationPresentation.Default
            },
            layout == LayoutStyle.Default ? MenuPresentation.Default : compact && layout is LayoutStyle.Cupertino or LayoutStyle.Material ? MenuPresentation.ActionSheet : MenuPresentation.Popup,
            layout switch
            {
                LayoutStyle.Fluent => NotificationPresentation.InfoBar,
                LayoutStyle.Cupertino => NotificationPresentation.Banner,
                LayoutStyle.Material => NotificationPresentation.Snackbar,
                _ => NotificationPresentation.Default
            });
    }
}
