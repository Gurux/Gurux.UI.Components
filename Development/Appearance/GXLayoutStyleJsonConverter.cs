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

namespace Gurux.UI.Components.Appearance;
/// <summary>Reads legacy Gurux names while writing the current layout names.</summary>
public sealed class LayoutStyleJsonConverter : System.Text.Json.Serialization.JsonConverter<LayoutStyle>
{
    /// <inheritdoc />
    public override LayoutStyle Read(ref System.Text.Json.Utf8JsonReader reader, Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
    {
        if (reader.TokenType == System.Text.Json.JsonTokenType.String)
        {
            var text = reader.GetString();
            if (string.Equals(text, "Gurux", StringComparison.OrdinalIgnoreCase))
            {
                return LayoutStyle.Default;
            }
            if (Enum.TryParse<LayoutStyle>(text, true, out var style) && Enum.IsDefined(style))
            {
                return style;
            }
        }
        else if (reader.TokenType == System.Text.Json.JsonTokenType.Number && reader.TryGetInt32(out var number) && Enum.IsDefined((LayoutStyle)number))
        {
            return (LayoutStyle)number;
        }
        throw new System.Text.Json.JsonException("Invalid layout style.");
    }

    /// <inheritdoc />
    public override void Write(System.Text.Json.Utf8JsonWriter writer, LayoutStyle value, System.Text.Json.JsonSerializerOptions options)
    {
        if (!Enum.IsDefined(value))
        {
            throw new System.Text.Json.JsonException("Invalid layout style.");
        }
        writer.WriteStringValue(value.ToString());
    }
}
