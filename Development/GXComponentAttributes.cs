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

namespace Gurux.UI.Components;

/// <summary>
/// Resolves component identifiers and combines captured HTML classes and styles without modifying the caller's attributes.
/// </summary>
public static class GXComponentAttributes
{
    /// <summary>
    /// Returns the supplied nonempty id, otherwise lazily creates and caches a Guid id in the caller's generatedId field.
    /// </summary>
    /// <param name="attributes">Captured HTML attributes that may contain an id.</param>
    /// <param name="generatedId">The per-component fallback id, initialized on the first call that needs it.</param>
    /// <returns>The supplied id or the stable generated fallback.</returns>
    public static string GetId(IEnumerable<KeyValuePair<string, object>>? attributes, ref string? generatedId)
    {
        string? id = GetValue(attributes, "id");
        return !string.IsNullOrWhiteSpace(id) ? id : generatedId ??= Guid.NewGuid().ToString("N");
    }

    /// <summary>
    /// Returns the named attribute's string value, checking dictionary keys directly and otherwise comparing names without case sensitivity.
    /// </summary>
    public static string? GetValue(IEnumerable<KeyValuePair<string, object>>? attributes, string name)
    {
        if (attributes == null)
        {
            return null;
        }
        if (attributes is IReadOnlyDictionary<string, object> dictionary && dictionary.TryGetValue(name, out var value))
        {
            return value?.ToString();
        }
        return attributes.FirstOrDefault(a => a.Key.Equals(name, StringComparison.OrdinalIgnoreCase)).Value?.ToString();
    }

    /// <summary>
    /// Combines default CSS classes with the captured class attribute.
    /// </summary>
    public static string? GetClass(IEnumerable<KeyValuePair<string, object>>? attributes, string? defaults = null)
        => CombineClasses(defaults, GetValue(attributes, "class"));

    /// <summary>
    /// Combines nonempty CSS class lists and removes duplicate class names while preserving their order.
    /// </summary>
    public static string? CombineClasses(params string?[] values)
    {
        var result = string.Join(' ', values.Where(v => !string.IsNullOrWhiteSpace(v))
            .SelectMany(v => v!.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)).Distinct(StringComparer.Ordinal));
        return result.Length == 0 ? null : result;
    }

    /// <summary>
    /// Combines default inline styles with the captured style attribute.
    /// </summary>
    public static string? GetStyle(IEnumerable<KeyValuePair<string, object>>? attributes, string? defaults = null)
        => CombineStyles(defaults, GetValue(attributes, "style"));

    /// <summary>
    /// Combines nonempty CSS declarations with semicolon separators and a trailing semicolon.
    /// </summary>
    public static string? CombineStyles(params string?[] values)
    {
        var result = string.Join(';', values.Where(v => !string.IsNullOrWhiteSpace(v)).Select(v => v!.Trim().TrimEnd(';')));
        return result.Length == 0 ? null : result + ";";
    }

    /// <summary>
    /// Copies captured attributes excluding class and style, or returns null when no attributes are supplied.
    /// </summary>
    public static IReadOnlyDictionary<string, object>? WithoutCss(IEnumerable<KeyValuePair<string, object>>? attributes)
        => attributes?.Where(a => !a.Key.Equals("class", StringComparison.OrdinalIgnoreCase)
            && !a.Key.Equals("style", StringComparison.OrdinalIgnoreCase)).ToDictionary(a => a.Key, a => a.Value);
}
