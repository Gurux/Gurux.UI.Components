# Gurux.UI.Components

Reusable Razor UI components for Gurux web, Windows, iOS, and Android applications. The library provides common form controls, layouts, navigation, tables, dialogs, notifications, file handling, and Markdown rendering for .NET applications built with Blazor.

## Requirements

- .NET 10.0 or later
- A Blazor application (WebAssembly, Server, or Web App)

## Install

Add the package to your application when it is available from your configured NuGet feed:

```bash
dotnet add package Gurux.UI.Components
```

Alternatively, reference the project directly from a solution:

```xml
<ProjectReference Include="..\\Gurux.UI.Components\\Gurux.UI.Components.csproj" />
```

Register the local-storage service in `Program.cs`:

```csharp
using Gurux.UI.Components;

builder.Services.AddScoped<IGXLocalStorage, GXLocalStorage>();
```

Add the component namespace to `_Imports.razor`:

```razor
@using Gurux.UI.Components
```

## Included components

- **Inputs:** text, number, email, password, phone, IP address, file, checkbox, switch, search, dropdown, date/time, and color controls.
- **Layout and navigation:** accordion, tabs, panels, menus, context menus, tooltips, callouts, badges, and spinners.
- **Data and feedback:** tables, progress bars, dialogs, validation UI, notifications, and Markdown rendering.
- **Browser integration:** local and cookie storage, resource storage, auto-refresh, image handling, and file selection.

## Examples

### Bind a text value

`GXInputText` is a styled text input with normal Blazor two-way binding.

```razor
@page "/profile"

<h1>Profile</h1>

<GXInputText @bind-Value="name"
             Placeholder="Your name"
             CssClass="form-control" />

<p>Hello, @name!</p>

@code {
    private string? name;
}
```

### Limit a numeric value

`GXInputNumber<TValue>` integrates with Blazor forms and supports minimum, maximum, and step values.

```razor
@page "/settings"

<EditForm Model="this">
    <GXInputNumber TValue="int"
                   @bind-Value="refreshInterval"
                   Min="1"
                   Max="60"
                   Step="1" />
</EditForm>

<p>Refresh interval: @refreshInterval seconds</p>

@code {
    private int refreshInterval = 10;
}
```

### Render Markdown content

Use `MarkdownView` to display Markdown stored in your application or received from an API.

```razor
@page "/help"

<MarkdownView Content="@markdown" />

@code {
    private const string markdown = """
        ## Welcome

        This page supports **bold text**, links, lists, and fenced code blocks.
        """;
}
```

### Select table columns at runtime

Set `GXTable<TItem>.AllowColumnCustomization="true"` to let users choose displayed columns and their responsive or print `Visibility`. The selector automatically discovers `Th` headers, using `Th.Id` as the stable identifier and falling back to `Th.Name`. Use the same identifier in `Fh.Id` and `Td.Id`. The header content supplies the selector label; optionally provide `ColumnOptions` with explicit identifiers and localized labels. These components update their visibility together without reloading the rows.

Open the column editor using the configuration icon in the last cell of the table header. Its dialog contains column selection, visibility, drag ordering, and Reset. Save applies and persists the draft settings. Cancel, the close icon, or Escape discards the draft. Reset restores default selection, visibility, order, and widths only when followed by Save. A failed Save keeps the dialog open for retry.

Add application-specific actions with `ColumnEditorFooterContent`. They appear next to Save and Cancel. For example:

```razor
<ColumnEditorFooterContent Context="editor">
    <button type="button" class="btn btn-outline-primary" disabled="@editor.IsSaving"
            @onclick="() => SaveDefaults(editor)">Save as Default</button>
</ColumnEditorFooterContent>
```

Place this fragment inside `GXTable`. The application's `SaveDefaults(GXTableColumnEditorContext editor)` handler can persist `editor.VisibleColumns`, `editor.ColumnOrder`, `editor.Visibility`, and `editor.Widths` to its own defaults store. These are read-only snapshots of the current draft; saving application defaults does not automatically commit personal table settings or close the dialog. Call `editor.SaveAsync()` after a successful application save when that behavior is desired.

Use `ColumnEditorContent` to replace the built-in column editor with your own UI. Both templates receive `GXTableColumnEditorContext`, with `SetVisibleAsync(name, visible)`, `SetVisibilityAsync(name, visibility)`, `MoveAsync(name, index)`, `ResetAsync()`, `SaveAsync()`, and `CancelAsync()`. Its `IsSaving` and `Error` properties describe the standard save operation. Commands affect only the draft that supplied the context and become inactive when that editor closes or the table is disposed. The dialog's standard Save, Cancel, and Reset actions remain available.

`OnColumnEditorReset` can apply application-specific original defaults to the reset draft, including when the table currently uses saved shared defaults. The callback receives the draft context; use its setters rather than calling `ResetAsync` again.

