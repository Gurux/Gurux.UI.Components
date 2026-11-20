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
/// Displays registered commands and dropdown menus and guards navigation away from modified forms.
/// </summary>
/// <example>
/// <para>
/// Register scoped menu and progress services in Program.cs:
/// </para>
/// <code language="csharp"><![CDATA[
/// using Gurux.UI.Components;
/// using Gurux.UI.Components.Enums;
/// 
/// builder.Services.AddScoped<IGXMenuService, GXMenuService>();
/// builder.Services.AddScoped<IGXProgress, GXProgressService>();
/// ]]></code>
/// <para>
/// The callbacks below are EventCallback values supplied by the owning component. Retain registrations for the page lifetime and dispose them when the page is disposed; the using declarations illustrate their ownership.
/// </para>
/// <para>
/// Register <c>IGXMenuService</c> with the scoped <c>GXMenuService</c> implementation. Menus
/// are independent for each <c>(MenuLocation, MenuKind)</c> pair:
/// </para>
/// <para>
/// - Locations: <c>Top</c>, <c>Left</c>, <c>Right</c>, <c>Bottom</c>, <c>Footer</c>.
/// - Kinds: <c>Navigation</c>, <c>Tabs</c>, <c>Configuration</c>, <c>MenuBar</c>, <c>ContextMenu</c>, <c>Dropdown</c>, <c>Overflow</c>.
/// </para>
/// <para>
/// Both enums are in <c>Gurux.UI.Components.Enums</c>. <c>MenuKind.All</c> is reserved for
/// regeneration and cannot address a rendered menu.
/// </para>
/// <code language="csharp"><![CDATA[
/// using var pageMenu = new GXPageMenu(menuService,
///     MenuLocation.Left, MenuKind.Navigation);
/// pageMenu.AddMenuItems(new GXMenuItem("Devices", "device", openDevices));
/// 
/// using var footer = menuService.RegisterMenu(footerCommands,
///     location: MenuLocation.Footer, kind: MenuKind.Overflow);
/// var visible = menuService.GetItems(MenuLocation.Footer, MenuKind.Overflow);
/// ]]></code>
/// <para>
/// Each pair has its own command ownership and EditContext. Use GetEditContext /
/// SetEditContext to access a chosen pair. Default Items, EditContext, AddMenuItems
/// and GXPageMenu calls target <c>Top / MenuBar</c>. Disposing an old registration cannot
/// clear its replacement or another location.
/// </para>
/// <para>
/// The existing GXMenuBar toolbar accepts Location and Kind parameters and reads only
/// that pair. Kinds identify separate menus; they do not automatically provide new
/// navigation/dropdown/context markup or insert regions into an application's layout.
/// Applications place the appropriate renderer where needed.
/// </para>
/// <para>
/// Dropdown commands
/// </para>
/// <para>
/// <c>GXMenuBar</c> renders a command with nonempty <c>GXMenuItem.Children</c> as a labelled
/// <c>GXMenuControl</c> dropdown. Its children are command buttons; the parent callback is
/// not invoked when opening the dropdown. Items without children remain direct
/// commands. Children can contain further child commands for nested submenus.
/// Use <c>new GXMenuItem { IsSeparator = true }</c> to separate command groups.
/// </para>
/// <code language="csharp"><![CDATA[
/// using var pageMenu = new GXPageMenu(menuService);
/// pageMenu.AddMenuItems(new GXMenuItem
/// {
///     Text = "File",
///     Children =
///     [
///         new GXMenuItem("Open", null, openCallback),
///         new GXMenuItem("Save", null, saveCallback, EnableStyle.Modified)
///     ]
/// });
/// ]]></code>
/// <code language="razor"><![CDATA[
/// <GXMenuBar />
/// ]]></code>
/// <para>
/// Both the parent and child commands respect the menu's edit context and progress
/// state. Starting a progress operation closes and disables the dropdown. Register
/// <c>IGXProgress</c> with <c>GXProgressService</c> as well as the menu service.
/// </para>
/// <para>
/// Commands in <c>GXMenuControl</c>, <c>GXContextMenuController</c>, and <c>GXMenuBar</c> enforce their
/// named <c>Policy</c> both when rendering and immediately before invoking a callback.
/// Use <c>GXMenuEntry.Policy</c>, <c>GXContextMenuItem.Policy</c>, or <c>GXMenuItem.Policy</c> respectively.
/// Commands without a policy retain their existing behavior. A protected command is
/// hidden when authorization services are unavailable or the policy is unknown.
/// Nested commands require their ancestor policies as well. Authentication changes
/// and <c>IGXAuthorizationRefreshSource.Changed</c> refresh visibility and invalidate pending
/// authorization decisions. Server endpoints must also enforce their own policies.
/// </para>
/// <para>
/// The <c>Tests/MenuAuthorization</c> and <c>Tests/ContextMenuAuthorization</c> executable checks
/// use the Blazor renderer and named authorization policies to cover visibility,
/// invocation, permission changes, nested commands, and asynchronous invalidation.
/// </para>
/// <para>
/// Clear(location, kind) clears one pair. ClearForNavigation(location, kind) returns
/// a one-use restoration action for that pair; each toolbar handles its own address.
/// ClearForNavigation() addresses all registered pairs. Canceled navigation restores
/// only unchanged pairs, preserving newly registered destination menus. Multiple
/// renderers for the same pair can safely clear it during the same navigation.
/// The service follows the UI renderer's synchronization context.
/// </para>
/// <para>
/// The former top-specific interface and service names have been removed; consumers
/// use IGXMenuService and GXMenuService.
/// </para>
/// <para>
/// Multiple menus in one location can use the existing <c>GXMenu.Id</c> as <c>MenuId</c>.
/// The optional ID defaults to null for the original shared menu. No additional
/// persisted field is required. Register commands with the same ID as the renderer:
/// </para>
/// <code language="csharp"><![CDATA[
/// using var commands = new GXPageMenu(menuService, menuId: definition.Id);
/// commands.AddMenuItems(new GXMenuItem("Clear", "clear", clearCallback));
/// ]]></code>
/// <para>
/// Two menu bars can be stacked in a layout:
/// </para>
/// <code language="razor"><![CDATA[
/// <div class="d-flex flex-column align-items-start">
///     <GXMenuBar MenuId="@firstMenu.Id" />
///     <GXMenuBar MenuId="@secondMenu.Id" />
/// </div>
/// ]]></code>
/// <para>
/// Each <c>(MenuLocation, MenuKind, MenuId)</c> address has independent command ownership,
/// form tracking and navigation rollback.
/// </para>
/// <para>
/// GXMenuBar clears its own commands on navigation by default and restores them if
/// the navigation is canceled. Configure grouped ownership and confirmation when a
/// page and its editor use separate menus:
/// </para>
/// <code language="razor"><![CDATA[
/// <GXMenuBar MenuId="@EditorId" ProtectedMenuIds="@ProtectedMenus"
///            PreserveCommandsOnNavigation="@IsTabNavigation"
///            ConfirmNavigation="@ConfirmDiscardAsync"
///            WarnOnUnsavedChanges="true" />
/// <GXMenuBar NavigationGuardEnabled="false" />
/// ]]></code>
/// <para>
/// <c>ProtectedMenus</c> is an <c>IReadOnlyList&lt;Guid?&gt;</c>; include null for the default menu.
/// It defaults to the renderer's own MenuId when omitted. Protected menus share the
/// renderer’s Location and Kind. <c>PreserveCommandsOnNavigation</c> accepts source and
/// target URL strings and returns true for navigation that retains the current
/// commands. <c>ConfirmNavigation</c> is a <c>Func&lt;CancellationToken, Task&lt;bool&gt;&gt;</c>; it is
/// called only when a protected EditContext is modified. Return false to stay and
/// honor the cancellation token to dismiss superseded confirmation requests.
/// </para>
/// <para>
/// <c>WarnOnUnsavedChanges</c> enables the built-in internal confirmation when no ConfirmNavigation callback is provided, and the browser warning for refresh, external links
/// and tab close. The component provides an internal discard/stay dialog. Applications may override it using ConfirmNavigation and supply
/// route classification; the UI library has no AMI dependency. Disable the guard on
/// secondary renderers when another GXMenuBar already protects their menus.
/// </para>
/// <para>
/// The built-in unsaved-changes dialog uses UnsavedChangesTitle,
/// UnsavedChangesMessage, DiscardChangesText and StayText for customizable captions.
/// It is dismissed when navigation is canceled or the component is disposed.
/// No separate application confirmation service or dialog host is required.
/// </para>
/// </example>
public partial class GXMenuBar
{
}
