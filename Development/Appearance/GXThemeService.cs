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
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace Gurux.UI.Components.Appearance;
/// <summary>Coordinates independent appearance preferences without any server OS detection.</summary>
public sealed class GXThemeService : IGXThemeService
{
    /// <summary>Loads and saves the scoped appearance preferences.</summary>
    private readonly IGXAppearancePersistence _persistence;
    /// <summary>Stores independent copies of built-in and application theme definitions by identifier.</summary>
    private readonly Dictionary<string, GXThemeDefinition> _themes = new(StringComparer.Ordinal);
    /// <summary>Identifies built-in themes that applications may not replace.</summary>
    private readonly HashSet<string> _builtInIds = BuiltInThemes().Select(x => x.Id).ToHashSet(StringComparer.Ordinal);
    /// <summary>Stores an independent copy of the application default appearance settings.</summary>
    private readonly GXAppearanceSettings _defaults;
    /// <summary>Serializes initialization and preference changes that access persistent storage.</summary>
    private readonly SemaphoreSlim _gate = new(1, 1);
    /// <summary>Stores the most recent validated appearance preferences.</summary>
    private GXAppearanceSettings _current;
    /// <summary>Records whether saved preferences have already been loaded.</summary>
    private bool _initialized;
    /// <summary>Stores the client platform used to resolve automatic layouts.</summary>
    private string? _platform;
    /// <summary>Stores the browser preference used to resolve automatic color mode.</summary>
    private bool _prefersDark;
    /// <inheritdoc />
    public ResponsiveLayoutMode ResponsiveMode { get; private set; } = ResponsiveLayoutMode.Expanded;
    /// <inheritdoc />
    public bool CoarsePointer { get; private set; }
    /// <inheritdoc />
    public GXAdaptiveBehavior GetAdaptiveBehavior(LayoutStyle? layout = null) => GXAdaptiveBehavior.Create(ResolveLayout(layout ?? _current.Layout), ResponsiveMode, CoarsePointer);