The column editor uses `GXTable` with `RearrangeRows`: drag a row to change the corresponding column's position in the headers, filters and data rows. Unnamed cells stay in their original positions. Reset restores the configured column selection, default order, responsive visibility and widths.

Use `Columns`, `DefaultColumnOrder` and `DefaultColumnVisibility` to supply administrator defaults. Personal settings override these defaults; Reset restores the configured defaults. `AllowColumnCustomization` controls whether the column editor is available.

With an explicit table `Id`, `Save="true"` (the default), and a registered `IGXLocalStorage`, selection, visibility and order are saved in browser storage and restored during `OnInitializedAsync`. Saved preferences still apply when customization controls are disabled. Without an ID or with `Save="false"`, changes last for the component's lifetime. Each column identifier must be unique within its table and remain stable across sessions.

Keep the table `Id` stable for the component's lifetime. When switching between table identities in the same view, use `@key` with the table ID so each table initializes its own preferences.

```razor
<GXTable TItem="Device" Id="devices" Save="true" AllowColumnCustomization="true" ItemsProvider="GetDevices">
    <HeaderContent>
        <Th TItem="Device" Id="name" Name="Name">Name</Th>
        <Th TItem="Device" Id="serial" Name="SerialNumber">Serial number</Th>
    </HeaderContent>
    <ItemContent Context="device">
        <Td Id="name">@device.Name</Td>
        <Td Id="serial">@device.SerialNumber</Td>
    </ItemContent>
</GXTable>
```

`Columns` supplies the initial visible identifiers: `null` shows all columns and an empty array hides all named columns. `ColumnsProvider` can asynchronously load a saved selection. `ColumnsChanged` receives the proposed visible identifiers; save them in that callback. A failed save displays an error and keeps the previous selection. Use `@bind-Columns` when the owning component also needs the updated selection.

Applications can additionally supply persistence through `ColumnsProvider` and `ColumnsChanged`; local browser preferences take precedence over provider defaults when available. Hiding a column retains its filter values and current sort. Header clicks refresh `ItemsProvider`, reset paging, and cycle between ascending, descending, and no sorting.

With `Save="true"`, an explicit table `Id` (or `PreferencesId`), and a registered `IGXLocalStorage`, header clicks also persist the sort column and direction, including no sorting. These are restored during initialization before loading items. Without a saved sort, the configured defaults apply.

Run the rendering and lifecycle checks with:

```powershell
dotnet run --project Tests/TableColumns/TableColumns.Checks.csproj
dotnet run --project Tests/TableCustomization/TableCustomization.Checks.csproj
```

### Persist table column widths

Give `GXTable` an explicit HTML `Id` and set `Save="true"` (the default) to persist column widths through the registered `IGXLocalStorage` service. Drag the right edge of a named `Th` header to resize it. Widths use `Th.Id`, falling back to `Th.Name`, and are restored during `OnInitializedAsync`. Tables without an explicit ID, or with `Save="false"`, do not read or write column widths.

```powershell
dotnet run --project Tests/TableWidths/TableWidths.Checks.csproj
```

### Row identifiers and edit links

Set `ItemKey` to select a stable row identifier and `EditLink` to build the edit URL once per row:

```razor
<GXTable TItem="GXView"
         ItemsProvider="GetViews"
         ItemKey="@(view => view.Id)"
         EditLink="@(view => NavigationManager.GetCurrentPagePath($"/Config/View/Edit/{view.Id}"))">
    <ItemContent Context="view">
        <Td EditLink="true">@view.Name</Td>
    </ItemContent>
</GXTable>
```

Without `ItemKey`, the table continues to use the item's `Id` property. Custom identifiers are used for row selection, provider refreshes, removal and render keys. Identifier values must be stable and unique within the table; they do not need to implement `IComparable`.

`Td.EditLink` uses the containing row, so it does not depend on which row is currently selected. An explicit `Td.Link` takes precedence. `CanEdit="false"` renders plain content, and the optional navigation guard is consulted before following a link.

```powershell
dotnet run --project Tests/TableRows/TableRows.Checks.csproj
```

### Build a data view

`Gurux.UI.Components.Views.GXViewEditor` edits a `GXViewModel` using application-supplied `Tables`, `Blocks`, `OnSave`, and `PreviewProvider`. Users can select fields, rename and reorder columns, configure joins and aggregates, choose formatters, and select pagination or all rows.

`GXViewTable` renders the definition using `QueryProvider`. Supply `OnViewChanged` to persist runtime column visibility and page-size changes; failed saves retain the previous settings. `Formatter` provides application-specific enum or time formatting. `HeaderContent`, `FooterContent`, and `CellContent` integrate blocks and row links. Formatting does not change sorting or filtering values.

```powershell
dotnet run --project Tests/ViewBuilder/ViewBuilder.Checks.csproj
```

## License

