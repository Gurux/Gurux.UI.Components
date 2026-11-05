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

/// <summary>A location displayed on a <see cref="GXMap"/>.</summary>
public class GXMapMarker
{
    /// <summary>Gets or sets the latitude in degrees, from -90 to 90.</summary>
    public double Latitude { get; set; }

    /// <summary>Gets or sets the longitude in degrees, from -180 to 180.</summary>
    public double Longitude { get; set; }

    /// <summary>Gets or sets the marker's accessible name and hover text.</summary>
    public string? Title { get; set; }

    /// <summary>Gets or sets an absolute HTTP or HTTPS URL opened in a new tab on click.</summary>
    public string? Url { get; set; }
}
