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

namespace Gurux.UI.Components;

/// <summary>
/// Displays declarative commands as a row of buttons or an expandable dropdown.
/// </summary>
/// <example>
/// <code language="razor"><![CDATA[
/// <GXMenuControl Dropdown="true" Text="File">
///     <GXMenuEntry Text="Open" OnClick="OpenAsync" />
///     <GXMenuEntry Text="Save" OnClick="SaveAsync" ReadOnly="@(!canSave)" />
/// </GXMenuControl>
/// ]]></code>
/// <para>
/// Alternatively, supply <c>MenuItems</c> as a <c>List&lt;GXMenuEntry&gt;</c>; replacing the list updates
/// the commands. <c>Icon</c> sets the toggle's resource icon, <c>ReadOnly</c> disables the whole
/// control, and <c>RightCorner</c> / <c>Bottom</c> align the popup. Without <c>Dropdown="true"</c>,
/// the existing row of buttons is preserved. A dropdown closes when a command is
/// selected, Escape is pressed, its toggle is clicked again, or the user clicks
/// outside it. The component uses Blazor events and scoped CSS; Bootstrap JavaScript
/// is not required. Ensure the application loads the generated scoped CSS bundle.
/// </para>
/// <code language="powershell"><![CDATA[
/// dotnet run --project Tests/MenuDropdowns/MenuDropdowns.Checks.csproj
/// dotnet run --project Tests/MenuAuthorization/MenuAuthorization.Checks.csproj
/// dotnet run --project Tests/ContextMenuAuthorization/ContextMenuAuthorization.Checks.csproj
/// ]]></code>
/// </example>
public partial class GXMenuControl
{
}
