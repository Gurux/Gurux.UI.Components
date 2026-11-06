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

### Provide contextual help

Implement `IGXHelpProvider` to supply help from embedded resources, files or a remote
service. The provider receives a stable topic, an optional subject version and an
optional culture. Return null when your provider does not handle the request.
The shared service calls providers in registration order and returns the first
non-null result. Errors and cancellation propagate to the caller.

```csharp
using Gurux.UI.Components;
using Gurux.UI.Components.Enums;
using Gurux.UI.Components.Help;

public sealed class ExampleHelpProvider : IGXHelpProvider
{
    public Task<GXHelpContent?> GetHelpAsync(
        GXHelpRequest request, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        GXHelpContent? content = request.Topic == "module.example"
            ? new GXHelpContent(GXHelpKind.Markdown, "# Example module\nModule instructions.")
            : null;
        return Task.FromResult(content);
    }
}
```

Register the service and each provider for the same application scope:

```csharp
builder.Services.AddScoped<IGXHelpProvider, ExampleHelpProvider>();
builder.Services.AddScoped<GXHelpService>();
```

A component can register a topic for its lifetime, including the version of the
object being described and the requested language:

```razor
@using Gurux.UI.Components.Help

<GXHelpContext Topic="module.example" Version="2" Culture="en" />
```

Inject `GXHelpService` into the host's help presenter and load the active request on
user demand. Alternatively, supply a request directly without changing the active
context:

```csharp
GXHelpContent? active = await helpService.GetHelpAsync(cancellationToken);
GXHelpContent? specific = await helpService.GetHelpAsync(
    new GXHelpRequest("module.example", Version: "2", Culture: "en"),
    cancellationToken);
```

`GXHelpContent.Kind` tells the presenter whether `Value` is a URL (`Link`), plain
text (`Text`) or Markdown (`Markdown`). Render Markdown using
`GXMarkdownView Content="@content.Value"`. Text content should be displayed as text,
and Link content should be offered as a documentation link. Storage paths belong
inside a provider; `Value` contains the actual text or a URL.

`GXHelpService.Request` exposes the complete active request. Its `Changed` event
also fires when only the version or culture changes. Removing a component context
restores the previous request, including its version and culture. The existing
`SetTopic`, `Topic`, `HelpUrl` and `GXHelp` documentation link continue to work.
`GXHelp` constructs a link from the configured base address; provider content is
retrieved explicitly by the host's help presenter.

The version is interpreted by the provider: it can be a COSEM class version or a
feature version, rather than the module package version. A null culture requests
the provider's default language. The service does not cache content; a provider
may cache immutable help using the complete `GXHelpRequest` as the cache key so
versions and languages do not mix. The records can also be serialized for an HTTP
adapter when help is provided by a server module.

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

Use `GXMarkdownView` to display Markdown stored in your application or received from an API.

```razor
@page "/help"

<GXMarkdownView Content="@markdown" />

@code {
    private const string markdown = """
        ## Welcome

        This page supports **bold text**, links, lists, and fenced code blocks.
        """;
}
```

### Select table columns at runtime

Set `GXTable<TItem>.AllowColumnCustomization="true"` to let users choose displayed columns and their responsive or print `Visibility`. The selector automatically discovers `GXTh` headers, using `GXTh.Id` as the stable identifier and falling back to `GXTh.Name`. Use the same identifier in `GXFh.Id` and `GXTd.Id`. The header content supplies the selector label; optionally provide `ColumnOptions` with explicit identifiers and localized labels. These components update their visibility together without reloading the rows.

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
        <GXTh TItem="Device" Id="name" Name="Name">Name</GXTh>
        <GXTh TItem="Device" Id="serial" Name="SerialNumber">Serial number</GXTh>
    </HeaderContent>
    <ItemContent Context="device">
        <GXTd Id="name">@device.Name</GXTd>
        <GXTd Id="serial">@device.SerialNumber</GXTd>
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

Give `GXTable` an explicit HTML `Id` and set `Save="true"` (the default) to persist column widths through the registered `IGXLocalStorage` service. Drag the right edge of a named `GXTh` header to resize it. Widths use `GXTh.Id`, falling back to `GXTh.Name`, and are restored during `OnInitializedAsync`. Tables without an explicit ID, or with `Save="false"`, do not read or write column widths.

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
        <GXTd EditLink="true">@view.Name</GXTd>
    </ItemContent>
