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

using System.Web;
using Microsoft.AspNetCore.Components;

namespace Gurux.UI.Components.Tab;

/// <summary>Stores nested tab selections in URL query parameters without application-specific routing.</summary>
public static class TabUrlState
{
    /// <summary>Identifies same-page changes affecting only tab query parameters.</summary>
    public static bool IsTabNavigation(string source, string target)
    {
        if (!Uri.TryCreate(source, UriKind.Absolute, out var before) || !Uri.TryCreate(before, target, out var after) ||
            before.GetLeftPart(UriPartial.Authority) != after.GetLeftPart(UriPartial.Authority) ||
            before.AbsolutePath != after.AbsolutePath || before.Fragment != after.Fragment)
        {
            return false;
        }
        var oldQuery = HttpUtility.ParseQueryString(before.Query);
        var newQuery = HttpUtility.ParseQueryString(after.Query);
        static bool IsTab(string key) => key.Equals("tab", StringComparison.OrdinalIgnoreCase) ||
            key.StartsWith("tab", StringComparison.OrdinalIgnoreCase) && int.TryParse(key.AsSpan(3), out var depth) && depth >= 2;
        var oldKeys = oldQuery.AllKeys.OfType<string>().Where(k => !IsTab(k)).Order(StringComparer.Ordinal).ToArray();
        var newKeys = newQuery.AllKeys.OfType<string>().Where(k => !IsTab(k)).Order(StringComparer.Ordinal).ToArray();
        return oldKeys.SequenceEqual(newKeys) && oldKeys.All(key =>
            (oldQuery.GetValues(key) ?? []).SequenceEqual(newQuery.GetValues(key) ?? []));
    }
    /// <summary>Gets the query parameter for a zero-based nesting depth.</summary>
    public static string QueryKey(int depth) => depth == 0 ? "tab" : "tab" + (depth + 1);

    /// <summary>Reads an unambiguous selection from the query string, or returns the fallback.</summary>
    public static string? Active(NavigationManager navigation, int depth, string? fallback)
    {
        var query = HttpUtility.ParseQueryString(new Uri(navigation.Uri).Query);
        return query.GetValues(QueryKey(depth)) is { Length: 1 } values ? values[0] : fallback;
    }

    /// <summary>Updates one selection and clears deeper selections while retaining other parameters and the fragment.</summary>
    /// <param name="navigation">Current page navigation.</param>
    /// <param name="depth">Zero-based control nesting depth.</param>
    /// <param name="tab">Selected tab key, or null when a canonical route already identifies the selection.</param>
    /// <param name="target">Optional target page path without its query string or fragment.</param>
    public static string SelectionUrl(NavigationManager navigation, int depth, string? tab, string? target = null)
    {
        var uri = new Uri(navigation.Uri);
        var changes = new Dictionary<string, object?> { [QueryKey(depth)] = tab };
        foreach (var key in HttpUtility.ParseQueryString(uri.Query).AllKeys.OfType<string>())
        {
            if (key.StartsWith("tab", StringComparison.OrdinalIgnoreCase) && int.TryParse(key.AsSpan(3), out var level) && level > depth + 1)
                {
                    changes[key] = null;
                }
        }
        return navigation.GetUriWithQueryParameters((target ?? uri.GetLeftPart(UriPartial.Path)) + uri.Query + uri.Fragment, changes);
    }
}
