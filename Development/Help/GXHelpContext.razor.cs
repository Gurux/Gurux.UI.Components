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

/// <summary>Registers a contextual help request for the lifetime of the component.</summary>
/// <remarks>
/// Disposing this component restores the previous contextual request, including its version and culture. Providers interpret versions and choose their default culture when none is specified.
/// </remarks>
/// <example>
/// <para>Implement <c>IGXHelpProvider</c> to supply help from embedded resources, files or a remote service. The provider receives a stable topic, an optional subject version and an optional culture. Return null when your provider does not handle the request. The shared service calls providers in registration order and returns the first non-null result. Errors and cancellation propagate to the caller.</para>
/// <code language="csharp"><![CDATA[
/// using Gurux.UI.Components;
/// using Gurux.UI.Components.Enums;
/// using Gurux.UI.Components.Help;
/// 
/// public sealed class ExampleHelpProvider : IGXHelpProvider
/// {
///     public Task<GXHelpContent?> GetHelpAsync(
///         GXHelpRequest request, CancellationToken cancellationToken = default)
///     {
///         cancellationToken.ThrowIfCancellationRequested();
///         GXHelpContent? content = request.Topic == "module.example"
///             ? new GXHelpContent(HelpKind.Markdown, "# Example module\nModule instructions.")
///             : null;
///         return Task.FromResult(content);
///     }
/// }
/// ]]></code>
/// <para>Register the service and each provider for the same application scope:</para>
/// <code language="csharp"><![CDATA[
/// builder.Services.AddScoped<IGXHelpProvider, ExampleHelpProvider>();
/// builder.Services.AddScoped<GXHelpService>();
/// ]]></code>
/// <para>A component can register a topic for its lifetime, including the version of the object being described and the requested language:</para>
/// <code language="razor"><![CDATA[
/// @using Gurux.UI.Components.Help
/// 
/// <GXHelpContext Topic="module.example" Version="2" Culture="en" />
/// ]]></code>
/// <para>Inject <c>GXHelpService</c> into the host&#39;s help presenter and load the active request on user demand. Alternatively, supply a request directly without changing the active context:</para>
/// <code language="csharp"><![CDATA[
/// GXHelpContent? active = await helpService.GetHelpAsync(cancellationToken);
/// GXHelpContent? specific = await helpService.GetHelpAsync(
///     new GXHelpRequest("module.example", Version: "2", Culture: "en"),
///     cancellationToken);
/// ]]></code>
/// <para><c>GXHelpContent.Kind</c> tells the presenter whether <c>Value</c> is a URL (<c>Link</c>), plain text (<c>Text</c>) or Markdown (<c>Markdown</c>). Render Markdown using <c>GXMarkdownView Content=&quot;@content.Value&quot;</c>. Text content should be displayed as text, and Link content should be offered as a documentation link. Storage paths belong inside a provider; <c>Value</c> contains the actual text or a URL.</para>
/// <para><c>GXHelpService.Request</c> exposes the complete active request. Its <c>Changed</c> event also fires when only the version or culture changes. Removing a component context restores the previous request, including its version and culture. The existing <c>SetTopic</c>, <c>Topic</c>, <c>HelpUrl</c> and <c>GXHelp</c> documentation link continue to work. <c>GXHelp</c> constructs a link from the configured base address; provider content is retrieved explicitly by the host&#39;s help presenter.</para>
/// <para>The version is interpreted by the provider: it can be a COSEM class version or a feature version, rather than the module package version. A null culture requests the provider&#39;s default language. The service does not cache content; a provider may cache immutable help using the complete <c>GXHelpRequest</c> as the cache key so versions and languages do not mix. The records can also be serialized for an HTTP adapter when help is provided by a server module.</para>
/// </example>
public partial class GXHelpContext
{
}
