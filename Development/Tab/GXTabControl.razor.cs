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

namespace Gurux.UI.Components.Tab;

/// <summary>
/// Groups tab pages and optionally persists their selection in the page URL.
/// </summary>
/// <example>
/// <para>
/// URL persistence is optional:
/// </para>
/// <code language="razor"><![CDATA[
/// @using Gurux.UI.Components.Tab
/// 
/// <GXTabControl PersistSelectionInUrl="true">
///     <GXTabPage Path="General" Text="General">General settings</GXTabPage>
///     <GXTabPage Path="Advanced" Text="Advanced">Advanced settings</GXTabPage>
/// </GXTabControl>
/// ]]></code>
/// <para>
/// Selection uses <c>?tab=Advanced</c>, with <c>tab2</c>, <c>tab3</c>, etc. for nested controls.
/// Reload and browser history restore the selection. Changing a parent clears deeper
/// selection parameters while retaining other query parameters and the fragment.
/// Set stable, unique <c>GXTabPage.Path</c> values for bookmarks; a missing Path falls back
/// to Text. Removing the query key restores Active or InitialTab.
/// </para>
/// <para>
/// Ordinary controls keep direct Path navigation. With persistence enabled, Path
/// is the selection key and <c>NavigateOnSelection</c> does not cause direct navigation
/// to that path. <c>SelectionChanged</c> still runs once for each explicit selection.
/// Sticky headers default to true; nested controls using persistence default to
/// false. Set StickyHeader explicitly to override this.
/// </para>
/// <para>
/// Applications can supply <c>ITabRouteResolver</c> through the RouteResolver parameter
/// or dependency injection. RouteGroup is passed to that resolver for optional
/// canonical application routes. Without a resolver, selection uses only the
/// current page's query string; the UI library has no application-specific route map.
/// </para>
/// </example>
public partial class GXTabControl
{
}
