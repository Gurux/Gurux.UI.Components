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

namespace Gurux.UI.Components.Enums
{
    /// <summary>
    /// Defines how an image value is interpreted and rendered.
    /// </summary>
    public enum ImageType : byte
    {
        /// <summary>
        /// Detects the rendering type from the image value.
        /// </summary>
        Auto,
        /// <summary>
        /// Treats the value as a raster image URI or Base64 image content.
        /// </summary>
        Raster,
        /// <summary>
        /// Treats the value as SVG markup.
        /// </summary>
        Svg,
        /// <summary>
        /// Treats the value as an icon ligature or icon class identifier.
        /// </summary>
        IconLigature,
        /// <summary>
        /// Treats the value as HTML markup.
        /// </summary>
        Html
    }
}