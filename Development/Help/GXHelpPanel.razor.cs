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

/// <summary>Opens contextual help alongside application content and loads the active provider request.</summary>
/// <remarks>
/// Host one panel alongside page content and register scoped help, progress and notification services. Closing or replacing a request cancels its pending provider load.
/// </remarks>
/// <example>
/// <para>Place one <c>GXHelpPanel</c> in the application layout alongside the page content:</para>
/// <code language="razor"><![CDATA[
/// @using Gurux.UI.Components.Help
/// 
/// <GXHelpPanel HelpPath="/help">
///     <EmptyContent Context="request">No instructions are available for @request.Topic.</EmptyContent>
/// </GXHelpPanel>
/// ]]></code>
/// <para>Register <c>GXHelpService</c> and the application&#39;s <c>IGXHelpProvider</c> implementations as described below, and set the service&#39;s <c>BaseAddress</c> to <c>/help</c>. Existing <c>GXHelp</c> links then open the panel in the same application session. The panel follows the active topic, preserves its version and culture, cancels unfinished loading on close, and uses <c>GXHelpContentView</c> with the existing <c>GXMarkdownView</c> renderer. Its JavaScript is supplied automatically by the component library.</para>
/// <para>The panel uses the shared progress and notification services. Register <c>IGXProgress</c> with <c>GXProgressService</c> and <c>IGXNotificationService</c> with <c>GXNotificationService</c> in the same application scope. Provider loading creates a cancellable progress operation; failures are reported through notifications. Closing or replacing a request ends its progress operation, including when a provider takes longer to observe cancellation. Cancellation does not create an error notification.</para>
/// <para>Use <c>ContentTemplate</c> instead of the default provider loader when the application already owns help rendering or a manual fallback. It receives a <c>GXHelpRequest</c>; the panel still handles opening, closing and contextual links. <c>EmptyContent</c> customizes missing-provider content when using the default loader. Title, width, progress messages, error notification titles, unavailable text and button/link labels are configurable. Panels have independent generated IDs; provide distinct IDs and help paths when hosting more than one. Modifier clicks and external links retain normal browser behavior. Documentation anchors scroll within their panel without changing the open editor&#39;s URL.</para>
/// </example>
public partial class GXHelpPanel
{
}
