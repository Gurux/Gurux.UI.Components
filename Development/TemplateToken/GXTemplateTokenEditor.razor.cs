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

namespace Gurux.UI.Components.TemplateToken;

/// <summary>
/// Edits a text template with token discovery, validation and a resolved text preview.
/// </summary>
/// <example>
/// <code language="razor"><![CDATA[
/// @using Gurux.UI.Components.TemplateToken
/// 
/// <GXTemplateTokenEditor @bind-Value="Model.Message" />
/// ]]></code>
/// <para>
/// Use inside an EditForm. The component lists available tokens, appends selected
/// <c>[namespace:name]</c> tokens, and provides a plain-text View dialog. Optional TargetType
/// and TargetId parameters scope token discovery and resolution. Register an
/// application implementation of <c>ITemplateTokenEditorService</c> in dependency
/// injection. The service receives <c>TemplateTokenEditorContext</c> (target and culture)
/// and returns generic <c>TemplateTokenDefinition</c> values. No AMI project or fixed API
/// endpoint is required by the UI library. Preview output is rendered as text.
/// </para>
/// <para>
/// TemplateTokenEditor also requires IGXProgress and IGXNotificationService in DI.
/// Catalog loading, preview and validation use cancellable progress scopes; request
/// failures are reported through the shared notification service. Validate refreshes
/// the available definitions and checks token syntax and names without changing the
/// template. It reports success or distinct validation errors through notifications.
/// Preview resolves the actual values. IGXNotifier is the event subscription contract
/// and is not used to display errors.
/// </para>
/// <para>
/// Template token definitions can supply an optional ExampleValue. Validate checks
/// syntax and names. View opens a dialog with actual values; without a TargetId,
/// unavailable known tokens may use their examples. Such a dialog is titled Example
/// text. Unknown references remain visible. ExampleValue is never a runtime default
/// and neither action modifies the bound template. Each original token reference
/// is resolved once per distinct name; replacement text is not expanded again.
/// </para>
/// <para>
/// TemplateTokenEditor supports an optional Page scope and Values dictionary for
/// page-specific draft values. Both are forwarded in TemplateTokenEditorContext.
/// Changing either invalidates the open View dialog and reloads token definitions.
/// The application decides how to discover and resolve tokens for each Page.
/// </para>
/// <para>
/// TemplateTokenEditor has two tabs. Editor contains the text field, searchable
/// token insertion, Validate and View actions. Tokens lists all definitions available
/// in the current page/target context using GXTable. Its immediate filter searches
/// names, descriptions and example values, ignoring case. Table filtering does not
/// modify the template or persist search settings in browser storage.
/// </para>
/// </example>
public partial class GXTemplateTokenEditor
{
}