</GXTable>
```

Without `ItemKey`, the table continues to use the item's `Id` property. Custom identifiers are used for row selection, provider refreshes, removal and render keys. Identifier values must be stable and unique within the table; they do not need to implement `IComparable`.

`GXTd.EditLink` uses the containing row, so it does not depend on which row is currently selected. An explicit `GXTd.Link` takes precedence. `CanEdit="false"` renders plain content, and the optional navigation guard is consulted before following a link.

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

This software is available under either of the following licenses:

- **Open-source license:** GNU General Public License, version 2 only
  (GPL-2.0-only). See [LICENSE](LICENSE) for the full license text.
- **Commercial license:** Available from Gurux Ltd under a separate
  written agreement. This option allows use under the terms of that
  agreement instead of GPL-2.0-only.

A valid commercial license is required to use the commercial licensing
option. See [Gurux Commercial License Notice](LICENSES/LicenseRef-Gurux-Commercial.txt).

For commercial licensing inquiries, [contact Gurux Ltd](https://www.gurux.fi/).

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
`GXMenuControl` dropdown. Its children are command buttons; the parent callback is
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

Commands in `GXMenuControl`, `GXContextMenuController`, and `GXMenuBar` enforce their
named `Policy` both when rendering and immediately before invoking a callback.
Use `GXMenuEntry.Policy`, `GXContextMenuItem.Policy`, or `GXMenuItem.Policy` respectively.
Commands without a policy retain their existing behavior. A protected command is
hidden when authorization services are unavailable or the policy is unknown.
Nested commands require their ancestor policies as well. Authentication changes
and `IGXAuthorizationRefreshSource.Changed` refresh visibility and invalidate pending
authorization decisions. Server endpoints must also enforce their own policies.

The `Tests/MenuAuthorization` and `Tests/ContextMenuAuthorization` executable checks
use the Blazor renderer and named authorization policies to cover visibility,
invocation, permission changes, nested commands, and asynchronous invalidation.

`GXMenuControl` also supports standalone declarative dropdowns:

```razor
<GXMenuControl Dropdown="true" Text="File">
    <GXMenuEntry Text="Open" OnClick="OpenAsync" />
    <GXMenuEntry Text="Save" OnClick="SaveAsync" ReadOnly="@(!canSave)" />
</GXMenuControl>
```

Alternatively, supply `MenuItems` as a `List<GXMenuEntry>`; replacing the list updates
the commands. `Icon` sets the toggle's resource icon, `ReadOnly` disables the whole
control, and `RightCorner` / `Bottom` align the popup. Without `Dropdown="true"`,
the existing row of buttons is preserved. A dropdown closes when a command is
selected, Escape is pressed, its toggle is clicked again, or the user clicks
outside it. The component uses Blazor events and scoped CSS; Bootstrap JavaScript
is not required. Ensure the application loads the generated scoped CSS bundle.

```powershell
dotnet run --project Tests/MenuDropdowns/MenuDropdowns.Checks.csproj
dotnet run --project Tests/MenuAuthorization/MenuAuthorization.Checks.csproj
dotnet run --project Tests/ContextMenuAuthorization/ContextMenuAuthorization.Checks.csproj
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

## Persistent tab selection

URL persistence is optional:

```razor
<GXTabControl PersistSelectionInUrl="true">
    <GXTabPage Path="General" Text="General">General settings</GXTabPage>
    <GXTabPage Path="Advanced" Text="Advanced">Advanced settings</GXTabPage>
</GXTabControl>
```

Selection uses `?tab=Advanced`, with `tab2`, `tab3`, etc. for nested controls.
Reload and browser history restore the selection. Changing a parent clears deeper
selection parameters while retaining other query parameters and the fragment.
Set stable, unique `GXTabPage.Path` values for bookmarks; a missing Path falls back
to Text. Removing the query key restores Active or InitialTab.

Ordinary controls keep direct Path navigation. With persistence enabled, Path
is the selection key and `NavigateOnSelection` does not cause direct navigation
to that path. `SelectionChanged` still runs once for each explicit selection.
Sticky headers default to true; nested controls using persistence default to
false. Set StickyHeader explicitly to override this.

Applications can supply `ITabRouteResolver` through the RouteResolver parameter
or dependency injection. RouteGroup is passed to that resolver for optional
canonical application routes. Without a resolver, selection uses only the
current page's query string; the UI library has no application-specific route map.

## Template token editor

```razor
@using Gurux.UI.Components.TemplateToken

<GXTemplateTokenEditor @bind-Value="Model.Message" />
```

