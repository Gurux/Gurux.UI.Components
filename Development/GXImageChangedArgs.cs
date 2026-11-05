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

namespace Gurux.UI.Components
{

    /// <summary>
    /// Carries the identifier and updated content of an image resource.
    /// </summary>
    public class GXImageChangedArgs
    {
        /// <summary>
        /// Gets or sets the identifier of the image resource that changed.
        /// </summary>
        public string Id { get; set; } = default!;
        /// <summary>
        /// Gets or sets the updated image content, or an empty value when the resource must be reloaded.
        /// </summary>
        public string? Value { get; set; }
    }
}
