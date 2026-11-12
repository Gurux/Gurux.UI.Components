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

/// <summary>Application layout, independent of Bootstrap theme and color mode.</summary>
[System.Text.Json.Serialization.JsonConverter(typeof(GXLayoutStyleJsonConverter))]
public enum GXLayoutStyle
{
    /// <summary>Preserves the existing layout without platform styling.</summary>
    Default = 0,
    /// <summary>Uses the client platform, falling back to Default.</summary>
    Auto = 1,
    /// <summary>Windows 11-inspired styling.</summary>
    Fluent = 2,
    /// <summary>iOS/macOS-inspired styling.</summary>
    Cupertino = 3,
    /// <summary>Android-inspired styling.</summary>
    Material = 4
}

/// <summary>Reads legacy Gurux names while writing the current layout names.</summary>
public sealed class GXLayoutStyleJsonConverter : System.Text.Json.Serialization.JsonConverter<GXLayoutStyle>
{
    public override GXLayoutStyle Read(ref System.Text.Json.Utf8JsonReader reader, Type typeToConvert, System.Text.Json.JsonSerializerOptions options)
    {
        if (reader.TokenType == System.Text.Json.JsonTokenType.String)
        {
            var text = reader.GetString();
            if (string.Equals(text, "Gurux", StringComparison.OrdinalIgnoreCase))
            {
                return GXLayoutStyle.Default;
            }
            if (Enum.TryParse<GXLayoutStyle>(text, true, out var style) && Enum.IsDefined(style))
            {
                return style;
            }
        }
        else if (reader.TokenType == System.Text.Json.JsonTokenType.Number && reader.TryGetInt32(out var number) && Enum.IsDefined((GXLayoutStyle)number))
        {
            return (GXLayoutStyle)number;
        }
        throw new System.Text.Json.JsonException("Invalid layout style.");
    }

    public override void Write(System.Text.Json.Utf8JsonWriter writer, GXLayoutStyle value, System.Text.Json.JsonSerializerOptions options)
    {
        if (!Enum.IsDefined(value))
        {
            throw new System.Text.Json.JsonException("Invalid layout style.");
        }
        writer.WriteStringValue(value.ToString());
    }
}
