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

### Display an RSS or Atom feed

`GXFeedViewer` uses `System.ServiceModel.Syndication.SyndicationFeed` to read RSS and Atom feeds.
Register an `HttpClient` in the application's dependency injection services if one is not already registered:

```csharp
builder.Services.AddScoped(_ => new HttpClient());
```

```razor
<GXFeedViewer Url="https://gurux.fi/rss.xml"
              MaxItems="10"
              ShowImages="true"
              ShowDescription="true" />
```

The defaults are ten entries with images and descriptions enabled. Entries keep the feed's order.
Images are read from image enclosures and Media RSS content or thumbnails. Descriptions are displayed as
plain text. Changing `Url` loads the new feed and cancels any previous request. Loading failures are shown
in the component. In Blazor WebAssembly, the feed server must allow cross-origin requests (CORS), or the
application must provide a server endpoint that fetches the feed.

### Display an OpenStreetMap map

`GXMap` uses the bundled Leaflet 1.9.4 JavaScript and CSS files, with no Blazor wrapper or additional
NuGet dependencies. Assets load automatically on the first interactive render. OpenStreetMap tiles
require an internet connection. The application must enable Blazor interactivity.

```razor
<GXMap Latitude="61.4978"
       Longitude="23.7610"
       Zoom="15"
       Markers="@markers" />

@code {
    private readonly List<GXMapMarker> markers =
    [
        new() { Latitude = 61.4978, Longitude = 23.7610,
                Title = "Tampere", Url = "https://www.tampere.fi/" }
    ];
}
```

Clicking a marker opens its absolute HTTP/HTTPS URL in a new tab. A marker without a valid URL
is displayed without navigation. `Height` defaults to `400px`; `Label` sets the accessible map name.
Coordinates, zoom and markers are updated when parameters change and the component renders.
Unrelated renders preserve the user's current pan and zoom. Multiple maps can appear on the same page.
The component removes map resources when disposed. Leaflet's license is included in `wwwroot/leaflet/LICENSE`.
Keep the displayed OpenStreetMap attribution and follow the
[OpenStreetMap tile usage policy](https://operations.osmfoundation.org/policies/tiles/).

### Use a same-origin HTTP proxy

Both `GXFeedViewer` and `GXMap` accept an optional `ProxyUrl` parameter. The endpoint must accept GET
requests with the original absolute URL in the `url` query parameter and return the original content type.
Relative proxy paths resolve against the application's base address. GXFeedViewer also proxies displayed
images; GXMap proxies map tiles while keeping its attribution and external marker navigation.

```razor
<GXFeedViewer Url="https://gurux.fi/rss.xml" ProxyUrl="api/HttpProxy" />
<GXMap Latitude="61.4978" Longitude="23.7610" Zoom="15"
       Markers="@markers" ProxyUrl="api/HttpProxy" />
```

The AMI server provides this endpoint with configurable allowed origins, caching and request limits.
When using a different proxy, return an `X-Gurux-Source-Url` header containing the final upstream URL
if redirects change the base used for relative feed links. Cookie authentication is supported by normal
same-origin tile/image requests. Omitting `ProxyUrl` keeps direct requests.

## License

This project is licensed under the [GNU General Public License v2.0](https://www.gnu.org/licenses/old-licenses/gpl-2.0.html) only.

## Links

- [Gurux](https://www.gurux.fi/)
- [Project repository](https://github.com/gurux/gurux.ui.components)
