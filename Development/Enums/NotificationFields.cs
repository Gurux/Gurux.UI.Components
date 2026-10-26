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

namespace Gurux.UI.Components.Enums;

/// <summary>
/// Specifies optional notification fields to display; flags can be combined.
/// </summary>
[Flags]
public enum NotificationFields
{
    /// <summary>
    /// Displays no optional notification fields.
    /// </summary>
    None = 0,
    /// <summary>
    /// Displays the notification title.
    /// </summary>
    Title = 1,
    /// <summary>
    /// Displays the notification creation time.
    /// </summary>
    CreatedAt = 2,
    /// <summary>
    /// Displays the notification identifier.
    /// </summary>
    Id = 4,
    /// <summary>
    /// Displays every optional notification field.
    /// </summary>
    All = Title | CreatedAt | Id
}