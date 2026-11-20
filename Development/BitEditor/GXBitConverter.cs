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

using System.Globalization;
using System.Numerics;
using System.Text;

using Gurux.UI.Components.Enums;

namespace Gurux.UI.Components.BitEditor;

/// <summary>Lossless conversion between bit editor values and byte storage.</summary>
public static class GXBitConverter
{
    /// <summary>Returns whether a type is a supported integer or enum.</summary>
    /// <param name="type">The value type to inspect.</param>
    /// <returns>Whether the type is a supported integer or enum.</returns>
    public static bool IsNumeric(Type type) => NumericSize(type) != 0;

    /// <summary>Determines the fixed byte width of an integer or enum type.</summary>
    /// <param name="type">The value type to inspect.</param>
    /// <returns>The byte width, or zero for an unsupported type.</returns>
    private static int NumericSize(Type type)
    {
        if (type.IsEnum)
        {
            type = Enum.GetUnderlyingType(type);
        }
        if (type == typeof(byte) || type == typeof(sbyte))
        {
            return 1;
        }
        if (type == typeof(short) || type == typeof(ushort))
        {
            return 2;
        }
        if (type == typeof(int) || type == typeof(uint))
        {
            return 4;
        }
        if (type == typeof(long) || type == typeof(ulong))
        {
            return 8;
        }
        if (type == typeof(Int128) || type == typeof(UInt128))
        {
            return 16;
        }
        return 0;
    }

    /// <summary>Checks whether an integer type includes negative values.</summary>
    /// <param name="type">The underlying integer type.</param>
    /// <returns>Whether the type is a supported signed integer.</returns>
    private static bool IsSigned(Type type) => type == typeof(sbyte) || type == typeof(short) ||
        type == typeof(int) || type == typeof(long) || type == typeof(Int128);

    /// <summary>Clones an encoding and rejects invalid text and byte sequences.</summary>
    /// <param name="encoding">The encoding to clone, or null for UTF-8.</param>
    /// <returns>An independent encoding with exception fallbacks.</returns>
    internal static Encoding StrictEncoding(Encoding? encoding)
    {
        var result = (Encoding)(encoding ?? Encoding.UTF8).Clone();
        result.EncoderFallback = EncoderFallback.ExceptionFallback;
        result.DecoderFallback = DecoderFallback.ExceptionFallback;
        return result;
    }