Use inside an EditForm. The component lists available tokens, appends selected
`[namespace:name]` tokens, and provides a plain-text View dialog. Optional TargetType
and TargetId parameters scope token discovery and resolution. Register an
application implementation of `ITemplateTokenEditorService` in dependency
injection. The service receives `TemplateTokenEditorContext` (target and culture)
and returns generic `TemplateTokenDefinition` values. No AMI project or fixed API
endpoint is required by the UI library. Preview output is rendered as text.

## Menu navigation guards

GXMenuBar clears its own commands on navigation by default and restores them if
the navigation is canceled. Configure grouped ownership and confirmation when a
page and its editor use separate menus:

```razor
<GXMenuBar MenuId="@EditorId" ProtectedMenuIds="@ProtectedMenus"
           PreserveCommandsOnNavigation="@IsTabNavigation"
           ConfirmNavigation="@ConfirmDiscardAsync"
           WarnOnUnsavedChanges="true" />
<GXMenuBar NavigationGuardEnabled="false" />
```

`ProtectedMenus` is an `IReadOnlyList<Guid?>`; include null for the default menu.
It defaults to the renderer's own MenuId when omitted. Protected menus share the
renderer’s Location and Kind. `PreserveCommandsOnNavigation` accepts source and
target URL strings and returns true for navigation that retains the current
commands. `ConfirmNavigation` is a `Func<CancellationToken, Task<bool>>`; it is
called only when a protected EditContext is modified. Return false to stay and
honor the cancellation token to dismiss superseded confirmation requests.

`WarnOnUnsavedChanges` enables the built-in internal confirmation when no ConfirmNavigation callback is provided, and the browser warning for refresh, external links
and tab close. The component provides an internal discard/stay dialog. Applications may override it using ConfirmNavigation and supply
route classification; the UI library has no AMI dependency. Disable the guard on
secondary renderers when another GXMenuBar already protects their menus.

TemplateTokenEditor also requires IGXProgress and IGXNotificationService in DI.
Catalog loading, preview and validation use cancellable progress scopes; request
failures are reported through the shared notification service. Validate refreshes
the available definitions and checks token syntax and names without changing the
template. It reports success or distinct validation errors through notifications.
Preview resolves the actual values. IGXNotifier is the event subscription contract
and is not used to display errors.

Template token definitions can supply an optional ExampleValue. Validate checks
syntax and names. View opens a dialog with actual values; without a TargetId,
unavailable known tokens may use their examples. Such a dialog is titled Example
text. Unknown references remain visible. ExampleValue is never a runtime default
and neither action modifies the bound template. Each original token reference
is resolved once per distinct name; replacement text is not expanded again.

The built-in unsaved-changes dialog uses UnsavedChangesTitle,
UnsavedChangesMessage, DiscardChangesText and StayText for customizable captions.
It is dismissed when navigation is canceled or the component is disposed.
No separate application confirmation service or dialog host is required.

## Notification page lifetime

GXNotificationService automatically observes successful LocationChanged events
when NavigationManager is available through dependency injection. Page-scoped
notifications (the default) are removed when leaving their page; Global notices
remain. Rejected navigation has no effect. Each notice tracks the page where it
was added so a destination notice is not erased by late navigation callbacks.

The default tab classifier retains notices for same-path URL changes affecting
only tab/tab2 query parameters. An optional ITabRouteResolver.IsTabNavigation
implementation can recognize application-specific canonical tab routes. The
service detaches navigation tracking on disposal. Manually constructed services
without a NavigationManager do not track browser navigation.

TemplateTokenEditor supports an optional Page scope and Values dictionary for
page-specific draft values. Both are forwarded in TemplateTokenEditorContext.
Changing either invalidates the open View dialog and reloads token definitions.
The application decides how to discover and resolve tokens for each Page.

TemplateTokenEditor has two tabs. Editor contains the text field, searchable
token insertion, Validate and View actions. Tokens lists all definitions available
in the current page/target context using GXTable. Its immediate filter searches
names, descriptions and example values, ignoring case. Table filtering does not
modify the template or persist search settings in browser storage.

URL tags can be displayed using the reusable UI component
`GXTemplateTokenText`: `[url:config/Manufacturer]Manufacturers[/url]`.
NotificationsTile supports these tags in both titles and descriptions.
HTTP, HTTPS and application-relative links are supported. Labels and ordinary HTML
are displayed as escaped text; unsafe URLs and incomplete tags remain text.
URL tags are presentation markup and do not require a server token provider.
