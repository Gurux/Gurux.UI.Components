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

namespace Gurux.UI.Components.Help;

/// <summary>Presents provider help content with the renderer appropriate to its content kind.</summary>
/// <remarks>
/// Applications select and load topics, authenticate requests and supply fallback content. This view can be hosted in a side panel or any other container.
/// </remarks>
/// <example>
/// <code language="razor"><![CDATA[
/// @using Gurux.UI.Components.Help
/// 
/// <GXHelpContentView Content="helpContent">
///     <EmptyContent>No instructions are available for this item.</EmptyContent>
/// </GXHelpContentView>
/// ]]></code>
/// <para><c>helpContent</c> is a <c>GXHelpContent</c> returned by an <c>IGXHelpProvider</c>. <c>GXHelpContentView</c> delegates Markdown to the existing <c>GXMarkdownView</c>, encodes plain text and displays HTTP/HTTPS or relative documentation links. Unsupported link schemes are displayed as text. Applications own topic selection, loading, authentication and fallback content. Use this component inside <c>GXSidePanel</c> or any other container. No second Markdown implementation is introduced.</para>
/// </example>
public partial class GXHelpContentView
{
}