    /// <summary>Copies a supported value into byte storage. Bit strings are packed MSB first, preserving character order.</summary>
    /// <typeparam name="T">The supported integer, enum, byte array or string value type.</typeparam>
    /// <param name="value">The value to copy; null strings and arrays become empty storage.</param>
    /// <param name="byteOrder">The byte order for integer values.</param>
    /// <param name="stringMode">Whether strings contain bits or encoded text.</param>
    /// <param name="encoding">The text encoding, or null for UTF-8.</param>
    /// <returns>A new byte array containing the value.</returns>
    /// <exception cref="FormatException">A bit string contains a character other than zero or one.</exception>
    /// <exception cref="EncoderFallbackException">Text cannot be represented by the selected encoding.</exception>
    /// <exception cref="NotSupportedException">The value type is unsupported.</exception>
    public static byte[] ToBytes<T>(T? value, ByteOrder byteOrder = ByteOrder.BigEndian,
        BitStringMode stringMode = BitStringMode.BitString, Encoding? encoding = null)
    {
        var type = typeof(T);
        if (type == typeof(byte[]))
        {
            return value is byte[] bytes ? (byte[])bytes.Clone() : [];
        }
        if (type == typeof(string))
        {
            var text = (string?)(object?)value ?? string.Empty;
            if (stringMode == BitStringMode.Text)
            {
                return StrictEncoding(encoding).GetBytes(text);
            }
            var result = new byte[(text.Length + 7) / 8];
            for (int i = 0; i < text.Length; ++i)
            {
                if (text[i] != '0' && text[i] != '1')
                {
                    throw new FormatException(Properties.Resources.ResourceManager.GetString("BitEditorInvalidBitString"));
                }
                if (text[i] == '1')
                {
                    result[i / 8] |= (byte)(128 >> (i % 8));
                }
            }
            return result;
        }
        int size = NumericSize(type);
        if (size == 0)
        {
            throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, Properties.Resources.ResourceManager.GetString("BitEditorUnsupportedValueType")!, type));
        }
        object boxed = value!;
        if (type.IsEnum)
        {
            boxed = Convert.ChangeType(boxed, Enum.GetUnderlyingType(type), CultureInfo.InvariantCulture);
        }
        var number = BigInteger.Parse(Convert.ToString(boxed, CultureInfo.InvariantCulture)!, CultureInfo.InvariantCulture);
        if (number < 0)
        {
            number += BigInteger.One << (size * 8);
        }
        var data = new byte[size];
        number.TryWriteBytes(data, out _, isUnsigned: true, isBigEndian: false);
        if (byteOrder == ByteOrder.BigEndian)
        {
            Array.Reverse(data);
        }
        return data;
    }

    /// <summary>Restores the original type from byte storage. Invalid text encodings throw a decoder fallback exception.</summary>
    /// <typeparam name="T">The supported integer, enum, byte array or string result type.</typeparam>
    /// <param name="bytes">The storage to read.</param>
    /// <param name="bitCount">The number of meaningful bits in the storage.</param>
    /// <param name="byteOrder">The byte order for integer values.</param>
    /// <param name="stringMode">Whether strings contain bits or decoded text.</param>
    /// <param name="encoding">The text encoding, or null for UTF-8.</param>
    /// <returns>The value reconstructed from the storage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The bit count is negative or exceeds available storage.</exception>
    /// <exception cref="ArgumentException">The storage width does not match the numeric type.</exception>
    /// <exception cref="DecoderFallbackException">The bytes are invalid in the selected encoding.</exception>
    /// <exception cref="NotSupportedException">The result type is unsupported.</exception>
    public static T FromBytes<T>(ReadOnlySpan<byte> bytes, int bitCount,
        ByteOrder byteOrder = ByteOrder.BigEndian, BitStringMode stringMode = BitStringMode.BitString,
        Encoding? encoding = null)
    {
        if (bitCount < 0 || bitCount > (long)bytes.Length * 8)
        {
            throw new ArgumentOutOfRangeException(nameof(bitCount));
        }
        var type = typeof(T);
        if (type == typeof(byte[]))
        {
            return (T)(object)bytes.ToArray();
        }
        if (type == typeof(string))
        {
            if (stringMode == BitStringMode.Text)
            {
                return (T)(object)StrictEncoding(encoding).GetString(bytes);
            }
            var chars = new char[bitCount];
            for (int i = 0; i < bitCount; ++i)
            {
                chars[i] = (bytes[i / 8] & (128 >> (i % 8))) == 0 ? '0' : '1';
            }
            return (T)(object)new string(chars);
        }
        int size = NumericSize(type);
        if (size == 0)
        {
            throw new NotSupportedException(string.Format(CultureInfo.CurrentCulture, Properties.Resources.ResourceManager.GetString("BitEditorUnsupportedValueType")!, type));
        }
        if (bytes.Length != size || bitCount != size * 8)
        {
            throw new ArgumentException(Properties.Resources.ResourceManager.GetString("BitEditorInvalidNumericWidth"), nameof(bytes));
        }
        var target = type.IsEnum ? Enum.GetUnderlyingType(type) : type;
        var number = new BigInteger(bytes, isUnsigned: !IsSigned(target), isBigEndian: byteOrder == ByteOrder.BigEndian);
        object value = target == typeof(Int128) ? (Int128)number : target == typeof(UInt128) ? (UInt128)number :
            Convert.ChangeType(number.ToString(CultureInfo.InvariantCulture), target, CultureInfo.InvariantCulture);
        return (T)(type.IsEnum ? Enum.ToObject(type, value) : value);
    }

    /// <summary>Parses invariant decimal input into an integer or enum value.</summary>
    /// <typeparam name="T">The target integer or enum type.</typeparam>
    /// <param name="text">The invariant decimal representation.</param>
    /// <returns>The parsed integer or enum value.</returns>
    /// <exception cref="FormatException">The input is not a valid number.</exception>
    /// <exception cref="OverflowException">The number exceeds the target type range.</exception>
    /// <exception cref="InvalidCastException">The target type does not support conversion.</exception>
    internal static T ParseNumber<T>(string text)
    {
        var type = typeof(T);
        var target = type.IsEnum ? Enum.GetUnderlyingType(type) : type;
        object value = target == typeof(Int128) ? Int128.Parse(text, CultureInfo.InvariantCulture) :
            target == typeof(UInt128) ? UInt128.Parse(text, CultureInfo.InvariantCulture) :
            Convert.ChangeType(text, target, CultureInfo.InvariantCulture);
        return (T)(type.IsEnum ? Enum.ToObject(type, value) : value);
    }

    /// <summary>Formats a numeric value using invariant decimal notation.</summary>
    /// <typeparam name="T">The integer or enum value type.</typeparam>
    /// <param name="value">The value to format.</param>
    /// <returns>The invariant decimal representation of the underlying value.</returns>
    internal static string FormatNumber<T>(T value)
    {
        object boxed = value!;
        if (typeof(T).IsEnum)
        {
            boxed = Convert.ChangeType(boxed, Enum.GetUnderlyingType(typeof(T)), CultureInfo.InvariantCulture);
        }
        return Convert.ToString(boxed, CultureInfo.InvariantCulture)!;
    }
}
