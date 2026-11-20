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
using Gurux.UI.Components.Enums;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web.Virtualization;
using Microsoft.JSInterop;

namespace Gurux.UI.Components.BitEditor;

/// <summary>Displays grouped bits in responsive rows and optionally virtualizes complete rows.</summary>
public partial class GXBitGrid
{
    /// <summary>Renders complete virtualized rows with a stable row-index comparer on supported frameworks.</summary>
    private RenderFragment RenderVirtualizedRows => builder =>
    {
        builder.OpenComponent<Virtualize<int>>(0);
        builder.SetKey((_columns, BitCount, RowHeight));
        builder.AddAttribute(1, nameof(Virtualize<int>.ItemsProvider), (ItemsProviderDelegate<int>)GetRows);
#if NET11_0_OR_GREATER
        builder.AddAttribute(2, nameof(Virtualize<int>.ItemComparer), EqualityComparer<int>.Default);
#endif
        builder.AddAttribute(3, nameof(Virtualize<int>.ItemSize), RowHeight);
        builder.AddAttribute(4, nameof(Virtualize<int>.OverscanCount), 3);
        builder.AddAttribute(5, nameof(Virtualize<int>.ChildContent), (RenderFragment<int>)RenderRow);
        builder.CloseComponent();
    };

    /// <summary>Current byte storage.</summary>
    [Parameter] public byte[] Bytes { get; set; } = [];
    /// <summary>Original byte storage used to highlight changes.</summary>
    [Parameter] public byte[] Original { get; set; } = [];
    /// <summary>Number of logical bits, including partial bit-string bytes.</summary>
    [Parameter] public int BitCount { get; set; }
    /// <summary>Whether logical indexes represent numeric significance.</summary>
    [Parameter] public bool Numeric { get; set; }
    /// <summary>Whether logical indexes follow bit-string character order.</summary>
    [Parameter] public bool BitString { get; set; }
    /// <summary>Byte order used for numeric storage and display.</summary>
    [Parameter] public ByteOrder ByteOrder { get; set; }
    /// <summary>Display order of bits within each byte; ignored for bit strings.</summary>
    [Parameter] public BitOrder BitOrder { get; set; }
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
    /// <summary>Bit definitions indexed by logical bit position.</summary>
    [Parameter] public IReadOnlyDictionary<int, GXBitDefinition> Definitions { get; set; } = new Dictionary<int, GXBitDefinition>();
    /// <summary>Callback invoked for a requested bit toggle.</summary>
    [Parameter] public EventCallback<int> Toggle { get; set; }
    /// <summary>Identifies the viewport observed for responsive column changes.</summary>
    private ElementReference _host;
    /// <summary>Owns the JavaScript resize observer and keyboard navigation functions.</summary>
    private IJSObjectReference? _module;
    /// <summary>Keeps this component available to JavaScript until its observer is disconnected.</summary>
    private DotNetObjectReference<GXBitGrid>? _reference;
    /// <summary>Stores the effective row length after applying available width and byte alignment.</summary>
    private int _columns = 16;
    /// <summary>Stores the last viewport width in CSS pixels; zero means no measurement is available.</summary>
    private double _width;
    /// <summary>Prevents late resize callbacks from updating a disposed component.</summary>
    private bool _disposed;
    /// <summary>Gets the total number of rows, including a final incomplete row.</summary>
    private int RowCount => (int)(((long)BitCount + _columns - 1) / _columns);
    /// <summary>Gets the fixed row height in CSS pixels, including optional byte labels.</summary>
    private float RowHeight => ShowByteIndex ? 90 : 68;
    /// <inheritdoc />
    protected override void OnParametersSet() => CalculateColumns();
    /// <summary>Fits the preferred row length to the measured width, preferring complete bytes.</summary>
    private void CalculateColumns()
    {
        int capacity = _width > 0 ? Math.Max(1, (int)(_width / 44)) : BitsPerRow;
        int columns = AutoWrap ? Math.Min(BitsPerRow, capacity) : BitsPerRow;
        if (AutoWrap && columns >= 8)
        {
            columns -= columns % 8;
        }
        _columns = Math.Max(1, columns);
    }
    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }
        try
        {
            _module = await JS.InvokeAsync<IJSObjectReference>("import", "./_content/Gurux.UI.Components/BitEditor/GXBitGrid.razor.js");
            if (_disposed)
            {
                await _module.DisposeAsync();
                return;
            }
            _reference = DotNetObjectReference.Create(this);
            await _module.InvokeVoidAsync("observe", _host, _reference);
        }
        catch (JSDisconnectedException) { }
    }
    /// <summary>Updates the row length after a viewport resize.</summary>
    /// <param name="width">The measured viewport width in CSS pixels.</param>
    /// <returns>A task that completes after the resize has been processed on the renderer dispatcher.</returns>
    [JSInvokable]
    public Task Resize(double width) => InvokeAsync(() =>
    {
        if (_disposed)
        {
            return;
        }
        _width = width;
        int previous = _columns;
        CalculateColumns();
        if (previous != _columns)
        {
            StateHasChanged();
        }
    });
    /// <summary>Supplies global row indexes for the visible virtualized range.</summary>
    /// <param name="request">The requested starting row, row count and cancellation token.</param>
    /// <returns>The requested row indexes clipped to the current row count, together with that total count.</returns>
    private ValueTask<ItemsProviderResult<int>> GetRows(ItemsProviderRequest request)
    {
        int count = Math.Max(0, Math.Min(request.Count, RowCount - request.StartIndex));
        return ValueTask.FromResult(new ItemsProviderResult<int>(Enumerable.Range(request.StartIndex, count), RowCount));
    }
    /// <summary>Maps a left-to-right display position to its logical bit index.</summary>
    /// <param name="display">The zero-based display position across all rows.</param>
    /// <returns>The string character index, numeric significance, or bit position within byte-array storage.</returns>
    private int LogicalIndex(int display)
    {
        if (BitString)
        {
            return display;
        }
        int within = BitOrder == BitOrder.MsbFirst ? 7 - display % 8 : display % 8;
        int b = display / 8;
        if (Numeric && ByteOrder == ByteOrder.BigEndian)
        {
            b = Bytes.Length - 1 - b;
        }
        return b * 8 + within;
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        _disposed = true;
        if (_module is not null)
        {
            try
            {
                await _module.InvokeVoidAsync("disconnect", _host);
                await _module.DisposeAsync();
            }
            catch (JSDisconnectedException) { }
        }
        _reference?.Dispose();
    }
}
