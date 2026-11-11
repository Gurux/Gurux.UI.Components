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

/// <summary>A browser-provided position, which may use GPS, Wi-Fi, cellular networks or other sources.</summary>
public sealed class GXGeoLocationResult
{
    /// <summary>Gets the latitude in degrees.</summary>
    public double Latitude { get; init; }
    /// <summary>Gets the longitude in degrees.</summary>
    public double Longitude { get; init; }
    /// <summary>Gets the reported horizontal accuracy in meters.</summary>
    public double Accuracy { get; init; }
    /// <summary>Gets the altitude in meters, or null when unavailable.</summary>
    public double? Altitude { get; init; }
    /// <summary>Gets the altitude accuracy in meters, or null when unavailable.</summary>
    public double? AltitudeAccuracy { get; init; }
    /// <summary>Gets the heading clockwise from true north in degrees, or null when unavailable.</summary>
    public double? Heading { get; init; }
    /// <summary>Gets the speed in meters per second, or null when unavailable.</summary>
    public double? Speed { get; init; }
    /// <summary>Gets the browser-reported position timestamp in UTC.</summary>
    public DateTimeOffset Timestamp { get; init; }
}
