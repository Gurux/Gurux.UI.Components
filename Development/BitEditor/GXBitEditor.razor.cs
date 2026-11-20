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
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

using Gurux.UI.Components.Enums;

namespace Gurux.UI.Components.BitEditor;

/// <summary>
/// Displays and edits individual bits of integers, enums, byte arrays and strings.
/// </summary>
/// <typeparam name="TValue">
/// The bound value type: byte, sbyte, short, ushort, int, uint, long, ulong,
/// Int128, UInt128, an enum, byte[] or string.
/// </typeparam>
/// <remarks>
/// <para>
/// Supports two-way binding through the inherited <c>Value</c> and <c>ValueChanged</c>
/// parameters and validation within an <see cref="EditForm"/>. Razor's
/// <c>@bind-Value</c> directive supplies <c>ValueExpression</c> automatically;
/// provide it explicitly when assigning <c>Value</c> and <c>ValueChanged</c> separately.
/// </para>
/// <para>
/// Rows default to 16 bits with groups of eight. <see cref="AutoWrap"/> reduces the
/// row length to fit the available width, preferring complete bytes. Disable it
/// to retain <see cref="BitsPerRow"/> and allow horizontal scrolling.
/// <see cref="Virtualize"/> renders complete rows in a bounded scrolling viewport.
/// </para>
/// <para>
/// Numeric bit zero is the least significant bit. <see cref="ByteOrder"/> controls
/// numeric byte storage and display; byte arrays retain their supplied byte order.
/// <see cref="BitOrder"/> controls the display order within each byte.
/// In <see cref="BitStringMode.BitString"/> mode, string characters always map
/// to logical indexes from left to right, independently of both order settings.
/// Leading zeros and bit counts that are not multiples of eight are preserved.
/// </para>
/// <para>
/// Select <see cref="BitStringMode.Text"/> with <see cref="StringMode"/> to edit
/// encoded text. <see cref="Encoding"/> defaults to UTF-8. Edits that would produce
/// invalid encoded text are rejected and reported through validation.
/// </para>
/// <para>
/// <see cref="BitDefinitions"/> supplies bit names, descriptions and read-only
/// restrictions. <see cref="ReadOnly"/>, <see cref="Disabled"/> and per-bit
/// restrictions apply to individual bits, representation fields and bulk operations.
/// <see cref="NamedValues"/> optionally displays named numeric masks as checkboxes.
/// Hide the bulk editing buttons with <see cref="ShowToolbar"/>.
/// </para>
/// <para>
/// <see cref="DisplayFormats"/> selects the available binary, hexadecimal, decimal
/// and text representations. Combine <see cref="GXBitEditorFormats"/> flags to
/// enable several formats, or use <see cref="GXBitEditorFormats.None"/> to hide
/// the grid, toolbar and all representation fields. The default is <see cref="GXBitEditorFormats.All"/>.
/// Include <see cref="GXBitEditorFormats.Editor"/> to display the bit grid and toolbar.
/// <see cref="ShowFormatSelector"/> controls the representation dropdown.
/// <see cref="GXBitEditorFormats.Editor"/> selects the bit grid and its toolbar
/// and is selected by default unless a saved preference is restored.
/// If <see cref="IGXLocalStorage"/>
/// is registered, the selection is restored and saved under
/// <c>GXBitEditor:DisplayFormat</c>.
/// </para>
/// <para>
/// Representation fields edit the current fixed bit width. Assign a new bound
/// value from the parent to resize the editor. External value changes establish
/// a new original value; binding echoes and layout changes retain the existing
/// baseline for <see cref="RestoreAsync"/> and modified-bit highlighting.
/// </para>
/// <para>
/// Tab moves focus between controls; Space or Enter toggles a focused editable bit.
/// Arrow keys navigate bits, and Home and End reach the beginning and end,
/// including virtualized rows.
/// </para>
/// </remarks>
/// <example>
/// Bind numeric values, bit strings or large byte arrays in Razor:
/// <code language="razor"><![CDATA[
/// @using Gurux.UI.Components.BitEditor
/// @using Gurux.UI.Components.Enums
///
/// <GXBitEditor TValue="uint" @bind-Value="status" />
/// <GXBitEditor TValue="string" @bind-Value="flags"
///              StringMode="BitStringMode.BitString" />
/// <GXBitEditor TValue="byte[]" @bind-Value="payload"
///              BitsPerRow="32" Virtualize="true" />
///
/// @code {
///     uint status;
///     string flags = "00101";
///     byte[] payload = new byte[512];
/// }
/// ]]></code>
/// </example>
public partial class GXBitEditor<TValue>
{
    /// <summary>Resolves optional local storage without requiring applications to register it.</summary>
    [Inject]
    private IServiceProvider Services { get; set; } = default!;

