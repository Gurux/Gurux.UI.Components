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

using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

namespace Gurux.UI.Components;

/// <summary>Scoped appearance state. Register once per browser application or Server circuit.</summary>
public interface IGXThemeService
{
    GXAppearanceSettings Current { get; }
    GXLayoutStyle EffectiveLayout { get; }
    GXLayoutStyle ResolveLayout(GXLayoutStyle requested);
    GXColorMode EffectiveColorMode { get; }
    GXResponsiveLayoutMode ResponsiveMode => GXResponsiveLayoutMode.Expanded;
    bool CoarsePointer => false;
    GXAdaptiveBehavior GetAdaptiveBehavior(GXLayoutStyle? layout = null) => GXAdaptiveBehavior.Create(ResolveLayout(layout ?? Current.Layout), ResponsiveMode, CoarsePointer);
    void UpdateViewport(int width, bool coarsePointer) { }
    IReadOnlyList<GXThemeDefinition> GetThemes();
    Task InitializeAsync();
    /// <summary>Register or replace an application theme at runtime. Built-in themes cannot be replaced.</summary>
    void RegisterTheme(GXThemeDefinition theme);
    Task SetThemeAsync(string themeId);
    Task SetLayoutAsync(GXLayoutStyle layout);
    Task SetColorModeAsync(GXColorMode mode);
    Task ResetAsync();
    void UpdateEnvironment(string? platform, bool prefersDark);
    event EventHandler? AppearanceChanged;
    Exception? PersistenceError { get; }
}

/// <summary>Coordinates independent appearance preferences without any server OS detection.</summary>
public sealed class GXThemeService : IGXThemeService
{
    private readonly IGXAppearancePersistence _persistence;
    private readonly Dictionary<string, GXThemeDefinition> _themes = new(StringComparer.Ordinal);
    private readonly HashSet<string> _builtInIds = BuiltInThemes().Select(x => x.Id).ToHashSet(StringComparer.Ordinal);
    private readonly GXAppearanceSettings _defaults;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private GXAppearanceSettings _current;
    private bool _initialized;
    private string? _platform;
    private bool _prefersDark;
    public GXResponsiveLayoutMode ResponsiveMode { get; private set; } = GXResponsiveLayoutMode.Expanded;
    public bool CoarsePointer { get; private set; }
    public GXAdaptiveBehavior GetAdaptiveBehavior(GXLayoutStyle? layout = null) => GXAdaptiveBehavior.Create(ResolveLayout(layout ?? _current.Layout), ResponsiveMode, CoarsePointer);

    public void UpdateViewport(int width, bool coarsePointer)
    {
        if (width <= 0)
        {
            return;
        }
        var mode = width <= 768 ? GXResponsiveLayoutMode.Compact : width < 1200 ? GXResponsiveLayoutMode.Medium : GXResponsiveLayoutMode.Expanded;
        if (mode != ResponsiveMode || coarsePointer != CoarsePointer)
        {
            ResponsiveMode = mode;
            CoarsePointer = coarsePointer;
            AppearanceChanged?.Invoke(this, EventArgs.Empty);
        }
    }

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

    public GXAppearanceSettings Current => _current.Copy();
    public GXLayoutStyle EffectiveLayout => ResolveLayout(_current.Layout);
    public GXColorMode EffectiveColorMode => _current.ColorMode == GXColorMode.Auto
        ? (_prefersDark ? GXColorMode.Dark : GXColorMode.Light) : _current.ColorMode;
    public Exception? PersistenceError { get; private set; }
    public event EventHandler? AppearanceChanged;

    public IReadOnlyList<GXThemeDefinition> GetThemes()
    {
        lock (_themes)
        {
            return new ReadOnlyCollection<GXThemeDefinition>(_themes.Values.Select(x => x.Copy()).ToList());
        }
    }

    public void RegisterTheme(GXThemeDefinition theme)
    {
        ArgumentNullException.ThrowIfNull(theme);
        ValidateTheme(theme);
        var copy = theme.Copy();
        lock (_themes)
        {
            if (_builtInIds.Contains(copy.Id))
            {
                throw new ArgumentException("Built-in themes cannot be replaced.", nameof(theme));
            }
            _themes[copy.Id] = copy;
        }
        AppearanceChanged?.Invoke(this, EventArgs.Empty);
    }

    private bool HasTheme(string? id)
    {
        lock (_themes)
        {
            return id != null && _themes.ContainsKey(id);
        }
    }

    public GXLayoutStyle ResolveLayout(GXLayoutStyle requested) => requested != GXLayoutStyle.Auto ? requested : _platform switch
    {
        "Windows" => GXLayoutStyle.Fluent,
        "iOS" or "macOS" => GXLayoutStyle.Cupertino,
        "Android" => GXLayoutStyle.Material,
        _ => GXLayoutStyle.Default
    };

    public void UpdateEnvironment(string? platform, bool prefersDark)
    {
        if (_platform != platform || _prefersDark != prefersDark)
        {
            _platform = platform;
            _prefersDark = prefersDark;
            AppearanceChanged?.Invoke(this, EventArgs.Empty);
        }
    }

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

    public Task SetThemeAsync(string themeId)
    {
        if (!HasTheme(themeId))
        {
            throw new ArgumentException("Select a registered theme ID.", nameof(themeId));
        }
        return ChangeAsync(settings => settings.BootstrapTheme = themeId);
    }

    public Task SetLayoutAsync(GXLayoutStyle layout)
    {
        if (!Enum.IsDefined(layout))
        {
            throw new ArgumentOutOfRangeException(nameof(layout));
        }
        return ChangeAsync(settings => settings.Layout = layout);
    }

    public Task SetColorModeAsync(GXColorMode mode)
    {
        if (!Enum.IsDefined(mode))
        {
            throw new ArgumentOutOfRangeException(nameof(mode));
        }
        return ChangeAsync(settings => settings.ColorMode = mode);
    }

    public Task ResetAsync() => ChangeAsync(settings =>
    {
        settings.Layout = _defaults.Layout;
        settings.BootstrapTheme = _defaults.BootstrapTheme;
        settings.ColorMode = _defaults.ColorMode;
    });

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

    private void Validate(GXAppearanceSettings settings)
    {
        if (!Enum.IsDefined(settings.Layout) || !Enum.IsDefined(settings.ColorMode) || !HasTheme(settings.BootstrapTheme))
        {
            throw new ArgumentException("Invalid appearance settings.");
        }
    }

    private static void ValidateTheme(GXThemeDefinition theme)
    {
        if (!Regex.IsMatch(theme.Id ?? "", "^[a-z][a-z0-9-]{0,63}$") || string.IsNullOrWhiteSpace(theme.Name))
        {
            throw new ArgumentException("A theme requires a lowercase ID and display name.");
        }
        if (theme.CssVariables == null)
        {
            throw new ArgumentException("Theme variables cannot be null.");
        }
        foreach (var variable in theme.CssVariables)
        {
            if (!Regex.IsMatch(variable.Key, "^--[a-zA-Z0-9_-]+$") || variable.Value == null)
            {
                throw new ArgumentException("Theme variables require CSS custom property names and non-null values.");
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
            throw new ArgumentException("Theme CSS must be a local .css asset, or an explicitly allowed HTTP(S) stylesheet.");
        }
    }

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
