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

namespace Gurux.UI.Components.TemplateToken;

/// <summary>Displays plain text and safe links expressed with URL presentation tags.</summary>
/// <remarks>
/// <para>HTTP, HTTPS and application-relative links are supported. Labels and ordinary HTML
/// are displayed as escaped text; unsafe URLs and incomplete tags remain text. URL tags
/// are presentation markup and do not require a server token provider.
/// <see cref="GXNotificationsTile"/> also supports these tags in titles and descriptions.</para>
/// </remarks>
/// <example>
/// <para>Render an application-relative link from a URL tag.</para>
/// <code language="razor"><![CDATA[
/// @using Gurux.UI.Components.TemplateToken
///
/// <GXTemplateTokenText Text="[url:config/Manufacturer]Manufacturers[/url]" />
/// ]]></code>
/// </example>
public partial class GXTemplateTokenText
{
}
