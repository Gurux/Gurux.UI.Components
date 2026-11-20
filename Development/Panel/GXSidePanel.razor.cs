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

namespace Gurux.UI.Components.Panel;

/// <summary>Displays arbitrary Blazor content in a closable side panel.</summary>
/// <remarks>
/// Child content exists only while the panel is open. Closing the panel disposes its content and restores focus through the existing dialog focus stack.
/// </remarks>
/// <example>
/// <code language="razor"><![CDATA[
/// @using Gurux.UI.Components.Panel
/// 
/// <button type="button" @onclick="() => _detailsOpen = true">Details</button>
/// <GXSidePanel @bind-Open="_detailsOpen" Title="Details" Width="36rem">
///     <p>Place a form, table, component or other Blazor content here.</p>
/// </GXSidePanel>
/// 
/// @code
/// {
///     private bool _detailsOpen;
/// }
/// ]]></code>
/// <para><c>HeaderContent</c> and <c>FooterContent</c> add application-owned actions. Supply <c>CloseButtonTitle</c> to localize the close button. Content is mounted only while open and disposed when closed. Escape closes the panel. Normal panels leave the underlying editor usable; panels opened above a modal dialog join the existing dialog focus stack and restore focus on close. The panel uses theme colors and fits narrow viewports.</para>
/// </example>
public partial class GXSidePanel
{
}
