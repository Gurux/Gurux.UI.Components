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

/// <summary>Loads and displays RSS or Atom entries using the syndication feed model.</summary>
/// <remarks>
/// <para>The defaults are ten entries with images and descriptions enabled. Entries keep the feed's order.
/// Images are read from image enclosures and Media RSS content or thumbnails. Descriptions are displayed
/// as plain text. Changing <see cref="Url"/> loads the new feed and cancels the previous request.
/// Loading failures are shown in the component.</para>
/// <para>In Blazor WebAssembly, the feed server must allow cross-origin requests, or the application
/// must provide a server endpoint that fetches the feed. <see cref="ProxyUrl"/> specifies an optional
/// same-origin GET endpoint that accepts the absolute target URL in its <c>url</c> query parameter
/// and returns the original content type. Relative proxy paths resolve against the application's
/// base address. Displayed feed images also pass through this endpoint.</para>
/// <para>When redirects change the upstream base URL, the proxy should return an
/// <c>X-Gurux-Source-Url</c> header containing the final source URL so relative links resolve correctly.
/// The AMI server supplies a compatible endpoint with configurable allowed origins, caching and
/// request limits. Other hosts can implement the same contract. Same-origin requests support cookie
/// authentication. Omitting <see cref="ProxyUrl"/> keeps direct requests.</para>
/// </remarks>
/// <example>
/// <para>Register an HTTP client in the application if one is not already available.</para>
/// <code language="csharp"><![CDATA[
/// builder.Services.AddScoped(_ => new HttpClient());
/// ]]></code>
/// <para>Display the latest RSS or Atom entries.</para>
/// <code language="razor"><![CDATA[
/// @using Gurux.UI.Components
///
/// <GXFeedViewer Url="https://gurux.fi/rss.xml"
///               MaxItems="10"
///               ShowImages="true"
///               ShowDescription="true" />
/// ]]></code>
/// </example>
/// <example>
/// <para>Fetch a feed through the application's same-origin proxy.</para>
/// <code language="razor"><![CDATA[
/// <GXFeedViewer Url="https://gurux.fi/rss.xml" ProxyUrl="api/HttpProxy" />
/// ]]></code>
/// </example>
public partial class GXFeedViewer
{
}
