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

namespace Gurux.UI.Components.TemplateToken;

/// <summary>Application-provided token discovery and preview operations for the editor.</summary>
public interface ITemplateTokenEditorService
{
    /// <summary>
    /// Lists tokens available in the current editor context.
    /// </summary>
    Task<IReadOnlyList<TemplateTokenDefinition>> GetDefinitionsAsync(TemplateTokenEditorContext context,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resolves template text for a plain-text preview without modifying the template.
    /// </summary>
    Task<string?> ResolveAsync(string text, TemplateTokenEditorContext context,
        CancellationToken cancellationToken = default);
}
