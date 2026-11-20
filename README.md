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

## Component documentation

Usage examples are included in each component's XML documentation and generated API
reference. Razor components keep their class documentation in the adjacent `.razor.cs`
partial class. Code-built components document their class in the existing `.cs` file.

| Component | Examples and integration notes |
|---|---|
| [GXSidePanel](Development/Panel/GXSidePanel.razor.cs) | Arbitrary content and panel lifetime |
| [GXHelpContentView](Development/Help/GXHelpContentView.razor.cs) | Provider content and empty states |
| [GXHelpPanel](Development/Help/GXHelpPanel.razor.cs) | Contextual help in an application layout |
| [GXHelpContext](Development/Help/GXHelpContext.razor.cs) | Help providers, registration and topic selection |
| [GXInputText](Development/GXInputText.razor.cs) | Text binding and HTML attributes |
| [GXInputNumber](Development/GXInputNumber.cs) | Numeric bounds and form integration |
| [GXMarkdownView](Development/GXMarkdownView.razor.cs) | Inline Markdown |
| [GXFeedViewer](Development/GXFeedViewer.razor.cs) | RSS/Atom feeds and same-origin proxies |
| [GXMap](Development/GXMap.razor.cs) | Markers and proxied map tiles |
| [GXTable](Development/Table/GXTable.razor.cs) | Column customization, persistence and row links |
| [GXMenuBar](Development/GXMenuBar.razor.cs) | Locations, menu IDs, dropdown commands and navigation guards |
| [GXMenuControl](Development/GXMenuControl.razor.cs) | Declarative dropdowns and policy checks |
| [GXTabControl](Development/Tab/GXTabControl.razor.cs) | Persistent tab selection |
| [GXTemplateTokenEditor](Development/TemplateToken/GXTemplateTokenEditor.razor.cs) | Token editing, validation and previews |
| [GXTemplateTokenText](Development/TemplateToken/GXTemplateTokenText.razor.cs) | URL presentation tags |
| [GXBitEditor](Development/BitEditor/GXBitEditor.razor.cs) | Bit editing, representations and virtualization |

The separately maintained data-view sources have documentation alongside
[GXViewEditor](Views/GXViewEditor.razor.cs) and [GXViewTable](Views/GXViewTable.razor.cs).
These sources are outside the current `Development` library project.

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

## Adaptive appearance

Opt-in layout, Bootstrap/Bootswatch theme and color-mode management is documented
in [GXThemeProvider](Development/GXThemeProvider.razor) and [GXAdaptiveLayout](Development/GXAdaptiveLayout.razor). `Default` preserves
the existing layout; select `Auto` explicitly to enable client-platform styling.