This project is licensed under the [GNU General Public License v2.0](https://www.gnu.org/licenses/old-licenses/gpl-2.0.html) only.

## Links

- [Gurux](https://www.gurux.fi/)
- [Project repository](https://github.com/gurux/gurux.ui.components)


## Menus in multiple locations

Register `IGXMenuService` with the scoped `GXMenuService` implementation. Menus
are independent for each `(MenuLocation, MenuKind)` pair:

- Locations: `Top`, `Left`, `Right`, `Bottom`, `Footer`.
- Kinds: `Navigation`, `Tabs`, `Configuration`, `MenuBar`, `ContextMenu`, `Dropdown`, `Overflow`.

Both enums are in `Gurux.UI.Components.Enums`. `MenuKind.All` is reserved for
regeneration and cannot address a rendered menu.

```csharp
using var pageMenu = new GXPageMenu(menuService,
    MenuLocation.Left, MenuKind.Navigation);
pageMenu.AddMenuItems(new GXMenuItem("Devices", "device", openDevices));

using var footer = menuService.RegisterMenu(footerCommands,
    location: MenuLocation.Footer, kind: MenuKind.Overflow);
var visible = menuService.GetItems(MenuLocation.Footer, MenuKind.Overflow);
```

Each pair has its own command ownership and EditContext. Use GetEditContext /
SetEditContext to access a chosen pair. Default Items, EditContext, AddMenuItems
and GXPageMenu calls target `Top / MenuBar`. Disposing an old registration cannot
clear its replacement or another location.

The existing GXMenuBar toolbar accepts Location and Kind parameters and reads only
that pair. Kinds identify separate menus; they do not automatically provide new
navigation/dropdown/context markup or insert regions into an application's layout.
Applications place the appropriate renderer where needed.

### Dropdown commands

`GXMenuBar` renders a command with nonempty `GXMenuItem.Children` as a labelled
`MenuControl` dropdown. Its children are command buttons; the parent callback is
not invoked when opening the dropdown. Items without children remain direct
commands. Children can contain further child commands for nested submenus.
Use `new GXMenuItem { IsSeparator = true }` to separate command groups.

```csharp
using var pageMenu = new GXPageMenu(menuService);
pageMenu.AddMenuItems(new GXMenuItem
{
    Text = "File",
    Children =
    [
        new GXMenuItem("Open", null, openCallback),
        new GXMenuItem("Save", null, saveCallback, EnableStyle.Modified)
    ]
});
```

```razor
<GXMenuBar />
```

Both the parent and child commands respect the menu's edit context and progress
state. Starting a progress operation closes and disables the dropdown. Register
`IGXProgress` with `GXProgressService` as well as the menu service.

`MenuControl` also supports standalone declarative dropdowns:

```razor
<MenuControl Dropdown="true" Text="File">
    <MenuItem Text="Open" OnClick="OpenAsync" />
    <MenuItem Text="Save" OnClick="SaveAsync" ReadOnly="@(!canSave)" />
</MenuControl>
```

Alternatively, supply `MenuItems` as a `List<MenuItem>`; replacing the list updates
the commands. `Icon` sets the toggle's resource icon, `ReadOnly` disables the whole
control, and `RightCorner` / `Bottom` align the popup. Without `Dropdown="true"`,
the existing row of buttons is preserved. A dropdown closes when a command is
selected, Escape is pressed, its toggle is clicked again, or the user clicks
outside it. The component uses Blazor events and scoped CSS; Bootstrap JavaScript
is not required. Ensure the application loads the generated scoped CSS bundle.

```powershell
dotnet run --project Tests/MenuDropdowns/MenuDropdowns.Checks.csproj
```

Clear(location, kind) clears one pair. ClearForNavigation(location, kind) returns
a one-use restoration action for that pair; each toolbar handles its own address.
ClearForNavigation() addresses all registered pairs. Canceled navigation restores
only unchanged pairs, preserving newly registered destination menus. Multiple
renderers for the same pair can safely clear it during the same navigation.
The service follows the UI renderer's synchronization context.

The former top-specific interface and service names have been removed; consumers
use IGXMenuService and GXMenuService.


Multiple menus in one location can use the existing `GXMenu.Id` as `MenuId`.
The optional ID defaults to null for the original shared menu. No additional
persisted field is required. Register commands with the same ID as the renderer:

```csharp
using var commands = new GXPageMenu(menuService, menuId: definition.Id);
commands.AddMenuItems(new GXMenuItem("Clear", "clear", clearCallback));
```

Two menu bars can be stacked in a layout:

```razor
<div class="d-flex flex-column align-items-start">
    <GXMenuBar MenuId="@firstMenu.Id" />
    <GXMenuBar MenuId="@secondMenu.Id" />
</div>
```

Each `(MenuLocation, MenuKind, MenuId)` address has independent command ownership,
form tracking and navigation rollback.
