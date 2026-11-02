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

using Microsoft.AspNetCore.Components;

namespace Gurux.UI.Components.Tab;

/// <summary>Maps optional application route groups to persistent tab selections.</summary>
public interface ITabRouteResolver
{
    /// <summary>Identifies navigation retaining the current page.</summary>
    bool IsTabNavigation(string source, string target) => TabUrlState.IsTabNavigation(source, target);
    /// <summary>Resolves the active tab from the URL and the supplied fallback.</summary>
    string? GetActive(NavigationManager navigation, string? routeGroup, int depth, string? fallback);
    /// <summary>Builds the URL for a selected tab while retaining unrelated page parameters.</summary>
    string GetSelectionUrl(NavigationManager navigation, string? routeGroup, int depth, string tab);
}
