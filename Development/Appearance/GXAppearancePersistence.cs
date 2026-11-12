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

using System.Text.Json;

namespace Gurux.UI.Components;

/// <summary>Replaceable persistence for user appearance, independent of application databases.</summary>
public interface IGXAppearancePersistence
{
    Task<GXAppearanceSettings?> LoadAsync();
    Task SaveAsync(GXAppearanceSettings settings);
}

/// <summary>Persists appearance through the existing browser storage abstraction.</summary>
public sealed class GXLocalAppearancePersistence(IGXLocalStorage storage, GXThemeOptions options) : IGXAppearancePersistence
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<GXAppearanceSettings?> LoadAsync()
    {
        var json = await storage.GetValueAsync(options.StorageKey);
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }
        // The layout converter normalizes legacy Gurux names. The next user save
        // writes Default without making a successful read depend on write access.
        return JsonSerializer.Deserialize<GXAppearanceSettings>(json, JsonOptions);
    }

    public Task SaveAsync(GXAppearanceSettings settings) => storage.SetValueAsync(options.StorageKey, JsonSerializer.Serialize(settings, JsonOptions));
}