    /// <inheritdoc />
    public void UpdateViewport(int width, bool coarsePointer)
    {
        if (width <= 0)
        {
            return;
        }
        var mode = width <= 768 ? ResponsiveLayoutMode.Compact : width < 1200 ? ResponsiveLayoutMode.Medium : ResponsiveLayoutMode.Expanded;
        if (mode != ResponsiveMode || coarsePointer != CoarsePointer)
        {
            ResponsiveMode = mode;
            CoarsePointer = coarsePointer;
            AppearanceChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <summary>Copies application defaults, registers trusted themes, and connects preference persistence.</summary>
    /// <param name="options">The application defaults and trusted theme registrations.</param>
    /// <param name="persistence">The storage used to load and save appearance preferences.</param>
    public GXThemeService(GXThemeOptions options, IGXAppearancePersistence persistence)
    {
        ArgumentNullException.ThrowIfNull(options);
        _persistence = persistence;
        foreach (var theme in BuiltInThemes().Concat(options.Themes))
        {
            ValidateTheme(theme);
            if (!_themes.TryAdd(theme.Id, theme.Copy()))
            {
                throw new ArgumentException($"Duplicate theme ID: {theme.Id}");
            }
        }
        _defaults = options.Defaults.Copy();
        Validate(_defaults);
        _current = _defaults.Copy();
    }

    /// <inheritdoc />
    public GXAppearanceSettings Current => _current.Copy();
    /// <inheritdoc />
    public LayoutStyle EffectiveLayout => ResolveLayout(_current.Layout);
    /// <inheritdoc />
    public ColorMode EffectiveColorMode => _current.ColorMode == ColorMode.Auto
        ? (_prefersDark ? ColorMode.Dark : ColorMode.Light) : _current.ColorMode;
    /// <inheritdoc />
    public Exception? PersistenceError { get; private set; }
    /// <inheritdoc />
    public event EventHandler? AppearanceChanged;

    /// <inheritdoc />
    public IReadOnlyList<GXThemeDefinition> GetThemes()
    {
        lock (_themes)
        {
            return new ReadOnlyCollection<GXThemeDefinition>(_themes.Values.Select(x => x.Copy()).ToList());
        }
    }

    /// <summary>Registers a validated application theme without allowing built-in themes to be replaced.</summary>
    /// <param name="theme">The trusted application theme definition to validate or register.</param>
    public void RegisterTheme(GXThemeDefinition theme)
    {
        ArgumentNullException.ThrowIfNull(theme);
        ValidateTheme(theme);
        var copy = theme.Copy();
        lock (_themes)
        {
            if (_builtInIds.Contains(copy.Id))
            {
                throw new ArgumentException(Gurux.UI.Components.Properties.Resources.BuiltInThemeReadOnly, nameof(theme));
            }
            _themes[copy.Id] = copy;
        }
        AppearanceChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Checks whether a theme identifier is registered in this service.</summary>
    /// <param name="id">The theme identifier to look up.</param>
    /// <returns>True when a theme with the supplied identifier exists.</returns>
    private bool HasTheme(string? id)
    {
        lock (_themes)
        {
            return id != null && _themes.ContainsKey(id);
        }
    }

    /// <inheritdoc />
    public LayoutStyle ResolveLayout(LayoutStyle requested) => requested != LayoutStyle.Auto ? requested : _platform switch
    {
        "Windows" => LayoutStyle.Fluent,
        "iOS" or "macOS" => LayoutStyle.Cupertino,
        "Android" => LayoutStyle.Material,
        _ => LayoutStyle.Default
    };

    /// <inheritdoc />
    public void UpdateEnvironment(string? platform, bool prefersDark)
    {
        if (_platform != platform || _prefersDark != prefersDark)
        {
            _platform = platform;
            _prefersDark = prefersDark;
            AppearanceChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    /// <inheritdoc />
    public async Task InitializeAsync()
    {
        await _gate.WaitAsync();
        try
        {
            if (_initialized)
            {
                return;
            }
            try
            {
                var saved = await _persistence.LoadAsync();
                if (saved != null)
                {
                    Validate(saved);
                    _current = saved.Copy();
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                PersistenceError = ex;
            }
            _initialized = true;
        }
        finally
        {
            _gate.Release();
        }
        AppearanceChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <inheritdoc />
    public Task SetThemeAsync(string themeId)
    {
        if (!HasTheme(themeId))
        {
            throw new ArgumentException(Gurux.UI.Components.Properties.Resources.RegisteredThemeRequired, nameof(themeId));
        }
        return ChangeAsync(settings => settings.BootstrapTheme = themeId);
    }

    /// <inheritdoc />
    public Task SetLayoutAsync(LayoutStyle layout)
    {
        if (!Enum.IsDefined(layout))
        {
            throw new ArgumentOutOfRangeException(nameof(layout));
        }
        return ChangeAsync(settings => settings.Layout = layout);
    }

    /// <inheritdoc />
    public Task SetColorModeAsync(ColorMode mode)
    {
        if (!Enum.IsDefined(mode))
        {
            throw new ArgumentOutOfRangeException(nameof(mode));
        }
        return ChangeAsync(settings => settings.ColorMode = mode);
    }

    /// <inheritdoc />
    public Task ResetAsync() => ChangeAsync(settings =>
    {
        settings.Layout = _defaults.Layout;
        settings.BootstrapTheme = _defaults.BootstrapTheme;
        settings.ColorMode = _defaults.ColorMode;
    });

    /// <summary>Serializes a preference update, saves an independent copy, and notifies subscribers.</summary>
    /// <param name="update">The action that mutates an independent copy of the current preferences.</param>
    /// <returns>The asynchronous preference update operation.</returns>
    private async Task ChangeAsync(Action<GXAppearanceSettings> update)
    {
        await InitializeAsync();
        await _gate.WaitAsync();
        try
        {
            var next = _current.Copy();
            update(next);
            _current = next;
            try
            {
                await _persistence.SaveAsync(next.Copy());
                PersistenceError = null;
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                PersistenceError = ex;
            }
        }
        finally
        {
            _gate.Release();
        }
        AppearanceChanged?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>Rejects settings with undefined layout or color modes or an unregistered theme identifier.</summary>
    /// <param name="settings">The appearance preferences to validate or persist.</param>
    private void Validate(GXAppearanceSettings settings)
    {
        if (!Enum.IsDefined(settings.Layout) || !Enum.IsDefined(settings.ColorMode) || !HasTheme(settings.BootstrapTheme))
        {
            throw new ArgumentException(Gurux.UI.Components.Properties.Resources.InvalidAppearanceSettings);
        }
    }

    /// <summary>Rejects invalid theme identifiers, CSS variables, and untrusted stylesheet URLs.</summary>
    /// <param name="theme">The trusted application theme definition to validate or register.</param>
    private static void ValidateTheme(GXThemeDefinition theme)
    {
        if (!Regex.IsMatch(theme.Id ?? "", "^[a-z][a-z0-9-]{0,63}$") || string.IsNullOrWhiteSpace(theme.Name))
        {
            throw new ArgumentException(Gurux.UI.Components.Properties.Resources.InvalidThemeIdentity);
        }
        if (theme.CssVariables == null)
        {
            throw new ArgumentException(Gurux.UI.Components.Properties.Resources.ThemeVariablesRequired);
        }
        foreach (var variable in theme.CssVariables)
        {
            if (!Regex.IsMatch(variable.Key, "^--[a-zA-Z0-9_-]+$") || variable.Value == null)
            {
                throw new ArgumentException(Gurux.UI.Components.Properties.Resources.InvalidThemeVariables);
            }
        }
        var url = theme.CssUrl ?? "";
        if (url.Length == 0)
        {
            // A variables-only theme uses the host's Bootstrap stylesheet.
            return;
        }
        if (theme.AllowExternalStylesheet && Uri.TryCreate(new Uri("https://appearance.invalid/"), url, out var remote) &&
            (remote.Scheme == Uri.UriSchemeHttps || remote.Scheme == Uri.UriSchemeHttp) &&
            string.IsNullOrEmpty(remote.UserInfo))
        {
            return;
        }
        if (url.StartsWith("//", StringComparison.Ordinal) || url.Contains(':') || url.Contains('\\') || url.Contains('?') || url.Contains('#') || url.Contains('%') || url.TrimStart('/').Split('/').Any(part => part is ".." or "." or "") || !url.EndsWith(".css", StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException(Gurux.UI.Components.Properties.Resources.InvalidThemeStylesheet);
        }
    }

    /// <summary>Creates the built-in Bootstrap and Bootswatch theme registrations.</summary>
    /// <returns>The built-in theme definitions in display order.</returns>
    private static IEnumerable<GXThemeDefinition> BuiltInThemes()
    {
        const string names = "default brite cerulean cosmo cyborg darkly flatly journal litera lumen lux materia minty morph pulse quartz sandstone simplex sketchy slate solar spacelab superhero united vapor yeti zephyr";
        var dark = new HashSet<string> { "cyborg", "darkly", "quartz", "slate", "solar", "superhero", "vapor" };
        foreach (var id in names.Split(' '))
        {
            yield return new GXThemeDefinition
            {
                Id = id,
                Name = char.ToUpperInvariant(id[0]) + id[1..],
                CssUrl = $"_content/Gurux.UI.Components/Appearance/themes/{id}.min.css",
                IsDark = dark.Contains(id),
                // Bootswatch palettes include fixed colors; only stock Bootstrap is fully mode-aware.
                SupportsColorModes = id == "default",
                Description = id == "default" ? "Bootstrap 5.3.8" : "Bootswatch 5.3.8"
            };
        }
    }
}
