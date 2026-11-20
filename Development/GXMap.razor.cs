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

using Gurux.UI.Components.Enums;

namespace Gurux.UI.Components;

/// <summary>Displays an interactive OpenStreetMap map using the bundled Leaflet assets.</summary>
/// <remarks>
/// <para>The component uses the bundled Leaflet 1.9.4 JavaScript and CSS files without a Blazor wrapper
/// or additional NuGet dependencies. Assets load automatically on the first interactive render.
/// OpenStreetMap tiles require an internet connection; the host must enable Blazor interactivity.</para>
/// <para>Clicking a marker opens its absolute HTTP or HTTPS URL in a new tab. A marker without a valid
/// URL has no navigation action. <see cref="Height"/> defaults to <c>400px</c> and <see cref="Label"/>
/// supplies the accessible map name. Coordinates, zoom and markers update when parameters change.
/// Unrelated renders preserve the current pan and zoom. Multiple maps can share a page, and map
/// resources are released on disposal. Leaflet's license is included in <c>wwwroot/leaflet/LICENSE</c>.
/// Keep the displayed attribution and follow the
/// <see href="https://operations.osmfoundation.org/policies/tiles/">OpenStreetMap tile usage policy</see>.</para>
/// <para><see cref="ProxyUrl"/> optionally routes tiles through a same-origin GET endpoint that accepts
/// the absolute target URL in its <c>url</c> query parameter and returns the original content type.
/// Relative proxy paths resolve against the application's base address. Attribution and external
/// marker navigation are preserved. The AMI server supplies a compatible endpoint; other applications
/// may provide the same contract. Same-origin requests support cookie authentication.
/// Omitting <see cref="ProxyUrl"/> keeps direct requests.</para>
/// </remarks>
/// <example>
/// <para>Center a map on Tampere and supply a marker with a navigation link.</para>
/// <code language="razor"><![CDATA[
/// @using Gurux.UI.Components
///
/// <GXMap Latitude="61.4978"
///        Longitude="23.7610"
///        Zoom="15"
///        Markers="@_markers" />
///
/// @code {
///     private readonly List<GXMapMarker> _markers =
///     [
///         new() { Latitude = 61.4978, Longitude = 23.7610,
///                 Title = "Tampere", Url = "https://www.tampere.fi/" }
///     ];
/// }
/// ]]></code>
/// </example>
/// <example>
/// <para>Use the application's same-origin HTTP proxy for tile requests, with the marker list above.</para>
/// <code language="razor"><![CDATA[
/// <GXMap Latitude="61.4978" Longitude="23.7610" Zoom="15"
///        Markers="@_markers" ProxyUrl="api/HttpProxy" />
/// ]]></code>
/// </example>
public partial class GXMap
{
}
