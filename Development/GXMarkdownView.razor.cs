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

namespace Gurux.UI.Components;

/// <summary>Renders inline or downloaded Markdown with section links and optional syntax highlighting.</summary>
/// <example>
/// <para>Display Markdown stored in the application or received from an API.</para>
/// <code language="razor"><![CDATA[
/// @page "/help"
/// @using Gurux.UI.Components
///
/// <GXMarkdownView Content="@Markdown" />
///
/// @code {
///     private const string Markdown = """
///         ## Welcome
///
///         This page supports **bold text**, links, lists, and fenced code blocks.
///         """;
/// }
/// ]]></code>
/// </example>
public partial class GXMarkdownView
{
}
