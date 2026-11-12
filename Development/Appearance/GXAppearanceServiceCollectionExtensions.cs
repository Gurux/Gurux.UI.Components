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

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Gurux.UI.Components;

/// <summary>Opt-in appearance registration for WebAssembly and Server circuits.</summary>
public static class GXAppearanceServiceCollectionExtensions
{
    public static IServiceCollection AddGXAppearance(this IServiceCollection services, Action<GXThemeOptions>? configure = null)
    {
        var options = new GXThemeOptions();
        configure?.Invoke(options);
        services.TryAddSingleton(options);
        services.TryAddScoped<IGXLocalStorage, global::GXLocalStorage>();
        services.TryAddScoped<IGXAppearancePersistence, GXLocalAppearancePersistence>();
        services.TryAddScoped<GXThemeService>();
        services.TryAddScoped<IGXThemeService>(provider => provider.GetRequiredService<GXThemeService>());
        return services;
    }
}
