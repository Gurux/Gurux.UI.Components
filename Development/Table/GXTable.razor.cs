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

namespace Gurux.UI.Components.Table;

/// <summary>
/// Displays typed rows with paging, sorting, selection, filtering, and optional row reordering.
/// </summary>
/// <typeparam name="TItem">The type of item displayed in each table row.</typeparam>
/// <remarks>
/// <para><b>Select table columns at runtime</b></para>
/// <para>Set <c>GXTable&lt;TItem&gt;.AllowColumnCustomization=&quot;true&quot;</c> to let users choose displayed columns and their responsive or print <c>Visibility</c>. The selector automatically discovers <c>GXTh</c> headers, using <c>GXTh.Id</c> as the stable identifier and falling back to <c>GXTh.Name</c>. Use the same identifier in <c>GXFh.Id</c> and <c>GXTd.Id</c>. The header content supplies the selector label; optionally provide <c>ColumnOptions</c> with explicit identifiers and localized labels. These components update their visibility together without reloading the rows.</para>
/// <para>Open the column editor using the configuration icon in the last cell of the table header. Its dialog contains column selection, visibility, drag ordering, and Reset. Save applies and persists the draft settings. Cancel, the close icon, or Escape discards the draft. Reset restores default selection, visibility, order, and widths only when followed by Save. A failed Save keeps the dialog open for retry.</para>
/// <para>Add application-specific actions with <c>ColumnEditorFooterContent</c>. They appear next to Save and Cancel. For example:</para>
/// <para>Place this fragment inside <c>GXTable</c>. The application&apos;s <c>SaveDefaults(GXTableColumnEditorContext editor)</c> handler can persist <c>editor.VisibleColumns</c>, <c>editor.ColumnOrder</c>, <c>editor.Visibility</c>, and <c>editor.Widths</c> to its own defaults store. These are read-only snapshots of the current draft; saving application defaults does not automatically commit personal table settings or close the dialog. Call <c>editor.SaveAsync()</c> after a successful application save when that behavior is desired.</para>
/// <para>Use <c>ColumnEditorContent</c> to replace the built-in column editor with your own UI. Both templates receive <c>GXTableColumnEditorContext</c>, with <c>SetVisibleAsync(name, visible)</c>, <c>SetVisibilityAsync(name, visibility)</c>, <c>MoveAsync(name, index)</c>, <c>ResetAsync()</c>, <c>SaveAsync()</c>, and <c>CancelAsync()</c>. Its <c>IsSaving</c> and <c>Error</c> properties describe the standard save operation. Commands affect only the draft that supplied the context and become inactive when that editor closes or the table is disposed. The dialog&apos;s standard Save, Cancel, and Reset actions remain available.</para>
/// <para><c>OnColumnEditorReset</c> can apply application-specific original defaults to the reset draft, including when the table currently uses saved shared defaults. The callback receives the draft context; use its setters rather than calling <c>ResetAsync</c> again.</para>
/// <para>The column editor uses <c>GXTable</c> with <c>RearrangeRows</c>: drag a row to change the corresponding column&apos;s position in the headers, filters and data rows. Unnamed cells stay in their original positions. Reset restores the configured column selection, default order, responsive visibility and widths.</para>
/// <para>Use <c>Columns</c>, <c>DefaultColumnOrder</c> and <c>DefaultColumnVisibility</c> to supply administrator defaults. Personal settings override these defaults; Reset restores the configured defaults. <c>AllowColumnCustomization</c> controls whether the column editor is available.</para>
/// <para>With an explicit table <c>Id</c>, <c>Save=&quot;true&quot;</c> (the default), and a registered <c>IGXLocalStorage</c>, selection, visibility and order are saved in browser storage and restored during <c>OnInitializedAsync</c>. Saved preferences still apply when customization controls are disabled. Without an ID or with <c>Save=&quot;false&quot;</c>, changes last for the component&apos;s lifetime. Each column identifier must be unique within its table and remain stable across sessions.</para>
/// <para>Keep the table <c>Id</c> stable for the component&apos;s lifetime. When switching between table identities in the same view, use <c>@key</c> with the table ID so each table initializes its own preferences.</para>
/// <para><c>Columns</c> supplies the initial visible identifiers: <c>null</c> shows all columns and an empty array hides all named columns. <c>ColumnsProvider</c> can asynchronously load a saved selection. <c>ColumnsChanged</c> receives the proposed visible identifiers; save them in that callback. A failed save displays an error and keeps the previous selection. Use <c>@bind-Columns</c> when the owning component also needs the updated selection.</para>
/// <para>Applications can additionally supply persistence through <c>ColumnsProvider</c> and <c>ColumnsChanged</c>; local browser preferences take precedence over provider defaults when available. Hiding a column retains its filter values and current sort. Header clicks refresh <c>ItemsProvider</c>, reset paging, and cycle between ascending, descending, and no sorting.</para>
/// <para>With <c>Save=&quot;true&quot;</c>, an explicit table <c>Id</c> (or <c>PreferencesId</c>), and a registered <c>IGXLocalStorage</c>, header clicks also persist the sort column and direction, including no sorting. These are restored during initialization before loading items. Without a saved sort, the configured defaults apply.</para>
/// <para>Run the rendering and lifecycle checks with:</para>
/// <para><b>Persist table column widths</b></para>
/// <para>Give <c>GXTable</c> an explicit HTML <c>Id</c> and set <c>Save=&quot;true&quot;</c> (the default) to persist column widths through the registered <c>IGXLocalStorage</c> service. Drag the right edge of a named <c>GXTh</c> header to resize it. Widths use <c>GXTh.Id</c>, falling back to <c>GXTh.Name</c>, and are restored during <c>OnInitializedAsync</c>. Tables without an explicit ID, or with <c>Save=&quot;false&quot;</c>, do not read or write column widths.</para>
/// <para><b>Row identifiers and edit links</b></para>
/// <para>Set <c>ItemKey</c> to select a stable row identifier and <c>EditLink</c> to build the edit URL once per row:</para>
/// <para>Without <c>ItemKey</c>, the table continues to use the item&apos;s <c>Id</c> property. Custom identifiers are used for row selection, provider refreshes, removal and render keys. Identifier values must be stable and unique within the table; they do not need to implement <c>IComparable</c>.</para>
/// <para><c>GXTd.EditLink</c> uses the containing row, so it does not depend on which row is currently selected. An explicit <c>GXTd.Link</c> takes precedence. <c>CanEdit=&quot;false&quot;</c> renders plain content, and the optional navigation guard is consulted before following a link.</para>
/// </remarks>
/// <example>
/// <code language="razor"><![CDATA[
/// <ColumnEditorFooterContent Context="editor">
///     <button type="button" class="btn btn-outline-primary" disabled="@editor.IsSaving"
///             @onclick="() => SaveDefaults(editor)">Save as Default</button>
/// </ColumnEditorFooterContent>
/// ]]></code>
/// </example>
/// <example>
/// <code language="razor"><![CDATA[
/// <GXTable TItem="Device" Id="devices" Save="true" AllowColumnCustomization="true" ItemsProvider="GetDevices">
///     <HeaderContent>
///         <GXTh TItem="Device" Id="name" Name="Name">Name</GXTh>
///         <GXTh TItem="Device" Id="serial" Name="SerialNumber">Serial number</GXTh>
///     </HeaderContent>
///     <ItemContent Context="device">
///         <GXTd Id="name">@device.Name</GXTd>
///         <GXTd Id="serial">@device.SerialNumber</GXTd>
///     </ItemContent>
/// </GXTable>
/// ]]></code>
/// </example>
/// <example>
/// <code language="powershell"><![CDATA[
/// dotnet run --project Tests/TableColumns/TableColumns.Checks.csproj
/// dotnet run --project Tests/TableCustomization/TableCustomization.Checks.csproj
/// ]]></code>
/// </example>
/// <example>
/// <code language="powershell"><![CDATA[
/// dotnet run --project Tests/TableWidths/TableWidths.Checks.csproj
/// ]]></code>
/// </example>
/// <example>
/// <code language="razor"><![CDATA[
/// <GXTable TItem="GXView"
///          ItemsProvider="GetViews"
///          ItemKey="@(view => view.Id)"
///          EditLink="@(view => NavigationManager.GetCurrentPagePath($"/Config/View/Edit/{view.Id}"))">
///     <ItemContent Context="view">
///         <GXTd EditLink="true">@view.Name</GXTd>
///     </ItemContent>
/// </GXTable>
/// ]]></code>
/// </example>
/// <example>
/// <code language="powershell"><![CDATA[
/// dotnet run --project Tests/TableRows/TableRows.Checks.csproj
/// ]]></code>
/// </example>
public partial class GXTable<TItem>
{
}
