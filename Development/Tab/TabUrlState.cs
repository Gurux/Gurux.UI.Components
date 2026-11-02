//
// --------------------------------------------------------------------------
//  Gurux Ltd
//
//
//
// Filename:        $HeadURL$
//
// Version:         $Revision$,
//                  $Date$
//                  $Author$
//
// Copyright (c) Gurux Ltd
//
//---------------------------------------------------------------------------
//
//  DESCRIPTION
//
// This file is a part of Gurux Device Framework.
//
// Gurux Device Framework is Open Source software; you can redistribute it
// and/or modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; version 2 of the License.
// Gurux Device Framework is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
// See the GNU General Public License for more details.
//
// This code is licensed under the GNU General Public License v2.
// Full text may be retrieved at http://www.gnu.org/licenses/gpl-2.0.txt
//---------------------------------------------------------------------------

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
            before.AbsolutePath != after.AbsolutePath || before.Fragment != after.Fragment) return false;
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
            if (key.StartsWith("tab", StringComparison.OrdinalIgnoreCase) && int.TryParse(key.AsSpan(3), out var level) && level > depth + 1)
                changes[key] = null;
        return navigation.GetUriWithQueryParameters((target ?? uri.GetLeftPart(UriPartial.Path)) + uri.Query + uri.Fragment, changes);
    }
}
