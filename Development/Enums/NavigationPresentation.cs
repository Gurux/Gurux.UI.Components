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
namespace Gurux.UI.Components.Enums;

/// <summary>Selects how adaptive navigation is presented at the current viewport width.</summary>
public enum NavigationPresentation
{
    /// <summary>Uses the existing navigation presentation without an adaptive replacement.</summary>
    Default,
    /// <summary>Displays navigation in a persistent side region.</summary>
    Sidebar,
    /// <summary>Displays navigation as tabs at the bottom of the viewport.</summary>
    BottomTabs,
    /// <summary>Displays navigation in a compact vertical rail.</summary>
    Rail,
    /// <summary>Displays navigation in a dismissible drawer.</summary>
    Drawer
}
