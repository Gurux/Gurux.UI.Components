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

/// <summary>Configurable dialog action conventions; Auto follows the selected style.</summary>
public enum GXDialogButtonOrder { Auto, PrimaryFirst, PrimaryLast }
/// <summary>Presentation is independent of action severity. Sheets are explicitly opted into.</summary>
public enum GXDialogPresentation { Auto, Dialog, BottomSheet }
/// <summary>Viewport modes, independent of client operating system.</summary>
public enum GXResponsiveLayoutMode { Compact, Medium, Expanded }
public enum GXNavigationPresentation { Default, Sidebar, BottomTabs, Rail, Drawer }
public enum GXMenuPresentation { Default, Popup, ActionSheet }
public enum GXNotificationPresentation { Default, InfoBar, Banner, Snackbar }

/// <summary>Immutable shared UI conventions resolved by the existing scoped theme service.</summary>
public sealed record GXAdaptiveBehavior(
    GXLayoutStyle Layout,
    GXResponsiveLayoutMode ResponsiveMode,
    bool CoarsePointer,
    GXDialogButtonOrder DialogButtonOrder,
    GXNavigationPresentation Navigation,
    GXMenuPresentation Menu,
    GXNotificationPresentation Notification)
{
    public static GXAdaptiveBehavior Default { get; } = Create(GXLayoutStyle.Default, GXResponsiveLayoutMode.Expanded, false);

    public static GXAdaptiveBehavior Create(GXLayoutStyle layout, GXResponsiveLayoutMode mode, bool coarsePointer)
    {
        var compact = mode == GXResponsiveLayoutMode.Compact;
        return new(layout, mode, coarsePointer,
            layout is GXLayoutStyle.Cupertino or GXLayoutStyle.Material ? GXDialogButtonOrder.PrimaryLast : GXDialogButtonOrder.PrimaryFirst,
            layout switch
            {
                GXLayoutStyle.Fluent => compact ? GXNavigationPresentation.Drawer : GXNavigationPresentation.Sidebar,
                GXLayoutStyle.Cupertino => compact ? GXNavigationPresentation.BottomTabs : GXNavigationPresentation.Sidebar,
                GXLayoutStyle.Material => compact ? GXNavigationPresentation.BottomTabs : mode == GXResponsiveLayoutMode.Medium ? GXNavigationPresentation.Rail : GXNavigationPresentation.Sidebar,
                _ => GXNavigationPresentation.Default
            },
            layout == GXLayoutStyle.Default ? GXMenuPresentation.Default : compact && layout is GXLayoutStyle.Cupertino or GXLayoutStyle.Material ? GXMenuPresentation.ActionSheet : GXMenuPresentation.Popup,
            layout switch
            {
                GXLayoutStyle.Fluent => GXNotificationPresentation.InfoBar,
                GXLayoutStyle.Cupertino => GXNotificationPresentation.Banner,
                GXLayoutStyle.Material => GXNotificationPresentation.Snackbar,
                _ => GXNotificationPresentation.Default
            });
    }
}
