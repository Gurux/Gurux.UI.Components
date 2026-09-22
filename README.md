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

## License

This project is licensed under the [GNU General Public License v2.0](https://www.gnu.org/licenses/old-licenses/gpl-2.0.html) only.

## Links

- [Gurux](https://www.gurux.fi/)
- [Project repository](https://github.com/gurux/gurux.ui.components)
