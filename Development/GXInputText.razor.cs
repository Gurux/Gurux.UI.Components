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

/// <summary>Edits a string with two-way binding, optional form tracking and browser persistence.</summary>
/// <remarks>
/// <para>Register <c>IGXLocalStorage</c> with <c>GXLocalStorage</c> in the application service collection.
/// Additional HTML attributes such as <c>class</c> are forwarded to the input element.</para>
/// </remarks>
/// <example>
/// <para>Bind a profile field to component state. Use application-localized captions and placeholder text.</para>
/// <code language="razor"><![CDATA[
/// @page "/profile"
/// @using Gurux.UI.Components
///
/// <h1>Profile</h1>
///
/// <GXInputText @bind-Value="_name"
///              Placeholder="Your name"
///              class="form-control" />
///
/// <p>Hello, @_name!</p>
///
/// @code {
///     private string? _name;
/// }
/// ]]></code>
/// </example>
public partial class GXInputText
{
}