    /// <summary>Identifies the shared user preference for the numeric representation.</summary>
    private const string FormatStorageKey = "GXBitEditor:DisplayFormat";

    /// <summary>Retains the preferred representation across value and parameter updates.</summary>
    private GXBitEditorFormats _preferredFormat = GXBitEditorFormats.Editor;

    /// <summary>Detects a user selection made while a saved preference is being read.</summary>
    private int _formatSelectionVersion;

    /// <summary>Gets the allowed numeric representations supported by the current value type.</summary>
    private IEnumerable<GXBitEditorFormats> AvailableFormats => new[]
        { GXBitEditorFormats.Editor, GXBitEditorFormats.Binary, GXBitEditorFormats.Hexadecimal, GXBitEditorFormats.Decimal }
        .Where(format => DisplayFormats.HasFlag(format) && (format != GXBitEditorFormats.Decimal || IsNumeric));

    /// <summary>Gets the preferred representation or the first one allowed by the caller.</summary>
    private GXBitEditorFormats SelectedFormat => AvailableFormats.Contains(_preferredFormat)
        ? _preferredFormat : AvailableFormats.FirstOrDefault();

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender && Services.GetService<IGXLocalStorage>() is { } storage)
        {
            var version = _formatSelectionVersion;
            var saved = await storage.GetValueAsync(FormatStorageKey);
            if (version == _formatSelectionVersion && Enum.TryParse<GXBitEditorFormats>(saved, out var format) &&
                format is GXBitEditorFormats.Editor or GXBitEditorFormats.Binary or GXBitEditorFormats.Hexadecimal or GXBitEditorFormats.Decimal)
            {
                _preferredFormat = format;
                StateHasChanged();
            }
        }
    }

    /// <summary>Changes only the displayed representation and optionally persists the user's preference.</summary>
    /// <param name="args">The format selected from the dropdown.</param>
    /// <returns>A task that completes after the optional preference write.</returns>
    private async Task ChangeDisplayFormatAsync(ChangeEventArgs args)
    {
        if (Enum.TryParse<GXBitEditorFormats>(args.Value?.ToString(), out var format) && AvailableFormats.Contains(format))
        {
            ++_formatSelectionVersion;
            _preferredFormat = format;
            if (Services.GetService<IGXLocalStorage>() is { } storage)
            {
                await storage.SetValueAsync(FormatStorageKey, format.ToString());
            }
        }
    }
    /// <summary>Byte order used for numeric storage and display.</summary>
    [Parameter] public ByteOrder ByteOrder { get; set; } = ByteOrder.BigEndian;
    /// <summary>Display order of bits within each byte; ignored for bit strings.</summary>
    [Parameter] public BitOrder BitOrder { get; set; } = BitOrder.MsbFirst;
    /// <summary>Interpretation of string values; defaults to a bit string.</summary>
    [Parameter] public BitStringMode StringMode { get; set; } = BitStringMode.BitString;
    /// <summary>Encoding used in text mode; invalid sequences are rejected.</summary>
    [Parameter] public Encoding Encoding { get; set; } = Encoding.UTF8;
    /// <summary>Whether row lengths adapt to the available width.</summary>
    [Parameter] public bool AutoWrap { get; set; } = true;
    /// <summary>Preferred maximum number of bits per row; defaults to 16.</summary>
    [Parameter] public int BitsPerRow { get; set; } = 16;
    /// <summary>Number of bits per visual group; defaults to eight.</summary>
    [Parameter] public int GroupSize { get; set; } = 8;
    /// <summary>Whether logical bit indexes are displayed.</summary>
    [Parameter] public bool ShowBitIndex { get; set; } = true;
    /// <summary>Whether storage byte indexes are displayed.</summary>
    [Parameter] public bool ShowByteIndex { get; set; }
    /// <summary>Whether complete rows are virtualized.</summary>
    [Parameter] public bool Virtualize { get; set; }
    /// <summary>Whether all editing is prevented.</summary>
    [Parameter] public bool ReadOnly { get; set; }
    /// <summary>Whether the editor is disabled.</summary>
    [Parameter] public bool Disabled { get; set; }
    /// <summary>Whether bulk editing buttons are displayed.</summary>
    [Parameter] public bool ShowToolbar { get; set; } = true;
    /// <summary>Gets or sets whether the numeric format selector is displayed above the value.</summary>
    [Parameter] public bool ShowFormatSelector { get; set; } = true;
    /// <summary>Value representations available in the format dropdown; defaults to every supported representation.</summary>
    [Parameter] public GXBitEditorFormats DisplayFormats { get; set; } = GXBitEditorFormats.All;
    /// <summary>Gets whether at least one selected representation supports the current value type.</summary>
    private bool HasDisplayFormats => (DisplayFormats & (GXBitEditorFormats.Editor | GXBitEditorFormats.Binary | GXBitEditorFormats.Hexadecimal)) != 0 ||
        IsNumeric && DisplayFormats.HasFlag(GXBitEditorFormats.Decimal) ||
        typeof(TValue) == typeof(string) && !IsBitString && DisplayFormats.HasFlag(GXBitEditorFormats.Text);
    /// <summary>Names, descriptions and read-only states keyed by logical bit index.</summary>
    [Parameter] public IEnumerable<GXBitDefinition>? BitDefinitions { get; set; }

    /// <summary>Optional named numeric masks displayed as checkboxes above the bit grid.</summary>
    /// <remarks>Named masks use unsigned 64-bit conversion. Use nonnegative numeric values representable
    /// by <see cref="ulong"/> and masks that fit the bound <typeparamref name="TValue"/> type.</remarks>
    [Parameter] public IReadOnlyDictionary<ulong, string>? NamedValues { get; set; }

    /// <summary>Gets the unsigned 64-bit mask parsed from the numeric value, or zero when it cannot be parsed.</summary>
    private ulong NamedMask => ulong.TryParse(GXBitConverter.FormatNumber(Value!), NumberStyles.Integer, CultureInfo.InvariantCulture, out var mask) ? mask : 0;

    /// <summary>Checks whether all bits of a named mask are selected.</summary>
    /// <param name="mask">The named numeric mask; zero represents a cleared value.</param>
    /// <returns>Whether the named value is selected.</returns>
    private bool IsNamedValueSet(ulong mask) => mask == 0 ? NamedMask == 0 : (NamedMask & mask) == mask;

    /// <summary>Updates a named mask while preserving unrelated bits and editor restrictions.</summary>
    /// <param name="mask">The numeric mask to update.</param>
    /// <param name="selected">Whether the named mask should be selected.</param>
    private void ChangeNamedValue(ulong mask, bool selected)
    {
        Edit(() =>
        {
            var next = mask == 0 ? (selected ? 0 : NamedMask) : selected ? NamedMask | mask : NamedMask & ~mask;
            ApplyRepresentation(GXBitConverter.ToBytes(GXBitConverter.ParseNumber<TValue>(next.ToString(CultureInfo.InvariantCulture)), ByteOrder), _bitCount);
        });
    }
    /// <summary>The current editable storage and original baseline for restore operations.</summary>
    private byte[] _bytes = [], _original = [];
    /// <summary>The number of meaningful bits in the current storage.</summary>
    private int _bitCount;
    /// <summary>Whether parameters have been read and whether the supplied value is invalid.</summary>
    private bool _initialized, _invalidValue;
    /// <summary>A snapshot of the most recently accepted bound value.</summary>
    private TValue? _lastValue;
    /// <summary>The byte order used by the preceding parameter update.</summary>
    private ByteOrder _lastByteOrder;
    /// <summary>The string interpretation used by the preceding parameter update.</summary>
    private BitStringMode _lastMode;
    /// <summary>The encoding used by the preceding parameter update.</summary>
    private Encoding? _lastEncoding;
    /// <summary>The current localized validation message, if any.</summary>
    private string? _error;
    /// <summary>Validation messages associated with the bound edit context.</summary>
    private ValidationMessageStore? _messages;
    /// <summary>Bit metadata indexed by logical bit position.</summary>
    private readonly Dictionary<int, GXBitDefinition> _definitions = new();
    /// <summary>Gets whether the bound value has a fixed integer width.</summary>
    private bool IsNumeric => GXBitConverter.IsNumeric(typeof(TValue));
    /// <summary>Gets whether the bound string contains literal binary digits.</summary>
    private bool IsBitString => typeof(TValue) == typeof(string) && StringMode == BitStringMode.BitString;
    /// <summary>Gets whether editor state prevents value changes.</summary>
    private bool CannotEdit => ReadOnly || Disabled || _invalidValue;
    /// <summary>Gets the current storage as a binary digit string.</summary>
    private string Binary => GXBitConverter.FromBytes<string>(_bytes, _bitCount);

    /// <inheritdoc />
    /// <exception cref="ArgumentOutOfRangeException">The row width or group size is not positive.</exception>
    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        if (BitsPerRow <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(BitsPerRow));
        }
        if (GroupSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(GroupSize));
        }
        _definitions.Clear();
        if (BitDefinitions is not null)
        {
            foreach (var definition in BitDefinitions)
            {
                _definitions[definition.Index] = definition;
            }
        }
        bool sameValue = Value is byte[] array && _lastValue is byte[] previous
            ? array.AsSpan().SequenceEqual(previous) : EqualityComparer<TValue>.Default.Equals(Value, _lastValue);
        bool sameMode = _lastMode == StringMode && Equals(_lastEncoding, Encoding);
        if (_initialized && sameValue && sameMode && !_invalidValue)
        {
            if (IsNumeric && _lastByteOrder != ByteOrder)
            {
                Array.Reverse(_bytes);
                Array.Reverse(_original);
            }
            _lastByteOrder = ByteOrder;
            return;
        }
        try
        {
            _bytes = GXBitConverter.ToBytes(Value, ByteOrder, StringMode, Encoding);
            _bitCount = IsBitString ? ((string?)(object?)Value)?.Length ?? 0 : checked(_bytes.Length * 8);
            _original = (byte[])_bytes.Clone();
            RememberValue();
            _invalidValue = false;
            SetError(null);
        }
        catch (Exception ex) when (ex is FormatException or EncoderFallbackException)
        {
            _bytes = [];
            _original = [];
            _bitCount = 0;
            _invalidValue = true;
            SetError(Properties.Resources.BitEditorInvalidValue);
        }
        _initialized = true;
        _lastByteOrder = ByteOrder;
        _lastMode = StringMode;
        _lastEncoding = Encoding;
    }

    /// <summary>Snapshots the bound value so binding echoes preserve the original baseline.</summary>
    private void RememberValue() => _lastValue = Value is byte[] array ? (TValue)(object)array.Clone() : Value;

    /// <summary>Updates the component error and bound field validation messages.</summary>
    /// <param name="error">The localized error, or null to clear validation.</param>
    private void SetError(string? error)
    {
        _error = error;
        if (EditContext is null)
        {
            return;
        }
        _messages ??= new ValidationMessageStore(EditContext);
        _messages.Clear(FieldIdentifier);
        if (error is not null)
        {
            _messages.Add(FieldIdentifier, error);
        }
        EditContext.NotifyValidationStateChanged();
    }

    /// <summary>Maps a logical bit index to its storage byte and mask.</summary>
    /// <param name="index">The zero-based logical bit index.</param>
    /// <returns>The storage byte index and single-bit mask.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside the current bit count.</exception>
    private (int Byte, int Mask) Location(int index)
    {
        if (index < 0 || index >= _bitCount)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }
        if (IsBitString)
        {
            return (index / 8, 128 >> (index % 8));
        }
        int b = index / 8;
        if (IsNumeric && ByteOrder == ByteOrder.BigEndian)
        {
            b = _bytes.Length - 1 - b;
        }
        return (b, 1 << (index % 8));
    }
    /// <summary>Checks whether metadata prevents editing a logical bit.</summary>
    /// <param name="index">The zero-based logical bit index.</param>
    /// <returns>Whether the bit has read-only metadata.</returns>
    private bool Locked(int index) => _definitions.TryGetValue(index, out var definition) && definition.ReadOnly;

    /// <summary>Toggles a logical bit, respecting editor and bit read-only states.</summary>
    /// <param name="index">The zero-based logical bit index, independent of display order.</param>
    /// <returns>A task that completes after the render update is requested.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside the current bit count.</exception>
    public Task ToggleBitAsync(int index) => ChangeBitAsync(index, null);
    /// <summary>Sets a logical bit, respecting editor and bit read-only states.</summary>
    /// <param name="index">The zero-based logical bit index, independent of display order.</param>
    /// <returns>A task that completes after the render update is requested.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside the current bit count.</exception>
    public Task SetBitAsync(int index) => ChangeBitAsync(index, true);
    /// <summary>Clears a logical bit, respecting editor and bit read-only states.</summary>
    /// <param name="index">The zero-based logical bit index, independent of display order.</param>
    /// <returns>A task that completes after the render update is requested.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside the current bit count.</exception>
    public Task ClearBitAsync(int index) => ChangeBitAsync(index, false);
    /// <summary>Changes one editable bit and validates the resulting text value.</summary>
    /// <param name="index">The zero-based logical bit index.</param>
    /// <param name="value">The desired state, or null to toggle the bit.</param>
    /// <returns>A task that completes when the render update is requested.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The index is outside the current bit count.</exception>
    private Task ChangeBitAsync(int index, bool? value)
    {
        var (b, mask) = Location(index);
        if (CannotEdit || Locked(index))
        {
            return Task.CompletedTask;
        }
        byte old = _bytes[b];
        _bytes[b] = (byte)(value is null ? old ^ mask : value.Value ? old | mask : old & ~mask);
        try
        { Publish(); }
        catch (DecoderFallbackException) { _bytes[b] = old; SetError(Properties.Resources.BitEditorInvalidEncoding); }
        return InvokeAsync(StateHasChanged);
    }

    /// <summary>Converts storage to the bound value and clears prior validation errors.</summary>
    /// <exception cref="DecoderFallbackException">The edited bytes do not form valid text.</exception>
    private void Publish()
    {
        TValue next = GXBitConverter.FromBytes<TValue>(_bytes, _bitCount, ByteOrder, StringMode, Encoding);
        SetError(null);
        _lastValue = next is byte[] array ? (TValue)(object)array.Clone() : next;
        CurrentValue = next;
    }

    /// <summary>Sets all editable bits.</summary>
    /// <returns>A task that completes after the render update is requested.</returns>
    public Task SetAllAsync() => ApplyAll(1);
    /// <summary>Clears all editable bits.</summary>
    /// <returns>A task that completes after the render update is requested.</returns>
    public Task ClearAllAsync() => ApplyAll(0);
    /// <summary>Inverts all editable bits.</summary>
    /// <returns>A task that completes after the render update is requested.</returns>
    public Task InvertAllAsync() => ApplyAll(2);
    /// <summary>Restores editable bits to the value last received from the parent.</summary>
    /// <returns>A task that completes after the render update is requested.</returns>
    public Task RestoreAsync() => ApplyAll(3);
    /// <summary>Applies a bulk operation while preserving read-only bits.</summary>
    /// <param name="operation">Zero clears, one sets, two inverts, and other values restore original bits.</param>
    /// <returns>A task that completes when the render update is requested.</returns>
    private Task ApplyAll(int operation)
    {
        if (CannotEdit)
        {
            return Task.CompletedTask;
        }
        var backup = (byte[])_bytes.Clone();
        for (int i = 0; i < _bitCount; ++i)
        {
            if (Locked(i))
            {
                continue;
            }
            var (b, mask) = Location(i);
            _bytes[b] = (byte)(operation switch
            {
                0 => _bytes[b] & ~mask,
                1 => _bytes[b] | mask,
                2 => _bytes[b] ^ mask,
                _ => (_bytes[b] & ~mask) | (_original[b] & mask)
            });
        }
        try
        { Publish(); }
        catch (DecoderFallbackException) { _bytes = backup; SetError(Properties.Resources.BitEditorInvalidEncoding); }
        return InvokeAsync(StateHasChanged);
    }

    /// <summary>Replaces storage at the current width while preserving read-only bits.</summary>
    /// <param name="bytes">The candidate storage; locked bits retain their existing values.</param>
    /// <param name="count">The number of meaningful bits represented by the candidate.</param>
    /// <exception cref="FormatException">The candidate changes the current storage width.</exception>
    /// <exception cref="DecoderFallbackException">The candidate does not form valid text.</exception>
    private void ApplyRepresentation(byte[] bytes, int count)
    {
        if (CannotEdit)
        {
            return;
        }
        // Representation edits preserve width and protect all locked bits.
        if (count != _bitCount || bytes.Length != _bytes.Length)
        {
            throw new FormatException();
        }
        for (int i = 0; i < _bitCount; ++i)
        {
            if (!Locked(i))
            {
                continue;
            }
            var (b, mask) = Location(i);
            bytes[b] = (byte)((bytes[b] & ~mask) | (_bytes[b] & mask));
        }
        if (IsBitString && count % 8 != 0 && bytes.Length != 0)
        {
            bytes[^1] &= (byte)(255 << (8 - count % 8));
        }
        var previous = _bytes;
        _bytes = bytes;
        try
        { Publish(); }
        catch { _bytes = previous; throw; }
    }
    /// <summary>Runs a representation edit and reports conversion failures through validation.</summary>
    /// <param name="action">The edit operation to validate.</param>
    private void Edit(Action action)
    {
        if (CannotEdit)
        {
            return;
        }
        try
        { action(); }
        catch (DecoderFallbackException) { SetError(Properties.Resources.BitEditorInvalidEncoding); }
        catch (Exception ex) when (ex is FormatException or OverflowException or EncoderFallbackException or ArgumentException)
        { SetError(Properties.Resources.BitEditorInvalidValue); }
    }
    /// <summary>Applies binary digit input at the current bit width.</summary>
    /// <param name="e">The changed representation input.</param>
    private void BinaryChanged(ChangeEventArgs e) => Edit(() =>
    {
        string text = e.Value?.ToString() ?? string.Empty;
        ApplyRepresentation(GXBitConverter.ToBytes(text), text.Length);
    });
    /// <summary>Applies hexadecimal input at the current storage width.</summary>
    /// <param name="e">The changed representation input.</param>
    private void HexChanged(ChangeEventArgs e) => Edit(() =>
        ApplyRepresentation(Convert.FromHexString(e.Value?.ToString() ?? string.Empty), _bitCount));
    /// <summary>Applies invariant decimal input using the configured byte order.</summary>
    /// <param name="e">The changed representation input.</param>
    private void NumberChanged(ChangeEventArgs e) => Edit(() =>
        ApplyRepresentation(GXBitConverter.ToBytes(GXBitConverter.ParseNumber<TValue>(e.Value?.ToString() ?? string.Empty), ByteOrder), _bitCount));
    /// <summary>Encodes edited text using strict encoding validation.</summary>
    /// <param name="e">The changed representation input.</param>
    private void TextChanged(ChangeEventArgs e) => Edit(() =>
        ApplyRepresentation(GXBitConverter.StrictEncoding(Encoding).GetBytes(e.Value?.ToString() ?? string.Empty), _bitCount));

    /// <inheritdoc />
    protected override bool TryParseValueFromString(string? value, out TValue result, [System.Diagnostics.CodeAnalysis.NotNullWhen(false)] out string? validationErrorMessage)
    {
        result = default!;
        validationErrorMessage = Properties.Resources.BitEditorInvalidValue;
        return false;
    }
}
